import { useState, useEffect, useMemo, useCallback } from 'react';
import { getDepartments, deleteDepartment } from '@/api/departmentApi';
import type { DepartmentListItem } from '../types/department.types';

interface DepartmentStats {
  total: number;
  active: number;
  totalMembers: number;
  totalDesignations: number;
}

interface UseDepartmentsReturn {
  departments: DepartmentListItem[];
  filteredDepartments: DepartmentListItem[];
  stats: DepartmentStats;
  isLoading: boolean;
  error: string | null;
  searchQuery: string;
  setSearchQuery: (q: string) => void;
  statusFilter: 'all' | 'active' | 'inactive';
  setStatusFilter: (f: 'all' | 'active' | 'inactive') => void;
  refresh: () => void;
  handleDelete: (id: string) => Promise<{ succeeded: boolean; message?: string }>;
}

export function useDepartments(): UseDepartmentsReturn {
  const [allDepartments, setAllDepartments] = useState<DepartmentListItem[]>([]);
  const [filteredDepartments, setFilteredDepartments] = useState<DepartmentListItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [statusFilter, setStatusFilter] = useState<'all' | 'active' | 'inactive'>('all');

  // Fetch overall departments for summary stats
  const fetchAllForStats = useCallback(async () => {
    try {
      const res = await getDepartments();
      if (res.succeeded && res.data) {
        setAllDepartments(res.data);
      }
    } catch {
      // Ignore errors for stats background fetch
    }
  }, []);

  // Fetch backend-filtered departments
  const fetchFilteredDepartments = useCallback(async (search: string, status: 'all' | 'active' | 'inactive') => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getDepartments({
        search: search.trim() || undefined,
        status: status !== 'all' ? status : undefined,
      });
      if (response.succeeded && response.data) {
        setFilteredDepartments(response.data);
      } else {
        setError(response.message || 'Failed to fetch departments.');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'An error occurred while loading departments.');
    } finally {
      setIsLoading(false);
    }
  }, []);

  // Backend query execution with 300ms debounce for live search typing
  useEffect(() => {
    const handler = setTimeout(() => {
      fetchFilteredDepartments(searchQuery, statusFilter);
    }, 300);

    return () => clearTimeout(handler);
  }, [searchQuery, statusFilter, fetchFilteredDepartments]);

  // Initial overall stats fetch
  useEffect(() => {
    fetchAllForStats();
  }, [fetchAllForStats]);

  const stats = useMemo<DepartmentStats>(() => {
    const source = allDepartments.length > 0 ? allDepartments : filteredDepartments;
    const total = source.length;
    const active = source.filter((d) => d.isActive).length;
    const totalMembers = source.reduce((acc, d) => acc + d.memberCount, 0);
    const totalDesignations = source.reduce((acc, d) => acc + d.designationCount, 0);
    return { total, active, totalMembers, totalDesignations };
  }, [allDepartments, filteredDepartments]);

  const refresh = useCallback(() => {
    fetchAllForStats();
    fetchFilteredDepartments(searchQuery, statusFilter);
  }, [fetchAllForStats, fetchFilteredDepartments, searchQuery, statusFilter]);

  const handleDelete = useCallback(async (id: string) => {
    try {
      const response = await deleteDepartment(id);
      if (response.succeeded) {
        refresh();
        return { succeeded: true };
      }
      return { succeeded: false, message: response.message || 'Failed to delete department.' };
    } catch (err: unknown) {
      return {
        succeeded: false,
        message: err instanceof Error ? err.message : 'An error occurred while deleting department.',
      };
    }
  }, [refresh]);

  return {
    departments: filteredDepartments,
    filteredDepartments,
    stats,
    isLoading,
    error,
    searchQuery,
    setSearchQuery,
    statusFilter,
    setStatusFilter,
    refresh,
    handleDelete,
  };
}
