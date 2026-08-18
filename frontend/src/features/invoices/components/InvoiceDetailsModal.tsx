import { Modal } from '@/components/ui/Modal/Modal';
import { Badge } from '@/components/ui/Badge/Badge';
import type { Invoice } from '@/types/invoice.types';

const STATUS_BADGE_MAP: Record<string, 'warning' | 'emerald' | 'danger' | 'info' | 'default'> = {
  Draft: 'default',
  Sent: 'info',
  Paid: 'emerald',
  Overdue: 'danger',
  Cancelled: 'warning',
};

interface InvoiceDetailsModalProps {
  isOpen: boolean;
  onClose: () => void;
  selectedInvoice: Invoice | null;
}

export function InvoiceDetailsModal({
  isOpen,
  onClose,
  selectedInvoice,
}: InvoiceDetailsModalProps) {
  if (!selectedInvoice) return null;

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Invoice details" size="md">
      <div className="space-y-6">
        <div className="flex justify-between items-start border-b border-wms-border pb-4">
          <div>
            <h3 className="text-lg font-bold text-wms-text">{selectedInvoice.invoiceNumber}</h3>
            <span className="text-xs text-wms-muted mt-1 block">Created on {new Date(selectedInvoice.createdAt).toLocaleDateString(undefined, { dateStyle: 'long' })}</span>
          </div>
          <Badge variant={STATUS_BADGE_MAP[selectedInvoice.status] || 'default'}>
            {selectedInvoice.status}
          </Badge>
        </div>

        <div className="grid grid-cols-2 gap-6 text-sm">
          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Company Client</span>
            <span className="text-wms-text font-semibold">{selectedInvoice.companyName}</span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Contact Person</span>
            <span className="text-wms-text">{selectedInvoice.contactName}</span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Amount Due</span>
            <span className="text-wms-text font-bold text-base text-wms-indigo">
              ${selectedInvoice.amount.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}
            </span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Issued Date</span>
            <span className="text-wms-text">{new Date(selectedInvoice.issuedDate).toLocaleDateString(undefined, { dateStyle: 'medium' })}</span>
          </div>

          <div>
            <span className="text-xs font-semibold text-wms-secondary uppercase tracking-wider block mb-1">Due Date</span>
            <span className="text-wms-text font-medium text-wms-danger">{new Date(selectedInvoice.dueDate).toLocaleDateString(undefined, { dateStyle: 'medium' })}</span>
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
