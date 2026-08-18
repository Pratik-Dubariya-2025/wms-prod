import { useState } from 'react';
import { FileText, RefreshCw } from 'lucide-react';

import { useInvoices } from '../hooks/useInvoices';
import type { Invoice } from '@/types/invoice.types';
import { InvoiceFilters } from '../components/InvoiceFilters';
import { InvoiceTable } from '../components/InvoiceTable';
import { InvoiceDetailsModal } from '../components/InvoiceDetailsModal';

import { useModal } from '@/components/ui/Modal/useModal';
import { classNames } from '@/utils/classNames';

export default function InvoicesPage() {
  const {
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
  } = useInvoices(10);

  const detailsModal = useModal();
  const [selectedInvoice, setSelectedInvoice] = useState<Invoice | null>(null);

  const handleOpenDetails = (invoice: Invoice) => {
    setSelectedInvoice(invoice);
    detailsModal.open();
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-wms-border pb-5">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-xl bg-gradient-to-br from-wms-indigo to-wms-cyan flex items-center justify-center shadow-lg shadow-wms-indigo/20">
            <FileText className="h-5 w-5 text-wms-text" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-wms-text tracking-tight">Financial Invoices</h1>
            <p className="text-sm text-wms-secondary mt-0.5">
              {pagination ? (
                <>
                  Track and view <span className="text-wms-indigo font-semibold">{pagination.totalCount}</span> billing invoices
                </>
              ) : (
                'Manage financial invoices generated from Closed Won CRM leads'
              )}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={refresh}
            disabled={isLoading}
            className="inline-flex items-center gap-2 px-3 py-2.5 rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer disabled:opacity-50 select-none"
            title="Refresh"
          >
            <RefreshCw className={classNames('h-4 w-4', isLoading && 'animate-spin')} />
          </button>
        </div>
      </div>

      {/* Filters */}
      <InvoiceFilters
        search={params.search || ''}
        onSearchChange={setSearch}
        status={params.status || ''}
        onStatusChange={setStatus}
      />

      {/* Error display */}
      {error && (
        <div className="glass-card rounded-xl p-6 border-wms-danger/20 flex items-center gap-3">
          <div className="h-8 w-8 rounded-lg bg-wms-danger/15 flex items-center justify-center flex-shrink-0">
            <span className="text-wms-danger font-semibold">!</span>
          </div>
          <div>
            <p className="text-sm font-medium text-wms-danger">Failed to load invoices</p>
            <p className="text-xs text-wms-muted mt-0.5">{error}</p>
          </div>
          <button
            onClick={refresh}
            className="ml-auto text-sm text-wms-indigo hover:text-indigo-400 font-medium transition cursor-pointer"
          >
            Retry
          </button>
        </div>
      )}

      {/* Table */}
      <InvoiceTable
        invoices={invoices}
        isLoading={isLoading}
        onViewDetails={handleOpenDetails}
        pageSize={params.pageSize}
        pagination={pagination}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        hasActiveFilters={!!params.status}
        searchQuery={params.search || ''}
      />

      {/* Details Modal */}
      <InvoiceDetailsModal
        isOpen={detailsModal.isOpen}
        onClose={detailsModal.close}
        selectedInvoice={selectedInvoice}
      />
    </div>
  );
}
