import { useState } from 'react';
import { Search, Filter, X } from 'lucide-react';
import { classNames } from '@/utils/classNames';

interface InvoiceFiltersProps {
  search: string;
  onSearchChange: (val: string) => void;
  status: string;
  onStatusChange: (val: string) => void;
}

export function InvoiceFilters({
  search,
  onSearchChange,
  status,
  onStatusChange,
}: InvoiceFiltersProps) {
  const [showFilters, setShowFilters] = useState(false);
  const hasActiveFilters = !!status;

  const clearFilters = () => {
    onStatusChange('');
    setShowFilters(false);
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-wms-muted" />
          <input
            type="text"
            placeholder="Search by invoice number, company, or contact..."
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
              1
            </span>
          )}
        </button>
      </div>

      {showFilters && (
        <div className="glass-card rounded-xl p-4 flex flex-wrap items-end gap-4 animate-fade-in">
          <div className="flex flex-col gap-1.5 min-w-[160px]">
            <label className="text-xs font-semibold text-wms-muted uppercase tracking-wide">Status</label>
            <select
              value={status}
              onChange={(e) => onStatusChange(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text appearance-none
                px-4 py-2.5 transition duration-200 outline-none cursor-pointer
                focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
            >
              <option value="" className="bg-wms-bg text-wms-muted">All Statuses</option>
              <option value="Draft" className="bg-wms-bg text-wms-text">Draft</option>
              <option value="Sent" className="bg-wms-bg text-wms-text">Sent</option>
              <option value="Paid" className="bg-wms-bg text-wms-text">Paid</option>
              <option value="Overdue" className="bg-wms-bg text-wms-text">Overdue</option>
              <option value="Cancelled" className="bg-wms-bg text-wms-text">Cancelled</option>
            </select>
          </div>

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
