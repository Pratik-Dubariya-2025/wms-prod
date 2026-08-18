// AUTO-GENERATED from docs/policy-module-guide.md — do not hand-edit, regenerate instead.
// Embedded (rather than fetched) so the in-app Policy Guide works offline and needs no extra backend route.

export const POLICY_GUIDE_MARKDOWN = `
# Policy Guide: How to Set Up Access Rules

This guide explains **Access Policies** — the rules that control who can view, add, modify, or
delete information in the system, and exactly which records they are permitted to see. Kindly go
through this guide before creating or editing a policy, as it shall help you avoid common mistakes
and set up rules correctly on the first attempt itself.

This guide uses the same terms and field names that appear on the Create/Edit Policy screen, so
that you can follow along on-screen as you read.

If you wish to see a real example directly, kindly jump to
[a real example](#7-example-a-real-world-scenario).

---

## 1. How Access Policies Work

There are two layers of access control in the system:

- **Basic Access (Permissions)** — every person is assigned a role (such as "Manager" or "Team
  Member"), and that role comes with a simple list of things they are allowed to do, e.g. "can view
  tasks" or "can create invoices." This is all-or-nothing in nature: either the person can do
  something, or they cannot. It does not care which specific record is being viewed.
- **Access Policies** — more detailed rules that you set up on top of Basic Access. A policy can
  state things such as "Managers may only view tasks belonging to their own team" or "Only HR may
  view salary details, and even they may not view the bank account number." Policies allow you to
  narrow down *which records* a person can see, hide or lock certain fields, or fully allow/deny
  access in special situations.

**How the system decides what a person may do, step by step:**

1. It checks what the person is attempting to do (for example, "view a task").
2. It looks for any Access Policies set up for that person — either through their Role, or set up
   for that specific User — for that particular Resource Module and Action.
3. **If no policy is set up for that Module + Action**, the system simply falls back to Basic
   Access (the person's role permissions).
4. **If one or more policies are set up**, they take over completely for that Module + Action. If
   even one policy explicitly says "Deny," the person is blocked, no matter how many other policies
   would otherwise have allowed it. **A Deny policy always wins over an Allow policy.**
5. If access is allowed, the system also applies the finer details: which specific records the
   person may see (Row Filter, Section 4) and whether any fields are hidden or locked (Field
   Restrictions, Section 5).

**Kindly note**: the menus and buttons a person sees on screen reflect their Basic Access plus a
summary of their policies. So a person may see a "Tasks" menu because a policy allows them to view
*some* tasks, even if that policy only ever shows them a single record. Seeing a menu item does not
always mean the person can see everything behind it — the actual check is done afresh each time
they open a screen or a record.

---

## 2. The Parts of a Policy

Every policy is one rule which answers the following: **for this Resource Module and Action,
applicable to this Role or User, should access be Allowed or Denied — and if Allowed, which records
may they see, and what may they see or change in each one?**

These are the same fields you shall find on the Create/Edit Policy form:

| Field on the form | What it means |
|---|---|
| Resource Module | Which area of the system this policy governs (see the table of modules below). |
| Allowed Action | The action being governed — usually \`create\`, \`read\`, \`update\`, or \`delete\`. |
| Effect | \`Allow\` or \`Deny\`. A Deny policy always wins over an Allow policy. |
| Target Subject (Role/User) | Whether this policy applies to an entire Role or to one specific User — never both at once. |
| Evaluation Priority | If two policies disagree on the same field restriction, the one with the lower priority number wins. |
| Row-Level Filter | Decides which specific records the person may see (Section 4). |
| Is Active / Expiry Date | Switches the policy on/off, or makes it stop working automatically after a chosen date. |
| Policy Conditions | A separate, optional check that decides whether the policy applies at all (Section 3). |
| Field Restrictions | Hides, partly hides, or locks specific fields on a record (Section 5). |

**The most common confusion — Conditions vs. Row Filter:**

- **Conditions** answer: *"Does this policy even apply to this person right now?"* — for example,
  "only switch on this policy for people in the Finance department."
- **Row Filter** answers: *"Out of everything this policy allows, which specific records may they
  actually see?"* — for example, "only show tasks owned by the person or their team."

Most policies only require a Row Filter. Conditions are meant for the rarer case where the entire
policy should switch on only for a particular set of people.

**Resource Modules available in the dropdown**, and what each one governs:

| Module Name (Code) | Governs |
|---|---|
| User Management (\`USER_MGMT\`) | Users / employee accounts |
| Department Management (\`DEPT_MGMT\`) | Departments |
| Role Management (\`ROLE_MGMT\`) | Roles and role assignments |
| Task Management (\`TASK_MGMT\`) | Tasks |
| Project Management (\`PROJECT_MGMT\`) | Projects and Teams |
| HR Management (\`HR_MGMT\`) | Employee HR profile — salary, bank details, PAN, etc. |
| Leave Management (\`LEAVE_MGMT\`) | Leave requests |
| CRM (\`CRM\`) | Leads |
| Accounts (\`ACCOUNTS\`) | Invoices |
| Policy Management (\`POLICY_MGMT\`) | Policies themselves |

For most of the above modules, the Field dropdown (in both Row Filter and Conditions) is filled in
automatically with the real field names available on that module — kindly refer to Section 6 for
further detail on this.

---

## 3. Conditions — Should This Policy Apply At All?

Conditions are simple comparisons, entered one at a time using the "Add Condition" form, and
organised into groups using the **Group** number. Conditions in the **same** group must **all** be
true together (this behaves as "AND"). Conditions in **different** groups are treated as
alternatives — if **any one** group is fully true, the policy applies (this behaves as "OR").

**Example** — "This policy applies to Finance department users, OR to anyone holding the Team Lead
role":

| Group | Attribute | Operator | Value |
|---|---|---|---|
| 1 | \`user.DepartmentCode\` | \`eq\` | \`FIN\` |
| 2 | \`user.RoleCode\` | \`eq\` | \`TL\` |

The **Left Attribute** field accepts free text, though a dropdown of suggestions (prefixed
\`user.\` for your own attributes and \`resource.\` for the record's attributes) is provided to assist
you — kindly make use of these suggestions rather than typing from memory, so as to avoid spelling
mistakes.

The **Operator** dropdown offers the following options: \`eq\` (Equals), \`neq\` (Not Equals),
\`contains\` (Contains), \`in\` (In a list, comma-separated), \`not_in\` (Not In a list), \`gt\` (Greater
Than), \`lt\` (Less Than), \`gte\` (Greater or Equal), \`lte\` (Less or Equal).

**Kindly note**: when a person is viewing a *list* of records (such as a task list), conditions
that refer to the record itself (\`resource.*\`) are not checked at that point, as there is no single
record to check them against as yet — only conditions about the person themself (\`user.*\`) are
checked for lists. In case you are trying to restrict who may view a list altogether, kindly ensure
your condition is based on \`user.*\`, not \`resource.*\`.

---

## 4. Row Filter — Which Records May They See?

The Row Filter is where you narrow down exactly which records a person is permitted to see. Unlike
Conditions, this is built visually using the Row Filter builder on the Create/Edit Policy form —
there is no need to type JSON by hand, though it helps to understand what is being generated behind
the scenes, as this same JSON is what gets saved, and can be viewed again later using the **"View
raw JSON"** option on the Policy Details screen.

**Building blocks, as shown on the form:**

- A **Clause** (the "+ Clause" button) checks one single thing, e.g. "Owner equals me."
- A **Group** (the "+ Group" button) combines several Clauses (or even other Groups) together,
  using the AND/OR toggle at the top of the group:
  - **AND** — every item inside the group must be true.
  - **OR** — at least one item inside the group must be true.
- Groups may be nested inside other Groups, so fairly detailed logic can be built up, e.g. "(Owned
  by me OR owned by my team) AND (not Archived)."

**Each Clause has the following parts, matching the form exactly:**

1. **Field** — the piece of record information being checked, e.g. \`OwnerId\`, \`Status\`,
   \`DepartmentId\`. This dropdown is filled in automatically for the module selected (see Section
   6); in case the module does not yet support this, a free-text box is shown instead, along with a
   note asking you to double-check the spelling.
2. **Operator** — same set of codes as mentioned in Section 3 above: \`eq\`, \`neq\`, \`contains\`, \`in\`,
   \`not_in\`, \`gt\`, \`lt\`, \`gte\`, \`lte\`.
3. **Value**, chosen using one of the three buttons provided:
   - **Static** — a fixed value typed by you, e.g. \`Active\` or \`1000\`.
   - **My Attribute** — automatically fills in with information about the person viewing the
     records (their own \`Id\`, \`DepartmentId\`, \`DesignationId\`, \`TeamId\`, \`ManagerId\`,
     \`ReportingOfficerId\`), so that the comparison always adjusts itself to whoever is signed in.
   - **My Subordinates** — automatically matches everyone reporting to the person viewing the
     records, including people reporting to their reports (i.e. their entire team, not merely
     direct reports). This button is only enabled when the Operator is \`in\` or \`not_in\`.

### What this looks like as JSON (for your reference)

A single Clause is saved like this:

\`\`\`jsonc
{ "field": "OwnerId", "operator": "eq", "value": "{user.Id}", "valueType": "dynamic" }
\`\`\`

This means: "OwnerId must equal my own Id" — in other words, only records owned by me shall be
shown. The \`{user.Id}\` portion is what "My Attribute → Id" produces automatically; you need not
type this yourself.

A Group with two Clauses joined by AND is saved like this:

\`\`\`jsonc
{
  "logic": "AND",
  "children": [
    { "field": "DepartmentId", "operator": "eq", "value": "{user.DepartmentId}", "valueType": "dynamic" },
    { "field": "Status", "operator": "neq", "value": "Archived" }
  ]
}
\`\`\`

This means: "Same department as me, AND status is not Archived." Kindly notice that the second
Clause has no \`valueType\` — this is because it uses a **Static** value (\`Archived\`), typed in
directly, rather than "My Attribute."

Groups may be nested. The following shows an AND group containing a nested OR group:

\`\`\`jsonc
{
  "logic": "AND",
  "children": [
    { "field": "OwnerId", "operator": "in", "value": "{user.SubordinateIds}", "valueType": "dynamic" },
    {
      "logic": "OR",
      "children": [
        { "field": "Status", "operator": "neq", "value": "Archived" },
        { "field": "Priority", "operator": "eq", "value": "Urgent" }
      ]
    }
  ]
}
\`\`\`

This means: "Owned by one of my subordinates, AND (status is not Archived OR priority is Urgent)."
The \`{user.SubordinateIds}\` value is what "My Subordinates" produces automatically.

**If more than one policy allows access to the same kind of record**, the results are combined so
that the person may see anything either policy allows. If any matching policy has no Row Filter at
all, that person may see everything for that action — kindly be careful about leaving a Row Filter
empty unless you genuinely intend "show everything."

**A note on "My Subordinates":** it is meaningful only with \`in\` / \`not_in\`, since it represents a
group of people, not a single value. Using it with \`eq\` or any other operator shall simply cause
access to be blocked, rather than the system guessing your intention.

In case you require only a person's **direct reports** (and not their entire team, several levels
down), kindly do not use "My Subordinates" — instead, set Field to \`ManagerId\` and Value to "My
Attribute → Id."

---

## 5. Field Restrictions — What May They See or Change in Each Record?

Once a person is permitted to see a record, you may further control what they may see or change,
field by field, using the "Add Lock" form on the Policy Details screen:

- **Hide** — the field is left blank for them. They shall not even know that a value exists there.
- **Mask** — applicable to text fields only; part of the value is shown and the remainder is
  covered up, e.g. a salary may be shown as \`****500\`.
- **Read-Only** — they may view the current value, but if they attempt to save a *different* value,
  the request shall be rejected. Saving the very same value back is permitted — this commonly
  happens when a form re-submits the whole record without actually changing that particular field.

In case more than one policy restricts the same field in different ways, the strictest restriction
wins: Hide is stricter than Mask, which is stricter than Read-Only.

Field names entered here must match the actual field name on the record (e.g. \`Salary\`, not
\`salary_amount\`) — matching is not case-sensitive, but the field must genuinely exist.

---

## 6. What Information Can I Use in a Rule?

While setting up a Field or a Value in the Row Filter or Conditions, the picker on the form shall
show you the exact options available for the Resource Module selected — there is no need to guess
or memorise anything, kindly simply choose from what is offered.

There are two kinds of information you may refer to:

- **Record fields (\`resource.*\` in Conditions, or the plain Field dropdown in Row Filter)** —
  details stored on the record itself, e.g. \`OwnerId\`, \`Status\`, \`DepartmentId\`. What is available
  depends on the Resource Module chosen (refer to the table in Section 2). A few modules do not yet
  have a ready-made list — in such a case, kindly type the field name yourself, and double-check
  the spelling carefully, as an incorrect field name may cause the policy to silently not work as
  expected.
- **My Attribute (\`user.*\` in Conditions)** — always the same short list, irrespective of which
  Resource Module you are working in:

  | My Attribute option | Meaning |
  |---|---|
  | \`Id\` | Your own User ID |
  | \`DepartmentId\` | The Department you belong to |
  | \`DesignationId\` | Your Designation |
  | \`TeamId\` | Your Team |
  | \`ManagerId\` | Your Manager |
  | \`ReportingOfficerId\` | Your Reporting Officer |
  | \`SubordinateIds\` | Everyone reporting to you, directly or indirectly — see "My Subordinates" in Section 4 |

  This list is kept short and safe on purpose, and shall never include anything sensitive, such as
  passwords or login details.

---

## 7. Example — A Real-World Scenario

**Goal**: A Department Manager should be permitted to grant or remove specific permissions for
their own team members — but only permissions related to Projects, Tasks, and Leave, only for
people reporting to them, and never for anything else (such as CRM access or complete User
Management access).

**How this is set up, using the Create Policy form:**

- **Resource Module**: \`USER_MGMT\`, **Allowed Action**: \`ManagePermissions\`, **Target Subject**:
  Role → Department Manager, **Effect**: Allow.
- **Row Filter**: the person whose permissions are being changed must be one of the Department
  Manager's subordinates:

  \`\`\`jsonc
  { "field": "Id", "operator": "in", "value": "{user.SubordinateIds}", "valueType": "dynamic" }
  \`\`\`

- **Condition**: the permission being granted or removed must belong to Projects, Tasks, or Leave —
  set up on the Policy Details screen as follows:

  | Group | Attribute | Operator | Value |
  |---|---|---|---|
  | 1 | \`resource.PermissionModuleCode\` | \`in\` | \`PROJECT_MGMT,TASK_MGMT,LEAVE_MGMT\` |

**Why this works:**

- The Row Filter ensures a Department Manager may only touch the permissions of people within their
  own reporting chain — never anyone outside their team.
- The Condition ensures that, even for their own team, a Department Manager may only touch
  Project/Task/Leave-related permissions — nothing else.
- As no separate policy was created for CRM or other modules, Department Managers simply fall back
  to their Basic Access for those areas — which, unless separately granted, amounts to nothing.

**The result**: Department Managers see a "manage permissions" screen showing only the relevant
toggles, and only for their own team — exactly as intended.

---

## 8. Common Mistakes to Avoid

The system is designed to be cautious by default: in case a policy is unclear, incomplete, or
refers to something that does not exist, it shall **block access or hide the record**, rather than
accidentally showing something it should not. Kindly keep the following in mind:

- **Misspelled field name** — if you type a field name instead of choosing it from the dropdown,
  and it does not match a real field on that module, the form shall stop you from saving the policy
  (for modules that have a ready-made list). For modules without a list as yet, or for "My
  Attribute" values, a typing mistake shall not be caught automatically — kindly double-check
  spelling before saving.
- **Using "My Subordinates" incorrectly** — kindly remember, it works only with \`in\` / \`not_in\`.
  Using it with \`eq\` or any other operator shall always result in access being blocked.
- **An empty or broken Row Filter** — an incomplete or invalid Row Filter is treated as "show
  nothing," not "show everything." In case a policy appears to be hiding all records unexpectedly,
  kindly check the Row Filter first.
- **A Deny policy blocking things unexpectedly** — kindly remember, Deny always wins over Allow. In
  case someone is unable to see something you expect them to, kindly check whether a Deny policy
  exists for that same Module and Action, before assuming the Allow policy's Row Filter is at
  fault. The **Preview** feature may be used to see exactly which policies matched, and why.
- **Conditions not working on lists** — as mentioned in Section 3, conditions referring to the
  record itself (\`resource.*\`) are skipped when a person is viewing a list. Kindly use the Row
  Filter instead, in case you need to control what appears in a list.
- **A short delay after making changes** — changes to permissions and to the reporting chain
  (Manager assignments) may take a few minutes to fully take effect everywhere. This is normal, and
  shall resolve itself shortly on its own; there is no need to raise a concern for this.
`;

/** Exact heading text (must match a "## N. ..." line above) used to compute deep-link anchors into the guide. */
export const GUIDE_HEADINGS = {
  overview: '1. How Access Policies Work',
  anatomy: '2. The Parts of a Policy',
  conditions: '3. Conditions — Should This Policy Apply At All?',
  rowFilter: '4. Row Filter — Which Records May They See?',
  fieldRestrictions: '5. Field Restrictions — What May They See or Change in Each Record?',
  attributes: '6. What Information Can I Use in a Rule?',
  example: '7. Example — A Real-World Scenario',
  pitfalls: '8. Common Mistakes to Avoid',
} as const;
