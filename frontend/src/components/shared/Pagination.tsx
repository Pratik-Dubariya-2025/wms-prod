import { ChevronLeft, ChevronRight } from 'lucide-react';
import { classNames } from '@/utils/classNames';

interface PaginationProps {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
  /** Show total count info */
  totalCount?: number;
  pageSize?: number;
}

export function Pagination({
  currentPage,
  totalPages,
  onPageChange,
  totalCount,
  pageSize,
}: PaginationProps) {
  if (totalPages <= 1) return null;

  const pages = getPageNumbers(currentPage, totalPages);

  return (
    <div className="flex items-center justify-between pt-4">
      {/* Info */}
      <div className="text-xs text-wms-muted">
        {totalCount != null && pageSize != null && (
          <>
            Showing {Math.min((currentPage - 1) * pageSize + 1, totalCount)}–
            {Math.min(currentPage * pageSize, totalCount)} of {totalCount}
          </>
        )}
      </div>

      {/* Controls */}
      <div className="flex items-center gap-1">
        <button
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage <= 1}
          className="p-2 rounded-lg text-wms-muted hover:bg-wms-hover hover:text-wms-text disabled:opacity-30 disabled:cursor-not-allowed transition duration-200 cursor-pointer"
        >
          <ChevronLeft className="h-4 w-4" />
        </button>

        {pages.map((page, idx) =>
          page === '...' ? (
            <span key={`dots-${idx}`} className="px-2 text-wms-muted text-sm">
              …
            </span>
          ) : (
            <button
              key={page}
              onClick={() => onPageChange(page as number)}
              className={classNames(
                'min-w-[2rem] h-8 rounded-lg text-sm font-medium transition duration-200 cursor-pointer',
                currentPage === page
                  ? 'bg-wms-indigo text-white'
                  : 'text-wms-muted hover:bg-wms-hover hover:text-wms-text',
              )}
            >
              {page}
            </button>
          ),
        )}

        <button
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage >= totalPages}
          className="p-2 rounded-lg text-wms-muted hover:bg-wms-hover hover:text-wms-text disabled:opacity-30 disabled:cursor-not-allowed transition duration-200 cursor-pointer"
        >
          <ChevronRight className="h-4 w-4" />
        </button>
      </div>
    </div>
  );
}

/** Generate page numbers with ellipsis */
function getPageNumbers(current: number, total: number): (number | '...')[] {
  if (total <= 7) {
    return Array.from({ length: total }, (_, i) => i + 1);
  }

  const pages: (number | '...')[] = [1];

  if (current > 3) pages.push('...');

  const start = Math.max(2, current - 1);
  const end = Math.min(total - 1, current + 1);

  for (let i = start; i <= end; i++) {
    pages.push(i);
  }

  if (current < total - 2) pages.push('...');

  pages.push(total);

  return pages;
}
