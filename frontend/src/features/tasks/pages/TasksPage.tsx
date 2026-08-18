import { useState, useCallback } from 'react';
import {
  Search,
  ClipboardList,
  RefreshCw,
  Plus,
  Filter,
  X,
  Calendar,
  Clock,
  AlertCircle,
  ChevronRight,
  Edit,
  Trash,
  PlusCircle,
  CheckCircle2,
} from 'lucide-react';

import { useTasks } from '@/features/tasks/hooks/useTasks';
import {
  getTaskById,
  updateTaskStatus,
  deleteTask,
  getTimeLogs,
  createTimeLog,
  approveTimeLog,
} from '@/api/tasksApi';
import { PermissionGate } from '@/components/shared/PermissionGate';
import { PERMISSIONS } from '@/constants/permissions';
import { classNames } from '@/utils/classNames';
import { formatId } from '@/utils/formatId';
import { useModal } from '@/components/ui/Modal/useModal';
import { CreateTaskModal } from '@/features/tasks/components/CreateTaskModal';
import { EditTaskModal } from '@/features/tasks/components/EditTaskModal';
import { Pagination } from '@/components/ui/Pagination/Pagination';
import { Badge } from '@/components/ui/Badge/Badge';
import { Modal } from '@/components/ui/Modal/Modal';
import { Spinner } from '@/components/ui/Spinner/Spinner';
import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { toast } from '@/components/ui/Toast/Toast';
import { useCurrentUser } from '@/hooks/useCurrentUser';
import type { TaskDetail, TimeLog } from '@/features/tasks/types/task.types';

const STATUS_CONFIG: Record<string, { color: string; bg: string; border: string; dot: string }> = {
  Draft: { color: 'text-wms-muted', bg: 'bg-wms-hover', border: 'border-wms-border', dot: 'bg-wms-muted' },
  InProgress: { color: 'text-wms-indigo', bg: 'bg-wms-indigo/10', border: 'border-wms-indigo/20', dot: 'bg-wms-indigo' },
  InReview: { color: 'text-wms-warning', bg: 'bg-wms-warning/10', border: 'border-wms-warning/20', dot: 'bg-wms-warning' },
  Done: { color: 'text-wms-emerald', bg: 'bg-wms-emerald/10', border: 'border-wms-emerald/20', dot: 'bg-wms-emerald' },
  Closed: { color: 'text-wms-purple', bg: 'bg-wms-purple/10', border: 'border-wms-purple/20', dot: 'bg-wms-purple' },
};

const PRIORITY_BADGE: Record<string, 'default' | 'indigo' | 'warning' | 'danger' | 'emerald'> = {
  Low: 'default',
  Medium: 'indigo',
  High: 'warning',
  Critical: 'danger',
};

function formatDate(dateStr: string | null): string {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleDateString('en-US', {
    month: 'short',
    day: 'numeric',
    year: 'numeric',
  });
}

export default function TasksPage() {
  const currentUser = useCurrentUser();
  const currentUserId = currentUser?.userId;

  const {
    tasks,
    pagination,
    isLoading,
    error,
    params,
    setSearch,
    setStatus,
    setPriority,
    setPage,
    setPageSize,
    refresh,
  } = useTasks('', 10);

  const [showFilters, setShowFilters] = useState(false);
  const createModal = useModal();
  const detailModal = useModal();
  const editModal = useModal();
  const confirmDeleteModal = useModal();

  const [selectedTask, setSelectedTask] = useState<TaskDetail | null>(null);
  const [isLoadingDetail, setIsLoadingDetail] = useState(false);
  const [isUpdatingStatus, setIsUpdatingStatus] = useState(false);
  const [isDeletingTask, setIsDeletingTask] = useState(false);

  // Time log states
  const [timeLogs, setTimeLogs] = useState<TimeLog[]>([]);
  const [isLoadingTimeLogs, setIsLoadingTimeLogs] = useState(false);
  const [isSubmittingTimeLog, setIsSubmittingTimeLog] = useState(false);
  const [loggedHours, setLoggedHours] = useState('');
  const [logDate, setLogDate] = useState(new Date().toISOString().split('T')[0]);
  const [timeLogNotes, setTimeLogNotes] = useState('');

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

  const handleStatusChange = async (newStatus: string) => {
    if (!selectedTask) return;
    setIsUpdatingStatus(true);
    try {
      const response = await updateTaskStatus(selectedTask.projectId, selectedTask.id, { status: newStatus });
      if (response.succeeded) {
        toast.success(response.message || 'Status updated successfully.');
        setSelectedTask((prev) => (prev ? { ...prev, status: newStatus as any } : null));
        refresh();
      } else {
        toast.error(response.message || 'Failed to update status.');
      }
    } catch (err: any) {
      toast.error(err?.response?.data?.message || err.message || 'Failed to update status.');
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
        confirmDeleteModal.close();
        detailModal.close();
        refresh();
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

  const hasActiveFilters = !!params.status || !!params.priority;

  const clearFilters = () => {
    setStatus('');
    setPriority('');
    setShowFilters(false);
  };

  const handleRowClick = useCallback(async (taskId: string) => {
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
  }, [detailModal]);

  return (
    <div className="space-y-6 animate-fade-in">
      {/* ─── Page Header ─── */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-wms-border pb-5">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-xl bg-gradient-to-br from-wms-cyan to-wms-emerald flex items-center justify-center shadow-lg shadow-wms-cyan/20">
            <ClipboardList className="h-5 w-5 text-wms-text" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-wms-text tracking-tight">Tasks</h1>
            <p className="text-sm text-wms-secondary mt-0.5">
              {pagination ? (
                <>
                  <span className="text-wms-cyan font-semibold">{pagination.totalCount}</span> total tasks
                </>
              ) : (
                'Manage project tasks and assignments'
              )}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={refresh}
            disabled={isLoading}
            className="inline-flex items-center gap-2 px-3 py-2.5 rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer disabled:opacity-50"
            title="Refresh"
          >
            <RefreshCw className={classNames('h-4 w-4', isLoading && 'animate-spin')} />
          </button>
          <PermissionGate permissions={PERMISSIONS.TASK_CREATE}>
            <button
              onClick={createModal.open}
              className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-wms-cyan hover:bg-cyan-600 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-cyan/25"
            >
              <Plus className="h-4 w-4" />
              Create Task
            </button>
          </PermissionGate>
        </div>
      </div>

      {/* ─── Search & Filters Bar ─── */}
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-wms-muted" />
          <input
            type="text"
            placeholder="Search tasks by title or description..."
            value={params.search || ''}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text placeholder:text-wms-muted
              pl-10 pr-4 py-2.5 transition duration-200 outline-none
              focus:border-wms-cyan focus:ring-2 focus:ring-wms-cyan/20 hover:border-wms-border"
            id="tasks-search"
          />
          {params.search && (
            <button
              onClick={() => setSearch('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-wms-muted hover:text-wms-text transition cursor-pointer"
            >
              <X className="h-4 w-4" />
            </button>
          )}
        </div>
        <button
          onClick={() => setShowFilters(!showFilters)}
          className={classNames(
            'inline-flex items-center gap-2 px-4 py-2.5 rounded-lg border text-sm font-medium transition duration-200 cursor-pointer',
            hasActiveFilters
              ? 'bg-wms-cyan/10 border-wms-cyan/30 text-wms-cyan'
              : 'bg-wms-hover border-wms-border text-wms-secondary hover:text-wms-text hover:bg-wms-hover',
          )}
        >
          <Filter className="h-4 w-4" />
          Filters
          {hasActiveFilters && (
            <span className="ml-1 h-5 w-5 rounded-full bg-wms-cyan text-white text-xs flex items-center justify-center">
              {(params.status ? 1 : 0) + (params.priority ? 1 : 0)}
            </span>
          )}
        </button>
      </div>

      {/* ─── Collapsible Filter Panel ─── */}
      {showFilters && (
        <div className="glass-card rounded-xl p-4 flex flex-wrap items-end gap-4 animate-fade-in">
          <div className="flex flex-col gap-1.5 min-w-[160px]">
            <label className="text-xs font-semibold text-wms-muted uppercase tracking-wide">Status</label>
            <select
              value={params.status || ''}
              onChange={(e) => setStatus(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text appearance-none
                px-4 py-2.5 transition duration-200 outline-none cursor-pointer
                focus:border-wms-cyan focus:ring-2 focus:ring-wms-cyan/20"
            >
              <option value="" className="bg-wms-bg text-wms-muted">All Statuses</option>
              <option value="Draft" className="bg-wms-bg text-wms-text">Draft</option>
              <option value="InProgress" className="bg-wms-bg text-wms-text">In Progress</option>
              <option value="InReview" className="bg-wms-bg text-wms-text">In Review</option>
              <option value="Done" className="bg-wms-bg text-wms-text">Done</option>
              <option value="Closed" className="bg-wms-bg text-wms-text">Closed</option>
            </select>
          </div>

          <div className="flex flex-col gap-1.5 min-w-[160px]">
            <label className="text-xs font-semibold text-wms-muted uppercase tracking-wide">Priority</label>
            <select
              value={params.priority || ''}
              onChange={(e) => setPriority(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text appearance-none
                px-4 py-2.5 transition duration-200 outline-none cursor-pointer
                focus:border-wms-cyan focus:ring-2 focus:ring-wms-cyan/20"
            >
              <option value="" className="bg-wms-bg text-wms-muted">All Priorities</option>
              <option value="Low" className="bg-wms-bg text-wms-text">Low</option>
              <option value="Medium" className="bg-wms-bg text-wms-text">Medium</option>
              <option value="High" className="bg-wms-bg text-wms-text">High</option>
              <option value="Critical" className="bg-wms-bg text-wms-text">Critical</option>
            </select>
          </div>

          {hasActiveFilters && (
            <button
              onClick={clearFilters}
              className="inline-flex items-center gap-1.5 px-3 py-2.5 text-sm text-wms-danger hover:text-red-400 transition cursor-pointer"
            >
              <X className="h-3.5 w-3.5" />
              Clear all
            </button>
          )}
        </div>
      )}

      {/* ─── Error State ─── */}
      {error && (
        <div className="glass-card rounded-xl p-6 border-wms-danger/20 flex items-center gap-3">
          <div className="h-8 w-8 rounded-lg bg-wms-danger/15 flex items-center justify-center flex-shrink-0">
            <AlertCircle className="h-4 w-4 text-wms-danger" />
          </div>
          <div>
            <p className="text-sm font-medium text-wms-danger">Failed to load tasks</p>
            <p className="text-xs text-wms-muted mt-0.5">{error}</p>
          </div>
          <button
            onClick={refresh}
            className="ml-auto text-sm text-wms-cyan hover:text-cyan-400 font-medium transition cursor-pointer"
          >
            Retry
          </button>
        </div>
      )}

      {/* ─── Task Table ─── */}
      <div className="glass-card rounded-xl overflow-hidden animate-fade-in">
        <div className="overflow-x-auto">
          <table className="w-full text-sm" id="tasks-table">
            <thead>
              <tr className="border-b border-wms-border">
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Task</th>
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Status</th>
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide hidden md:table-cell">Priority</th>
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide hidden lg:table-cell">Project</th>
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide hidden xl:table-cell">Assignee</th>
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide hidden xl:table-cell">Due Date</th>
                <th className="px-5 py-3.5 w-10"></th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                Array.from({ length: params.pageSize }).map((_, i) => (
                  <tr key={`skeleton-${i}`} className="border-b border-wms-border">
                    <td className="px-5 py-4"><div className="h-4 w-48 bg-wms-hover rounded animate-pulse" /></td>
                    <td className="px-5 py-4"><div className="h-5 w-20 bg-wms-hover rounded-full animate-pulse" /></td>
                    <td className="px-5 py-4 hidden md:table-cell"><div className="h-5 w-16 bg-wms-hover rounded-full animate-pulse" /></td>
                    <td className="px-5 py-4 hidden lg:table-cell"><div className="h-4 w-28 bg-wms-hover rounded animate-pulse" /></td>
                    <td className="px-5 py-4 hidden xl:table-cell"><div className="h-4 w-24 bg-wms-hover rounded animate-pulse" /></td>
                    <td className="px-5 py-4 hidden xl:table-cell"><div className="h-4 w-20 bg-wms-hover rounded animate-pulse" /></td>
                    <td className="px-5 py-4"><div className="h-4 w-4 bg-wms-hover rounded animate-pulse" /></td>
                  </tr>
                ))
              ) : tasks.length === 0 ? (
                <tr>
                  <td colSpan={7} className="px-5 py-16 text-center">
                    <div className="flex flex-col items-center gap-3">
                      <div className="h-14 w-14 rounded-2xl bg-wms-hover flex items-center justify-center">
                        <ClipboardList className="h-7 w-7 text-wms-muted" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-wms-secondary">No tasks found</p>
                        <p className="text-xs text-wms-muted mt-1">
                          {params.search || hasActiveFilters
                            ? 'Try adjusting your search or filters'
                            : 'No tasks have been created yet'}
                        </p>
                      </div>
                      {(params.search || hasActiveFilters) && (
                        <button
                          onClick={() => { setSearch(''); clearFilters(); }}
                          className="mt-1 text-sm text-wms-cyan hover:text-cyan-400 font-medium transition cursor-pointer"
                        >
                          Clear all filters
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ) : (
                tasks.map((task) => {
                  const statusCfg = STATUS_CONFIG[task.status] || STATUS_CONFIG.Draft;
                  return (
                    <tr
                      key={task.id}
                      onClick={() => handleRowClick(task.id)}
                      className="border-b border-wms-border last:border-b-0 transition duration-150 hover:bg-wms-hover cursor-pointer group"
                    >
                      {/* Task title */}
                      <td className="px-5 py-4">
                        <div className="min-w-0">
                          <p className="text-sm font-semibold text-wms-text truncate max-w-[300px] group-hover:text-wms-cyan transition">
                            {task.title}
                          </p>
                          <p className="text-xs text-wms-muted mt-0.5 truncate max-w-[300px]">
                            {task.teamName} · by {task.createdByName}
                          </p>
                        </div>
                      </td>

                      {/* Status */}
                      <td className="px-5 py-4">
                        <span className={classNames(
                          'inline-flex items-center gap-1.5 text-xs font-medium px-2.5 py-1 rounded-full border',
                          statusCfg.color, statusCfg.bg, statusCfg.border
                        )}>
                          <span className={classNames('h-1.5 w-1.5 rounded-full', statusCfg.dot)} />
                          {task.status === 'InProgress' ? 'In Progress' : task.status === 'InReview' ? 'In Review' : task.status}
                        </span>
                      </td>

                      {/* Priority */}
                      <td className="px-5 py-4 hidden md:table-cell">
                        <Badge variant={PRIORITY_BADGE[task.priority] || 'default'}>
                          {task.priority}
                        </Badge>
                      </td>

                      {/* Project */}
                      <td className="px-5 py-4 hidden lg:table-cell">
                        <p className="text-sm text-wms-text truncate max-w-[180px]">{task.projectName}</p>
                      </td>

                      {/* Assignee */}
                      <td className="px-5 py-4 hidden xl:table-cell">
                        <p className="text-sm text-wms-text truncate max-w-[160px]">
                          {task.assigneeName || <span className="text-wms-muted italic">Unassigned</span>}
                        </p>
                      </td>

                      {/* Due Date */}
                      <td className="px-5 py-4 hidden xl:table-cell">
                        <div className="flex items-center gap-1.5 text-sm text-wms-secondary">
                          {task.dueDate && <Calendar className="h-3.5 w-3.5 text-wms-muted" />}
                          {formatDate(task.dueDate)}
                        </div>
                      </td>

                      {/* Arrow */}
                      <td className="px-5 py-4">
                        <ChevronRight className="h-4 w-4 text-wms-muted group-hover:text-wms-cyan transition" />
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>

        {/* Pagination Footer */}
        {pagination && (
          <Pagination
            pageNumber={pagination.pageNumber}
            pageSize={pagination.pageSize}
            totalCount={pagination.totalCount}
            totalPages={pagination.totalPages}
            hasPreviousPage={pagination.hasPreviousPage}
            hasNextPage={pagination.hasNextPage}
            onPageChange={setPage}
            onPageSizeChange={setPageSize}
            isLoading={isLoading}
          />
        )}
      </div>

      {/* ─── Create Task Modal ─── */}
      <CreateTaskModal
        isOpen={createModal.isOpen}
        onClose={createModal.close}
        onSuccess={refresh}
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
                        onChange={(e) => handleStatusChange(e.target.value)}
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
                value={formatDate(selectedTask.dueDate)}
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
                              <span className="text-[10px] text-wms-muted">{formatDate(log.logDate)}</span>
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
                                  {log.userId !== currentUserId && (
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
              <span>Created {formatDate(selectedTask.createdAt)}{selectedTask.createdBy ? ` by ${selectedTask.createdBy}` : ''}</span>
              {selectedTask.modifiedAt && (
                <span>Modified {formatDate(selectedTask.modifiedAt)}{selectedTask.modifiedBy ? ` by ${selectedTask.modifiedBy}` : ''}</span>
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
                      onClick={confirmDeleteModal.open}
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
                        editModal.open();
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

      {/* ─── Edit Task Modal ─── */}
      <EditTaskModal
        isOpen={editModal.isOpen}
        onClose={editModal.close}
        task={selectedTask}
        onSuccess={() => {
          refresh();
          // Reload details if selectedTask is set
          if (selectedTask) {
            handleRowClick(selectedTask.id);
          }
        }}
      />

      {/* ─── Delete Task Confirmation Modal ─── */}
      <ConfirmDialog
        isOpen={confirmDeleteModal.isOpen}
        onClose={confirmDeleteModal.close}
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

/** Reusable detail field component */
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
