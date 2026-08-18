import { useState } from 'react';
import { Calendar, RefreshCw, Plus } from 'lucide-react';

import { useLeaves } from '@/features/leaves/hooks/useLeaves';
import type { LeaveRequest } from '@/types/leave.types';
import { LeaveFilters } from '../components/LeaveFilters';
import { LeaveTable } from '../components/LeaveTable';
import { LeaveRequestModal } from '../components/LeaveRequestModal';
import { LeaveApproveRejectModal } from '../components/LeaveApproveRejectModal';
import { LeaveDetailsModal } from '../components/LeaveDetailsModal';

import { useModal } from '@/components/ui/Modal/useModal';
import { classNames } from '@/utils/classNames';

interface LeavesPageProps {
  approvalsOnly?: boolean;
  teamOnly?: boolean;
}

export default function LeavesPage({ approvalsOnly = false, teamOnly = false }: LeavesPageProps) {
  const {
    leaves,
    pagination,
    isLoading,
    error,
    params,
    setSearch,
    setStatus,
    setLeaveType,
    setPage,
    setPageSize,
    refresh,
    submitLeave,
    approveLeave,
    rejectLeave,
  } = useLeaves(10, approvalsOnly, teamOnly);

  // "My Leaves" lets you apply for leave; the other two are read-only views into other people's requests.
  const isReadOnlyView = approvalsOnly || teamOnly;

  // Modals state control
  const requestModal = useModal();
  const approveRejectModal = useModal();
  const detailsModal = useModal();

  // Selected item state for details and approval reviews
  const [selectedLeave, setSelectedLeave] = useState<LeaveRequest | null>(null);
  const [approvalAction, setApprovalAction] = useState<'Approved' | 'Rejected' | null>(null);

  const handleOpenApproveReject = (leave: LeaveRequest, action: 'Approved' | 'Rejected') => {
    setSelectedLeave(leave);
    setApprovalAction(action);
    approveRejectModal.open();
  };

  const handleOpenDetails = (leave: LeaveRequest) => {
    setSelectedLeave(leave);
    detailsModal.open();
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* ─── Page Header ─── */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-wms-border pb-5">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-xl bg-gradient-to-br from-wms-indigo to-wms-cyan flex items-center justify-center shadow-lg shadow-wms-indigo/20">
            <Calendar className="h-5 w-5 text-wms-text" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-wms-text tracking-tight">
              {approvalsOnly ? 'Leave Approvals' : teamOnly ? 'Team Leaves' : 'My Leaves'}
            </h1>
            <p className="text-sm text-wms-secondary mt-0.5">
              {approvalsOnly ? (
                'Review and manage team leave requests'
              ) : teamOnly ? (
                "Leave requests visible to you based on your team's access policy"
              ) : (
                pagination ? (
                  <>
                    Track and manage <span className="text-wms-indigo font-semibold">{pagination.totalCount}</span> leave applications
                  </>
                ) : (
                  'Submit and track your leave applications'
                )
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
          {!isReadOnlyView && (
            <button
              onClick={requestModal.open}
              className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-indigo/25 select-none"
            >
              <Plus className="h-4 w-4" />
              Apply Leave
            </button>
          )}
        </div>
      </div>

      {/* ─── Search & Filters Bar ─── */}
      <LeaveFilters
        search={params.search || ''}
        onSearchChange={setSearch}
        status={params.status || ''}
        onStatusChange={setStatus}
        leaveType={params.leaveType || ''}
        onLeaveTypeChange={setLeaveType}
      />

      {/* ─── Error Notification ─── */}
      {error && (
        <div className="glass-card rounded-xl p-6 border-wms-danger/20 flex items-center gap-3">
          <div className="h-8 w-8 rounded-lg bg-wms-danger/15 flex items-center justify-center flex-shrink-0">
            <span className="text-wms-danger font-semibold">!</span>
          </div>
          <div>
            <p className="text-sm font-medium text-wms-danger">Failed to load leave requests</p>
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

      <LeaveTable
        leaves={leaves}
        isLoading={isLoading}
        onViewDetails={handleOpenDetails}
        onApproveReject={handleOpenApproveReject}
        pageSize={params.pageSize}
        pagination={pagination}
        onPageChange={setPage}
        onPageSizeChange={setPageSize}
        hasActiveFilters={!!params.status || !!params.leaveType}
        searchQuery={params.search || ''}
        isApprovalTable={approvalsOnly}
      />

      {/* ─── MODAL: APPLY LEAVE REQUEST ─── */}
      <LeaveRequestModal
        isOpen={requestModal.isOpen}
        onClose={requestModal.close}
        onSubmitLeave={submitLeave}
      />

      {/* ─── MODAL: APPROVE / REJECT LEAVE ─── */}
      <LeaveApproveRejectModal
        isOpen={approveRejectModal.isOpen}
        onClose={approveRejectModal.close}
        selectedLeave={selectedLeave}
        action={approvalAction}
        onApproveLeave={approveLeave}
        onRejectLeave={rejectLeave}
        onSuccess={refresh}
      />

      {/* ─── MODAL: LEAVE DETAILS ─── */}
      <LeaveDetailsModal
        isOpen={detailsModal.isOpen}
        onClose={detailsModal.close}
        selectedLeave={selectedLeave}
      />
    </div>
  );
}
