import axiosInstance from './axiosInstance';
import type { ApiResponse, PaginatedResult } from '@/types/api.types';
import type { Invoice, CreateInvoiceCommand } from '@/types/invoice.types';

export interface InvoiceQueryParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  status?: string;
}

/**
 * GET /api/accounts/invoices
 * Lists financial invoices.
 */
export async function getInvoices(params: InvoiceQueryParams): Promise<ApiResponse<PaginatedResult<Invoice>>> {
  const query: Record<string, string> = {
    pageNumber: String(params.pageNumber),
    pageSize: String(params.pageSize),
  };

  if (params.search) query.search = params.search;
  if (params.status) query.status = params.status;

  const { data } = await axiosInstance.get<ApiResponse<PaginatedResult<Invoice>>>('/accounts/invoices', { params: query });
  return data;
}

/**
 * POST /api/accounts/invoices
 * Generates an invoice from a closed won deal.
 */
export async function createInvoice(payload: CreateInvoiceCommand): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/accounts/invoices', payload);
  return data;
}
