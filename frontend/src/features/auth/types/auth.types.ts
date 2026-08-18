export interface AuthResponse {
  accessToken: string | null;
  requiresMfa: boolean;
}

/** Login request payload */
export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe?: boolean;
  mfaCode?: string;
}

/** User info decoded from JWT */
export interface CurrentUser {
  userId: string;
  email: string;
  username: string;
  employeeCode: string;
  department: string;
  designation: string;
  roles: string[];
  permissions: string[];
}
