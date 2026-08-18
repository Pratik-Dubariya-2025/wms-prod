import axiosInstance from './axiosInstance';
import type { ApiResponse } from '@/types/api.types';
import type { EmployeeProfile, UpdateEmployeeProfileRequest } from '@/types/hr.types';

/**
 * GET /api/hr/profiles/{userId}
 * Fetches the extended HR and salary profile for a user.
 * Requires salary.read permission.
 */
export async function getEmployeeProfile(userId: string): Promise<ApiResponse<EmployeeProfile>> {
  const { data } = await axiosInstance.get<ApiResponse<EmployeeProfile>>(`/hr/profiles/${userId}`);
  return data;
}

/**
 * PUT /api/hr/profiles/{userId}
 * Creates or updates the extended HR and salary profile for a user.
 * Requires salary.write permission.
 */
export async function updateEmployeeProfile(
  userId: string,
  payload: UpdateEmployeeProfileRequest
): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.put<ApiResponse<boolean>>(`/hr/profiles/${userId}`, payload);
  return data;
}
