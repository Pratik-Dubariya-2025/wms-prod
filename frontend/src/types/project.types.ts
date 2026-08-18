export interface ProjectListItem {
  id: string;
  name: string;
  status: string;
  description: string | null;
  startDate: string | null;
  endDate: string | null;
  ownerName: string;
  teamLeadName: string | null;
  departmentName: string;
  memberCount: number;
  taskCount: number;
  createdAt: string;
}

export interface TaskStatusSummary {
  total: number;
  draft: number;
  inProgress: number;
  inReview: number;
  done: number;
  closed: number;
}

export interface ProjectDetail {
  id: string;
  name: string;
  description: string | null;
  status: string;
  startDate: string | null;
  endDate: string | null;
  departmentId: string;
  departmentName: string;
  ownerId: string;
  ownerName: string;
  teamLeadId: string | null;
  teamLeadName: string | null;
  teamId: string | null;
  memberCount: number;
  taskSummary: TaskStatusSummary;
  createdAt: string;
  createdBy: string | null;
  modifiedAt: string | null;
  modifiedBy: string | null;
}

export interface ProjectMember {
  id: string;
  userId: string;
  fullName: string;
  email: string;
  designationName: string;
  joinedAt: string;
}

export interface ProjectsQueryParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  status?: string;
}

export interface TeamLeadOption {
  id: string;
  fullName: string;
  teamId: string | null;
  teamName: string | null;
}

export interface ProjectCreateMeta {
  departmentId: string;
  departmentName: string;
  teamLeads: TeamLeadOption[];
}

export interface CreateProjectPayload {
  name: string;
  description?: string;
  status?: string;
  startDate?: string | null;
  endDate?: string | null;
  // Department is derived server-side from the creator; not sent from the form.
  teamLeadId?: string | null;
}

export interface UpdateProjectPayload {
  name: string;
  description?: string;
  status: string;
  startDate?: string | null;
  endDate?: string | null;
  departmentId: string;
  teamLeadId?: string | null;
}

export interface AddProjectMemberPayload {
  userId: string;
}
