import { useState, useEffect, useCallback } from 'react';
import {
  Shield,
  Search,
  Plus,
  RefreshCw,
  Check,
  AlertCircle,
  Edit2
} from 'lucide-react';

import { useRoles } from '../hooks/useRoles';
import { getRolePermissions, assignRolePermission, removeRolePermission, updateRole } from '@/api/rolesApi';
import { PERMISSIONS } from '@/constants/permissions';
import { PermissionGate } from '@/components/shared/PermissionGate';
import { useModal } from '@/components/ui/Modal/useModal';
import { usePermissions } from '@/hooks/usePermissions';
import { CreateRoleModal } from '../components/CreateRoleModal';
import { Pagination } from '@/components/ui/Pagination/Pagination';
import { classNames } from '@/utils/classNames';
import { Switch } from '@/components/ui/Switch';
import { formatId } from '@/utils/formatId';
import type { RoleListItem } from '@/api/rolesApi';

// Group permissions by category for a cleaner UI
const PERMISSION_GROUPS = [
  {
    category: 'User Management',
    permissions: [
      { code: PERMISSIONS.USER_CREATE, name: 'Create User', desc: 'Allows inviting and creating new users' },
      { code: PERMISSIONS.USER_READ, name: 'Read User', desc: 'Allows viewing employee lists and details' },
      { code: PERMISSIONS.USER_UPDATE, name: 'Update User', desc: 'Allows updating user profiles, roles, and status' },
      { code: PERMISSIONS.USER_DELETE, name: 'Delete User', desc: 'Allows soft deleting user accounts' },
    ]
  },
  {
    category: 'Department Management',
    permissions: [
      { code: PERMISSIONS.DEPARTMENT_CREATE, name: 'Create Department', desc: 'Allows adding new departments' },
      { code: PERMISSIONS.DEPARTMENT_READ, name: 'Read Department', desc: 'Allows viewing warehouse departments' },
      { code: PERMISSIONS.DEPARTMENT_UPDATE, name: 'Update Department', desc: 'Allows editing department configurations' },
      { code: PERMISSIONS.DEPARTMENT_DELETE, name: 'Delete Department', desc: 'Allows removing departments' },
    ]
  },
  {
    category: 'Role & RBAC Management',
    permissions: [
      { code: PERMISSIONS.ROLE_CREATE, name: 'Create Role', desc: 'Allows creating custom security roles' },
      { code: PERMISSIONS.ROLE_READ, name: 'Read Role', desc: 'Allows viewing roles and assigned permissions' },
      { code: PERMISSIONS.ROLE_UPDATE, name: 'Update Role', desc: 'Allows editing roles and assigning permissions' },
      { code: PERMISSIONS.ROLE_DELETE, name: 'Delete Role', desc: 'Allows deleting custom roles' },
    ]
  },
  {
    category: 'Policy Management',
    permissions: [
      { code: PERMISSIONS.POLICY_READ, name: 'Read Policies', desc: 'Allows viewing security and data policies' },
      { code: PERMISSIONS.POLICY_WRITE, name: 'Manage Policies', desc: 'Allows creating, editing, and deleting security and data policies' },
    ]
  },
  {
    category: 'Task Management',
    permissions: [
      { code: PERMISSIONS.TASK_CREATE, name: 'Create Task', desc: 'Allows creating new tasks' },
      { code: PERMISSIONS.TASK_READ, name: 'Read Task', desc: 'Allows viewing task details' },
      { code: PERMISSIONS.TASK_UPDATE, name: 'Update Task', desc: 'Allows editing task progress and details' },
      { code: PERMISSIONS.TASK_DELETE, name: 'Delete Task', desc: 'Allows deleting tasks from systems' },
    ]
  },
  {
    category: 'Project Management',
    permissions: [
      { code: PERMISSIONS.PROJECT_CREATE, name: 'Create Project', desc: 'Allows creating new projects and teams' },
      { code: PERMISSIONS.PROJECT_READ, name: 'Read Project', desc: 'Allows viewing projects and teams' },
      { code: PERMISSIONS.PROJECT_UPDATE, name: 'Update Project', desc: 'Allows updating project/team details' },
      { code: PERMISSIONS.PROJECT_DELETE, name: 'Delete Project', desc: 'Allows deleting projects and teams' },
    ]
  },
  {
    category: 'CRM Management',
    permissions: [
      { code: PERMISSIONS.LEADS_READ, name: 'Read Leads', desc: 'Allows viewing CRM lead pipeline' },
      { code: PERMISSIONS.LEADS_WRITE, name: 'Manage Leads', desc: 'Allows creating and updating CRM leads' },
    ]
  },
  {
    category: 'Accounts & Invoices',
    permissions: [
      { code: PERMISSIONS.INVOICES_READ, name: 'Read Invoices', desc: 'Allows viewing financial invoices' },
      { code: PERMISSIONS.INVOICES_WRITE, name: 'Manage Invoices', desc: 'Allows creating financial invoices' },
    ]
  },
  {
    category: 'Leave Management',
    permissions: [
      { code: PERMISSIONS.LEAVE_READ, name: 'Read Leaves', desc: 'Allows viewing leave requests' },
      { code: PERMISSIONS.LEAVE_WRITE, name: 'Submit Leaves', desc: 'Allows submitting leave requests' },
      { code: PERMISSIONS.LEAVE_APPROVE, name: 'Approve Leaves', desc: 'Allows approving or rejecting leave requests' },
    ]
  },
  {
    category: 'HR & Salary Management',
    permissions: [
      { code: PERMISSIONS.SALARY_READ, name: 'Read Salary', desc: 'Allows viewing sensitive HR salary details' },
      { code: PERMISSIONS.SALARY_WRITE, name: 'Manage Salary', desc: 'Allows updating sensitive HR salary details' },
    ]
  }
];

export default function RolesPage() {
  const { hasPermission, isInRole } = usePermissions();
  const canUpdateRole = hasPermission(PERMISSIONS.ROLE_UPDATE);

  const {
    roles,
    pagination,
    isLoading,
    params,
    setSearch,
    setPage,
    setPageSize,
    refresh,
  } = useRoles(5);

  const createModal = useModal();
  const [selectedRole, setSelectedRole] = useState<RoleListItem | null>(null);

  // Details edit state
  const [roleDescription, setRoleDescription] = useState('');
  const [roleName, setRoleName] = useState('');
  const [isUpdatingMetadata, setIsUpdatingMetadata] = useState(false);
  const [metadataMessage, setMetadataMessage] = useState<{ type: 'success' | 'error', text: string } | null>(null);

  // Permissions state
  const [assignedPermissions, setAssignedPermissions] = useState<string[]>([]);
  const [manageablePermissions, setManageablePermissions] = useState<string[]>([]);
  const [isLoadingPermissions, setIsLoadingPermissions] = useState(false);
  const [updatingPermissions, setUpdatingPermissions] = useState<Record<string, boolean>>({});
  const [permissionError, setPermissionError] = useState<string | null>(null);

  // Auto-select first role on load
  useEffect(() => {
    if (roles.length > 0 && !selectedRole) {
      setSelectedRole(roles[0]);
    }
  }, [roles, selectedRole]);

  // Handle role selection
  const handleSelectRole = useCallback((role: RoleListItem) => {
    setSelectedRole(role);
    setRoleDescription(role.description || '');
    setRoleName(role.name);
    setMetadataMessage(null);
    setPermissionError(null);
  }, []);

  // Fetch permissions of selected role
  const fetchAssignedPermissions = useCallback(async (roleId: string) => {
    setIsLoadingPermissions(true);
    setPermissionError(null);
    try {
      const response = await getRolePermissions(roleId);
      if (response.succeeded && response.data) {
        setAssignedPermissions(response.data.assignedPermissions);
        setManageablePermissions(response.data.manageablePermissions);
      } else {
        setPermissionError(response.message || 'Failed to fetch assigned permissions');
      }
    } catch (err: unknown) {
      setPermissionError(err instanceof Error ? err.message : 'An unexpected error occurred');
    } finally {
      setIsLoadingPermissions(false);
    }
  }, []);

  // Fetch when selectedRole changes
  useEffect(() => {
    if (selectedRole) {
      fetchAssignedPermissions(selectedRole.id);
    }
  }, [selectedRole, fetchAssignedPermissions]);

  // Handle toggling permission
  const handlePermissionToggle = async (permissionCode: string, isAssigned: boolean) => {
    if (!selectedRole) return;

    setUpdatingPermissions(prev => ({ ...prev, [permissionCode]: true }));
    try {
      if (isAssigned) {
        // Remove assignment
        const response = await removeRolePermission(selectedRole.id, permissionCode);
        if (response.succeeded) {
          setAssignedPermissions(prev => prev.filter(code => code !== permissionCode));
        } else {
          setPermissionError(response.message || 'Failed to remove permission');
        }
      } else {
        // Assign permission
        const response = await assignRolePermission(selectedRole.id, permissionCode);
        if (response.succeeded) {
          setAssignedPermissions(prev => [...prev, permissionCode]);
        } else {
          setPermissionError(response.message || 'Failed to assign permission');
        }
      }
    } catch (err: unknown) {
      setPermissionError(err instanceof Error ? err.message : 'Network error occurred');
    } finally {
      setUpdatingPermissions(prev => ({ ...prev, [permissionCode]: false }));
    }
  };

  // Save Role Metadata (description-only or name + description)
  const handleSaveMetadata = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedRole) return;

    setIsUpdatingMetadata(true);
    setMetadataMessage(null);
    try {
      const response = await updateRole(selectedRole.id, {
        name: selectedRole.isSystemRole ? selectedRole.name : roleName.trim(),
        description: roleDescription.trim() || undefined,
        priority: selectedRole.priority
      });

      if (response.succeeded) {
        setMetadataMessage({ type: 'success', text: 'Role details updated successfully!' });
        // Refresh the list of roles to capture any descriptive update
        refresh();
        // Update local object description
        setSelectedRole(prev => prev ? { ...prev, name: selectedRole.isSystemRole ? prev.name : roleName.trim(), description: roleDescription.trim() } : null);
      } else {
        setMetadataMessage({ type: 'error', text: response.message || 'Failed to update role details' });
      }
    } catch (err: unknown) {
      setMetadataMessage({
        type: 'error',
        text: err instanceof Error ? err.message : 'An unexpected error occurred'
      });
    } finally {
      setIsUpdatingMetadata(false);
    }
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* ─── Page Header ─── */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-wms-border pb-5">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-xl bg-linear-to-br from-wms-indigo to-wms-cyan flex items-center justify-center shadow-lg shadow-wms-indigo/20">
            <Shield className="h-5 w-5 text-wms-text" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-wms-text tracking-tight">Security Roles & Permissions</h1>
            <p className="text-sm text-wms-secondary mt-0.5">
              Configure system roles, customize permissions, and manage access privileges. Row/column-level
              data policies are managed separately under Policies.
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={refresh}
            disabled={isLoading}
            className="inline-flex items-center gap-2 px-3 py-2.5 rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer disabled:opacity-50"
            title="Refresh Roles"
          >
            <RefreshCw className={classNames('h-4 w-4', isLoading && 'animate-spin')} />
          </button>
          <PermissionGate permissions={PERMISSIONS.ROLE_CREATE}>
            <button
              onClick={createModal.open}
              className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-indigo/25"
            >
              <Plus className="h-4 w-4" />
              Create Custom Role
            </button>
          </PermissionGate>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
          {/* Left Side: Roles List (5 Cols) */}
          <div className="lg:col-span-5 space-y-4">
            <div className="glass-card rounded-xl p-4 space-y-4">
              <h2 className="text-sm font-bold text-wms-text uppercase tracking-wider">Security Roles</h2>

              {/* Search Input */}
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-wms-muted" />
                <input
                  type="text"
                  placeholder="Search roles..."
                  value={params.search || ''}
                  onChange={(e) => setSearch(e.target.value)}
                  className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text placeholder:text-wms-muted
                    pl-10 pr-4 py-2 transition duration-200 outline-none
                    focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20 hover:border-wms-border"
                  id="roles-search"
                />
              </div>

              {/* List */}
              <div className="space-y-2 max-h-100 overflow-y-auto pr-1">
                {isLoading ? (
                  Array.from({ length: 4 }).map((_, i) => (
                    <div key={i} className="h-16 rounded-lg bg-wms-hover animate-pulse" />
                  ))
                ) : roles.length === 0 ? (
                  <div className="text-center py-8 text-wms-muted text-sm">No roles found</div>
                ) : (
                  roles.map((role) => {
                    const isSelected = selectedRole?.id === role.id;
                    return (
                      <button
                        key={role.id}
                        onClick={() => handleSelectRole(role)}
                        className={classNames(
                          'w-full text-left p-4 rounded-lg border transition duration-200 cursor-pointer flex justify-between items-start gap-3',
                          isSelected
                            ? 'bg-wms-indigo/10 border-wms-indigo/40 text-wms-text'
                            : 'bg-wms-hover border-wms-border text-wms-secondary hover:bg-wms-hover hover:border-wms-border'
                        )}
                      >
                        <div className="min-w-0">
                          <div className="flex items-center gap-2">
                            <span className="font-semibold text-sm truncate">{role.name}</span>
                            {role.isSystemRole ? (
                              <span className="text-[10px] px-1.5 py-0.5 rounded-full bg-wms-hover text-wms-muted font-medium">System</span>
                            ) : (
                              <span className="text-[10px] px-1.5 py-0.5 rounded-full bg-wms-indigo/20 text-wms-indigo font-medium">Custom</span>
                            )}
                          </div>
                          <p className="text-xs text-wms-muted mt-1 line-clamp-1">{role.description || 'No description provided.'}</p>
                        </div>
                        <Shield className={classNames('h-4 w-4 shrink-0 mt-0.5', isSelected ? 'text-wms-indigo' : 'text-wms-muted')} />
                      </button>
                    );
                  })
                )}
              </div>

              {/* Pagination Footer */}
              {pagination && (
                <div className="pt-2 border-t border-wms-border">
                  <Pagination
                    pageNumber={pagination.pageNumber}
                    pageSize={pagination.pageSize}
                    totalCount={pagination.totalCount}
                    totalPages={pagination.totalPages}
                    hasPreviousPage={pagination.hasPreviousPage}
                    hasNextPage={pagination.hasNextPage}
                    onPageChange={setPage}
                    onPageSizeChange={setPageSize}
                    isLoading={isLoading}
                  />
                </div>
              )}
            </div>
          </div>

          {/* Right Side: Selected Role Details & Permissions Grid (7 Cols) */}
          <div className="lg:col-span-7">
            {selectedRole ? (
              <div className="glass-card rounded-xl p-4 space-y-4">
                {/* Role Title Card */}
                <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-4 border-b border-wms-border">
                  <div>
                    <h2 className="text-lg font-bold text-wms-text flex items-center gap-2 flex-wrap">
                      {selectedRole.name}
                      {selectedRole.isSystemRole ? (
                        <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded bg-amber-500/10 text-amber-400 border border-amber-500/20">
                          System Role
                        </span>
                      ) : (
                        <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded bg-wms-indigo/20 text-wms-indigo border border-wms-indigo/20">
                          Custom Role
                        </span>
                      )}
                      <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded bg-wms-hover text-wms-muted border border-wms-border">
                        Code: {selectedRole.code}
                      </span>
                      <span className="text-[10px] uppercase font-bold tracking-wider px-2 py-0.5 rounded bg-wms-hover text-wms-muted border border-wms-border">
                        Priority: {selectedRole.priority}
                      </span>
                    </h2>
                    <p className="text-xs text-wms-muted mt-1 font-mono">ID: {formatId(selectedRole.id)}</p>
                  </div>
                </div>

                {/* Role Metadata Form */}
                <form onSubmit={handleSaveMetadata} className="space-y-4">
                  {metadataMessage && (
                    <div className={classNames(
                      'p-3 rounded-lg text-sm border flex items-center gap-2',
                      metadataMessage.type === 'success'
                        ? 'bg-wms-emerald/10 border-wms-emerald/20 text-wms-emerald'
                        : 'bg-wms-danger/10 border-wms-danger/20 text-wms-danger'
                    )}>
                      {metadataMessage.type === 'success' ? <Check className="h-4 w-4" /> : <AlertCircle className="h-4 w-4" />}
                      {metadataMessage.text}
                    </div>
                  )}

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    <div className="space-y-1.5">
                      <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">
                        Role Identifier
                      </label>
                      <input
                        type="text"
                        disabled={selectedRole.isSystemRole || !canUpdateRole}
                        value={roleName}
                        onChange={(e) => setRoleName(e.target.value)}
                        className="w-full rounded-lg bg-wms-hover border border-wms-border px-4 py-2 text-sm text-wms-text placeholder:text-wms-muted focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20 outline-none transition disabled:opacity-60 disabled:cursor-not-allowed"
                        id="selected-role-name"
                      />
                    </div>

                    <div className="space-y-1.5">
                      <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider flex items-center gap-1">
                        Role Description
                        {selectedRole.isSystemRole && (
                          <span className="text-[10px] text-amber-400 font-normal uppercase">(Description editable only)</span>
                        )}
                      </label>
                      <input
                        type="text"
                        disabled={!canUpdateRole}
                        value={roleDescription}
                        onChange={(e) => setRoleDescription(e.target.value)}
                        placeholder="e.g. Permission definition"
                        className="w-full rounded-lg bg-wms-hover border border-wms-border px-4 py-2 text-sm text-wms-text placeholder:text-wms-muted focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20 outline-none transition disabled:opacity-60 disabled:cursor-not-allowed"
                        id="selected-role-description"
                      />
                    </div>
                  </div>

                  <div className="flex justify-end">
                    <PermissionGate permissions={PERMISSIONS.ROLE_UPDATE}>
                      <button
                        type="submit"
                        disabled={isUpdatingMetadata || (!selectedRole.isSystemRole && !roleName.trim())}
                        className="inline-flex items-center gap-2 px-4 py-2 rounded-lg bg-wms-hover border border-wms-border hover:bg-wms-hover text-sm font-semibold text-wms-text transition cursor-pointer disabled:opacity-50"
                      >
                        <Edit2 className="h-3.5 w-3.5" />
                        {isUpdatingMetadata ? 'Saving...' : 'Update Details'}
                      </button>
                    </PermissionGate>
                  </div>
                </form>

                {/* Permissions Control Grid */}
                <div className="space-y-4 pt-4 border-t border-wms-border">
                  <div className="flex items-center justify-between">
                    <h3 className="text-sm font-bold text-wms-text uppercase tracking-wider">Assigned Permissions</h3>
                    {isLoadingPermissions && <RefreshCw className="h-4 w-4 text-wms-muted animate-spin" />}
                  </div>

                  {permissionError && (
                    <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 rounded-lg text-sm text-wms-danger flex items-center gap-2">
                      <AlertCircle className="h-4 w-4" />
                      {permissionError}
                    </div>
                  )}

                  {isLoadingPermissions ? (
                    <div className="space-y-4 py-4">
                      <div className="h-20 bg-wms-hover rounded-lg animate-pulse" />
                      <div className="h-20 bg-wms-hover rounded-lg animate-pulse" />
                    </div>
                  ) : (
                    <div className="max-h-100 overflow-y-auto pr-2 space-y-4">
                      {PERMISSION_GROUPS.map((group) => {
                        const visiblePermissions = group.permissions.filter(
                          (perm) => isInRole('ADMIN') || manageablePermissions.includes(perm.code)
                        );

                        if (visiblePermissions.length === 0) return null;

                        return (
                          <div key={group.category} className="space-y-3">
                            <h4 className="text-xs font-semibold text-wms-indigo uppercase tracking-wider">{group.category}</h4>
                            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
                              {visiblePermissions.map((perm) => {
                                const isAssigned = assignedPermissions.includes(perm.code);
                                const isPending = updatingPermissions[perm.code] || false;

                                return (
                                  <div
                                    key={perm.code}
                                    className={classNames(
                                      'p-3.5 rounded-xl border flex items-center justify-between gap-3 transition-all duration-200',
                                      isAssigned
                                        ? 'bg-wms-indigo/10 border-wms-indigo/30 shadow-sm'
                                        : 'bg-wms-hover/60 border-wms-border hover:border-wms-border hover:bg-wms-hover'
                                    )}
                                  >
                                    <div className="min-w-0 flex-1">
                                      <div className="flex items-center gap-1.5 flex-wrap">
                                        <span className="text-sm font-semibold text-wms-text leading-tight">{perm.name}</span>
                                        <code className="text-[9px] font-mono text-wms-indigo bg-wms-indigo/10 px-1.5 py-0.5 rounded font-medium">{perm.code}</code>
                                      </div>
                                      <span className="text-[11px] text-wms-muted block mt-1 leading-snug">{perm.desc}</span>
                                    </div>

                                    <Switch
                                      checked={isAssigned}
                                      onChange={() => handlePermissionToggle(perm.code, isAssigned)}
                                      disabled={!canUpdateRole || isLoadingPermissions}
                                      isLoading={isPending}
                                      size="sm"
                                      ariaLabel={`Toggle permission ${perm.name}`}
                                    />
                                  </div>
                                );
                              })}
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  )}
                </div>
              </div>
            ) : (
              <div className="glass-card rounded-xl p-16 text-center text-wms-muted">
                <Shield className="h-10 w-10 mx-auto text-wms-muted mb-3 opacity-50" />
                Select a security role from the left list to manage permissions.
              </div>
            )}
          </div>
        </div>
      {/* ─── Create Custom Role Modal ─── */}
      <CreateRoleModal
        isOpen={createModal.isOpen}
        onClose={createModal.close}
        onSuccess={refresh}
      />
    </div>
  );
}
