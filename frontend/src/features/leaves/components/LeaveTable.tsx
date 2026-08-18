import { Calendar, Check, Ban } from 'lucide-react';
import type { LeaveRequest, LeaveStatus, LeaveType } from '@/types/leave.types';
import { Avatar } from '@/components/ui/Avatar/Avatar';
import { Badge } from '@/components/ui/Badge/Badge';
import { PermissionGate } from '@/components/shared/PermissionGate';
import { PERMISSIONS } from '@/constants/permissions';
import { Pagination } from '@/components/ui/Pagination/Pagination';
import { classNames } from '@/utils/classNames';

const STATUS_BADGE_MAP: Record<LeaveStatus, 'warning' | 'emerald' | 'danger' | 'default'> = {
  Pending: 'warning',
  Approved: 'emerald',
  Rejected: 'danger',
  Cancelled: 'default',
};

const TYPE_COLOR_MAP: Record<LeaveType, string> = {
  Annual: 'text-blue-500 bg-blue-500/10 border-blue-500/20',
  Sick: 'text-red-500 bg-red-500/10 border-red-500/20',
  Casual: 'text-purple-500 bg-purple-500/10 border-purple-500/20',
  Maternity: 'text-pink-500 bg-pink-500/10 border-pink-500/20',
  Paternity: 'text-indigo-500 bg-indigo-500/10 border-indigo-500/20',
  Unpaid: 'text-amber-500 bg-amber-500/10 border-amber-500/20',
};

interface LeaveTableProps {
  leaves: LeaveRequest[];
  isLoading: boolean;
  onViewDetails: (leave: LeaveRequest) => void;
  onApproveReject: (leave: LeaveRequest, action: 'Approved' | 'Rejected') => void;
  pageSize: number;
  pagination: any;
  onPageChange: (page: number) => void;
  onPageSizeChange: (size: number) => void;
  hasActiveFilters: boolean;
  searchQuery: string;
  isApprovalTable?: boolean;
}

export function LeaveTable({
  leaves,
  isLoading,
  onViewDetails,
  onApproveReject,
  pageSize,
  pagination,
  onPageChange,
  onPageSizeChange,
  hasActiveFilters,
  searchQuery,
  isApprovalTable = false,
}: LeaveTableProps) {
  return (
    <div className="glass-card rounded-xl overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b border-wms-border">
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Employee</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Leave Type</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Duration</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Days</th>
              <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Status</th>
              <th className="px-5 py-3.5 text-right text-xs font-semibold text-wms-muted uppercase tracking-wide">Actions</th>
            </tr>
          </thead>
          <tbody>
            {isLoading ? (
              Array.from({ length: pageSize }).map((_, i) => (
                <tr key={`skeleton-${i}`} className="border-b border-wms-border">
                  <td className="px-5 py-4">
                    <div className="flex items-center gap-3">
                      <div className="h-10 w-10 rounded-full bg-wms-hover animate-pulse" />
                      <div className="space-y-2">
                        <div className="h-3.5 w-32 bg-wms-hover rounded animate-pulse" />
                        <div className="h-3 w-16 bg-wms-hover rounded animate-pulse" />
                      </div>
                    </div>
                  </td>
                  <td className="px-5 py-4"><div className="h-6 w-20 bg-wms-hover rounded animate-pulse" /></td>
                  <td className="px-5 py-4"><div className="h-3.5 w-24 bg-wms-hover rounded animate-pulse" /></td>
                  <td className="px-5 py-4"><div className="h-3.5 w-8 bg-wms-hover rounded animate-pulse" /></td>
                  <td className="px-5 py-4"><div className="h-5 w-14 bg-wms-hover rounded-full animate-pulse" /></td>
                  <td className="px-5 py-4 text-right"><div className="h-8 w-16 bg-wms-hover rounded ml-auto animate-pulse" /></td>
                </tr>
              ))
            ) : leaves.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-5 py-16 text-center">
                  <div className="flex flex-col items-center gap-3">
                    <div className="h-14 w-14 rounded-2xl bg-wms-hover flex items-center justify-center">
                      <Calendar className="h-7 w-7 text-wms-muted" />
                    </div>
                    <div>
                      <p className="text-sm font-medium text-wms-secondary">No leave requests found</p>
                      <p className="text-xs text-wms-muted mt-1">
                        {searchQuery || hasActiveFilters
                          ? 'Try adjusting your search or filters'
                          : 'No leave requests have been submitted yet'}
                      </p>
                    </div>
                  </div>
                </td>
              </tr>
            ) : (
              leaves.map((leave) => (
                <tr
                  key={leave.id}
                  className="border-b border-wms-border last:border-b-0 transition duration-150 hover:bg-wms-hover group"
                >
                  {/* Employee Info */}
                  <td className="px-5 py-4">
                    <div className="flex items-center gap-3">
                      <Avatar name={leave.userName} size="md" />
                      <div className="min-w-0">
                        <span className="text-sm font-semibold text-wms-text truncate block">
                          {leave.userName}
                        </span>
                        <span className="text-xs text-wms-muted block">{leave.employeeCode}</span>
                      </div>
                    </div>
                  </td>

                  {/* Leave Type */}
                  <td className="px-5 py-4">
                    <span className={classNames(
                      'px-2.5 py-1 rounded-md text-xs font-semibold border',
                      TYPE_COLOR_MAP[leave.leaveType] || 'text-wms-text bg-wms-hover border-wms-border'
                    )}>
                      {leave.leaveType}
                    </span>
                  </td>

                  {/* Duration Dates */}
                  <td className="px-5 py-4">
                    <div className="text-xs font-semibold text-wms-text">
                      {new Date(leave.fromDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
                      <span className="text-wms-muted font-normal mx-1">to</span>
                      {new Date(leave.toDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })}
                    </div>
                  </td>

                  {/* Days Count */}
                  <td className="px-5 py-4">
                    <span className="text-sm font-bold text-wms-text">{leave.daysCount}</span>
                  </td>

                  {/* Status */}
                  <td className="px-5 py-4">
                    <Badge variant={STATUS_BADGE_MAP[leave.status] || 'default'}>
                      {leave.status}
                    </Badge>
                  </td>

                  {/* Actions */}
                  <td className="px-5 py-4 text-right">
                    <div className="flex items-center justify-end gap-1.5">
                      <button
                        onClick={() => onViewDetails(leave)}
                        className="px-2.5 py-1.5 rounded-lg border border-wms-border bg-wms-hover hover:text-wms-indigo text-wms-secondary text-xs font-semibold transition cursor-pointer"
                      >
                        Details
                      </button>
                      
                      {isApprovalTable && leave.status === 'Pending' && (
                        <PermissionGate permissions={PERMISSIONS.LEAVE_APPROVE}>
                          <button
                            onClick={() => onApproveReject(leave, 'Approved')}
                            className="p-1.5 rounded-lg bg-wms-emerald/10 hover:bg-wms-emerald/20 text-wms-emerald transition cursor-pointer"
                            title="Approve Request"
                          >
                            <Check className="h-4 w-4" />
                          </button>
                          <button
                            onClick={() => onApproveReject(leave, 'Rejected')}
                            className="p-1.5 rounded-lg bg-wms-danger/10 hover:bg-wms-danger/20 text-wms-danger transition cursor-pointer"
                            title="Reject Request"
                          >
                            <Ban className="h-4 w-4" />
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

      {/* Pagination Footer */}
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
