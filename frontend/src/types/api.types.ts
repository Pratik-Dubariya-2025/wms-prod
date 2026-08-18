// Mirrors the backend WMS.Application.Common.Models.ApiResponse<T>
export interface ApiResponse<T = unknown> {
  succeeded: boolean;
  data: T | null;
  message: string | null;
  statusCode: number;
  errors: Record<string, string[]> | null;
}

// Mirrors the backend WMS.Application.Common.Models.PaginatedResult<T>
export interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalCount: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

// Mirrors the backend WMS.Application.Common.Models.PaginationDTO<T>
export interface PaginationRequest {
  pageNumber: number;
  itemsPerPage: number;
  isSort: boolean;
  isAsc: boolean;
  sortBy: string;
  search?: string;
}
