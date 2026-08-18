import { useState, useEffect, useCallback, useRef } from 'react';
import { getUsers } from '@/api/usersApi';
import type { UserListItem, UsersQueryParams } from '@/features/users/types/user.types';
import type { PaginatedResult } from '@/types/api.types';

interface UseUsersReturn {
  users: UserListItem[];
  pagination: Omit<PaginatedResult<UserListItem>, 'items'> | null;
  isLoading: boolean;
  error: string | null;
  params: UsersQueryParams;
  setSearch: (search: string) => void;
  setRole: (role: string) => void;
  setStatus: (status: boolean | null) => void;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  refresh: () => void;
}

export function useUsers(initialPageSize = 10): UseUsersReturn {
  const [users, setUsers] = useState<UserListItem[]>([]);
  const [pagination, setPagination] = useState<Omit<PaginatedResult<UserListItem>, 'items'> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [params, setParams] = useState<UsersQueryParams>({
    pageNumber: 1,
    pageSize: initialPageSize,
    search: '',
    role: '',
    isActive: null,
  });

  // Debounce timer ref for search
  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  // Track the actual search value sent to API (debounced)
  const [debouncedSearch, setDebouncedSearch] = useState('');

  const fetchUsers = useCallback(async (queryParams: UsersQueryParams, search: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getUsers({ ...queryParams, search });
      if (response.succeeded && response.data) {
        setUsers(response.data.items);
        const { items: _, ...paginationData } = response.data;
        setPagination(paginationData);
      } else {
        setError(response.message || 'Failed to fetch users');
        setUsers([]);
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'An unexpected error occurred';
      setError(message);
      setUsers([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Fetch on params or debouncedSearch change
  useEffect(() => {
    fetchUsers(params, debouncedSearch);
  }, [params.pageNumber, params.pageSize, params.role, params.isActive, debouncedSearch, fetchUsers]);

  const setSearch = useCallback((search: string) => {
    setParams((prev) => ({ ...prev, search, pageNumber: 1 }));
    // Debounce the actual API call
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    searchTimerRef.current = setTimeout(() => {
      setDebouncedSearch(search);
    }, 400);
  }, []);

  const setRole = useCallback((role: string) => {
    setParams((prev) => ({ ...prev, role, pageNumber: 1 }));
  }, []);

  const setStatus = useCallback((status: boolean | null) => {
    setParams((prev) => ({ ...prev, isActive: status, pageNumber: 1 }));
  }, []);

  const setPage = useCallback((page: number) => {
    setParams((prev) => ({ ...prev, pageNumber: page }));
  }, []);

  const setPageSize = useCallback((size: number) => {
    setParams((prev) => ({ ...prev, pageSize: size, pageNumber: 1 }));
  }, []);

  const refresh = useCallback(() => {
    fetchUsers(params, debouncedSearch);
  }, [params, debouncedSearch, fetchUsers]);

  return {
    users,
    pagination,
    isLoading,
    error,
    params,
    setSearch,
    setRole,
    setStatus,
    setPage,
    setPageSize,
    refresh,
  };
}
