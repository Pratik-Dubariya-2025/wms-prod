import type { DepartmentUser } from '../types/department.types';

interface DepartmentUserListProps {
  users: DepartmentUser[];
}

export function DepartmentUserList({ users }: DepartmentUserListProps) {
  if (users.length === 0) {
    return (
      <div className="text-center py-8 text-wms-muted text-sm border border-dashed border-wms-border rounded-xl">
        No users currently assigned to this department.
      </div>
    );
  }

  return (
    <div className="space-y-3">
      {users.map((u) => (
        <div
          key={u.id}
          className="p-3.5 rounded-xl bg-wms-hover/50 border border-wms-border flex items-center justify-between hover:border-wms-indigo/30 transition"
        >
          <div>
            <div className="flex items-center gap-2">
              <span className="font-semibold text-wms-text text-sm">{u.fullName}</span>
              <span className="text-xs px-2 py-0.5 rounded bg-wms-indigo/10 text-wms-indigo font-mono">
                {u.employeeCode}
              </span>
            </div>
            <p className="text-xs text-wms-muted mt-0.5">{u.email}</p>
          </div>
          <div className="text-right">
            <span className="text-xs font-medium text-wms-secondary block">
              {u.designationName || 'No Designation'}
            </span>
            {u.roleName && (
              <span className="text-[10px] font-semibold text-wms-muted uppercase tracking-wider block mt-0.5">
                {u.roleName}
              </span>
            )}
          </div>
        </div>
      ))}
    </div>
  );
}
