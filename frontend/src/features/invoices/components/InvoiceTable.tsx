import { FileText, Eye } from 'lucide-react';
import type { Invoice } from '@/types/invoice.types';
import { Badge } from '@/components/ui/Badge/Badge';
import { Pagination } from '@/components/ui/Pagination/Pagination';

const STATUS_BADGE_MAP: Record<string, 'warning' | 'emerald' | 'danger' | 'info' | 'default'> = {
  Draft: 'default',
  Sent: 'info',
  Paid: 'emerald',
  Overdue: 'danger',
  Cancelled: 'warning',
};

interface InvoiceTableProps {
  invoices: Invoice[];
  isLoading: boolean;
  onViewDetails: (invoice: Invoice) => void;
  pageSize: number;
  pagination: any;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  hasActiveFilters: boolean;
  searchQuery: string;
}

export function InvoiceTable({
  invoices,
  isLoading,
  onViewDetails,
  pageSize,
  pagination,
  onPageChange,
  onPageSizeChange,
  hasActiveFilters,
  searchQuery,
}: InvoiceTableProps) {
  return (
    <div className="glass-card rounded-xl overflow-hidden animate-fade-in">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-wms-border">
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Invoice #</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Client / Company</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Amount</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Issued Date</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Due Date</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Status</th>
              <th className="px-5 py-3.5 text-right text-xs font-semibold text-wms-muted uppercase tracking-wide">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              Array.from({ length: pageSize }).map((_, i) => (
                <tr key={`skeleton-${i}`} className="border-b border-wms-border">
                  <td className="px-5 py-4"><div className="h-3.5 w-16 bg-wms-hover rounded animate-pulse" /></td>
                  <td className="px-5 py-4">
                    <div className="space-y-2">
                      <div className="h-3.5 w-32 bg-wms-hover rounded animate-pulse" />
                      <div className="h-3 w-20 bg-wms-hover rounded animate-pulse" />
                    </div>
                  </td>
                  <td className="px-5 py-4"><div className="h-3.5 w-14 bg-wms-hover rounded animate-pulse" /></td>
                  <td className="px-5 py-4"><div className="h-3.5 w-20 bg-wms-hover rounded animate-pulse" /></td>
                  <td className="px-5 py-4"><div className="h-3.5 w-20 bg-wms-hover rounded animate-pulse" /></td>
                  <td className="px-5 py-4"><div className="h-5 w-14 bg-wms-hover rounded-full animate-pulse" /></td>
                  <td className="px-5 py-4 text-right"><div className="h-8 w-16 bg-wms-hover rounded ml-auto animate-pulse" /></td>
                </tr>
              ))
            ) : invoices.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-5 py-16 text-center">
                  <div className="flex flex-col items-center gap-3">
                    <div className="h-14 w-14 rounded-2xl bg-wms-hover flex items-center justify-center">
                      <FileText className="h-7 w-7 text-wms-muted" />
                    </div>
                    <div>
                      <p className="text-sm font-medium text-wms-secondary">No invoices found</p>
                      <p className="text-xs text-wms-muted mt-1">
                        {searchQuery || hasActiveFilters
                          ? 'Try adjusting your search or filters'
                          : 'No invoices have been generated yet'}
                      </p>
                    </div>
                  </div>
                </td>
              </tr>
            ) : (
              invoices.map((invoice) => (
                <tr
                  key={invoice.id}
                  className="border-b border-wms-border last:border-b-0 transition duration-150 hover:bg-wms-hover group"
                >
                  <td className="px-5 py-4 font-semibold text-wms-text">
                    {invoice.invoiceNumber}
                  </td>
                  <td className="px-5 py-4">
                    <span className="text-sm font-semibold text-wms-text block truncate max-w-[200px]">
                      {invoice.companyName}
                    </span>
                    <span className="text-xs text-wms-muted block">
                      {invoice.contactName}
                    </span>
                  </td>
                  <td className="px-5 py-4">
                    <span className="text-sm font-bold text-wms-text">
                      ${invoice.amount.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
                    </span>
                  </td>
                  <td className="px-5 py-4 text-xs font-medium text-wms-secondary">
                    {new Date(invoice.issuedDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
                  </td>
                  <td className="px-5 py-4 text-xs font-medium text-wms-secondary">
                    {new Date(invoice.dueDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
                  </td>
                  <td className="px-5 py-4">
                    <Badge variant={STATUS_BADGE_MAP[invoice.status] || 'default'}>
                      {invoice.status}
                    </Badge>
                  </td>
                  <td className="px-5 py-4 text-right">
                    <button
                      onClick={() => onViewDetails(invoice)}
                      className="p-1.5 rounded-lg border border-wms-border bg-wms-hover hover:text-wms-indigo text-wms-secondary transition cursor-pointer inline-flex"
                      title="View Invoice Details"
                    >
                      <Eye className="h-4 w-4" />
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {pagination && (
        <Pagination
          pageNumber={pagination.pageNumber}
          pageSize={pagination.pageSize}
          totalCount={pagination.totalCount}
          totalPages={pagination.totalPages}
          hasPreviousPage={pagination.hasPreviousPage}
          hasNextPage={pagination.hasNextPage}
          onPageChange={onPageChange}
          onPageSizeChange={onPageSizeChange}
          isLoading={isLoading}
        />
      )}
    </div>
  );
}
