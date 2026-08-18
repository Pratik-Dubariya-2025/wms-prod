export interface DepartmentListItem {
  id: string;
  name: string;
  code: string;
  description?: string | null;
  isActive: boolean;
  memberCount: number;
  designationCount: number;
  createdAt: string;
}

export interface DepartmentUser {
  id: string;
  employeeCode: string;
  fullName: string;
  email: string;
  designationName?: string | null;
  roleName?: string | null;
  isActive: boolean;
}

export interface DepartmentDesignation {
  id: string;
  name: string;
  code?: string;
  description?: string | null;
  level: number;
  isActive: boolean;
}

export interface DepartmentDetail {
  id: string;
  name: string;
  code: string;
  description?: string | null;
  isActive: boolean;
  createdAt: string;
  users: DepartmentUser[];
  designations: DepartmentDesignation[];
}

export interface CreateDepartmentPayload {
  name: string;
  code: string;
  description?: string;
}

export interface UpdateDepartmentPayload {
  name: string;
  code: string;
  description?: string;
  isActive: boolean;
}

export interface CreateDesignationPayload {
  name: string;
  code: string;
  description?: string;
  level: number;
}

export interface UpdateDesignationPayload {
  name: string;
  code: string;
  description?: string;
  level: number;
  isActive: boolean;
}
