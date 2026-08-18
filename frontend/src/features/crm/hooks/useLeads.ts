import { useState, useEffect, useCallback, useRef } from 'react';
import { getLeads, createLead, updateLeadStage } from '@/api/crmApi';
import type { LeadQueryParams } from '@/api/crmApi';
import type { Lead, LeadStage, CreateLeadCommand } from '@/types/crm.types';
import type { PaginatedResult } from '@/types/api.types';

interface UseLeadsReturn {
  leads: Lead[];
  pagination: Omit<PaginatedResult<Lead>, 'items'> | null;
  isLoading: boolean;
  error: string | null;
  params: LeadQueryParams;
  setSearch: (search: string) => void;
  setStage: (stage: LeadStage | '') => void;
  setRegion: (region: string) => void;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  refresh: () => void;
  submitLead: (payload: CreateLeadCommand) => Promise<{ success: boolean; message?: string }>;
  changeLeadStage: (id: string, stage: LeadStage) => Promise<{ success: boolean; message?: string }>;
}

export function useLeads(initialPageSize = 10): UseLeadsReturn {
  const [leads, setLeads] = useState<Lead[]>([]);
  const [pagination, setPagination] = useState<Omit<PaginatedResult<Lead>, 'items'> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [params, setParams] = useState<LeadQueryParams>({
    pageNumber: 1,
    pageSize: initialPageSize,
    search: '',
    stage: '',
    region: '',
  });

  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [debouncedSearch, setDebouncedSearch] = useState('');

  const fetchLeads = useCallback(async (queryParams: LeadQueryParams, search: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getLeads({ ...queryParams, search });
      if (response.succeeded && response.data) {
        setLeads(response.data.items);
        const { items: _, ...paginationData } = response.data;
        setPagination(paginationData);
      } else {
        setError(response.message || 'Failed to fetch CRM leads');
        setLeads([]);
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'An unexpected error occurred';
      setError(message);
      setLeads([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchLeads(params, debouncedSearch);
  }, [params.pageNumber, params.pageSize, params.stage, params.region, debouncedSearch, fetchLeads]);

  const setSearch = useCallback((search: string) => {
    setParams((prev) => ({ ...prev, search, pageNumber: 1 }));
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    searchTimerRef.current = setTimeout(() => {
      setDebouncedSearch(search);
    }, 400);
  }, []);

  const setStage = useCallback((stage: LeadStage | '') => {
    setParams((prev) => ({ ...prev, stage, pageNumber: 1 }));
  }, []);

  const setRegion = useCallback((region: string) => {
    setParams((prev) => ({ ...prev, region, pageNumber: 1 }));
  }, []);

  const setPage = useCallback((page: number) => {
    setParams((prev) => ({ ...prev, pageNumber: page }));
  }, []);

  const setPageSize = useCallback((size: number) => {
    setParams((prev) => ({ ...prev, pageSize: size, pageNumber: 1 }));
  }, []);

  const refresh = useCallback(() => {
    fetchLeads(params, debouncedSearch);
  }, [params, debouncedSearch, fetchLeads]);

  const submitLead = useCallback(async (payload: CreateLeadCommand) => {
    try {
      const response = await createLead(payload);
      if (response.succeeded) {
        refresh();
        return { success: true, message: response.message || 'Lead created successfully.' };
      } else {
        return { success: false, message: response.message || 'Failed to create lead.' };
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Network error occurred.';
      return { success: false, message };
    }
  }, [refresh]);

  const changeLeadStage = useCallback(async (id: string, stage: LeadStage) => {
    try {
      const response = await updateLeadStage(id, stage);
      if (response.succeeded) {
        refresh();
        return { success: true, message: response.message || 'Lead stage updated successfully.' };
      } else {
        return { success: false, message: response.message || 'Failed to update lead stage.' };
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Network error occurred.';
      return { success: false, message };
    }
  }, [refresh]);

  return {
    leads,
    pagination,
    isLoading,
    error,
    params,
    setSearch,
    setStage,
    setRegion,
    setPage,
    setPageSize,
    refresh,
    submitLead,
    changeLeadStage,
  };
}
