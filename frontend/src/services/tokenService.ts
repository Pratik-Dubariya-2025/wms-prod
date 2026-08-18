/**
 * Decode a JWT access token payload (without verification).
 * Used client-side to read claims like user info, roles, permissions.
 */
export function decodeJwt<T = Record<string, unknown>>(token: string): T | null {
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;

    const payload = parts[1];
    const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decoded) as T;
  } catch {
    return null;
  }
}

/** .NET serializes ClaimTypes.Role to this schema URI key in the JWT. */
const ROLE_CLAIM = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';

/** Standard JWT payload claims */
export interface JwtPayload {
  sub: string; // UserId
  email: string;
  Username: string; // Username
  EmployeeCode: string;
  Department: string;
  Designation: string;
  Role?: string | string[];
  [ROLE_CLAIM]?: string | string[];
  Permissions: string | string[];
  IsMfaEnabled: string;
  exp: number;
  iss: string;
  aud: string;
}

/**
 * Check whether a JWT token is expired.
 * Returns true if expired or invalid.
 */
export function isTokenExpired(token: string): boolean {
  const payload = decodeJwt<JwtPayload>(token);
  if (!payload?.exp) return true;

  // exp is in seconds, Date.now() is in milliseconds
  return Date.now() >= payload.exp * 1000;
}

/**
 * Extract user info from JWT claims.
 */
export function getUserFromToken(token: string) {
  const payload = decodeJwt<JwtPayload>(token);
  if (!payload) return null;

  // Roles come from the .NET ClaimTypes.Role schema-URI key (fallback to "Role").
  const rawRoles = payload[ROLE_CLAIM] ?? payload.Role;

  return {
    userId: payload.sub,
    email: payload.email,
    username: payload.Username,
    employeeCode: payload.EmployeeCode,
    department: payload.Department,
    designation: payload.Designation,
    roles: Array.isArray(rawRoles) ? rawRoles : rawRoles ? [rawRoles] : [],
    permissions: Array.isArray(payload.Permissions) ? payload.Permissions : payload.Permissions ? [payload.Permissions] : [],
    isMfaEnabled: payload.IsMfaEnabled === 'true',
  };
}
