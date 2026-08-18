import { useState, useEffect, useCallback, useRef } from 'react';
import { getProjects } from '@/api/projectsApi';
import type { ProjectListItem, ProjectsQueryParams } from '@/types/project.types';
import type { PaginatedResult } from '@/types/api.types';

interface UseProjectsReturn {
  projects: ProjectListItem[];
  pagination: Omit<PaginatedResult<ProjectListItem>, 'items'> | null;
  isLoading: boolean;
  error: string | null;
  params: ProjectsQueryParams;
  setSearch: (search: string) => void;
  setStatus: (status: string) => void;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  refresh: () => void;
}

export function useProjects(initialPageSize = 10): UseProjectsReturn {
  const [projects, setProjects] = useState<ProjectListItem[]>([]);
  const [pagination, setPagination] = useState<Omit<PaginatedResult<ProjectListItem>, 'items'> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [params, setParams] = useState<ProjectsQueryParams>({
    pageNumber: 1,
    pageSize: initialPageSize,
    search: '',
    status: '',
  });

  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [debouncedSearch, setDebouncedSearch] = useState('');

  const fetchProjects = useCallback(async (queryParams: ProjectsQueryParams, search: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getProjects({ ...queryParams, search });
      if (response.succeeded && response.data) {
        setProjects(response.data.items);
        const { items: _, ...paginationData } = response.data;
        setPagination(paginationData);
      } else {
        setError(response.message || 'Failed to fetch projects');
        setProjects([]);
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred');
      setProjects([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchProjects(params, debouncedSearch);
  }, [params.pageNumber, params.pageSize, params.status, debouncedSearch, fetchProjects]);

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

  const setPage = useCallback((page: number) => {
    setParams((prev) => ({ ...prev, pageNumber: page }));
  }, []);

  const setPageSize = useCallback((size: number) => {
    setParams((prev) => ({ ...prev, pageSize: size, pageNumber: 1 }));
  }, []);

  const refresh = useCallback(() => {
    fetchProjects(params, debouncedSearch);
  }, [params, debouncedSearch, fetchProjects]);

  return {
    projects,
    pagination,
    isLoading,
    error,
    params,
    setSearch,
    setStatus,
    setPage,
    setPageSize,
    refresh,
  };
}
