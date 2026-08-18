import axiosInstance from './axiosInstance';
import type { ApiResponse, PaginatedResult } from '@/types/api.types';
import type {
  ProjectListItem,
  ProjectDetail,
  ProjectMember,
  ProjectsQueryParams,
  CreateProjectPayload,
  UpdateProjectPayload,
  AddProjectMemberPayload,
  ProjectCreateMeta,
} from '@/types/project.types';

/**
 * GET /api/projects/create-meta
 * Returns the creator's department and the Tech Leads available to lead a project.
 */
export async function getProjectCreateMeta(): Promise<ApiResponse<ProjectCreateMeta>> {
  const { data } = await axiosInstance.get<ApiResponse<ProjectCreateMeta>>('/projects/create-meta');
  return data;
}

/**
 * GET /api/projects
 * Fetches paginated project list.
 */
export async function getProjects(
  params: ProjectsQueryParams
): Promise<ApiResponse<PaginatedResult<ProjectListItem>>> {
  const query: Record<string, string> = {
    pageNumber: String(params.pageNumber),
    pageSize: String(params.pageSize),
  };

  if (params.search) query.search = params.search;
  if (params.status) query.status = params.status;

  const { data } = await axiosInstance.get<ApiResponse<PaginatedResult<ProjectListItem>>>(
    '/projects',
    { params: query }
  );
  return data;
}

/**
 * GET /api/projects/{id}
 * Fetches a single project's detail.
 */
export async function getProjectById(id: string): Promise<ApiResponse<ProjectDetail>> {
  const { data } = await axiosInstance.get<ApiResponse<ProjectDetail>>(`/projects/${id}`);
  return data;
}

/**
 * POST /api/projects
 * Creates a new project.
 */
export async function createProject(payload: CreateProjectPayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/projects', payload);
  return data;
}

/**
 * PUT /api/projects/{id}
 * Updates an existing project.
 */
export async function updateProject(id: string, payload: UpdateProjectPayload): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.put<ApiResponse<boolean>>(`/projects/${id}`, payload);
  return data;
}

/**
 * DELETE /api/projects/{id}
 * Deletes a project.
 */
export async function deleteProject(id: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/projects/${id}`);
  return data;
}

/**
 * GET /api/projects/{id}/members
 * Fetches all members of a project.
 */
export async function getProjectMembers(id: string): Promise<ApiResponse<ProjectMember[]>> {
  const { data } = await axiosInstance.get<ApiResponse<ProjectMember[]>>(`/projects/${id}/members`);
  return data;
}

/**
 * POST /api/projects/{projectId}/members
 * Adds a user as a member to a project.
 */
export async function addProjectMember(
  projectId: string,
  payload: AddProjectMemberPayload
): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.post<ApiResponse<boolean>>(`/projects/${projectId}/members`, payload);
  return data;
}

/**
 * DELETE /api/projects/{projectId}/members/{userId}
 * Removes a member from a project.
 */
export async function removeProjectMember(projectId: string, userId: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/projects/${projectId}/members/${userId}`);
  return data;
}
