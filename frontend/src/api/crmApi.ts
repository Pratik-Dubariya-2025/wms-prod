import axiosInstance from './axiosInstance';
import type { ApiResponse, PaginatedResult } from '@/types/api.types';
import type { Lead, CreateLeadCommand, LeadStage } from '@/types/crm.types';

export interface LeadQueryParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  stage?: LeadStage | '';
  region?: string;
}

/**
 * GET /api/crm/leads
 * Lists CRM leads.
 */
export async function getLeads(params: LeadQueryParams): Promise<ApiResponse<PaginatedResult<Lead>>> {
  const query: Record<string, string> = {
    pageNumber: String(params.pageNumber),
    pageSize: String(params.pageSize),
  };

  if (params.search) query.search = params.search;
  if (params.stage) query.stage = params.stage;
  if (params.region) query.region = params.region;

  const { data } = await axiosInstance.get<ApiResponse<PaginatedResult<Lead>>>('/crm/leads', { params: query });
  return data;
}

/**
 * POST /api/crm/leads
 * Creates a new CRM lead.
 */
export async function createLead(payload: CreateLeadCommand): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/crm/leads', payload);
  return data;
}

/**
 * PATCH /api/crm/leads/{id}/stage
 * Updates the stage of a lead.
 */
export async function updateLeadStage(id: string, stage: LeadStage): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.patch<ApiResponse<boolean>>(`/crm/leads/${id}/stage`, { stage });
  return data;
}
