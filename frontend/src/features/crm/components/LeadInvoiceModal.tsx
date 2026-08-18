import { useState, useEffect } from 'react';
import { AlertCircle, CheckCircle2 } from 'lucide-react';
import { Modal } from '@/components/ui/Modal/Modal';
import type { Lead } from '@/types/crm.types';
import type { CreateInvoiceCommand } from '@/types/invoice.types';

interface LeadInvoiceModalProps {
  isOpen: boolean;
  onClose: () => void;
  selectedLead: Lead | null;
  onGenerateInvoice: (data: CreateInvoiceCommand) => Promise<{ success: boolean; message?: string }>;
}

export function LeadInvoiceModal({
  isOpen,
  onClose,
  selectedLead,
  onGenerateInvoice,
}: LeadInvoiceModalProps) {
  const [amount, setAmount] = useState<number | ''>('');
  const [dueDays, setDueDays] = useState<number>(30);

  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState<string | null>(null);
  const [submitLoading, setSubmitLoading] = useState(false);

  useEffect(() => {
    if (selectedLead) {
      setAmount(selectedLead.estimatedValue || '');
      setDueDays(30);
    }
  }, [selectedLead]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedLead) return;

    setSubmitError(null);
    setSubmitSuccess(null);

    const val = Number(amount);
    if (isNaN(val) || val <= 0) {
      setSubmitError('Invoice amount must be greater than 0.');
      return;
    }

    setSubmitLoading(true);
    const result = await onGenerateInvoice({
      leadId: selectedLead.id,
      amount: val,
      dueDays: dueDays,
    });

    setSubmitLoading(false);
    if (result.success) {
      setSubmitSuccess(result.message || 'Invoice generated successfully.');
      setTimeout(() => {
        onClose();
        setSubmitSuccess(null);
      }, 1500);
    } else {
      setSubmitError(result.message || 'Failed to generate invoice.');
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Generate Financial Invoice" size="md">
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
          <div className="text-sm border-b border-wms-border pb-3 grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <span className="text-wms-muted block">Company</span>
              <span className="font-semibold text-wms-text">{selectedLead.companyName}</span>
            </div>
            <div>
              <span className="text-wms-muted block">Estimated Value</span>
              <span className="font-bold text-wms-text">${selectedLead.estimatedValue?.toLocaleString() || '—'}</span>
            </div>
          </div>
        )}

        <div className="space-y-1.5">
          <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Invoice Amount ($) *</label>
          <input
            type="number"
            min="1"
            required
            value={amount}
            onChange={(e) => setAmount(e.target.value !== '' ? Number(e.target.value) : '')}
            className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
          />
        </div>

        <div className="space-y-1.5">
          <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Payment Due Days *</label>
          <input
            type="number"
            min="1"
            required
            value={dueDays}
            onChange={(e) => setDueDays(Number(e.target.value))}
            className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
          />
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
            {submitLoading ? 'Generating...' : 'Generate Invoice'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
