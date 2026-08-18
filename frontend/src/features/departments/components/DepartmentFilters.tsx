import { Search, LayoutGrid, List } from 'lucide-react';

interface DepartmentFiltersProps {
  searchQuery: string;
  onSearchChange: (value: string) => void;
  statusFilter: 'all' | 'active' | 'inactive';
  onStatusChange: (value: 'all' | 'active' | 'inactive') => void;
  viewMode: 'grid' | 'table';
  onViewModeChange: (value: 'grid' | 'table') => void;
}

export function DepartmentFilters({
  searchQuery,
  onSearchChange,
  statusFilter,
  onStatusChange,
  viewMode,
  onViewModeChange,
}: DepartmentFiltersProps) {
  return (
    <div className="glass-card p-4 rounded-xl border border-wms-border flex flex-col md:flex-row md:items-center justify-between gap-4">
      {/* Search */}
      <div className="relative flex-1 max-w-md">
        <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-wms-muted" />
        <input
          type="text"
          placeholder="Search by department name or code..."
          value={searchQuery}
          onChange={(e) => onSearchChange(e.target.value)}
          className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text placeholder:text-wms-muted pl-10 pr-4 py-2 transition outline-none focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
          id="department-search-input"
        />
      </div>

      {/* Status & View Mode */}
      <div className="flex items-center gap-3">
        <div className="flex bg-wms-hover p-1 rounded-lg border border-wms-border text-xs font-medium">
          {(['all', 'active', 'inactive'] as const).map((status) => (
            <button
              key={status}
              onClick={() => onStatusChange(status)}
              className={`px-3 py-1.5 rounded-md transition cursor-pointer capitalize ${
                statusFilter === status
                  ? 'bg-wms-indigo text-white font-semibold'
                  : 'text-wms-secondary hover:text-wms-text'
              }`}
            >
              {status}
            </button>
          ))}
        </div>

        <div className="h-6 w-[1px] bg-wms-border hidden sm:block" />

        {/* Grid / Table Toggle */}
        <div className="flex bg-wms-hover p-1 rounded-lg border border-wms-border">
          <button
            onClick={() => onViewModeChange('grid')}
            className={`p-1.5 rounded-md transition cursor-pointer ${
              viewMode === 'grid'
                ? 'bg-wms-surface text-wms-indigo shadow'
                : 'text-wms-muted hover:text-wms-text'
            }`}
            title="Grid View"
          >
            <LayoutGrid className="h-4 w-4" />
          </button>
          <button
            onClick={() => onViewModeChange('table')}
            className={`p-1.5 rounded-md transition cursor-pointer ${
              viewMode === 'table'
                ? 'bg-wms-surface text-wms-indigo shadow'
                : 'text-wms-muted hover:text-wms-text'
            }`}
            title="Table View"
          >
            <List className="h-4 w-4" />
          </button>
        </div>
      </div>
    </div>
  );
}
