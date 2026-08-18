import { Building, CheckCircle2, Users, Award } from 'lucide-react';

interface DepartmentStatsBarProps {
  total: number;
  active: number;
  totalMembers: number;
  totalDesignations: number;
}

export function DepartmentStatsBar({ total, active, totalMembers, totalDesignations }: DepartmentStatsBarProps) {
  return (
    <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div className="glass-card p-5 rounded-xl border border-wms-border flex items-center justify-between">
        <div>
          <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider block">Total Departments</span>
          <span className="text-2xl font-bold text-wms-text mt-1 block">{total}</span>
        </div>
        <div className="h-10 w-10 rounded-xl bg-wms-indigo/10 text-wms-indigo flex items-center justify-center">
          <Building className="h-5 w-5" />
        </div>
      </div>

      <div className="glass-card p-5 rounded-xl border border-wms-border flex items-center justify-between">
        <div>
          <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider block">Active Units</span>
          <span className="text-2xl font-bold text-emerald-500 mt-1 block">{active}</span>
        </div>
        <div className="h-10 w-10 rounded-xl bg-emerald-500/10 text-emerald-500 flex items-center justify-center">
          <CheckCircle2 className="h-5 w-5" />
        </div>
      </div>

      <div className="glass-card p-5 rounded-xl border border-wms-border flex items-center justify-between">
        <div>
          <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider block">Assigned Members</span>
          <span className="text-2xl font-bold text-wms-text mt-1 block">{totalMembers}</span>
        </div>
        <div className="h-10 w-10 rounded-xl bg-wms-cyan/10 text-wms-cyan flex items-center justify-center">
          <Users className="h-5 w-5" />
        </div>
      </div>

      <div className="glass-card p-5 rounded-xl border border-wms-border flex items-center justify-between">
        <div>
          <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider block">Total Designations</span>
          <span className="text-2xl font-bold text-wms-text mt-1 block">{totalDesignations}</span>
        </div>
        <div className="h-10 w-10 rounded-xl bg-amber-500/10 text-amber-500 flex items-center justify-center">
          <Award className="h-5 w-5" />
        </div>
      </div>
    </div>
  );
}
