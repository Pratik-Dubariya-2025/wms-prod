import { useState, useEffect } from 'react';
import { AlertCircle, CheckCircle2 } from 'lucide-react';
import { Modal } from '@/components/ui/Modal/Modal';
import type { Lead, LeadStage } from '@/types/crm.types';

interface LeadStageModalProps {
  isOpen: boolean;
  onClose: () => void;
  selectedLead: Lead | null;
  onUpdateStage: (id: string, stage: LeadStage) => Promise<{ success: boolean; message?: string }>;
  onSuccess: () => void;
}

export function LeadStageModal({
  isOpen,
  onClose,
  selectedLead,
  onUpdateStage,
  onSuccess,
}: LeadStageModalProps) {
  const [stage, setStage] = useState<LeadStage>('NewLead');
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState<string | null>(null);
  const [submitLoading, setSubmitLoading] = useState(false);

  useEffect(() => {
    if (selectedLead) {
      setStage(selectedLead.stage);
    }
  }, [selectedLead]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedLead) return;

    setSubmitError(null);
    setSubmitSuccess(null);
    setSubmitLoading(true);

    const result = await onUpdateStage(selectedLead.id, stage);

    setSubmitLoading(false);
    if (result.success) {
      setSubmitSuccess(result.message || 'Lead stage updated successfully.');
      setTimeout(() => {
        onSuccess();
        onClose();
        setSubmitSuccess(null);
      }, 1200);
    } else {
      setSubmitError(result.message || 'Failed to update lead stage.');
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Update Lead Stage" size="sm">
      <form onSubmit={handleSubmit} className="space-y-4">
        {submitError && (
          <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 text-wms-danger text-sm rounded-lg flex items-center gap-2">
            <AlertCircle className="h-4 w-4" />
            {submitError}
          </div>
        )}
        {submitSuccess && (
          <div className="p-3 bg-wms-emerald/10 border border-wms-emerald/20 text-wms-emerald text-sm rounded-lg flex items-center gap-2">
            <CheckCircle2 className="h-4 w-4" />
            {submitSuccess}
          </div>
        )}

        {selectedLead && (
          <div className="text-sm border-b border-wms-border pb-3">
            <span className="text-wms-muted block">Company</span>
            <span className="font-semibold text-wms-text">{selectedLead.companyName}</span>
          </div>
        )}

        <div className="space-y-1.5">
          <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Pipeline Stage</label>
          <select
            value={stage}
            onChange={(e) => setStage(e.target.value as LeadStage)}
            className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo cursor-pointer"
          >
            <option value="NewLead">New Lead</option>
            <option value="Qualified">Qualified</option>
            <option value="Proposal">Proposal</option>
            <option value="Negotiation">Negotiation</option>
            <option value="ClosedWon">Closed Won</option>
            <option value="ClosedLost">Closed Lost</option>
          </select>
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 rounded-lg bg-wms-hover border border-wms-border text-sm font-semibold text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition cursor-pointer"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={submitLoading}
            className="px-5 py-2 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition cursor-pointer disabled:opacity-50"
          >
            {submitLoading ? 'Saving...' : 'Update Stage'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
