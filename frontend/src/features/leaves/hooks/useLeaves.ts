import { useState, useEffect, useCallback, useRef } from 'react';
import { getLeaveRequests, createLeaveRequest, approveLeaveRequest, rejectLeaveRequest } from '@/api/leavesApi';
import type { LeaveRequestsQueryParams } from '@/api/leavesApi';
import type { LeaveRequest, LeaveStatus, LeaveType, CreateLeaveRequestCommand } from '@/types/leave.types';
import type { PaginatedResult } from '@/types/api.types';

interface UseLeavesReturn {
  leaves: LeaveRequest[];
  pagination: Omit<PaginatedResult<LeaveRequest>, 'items'> | null;
  isLoading: boolean;
  error: string | null;
  params: LeaveRequestsQueryParams;
  setSearch: (search: string) => void;
  setStatus: (status: LeaveStatus | '') => void;
  setLeaveType: (type: LeaveType | '') => void;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  refresh: () => void;
  submitLeave: (payload: CreateLeaveRequestCommand) => Promise<{ success: boolean; message?: string }>;
  approveLeave: (id: string) => Promise<{ success: boolean; message?: string }>;
  rejectLeave: (id: string, rejectionReason: string) => Promise<{ success: boolean; message?: string }>;
}

export function useLeaves(initialPageSize = 10, approvalsOnly?: boolean, teamOnly?: boolean): UseLeavesReturn {
  const [leaves, setLeaves] = useState<LeaveRequest[]>([]);
  const [pagination, setPagination] = useState<Omit<PaginatedResult<LeaveRequest>, 'items'> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [params, setParams] = useState<LeaveRequestsQueryParams>({
    pageNumber: 1,
    pageSize: initialPageSize,
    search: '',
    status: '',
    leaveType: '',
    approvalsOnly,
    teamOnly,
  });

  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [debouncedSearch, setDebouncedSearch] = useState('');

  const fetchLeaves = useCallback(async (queryParams: LeaveRequestsQueryParams, search: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getLeaveRequests({ ...queryParams, search });
      if (response.succeeded && response.data) {
        setLeaves(response.data.items);
        const { items: _, ...paginationData } = response.data;
        setPagination(paginationData);
      } else {
        setError(response.message || 'Failed to fetch leave requests');
        setLeaves([]);
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'An unexpected error occurred';
      setError(message);
      setLeaves([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchLeaves(params, debouncedSearch);
  }, [params.pageNumber, params.pageSize, params.status, params.leaveType, params.approvalsOnly, params.teamOnly, debouncedSearch, fetchLeaves]);

  const setSearch = useCallback((search: string) => {
    setParams((prev) => ({ ...prev, search, pageNumber: 1 }));
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    searchTimerRef.current = setTimeout(() => {
      setDebouncedSearch(search);
    }, 400);
  }, []);

  const setStatus = useCallback((status: LeaveStatus | '') => {
    setParams((prev) => ({ ...prev, status, pageNumber: 1 }));
  }, []);

  const setLeaveType = useCallback((leaveType: LeaveType | '') => {
    setParams((prev) => ({ ...prev, leaveType, pageNumber: 1 }));
  }, []);

  const setPage = useCallback((page: number) => {
    setParams((prev) => ({ ...prev, pageNumber: page }));
  }, []);

  const setPageSize = useCallback((size: number) => {
    setParams((prev) => ({ ...prev, pageSize: size, pageNumber: 1 }));
  }, []);

  const refresh = useCallback(() => {
    fetchLeaves(params, debouncedSearch);
  }, [params, debouncedSearch, fetchLeaves]);

  const submitLeave = useCallback(async (payload: CreateLeaveRequestCommand) => {
    try {
      const response = await createLeaveRequest(payload);
      if (response.succeeded) {
        refresh();
        return { success: true, message: response.message || 'Leave request submitted successfully.' };
      } else {
        return { success: false, message: response.message || 'Failed to submit leave request.' };
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Network error occurred.';
      return { success: false, message };
    }
  }, [refresh]);

  const approveLeave = useCallback(async (id: string) => {
    try {
      const response = await approveLeaveRequest(id);
      if (response.succeeded) {
        refresh();
        return { success: true, message: response.message || 'Leave request approved successfully.' };
      } else {
        return { success: false, message: response.message || 'Failed to approve leave request.' };
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Network error occurred.';
      return { success: false, message };
    }
  }, [refresh]);

  const rejectLeave = useCallback(async (id: string, rejectionReason: string) => {
    try {
      const response = await rejectLeaveRequest(id, { rejectionReason });
      if (response.succeeded) {
        refresh();
        return { success: true, message: response.message || 'Leave request rejected successfully.' };
      } else {
        return { success: false, message: response.message || 'Failed to reject leave request.' };
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Network error occurred.';
      return { success: false, message };
    }
  }, [refresh]);

  return {
    leaves,
    pagination,
    isLoading,
    error,
    params,
    setSearch,
    setStatus,
    setLeaveType,
    setPage,
    setPageSize,
    refresh,
    submitLeave,
    approveLeave,
    rejectLeave,
  };
}
