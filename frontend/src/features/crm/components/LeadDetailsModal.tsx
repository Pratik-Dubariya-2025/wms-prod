import { Modal } from '@/components/ui/Modal/Modal';
import { Badge } from '@/components/ui/Badge/Badge';
import type { Lead, LeadStage } from '@/types/crm.types';

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

interface LeadDetailsModalProps {
  isOpen: boolean;
  onClose: () => void;
  selectedLead: Lead | null;
}

export function LeadDetailsModal({
  isOpen,
  onClose,
  selectedLead,
}: LeadDetailsModalProps) {
  if (!selectedLead) return null;

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="CRM Lead Details" size="md">
      <div className="space-y-6">
        <div className="flex justify-between items-start border-b border-wms-border pb-4">
          <div>
            <h3 className="text-lg font-bold text-wms-text">{selectedLead.companyName}</h3>
            <span className="text-xs text-wms-muted mt-1 block">Created on {new Date(selectedLead.createdAt).toLocaleDateString(undefined, { dateStyle: 'long' })}</span>
          </div>
          <Badge variant={STAGE_BADGE_MAP[selectedLead.stage] || 'default'}>
            {STAGE_LABEL_MAP[selectedLead.stage] || selectedLead.stage}
          </Badge>
        </div>

        <div className="grid grid-cols-2 gap-6 text-sm">
          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Contact Person</span>
            <span className="text-wms-text font-medium">{selectedLead.contactName}</span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Estimated Value</span>
            <span className="text-wms-text font-bold text-base">
              {selectedLead.estimatedValue !== undefined && selectedLead.estimatedValue !== null
                ? `$${selectedLead.estimatedValue.toLocaleString()}`
                : '—'}
            </span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Contact Email</span>
            <span className="text-wms-text">{selectedLead.contactEmail || '—'}</span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Contact Phone</span>
            <span className="text-wms-text">{selectedLead.contactPhone || '—'}</span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Region</span>
            <span className="text-wms-text">{selectedLead.region || '—'}</span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Lead Source</span>
            <span className="text-wms-text">{selectedLead.source || '—'}</span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Assigned Owner</span>
            <span className="text-wms-text">{selectedLead.ownerName}</span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Closed At Date</span>
            <span className="text-wms-text">
              {selectedLead.closedAt
                ? new Date(selectedLead.closedAt).toLocaleString(undefined, { dateStyle: 'medium', timeStyle: 'short' })
                : '—'}
            </span>
          </div>
        </div>

        <div className="flex justify-end pt-4 border-t border-wms-border">
          <button
            onClick={onClose}
            className="px-5 py-2.5 rounded-lg bg-wms-hover border border-wms-border text-sm font-semibold text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition cursor-pointer"
          >
            Close
          </button>
        </div>
      </div>
    </Modal>
  );
}
