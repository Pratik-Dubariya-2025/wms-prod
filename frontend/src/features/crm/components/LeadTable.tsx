import { Briefcase, ChevronRight, FilePlus, Eye } from 'lucide-react';
import type { Lead, LeadStage } from '@/types/crm.types';
import { Badge } from '@/components/ui/Badge/Badge';
import { PermissionGate } from '@/components/shared/PermissionGate';
import { PERMISSIONS } from '@/constants/permissions';
import { Pagination } from '@/components/ui/Pagination/Pagination';

const STAGE_BADGE_MAP: Record<LeadStage, 'warning' | 'emerald' | 'danger' | 'cyan' | 'indigo' | 'default'> = {
  NewLead: 'default',
  Qualified: 'indigo',
  Proposal: 'cyan',
  Negotiation: 'warning',
  ClosedWon: 'emerald',
  ClosedLost: 'danger',
};

const STAGE_LABEL_MAP: Record<LeadStage, string> = {
  NewLead: 'New Lead',
  Qualified: 'Qualified',
  Proposal: 'Proposal',
  Negotiation: 'Negotiation',
  ClosedWon: 'Closed Won',
  ClosedLost: 'Closed Lost',
};

interface LeadTableProps {
  leads: Lead[];
  isLoading: boolean;
  onViewDetails: (lead: Lead) => void;
  onUpdateStage: (lead: Lead) => void;
  onCreateInvoice: (lead: Lead) => void;
  pageSize: number;
  pagination: any;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  hasActiveFilters: boolean;
  searchQuery: string;
}

export function LeadTable({
  leads,
  isLoading,
  onViewDetails,
  onUpdateStage,
  onCreateInvoice,
  pageSize,
  pagination,
  onPageChange,
  onPageSizeChange,
  hasActiveFilters,
  searchQuery,
}: LeadTableProps) {
  return (
    <div className="glass-card rounded-xl overflow-hidden animate-fade-in">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-wms-border">
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Company</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Contact Person</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Estimated Value</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Owner</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Stage</th>
              <th className="px-5 py-3.5 text-right text-xs font-semibold text-wms-muted uppercase tracking-wide">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              Array.from({ length: pageSize }).map((_, i) => (
                <tr key={`skeleton-${i}`} className="border-b border-wms-border">
                  <td className="px-5 py-4">
                    <div className="space-y-2">
                      <div className="h-3.5 w-32 bg-wms-hover rounded animate-pulse" />
                      <div className="h-3 w-16 bg-wms-hover rounded animate-pulse" />
                    </div>
                  </td>
                  <td className="px-5 py-4"><div className="h-3.5 w-24 bg-wms-hover rounded animate-pulse" /></td>
                  <td className="px-5 py-4"><div className="h-3.5 w-20 bg-wms-hover rounded animate-pulse" /></td>
                  <td className="px-5 py-4"><div className="h-3.5 w-28 bg-wms-hover rounded animate-pulse" /></td>
                  <td className="px-5 py-4"><div className="h-5 w-16 bg-wms-hover rounded-full animate-pulse" /></td>
                  <td className="px-5 py-4 text-right"><div className="h-8 w-20 bg-wms-hover rounded ml-auto animate-pulse" /></td>
                </tr>
              ))
            ) : leads.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-5 py-16 text-center">
                  <div className="flex flex-col items-center gap-3">
                    <div className="h-14 w-14 rounded-2xl bg-wms-hover flex items-center justify-center">
                      <Briefcase className="h-7 w-7 text-wms-muted" />
                    </div>
                    <div>
                      <p className="text-sm font-medium text-wms-secondary">No CRM leads found</p>
                      <p className="text-xs text-wms-muted mt-1">
                        {searchQuery || hasActiveFilters
                          ? 'Try adjusting your search or filters'
                          : 'No CRM leads have been created yet'}
                      </p>
                    </div>
                  </div>
                </td>
              </tr>
            ) : (
              leads.map((lead) => (
                <tr
                  key={lead.id}
                  className="border-b border-wms-border last:border-b-0 transition duration-150 hover:bg-wms-hover group"
                >
                  <td className="px-5 py-4">
                    <span className="text-sm font-semibold text-wms-text block truncate max-w-[200px]">
                      {lead.companyName}
                    </span>
                    <span className="text-xs text-wms-muted block">
                      {lead.region || 'No Region'}
                    </span>
                  </td>
                  <td className="px-5 py-4">
                    <span className="text-sm font-medium text-wms-text block">{lead.contactName}</span>
                    <span className="text-xs text-wms-muted block">{lead.contactEmail || 'No Email'}</span>
                  </td>
                  <td className="px-5 py-4">
                    <span className="text-sm font-bold text-wms-text">
                      {lead.estimatedValue !== undefined && lead.estimatedValue !== null
                        ? `$${lead.estimatedValue.toLocaleString()}`
                        : '—'}
                    </span>
                  </td>
                  <td className="px-5 py-4">
                    <span className="text-sm text-wms-secondary">{lead.ownerName}</span>
                  </td>
                  <td className="px-5 py-4">
                    <Badge variant={STAGE_BADGE_MAP[lead.stage] || 'default'}>
                      {STAGE_LABEL_MAP[lead.stage] || lead.stage}
                    </Badge>
                  </td>
                  <td className="px-5 py-4 text-right">
                    <div className="flex items-center justify-end gap-1.5">
                      <button
                        onClick={() => onViewDetails(lead)}
                        className="p-1.5 rounded-lg border border-wms-border bg-wms-hover hover:text-wms-indigo text-wms-secondary transition cursor-pointer"
                        title="View Lead Details"
                      >
                        <Eye className="h-4 w-4" />
                      </button>

                      <PermissionGate permissions={PERMISSIONS.LEADS_WRITE}>
                        <button
                          onClick={() => onUpdateStage(lead)}
                          className="px-2.5 py-1.5 rounded-lg border border-wms-border bg-wms-hover hover:text-wms-indigo text-wms-secondary text-xs font-semibold transition cursor-pointer inline-flex items-center gap-1"
                          title="Update Stage"
                        >
                          Stage
                          <ChevronRight className="h-3 w-3" />
                        </button>
                      </PermissionGate>

                      {lead.stage === 'ClosedWon' && (
                        <PermissionGate permissions={PERMISSIONS.INVOICES_WRITE}>
                          <button
                            onClick={() => onCreateInvoice(lead)}
                            className="p-1.5 rounded-lg bg-wms-emerald/10 hover:bg-wms-emerald/20 text-wms-emerald transition cursor-pointer"
                            title="Generate Invoice"
                          >
                            <FilePlus className="h-4 w-4" />
                          </button>
                        </PermissionGate>
                      )}
                    </div>
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
