import { Users, Award, Eye, Edit2, Trash2, CheckCircle2, XCircle } from 'lucide-react';
import { PERMISSIONS } from '@/constants/permissions';
import { PermissionGate } from '@/components/shared/PermissionGate';
import type { DepartmentListItem } from '../types/department.types';

interface DepartmentCardProps {
  department: DepartmentListItem;
  onView: (id: string) => void;
  onEdit: (dept: DepartmentListItem) => void;
  onDelete: (dept: DepartmentListItem) => void;
}

export function DepartmentCard({ department: dept, onView, onEdit, onDelete }: DepartmentCardProps) {
  return (
    <div className="glass-card rounded-2xl border border-wms-border p-6 flex flex-col justify-between hover:border-wms-indigo/40 transition duration-300 group shadow-lg shadow-black/5">
      <div>
        {/* Header */}
        <div className="flex items-start justify-between gap-3 mb-4">
          <div className="flex items-center gap-3">
            <div className="h-11 w-11 rounded-xl bg-wms-indigo/10 text-wms-indigo flex items-center justify-center font-bold text-lg group-hover:scale-105 transition-transform">
              {dept.code.substring(0, 2)}
            </div>
            <div>
              <h3 className="font-bold text-base text-wms-text group-hover:text-wms-indigo transition-colors">
                {dept.name}
              </h3>
              <span className="text-xs font-mono font-semibold px-2 py-0.5 rounded bg-wms-hover text-wms-secondary border border-wms-border inline-block mt-0.5">
                {dept.code}
              </span>
            </div>
          </div>

          <span
            className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold ${
              dept.isActive
                ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-500/20'
                : 'bg-wms-danger/10 text-wms-danger border border-wms-danger/20'
            }`}
          >
            {dept.isActive ? <CheckCircle2 className="h-3.5 w-3.5" /> : <XCircle className="h-3.5 w-3.5" />}
            {dept.isActive ? 'Active' : 'Inactive'}
          </span>
        </div>

        {/* Description */}
        <p className="text-xs text-wms-muted line-clamp-2 min-h-[36px] mb-5">
          {dept.description || 'No description provided for this department unit.'}
        </p>

        {/* Stats row */}
        <div className="grid grid-cols-2 gap-3 p-3 rounded-xl bg-wms-hover/50 border border-wms-border mb-5">
          <div className="flex items-center gap-2">
            <Users className="h-4 w-4 text-wms-indigo" />
            <div>
              <span className="text-[10px] text-wms-muted uppercase tracking-wider block">Members</span>
              <span className="text-sm font-bold text-wms-text">{dept.memberCount}</span>
            </div>
          </div>
          <div className="flex items-center gap-2">
            <Award className="h-4 w-4 text-amber-500" />
            <div>
              <span className="text-[10px] text-wms-muted uppercase tracking-wider block">Designations</span>
              <span className="text-sm font-bold text-wms-text">{dept.designationCount}</span>
            </div>
          </div>
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex items-center justify-between pt-3 border-t border-wms-border">
        <button
          onClick={() => onView(dept.id)}
          className="inline-flex items-center gap-1.5 text-xs font-semibold text-wms-indigo hover:text-indigo-400 transition cursor-pointer"
        >
          <Eye className="h-4 w-4" />
          View Details
        </button>

        <div className="flex items-center gap-1">
          <PermissionGate permissions={PERMISSIONS.DEPARTMENT_UPDATE}>
            <button
              onClick={() => onEdit(dept)}
              className="p-2 rounded-lg text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition cursor-pointer"
              title="Edit Department"
            >
              <Edit2 className="h-4 w-4" />
            </button>
          </PermissionGate>

          <PermissionGate permissions={PERMISSIONS.DEPARTMENT_DELETE}>
            <button
              onClick={() => onDelete(dept)}
              className="p-2 rounded-lg text-wms-muted hover:text-wms-danger hover:bg-wms-danger/10 transition cursor-pointer"
              title="Delete Department"
            >
              <Trash2 className="h-4 w-4" />
            </button>
          </PermissionGate>
        </div>
      </div>
    </div>
  );
}
