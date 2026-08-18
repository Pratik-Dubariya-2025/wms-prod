# Role-Based Access Control (RBAC)
### A Practical Guide — From an Unguarded System to a Scalable, Permission-Driven Architecture

> **Audience:** Client, Project Manager, and Developers
> **Goal of this session:** Understand *what* RBAC is, *why* it matters, *how* it can be implemented in .NET, and *how we built it* in our system — with real code we can demo.

---

## Table of Contents

1. [The Starting Point — A System Without RBAC](#1-the-starting-point--a-system-without-rbac)
2. [What is RBAC?](#2-what-is-rbac)
3. [Why Do We Need RBAC?](#3-why-do-we-need-rbac)
4. [The Core Building Blocks](#4-the-core-building-blocks)
5. [How to Implement RBAC in .NET — The Options](#5-how-to-implement-rbac-in-net--the-options)
6. [How We Implemented RBAC in Our Project](#6-how-we-implemented-rbac-in-our-project)
7. [Two Layers of Defense (and a real bug it caught)](#7-two-layers-of-defense-coarse--fine-grained)
8. [How It Benefits the Project](#8-how-it-benefits-the-project)
9. [Why This Approach Scales](#9-why-this-approach-scales)
10. [Live Demo Script](#10-live-demo-script)
11. [Glossary & FAQ](#11-glossary--faq)
12. [Summary](#12-summary)

---

## 1. The Starting Point — A System Without RBAC

Before we talk about RBAC, let's be honest about what a system looks like **without** it.

In an unguarded system, *authentication* answers **"Who are you?"** — but nothing answers **"What are you allowed to do?"** Once a user is logged in, they can call any endpoint and touch any data.

When teams *do* try to add access control without a proper model, it usually starts as scattered `if` checks sprinkled across the codebase:

```csharp
// ❌ The "without RBAC" anti-pattern — authorization logic hardcoded everywhere

public async Task<IActionResult> DeleteProject(Guid id)
{
    // Hardcoded role string, checked inline, copy-pasted into every action
    if (currentUser.Role != "Admin" && currentUser.Role != "Manager")
    {
        return Forbid();
    }
    // ...what about "can THIS manager delete THIS project"? Not handled.
    await _service.Delete(id);
    return Ok();
}
```

### Why this is bad

| Problem | Consequence |
|---|---|
| **Magic strings everywhere** (`"Admin"`, `"Manager"`) | One typo (`"Manger"`) silently disables a security check. |
| **Logic is duplicated** across dozens of controllers | A policy change means editing 50 files and missing some. |
| **Roles are hardcoded** | Adding a new role = code change + redeploy. |
| **No separation** between *role* and *capability* | You can't grant "just this one extra permission" to a user. |
| **No central place to audit** "who can do what" | Security reviews become archaeology. |
| **Coarse-grained only** | "Is a Manager?" — but not "Is this manager the *owner* of this record?" |

> **The core problem:** Authorization becomes a tangle of scattered, inconsistent, copy-pasted rules. It does not scale, it is error-prone, and it is impossible to reason about as a whole.

This is exactly the pain RBAC is designed to remove.

---

## 2. What is RBAC?

**Role-Based Access Control (RBAC)** is an authorization model where *permissions are not assigned to users directly*. Instead:

- **Permissions** are grouped into **Roles**.
- **Roles** are assigned to **Users**.

A user's effective capabilities are simply the union of the permissions of all the roles they hold.

```
   USER  ───holds──▶  ROLE  ───grants──▶  PERMISSION  ───protects──▶  ACTION / RESOURCE
 (Alice)            (Manager)          (project.delete)            (Delete a Project)
```

### The four nouns

| Term | Meaning | Example |
|---|---|---|
| **User** | An identity that logs in | `manager.01@wms.com` |
| **Role** | A named job function | `ADMIN`, `MANAGER`, `TL`, `EMPLOYEE` |
| **Permission** | A single, fine-grained capability | `project.create`, `task.delete`, `leave.approve` |
| **Resource** | The thing being protected | A specific project, task, or invoice |

### The key idea

> Users change teams, get promoted, and leave. Permissions for a *job function* rarely change.
> By inserting **Role** as a layer between **User** and **Permission**, we manage access by *job function* instead of by *individual*.

When a new developer joins, you don't wire up 20 permissions — you assign them the `DEVELOPER` role, and they instantly inherit exactly the right set.

---

## 3. Why Do We Need RBAC?

| Driver | What RBAC gives us |
|---|---|
| **Security — Principle of Least Privilege** | Every user gets the *minimum* access needed for their job, nothing more. |
| **Single source of truth** | "Who can do what" lives in data (roles + permissions), not scattered in code. |
| **Maintainability** | Change a role's permissions once, and it applies to everyone with that role. |
| **Onboarding speed** | New hire → assign a role → done. No bespoke permission wiring. |
| **Auditability & Compliance** | You can answer "Who can approve leave?" with a query, not a code search. (Important for ISO 27001, SOC 2, GDPR.) |
| **Separation of duties** | The person who *creates* an invoice need not be the one who *approves* it. |
| **Scalability** | Add features, roles, and users without rewriting authorization logic. |

> In short: RBAC turns authorization from a *coding problem* into a *configuration problem*.

---

## 4. The Core Building Blocks

RBAC is built on a **many-to-many** relationship between roles and permissions, and between users and roles.

```
┌────────┐        ┌────────────┐        ┌──────────────┐        ┌──────────────┐
│  User  │◀──M:N─▶│    Role    │◀──M:N─▶│  Permission  │        │  Resource    │
└────────┘        └────────────┘        └──────────────┘        └──────────────┘
     │                                                                  ▲
     └──────────── UserPermissionOverride (per-user grant/deny) ────────┘
```

In our system the model is:

- **User ↔ Role** — a join table (`UserRole`). A user can hold **multiple roles** (multi-role support).
- **Role ↔ Permission** — a join table (`RolePermission`).
- **UserPermissionOverride** — an *escape hatch*: grant or deny a single permission to a single user, optionally with an **expiry date**. This handles the real-world "just give Bob report access until Friday" case without inventing a new role.

A user's **effective permissions** are computed as:

```
effective = (all permissions from all roles)
            MINUS (overrides where IsGranted = false)
            PLUS  (overrides where IsGranted = true)
```

---

## 5. How to Implement RBAC in .NET — The Options

There are three common approaches in the .NET world. They are not mutually exclusive, but each has a different "altitude."

### Option A — Role checks / `[Authorize(Roles=...)]` (the naive way)

```csharp
[Authorize(Roles = "Admin,Manager")]   // hardcoded role names
public IActionResult DeleteProject(Guid id) { ... }
```

**Pros:** Built-in, zero setup.
**Cons:** Hardcoded role strings, no concept of *permissions*, changing access requires code + redeploy. Doesn't scale.

---

### Option B — Policy-Based Authorization with `IAuthorizationHandler`

This is ASP.NET Core's first-class extensibility point. You define a **requirement** and a **handler**, register a **policy**, and apply it with `[Authorize(Policy = "...")]`.

```csharp
// 1) A requirement (just a marker carrying data)
public class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }
    public PermissionRequirement(string permission) => Permission = permission;
}

// 2) A handler that decides if the requirement is met
public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim("permission", requirement.Permission))
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}

// 3) Register a policy and apply it
//    options.AddPolicy("project.delete",
//        p => p.Requirements.Add(new PermissionRequirement("project.delete")));

[Authorize(Policy = "project.delete")]
public IActionResult DeleteProject(Guid id) { ... }
```

**Pros:** Native to ASP.NET Core, integrates with the framework's auth pipeline.
**Cons:** Authorization lives in the **web/controller layer**. If you have a clean architecture where business logic is in an application layer (CQRS handlers), the check sits *outside* your use-case. It also requires registering a policy per permission.

---

### Option C — Permission-Based via MediatR `IPipelineBehavior` (our approach) ✅

In a **CQRS + MediatR** architecture, every use-case is a `Command` or `Query` (a "request") handled by a single handler. MediatR lets you wrap every request in a **pipeline behavior** — middleware for your application layer.

We put the authorization check *there*, so it runs for **every** command/query automatically, regardless of whether it was triggered by a controller, a background job, or a test.

```csharp
// Mark a request with the permission it needs — declarative, no magic strings.
[RequirePermission(PermissionCodes.ProjectDelete)]
public class DeleteProjectCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
```

The behavior reads that attribute and enforces it *before* the handler runs.

**Why we chose this:**

| Criterion | Option B (`IAuthorizationHandler`) | Option C (MediatR behavior — ours) |
|---|---|---|
| Where the check lives | Web layer (controllers) | Application layer (use-cases) |
| Coverage | Only HTTP entry points | **Every** command/query, automatically |
| Coupling to ASP.NET | High | None (pure application logic, easy to unit test) |
| Declaring a permission | Register a policy per permission | Just add an attribute |
| Fits CQRS | Awkward | Natural |

> We use **Option C** as our primary mechanism. It centralizes the "do you have the permission?" check in exactly one place, while keeping each use-case self-describing via an attribute.

---

## 6. How We Implemented RBAC in Our Project

Our implementation has **five moving parts**, working together from login to data access.

### 6.1 Authentication — JWT carries the roles

At login we issue a JWT. The token embeds the user's identity and **role claims** (it deliberately does *not* embed permissions — those can change and are resolved fresh, see 6.3).

```csharp
// TokenService.GenerateAccessToken(User user)  — simplified
List<Claim> claims =
[
    new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
    new(JwtRegisteredClaimNames.Email, user.Email),
];

// One Role claim per role the user holds (multi-role supported)
foreach (var roleClaim in roleClaims.Distinct())
{
    claims.Add(new Claim(ClaimTypes.Role, roleClaim));
}
```

### 6.2 Reading the current user — `ICurrentUserService`

A thin service exposes the logged-in user's identity, roles, and (resolved) permissions to the rest of the app, sourced from the validated JWT in the HTTP context.

```csharp
public interface ICurrentUserService
{
    Guid?  UserId { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }
    bool IsAuthenticated { get; }
    bool HasPermission(string permissionCode);
}
```

### 6.3 Resolving & caching permissions — `PermissionCacheService`

Roles live in the token, but **permissions are computed from the database** so that an admin changing a role's permissions takes effect quickly (no re-login needed). To avoid a DB hit on every request, results are cached in-memory for **5 minutes**.

```csharp
// Cache miss → compute from DB
List<string> rolePermissions = await _unitOfWork.RolePermission.GetAllAsync(
    s => s.Permission.Code,
    s => s.Role.UserRoles.Any(ur => ur.UserId == userId));

var overrides = await _unitOfWork.UserPermissionOverride.IncludeAndGetAllAsync(
    o => o.UserId == userId && !o.IsDeleted
         && (o.ExpiresAt == null || o.ExpiresAt > DateTime.UtcNow),
    o => o.Permission);

var denied  = overrides.Where(o => !o.IsGranted).Select(o => o.Permission.Code);
var granted = overrides.Where(o =>  o.IsGranted).Select(o => o.Permission.Code);

// effective = rolePermissions - denied + granted
List<string> permissions = rolePermissions
    .Except(denied,  StringComparer.OrdinalIgnoreCase)
    .Union (granted, StringComparer.OrdinalIgnoreCase)
    .ToList();
```

> When an admin changes a user's roles or permissions, we call `InvalidateUser(userId)` to clear that user's cache entry immediately.

### 6.4 Layer 1 — Declarative, coarse-grained checks

**(a)** Tag each command/query with the permission it needs:

```csharp
[RequirePermission(PermissionCodes.TaskCreate)]
public class CreateTaskCommand : IRequest<ApiResponse<Guid>> { /* ... */ }
```

**(b)** The attribute itself supports AND / OR logic:

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class RequirePermissionAttribute : Attribute
{
    public string[] Permissions { get; }
    // false (default) = user must have ALL; true = user needs ANY ONE.
    public bool RequireAny { get; set; } = false;
    public RequirePermissionAttribute(params string[] permissions) => Permissions = permissions;
}
```

**(c)** The MediatR pipeline behavior enforces it **before the handler runs**:

```csharp
public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var attribute = typeof(TRequest).GetCustomAttribute<RequirePermissionAttribute>();

        // No attribute → public request → continue.
        if (attribute is null || attribute.Permissions.Length == 0)
            return await next();

        if (!_currentUserService.IsAuthenticated)
            return CreateFailureResponse("You are not authenticated.", 401);

        bool hasPermission = attribute.RequireAny
            ? attribute.Permissions.Any(p => _currentUserService.HasPermission(p))
            : attribute.Permissions.All(p => _currentUserService.HasPermission(p));

        if (!hasPermission)
            return CreateFailureResponse("You do not have the required permission(s).", 403);

        return await next();   // ✅ authorized → run the handler
    }
}
```

**(d)** It is wired in once, globally, in DI — every request flows through it:

```csharp
services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>)); // runs first
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>));
});
```

> Authorization runs **first** — unauthorized requests are rejected before we waste time on validation or database work.

### 6.5 Layer 2 — Fine-grained, row-level checks

Layer 1 answers *"Can this user create tasks at all?"* It does **not** answer *"Can this user create a task in **this specific** project?"* For that we add ownership/membership checks **inside the handler**, centralized in small authorizers.

```csharp
// ProjectAccessAuthorizer — only ADMIN is global; everyone else (incl. MANAGER)
// is scoped to projects they Own, lead (TeamLead), or are a Member of.
internal static class ProjectAccessAuthorizer
{
    public static bool CanManage(Project project, ICurrentUserService user)
    {
        if (!user.UserId.HasValue) return false;
        if (user.Roles.Contains("ADMIN")) return true;          // global super-user
        var uid = user.UserId.Value;
        return project.OwnerId == uid || project.TeamLeadId == uid;  // owner / TL only
    }
}
```

Used inside the command:

```csharp
[RequirePermission(PermissionCodes.ProjectUpdate)]            // Layer 1: has the permission?
public class UpdateProjectCommand : IRequest<ApiResponse<bool>> { /* ... */ }

// inside the handler — Layer 2: is it THEIR project?
if (!ProjectAccessAuthorizer.CanManage(project, _currentUserService))
    return ApiResponse<bool>.Failure("You do not have permission to edit this project.", null, 403);
```

### 6.6 The full request lifecycle

```
  Client (JWT in header)
        │
        ▼
  [JWT Middleware]  ── validates token, builds ClaimsPrincipal (roles)
        │
        ▼
  Controller  ──  sends Command/Query via MediatR
        │
        ▼
  ┌──────────────────────── MediatR Pipeline ────────────────────────┐
  │  AuthorizationBehaviour   → reads [RequirePermission], checks      │  ← Layer 1
  │                             HasPermission() (cache → DB)           │     (coarse)
  │  ValidationBehaviour      → validates input                        │
  │  Handler                  → row-level ownership check + business   │  ← Layer 2
  │                             logic (ProjectAccessAuthorizer, etc.)  │     (fine)
  └───────────────────────────────────────────────────────────────────┘
        │
        ▼
  Database
```

---

## 7. Two Layers of Defense (Coarse + Fine-Grained)

This two-layer design is the heart of robust authorization. A real example from our system shows why **both** are required.

**The scenario:** Every Department Manager has the `project.update` permission — that's correct, managers *do* edit projects.

- ✅ **Layer 1 (permission)** correctly allows any manager to reach the "edit project" use-case.
- ❌ But Layer 1 alone cannot tell whether *Manager 02* is editing *their own* project or *Manager 01's* project.

Without Layer 2, **Manager 02 could add a task to Manager 01's project** — a cross-tenant data breach, even though every permission check "passed."

**The fix** is Layer 2: the handler verifies the manager actually *owns or leads* the specific project (`ProjectAccessAuthorizer.CanManage`). The rule we enforce:

> **Only `ADMIN` is a global super-user. Every other role — including `MANAGER` — is scoped to the records they own, lead, or belong to.**

| Question | Answered by |
|---|---|
| "Can this user edit projects *in general*?" | **Layer 1** — permission (`project.update`) |
| "Can this user edit *this* project?" | **Layer 2** — ownership (`OwnerId / TeamLeadId`) |

> **Takeaway for the demo:** Permissions gate the *capability*; row-level checks gate the *specific resource*. You need both.

---

## 8. How It Benefits the Project

- **One place to change a rule.** Want to let Team Leads delete tasks? Grant `task.delete` to the `TL` role — no code change.
- **Self-documenting use-cases.** Each command says, at the top, exactly what it requires: `[RequirePermission(...)]`.
- **No magic strings in business logic.** Permission codes are centralized constants (`PermissionCodes`), so typos become compile errors.
- **Consistent enforcement.** Because the check is a global pipeline behavior, no developer can *forget* to add it — if a request has the attribute, it is enforced, everywhere.
- **Fast.** Permissions are cached for 5 minutes per user, so authorization adds negligible overhead.
- **Flexible exceptions.** `UserPermissionOverride` (with expiry) handles one-off grants/denies without polluting the role model.
- **Secure by default.** Row-level scoping ensures tenants/managers only see their own data.

---

## 9. Why This Approach Scales

| Growth scenario | What you do | Code change? |
|---|---|---|
| **New employee joins** | Assign an existing role | ❌ None |
| **New role needed** (e.g., "Auditor") | Insert a role + map permissions (data) | ❌ None |
| **Change what a role can do** | Update `RolePermission` rows | ❌ None |
| **Temporary elevated access** | Add a `UserPermissionOverride` with `ExpiresAt` | ❌ None |
| **New feature/module** | Add new permission codes + `[RequirePermission]` on the new commands | ✅ Minimal, localized |
| **New entry point** (CLI, job, gRPC) | Reuse the same MediatR commands | ❌ Auth comes for free |

Because authorization is **data-driven** (roles & permissions in the DB) and **centrally enforced** (one pipeline behavior), the system grows by *configuration*, not by rewriting security logic. That is the definition of a scalable access-control model.

---

## 10. Live Demo Script

A suggested flow for the session:

1. **Show the pain (2 min).** Open the "without RBAC" anti-pattern slide — scattered `if (role == "Admin")` checks.
2. **Show the model (3 min).** Walk the Users ↔ Roles ↔ Permissions diagram. Point out `UserPermissionOverride`.
3. **Show Layer 1 in code (3 min).**
   - Open `CreateTaskCommand` → highlight `[RequirePermission(PermissionCodes.TaskCreate)]`.
   - Open `AuthorizationBehaviour` → show the single enforcement point.
4. **Demo it live (4 min).**
   - Log in as a low-privilege user → attempt an action they lack the permission for → show the **403** response.
   - Grant the permission (or assign a role) → retry → it now works. *No redeploy.*
5. **Show Layer 2 (3 min).**
   - Explain the Manager-02-edits-Manager-01's-project scenario.
   - Open `ProjectAccessAuthorizer.CanManage` → show the ownership check.
   - Demo: as Manager 02, try to open Manager 01's project → **403 / not visible**.
6. **Show scalability (2 min).** Add a permission to a role in the DB/UI, invalidate cache, show it takes effect immediately.
7. **Q&A.**

---

## 11. Glossary & FAQ

**Glossary**

- **Authentication** — verifying *who* a user is (login).
- **Authorization** — verifying *what* a user may do (RBAC).
- **Role** — a named bundle of permissions tied to a job function.
- **Permission / Permission code** — a single capability string, e.g. `task.delete`.
- **Claim** — a key/value fact about the user, carried in the JWT (e.g., a role).
- **Pipeline behavior** — MediatR middleware that wraps every request.
- **Row-level security** — restricting *which records* a user may act on, not just *which actions*.
- **Override** — a per-user grant/deny that supplements the role-derived permissions.

**FAQ**

- **Q: Why not put permissions directly in the JWT?**
  A: Permissions can change (an admin edits a role). Keeping them out of the token and resolving them (cached) means changes apply within minutes without forcing users to log in again.

- **Q: Isn't checking the DB for permissions slow?**
  A: We cache per-user permissions in memory for 5 minutes, so the common path never hits the DB.

- **Q: Why both an attribute *and* in-handler checks?**
  A: The attribute is coarse ("can do X at all"); the handler check is fine ("can do X to *this* record"). Defense in depth.

- **Q: What about a user with multiple roles?**
  A: Supported. Effective permissions are the union of all their roles, then adjusted by overrides.

---

## 12. Summary

| Without RBAC | With RBAC (our approach) |
|---|---|
| Scattered `if (role == "Admin")` checks | One declarative `[RequirePermission]` + one pipeline behavior |
| Magic strings, easy to typo | Centralized permission-code constants |
| Roles hardcoded in code | Roles & permissions are data |
| Change access = redeploy | Change access = update data |
| Coarse "is admin?" only | Coarse permission **+** fine row-level ownership |
| Hard to audit | "Who can do X?" is a query |
| Doesn't scale | Grows by configuration, not code |

> **The narrative in one line:** We moved authorization from *scattered, hardcoded, un-auditable code* to a *centralized, data-driven, two-layered model* — secure by default and scalable by design.

---

*Prepared for an RBAC knowledge-sharing session. Code samples are taken from the live implementation.*
