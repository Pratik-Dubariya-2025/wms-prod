import type { BaseEntity } from './common.types';

export type LeaveStatus = 'Pending' | 'Approved' | 'Rejected' | 'Cancelled';
export type LeaveType = 'Annual' | 'Sick' | 'Casual' | 'Maternity' | 'Paternity' | 'Unpaid';

export interface LeaveRequest extends BaseEntity {
  userId: string;
  userName: string;
  userEmail: string;
  employeeCode: string;
  leaveType: LeaveType;
  fromDate: string;
  toDate: string;
  daysCount: number;
  reason: string | null;
  status: LeaveStatus;
  approvedByName: string | null;
  approvedAt: string | null;
  rejectionReason: string | null;
}

export interface CreateLeaveRequestCommand {
  leaveType: LeaveType;
  fromDate: string;
  toDate: string;
  daysCount: number;
  reason: string;
}

export interface RejectLeaveRequestCommand {
  rejectionReason: string;
}
