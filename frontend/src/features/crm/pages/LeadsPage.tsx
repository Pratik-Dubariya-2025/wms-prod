import { useState } from 'react';
import { Briefcase, RefreshCw, Plus } from 'lucide-react';

import { useLeads } from '../hooks/useLeads';
import type { Lead } from '@/types/crm.types';
import { LeadFilters } from '../components/LeadFilters';
import { LeadTable } from '../components/LeadTable';
import { LeadRequestModal } from '../components/LeadRequestModal';
import { LeadStageModal } from '../components/LeadStageModal';
import { LeadDetailsModal } from '../components/LeadDetailsModal';
import { LeadInvoiceModal } from '../components/LeadInvoiceModal';

import { createInvoice } from '@/api/invoicesApi';
import { PermissionGate } from '@/components/shared/PermissionGate';
import { PERMISSIONS } from '@/constants/permissions';
import { useModal } from '@/components/ui/Modal/useModal';
import { classNames } from '@/utils/classNames';

export default function LeadsPage() {
  const {
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
  } = useLeads(10);

  // Modals state control
  const createLeadModal = useModal();
  const stageModal = useModal();
  const detailsModal = useModal();
  const invoiceModal = useModal();

  // Selected item state for details and updates
  const [selectedLead, setSelectedLead] = useState<Lead | null>(null);

  const handleOpenStage = (lead: Lead) => {
    setSelectedLead(lead);
    stageModal.open();
  };

  const handleOpenDetails = (lead: Lead) => {
    setSelectedLead(lead);
    detailsModal.open();
  };

  const handleOpenInvoice = (lead: Lead) => {
    setSelectedLead(lead);
    invoiceModal.open();
  };

  const handleGenerateInvoice = async (payload: any) => {
    try {
      const response = await createInvoice(payload);
      if (response.succeeded) {
        refresh();
        return { success: true, message: response.message || 'Invoice generated successfully.' };
      } else {
        return { success: false, message: response.message || 'Failed to generate invoice.' };
      }
    } catch (err: unknown) {
      const message = err instanceof Error ? err.message : 'Network error occurred.';
      return { success: false, message };
    }
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-wms-border pb-5">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-xl bg-gradient-to-br from-wms-indigo to-wms-cyan flex items-center justify-center shadow-lg shadow-wms-indigo/20">
            <Briefcase className="h-5 w-5 text-wms-text" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-wms-text tracking-tight">CRM Leads</h1>
            <p className="text-sm text-wms-secondary mt-0.5">
              {pagination ? (
                <>
                  Track and manage <span className="text-wms-indigo font-semibold">{pagination.totalCount}</span> CRM leads
                </>
              ) : (
                'Manage customer relation leads and conversion pipeline'
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
          <PermissionGate permissions={PERMISSIONS.LEADS_WRITE}>
            <button
              onClick={createLeadModal.open}
              className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-indigo/25 select-none"
            >
              <Plus className="h-4 w-4" />
              Create Lead
            </button>
          </PermissionGate>
        </div>
      </div>

      {/* Search & Filters */}
      <LeadFilters
        search={params.search || ''}
        onSearchChange={setSearch}
        stage={params.stage || ''}
        onStageChange={setStage}
        region={params.region || ''}
        onRegionChange={setRegion}
      />

      {/* Error View */}
      {error && (
        <div className="glass-card rounded-xl p-6 border-wms-danger/20 flex items-center gap-3">
          <div className="h-8 w-8 rounded-lg bg-wms-danger/15 flex items-center justify-center flex-shrink-0">
            <span className="text-wms-danger font-semibold">!</span>
          </div>
          <div>
            <p className="text-sm font-medium text-wms-danger">Failed to load CRM leads</p>
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

      {/* Leads Table */}
      <LeadTable
        leads={leads}
        isLoading={isLoading}
        onViewDetails={handleOpenDetails}
        onUpdateStage={handleOpenStage}
        onCreateInvoice={handleOpenInvoice}
        pageSize={params.pageSize}
        pagination={pagination}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        hasActiveFilters={!!params.stage || !!params.region}
        searchQuery={params.search || ''}
      />

      {/* Create Lead Modal */}
      <LeadRequestModal
        isOpen={createLeadModal.isOpen}
        onClose={createLeadModal.close}
        onSubmitLead={submitLead}
      />

      {/* Update Stage Modal */}
      <LeadStageModal
        isOpen={stageModal.isOpen}
        onClose={stageModal.close}
        selectedLead={selectedLead}
        onUpdateStage={changeLeadStage}
        onSuccess={refresh}
      />

      {/* Lead Details Modal */}
      <LeadDetailsModal
        isOpen={detailsModal.isOpen}
        onClose={detailsModal.close}
        selectedLead={selectedLead}
      />

      {/* Generate Invoice Modal */}
      <LeadInvoiceModal
        isOpen={invoiceModal.isOpen}
        onClose={invoiceModal.close}
        selectedLead={selectedLead}
        onGenerateInvoice={handleGenerateInvoice}
      />
    </div>
  );
}
