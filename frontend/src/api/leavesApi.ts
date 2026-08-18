import axiosInstance from './axiosInstance';
import type { ApiResponse, PaginatedResult } from '@/types/api.types';
import type {
  LeaveRequest,
  CreateLeaveRequestCommand,
  RejectLeaveRequestCommand,
  LeaveType,
  LeaveStatus
} from '@/types/leave.types';

export interface LeaveRequestsQueryParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  status?: LeaveStatus | '';
  leaveType?: LeaveType | '';
  approvalsOnly?: boolean;
  teamOnly?: boolean;
}

/**
 * GET /api/leaves
 * Lists leave requests (scoped dynamically by role on backend).
 * Requires leave.read permission.
 */
export async function getLeaveRequests(params: LeaveRequestsQueryParams): Promise<ApiResponse<PaginatedResult<LeaveRequest>>> {
  const query: Record<string, string> = {
    pageNumber: String(params.pageNumber),
    pageSize: String(params.pageSize),
  };

  if (params.search) query.search = params.search;
  if (params.status) query.status = params.status;
  if (params.leaveType) query.leaveType = params.leaveType;
  if (params.approvalsOnly !== undefined) query.approvalsOnly = String(params.approvalsOnly);
  if (params.teamOnly !== undefined) query.teamOnly = String(params.teamOnly);

  const { data } = await axiosInstance.get<ApiResponse<PaginatedResult<LeaveRequest>>>('/leaves', { params: query });
  return data;
}

/**
 * POST /api/leaves
 * Submits a new leave request.
 * Requires leave.write permission.
 */
export async function createLeaveRequest(payload: CreateLeaveRequestCommand): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/leaves', payload);
  return data;
}

/**
 * PATCH /api/leaves/{id}/approve
 * Approves a pending leave request.
 * Requires leave.approve permission.
 */
export async function approveLeaveRequest(id: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.patch<ApiResponse<boolean>>(`/leaves/${id}/approve`);
  return data;
}

/**
 * PATCH /api/leaves/{id}/reject
 * Rejects a pending leave request (rejection reason required).
 * Requires leave.approve permission.
 */
export async function rejectLeaveRequest(
  id: string,
  payload: RejectLeaveRequestCommand
): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.patch<ApiResponse<boolean>>(`/leaves/${id}/reject`, payload);
  return data;
}
