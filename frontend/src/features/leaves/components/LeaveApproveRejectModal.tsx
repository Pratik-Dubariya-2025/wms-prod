import { useState, useEffect } from 'react';
import { AlertCircle } from 'lucide-react';
import type { LeaveRequest } from '@/types/leave.types';
import { Modal } from '@/components/ui/Modal/Modal';
import { classNames } from '@/utils/classNames';

interface LeaveApproveRejectModalProps {
  isOpen: boolean;
  onClose: () => void;
  selectedLeave: LeaveRequest | null;
  action: 'Approved' | 'Rejected' | null;
  onApproveLeave: (id: string) => Promise<{ success: boolean; message?: string }>;
  onRejectLeave: (id: string, rejectionReason: string) => Promise<{ success: boolean; message?: string }>;
  onSuccess: () => void;
}

export function LeaveApproveRejectModal({
  isOpen,
  onClose,
  selectedLeave,
  action,
  onApproveLeave,
  onRejectLeave,
  onSuccess,
}: LeaveApproveRejectModalProps) {
  const [rejectionReason, setRejectionReason] = useState('');
  const [actionLoading, setActionLoading] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  // Clear fields when modal state changes
  useEffect(() => {
    if (isOpen) {
      setRejectionReason('');
      setActionError(null);
      setActionLoading(false);
    }
  }, [isOpen]);

  const handleApproveRejectSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedLeave || !action) return;

    if (action === 'Rejected' && !rejectionReason.trim()) {
      setActionError('Rejection reason is required.');
      return;
    }

    setActionLoading(true);
    setActionError(null);

    const result =
      action === 'Rejected'
        ? await onRejectLeave(selectedLeave.id, rejectionReason.trim())
        : await onApproveLeave(selectedLeave.id);

    setActionLoading(false);
    if (result.success) {
      onClose();
      onSuccess();
    } else {
      setActionError(result.message || 'Failed to update leave request.');
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={action === 'Approved' ? 'Approve Leave Request' : 'Reject Leave Request'}
      size="md"
    >
      <form onSubmit={handleApproveRejectSubmit} className="space-y-4">
        {actionError && (
          <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 text-wms-danger text-sm rounded-lg flex items-center gap-2">
            <AlertCircle className="h-4 w-4" />
            {actionError}
          </div>
        )}

        {selectedLeave && (
          <div className="p-3 rounded-lg bg-wms-hover border border-wms-border space-y-1.5 text-xs text-wms-text">
            <div>
              <span className="text-wms-secondary font-semibold">Employee:</span>{' '}
              {selectedLeave.userName} ({selectedLeave.employeeCode})
            </div>
            <div>
              <span className="text-wms-secondary font-semibold">Type:</span>{' '}
              {selectedLeave.leaveType}
            </div>
            <div>
              <span className="text-wms-secondary font-semibold">Period:</span>{' '}
              {new Date(selectedLeave.fromDate).toLocaleDateString()} to{' '}
              {new Date(selectedLeave.toDate).toLocaleDateString()} ({selectedLeave.daysCount} days)
            </div>
            <div>
              <span className="text-wms-secondary font-semibold">Reason:</span>{' '}
              {selectedLeave.reason}
            </div>
          </div>
        )}

        {action === 'Rejected' ? (
          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">
              Rejection Reason
            </label>
            <textarea
              placeholder="Explain why this leave request is being rejected..."
              required
              rows={3}
              value={rejectionReason}
              onChange={(e) => setRejectionReason(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo resize-none"
            />
          </div>
        ) : (
          <p className="text-sm text-wms-secondary">
            Are you sure you want to approve this leave request? This will mark the employee as on leave for the
            specified duration.
          </p>
        )}

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
            disabled={actionLoading}
            className={classNames(
              'px-5 py-2 rounded-lg text-white text-sm font-semibold transition cursor-pointer disabled:opacity-50',
              action === 'Approved' ? 'bg-wms-emerald hover:bg-emerald-600' : 'bg-wms-danger hover:bg-red-600'
            )}
          >
            {actionLoading ? 'Saving...' : action === 'Approved' ? 'Approve' : 'Reject'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
