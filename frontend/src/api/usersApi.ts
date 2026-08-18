import axiosInstance from './axiosInstance';
import type { ApiResponse, PaginatedResult } from '@/types/api.types';
import type { UserListItem, UsersQueryParams, InviteUserPayload, InviteMeta, EligibleUser } from '@/features/users/types/user.types';



/**
 * GET /api/users
 * Fetches paginated user list with optional search, role filter, and status filter.
 */
export async function getUsers(params: UsersQueryParams): Promise<ApiResponse<PaginatedResult<UserListItem>>> {
  const query: Record<string, string> = {
    pageNumber: String(params.pageNumber),
    pageSize: String(params.pageSize),
  };

  if (params.search) query.search = params.search;
  if (params.role) query.role = params.role;
  if (params.isActive !== null && params.isActive !== undefined) {
    query.isActive = String(params.isActive);
  }
  if (params.teamId) query.teamId = params.teamId;
  if (params.departmentId) query.departmentId = params.departmentId;

  const { data } = await axiosInstance.get<ApiResponse<PaginatedResult<UserListItem>>>('/users', { params: query });
  return data;
}

/**
 * POST /api/users
 * Invites/creates a new user.
 */
export async function inviteUser(payload: InviteUserPayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/users', payload);
  return data;
}

/**
 * GET /api/users/invite-meta
 * Fetches departments, designations, and roles.
 */
export async function getInviteMeta(): Promise<ApiResponse<InviteMeta>> {
  const { data } = await axiosInstance.get<ApiResponse<InviteMeta>>('/users/invite-meta');
  return data;
}

export interface UpdateUserPayload {
  firstName: string;
  lastName: string;
  phoneNumber?: string | null;
  departmentId: string;
  designationId: string;
  isActive: boolean;
  roleIds: string[];
}

/**
 * PUT /api/users/{id}
 * Updates user profile and roles.
 */
export async function updateUser(id: string, payload: UpdateUserPayload): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.put<ApiResponse<boolean>>(`/users/${id}`, payload);
  return data;
}

/**
 * DELETE /api/users/{id}
 * Soft deletes user and revokes active sessions.
 */
export async function deleteUser(id: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/users/${id}`);
  return data;
}

/**
 * GET /api/users/{id}/permissions
 * Retrieve effective permissions of a user.
 */
export async function getUserPermissions(id: string): Promise<ApiResponse<string[]>> {
  const { data } = await axiosInstance.get<ApiResponse<string[]>>(`/users/${id}/permissions`);
  return data;
}

/**
 * GET /api/users/{id}/roles
 * Retrieve roles assigned to a user.
 */
export async function getUserRoles(id: string): Promise<ApiResponse<Array<{ id: string; name: string; description?: string }>>> {
  const { data } = await axiosInstance.get<ApiResponse<Array<{ id: string; name: string; description?: string }>>>(`/users/${id}/roles`);
  return data;
}

/**
 * GET /api/users/eligible-managers
 */
export async function getEligibleManagers(departmentId: string): Promise<ApiResponse<EligibleUser[]>> {
  const { data } = await axiosInstance.get<ApiResponse<EligibleUser[]>>('/users/eligible-managers', {
    params: { departmentId },
  });
  return data;
}

/**
 * GET /api/users/eligible-reporting-officers
 */
export async function getEligibleReportingOfficers(departmentId: string, invitedDesignationLevel: number, managerId: string): Promise<ApiResponse<EligibleUser[]>> {
  const { data } = await axiosInstance.get<ApiResponse<EligibleUser[]>>('/users/eligible-reporting-officers', {
    params: { departmentId, invitedDesignationLevel, managerId },
  });
  return data;
}


