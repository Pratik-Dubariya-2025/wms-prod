import { useState, useEffect, useCallback, useRef } from 'react';
import { getRoles } from '@/api/rolesApi';
import type { RoleListItem, RolesQueryParams } from '@/api/rolesApi';
import type { PaginatedResult } from '@/types/api.types';

interface UseRolesReturn {
  roles: RoleListItem[];
  pagination: Omit<PaginatedResult<RoleListItem>, 'items'> | null;
  isLoading: boolean;
  error: string | null;
  params: RolesQueryParams;
  setSearch: (search: string) => void;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  refresh: () => void;
}

export function useRoles(initialPageSize = 5): UseRolesReturn {
  const [roles, setRoles] = useState<RoleListItem[]>([]);
  const [pagination, setPagination] = useState<Omit<PaginatedResult<RoleListItem>, 'items'> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [params, setParams] = useState<RolesQueryParams>({
    pageNumber: 1,
    pageSize: initialPageSize,
    search: '',
  });

  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [debouncedSearch, setDebouncedSearch] = useState('');

  const fetchRoles = useCallback(async (queryParams: RolesQueryParams, search: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getRoles({ ...queryParams, search });
      if (response.succeeded && response.data) {
        setRoles(response.data.items);
        const { items: _, ...paginationData } = response.data;
        setPagination(paginationData);
      } else {
        setError(response.message || 'Failed to fetch roles');
        setRoles([]);
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'An unexpected error occurred';
      setError(message);
      setRoles([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchRoles(params, debouncedSearch);
  }, [params.pageNumber, params.pageSize, debouncedSearch, fetchRoles]);

  const setSearch = useCallback((search: string) => {
    setParams((prev) => ({ ...prev, search, pageNumber: 1 }));
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    searchTimerRef.current = setTimeout(() => {
      setDebouncedSearch(search);
    }, 400);
  }, []);

  const setPage = useCallback((page: number) => {
    setParams((prev) => ({ ...prev, pageNumber: page }));
  }, []);

  const setPageSize = useCallback((size: number) => {
    setParams((prev) => ({ ...prev, pageSize: size, pageNumber: 1 }));
  }, []);

  const refresh = useCallback(() => {
    fetchRoles(params, debouncedSearch);
  }, [params, debouncedSearch, fetchRoles]);

  return {
    roles,
    pagination,
    isLoading,
    error,
    params,
    setSearch,
    setPage,
    setPageSize,
    refresh,
  };
}
