import axiosInstance from './axiosInstance';
import type { ApiResponse, PaginatedResult } from '@/types/api.types';

export interface RoleListItem {
  id: string;
  name: string;
  code: string;
  priority: number;
  description?: string;
  isSystemRole: boolean;
}

export interface RolesQueryParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
}

export interface CreateRolePayload {
  name: string;
  code: string;
  priority: number;
  description?: string;
}

export interface UpdateRolePayload {
  name: string;
  description?: string;
  priority: number;
}

export interface PermissionOverridePayload {
  permissionCode: string;
  isGranted: boolean;
  reason?: string;
  expiresAt?: string;
}

/**
 * GET /api/roles
 * Fetches paginated list of roles.
 */
export async function getRoles(params: RolesQueryParams): Promise<ApiResponse<PaginatedResult<RoleListItem>>> {
  const query: Record<string, string> = {
    pageNumber: String(params.pageNumber),
    pageSize: String(params.pageSize),
  };
  if (params.search) query.search = params.search;

  const { data } = await axiosInstance.get<ApiResponse<PaginatedResult<RoleListItem>>>('/roles', { params: query });
  return data;
}

/**
 * POST /api/roles
 * Creates a new custom role.
 */
export async function createRole(payload: CreateRolePayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/roles', payload);
  return data;
}

/**
 * PUT /api/roles/{id}
 * Updates role metadata.
 */
export async function updateRole(id: string, payload: UpdateRolePayload): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.put<ApiResponse<boolean>>(`/roles/${id}`, payload);
  return data;
}

export interface RolePermissionsData {
  assignedPermissions: string[];
  manageablePermissions: string[];
}

/**
 * GET /api/roles/{id}/permissions
 * Fetches permissions assigned to a role.
 */
export async function getRolePermissions(id: string): Promise<ApiResponse<RolePermissionsData>> {
  const { data } = await axiosInstance.get<ApiResponse<RolePermissionsData>>(`/roles/${id}/permissions`);
  return data;
}

/**
 * POST /api/roles/{id}/permissions
 * Assigns a permission to a role.
 */
export async function assignRolePermission(id: string, permissionCode: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.post<ApiResponse<boolean>>(`/roles/${id}/permissions`, { permissionCode });
  return data;
}

/**
 * DELETE /api/roles/{id}/permissions/{permissionCode}
 * Removes a permission from a role.
 */
export async function removeRolePermission(id: string, permissionCode: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/roles/${id}/permissions/${permissionCode}`);
  return data;
}

/**
 * POST /api/users/{userId}/roles
 * Assigns a role to a user.
 */
export async function assignUserRole(userId: string, roleId: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.post<ApiResponse<boolean>>(`/users/${userId}/roles`, { roleId });
  return data;
}

/**
 * DELETE /api/users/{userId}/roles/{roleId}
 * Removes a role from a user.
 */
export async function removeUserRole(userId: string, roleId: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/users/${userId}/roles/${roleId}`);
  return data;
}

/**
 * POST /api/users/{userId}/permissions/override
 * Adds or updates a user permission override.
 */
export async function addPermissionOverride(userId: string, payload: PermissionOverridePayload): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.post<ApiResponse<boolean>>(`/users/${userId}/permissions/override`, payload);
  return data;
}

/**
 * DELETE /api/users/{userId}/permissions/override/{permissionCode}
 * Removes a user permission override.
 */
export async function removePermissionOverride(userId: string, permissionCode: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/users/${userId}/permissions/override/${permissionCode}`);
  return data;
}

export interface UserPermissionOverrideItem {
  permissionCode: string;
  isGranted: boolean;
  reason?: string;
  expiresAt?: string;
}

export interface UserPermissionOverridesData {
  overrides: UserPermissionOverrideItem[];
  /** Permission codes the CURRENT caller (viewer) is allowed to grant/revoke for this target user. ADMIN gets every code. */
  manageablePermissions: string[];
}

/**
 * GET /api/users/{userId}/permissions/overrides
 * Retrieves permission overrides for a user, plus the set of permission
 * codes the current caller is allowed to manage for that user.
 */
export async function getUserPermissionOverrides(userId: string): Promise<ApiResponse<UserPermissionOverridesData>> {
  const { data } = await axiosInstance.get<ApiResponse<UserPermissionOverridesData>>(`/users/${userId}/permissions/overrides`);
  return data;
}

export interface UserRoleItem {
  id: string;
  name: string;
  description?: string;
}

/**
 * GET /api/users/{userId}/roles
 * Retrieves assigned roles for a user.
 */
export async function getUserRoles(userId: string): Promise<ApiResponse<UserRoleItem[]>> {
  const { data } = await axiosInstance.get<ApiResponse<UserRoleItem[]>>(`/users/${userId}/roles`);
  return data;
}
