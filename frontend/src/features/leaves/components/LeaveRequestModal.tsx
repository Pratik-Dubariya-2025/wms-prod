import { useState } from 'react';
import { AlertCircle, CheckCircle2 } from 'lucide-react';
import type { LeaveType, CreateLeaveRequestCommand } from '@/types/leave.types';
import { Modal } from '@/components/ui/Modal/Modal';

interface LeaveRequestModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmitLeave: (data: CreateLeaveRequestCommand) => Promise<{ success: boolean; message?: string }>;
}

export function LeaveRequestModal({
  isOpen,
  onClose,
  onSubmitLeave,
}: LeaveRequestModalProps) {
  const [newLeaveType, setNewLeaveType] = useState<LeaveType>('Annual');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');
  const [daysCount, setDaysCount] = useState<number>(1);
  const [reason, setReason] = useState('');
  
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [submitSuccess, setSubmitSuccess] = useState<string | null>(null);
  const [submitLoading, setSubmitLoading] = useState(false);

  // Calculate days count automatically when dates change
  const handleDateChange = (from: string, to: string) => {
    if (!from || !to) return;
    const start = new Date(from);
    const end = new Date(to);
    if (start <= end) {
      const diffTime = Math.abs(end.getTime() - start.getTime());
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1;
      setDaysCount(diffDays);
    }
  };

  const handleRequestSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitError(null);
    setSubmitSuccess(null);

    if (!fromDate || !toDate) {
      setSubmitError('Please select both start and end dates.');
      return;
    }

    if (new Date(fromDate) > new Date(toDate)) {
      setSubmitError('Start date cannot be after the end date.');
      return;
    }

    if (daysCount <= 0) {
      setSubmitError('Days count must be greater than 0.');
      return;
    }

    if (!reason.trim()) {
      setSubmitError('Please provide a reason for the leave.');
      return;
    }

    setSubmitLoading(true);
    const result = await onSubmitLeave({
      leaveType: newLeaveType,
      fromDate,
      toDate,
      daysCount,
      reason,
    });

    setSubmitLoading(false);
    if (result.success) {
      setSubmitSuccess(result.message || 'Leave request submitted successfully.');
      setReason('');
      setFromDate('');
      setToDate('');
      setDaysCount(1);
      setTimeout(() => {
        onClose();
        setSubmitSuccess(null);
      }, 1500);
    } else {
      setSubmitError(result.message || 'Failed to submit leave request.');
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Apply For Leave" size="md">
      <form onSubmit={handleRequestSubmit} className="space-y-4">
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

        {/* Leave Type Select */}
        <div className="space-y-1.5">
          <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Leave Type</label>
          <select
            value={newLeaveType}
            onChange={(e) => setNewLeaveType(e.target.value as LeaveType)}
            className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
          >
            <option value="Annual">Annual</option>
            <option value="Sick">Sick</option>
            <option value="Casual">Casual</option>
            <option value="Maternity">Maternity</option>
            <option value="Paternity">Paternity</option>
            <option value="Unpaid">Unpaid</option>
          </select>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {/* From Date */}
          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Start Date</label>
            <input
              type="date"
              required
              value={fromDate}
              onChange={(e) => {
                setFromDate(e.target.value);
                handleDateChange(e.target.value, toDate);
              }}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
            />
          </div>

          {/* To Date */}
          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">End Date</label>
            <input
              type="date"
              required
              value={toDate}
              onChange={(e) => {
                setToDate(e.target.value);
                handleDateChange(fromDate, e.target.value);
              }}
              className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
            />
          </div>
        </div>

        {/* Days Count */}
        <div className="space-y-1.5">
          <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider flex justify-between">
            <span>Days Count</span>
            <span className="text-[10px] text-wms-muted normal-case font-normal">(allows half days, e.g. 0.5)</span>
          </label>
          <input
            type="number"
            step="0.5"
            min="0.5"
            required
            value={daysCount}
            onChange={(e) => setDaysCount(Number(e.target.value))}
            className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
          />
        </div>

        {/* Reason */}
        <div className="space-y-1.5">
          <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Reason</label>
          <textarea
            placeholder="Provide a detailed explanation for your leave..."
            required
            rows={3}
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo resize-none"
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
            {submitLoading ? 'Submitting...' : 'Submit Request'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
