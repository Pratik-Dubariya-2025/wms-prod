import { useState, useEffect, useCallback, useRef } from 'react';
import { getTasks } from '@/api/tasksApi';
import type { TaskListItem, TasksQueryParams } from '@/features/tasks/types/task.types';
import type { PaginatedResult } from '@/types/api.types';

interface UseTasksReturn {
  tasks: TaskListItem[];
  pagination: Omit<PaginatedResult<TaskListItem>, 'items'> | null;
  isLoading: boolean;
  error: string | null;
  params: TasksQueryParams;
  setSearch: (search: string) => void;
  setStatus: (status: string) => void;
  setPriority: (priority: string) => void;
  setAssigneeId: (assigneeId: string) => void;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  refresh: () => void;
}

export function useTasks(projectId: string, initialPageSize = 10): UseTasksReturn {
  const [tasks, setTasks] = useState<TaskListItem[]>([]);
  const [pagination, setPagination] = useState<Omit<PaginatedResult<TaskListItem>, 'items'> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [params, setParams] = useState<TasksQueryParams>({
    pageNumber: 1,
    pageSize: initialPageSize,
    search: '',
    status: '',
    priority: '',
    projectId: projectId,
    assigneeId: '',
  });

  // Debounce timer ref for search
  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [debouncedSearch, setDebouncedSearch] = useState('');

  const fetchTasks = useCallback(async (queryParams: TasksQueryParams, search: string) => {
    if (!projectId) return;
    setIsLoading(true);
    setError(null);
    try {
      const response = await getTasks(projectId, { ...queryParams, search });
      if (response.succeeded && response.data) {
        setTasks(response.data.items);
        const { items: _, ...paginationData } = response.data;
        setPagination(paginationData);
      } else {
        setError(response.message || 'Failed to fetch tasks');
        setTasks([]);
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'An unexpected error occurred';
      setError(message);
      setTasks([]);
    } finally {
      setIsLoading(false);
    }
  }, [projectId]);

  // Fetch on params or debouncedSearch change
  useEffect(() => {
    fetchTasks(params, debouncedSearch);
  }, [
    params.pageNumber,
    params.pageSize,
    params.status,
    params.priority,
    params.assigneeId,
    debouncedSearch,
    fetchTasks,
  ]);

  const setSearch = useCallback((search: string) => {
    setParams((prev) => ({ ...prev, search, pageNumber: 1 }));
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    searchTimerRef.current = setTimeout(() => {
      setDebouncedSearch(search);
    }, 400);
  }, []);

  const setStatus = useCallback((status: string) => {
    setParams((prev) => ({ ...prev, status, pageNumber: 1 }));
  }, []);

  const setPriority = useCallback((priority: string) => {
    setParams((prev) => ({ ...prev, priority, pageNumber: 1 }));
  }, []);

  const setAssigneeId = useCallback((assigneeId: string) => {
    setParams((prev) => ({ ...prev, assigneeId, pageNumber: 1 }));
  }, []);

  const setPage = useCallback((page: number) => {
    setParams((prev) => ({ ...prev, pageNumber: page }));
  }, []);

  const setPageSize = useCallback((size: number) => {
    setParams((prev) => ({ ...prev, pageSize: size, pageNumber: 1 }));
  }, []);

  const refresh = useCallback(() => {
    fetchTasks(params, debouncedSearch);
  }, [params, debouncedSearch, fetchTasks]);

  return {
    tasks,
    pagination,
    isLoading,
    error,
    params,
    setSearch,
    setStatus,
    setPriority,
    setAssigneeId,
    setPage,
    setPageSize,
    refresh,
  };
}
