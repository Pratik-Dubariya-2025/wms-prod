import { useState, useEffect, useCallback, useRef } from 'react';
import { getInvoices } from '@/api/invoicesApi';
import type { InvoiceQueryParams } from '@/api/invoicesApi';
import type { Invoice } from '@/types/invoice.types';
import type { PaginatedResult } from '@/types/api.types';

interface UseInvoicesReturn {
  invoices: Invoice[];
  pagination: Omit<PaginatedResult<Invoice>, 'items'> | null;
  isLoading: boolean;
  error: string | null;
  params: InvoiceQueryParams;
  setSearch: (search: string) => void;
  setStatus: (status: string) => void;
  setPage: (page: number) => void;
  setPageSize: (size: number) => void;
  refresh: () => void;
}

export function useInvoices(initialPageSize = 10): UseInvoicesReturn {
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [pagination, setPagination] = useState<Omit<PaginatedResult<Invoice>, 'items'> | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [params, setParams] = useState<InvoiceQueryParams>({
    pageNumber: 1,
    pageSize: initialPageSize,
    search: '',
    status: '',
  });

  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [debouncedSearch, setDebouncedSearch] = useState('');

  const fetchInvoices = useCallback(async (queryParams: InvoiceQueryParams, search: string) => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getInvoices({ ...queryParams, search });
      if (response.succeeded && response.data) {
        setInvoices(response.data.items);
        const { items: _, ...paginationData } = response.data;
        setPagination(paginationData);
      } else {
        setError(response.message || 'Failed to fetch financial invoices');
        setInvoices([]);
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'An unexpected error occurred';
      setError(message);
      setInvoices([]);
    } finally {
      setIsLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchInvoices(params, debouncedSearch);
  }, [params.pageNumber, params.pageSize, params.status, debouncedSearch, fetchInvoices]);

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
    fetchInvoices(params, debouncedSearch);
  }, [params, debouncedSearch, fetchInvoices]);

  return {
    invoices,
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
