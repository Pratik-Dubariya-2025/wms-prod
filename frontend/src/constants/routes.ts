/** Centralized route path constants */
export const ROUTES = {
  // Auth
  LOGIN: '/login',
  VERIFY_EMAIL: '/verify-email',
  FORGOT_PASSWORD: '/forgot-password',
  RESET_PASSWORD: '/reset-password',
  CHANGE_PASSWORD: '/change-password',



  // Dashboard
  DASHBOARD: '/',

  // User
  USER: '/users',

  // Role
  ROLE: '/roles',
  POLICIES: '/policies',
  
  // Department
  DEPARTMENT: '/departments',

  // Project
  PROJECTS: '/projects',
  PROJECT_DETAILS: '/projects/:id',

  // HR & Leaves
  HR_PROFILE: '/hr/profile/:userId',
  LEAVES: '/leaves',

  // CRM & Accounts
  LEADS: '/crm/leads',
  INVOICES: '/accounts/invoices',

  // CSS Guide (development reference)
  CSS_GUIDE: '/css-guide',
} as const;
