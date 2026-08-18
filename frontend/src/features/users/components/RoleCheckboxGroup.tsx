import type { RoleLookup } from '../types/user.types';

interface RoleCheckboxGroupProps {
  roles: RoleLookup[];
  value: string[];
  onChange: (roleIds: string[]) => void;
  error?: string;
}

/**
 * Controlled multi-select of roles. Kept as a standalone component so the
 * invite form stays declarative and the checkbox wiring is reusable.
 */
export function RoleCheckboxGroup({ roles, value, onChange, error }: RoleCheckboxGroupProps) {
  const toggle = (roleId: string, checked: boolean) => {
    onChange(checked ? [...value, roleId] : value.filter((id) => id !== roleId));
  };

  return (
    <div className="space-y-2">
      <label className="text-xs font-semibold text-wms-muted uppercase tracking-wide">Assign Roles</label>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
        {roles.map((role) => (
          <label
            key={role.id}
            className="flex items-start gap-3 p-3 rounded-lg bg-wms-hover border border-wms-border hover:bg-wms-hover transition cursor-pointer"
          >
            <input
              type="checkbox"
              value={role.id}
              checked={value.includes(role.id)}
              onChange={(e) => toggle(role.id, e.target.checked)}
              className="mt-1 accent-wms-indigo cursor-pointer"
            />
            <div>
              <p className="text-sm font-semibold text-wms-text">{role.name}</p>
              <p className="text-xs text-wms-muted mt-0.5">{role.description || 'No description provided'}</p>
            </div>
          </label>
        ))}
      </div>
      {error && <p className="text-xs text-wms-danger mt-1">{error}</p>}
    </div>
  );
}
