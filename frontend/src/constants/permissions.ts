/**
 * Permission code constants mirroring backend permission codes.
 * Updated as backend permissions are added.
 */
export const PERMISSIONS = {
  // User Management
  USER_CREATE: 'user.create',
  USER_READ: 'user.read',
  USER_UPDATE: 'user.update',
  USER_DELETE: 'user.delete',

  // Department Management
  DEPARTMENT_CREATE: 'department.create',
  DEPARTMENT_READ: 'department.read',
  DEPARTMENT_UPDATE: 'department.update',
  DEPARTMENT_DELETE: 'department.delete',

  // Role Management
  ROLE_CREATE: 'role.create',
  ROLE_READ: 'role.read',
  ROLE_UPDATE: 'role.update',
  ROLE_DELETE: 'role.delete',

  // Task Management
  TASK_CREATE: 'task.create',
  TASK_READ: 'task.read',
  TASK_UPDATE: 'task.update',
  TASK_DELETE: 'task.delete',

  // Project Management
  PROJECT_CREATE: 'project.create',
  PROJECT_READ: 'project.read',
  PROJECT_UPDATE: 'project.update',
  PROJECT_DELETE: 'project.delete',

  // TimeLog Management
  TIMELOG_CREATE: 'timelog.create',
  TIMELOG_READ: 'timelog.read',
  TIMELOG_APPROVE: 'timelog.approve',

  // HR Management
  SALARY_READ: 'salary.read',
  SALARY_WRITE: 'salary.write',

  // Leave Management
  LEAVE_READ: 'leave.read',
  LEAVE_WRITE: 'leave.write',
  LEAVE_APPROVE: 'leave.approve',

  // CRM / Leads
  LEADS_READ: 'leads.read',
  LEADS_WRITE: 'leads.write',

  // Accounts / Invoices
  INVOICES_READ: 'invoices.read',
  INVOICES_WRITE: 'invoices.write',

  // Policy Management
  POLICY_READ: 'policy.read',
  POLICY_WRITE: 'policy.manage',
} as const;

export type PermissionCode = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];
