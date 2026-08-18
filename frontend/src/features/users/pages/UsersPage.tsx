import { useState } from 'react';
import { 
  Search, 
  Users as UsersIcon, 
  RefreshCw, 
  UserPlus, 
  Filter, 
  X 
} from 'lucide-react';

import { useUsers } from '@/features/users/hooks/useUsers';
import type { UserListItem } from '@/features/users/types/user.types';
import { Avatar } from '@/components/ui/Avatar/Avatar';
import { Badge } from '@/components/ui/Badge/Badge';
import { PermissionGate } from '@/components/shared/PermissionGate';
import { PERMISSIONS } from '@/constants/permissions';
import { usePermissions } from '@/hooks/usePermissions';
import { classNames } from '@/utils/classNames';
import { useModal } from '@/components/ui/Modal/useModal';
import { InviteUserModal } from '@/features/users/components/InviteUserModal';
import { ManageUserRolesModal } from '@/features/users/components/ManageUserRolesModal';
import { Pagination } from '@/components/ui/Pagination/Pagination';

const ROLE_BADGE_MAP: Record<string, 'indigo' | 'purple' | 'cyan' | 'emerald' | 'warning'> = {
  ADMIN: 'indigo',
  EMPLOYEE: 'cyan',
  HR_MANAGER: 'purple',
  TEAM_LEAD: 'emerald',
};

export default function UsersPage() {
  const {
    users,
    pagination,
    isLoading,
    error,
    params,
    setSearch,
    setRole,
    setStatus,
    setPage,
    setPageSize,
    refresh,
  } = useUsers(10);

  const { hasPermission } = usePermissions();
  const [showFilters, setShowFilters] = useState(false);
  const inviteModal = useModal();
  const rbacModal = useModal();
  const [selectedUser, setSelectedUser] = useState<UserListItem | null>(null);

  const hasActiveFilters = !!params.role || params.isActive !== null;

  const handleRowClick = (user: UserListItem) => {
    if (!hasPermission(PERMISSIONS.USER_READ)) return;
    setSelectedUser(user);
    rbacModal.open();
  };

  const clearFilters = () => {
    setRole('');
    setStatus(null);
    setShowFilters(false);
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* ─── Page Header ─── */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-wms-border pb-5">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-xl bg-gradient-to-br from-wms-indigo to-wms-cyan flex items-center justify-center shadow-lg shadow-wms-indigo/20">
            <UsersIcon className="h-5 w-5 text-wms-text" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-wms-text tracking-tight">Users</h1>
            <p className="text-sm text-wms-secondary mt-0.5">
              {pagination ? (
                <>
                  <span className="text-wms-indigo font-semibold">{pagination.totalCount}</span> total employees
                </>
              ) : (
                'Manage employee accounts'
              )}
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={refresh}
            disabled={isLoading}
            className="inline-flex items-center gap-2 px-3 py-2.5 rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer disabled:opacity-50"
            title="Refresh"
          >
            <RefreshCw className={classNames('h-4 w-4', isLoading && 'animate-spin')} />
          </button>
          <PermissionGate permissions={PERMISSIONS.USER_CREATE}>
            <button 
              onClick={inviteModal.open}
              className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-indigo/25"
            >
              <UserPlus className="h-4 w-4" />
              Invite User
            </button>
          </PermissionGate>
        </div>
      </div>

      {/* ─── Search & Filters Bar ─── */}
      <div className="flex flex-col sm:flex-row gap-3">
        {/* Search Input */}
        <div className="relative flex-1">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-wms-muted" />
          <input
            type="text"
            placeholder="Search by name, email, or employee code..."
            value={params.search || ''}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text placeholder:text-wms-muted
              pl-10 pr-4 py-2.5 transition duration-200 outline-none
              focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20 hover:border-wms-border"
            id="users-search"
          />
          {params.search && (
            <button
              onClick={() => setSearch('')}
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
            'inline-flex items-center gap-2 px-4 py-2.5 rounded-lg border text-sm font-medium transition duration-200 cursor-pointer',
            hasActiveFilters
              ? 'bg-wms-indigo/10 border-wms-indigo/30 text-wms-indigo'
              : 'bg-wms-hover border-wms-border text-wms-secondary hover:text-wms-text hover:bg-wms-hover',
          )}
        >
          <Filter className="h-4 w-4" />
          Filters
          {hasActiveFilters && (
            <span className="ml-1 h-5 w-5 rounded-full bg-wms-indigo text-white text-xs flex items-center justify-center">
              {(params.role ? 1 : 0) + (params.isActive !== null ? 1 : 0)}
            </span>
          )}
        </button>
      </div>

      {/* ─── Collapsible Filter Panel ─── */}
      {showFilters && (
        <div className="glass-card rounded-xl p-4 flex flex-wrap items-end gap-4 animate-fade-in">
          {/* Role filter */}
          <div className="flex flex-col gap-1.5 min-w-[160px]">
            <label className="text-xs font-semibold text-wms-muted uppercase tracking-wide">Role</label>
            <select
              value={params.role || ''}
              onChange={(e) => setRole(e.target.value)}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text appearance-none
                px-4 py-2.5 transition duration-200 outline-none cursor-pointer
                focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
            >
              <option value="" className="bg-wms-bg text-wms-muted">All Roles</option>
              <option value="ADMIN" className="bg-wms-bg text-wms-text">Admin</option>
              <option value="EMPLOYEE" className="bg-wms-bg text-wms-text">Employee</option>
              <option value="HR_MANAGER" className="bg-wms-bg text-wms-text">HR Manager</option>
              <option value="TEAM_LEAD" className="bg-wms-bg text-wms-text">Team Lead</option>
            </select>
          </div>

          {/* Status filter */}
          <div className="flex flex-col gap-1.5 min-w-[160px]">
            <label className="text-xs font-semibold text-wms-muted uppercase tracking-wide">Status</label>
            <select
              value={params.isActive === null ? '' : String(params.isActive)}
              onChange={(e) => {
                const v = e.target.value;
                setStatus(v === '' ? null : v === 'true');
              }}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text appearance-none
                px-4 py-2.5 transition duration-200 outline-none cursor-pointer
                focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
            >
              <option value="" className="bg-wms-bg text-wms-muted">All Status</option>
              <option value="true" className="bg-wms-bg text-wms-text">Active</option>
              <option value="false" className="bg-wms-bg text-wms-text">Inactive</option>
            </select>
          </div>

          {/* Clear all */}
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

      {/* ─── Error State ─── */}
      {error && (
        <div className="glass-card rounded-xl p-6 border-wms-danger/20 flex items-center gap-3">
          <div className="h-8 w-8 rounded-lg bg-wms-danger/15 flex items-center justify-center flex-shrink-0">
            <X className="h-4 w-4 text-wms-danger" />
          </div>
          <div>
            <p className="text-sm font-medium text-wms-danger">Failed to load users</p>
            <p className="text-xs text-wms-muted mt-0.5">{error}</p>
          </div>
          <button
            onClick={refresh}
            className="ml-auto text-sm text-wms-indigo hover:text-indigo-400 font-medium transition cursor-pointer"
          >
            Retry
          </button>
        </div>
      )}

      {/* ─── Table ─── */}
      <div className="glass-card rounded-xl overflow-hidden animate-fade-in">
        <div className="overflow-x-auto">
          <table className="w-full text-sm" id="users-table">
            <thead>
              <tr className="border-b border-wms-border">
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Employee</th>
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide hidden lg:table-cell">Department</th>
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide hidden md:table-cell">Roles</th>
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide">Status</th>
                <th className="px-5 py-3.5 text-left text-xs font-semibold text-wms-muted uppercase tracking-wide hidden xl:table-cell">Contact</th>
              </tr>
            </thead>
            <tbody>
              {isLoading ? (
                // Skeleton rows
                Array.from({ length: params.pageSize }).map((_, i) => (
                  <tr key={`skeleton-${i}`} className="border-b border-wms-border">
                    <td className="px-5 py-4">
                      <div className="flex items-center gap-3">
                        <div className="h-10 w-10 rounded-full bg-wms-hover animate-pulse" />
                        <div className="space-y-2">
                          <div className="h-3.5 w-32 bg-wms-hover rounded animate-pulse" />
                          <div className="h-3 w-20 bg-wms-hover rounded animate-pulse" />
                        </div>
                      </div>
                    </td>
                    <td className="px-5 py-4 hidden lg:table-cell"><div className="h-3.5 w-24 bg-wms-hover rounded animate-pulse" /></td>
                    <td className="px-5 py-4 hidden md:table-cell"><div className="h-5 w-16 bg-wms-hover rounded-full animate-pulse" /></td>
                    <td className="px-5 py-4"><div className="h-5 w-14 bg-wms-hover rounded-full animate-pulse" /></td>
                    <td className="px-5 py-4 hidden xl:table-cell"><div className="h-3.5 w-36 bg-wms-hover rounded animate-pulse" /></td>
                  </tr>
                ))
              ) : users.length === 0 ? (
                <tr>
                  <td colSpan={5} className="px-5 py-16 text-center">
                    <div className="flex flex-col items-center gap-3">
                      <div className="h-14 w-14 rounded-2xl bg-wms-hover flex items-center justify-center">
                        <UsersIcon className="h-7 w-7 text-wms-muted" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-wms-secondary">No users found</p>
                        <p className="text-xs text-wms-muted mt-1">
                          {params.search || hasActiveFilters
                            ? 'Try adjusting your search or filters'
                            : 'No employees have been added yet'}
                        </p>
                      </div>
                      {(params.search || hasActiveFilters) && (
                        <button
                          onClick={() => { setSearch(''); clearFilters(); }}
                          className="mt-1 text-sm text-wms-indigo hover:text-indigo-400 font-medium transition cursor-pointer"
                        >
                          Clear all filters
                        </button>
                      )}
                    </div>
                  </td>
                </tr>
              ) : (
                users.map((user) => (
                  <tr
                    key={user.id}
                    onClick={() => handleRowClick(user)}
                    className={classNames(
                      "border-b border-wms-border last:border-b-0 transition duration-150 hover:bg-wms-hover group",
                      hasPermission(PERMISSIONS.USER_READ) ? "cursor-pointer" : "cursor-default"
                    )}
                  >
                    {/* Employee info */}
                    <td className="px-5 py-4">
                      <div className="flex items-center gap-3">
                        <Avatar name={`${user.firstName} ${user.lastName}`} size="md" />
                        <div className="min-w-0">
                          <p className="text-sm font-semibold text-wms-text truncate group-hover:text-wms-indigo transition">
                            {user.firstName} {user.lastName}
                          </p>
                          <p className="text-xs text-wms-muted truncate">{user.employeeCode}</p>
                        </div>
                      </div>
                    </td>

                    {/* Department */}
                    <td className="px-5 py-4 hidden lg:table-cell">
                      <div>
                        <p className="text-sm text-wms-text">{user.departmentName}</p>
                        <p className="text-xs text-wms-muted">{user.designationName}</p>
                      </div>
                    </td>

                    {/* Roles */}
                    <td className="px-5 py-4 hidden md:table-cell">
                      <div className="flex flex-wrap gap-1.5">
                        {user.roles.map((role) => (
                          <Badge key={role} variant={ROLE_BADGE_MAP[role] || 'default'}>
                            {role.replace('_', ' ')}
                          </Badge>
                        ))}
                      </div>
                    </td>

                    {/* Status */}
                    <td className="px-5 py-4">
                      <div className="flex items-center gap-2">
                        <span className={classNames(
                          'h-2 w-2 rounded-full',
                          user.isActive ? 'bg-wms-emerald shadow-sm shadow-wms-emerald/50' : 'bg-wms-muted'
                        )} />
                        <span className={classNames(
                          'text-xs font-medium',
                          user.isActive ? 'text-wms-emerald' : 'text-wms-muted',
                        )}>
                          {user.isActive ? 'Active' : 'Inactive'}
                        </span>
                      </div>
                    </td>

                    {/* Contact */}
                    <td className="px-5 py-4 hidden xl:table-cell">
                      <div>
                        <p className="text-sm text-wms-text truncate">{user.email}</p>
                        {user.phoneNumber && (
                          <p className="text-xs text-wms-muted">{user.phoneNumber}</p>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        {/* ─── Pagination Footer ─── */}
        {pagination && (
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
        )}
      </div>

      {/* ─── Invite User Modal ─── */}
      <InviteUserModal 
        isOpen={inviteModal.isOpen} 
        onClose={inviteModal.close} 
        onSuccess={refresh} 
      />

      {/* ─── Manage User Roles / Permission Overrides Modal ─── */}
      <ManageUserRolesModal
        isOpen={rbacModal.isOpen}
        onClose={rbacModal.close}
        user={selectedUser}
        onSuccess={refresh}
      />
    </div>
  );
}
