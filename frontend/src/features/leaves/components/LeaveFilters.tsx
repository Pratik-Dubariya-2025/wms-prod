import { useState } from 'react';
import { Search, Filter, X } from 'lucide-react';
import type { LeaveStatus, LeaveType } from '@/types/leave.types';
import { classNames } from '@/utils/classNames';

interface LeaveFiltersProps {
  search: string;
  onSearchChange: (val: string) => void;
  status: LeaveStatus | '';
  onStatusChange: (val: LeaveStatus | '') => void;
  leaveType: LeaveType | '';
  onLeaveTypeChange: (val: LeaveType | '') => void;
}

export function LeaveFilters({
  search,
  onSearchChange,
  status,
  onStatusChange,
  leaveType,
  onLeaveTypeChange,
}: LeaveFiltersProps) {
  const [showFilters, setShowFilters] = useState(false);
  const hasActiveFilters = !!status || !!leaveType;

  const clearFilters = () => {
    onStatusChange('');
    onLeaveTypeChange('');
    setShowFilters(false);
  };

  return (
    <div className="space-y-4">
      {/* Search & Filter Toggles */}
      <div className="flex flex-col sm:flex-row gap-3">
        {/* Search Input */}
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-wms-muted" />
          <input
            type="text"
            placeholder="Search by employee name or code..."
            value={search}
            onChange={(e) => onSearchChange(e.target.value)}
            className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text placeholder:text-wms-muted
              pl-10 pr-4 py-2.5 transition duration-200 outline-none
              focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20 hover:border-wms-border"
          />
          {search && (
            <button
              onClick={() => onSearchChange('')}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-wms-muted hover:text-wms-text transition cursor-pointer"
            >
              <X className="h-4 w-4" />
            </button>
          )}
        </div>

        {/* Filter Toggle */}
        <button
          onClick={() => setShowFilters(!showFilters)}
          className={classNames(
            'inline-flex items-center gap-2 px-4 py-2.5 rounded-lg border text-sm font-medium transition duration-200 cursor-pointer select-none',
            hasActiveFilters
              ? 'bg-wms-indigo/10 border-wms-indigo/30 text-wms-indigo'
              : 'bg-wms-hover border-wms-border text-wms-secondary hover:text-wms-text hover:bg-wms-hover',
          )}
        >
          <Filter className="h-4 w-4" />
          Filters
          {hasActiveFilters && (
            <span className="ml-1 h-5 w-5 rounded-full bg-wms-indigo text-white text-xs flex items-center justify-center">
              {(status ? 1 : 0) + (leaveType ? 1 : 0)}
            </span>
          )}
        </button>
      </div>

      {/* Collapsible Filters Panel */}
      {showFilters && (
        <div className="glass-card rounded-xl p-4 flex flex-wrap items-end gap-4 animate-fade-in">
          {/* Status Select */}
          <div className="flex flex-col gap-1.5 min-w-[160px]">
            <label className="text-xs font-semibold text-wms-muted uppercase tracking-wide">Status</label>
            <select
              value={status}
              onChange={(e) => onStatusChange(e.target.value as LeaveStatus | '')}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text appearance-none
                px-4 py-2.5 transition duration-200 outline-none cursor-pointer
                focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
            >
              <option value="" className="bg-wms-bg text-wms-muted">All Statuses</option>
              <option value="Pending" className="bg-wms-bg text-wms-text">Pending</option>
              <option value="Approved" className="bg-wms-bg text-wms-text">Approved</option>
              <option value="Rejected" className="bg-wms-bg text-wms-text">Rejected</option>
              <option value="Cancelled" className="bg-wms-bg text-wms-text">Cancelled</option>
            </select>
          </div>

          {/* Leave Type Select */}
          <div className="flex flex-col gap-1.5 min-w-[160px]">
            <label className="text-xs font-semibold text-wms-muted uppercase tracking-wide">Leave Type</label>
            <select
              value={leaveType}
              onChange={(e) => onLeaveTypeChange(e.target.value as LeaveType | '')}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text appearance-none
                px-4 py-2.5 transition duration-200 outline-none cursor-pointer
                focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
            >
              <option value="" className="bg-wms-bg text-wms-muted">All Types</option>
              <option value="Annual" className="bg-wms-bg text-wms-text">Annual</option>
              <option value="Sick" className="bg-wms-bg text-wms-text">Sick</option>
              <option value="Casual" className="bg-wms-bg text-wms-text">Casual</option>
              <option value="Maternity" className="bg-wms-bg text-wms-text">Maternity</option>
              <option value="Paternity" className="bg-wms-bg text-wms-text">Paternity</option>
              <option value="Unpaid" className="bg-wms-bg text-wms-text">Unpaid</option>
            </select>
          </div>

          {/* Clear Filters Button */}
          {hasActiveFilters && (
            <button
              onClick={clearFilters}
              className="inline-flex items-center gap-1.5 px-3 py-2.5 text-sm text-wms-danger hover:text-red-400 transition cursor-pointer"
            >
              <X className="h-3.5 w-3.5" />
              Clear all
            </button>
          )}
        </div>
      )}
    </div>
  );
}
