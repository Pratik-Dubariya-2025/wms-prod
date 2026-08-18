import axiosInstance from './axiosInstance';
import type { ApiResponse } from '@/types/api.types';
import type {
  DepartmentListItem,
  DepartmentDetail,
  CreateDepartmentPayload,
  UpdateDepartmentPayload,
  CreateDesignationPayload,
  UpdateDesignationPayload
} from '@/features/departments/types/department.types';

/**
 * GET /api/departments
 * Fetch list of departments
 */
export async function getDepartments(params?: {
  search?: string;
  status?: string;
  isActiveOnly?: boolean;
}): Promise<ApiResponse<DepartmentListItem[]>> {
  const queryParams: Record<string, string> = {};
  if (params?.search) {
    queryParams.search = params.search;
  }
  if (params?.status && params.status !== 'all') {
    queryParams.status = params.status;
  }
  if (params?.isActiveOnly !== undefined) {
    queryParams.isActiveOnly = String(params.isActiveOnly);
  }
  const { data } = await axiosInstance.get<ApiResponse<DepartmentListItem[]>>('/departments', { params: queryParams });
  return data;
}

/**
 * GET /api/departments/{id}
 * Fetch department detail including users and designations
 */
export async function getDepartmentById(id: string): Promise<ApiResponse<DepartmentDetail>> {
  const { data } = await axiosInstance.get<ApiResponse<DepartmentDetail>>(`/departments/${id}`);
  return data;
}

/**
 * POST /api/departments
 * Create new department
 */
export async function createDepartment(payload: CreateDepartmentPayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/departments', payload);
  return data;
}

/**
 * PUT /api/departments/{id}
 * Update existing department
 */
export async function updateDepartment(id: string, payload: UpdateDepartmentPayload): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.put<ApiResponse<boolean>>(`/departments/${id}`, payload);
  return data;
}

/**
 * DELETE /api/departments/{id}
 * Soft delete department
 */
export async function deleteDepartment(id: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/departments/${id}`);
  return data;
}

/**
 * POST /api/departments/{departmentId}/designations
 * Add a designation to a department
 */
export async function createDesignation(departmentId: string, payload: CreateDesignationPayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>(`/departments/${departmentId}/designations`, payload);
  return data;
}

/**
 * PUT /api/departments/designations/{id}
 * Update an existing designation
 */
export async function updateDesignation(id: string, payload: UpdateDesignationPayload): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.put<ApiResponse<boolean>>(`/departments/designations/${id}`, payload);
  return data;
}

/**
 * DELETE /api/departments/designations/{id}
 * Soft delete a designation
 */
export async function deleteDesignation(id: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/departments/designations/${id}`);
  return data;
}
