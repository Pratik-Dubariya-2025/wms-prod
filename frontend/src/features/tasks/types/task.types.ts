/** Task status enum matching backend TaskItem.Status */
export const TaskStatus = {
  Draft: 'Draft',
  InProgress: 'InProgress',
  InReview: 'InReview',
  Done: 'Done',
  Closed: 'Closed',
} as const;

export type TaskStatus = (typeof TaskStatus)[keyof typeof TaskStatus];

/** Task priority enum matching backend TaskItem.Priority */
export const TaskPriority = {
  Low: 'Low',
  Medium: 'Medium',
  High: 'High',
  Critical: 'Critical',
} as const;

export type TaskPriority = (typeof TaskPriority)[keyof typeof TaskPriority];

/** Task list item — mirrors backend TaskListDto */
export interface TaskListItem {
  id: string;
  title: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate: string | null;
  estimatedHours: number | null;
  projectName: string;
  teamName: string | null;
  assigneeName: string | null;
  assigneeId?: string | null;
  createdByName: string;
  createdAt: string;
}

/** Task detail — mirrors backend TaskDetailDto */
export interface TaskDetail {
  id: string;
  title: string;
  description: string | null;
  status: TaskStatus;
  priority: TaskPriority;
  estimatedHours: number | null;
  dueDate: string | null;
  projectId: string;
  projectName: string;
  teamId: string | null;
  teamName: string | null;
  assigneeId: string | null;
  assigneeName: string | null;
  assigneeEmail: string | null;
  createdById: string;
  createdByName: string;
  createdAt: string;
  createdBy: string | null;
  modifiedAt: string | null;
  modifiedBy: string | null;
}

/** Query params for GET /api/tasks */
export interface TasksQueryParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  status?: string;
  priority?: string;
  projectId?: string;
  assigneeId?: string;
}

/** Payload for POST /api/tasks */
export interface CreateTaskPayload {
  title: string;
  description?: string;
  priority: string;
  estimatedHours?: number | null;
  dueDate?: string | null;
  projectId: string;
  assigneeId?: string | null;
}

/** Lookup DTOs for task creation form */
export interface TaskProjectLookup {
  id: string;
  name: string;
  departmentId: string;
}

export interface TaskTeamLookup {
  id: string;
  name: string;
  departmentId: string;
}

export interface TaskUserLookup {
  id: string;
  fullName: string;
}

export interface TaskCreateMeta {
  projects: TaskProjectLookup[];
  teams: TaskTeamLookup[];
  users: TaskUserLookup[];
}

/** Payload for PUT /api/tasks/{id} */
export interface UpdateTaskPayload {
  title: string;
  description?: string;
  status: string;
  priority: string;
  estimatedHours?: number | null;
  dueDate?: string | null;
  projectId: string;
  assigneeId?: string | null;
}

/** Payload for PATCH /api/tasks/{id}/status */
export interface UpdateTaskStatusPayload {
  status: string;
}

/** Time log entry details matching backend TimeLogDto */
export interface TimeLog {
  id: string;
  taskId: string;
  taskTitle: string;
  userId: string;
  userName: string;
  loggedHours: number;
  logDate: string;
  notes: string | null;
  isApproved: boolean;
  approvedByName: string | null;
  approvedAt: string | null;
  createdAt: string;
}

/** Payload for logging time on a task */
export interface CreateTimeLogPayload {
  loggedHours: number;
  logDate: string;
  notes?: string;
}

