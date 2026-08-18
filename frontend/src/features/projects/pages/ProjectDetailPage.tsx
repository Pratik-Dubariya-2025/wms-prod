import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  FolderGit,
  ArrowLeft,
  Calendar,
  User,
  Users,
  CheckSquare,
  Plus,
  Trash2,
  Edit3,
  Search,
  CheckCircle2,
  ClipboardList,
  RefreshCw,
  Clock,
  Trash,
  PlusCircle,
  Edit,
} from 'lucide-react';

import { useProjectDetail } from '../hooks/useProjectDetail';
import { useProjectMembers } from '../hooks/useProjectMembers';
import { useTasks } from '@/features/tasks/hooks/useTasks';
import { useCurrentUser } from '@/hooks/useCurrentUser';
import { usePermissions } from '@/hooks/usePermissions';
import { PERMISSIONS } from '@/constants/permissions';
import { Badge } from '@/components/ui/Badge/Badge';
import { Pagination } from '@/components/ui/Pagination/Pagination';
import { formatId } from '@/utils/formatId';
import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { useModal } from '@/components/ui/Modal/useModal';
import { Modal } from '@/components/ui/Modal/Modal';
import { Spinner } from '@/components/ui/Spinner/Spinner';
import { PermissionGate } from '@/components/shared/PermissionGate';
import { ProjectCreateTaskModal } from '../components/ProjectCreateTaskModal';
import { ProjectEditTaskModal } from '../components/ProjectEditTaskModal';
import {
  updateTaskStatus,
  getTaskById,
  deleteTask,
  getTimeLogs,
  createTimeLog,
  approveTimeLog
} from '@/api/tasksApi';
import { getUsers } from '@/api/usersApi';
import type { UserListItem } from '@/features/users/types/user.types';
import type { TaskDetail, TimeLog } from '@/features/tasks/types/task.types';
import { toast } from '@/components/ui/Toast/Toast';

const STATUS_COLOR_MAP: Record<string, 'indigo' | 'purple' | 'cyan' | 'emerald' | 'warning' | 'danger'> = {
  Planning: 'purple',
  Active: 'emerald',
  OnHold: 'warning',
  Completed: 'indigo',
  Cancelled: 'danger',
};

const TASK_STATUS_COLORS: Record<string, string> = {
  Draft: 'bg-slate-500/10 text-wms-secondary border border-slate-500/20',
  InProgress: 'bg-blue-500/10 text-blue-400 border border-blue-500/20',
  InReview: 'bg-amber-500/10 text-amber-400 border border-amber-500/20',
  Done: 'bg-emerald-500/10 text-emerald-400 border border-emerald-500/20',
  Closed: 'bg-rose-500/10 text-rose-400 border border-rose-500/20',
};

const PRIORITY_COLORS: Record<string, string> = {
  Low: 'bg-slate-500/10 text-wms-secondary',
  Medium: 'bg-blue-500/10 text-blue-400',
  High: 'bg-orange-500/10 text-orange-400',
  Critical: 'bg-red-500/10 text-red-400 border border-red-500/30',
};

export default function ProjectDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const currentUser = useCurrentUser();
  const { hasPermission } = usePermissions();

  const { project, isLoading: isLoadingProject, error: projectError, refresh: refreshProject } = useProjectDetail(id || '');
  const { members, isLoading: isLoadingMembers, addMember, removeMember } = useProjectMembers(id || '');
  const {
    tasks,
    pagination: taskPagination,
    isLoading: isLoadingTasks,
    params: taskParams,
    setSearch: setTaskSearch,
    setStatus: setTaskStatus,
    setPriority: setTaskPriority,
    setAssigneeId: setTaskAssigneeId,
    setPage: setTaskPage,
    refresh: refreshTasks,
  } = useTasks(id || '', 10);

  const [activeTab, setActiveTab] = useState<'tasks' | 'members'>('tasks');

  // Modals
  const createTaskModal = useModal();
  const editTaskModal = useModal();
  const detailModal = useModal();
  const confirmDeleteModal = useModal();
  const confirmDeleteTaskModal = useModal();
  const [selectedTask, setSelectedTask] = useState<TaskDetail | null>(null);
  const [userIdToRemove, setUserIdToRemove] = useState<string | null>(null);
  const [isRemovingMember, setIsRemovingMember] = useState(false);

  // Detail / Time Log state variables
  const [isLoadingDetail, setIsLoadingDetail] = useState(false);
  const [isUpdatingStatus, setIsUpdatingStatus] = useState(false);
  const [isDeletingTask, setIsDeletingTask] = useState(false);
  const [timeLogs, setTimeLogs] = useState<TimeLog[]>([]);
  const [isLoadingTimeLogs, setIsLoadingTimeLogs] = useState(false);
  const [isSubmittingTimeLog, setIsSubmittingTimeLog] = useState(false);
  const [loggedHours, setLoggedHours] = useState('');
  const [logDate, setLogDate] = useState(new Date().toISOString().split('T')[0]);
  const [timeLogNotes, setTimeLogNotes] = useState('');

  // Assign member helpers
  const [allUsers, setAllUsers] = useState<UserListItem[]>([]);
  const [selectedUserIdToAdd, setSelectedUserIdToAdd] = useState('');
  const [isAddingMember, setIsAddingMember] = useState(false);

  useEffect(() => {
    async function loadAllUsers() {
      if (!project) return;
      try {
        const res = await getUsers({
          pageNumber: 1,
          pageSize: 200,
          isActive: true,
          teamId: project.teamId || undefined,
          departmentId: project.teamId ? undefined : project.departmentId,
        });
        if (res.succeeded && res.data) {
          setAllUsers(res.data.items);
        }
      } catch {
        // Silent error
      }
    }
    loadAllUsers();
  }, [project?.id, project?.teamId, project?.departmentId]);

  if (isLoadingProject) {
    return (
      <div className="flex justify-center items-center py-32">
        <RefreshCw className="h-8 w-8 text-wms-indigo animate-spin" />
      </div>
    );
  }

  if (projectError || !project) {
    return (
      <div className="p-4 bg-red-500/10 border border-red-500/20 text-red-400 rounded-xl text-center">
        {projectError || 'Project not found.'}
      </div>
    );
  }

  // Authorizations
  const isOwner = currentUser?.userId === project.ownerId;
  const isTeamLead = currentUser?.userId === project.teamLeadId;
  const isManagerOrAdmin = hasPermission(PERMISSIONS.PROJECT_CREATE);

  // Rights to manage project members
  const canManageMembers = isManagerOrAdmin || isOwner || isTeamLead;
  // Rights to create/edit tasks in project
  const canManageTasks = isOwner || isTeamLead || hasPermission(PERMISSIONS.TASK_CREATE);

  // Filter users that are NOT currently project members
  const nonMembers = allUsers.filter(u => !members.some(m => m.userId === u.id));

  const handleAddMemberSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedUserIdToAdd) return;
    setIsAddingMember(true);
    const result = await addMember(selectedUserIdToAdd);
    setIsAddingMember(false);
    if (result.success) {
      toast.success(result.message || 'Member added successfully.');
      setSelectedUserIdToAdd('');
      refreshProject();
    } else {
      toast.error(result.message || 'Failed to add member.');
    }
  };

  const handleRemoveMember = (userId: string) => {
    setUserIdToRemove(userId);
    confirmDeleteModal.open();
  };

  const handleConfirmRemoveMember = async () => {
    if (!userIdToRemove) return;
    setIsRemovingMember(true);
    const result = await removeMember(userIdToRemove);
    setIsRemovingMember(false);
    confirmDeleteModal.close();
    setUserIdToRemove(null);
    if (result.success) {
      toast.success(result.message || 'Member removed successfully.');
      refreshProject();
    } else {
      toast.error(result.message || 'Failed to remove member.');
    }
  };

  const fetchTimeLogs = async (projectId: string, taskId: string) => {
    setIsLoadingTimeLogs(true);
    try {
      const res = await getTimeLogs(projectId, taskId);
      if (res.succeeded && res.data) {
        setTimeLogs(res.data);
      }
    } catch (err) {
      console.error('Failed to load time logs', err);
    } finally {
      setIsLoadingTimeLogs(false);
    }
  };

  const handleStatusChange = async (taskId: string, newStatus: string) => {
    setIsUpdatingStatus(true);
    try {
      const res = await updateTaskStatus(id || '', taskId, { status: newStatus });
      if (res.succeeded) {
        toast.success(res.message || 'Status updated successfully.');
        if (selectedTask && selectedTask.id === taskId) {
          setSelectedTask((prev) => (prev ? { ...prev, status: newStatus as any } : null));
        }
        refreshTasks();
        refreshProject();
      } else {
        toast.error(res.message || 'Failed to update status.');
      }
    } catch (err: any) {
      toast.error(err?.response?.data?.message || err.message || 'Error updating status.');
    } finally {
      setIsUpdatingStatus(false);
    }
  };

  const handleDeleteTask = async () => {
    if (!selectedTask) return;
    setIsDeletingTask(true);
    try {
      const res = await deleteTask(selectedTask.projectId, selectedTask.id);
      if (res.succeeded) {
        toast.success(res.message || 'Task deleted successfully.');
        confirmDeleteTaskModal.close();
        detailModal.close();
        refreshTasks();
        refreshProject();
      } else {
        toast.error(res.message || 'Failed to delete task.');
      }
    } catch (err: any) {
      toast.error(err?.response?.data?.message || err.message || 'Failed to delete task.');
    } finally {
      setIsDeletingTask(false);
    }
  };

  const handleLogTimeSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedTask) return;

    const hours = parseFloat(loggedHours);
    if (isNaN(hours) || hours <= 0 || hours > 24) {
      toast.error('Logged hours must be between 0.01 and 24.');
      return;
    }

    if (new Date(logDate) < new Date(new Date(selectedTask.createdAt).setHours(0, 0, 0, 0))) {
      toast.error(`Cannot log time before the task was created (${new Date(selectedTask.createdAt).toLocaleDateString()}).`);
      return;
    }

    setIsSubmittingTimeLog(true);
    try {
      const res = await createTimeLog(selectedTask.projectId, selectedTask.id, {
        loggedHours: hours,
        logDate,
        notes: timeLogNotes.trim() || undefined,
      });
      if (res.succeeded) {
        toast.success(res.message || 'Time logged successfully.');
        setLoggedHours('');
        setTimeLogNotes('');
        fetchTimeLogs(selectedTask.projectId, selectedTask.id);
      } else {
        toast.error(res.message || 'Failed to log time.');
      }
    } catch (err: any) {
      toast.error(err?.response?.data?.message || err.message || 'Failed to log time.');
    } finally {
      setIsSubmittingTimeLog(false);
    }
  };

  const handleApproveTimeLog = async (logId: string) => {
    if (!selectedTask) return;
    try {
      const res = await approveTimeLog(selectedTask.projectId, selectedTask.id, logId);
      if (res.succeeded) {
        toast.success(res.message || 'Time log approved successfully.');
        fetchTimeLogs(selectedTask.projectId, selectedTask.id);
      } else {
        toast.error(res.message || 'Failed to approve time log.');
      }
    } catch (err: any) {
      toast.error(err?.response?.data?.message || err.message || 'Failed to approve time log.');
    }
  };

  const handleRowClick = async (taskId: string) => {
    setIsLoadingDetail(true);
    setSelectedTask(null);
    setTimeLogs([]);
    detailModal.open();
    try {
      const response = await getTaskById(taskId);
      if (response.succeeded && response.data) {
        setSelectedTask(response.data);
        fetchTimeLogs(response.data.projectId, response.data.id);
      }
    } catch {
      // Error handled by the detail view
    } finally {
      setIsLoadingDetail(false);
    }
  };

  const openEditTask = (taskItem: any) => {
    // Map to TaskDetail shape
    const detail: TaskDetail = {
      id: taskItem.id,
      title: taskItem.title,
      description: taskItem.description || '',
      status: taskItem.status,
      priority: taskItem.priority,
      estimatedHours: taskItem.estimatedHours,
      dueDate: taskItem.dueDate,
      projectId: id || '',
      projectName: project.name,
      teamId: null,
      teamName: null,
      assigneeId: taskItem.assigneeId || null,
      assigneeName: taskItem.assigneeName || null,
      assigneeEmail: null,
      createdById: '',
      createdByName: taskItem.createdByName,
      createdAt: taskItem.createdAt,
      createdBy: null,
      modifiedAt: null,
      modifiedBy: null,
    };
    setSelectedTask(detail);
    editTaskModal.open();
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Back to list */}
      <div>
        <button
          onClick={() => navigate('/projects')}
          className="inline-flex items-center gap-2 text-xs font-semibold text-wms-text/50 hover:text-wms-text transition duration-200"
        >
          <ArrowLeft className="h-4 w-4" />
          BACK TO PROJECTS
        </button>
      </div>

      {/* Hero Header Card */}
      <div className="relative bg-wms-hover border border-wms-border rounded-2xl p-6 shadow-xl overflow-hidden">
        <div className="absolute -right-16 -top-16 w-36 h-36 rounded-full bg-wms-indigo/10 blur-3xl pointer-events-none" />

        <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
          <div className="space-y-3 flex-1">
            <div className="flex items-center gap-3">
              <h1 className="text-3xl font-extrabold text-wms-text tracking-tight">{project.name}</h1>
              <Badge variant={STATUS_COLOR_MAP[project.status] || 'indigo'}>
                {project.status}
              </Badge>
            </div>
            <p className="text-wms-text/60 text-sm max-w-3xl">{project.description || 'No description provided.'}</p>

            <div className="flex flex-wrap gap-4 text-xs text-wms-text/40 pt-1">
              <span className="flex items-center gap-1.5">
                <FolderGit className="h-4 w-4 text-wms-cyan" />
                Dept: <span className="text-wms-text/70 font-semibold">{project.departmentName}</span>
              </span>
              <span className="flex items-center gap-1.5">
                <User className="h-4 w-4 text-wms-indigo" />
                Lead: <span className="text-wms-text/70 font-semibold">{project.teamLeadName || 'Unassigned'}</span>
              </span>
              <span className="flex items-center gap-1.5">
                <User className="h-4 w-4 text-emerald-400" />
                Owner: <span className="text-wms-text/70 font-semibold">{project.ownerName}</span>
              </span>
              {project.startDate && (
                <span className="flex items-center gap-1.5">
                  <Calendar className="h-4 w-4 text-amber-400" />
                  Start: <span className="text-wms-text/70 font-semibold">{new Date(project.startDate).toLocaleDateString()}</span>
                </span>
              )}
              {project.endDate && (
                <span className="flex items-center gap-1.5">
                  <Calendar className="h-4 w-4 text-red-400" />
                  End: <span className="text-wms-text/70 font-semibold">{new Date(project.endDate).toLocaleDateString()}</span>
                </span>
              )}
            </div>
          </div>

          {/* Mini Task Stats summary */}
          <div className="grid grid-cols-1 sm:grid-cols-3 gap-3 bg-wms-hover border border-wms-border rounded-xl p-4 text-center">
            <div>
              <p className="text-2xl font-black text-wms-text">{project.taskSummary.total}</p>
              <p className="text-[10px] text-wms-text/40 font-semibold uppercase tracking-wider">Total Tasks</p>
            </div>
            <div>
              <p className="text-2xl font-black text-blue-400">{project.taskSummary.inProgress}</p>
              <p className="text-[10px] text-wms-text/40 font-semibold uppercase tracking-wider">In Progress</p>
            </div>
            <div>
              <p className="text-2xl font-black text-emerald-400">{project.taskSummary.done}</p>
              <p className="text-[10px] text-wms-text/40 font-semibold uppercase tracking-wider">Done</p>
            </div>
          </div>
        </div>
      </div>

      {/* Tabs list */}
      <div className="flex border-b border-wms-border">
        <button
          onClick={() => setActiveTab('tasks')}
          className={`flex items-center gap-2 px-6 py-3 font-semibold text-sm transition-all duration-200 border-b-2 ${activeTab === 'tasks'
              ? 'border-wms-indigo text-wms-indigo bg-wms-indigo/5'
              : 'border-transparent text-wms-text/50 hover:text-wms-text'
            }`}
        >
          <CheckSquare className="h-4 w-4" />
          Tasks ({project.taskSummary.total})
        </button>
        <button
          onClick={() => setActiveTab('members')}
          className={`flex items-center gap-2 px-6 py-3 font-semibold text-sm transition-all duration-200 border-b-2 ${activeTab === 'members'
              ? 'border-wms-indigo text-wms-indigo bg-wms-indigo/5'
              : 'border-transparent text-wms-text/50 hover:text-wms-text'
            }`}
        >
          <Users className="h-4 w-4" />
          Team Members ({members.length})
        </button>
      </div>

      {/* Tab Panels */}
      {activeTab === 'tasks' && (
        <div className="space-y-6">
          {/* Tasks Filters & New Task button */}
          <div className="flex flex-col md:flex-row gap-4 items-center justify-between">
            {/* Left filters */}
            <div className="grid grid-cols-1 sm:grid-cols-4 gap-3 w-full md:w-auto">
              {/* Search */}
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-3.5 w-3.5 text-wms-text/30" />
                <input
                  type="text"
                  placeholder="Search tasks..."
                  defaultValue={taskParams.search || ''}
                  onChange={(e) => setTaskSearch(e.target.value)}
                  className="w-full sm:w-48 pl-9 pr-3 py-2 bg-wms-hover border border-wms-border rounded-lg text-xs text-wms-text placeholder-white/20 focus:outline-none focus:border-wms-indigo"
                />
              </div>

              {/* Status */}
              <select
                value={taskParams.status || ''}
                onChange={(e) => setTaskStatus(e.target.value)}
                className="py-2 px-3 bg-wms-hover border border-wms-border rounded-lg text-xs text-wms-text/80"
              >
                <option value="" className="bg-wms-surface">All Statuses</option>
                <option value="Draft" className="bg-wms-surface">Draft</option>
                <option value="InProgress" className="bg-wms-surface">In Progress</option>
                <option value="InReview" className="bg-wms-surface">In Review</option>
                <option value="Done" className="bg-wms-surface">Done</option>
                <option value="Closed" className="bg-wms-surface">Closed</option>
              </select>

              {/* Priority */}
              <select
                value={taskParams.priority || ''}
                onChange={(e) => setTaskPriority(e.target.value)}
                className="py-2 px-3 bg-wms-hover border border-wms-border rounded-lg text-xs text-wms-text/80"
              >
                <option value="" className="bg-wms-surface">All Priorities</option>
                <option value="Low" className="bg-wms-surface">Low</option>
                <option value="Medium" className="bg-wms-surface">Medium</option>
                <option value="High" className="bg-wms-surface">High</option>
                <option value="Critical" className="bg-wms-surface">Critical</option>
              </select>

              {/* Assignee */}
              <select
                value={taskParams.assigneeId || ''}
                onChange={(e) => setTaskAssigneeId(e.target.value)}
                className="py-2 px-3 bg-wms-hover border border-wms-border rounded-lg text-xs text-wms-text/80"
              >
                <option value="" className="bg-wms-surface">All Assignees</option>
                {members.map(m => (
                  <option key={m.id} value={m.userId} className="bg-wms-surface">
                    {m.fullName}
                  </option>
                ))}
              </select>
            </div>

            {/* Right button */}
            {canManageTasks && (
              <button
                onClick={createTaskModal.open}
                className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-xs font-semibold shadow-lg shadow-wms-indigo/25 transition duration-200"
              >
                <Plus className="h-3.5 w-3.5" />
                Add Task
              </button>
            )}
          </div>

          {/* Tasks List */}
          {isLoadingTasks ? (
            <div className="flex justify-center items-center py-16">
              <RefreshCw className="h-6 w-6 text-wms-indigo animate-spin" />
            </div>
          ) : tasks.length === 0 ? (
            <div className="p-12 text-center border border-dashed border-wms-border rounded-xl">
              <ClipboardList className="h-10 w-10 text-wms-text/20 mx-auto mb-2" />
              <h3 className="text-wms-text font-medium text-sm">No Tasks Found</h3>
              <p className="text-wms-text/40 text-xs mt-1">Create a new task or modify filters.</p>
            </div>
          ) : (
            <div className="space-y-3">
              {tasks.map((task) => {
                const canEditTask = isOwner || isTeamLead || currentUser?.userId === task.assigneeId;
                const isAssignee = currentUser?.userId === task.assigneeId;

                return (
                  <div
                    key={task.id}
                    className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 bg-wms-hover hover:bg-wms-hover border border-wms-border hover:border-wms-border rounded-xl p-4 transition-all duration-200"
                  >
                    {/* Left: title, meta info */}
                    <div
                      onClick={() => handleRowClick(task.id)}
                      className="space-y-2 flex-1 cursor-pointer hover:opacity-80 transition duration-150"
                    >
                      <div className="flex items-start gap-2">
                        <h4 className="text-sm font-semibold text-wms-text tracking-tight leading-snug">
                          {task.title}
                        </h4>
                        <span className={`px-2 py-0.5 rounded text-[10px] font-bold ${PRIORITY_COLORS[task.priority]}`}>
                          {task.priority}
                        </span>
                      </div>
                      <div className="flex flex-wrap gap-x-4 gap-y-1.5 text-xs text-wms-text/40">
                        <span>
                          Assignee: <strong className="text-wms-text/70 font-semibold">{task.assigneeName || 'Unassigned'}</strong>
                        </span>
                        {task.estimatedHours && (
                          <span>Est: {task.estimatedHours} hrs</span>
                        )}
                        {task.dueDate && (
                          <span>Due: {new Date(task.dueDate).toLocaleDateString()}</span>
                        )}
                      </div>
                    </div>

                    {/* Right: status update or edit controls */}
                    <div className="flex items-center gap-3">
                      {/* Status badge / Selector */}
                      {(isAssignee || canEditTask) && task.status !== 'Closed' ? (
                        <select
                          value={task.status}
                          onChange={(e) => handleStatusChange(task.id, e.target.value)}
                          className="py-1 px-2.5 bg-wms-hover border border-wms-border rounded-lg text-xs text-wms-text focus:outline-none"
                        >
                          <option value="Draft" className="bg-wms-surface">Draft</option>
                          <option value="InProgress" className="bg-wms-surface">In Progress</option>
                          <option value="InReview" className="bg-wms-surface">In Review</option>
                          <option value="Done" className="bg-wms-surface">Done</option>
                          <option value="Closed" className="bg-wms-surface">Closed</option>
                        </select>
                      ) : (
                        <div className={`px-2.5 py-1 text-xs font-semibold rounded ${TASK_STATUS_COLORS[task.status]}`}>
                          {task.status}
                        </div>
                      )}

                      {/* Edit option */}
                      {canEditTask && !['InReview', 'Done', 'Closed'].includes(task.status) && (
                        <button
                          onClick={() => openEditTask(task)}
                          className="p-1.5 rounded bg-wms-hover border border-wms-border text-wms-text/60 hover:text-wms-text hover:bg-wms-hover transition"
                          title="Edit Task"
                        >
                          <Edit3 className="h-3.5 w-3.5" />
                        </button>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          )}

          {/* Tasks Pagination */}
          {taskPagination && (
            <div className="flex justify-end pt-3">
              <Pagination
                pageNumber={taskPagination.pageNumber}
                pageSize={taskPagination.pageSize}
                totalCount={taskPagination.totalCount}
                totalPages={taskPagination.totalPages}
                hasPreviousPage={taskPagination.hasPreviousPage}
                hasNextPage={taskPagination.hasNextPage}
                onPageChange={setTaskPage}
                onPageSizeChange={() => { }}
              />
            </div>
          )}
        </div>
      )}

      {activeTab === 'members' && (
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Members List */}
          <div className="lg:col-span-2 space-y-4">
            <h3 className="text-lg font-bold text-wms-text tracking-tight flex items-center gap-2">
              <Users className="h-5 w-5 text-wms-indigo" />
              Project Members
            </h3>

            {isLoadingMembers ? (
              <div className="flex justify-center items-center py-12">
                <RefreshCw className="h-6 w-6 text-wms-indigo animate-spin" />
              </div>
            ) : members.length === 0 ? (
              <div className="p-8 border border-dashed border-wms-border rounded-xl text-center text-wms-text/40 text-sm">
                No members found.
              </div>
            ) : (
              <div className="space-y-3">
                {members.map((member) => (
                  <div
                    key={member.id}
                    className="flex items-center justify-between p-4 bg-wms-hover border border-wms-border rounded-xl"
                  >
                    <div className="flex items-center gap-3">
                      <div className="h-9 w-9 rounded-full bg-wms-indigo/15 border border-wms-indigo/35 flex items-center justify-center font-bold text-wms-indigo text-sm uppercase">
                        {member.fullName.substring(0, 2)}
                      </div>
                      <div>
                        <h4 className="text-sm font-semibold text-wms-text leading-tight">{member.fullName}</h4>
                        <p className="text-xs text-wms-text/40 mt-0.5">{member.email} • {member.designationName}</p>
                      </div>
                    </div>

                    {/* Member actions */}
                    {canManageMembers && member.userId !== project.ownerId && member.userId !== project.teamLeadId && (
                      <button
                        onClick={() => handleRemoveMember(member.userId)}
                        className="p-2 text-wms-text/40 hover:text-red-400 bg-wms-hover hover:bg-red-500/10 border border-wms-border hover:border-red-500/20 rounded-lg transition duration-200"
                        title="Remove member"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    )}
                  </div>
                ))}
              </div>
            )}
          </div>

          {/* Add Member form */}
          {canManageMembers && (
            <div className="bg-wms-hover border border-wms-border rounded-xl p-5 space-y-4">
              <h3 className="text-base font-bold text-wms-text">Add Project Member</h3>
              <p className="text-xs text-wms-text/50">Select an employee from the workspace to grant them access to this project and its tasks.</p>

              <form onSubmit={handleAddMemberSubmit} className="space-y-4">
                <div>
                  <select
                    value={selectedUserIdToAdd}
                    onChange={(e) => setSelectedUserIdToAdd(e.target.value)}
                    className="w-full px-3 py-2 bg-wms-surface border border-wms-border rounded-lg text-sm text-wms-text/90 focus:outline-none focus:border-wms-indigo"
                  >
                    <option value="">-- Choose User --</option>
                    {nonMembers.map(u => (
                      <option key={u.id} value={u.id}>
                        {u.firstName} {u.lastName} ({u.designationName})
                      </option>
                    ))}
                  </select>
                </div>

                <button
                  type="submit"
                  disabled={!selectedUserIdToAdd || isAddingMember}
                  className="w-full py-2 bg-wms-indigo hover:bg-indigo-700 text-white font-semibold rounded-lg text-xs shadow transition duration-200 disabled:opacity-50"
                >
                  {isAddingMember ? 'Adding...' : 'Add to Project'}
                </button>
              </form>
            </div>
          )}
        </div>
      )}

      {/* Create Task Modal */}
      <ProjectCreateTaskModal
        isOpen={createTaskModal.isOpen}
        onClose={createTaskModal.close}
        onSuccess={() => {
          refreshTasks();
          refreshProject();
        }}
        projectId={id || ''}
      />

      {/* Edit Task Modal */}
      <ProjectEditTaskModal
        isOpen={editTaskModal.isOpen}
        onClose={editTaskModal.close}
        task={selectedTask}
        onSuccess={() => {
          refreshTasks();
          refreshProject();
        }}
        projectId={id || ''}
      />

      {/* ─── Task Detail Modal ─── */}
      <Modal
        isOpen={detailModal.isOpen}
        onClose={detailModal.close}
        title="Task Details"
        size="xl"
      >
        {isLoadingDetail ? (
          <div className="flex flex-col items-center justify-center py-16 gap-3">
            <Spinner size="lg" />
            <p className="text-sm text-wms-secondary">Loading task details...</p>
          </div>
        ) : selectedTask ? (
          <div className="space-y-6">
            {/* Title + Status Header */}
            <div className="flex flex-col gap-3 pb-4 border-b border-wms-border">
              <h2 className="text-lg font-bold text-wms-text leading-snug">{selectedTask.title}</h2>
              <div className="flex flex-wrap items-center justify-between gap-3">
                <div className="flex flex-wrap items-center gap-2">
                  {(() => {
                    const cfg = STATUS_CONFIG[selectedTask.status] || STATUS_CONFIG.Draft;
                    return (
                      <span className={classNames(
                        'inline-flex items-center gap-1.5 text-xs font-medium px-2.5 py-1 rounded-full border',
                        cfg.color, cfg.bg, cfg.border
                      )}>
                        <span className={classNames('h-1.5 w-1.5 rounded-full', cfg.dot)} />
                        {selectedTask.status === 'InProgress' ? 'In Progress' : selectedTask.status === 'InReview' ? 'In Review' : selectedTask.status}
                      </span>
                    );
                  })()}
                  <Badge variant={PRIORITY_BADGE[selectedTask.priority] || 'default'}>
                    {selectedTask.priority} Priority
                  </Badge>
                </div>

                {/* Status Quick Update */}
                <PermissionGate permissions={PERMISSIONS.TASK_UPDATE}>
                  {selectedTask.status !== 'Closed' && (
                    <div className="flex items-center gap-2">
                      <label className="text-[10px] font-bold text-wms-muted uppercase">Change Status:</label>
                      <select
                        value={selectedTask.status}
                        disabled={isUpdatingStatus}
                        onChange={(e) => handleStatusChange(selectedTask.id, e.target.value)}
                        className="rounded-md bg-wms-hover border border-wms-border px-2.5 py-1 text-xs text-wms-text outline-none focus:border-wms-cyan cursor-pointer disabled:opacity-50"
                      >
                        <option value="Draft" className="bg-wms-bg text-wms-text">Draft</option>
                        <option value="InProgress" className="bg-wms-bg text-wms-text">In Progress</option>
                        <option value="InReview" className="bg-wms-bg text-wms-text">In Review</option>
                        <option value="Done" className="bg-wms-bg text-wms-text">Done</option>
                        <option value="Closed" className="bg-wms-bg text-wms-text">Closed</option>
                      </select>
                    </div>
                  )}
                </PermissionGate>
              </div>
            </div>

            {/* Description */}
            {selectedTask.description && (
              <div className="space-y-1.5">
                <label className="text-xs font-semibold text-wms-muted uppercase tracking-wider">Description</label>
                <p className="text-sm text-wms-secondary leading-relaxed whitespace-pre-wrap">
                  {selectedTask.description}
                </p>
              </div>
            )}

            {/* Detail Grid */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <DetailField label="Project" value={selectedTask.projectName} />
              <DetailField label="Team" value={selectedTask.teamName || '—'} />
              <DetailField label="Assignee" value={selectedTask.assigneeName || 'Unassigned'} />
              <DetailField label="Created By" value={selectedTask.createdByName} />
              <DetailField
                label="Estimated Hours"
                value={selectedTask.estimatedHours != null ? `${selectedTask.estimatedHours}h` : '—'}
                icon={<Clock className="h-3.5 w-3.5 text-wms-muted" />}
              />
              <DetailField
                label="Due Date"
                value={selectedTask.dueDate ? new Date(selectedTask.dueDate).toLocaleDateString('en-US', {
                  month: 'short',
                  day: 'numeric',
                  year: 'numeric',
                }) : '—'}
                icon={<Calendar className="h-3.5 w-3.5 text-wms-muted" />}
              />
            </div>

            {/* ─── Time Logs Section ─── */}
            <div className="pt-5 border-t border-wms-border space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-sm font-bold text-wms-text tracking-wide uppercase">Time Logs</h3>
                <span className="text-xs text-wms-muted">
                  Total Logged: <span className="text-wms-cyan font-bold">{timeLogs.reduce((acc, log) => acc + log.loggedHours, 0).toFixed(2)}h</span>
                </span>
              </div>

              {/* Log Time Form */}
              <PermissionGate permissions={PERMISSIONS.TIMELOG_CREATE}>
                {(selectedTask.status === 'InProgress' || selectedTask.status === 'InReview' || selectedTask.status === 'Done') ? (
                  <form onSubmit={handleLogTimeSubmit} className="glass-card rounded-lg p-3 border-wms-border/50 bg-wms-hover/30 space-y-3">
                    <div className="text-xs font-semibold text-wms-secondary flex items-center gap-1.5">
                      <PlusCircle className="h-3.5 w-3.5 text-wms-cyan" />
                      Log Time
                    </div>
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
                      <div>
                        <input
                          type="number"
                          step="0.01"
                          min="0.01"
                          max="24"
                          required
                          placeholder="Hours (e.g. 2.5)"
                          value={loggedHours}
                          onChange={(e) => setLoggedHours(e.target.value)}
                          className="w-full rounded-md bg-wms-hover border border-wms-border text-xs text-wms-text px-3 py-2 outline-none focus:border-wms-cyan"
                        />
                      </div>
                      <div>
                        <input
                          type="date"
                          required
                          max={new Date().toISOString().split('T')[0]}
                          value={logDate}
                          onChange={(e) => setLogDate(e.target.value)}
                          className="w-full rounded-md bg-wms-hover border border-wms-border text-xs text-wms-text px-3 py-2 outline-none focus:border-wms-cyan"
                        />
                      </div>
                      <div>
                        <button
                          type="submit"
                          disabled={isSubmittingTimeLog}
                          className="w-full inline-flex items-center justify-center gap-1.5 px-4 py-2 rounded-md bg-wms-cyan hover:bg-cyan-600 text-white text-xs font-semibold transition cursor-pointer disabled:opacity-50"
                        >
                          {isSubmittingTimeLog ? 'Saving...' : 'Add Log'}
                        </button>
                      </div>
                    </div>
                    <div>
                      <input
                        type="text"
                        placeholder="Notes (optional)..."
                        value={timeLogNotes}
                        onChange={(e) => setTimeLogNotes(e.target.value)}
                        className="w-full rounded-md bg-wms-hover border border-wms-border text-xs text-wms-text px-3 py-2 outline-none focus:border-wms-cyan"
                      />
                    </div>
                  </form>
                ) : (
                  <p className="text-xs text-wms-muted italic bg-wms-hover/10 p-2.5 rounded border border-dashed border-wms-border">
                    Time logs can only be added when task status is In Progress, In Review, or Done.
                  </p>
                )}
              </PermissionGate>

              {/* Time Logs List */}
              <PermissionGate permissions={PERMISSIONS.TIMELOG_READ}>
                <div className="space-y-2">
                  {isLoadingTimeLogs ? (
                    <div className="flex justify-center py-4">
                      <Spinner size="sm" />
                    </div>
                  ) : timeLogs.length === 0 ? (
                    <p className="text-xs text-wms-muted italic text-center py-3">No hours logged on this task yet.</p>
                  ) : (
                    <div className="max-h-48 overflow-y-auto space-y-2 pr-1 border border-wms-border/30 rounded-lg p-2 bg-wms-hover/10">
                      {timeLogs.map((log) => (
                        <div key={log.id} className="flex flex-col sm:flex-row sm:items-center justify-between gap-2 p-2 rounded bg-wms-hover/40 border border-wms-border/40 text-xs">
                          <div className="space-y-1">
                            <div className="flex items-center gap-2">
                              <span className="font-semibold text-wms-text">{log.userName}</span>
                              <span className="text-[10px] text-wms-muted">{
                                new Date(log.logDate).toLocaleDateString('en-US', {
                                  month: 'short',
                                  day: 'numeric',
                                  year: 'numeric',
                                })
                              }</span>
                              <Badge variant="indigo" className="text-[10px] py-0 px-1.5">{log.loggedHours}h</Badge>
                            </div>
                            {log.notes && <p className="text-wms-secondary italic text-[11px] leading-snug">{log.notes}</p>}
                          </div>

                          <div className="flex items-center gap-2 self-end sm:self-center">
                            {log.isApproved ? (
                              <span className="inline-flex items-center gap-1 text-[10px] font-medium text-wms-emerald bg-wms-emerald/10 border border-wms-emerald/20 px-2 py-0.5 rounded-full" title={log.approvedByName ? `Approved by ${log.approvedByName}` : undefined}>
                                <CheckCircle2 className="h-3 w-3" />
                                Approved
                              </span>
                            ) : (
                              <>
                                <span className="text-[10px] text-wms-muted bg-wms-hover border border-wms-border px-2 py-0.5 rounded-full">
                                  Pending
                                </span>
                                <PermissionGate permissions={PERMISSIONS.TIMELOG_APPROVE}>
                                  {log.userId !== currentUser?.userId && (
                                    <button
                                      type="button"
                                      onClick={() => handleApproveTimeLog(log.id)}
                                      className="inline-flex items-center gap-1 text-[10px] font-semibold text-wms-emerald hover:text-green-400 bg-wms-emerald/10 hover:bg-wms-emerald/20 border border-wms-emerald/25 px-2 py-0.5 rounded transition cursor-pointer"
                                    >
                                      Approve
                                    </button>
                                  )}
                                </PermissionGate>
                              </>
                            )}
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </PermissionGate>
            </div>

            {/* Audit */}
            <div className="pt-4 border-t border-wms-border flex flex-wrap items-center gap-x-6 gap-y-2 text-xs text-wms-muted pb-4">
              <span>Created {selectedTask.createdAt ? new Date(selectedTask.createdAt).toLocaleDateString() : '—'}{selectedTask.createdBy ? ` by ${selectedTask.createdBy}` : ''}</span>
              {selectedTask.modifiedAt && (
                <span>Modified {new Date(selectedTask.modifiedAt).toLocaleDateString()}{selectedTask.modifiedBy ? ` by ${selectedTask.modifiedBy}` : ''}</span>
              )}
              <span className="text-[10px] bg-wms-hover px-2 py-0.5 rounded font-mono text-wms-muted">{formatId(selectedTask.id)}</span>
            </div>

            {/* Modal Actions */}
            <div className="flex justify-between items-center gap-3 pt-4 border-t border-wms-border">
              <div>
                <PermissionGate permissions={PERMISSIONS.TASK_DELETE}>
                  {!['InReview', 'Done', 'Closed'].includes(selectedTask.status) && (
                    <button
                      type="button"
                      disabled={isDeletingTask}
                      onClick={confirmDeleteTaskModal.open}
                      className="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-wms-danger/10 hover:bg-wms-danger/25 border border-wms-danger/20 text-wms-danger text-sm font-semibold transition cursor-pointer disabled:opacity-50"
                    >
                      <Trash className="h-4 w-4" />
                      {isDeletingTask ? 'Deleting...' : 'Delete Task'}
                    </button>
                  )}
                </PermissionGate>
              </div>
              <div className="flex gap-3">
                <PermissionGate permissions={PERMISSIONS.TASK_UPDATE}>
                  {!['InReview', 'Done', 'Closed'].includes(selectedTask.status) && (
                    <button
                      type="button"
                      onClick={() => {
                        detailModal.close();
                        editTaskModal.open();
                      }}
                      className="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition cursor-pointer"
                    >
                      <Edit className="h-4 w-4" />
                      Edit Task
                    </button>
                  )}
                </PermissionGate>
                <button
                  type="button"
                  onClick={detailModal.close}
                  className="px-4 py-2 rounded-lg bg-wms-hover border border-wms-border text-sm font-semibold text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition cursor-pointer"
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        ) : (
          <div className="text-center py-8">
            <p className="text-sm text-wms-danger font-medium">Failed to load task details</p>
            <p className="text-xs text-wms-muted mt-1">Please close and try again.</p>
          </div>
        )}
      </Modal>

      {/* Confirm Delete Member Modal */}
      <ConfirmDialog
        isOpen={confirmDeleteModal.isOpen}
        onClose={() => {
          confirmDeleteModal.close();
          setUserIdToRemove(null);
        }}
        onConfirm={handleConfirmRemoveMember}
        title="Remove Member"
        message="Are you sure you want to remove this member from the project?"
        confirmLabel="Remove"
        cancelLabel="Cancel"
        isLoading={isRemovingMember}
        variant="danger"
      />

      {/* Confirm Delete Task Modal */}
      <ConfirmDialog
        isOpen={confirmDeleteTaskModal.isOpen}
        onClose={confirmDeleteTaskModal.close}
        onConfirm={handleDeleteTask}
        title="Delete Task"
        message="Are you sure you want to delete this task? This will also soft-delete all logged time."
        confirmLabel="Delete"
        cancelLabel="Cancel"
        isLoading={isDeletingTask}
        variant="danger"
      />
    </div>
  );
}

// Helper components & constants for task details modal
import { classNames } from '@/utils/classNames';

const STATUS_CONFIG: Record<string, { color: string; bg: string; border: string; dot: string }> = {
  Draft: { color: 'text-wms-muted', bg: 'bg-wms-hover', border: 'border-wms-border', dot: 'bg-wms-muted' },
  InProgress: { color: 'text-wms-cyan border-wms-cyan/30', bg: 'bg-wms-cyan/10', border: 'border-wms-cyan/20', dot: 'bg-wms-cyan' },
  InReview: { color: 'text-wms-indigo border-wms-indigo/30', bg: 'bg-wms-indigo/10', border: 'border-wms-indigo/20', dot: 'bg-wms-indigo' },
  Done: { color: 'text-wms-emerald border-wms-emerald/30', bg: 'bg-wms-emerald/10', border: 'border-wms-emerald/20', dot: 'bg-wms-emerald' },
  Closed: { color: 'text-wms-muted border-wms-border', bg: 'bg-wms-hover', border: 'border-wms-border', dot: 'bg-wms-muted' },
};

const PRIORITY_BADGE: Record<string, 'default' | 'indigo' | 'warning' | 'danger'> = {
  Low: 'default',
  Medium: 'indigo',
  High: 'warning',
  Critical: 'danger',
};

function DetailField({ label, value, icon }: { label: string; value: string; icon?: React.ReactNode }) {
  return (
    <div className="space-y-1">
      <label className="text-xs font-semibold text-wms-muted uppercase tracking-wider">{label}</label>
      <p className="text-sm text-wms-text font-medium flex items-center gap-1.5">
        {icon}
        {value}
      </p>
    </div>
  );
}
