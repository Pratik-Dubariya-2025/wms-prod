import { CheckCircle2, XCircle } from 'lucide-react';
import type { LeaveRequest, LeaveStatus } from '@/types/leave.types';
import { Modal } from '@/components/ui/Modal/Modal';
import { Avatar } from '@/components/ui/Avatar/Avatar';
import { Badge } from '@/components/ui/Badge/Badge';

const STATUS_BADGE_MAP: Record<LeaveStatus, 'warning' | 'emerald' | 'danger' | 'default'> = {
  Pending: 'warning',
  Approved: 'emerald',
  Rejected: 'danger',
  Cancelled: 'default',
};

interface LeaveDetailsModalProps {
  isOpen: boolean;
  onClose: () => void;
  selectedLeave: LeaveRequest | null;
}

export function LeaveDetailsModal({
  isOpen,
  onClose,
  selectedLeave,
}: LeaveDetailsModalProps) {
  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Leave Request Details" size="md">
      {selectedLeave && (
        <div className="space-y-4">
          {/* Header Employee details */}
          <div className="flex items-center gap-3 pb-3 border-b border-wms-border">
            <Avatar name={selectedLeave.userName} size="md" />
            <div>
              <span className="text-sm font-bold text-wms-text block">{selectedLeave.userName}</span>
              <span className="text-xs text-wms-muted block">{selectedLeave.employeeCode}</span>
            </div>
            <Badge variant={STATUS_BADGE_MAP[selectedLeave.status]} className="ml-auto animate-fade-in">
              {selectedLeave.status}
            </Badge>
          </div>

          {/* Details Body */}
          <div className="space-y-3 text-sm">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <span className="text-xs text-wms-secondary uppercase tracking-wider block font-semibold">Leave Type</span>
                <span className="text-sm font-semibold text-wms-text">{selectedLeave.leaveType}</span>
              </div>
              <div>
                <span className="text-xs text-wms-secondary uppercase tracking-wider block font-semibold">Total Days</span>
                <span className="text-sm font-semibold text-wms-text">{selectedLeave.daysCount} days</span>
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <span className="text-xs text-wms-secondary uppercase tracking-wider block font-semibold">Start Date</span>
                <span className="text-sm font-semibold text-wms-text">
                  {new Date(selectedLeave.fromDate).toLocaleDateString()}
                </span>
              </div>
              <div>
                <span className="text-xs text-wms-secondary uppercase tracking-wider block font-semibold">End Date</span>
                <span className="text-sm font-semibold text-wms-text">
                  {new Date(selectedLeave.toDate).toLocaleDateString()}
                </span>
              </div>
            </div>

            <div>
              <span className="text-xs text-wms-secondary uppercase tracking-wider block font-semibold">Reason for Leave</span>
              <div className="p-3 rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text mt-1 whitespace-pre-wrap">
                {selectedLeave.reason || 'No reason provided.'}
              </div>
            </div>

            {/* Approval/Rejection Audits */}
            {selectedLeave.status !== 'Pending' && selectedLeave.approvedByName && (
              <div className="pt-3 border-t border-wms-border space-y-3">
                <div className="flex items-center gap-2 text-wms-secondary">
                  {selectedLeave.status === 'Approved' ? (
                    <CheckCircle2 className="h-4 w-4 text-wms-emerald" />
                  ) : (
                    <XCircle className="h-4 w-4 text-wms-danger" />
                  )}
                  <span className="text-xs font-semibold uppercase tracking-wider">
                    {selectedLeave.status === 'Approved' ? 'Approval' : 'Rejection'} Info
                  </span>
                </div>

                <div className="grid grid-cols-2 gap-4 text-xs">
                  <div>
                    <span className="text-wms-secondary font-semibold block">
                      {selectedLeave.status === 'Approved' ? 'Approved By' : 'Rejected By'}
                    </span>
                    <span className="text-wms-text font-medium">{selectedLeave.approvedByName}</span>
                  </div>
                  <div>
                    <span className="text-wms-secondary font-semibold block">Action Date</span>
                    <span className="text-wms-text font-medium">
                      {selectedLeave.approvedAt ? new Date(selectedLeave.approvedAt).toLocaleString() : '-'}
                    </span>
                  </div>
                </div>

                {selectedLeave.status === 'Rejected' && selectedLeave.rejectionReason && (
                  <div>
                    <span className="text-xs text-wms-secondary uppercase tracking-wider block font-semibold">
                      Rejection Reason
                    </span>
                    <div className="p-3 rounded-lg bg-wms-danger/5 border border-wms-danger/10 text-sm text-wms-text mt-1 whitespace-pre-wrap">
                      {selectedLeave.rejectionReason}
                    </div>
                  </div>
                )}
              </div>
            )}
          </div>

          {/* Footer actions */}
          <div className="flex justify-end pt-3 border-t border-wms-border">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 rounded-lg bg-wms-hover border border-wms-border text-sm font-semibold text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition cursor-pointer"
            >
              Close
            </button>
          </div>
        </div>
      )}
    </Modal>
  );
}
