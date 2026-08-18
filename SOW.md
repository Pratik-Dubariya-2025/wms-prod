# Statement of Work — Role-Based Access Control (RBAC)

**Project:** Workspace Management System (WMS)
**Module:** Role-Based Access Control (RBAC) Engine
**Stack:** .NET 9 (Clean Architecture, EF Core, MediatR/CQRS) · React 19 + TypeScript · SQL Server 2022
**Document type:** Statement of Work — RBAC scope only
**Status:** Demo build

---

## 1. Purpose

This SOW defines the scope, deliverables, and acceptance criteria for the **RBAC subsystem** of WMS.
It governs *what every user can see and do* across the application. The rest of the WMS feature set
(tasks, projects, HR, CRM, etc.) is **out of scope** for this document except where it consumes RBAC.

The goal: permissions are **data, not code** — access rules can be changed at runtime (assigning roles,
editing role permissions, adding per-user overrides) **without redeploying** the application.

---

## 2. Objectives

1. Authenticate users and issue identity tokens carrying their roles.
2. Authorize every protected operation against a permission the user effectively holds.
3. Model an organizational **authority hierarchy** so higher roles manage lower ones.
4. Allow **per-user exceptions** (grant/deny) on top of role-derived permissions.
5. Drive the **UI** from the same permission set (hide/disable/mask what the user can't access).
6. Keep permission checks **fast** via caching.

---

## 3. RBAC Model

Access is evaluated on multiple levels (deny-by-default):

| Level | Meaning | Mechanism |
| ----- | ------- | --------- |
| **Role-level** | Access granted through job roles, never directly to users | `UserRole` (user ↔ role, many-to-many) |
| **Resource-level** | Access per resource + action, e.g. `user.create`, `role.read` | `Permission` + `RolePermission` |
| **Scope-level** | Breadth of a permission: `all` / `dept` / `team` / `self` | `Permission.Scope` column |
| **Per-user override** | Grant or deny one permission for one user, overriding their role | `UserPermissionOverride` |
| **Field-level (UI)** | Hide or mask sensitive fields the user may not view | `PermissionGate` (frontend) |

**Authority hierarchy** — roles carry a numeric `Priority` (lower number = higher authority). A user can
only invite/manage users of **strictly lower authority** than themselves; Admin bypasses the check.

Seeded role set (highest → lowest authority): `ADMIN (1)`, `HR (10)`, `ACCOUNTS (10)`, `MANAGER (20)`,
`BDE (25)`, `TL (30)`, `BDA (35)`, `SSE (40)`, `SE (50)`, `ASE_TSE (60)`, `EMPLOYEE (100)`.

---

## 4. Scope of Work

### 4.1 In Scope

**A. Data model (SQL Server / EF Core)**
- `Role` — `Name`, unique `Code`, `Priority`, `IsSystemRole`
- `Permission` — `Module`, `Action`, unique `Code`, `Scope`
- `Module` — grouping for permissions
- `RolePermission` — role ↔ permission mapping
- `UserRole` — user ↔ role mapping
- `UserPermissionOverride` — per-user grant/deny, with `reason` and optional `expires_at`
- `User` links to `Department`, `Designation`, `Manager` (reports-to), `Team`
- Seed data: system roles, modules, base permission set, role-permission grants

**B. Authentication & token**
- Email/username + password login (BCrypt-hashed), JWT issuance
- JWT carries the user's identity and **role claims**
- First-time-login flow, forgot/reset password, account lockout, optional MFA (TOTP)

**C. Authorization enforcement (backend)**
- `RequirePermissionAttribute` declaring the permission(s) a command/query needs
- `AuthorizationBehaviour` (MediatR pipeline) that blocks unauthorized requests **before** the handler runs
- `ICurrentUserService.HasPermission(code)` / `IsInRole(role)` resolving effective permissions
- **Permission cache** (5-minute TTL) so repeated checks don't hit the DB each time
- Hierarchy enforcement in user invitation (`sp_InviteUser`) by role `Priority`

**D. RBAC administration (CQRS + API)**

| Capability | Command / Query |
| ---------- | --------------- |
| List / read roles | `GetRolesQuery`, `GetRolePermissionsQuery` |
| Create / update role | `CreateRoleCommand`, `UpdateRoleCommand` |
| Assign / remove permission ↔ role | `AssignRolePermissionCommand`, `RemoveRolePermissionCommand` |
| Assign / remove role ↔ user | `AssignUserRoleCommand`, `RemoveUserRoleCommand` |
| Add / remove per-user override | `AddUserPermissionOverrideCommand`, `RemoveUserPermissionOverrideCommand` |
| Read a user's effective permissions / roles / overrides | `GetUserPermissionsQuery`, `GetUserRolesQuery`, `GetUserPermissionOverridesQuery` |

Exposed via `RolesController` and `UsersController` REST endpoints (each protected by the matching permission).

**E. Frontend (React)**
- `permissionStore` (Zustand) — holds `permissions[]`, `roles[]`, `hasPermission()`, `isInRole()`
- `usePermissions()` hook — permission checks in components
- `PermissionGate` — render / hide / **mask** children based on a permission
- `ProtectedRoute` / `RouteGate` / `RoleGuard` — route-level guards (redirect to login / 403)
- `PERMISSIONS` constants — no magic strings; mirror backend codes
- **Admin UI:** Roles page, Manage-User-Roles modal, permission assignment

### 4.2 Out of Scope (this SOW)

- Row-level data filtering enforcement at query time (scope column is modeled; runtime filtering is a defined extension)
- Distributed cache (Redis) and real-time permission push (SignalR) — currently in-process cache
- Immutable audit logging of permission changes
- Field-level **encryption** of sensitive columns
- Non-RBAC business modules (tasks/projects/HR/CRM features themselves)

---

## 5. Key Workflows (demo script)

1. **Login** → JWT issued with role claims; frontend loads the user's permission set.
2. **Create a role** → assign `resource.action` permissions to it.
3. **Assign the role to a user** → user immediately gains those permissions (after cache refresh).
4. **Add a per-user override** → grant one extra permission, or deny one the role would give.
5. **Attempt a protected action without permission** → API returns **403**; UI hides/disables the control.
6. **Hierarchy** → a Manager can invite an Engineer but **cannot** invite/another Admin or peer.

---

## 6. Deliverables

| # | Deliverable |
| - | ----------- |
| D1 | RBAC database schema + EF Core migrations + seed data |
| D2 | JWT authentication with role claims |
| D3 | Authorization pipeline (`RequirePermission` + `AuthorizationBehaviour`) + permission cache |
| D4 | RBAC admin CQRS handlers + REST endpoints (roles, role-permissions, user-roles, overrides) |
| D5 | Frontend permission store, hooks, `PermissionGate`, route guards, Roles admin UI |
| D6 | Documentation (this SOW, README run instructions) |

---

## 7. Acceptance Criteria

- [ ] Unauthenticated requests to protected endpoints are rejected (401).
- [ ] A user **without** the required permission receives **403**; the corresponding UI control is hidden/disabled.
- [ ] Assigning a role grants its permissions; removing it revokes them (after cache TTL / refresh).
- [ ] A **deny** override blocks a permission the role otherwise grants; a **grant** override adds one.
- [ ] Overrides with `expires_at` in the past have no effect.
- [ ] A user can only invite/manage users of strictly lower role authority (`Priority`); Admin is exempt.
- [ ] Permission codes are referenced via shared constants on both backend and frontend (no magic strings).
- [ ] Permission lookups are served from cache on repeat checks within the TTL.

---

## 8. Assumptions & Dependencies

- SQL Server 2022 and the .NET 9 runtime are available (locally via Docker, per the README).
- Seed Admin account (`admin@wms.com`) exists for demoing administration.
- Permission changes take effect on the next request after the cache entry expires/refreshes.
- Roles flagged `IsSystemRole` are protected from deletion.

---

## 9. Architecture Summary

```
Login ──► JWT (role claims)
                │
HTTP request ──►│ AuthorizationBehaviour (MediatR pipeline)
                │     └─ ICurrentUserService.HasPermission(code)
                │            └─ PermissionCache ──(miss)──► DB:
                │                 UserRole → RolePermission → Permission
                │                 (+ UserPermissionOverride grant/deny)
                ▼
        Handler runs  ◄── only if authorized, else 403

Frontend: permissionStore ──► usePermissions / PermissionGate / RouteGuards
```

---

*End of Statement of Work — RBAC.*
