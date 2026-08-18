import { useState } from 'react';
import { Search, Filter, X } from 'lucide-react';
import type { LeadStage } from '@/types/crm.types';
import { classNames } from '@/utils/classNames';

interface LeadFiltersProps {
  search: string;
  onSearchChange: (val: string) => void;
  stage: LeadStage | '';
  onStageChange: (val: LeadStage | '') => void;
  region: string;
  onRegionChange: (val: string) => void;
}

export function LeadFilters({
  search,
  onSearchChange,
  stage,
  onStageChange,
  region,
  onRegionChange,
}: LeadFiltersProps) {
  const [showFilters, setShowFilters] = useState(false);
  const hasActiveFilters = !!stage || !!region;

  const clearFilters = () => {
    onStageChange('');
    onRegionChange('');
    setShowFilters(false);
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-col sm:flex-row gap-3">
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-wms-muted" />
          <input
            type="text"
            placeholder="Search by company, contact, or region..."
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
              {(stage ? 1 : 0) + (region ? 1 : 0)}
            </span>
          )}
        </button>
      </div>

      {showFilters && (
        <div className="glass-card rounded-xl p-4 flex flex-wrap items-end gap-4 animate-fade-in">
          <div className="flex flex-col gap-1.5 min-w-[160px]">
            <label className="text-xs font-semibold text-wms-muted uppercase tracking-wide">Stage</label>
            <select
              value={stage}
              onChange={(e) => onStageChange(e.target.value as LeadStage | '')}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text appearance-none
                px-4 py-2.5 transition duration-200 outline-none cursor-pointer
                focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
            >
              <option value="" className="bg-wms-bg text-wms-muted">All Stages</option>
              <option value="NewLead" className="bg-wms-bg text-wms-text">New Lead</option>
              <option value="Qualified" className="bg-wms-bg text-wms-text">Qualified</option>
              <option value="Proposal" className="bg-wms-bg text-wms-text">Proposal</option>
              <option value="Negotiation" className="bg-wms-bg text-wms-text">Negotiation</option>
              <option value="ClosedWon" className="bg-wms-bg text-wms-text">Closed Won</option>
              <option value="ClosedLost" className="bg-wms-bg text-wms-text">Closed Lost</option>
            </select>
          </div>

          <div className="flex flex-col gap-1.5 min-w-[160px]">
            <label className="text-xs font-semibold text-wms-muted uppercase tracking-wide">Region</label>
            <input
              type="text"
              placeholder="e.g. North America"
              value={region}
              onChange={(e) => onRegionChange(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text
                px-4 py-2.5 transition duration-200 outline-none
                focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
            />
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
