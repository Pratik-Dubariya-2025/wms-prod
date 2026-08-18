import { ChevronLeft, ChevronRight, ChevronsLeft, ChevronsRight } from 'lucide-react';
import { classNames } from '@/utils/classNames';

export interface PaginationProps {
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  pageSizeOptions?: number[];
  isLoading?: boolean;
}

export function Pagination({
  pageNumber,
  pageSize,
  totalCount,
  totalPages,
  hasPreviousPage,
  hasNextPage,
  onPageChange,
  onPageSizeChange,
  pageSizeOptions = [5, 10, 20, 50],
  isLoading = false,
}: PaginationProps) {
  if (totalCount === 0) return null;

  const generatePageNumbers = (current: number, total: number): (number | '...')[] => {
    if (total <= 7) {
      return Array.from({ length: total }, (_, i) => i + 1);
    }

    const pages: (number | '...')[] = [];

    if (current <= 4) {
      pages.push(1, 2, 3, 4, 5, '...', total);
    } else if (current >= total - 3) {
      pages.push(1, '...', total - 4, total - 3, total - 2, total - 1, total);
    } else {
      pages.push(1, '...', current - 1, current, current + 1, '...', total);
    }

    return pages;
  };

  const from = (pageNumber - 1) * pageSize + 1;
  const to = Math.min(pageNumber * pageSize, totalCount);

  return (
    <div className="flex flex-col sm:flex-row items-center justify-between gap-4 px-5 py-4 border-t border-wms-border bg-wms-hover">
      {/* Left: page info + page size selector */}
      <div className="flex items-center gap-4 text-sm text-wms-muted">
        <span>
          Showing{' '}
          <span className="text-wms-text font-medium">{from}</span>
          –
          <span className="text-wms-text font-medium">{to}</span>
          {' '}of{' '}
          <span className="text-wms-text font-medium">{totalCount}</span>
        </span>
        <div className="flex items-center gap-2">
          <span className="text-xs">Rows:</span>
          <select
            value={pageSize}
            onChange={(e) => onPageSizeChange(Number(e.target.value))}
            className="bg-wms-hover border border-wms-border rounded-md text-xs text-wms-text px-2.5 py-1
              outline-none focus:border-wms-indigo focus:ring-1 focus:ring-wms-indigo/30 cursor-pointer appearance-none"
          >
            {pageSizeOptions.map((size) => (
              <option key={size} value={size} className="bg-wms-bg">
                {size}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Right: page navigation */}
      <div className="flex items-center gap-1">
        <button
          onClick={() => onPageChange(1)}
          disabled={!hasPreviousPage || isLoading}
          className="p-2 rounded-lg text-wms-muted hover:text-wms-text hover:bg-wms-hover transition disabled:opacity-30 disabled:cursor-not-allowed cursor-pointer"
          title="First page"
        >
          <ChevronsLeft className="h-4 w-4" />
        </button>
        <button
          onClick={() => onPageChange(pageNumber - 1)}
          disabled={!hasPreviousPage || isLoading}
          className="p-2 rounded-lg text-wms-muted hover:text-wms-text hover:bg-wms-hover transition disabled:opacity-30 disabled:cursor-not-allowed cursor-pointer"
          title="Previous page"
        >
          <ChevronLeft className="h-4 w-4" />
        </button>

        {/* Page number buttons */}
        {generatePageNumbers(pageNumber, totalPages).map((page, idx) =>
          page === '...' ? (
            <span key={`ellipsis-${idx}`} className="px-2 text-wms-muted text-sm select-none">
              …
            </span>
          ) : (
            <button
              key={page}
              onClick={() => onPageChange(page as number)}
              disabled={isLoading}
              className={classNames(
                'h-9 min-w-9 px-2 rounded-lg text-sm font-medium transition duration-200 cursor-pointer',
                pageNumber === page
                  ? 'bg-wms-indigo text-white shadow-md shadow-wms-indigo/30 font-bold'
                  : 'text-wms-muted hover:text-wms-text hover:bg-wms-hover'
              )}
            >
              {page}
            </button>
          )
        )}

        <button
          onClick={() => onPageChange(pageNumber + 1)}
          disabled={!hasNextPage || isLoading}
          className="p-2 rounded-lg text-wms-muted hover:text-wms-text hover:bg-wms-hover transition disabled:opacity-30 disabled:cursor-not-allowed cursor-pointer"
          title="Next page"
        >
          <ChevronRight className="h-4 w-4" />
        </button>
        <button
          onClick={() => onPageChange(totalPages)}
          disabled={!hasNextPage || isLoading}
          className="p-2 rounded-lg text-wms-muted hover:text-wms-text hover:bg-wms-hover transition disabled:opacity-30 disabled:cursor-not-allowed cursor-pointer"
          title="Last page"
        >
          <ChevronsRight className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
}
