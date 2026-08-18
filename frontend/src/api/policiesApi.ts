import axiosInstance from './axiosInstance';
import type { ApiResponse, PaginatedResult } from '@/types/api.types';

export interface PolicyCondition {
  id: string;
  conditionGroup: number;
  attribute: string;
  operator: string;
  value: string;
}

export interface FieldRestriction {
  id: string;
  fieldName: string;
  restrictionType: string; // Hide, Mask, ReadOnly
  maskPattern?: string;
}

export interface PolicyDetailDto {
  id: string;
  name: string;
  description?: string;
  moduleId: string;
  moduleCode: string;
  moduleName: string;
  action: string;
  effect: string; // Allow, Deny
  roleId?: string;
  roleName?: string;
  userId?: string;
  userName?: string;
  rowFilterExpression?: string;
  priority: number;
  isActive: boolean;
  expiresAt?: string;
  conditions: PolicyCondition[];
  fieldRestrictions: FieldRestriction[];
}

export interface PolicyListDto {
  id: string;
  name: string;
  moduleCode: string;
  action: string;
  effect: string;
  targetType: string; // Role or User
  targetName: string;
  priority: number;
  isActive: boolean;
  expiresAt?: string;
  createdAt: string;
}

export interface GetPoliciesQueryParams {
  pageNumber: number;
  pageSize: number;
  search?: string;
  roleId?: string;
  userId?: string;
  moduleId?: string;
}

export interface CreatePolicyPayload {
  name: string;
  description?: string;
  moduleId: string;
  action: string;
  effect: string;
  roleId?: string;
  userId?: string;
  rowFilterExpression?: string;
  priority: number;
  expiresAt?: string;
  conditions?: {
    conditionGroup: number;
    attribute: string;
    operator: string;
    value: string;
  }[];
  fieldRestrictions?: {
    fieldName: string;
    restrictionType: string;
    maskPattern?: string;
  }[];
}

export interface UpdatePolicyPayload {
  name: string;
  description?: string;
  moduleId: string;
  action: string;
  effect: string;
  roleId?: string;
  userId?: string;
  rowFilterExpression?: string;
  priority: number;
  isActive: boolean;
  expiresAt?: string;
}

export interface ModuleActionDto {
  id: string;
  code: string;
  name: string;
  availableActions: string[];
}

export interface AddConditionPayload {
  conditionGroup: number;
  attribute: string;
  operator: string;
  value: string;
}

export interface AddRestrictionPayload {
  fieldName: string;
  restrictionType: string;
  maskPattern?: string;
}

export async function getPolicies(params: GetPoliciesQueryParams): Promise<ApiResponse<PaginatedResult<PolicyListDto>>> {
  const query: Record<string, string> = {
    pageNumber: String(params.pageNumber),
    pageSize: String(params.pageSize),
  };
  if (params.search) query.search = params.search;
  if (params.roleId) query.roleId = params.roleId;
  if (params.userId) query.userId = params.userId;
  if (params.moduleId) query.moduleId = params.moduleId;

  const { data } = await axiosInstance.get<ApiResponse<PaginatedResult<PolicyListDto>>>('/policies', { params: query });
  return data;
}

export async function getPolicyById(id: string): Promise<ApiResponse<PolicyDetailDto>> {
  const { data } = await axiosInstance.get<ApiResponse<PolicyDetailDto>>(`/policies/${id}`);
  return data;
}

export async function createPolicy(payload: CreatePolicyPayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/policies', payload);
  return data;
}

export async function updatePolicy(id: string, payload: UpdatePolicyPayload): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.put<ApiResponse<boolean>>(`/policies/${id}`, payload);
  return data;
}

export async function deletePolicy(id: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/policies/${id}`);
  return data;
}

export async function addPolicyCondition(policyId: string, payload: AddConditionPayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>(`/policies/${policyId}/conditions`, payload);
  return data;
}

export async function removePolicyCondition(policyId: string, conditionId: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/policies/${policyId}/conditions/${conditionId}`);
  return data;
}

export async function addFieldRestriction(policyId: string, payload: AddRestrictionPayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>(`/policies/${policyId}/fields`, payload);
  return data;
}

export async function removeFieldRestriction(policyId: string, restrictionId: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.delete<ApiResponse<boolean>>(`/policies/${policyId}/fields/${restrictionId}`);
  return data;
}

export async function getModulesWithActions(): Promise<ApiResponse<ModuleActionDto[]>> {
  const { data } = await axiosInstance.get<ApiResponse<ModuleActionDto[]>>('/policies/modules');
  return data;
}

export interface PolicyAttributesDto {
  userAttributes: string[];
  resourceAttributes: string[];
  moduleSupported: boolean;
}

export async function getPolicyAttributes(moduleCode: string): Promise<ApiResponse<PolicyAttributesDto>> {
  const { data } = await axiosInstance.get<ApiResponse<PolicyAttributesDto>>(`/policies/modules/${moduleCode}/attributes`);
  return data;
}

export interface PolicyPreviewResult {
  isAllowed: boolean;
  denialReason?: string;
  noPoliciesExist: boolean;
  evaluatedPolicies: EvaluatedPolicyDetail[];
  finalFieldRestrictions: {
    fieldName: string;
    restrictionType: string;
    maskPattern?: string;
  }[];
  resolvedRowFilter?: string;
}

export interface EvaluatedPolicyDetail {
  policyId: string;
  policyName: string;
  effect: string;
  priority: number;
  passedConditions: boolean;
  conditions: EvaluatedConditionDetail[];
}

export interface EvaluatedConditionDetail {
  conditionGroup: number;
  attribute: string;
  operator: string;
  value: string;
  resolvedLeftValue?: string;
  resolvedRightValue?: string;
  passed: boolean;
}

export interface PreviewPolicyParams {
  userId: string;
  moduleCode: string;
  action: string;
  resourceContextJson?: string;
}

export interface ImportPoliciesPayload {
  policies: any[];
  overwriteExisting: boolean;
}

export interface ImportReportDto {
  totalProcessed: number;
  importedCount: number;
  overwrittenCount: number;
  skippedCount: number;
  errors: string[];
}

export async function previewPolicyEffect(payload: PreviewPolicyParams): Promise<ApiResponse<PolicyPreviewResult>> {
  const { data } = await axiosInstance.post<ApiResponse<PolicyPreviewResult>>('/policies/preview', payload);
  return data;
}

export async function exportPolicies(): Promise<ApiResponse<any[]>> {
  const { data } = await axiosInstance.get<ApiResponse<any[]>>('/policies/export');
  return data;
}

export async function importPolicies(payload: ImportPoliciesPayload): Promise<ApiResponse<ImportReportDto>> {
  const { data } = await axiosInstance.post<ApiResponse<ImportReportDto>>('/policies/import', payload);
  return data;
}
