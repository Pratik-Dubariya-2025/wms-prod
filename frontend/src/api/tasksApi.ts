import axiosInstance from './axiosInstance';
import type { ApiResponse, PaginatedResult } from '@/types/api.types';
import type {
  TaskListItem,
  TasksQueryParams,
  TaskDetail,
  CreateTaskPayload,
  TaskCreateMeta,
  UpdateTaskPayload,
  UpdateTaskStatusPayload,
  TimeLog,
  CreateTimeLogPayload,
} from '@/features/tasks/types/task.types';

/**
 * GET /api/projects/{projectId}/tasks
 * Fetches paginated task list for a specific project.
 */
export async function getTasks(
  projectId: string,
  params: TasksQueryParams
): Promise<ApiResponse<PaginatedResult<TaskListItem>>> {
  const query: Record<string, string> = {
    pageNumber: String(params.pageNumber),
    pageSize: String(params.pageSize),
  };

  if (params.search) query.search = params.search;
  if (params.status) query.status = params.status;
  if (params.priority) query.priority = params.priority;
  if (params.assigneeId) query.assigneeId = params.assigneeId;

  const { data } = await axiosInstance.get<ApiResponse<PaginatedResult<TaskListItem>>>(
    `/projects/${projectId}/tasks`,
    { params: query }
  );
  return data;
}

/**
 * GET /api/tasks/{id}
 * Fetches a single task's full detail.
 */
export async function getTaskById(id: string): Promise<ApiResponse<TaskDetail>> {
  const { data } = await axiosInstance.get<ApiResponse<TaskDetail>>(`/tasks/${id}`);
  return data;
}

/**
 * GET /api/projects/{projectId}/tasks/create-meta
 * Fetches assignable project members for task creation form.
 */
export async function getTaskCreateMeta(projectId: string): Promise<ApiResponse<TaskCreateMeta>> {
  const { data } = await axiosInstance.get<ApiResponse<TaskCreateMeta>>(`/projects/${projectId}/tasks/create-meta`);
  return data;
}

/**
 * POST /api/projects/{projectId}/tasks
 * Creates a new task within a project.
 */
export async function createTask(
  projectId: string,
  payload: CreateTaskPayload
): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>(`/projects/${projectId}/tasks`, payload);
  return data;
}

/**
 * PUT /api/projects/{projectId}/tasks/{id}
 * Updates an existing task.
 */
export async function updateTask(
  projectId: string,
  id: string,
  payload: UpdateTaskPayload
): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.put<ApiResponse<boolean>>(`/projects/${projectId}/tasks/${id}`, payload);
  return data;
}

/**
 * PATCH /api/projects/{projectId}/tasks/{id}/status
 * Updates the status of an existing task.
 */
export async function updateTaskStatus(
  projectId: string,
  id: string,
  payload: UpdateTaskStatusPayload
): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.patch<ApiResponse<boolean>>(`/projects/${projectId}/tasks/${id}/status`, payload);
  return data;
}

/**
 * DELETE /api/projects/{projectId}/tasks/{id}
 * Soft-deletes a task.
 */
export async function deleteTask(
  projectId: string,
  id: string
): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/projects/${projectId}/tasks/${id}`);
  return data;
}

/**
 * POST /api/projects/{projectId}/tasks/{taskId}/time-logs
 * Logs time against a task.
 */
export async function createTimeLog(
  projectId: string,
  taskId: string,
  payload: CreateTimeLogPayload
): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>(`/projects/${projectId}/tasks/${taskId}/time-logs`, payload);
  return data;
}

/**
 * GET /api/projects/{projectId}/tasks/{taskId}/time-logs
 * Retrieves all time logs for a task.
 */
export async function getTimeLogs(
  projectId: string,
  taskId: string
): Promise<ApiResponse<TimeLog[]>> {
  const { data } = await axiosInstance.get<ApiResponse<TimeLog[]>>(`/projects/${projectId}/tasks/${taskId}/time-logs`);
  return data;
}

/**
 * PUT /api/projects/{projectId}/tasks/{taskId}/time-logs/{logId}/approve
 * Approves a time log entry.
 */
export async function approveTimeLog(
  projectId: string,
  taskId: string,
  logId: string
): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.put<ApiResponse<boolean>>(
    `/projects/${projectId}/tasks/${taskId}/time-logs/${logId}/approve`
  );
  return data;
}

