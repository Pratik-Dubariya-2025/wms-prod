import type { BaseEntity } from './common.types';

export interface EmployeeProfile extends BaseEntity {
  userId: string;
  userName: string;
  userEmail: string;
  employeeCode: string;
  salary: number;
  bankAccountNo: string | null;
  bankIfsc: string | null;
  panNumber: string | null;
  aadhaarLast4: string | null;
  emergencyContactName: string | null;
  emergencyContactPhone: string | null;
  bloodGroup: string | null;
  address: string | null;
}

export interface UpdateEmployeeProfileRequest {
  salary: number;
  bankAccountNo?: string;
  bankIfsc?: string;
  panNumber?: string;
  aadhaarLast4?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  bloodGroup?: string;
  address?: string;
}
