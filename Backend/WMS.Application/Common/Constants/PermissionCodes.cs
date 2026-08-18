namespace WMS.Application.Common.Constants
{
    /// <summary>
    /// Centralized permission code constants matching the database seed.
    /// Use these instead of magic strings in handlers.
    /// </summary>
    public static class PermissionCodes
    {
        // User Management
        public const string UserCreate = "user.create";
        public const string UserRead = "user.read";
        public const string UserUpdate = "user.update";
        public const string UserDelete = "user.delete";

        // Department Management
        public const string DepartmentCreate = "department.create";
        public const string DepartmentRead = "department.read";
        public const string DepartmentUpdate = "department.update";
        public const string DepartmentDelete = "department.delete";

        // Role Management
        public const string RoleCreate = "role.create";
        public const string RoleRead = "role.read";
        public const string RoleUpdate = "role.update";
        public const string RoleDelete = "role.delete";

        // Delegated per-user permission management (distinct from RoleUpdate so it can be
        // scoped independently via PBAC, e.g. to a manager's subordinates + specific modules).
        public const string UserPermissionOverrideManage = "permission.override.manage";

        // Task Management
        public const string TaskCreate = "task.create";
        public const string TaskRead = "task.read";
        public const string TaskUpdate = "task.update";
        public const string TaskDelete = "task.delete";

        // Project Management
        public const string ProjectCreate = "project.create";
        public const string ProjectRead = "project.read";
        public const string ProjectUpdate = "project.update";
        public const string ProjectDelete = "project.delete";
        public const string ProjectMemberAdd = "project.member.add";
        public const string ProjectMemberRemove = "project.member.remove";

        // TimeLog Management
        public const string TimeLogCreate = "timelog.create";
        public const string TimeLogRead = "timelog.read";
        public const string TimeLogApprove = "timelog.approve";

        // HR / Salary Management
        public const string SalaryRead = "salary.read";
        public const string SalaryWrite = "salary.write";

        // Leave Management
        public const string LeaveRead = "leave.read";
        public const string LeaveWrite = "leave.write";
        public const string LeaveApprove = "leave.approve";

        // CRM / Leads
        public const string LeadsRead = "leads.read";
        public const string LeadsWrite = "leads.write";

        // Accounts / Invoices
        public const string InvoicesRead = "invoices.read";
        public const string InvoicesWrite = "invoices.write";

        // Policy Management
        public const string PolicyRead = "policy.read";
        public const string PolicyWrite = "policy.manage";
    }
}
