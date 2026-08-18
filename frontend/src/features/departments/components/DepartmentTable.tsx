import { Eye, Edit2, Trash2 } from 'lucide-react';
import { PERMISSIONS } from '@/constants/permissions';
import { PermissionGate } from '@/components/shared/PermissionGate';
import type { DepartmentListItem } from '../types/department.types';

interface DepartmentTableProps {
  departments: DepartmentListItem[];
  onView: (id: string) => void;
  onEdit: (dept: DepartmentListItem) => void;
  onDelete: (dept: DepartmentListItem) => void;
}

export function DepartmentTable({ departments, onView, onEdit, onDelete }: DepartmentTableProps) {
  return (
    <div className="glass-card rounded-xl border border-wms-border overflow-hidden">
      <div className="overflow-x-auto">
        <table className="w-full text-left text-sm">
          <thead className="bg-wms-hover/50 text-xs font-semibold text-wms-muted uppercase tracking-wider border-b border-wms-border">
            <tr>
              <th className="p-4">Department</th>
              <th className="p-4">Code</th>
              <th className="p-4">Status</th>
              <th className="p-4">Members</th>
              <th className="p-4">Designations</th>
              <th className="p-4 text-right">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-wms-border text-wms-text">
            {departments.map((dept) => (
              <tr key={dept.id} className="hover:bg-wms-hover/40 transition">
                <td className="p-4 font-semibold">
                  <div className="flex items-center gap-3">
                    <div className="h-9 w-9 rounded-lg bg-wms-indigo/10 text-wms-indigo font-bold text-sm flex items-center justify-center">
                      {dept.code.substring(0, 2)}
                    </div>
                    <div>
                      <span className="block font-bold">{dept.name}</span>
                      <span className="text-xs text-wms-muted line-clamp-1">
                        {dept.description || 'No description'}
                      </span>
                    </div>
                  </div>
                </td>
                <td className="p-4">
                  <span className="font-mono font-semibold text-xs px-2 py-0.5 rounded bg-wms-hover border border-wms-border">
                    {dept.code}
                  </span>
                </td>
                <td className="p-4">
                  <span
                    className={`inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold ${
                      dept.isActive
                        ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border border-emerald-500/20'
                        : 'bg-wms-danger/10 text-wms-danger border border-wms-danger/20'
                    }`}
                  >
                    {dept.isActive ? 'Active' : 'Inactive'}
                  </span>
                </td>
                <td className="p-4 font-semibold">{dept.memberCount}</td>
                <td className="p-4 font-semibold">{dept.designationCount}</td>
                <td className="p-4 text-right">
                  <div className="flex items-center justify-end gap-2">
                    <button
                      onClick={() => onView(dept.id)}
                      className="p-1.5 rounded-lg text-wms-indigo hover:bg-wms-indigo/10 transition cursor-pointer"
                      title="View Details"
                    >
                      <Eye className="h-4 w-4" />
                    </button>
                    <PermissionGate permissions={PERMISSIONS.DEPARTMENT_UPDATE}>
                      <button
                        onClick={() => onEdit(dept)}
                        className="p-1.5 rounded-lg text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition cursor-pointer"
                        title="Edit"
                      >
                        <Edit2 className="h-4 w-4" />
                      </button>
                    </PermissionGate>
                    <PermissionGate permissions={PERMISSIONS.DEPARTMENT_DELETE}>
                      <button
                        onClick={() => onDelete(dept)}
                        className="p-1.5 rounded-lg text-wms-muted hover:text-wms-danger hover:bg-wms-danger/10 transition cursor-pointer"
                        title="Delete"
                      >
                        <Trash2 className="h-4 w-4" />
                      </button>
                    </PermissionGate>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}
