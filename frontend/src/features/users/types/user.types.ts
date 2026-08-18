export interface UserListItem {
  id: string;
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string | null;
  isActive: boolean;
  departmentName: string;
  designationName: string;
  roles: string[];
}

export interface UsersQueryParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  role?: string;
  isActive?: boolean | null;
  teamId?: string;
  departmentId?: string;
}
export interface InviteUserPayload {
  employeeCode: string;
  firstName: string;
  lastName: string;
  email: string;
  username: string;
  password?: string;
  phoneNumber?: string;
  departmentId: string;
  designationId: string;
  roleIds: string[];
  managerId?: string;
  reportingOfficerId?: string;
}

export interface DepartmentLookup {
  id: string;
  name: string;
}

export interface DesignationLookup {
  id: string;
  departmentId: string;
  name: string;
  level: number;
}

export interface RoleLookup {
  id: string;
  name: string;
  description: string | null;
}

export interface InviteMeta {
  departments: DepartmentLookup[];
  designations: DesignationLookup[];
  roles: RoleLookup[];
}

export interface EligibleUser {
  id: string;
  firstName: string;
  lastName: string;
  designationName: string;
  designationLevel: number;
}

