import { useState } from 'react';
import { AlertCircle, CheckCircle2 } from 'lucide-react';
import { Modal } from '@/components/ui/Modal/Modal';
import type { CreateLeadCommand } from '@/types/crm.types';

interface LeadRequestModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmitLead: (data: CreateLeadCommand) => Promise<{ success: boolean; message?: string }>;
}

export function LeadRequestModal({
  isOpen,
  onClose,
  onSubmitLead,
}: LeadRequestModalProps) {
  const [companyName, setCompanyName] = useState('');
  const [contactName, setContactName] = useState('');
  const [contactEmail, setContactEmail] = useState('');
  const [contactPhone, setContactPhone] = useState('');
  const [estimatedValue, setEstimatedValue] = useState<number | ''>('');
  const [region, setRegion] = useState('');
  const [source, setSource] = useState('');

  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState<string | null>(null);
  const [submitLoading, setSubmitLoading] = useState(false);

  const handleRequestSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);
    setSubmitSuccess(null);

    if (!companyName.trim()) {
      setSubmitError('Company name is required.');
      return;
    }

    if (!contactName.trim()) {
      setSubmitError('Contact person name is required.');
      return;
    }

    setSubmitLoading(true);
    const result = await onSubmitLead({
      companyName,
      contactName,
      contactEmail: contactEmail.trim() || undefined,
      contactPhone: contactPhone.trim() || undefined,
      estimatedValue: estimatedValue !== '' ? Number(estimatedValue) : undefined,
      region: region.trim() || undefined,
      source: source.trim() || undefined,
    });

    setSubmitLoading(false);
    if (result.success) {
      setSubmitSuccess(result.message || 'Lead created successfully.');
      setCompanyName('');
      setContactName('');
      setContactEmail('');
      setContactPhone('');
      setEstimatedValue('');
      setRegion('');
      setSource('');
      setTimeout(() => {
        onClose();
        setSubmitSuccess(null);
      }, 1500);
    } else {
      setSubmitError(result.message || 'Failed to create lead.');
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Create CRM Lead" size="lg">
      <form onSubmit={handleRequestSubmit} className="space-y-4">
        {submitError && (
          <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 text-wms-danger text-sm rounded-lg flex items-center gap-2">
            <AlertCircle className="h-4 w-4 shrink-0" />
            {submitError}
          </div>
        )}
        {submitSuccess && (
          <div className="p-3 bg-wms-emerald/10 border border-wms-emerald/20 text-wms-emerald text-sm rounded-lg flex items-center gap-2">
            <CheckCircle2 className="h-4 w-4 shrink-0" />
            {submitSuccess}
          </div>
        )}

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block">Company Name *</label>
            <input
              type="text"
              required
              placeholder="e.g. Acme Corporation"
              value={companyName}
              onChange={(e) => setCompanyName(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block">Contact Person *</label>
            <input
              type="text"
              required
              placeholder="e.g. John Doe"
              value={contactName}
              onChange={(e) => setContactName(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block">Contact Email</label>
            <input
              type="email"
              placeholder="e.g. john@acme.com"
              value={contactEmail}
              onChange={(e) => setContactEmail(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block">Contact Phone</label>
            <input
              type="text"
              placeholder="e.g. +1 555-0199"
              value={contactPhone}
              onChange={(e) => setContactPhone(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
            />
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block whitespace-nowrap overflow-hidden text-ellipsis" title="Estimated Value ($)">
              Est. Value ($)
            </label>
            <input
              type="number"
              min="0"
              placeholder="e.g. 50000"
              value={estimatedValue}
              onChange={(e) => setEstimatedValue(e.target.value !== '' ? Number(e.target.value) : '')}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block">Region</label>
            <input
              type="text"
              placeholder="e.g. EMEA"
              value={region}
              onChange={(e) => setRegion(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
            />
          </div>

          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block">Source</label>
            <input
              type="text"
              placeholder="e.g. Website"
              value={source}
              onChange={(e) => setSource(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
            />
          </div>
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
            {submitLoading ? 'Creating...' : 'Create Lead'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
