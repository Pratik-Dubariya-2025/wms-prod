import { useState, useEffect, useCallback } from 'react';
import { Modal } from '@/components/ui/Modal/Modal';
import {
  getUserRoles,
  getUserPermissionOverrides,
  assignUserRole,
  removeUserRole,
  addPermissionOverride,
  removePermissionOverride
} from '@/api/rolesApi';
import type { UserPermissionOverrideItem, UserRoleItem } from '@/api/rolesApi';
import { getInviteMeta } from '@/api/usersApi';
import { getEmployeeProfile, updateEmployeeProfile } from '@/api/hrApi';
import type { EmployeeProfile } from '@/types/hr.types';
import type { RoleLookup } from '../types/user.types';
import { PERMISSIONS } from '@/constants/permissions';
import { classNames } from '@/utils/classNames';
import { usePermissions } from '@/hooks/usePermissions';
import {
  Shield,
  Key,
  Plus,
  Trash2,
  Clock,
  AlertCircle,
  X,
  FileText,
  Check
} from 'lucide-react';
import type { UserListItem } from '../types/user.types';

interface ManageUserRolesModalProps {
  isOpen: boolean;
  onClose: () => void;
  user: UserListItem | null;
  onSuccess: () => void;
}

// Flat list of all system permission codes
const ALL_PERMISSIONS = Object.values(PERMISSIONS);

export function ManageUserRolesModal({ isOpen, onClose, user, onSuccess }: ManageUserRolesModalProps) {
  const { hasPermission, isInRole } = usePermissions();
  const canUpdate = hasPermission(PERMISSIONS.ROLE_UPDATE);
  const canReadSalary = hasPermission(PERMISSIONS.SALARY_READ);
  const canWriteSalary = hasPermission(PERMISSIONS.SALARY_WRITE);

  const [activeTab, setActiveTab] = useState<'roles' | 'overrides' | 'hrProfile'>('roles');

  // Roles Tab state
  const [userRoles, setUserRoles] = useState<UserRoleItem[]>([]);
  const [allRoles, setAllRoles] = useState<RoleLookup[]>([]);
  const [roleToAssign, setRoleToAssign] = useState('');
  const [rolesLoading, setRolesLoading] = useState(false);
  const [rolesError, setRolesError] = useState<string | null>(null);

  // Overrides Tab state
  const [overrides, setOverrides] = useState<UserPermissionOverrideItem[]>([]);
  const [manageableOverridePermissions, setManageableOverridePermissions] = useState<string[]>([]);
  const [overridesLoading, setOverridesLoading] = useState(false);
  const [overridesError, setOverridesError] = useState<string | null>(null);

  // A permission is manageable by the current caller if they're ADMIN (grants everything)
  // or the backend explicitly returned it in `manageablePermissions` for this target user.
  const canManageOverride = (permissionCode: string) =>
    isInRole('ADMIN') || manageableOverridePermissions.includes(permissionCode);

  // Form state for new override
  const [newOverridePerm, setNewOverridePerm] = useState('');
  const [newOverrideGranted, setNewOverrideGranted] = useState(true);
  const [newOverrideReason, setNewOverrideReason] = useState('');
  const [newOverrideExpires, setNewOverrideExpires] = useState('');
  const [overrideSubmitting, setOverrideSubmitting] = useState(false);

  // HR Profile Tab state
  const [hrLoading, setHrLoading] = useState(false);
  const [hrSaving, setHrSaving] = useState(false);
  const [hrError, setHrError] = useState<string | null>(null);
  const [hrSuccess, setHrSuccess] = useState<string | null>(null);

  // HR Profile Form states
  const [salary, setSalary] = useState<number>(0);
  const [bankAccountNo, setBankAccountNo] = useState('');
  const [bankIfsc, setBankIfsc] = useState('');
  const [panNumber, setPanNumber] = useState('');
  const [aadhaarLast4, setAadhaarLast4] = useState('');
  const [emergencyContactName, setEmergencyContactName] = useState('');
  const [emergencyContactPhone, setEmergencyContactPhone] = useState('');
  const [bloodGroup, setBloodGroup] = useState('');
  const [address, setAddress] = useState('');

  // Fetch all system roles lookup
  const fetchAllRoles = useCallback(async () => {
    try {
      const response = await getInviteMeta();
      if (response.succeeded && response.data) {
        setAllRoles(response.data.roles);
      }
    } catch (err: unknown) {
      console.error('Failed to load roles', err);
    }
  }, []);

  // Fetch user's assigned roles
  const fetchUserRoles = useCallback(async (userId: string) => {
    setRolesLoading(true);
    setRolesError(null);
    try {
      const response = await getUserRoles(userId);
      if (response.succeeded && response.data) {
        setUserRoles(response.data);
      } else {
        setRolesError(response.message || 'Failed to fetch user roles.');
      }
    } catch (err: unknown) {
      setRolesError(err instanceof Error ? err.message : 'An unexpected error occurred.');
    } finally {
      setRolesLoading(false);
    }
  }, []);

  // Fetch user's overrides
  const fetchUserOverrides = useCallback(async (userId: string) => {
    setOverridesLoading(true);
    setOverridesError(null);
    try {
      const response = await getUserPermissionOverrides(userId);
      if (response.succeeded && response.data) {
        setOverrides(response.data.overrides);
        setManageableOverridePermissions(response.data.manageablePermissions);
      } else {
        setOverridesError(response.message || 'Failed to fetch user permission overrides.');
      }
    } catch (err: unknown) {
      setOverridesError(err instanceof Error ? err.message : 'An unexpected error occurred.');
    } finally {
      setOverridesLoading(false);
    }
  }, []);

  // Fetch user's HR profile
  const fetchUserHrProfile = useCallback(async (userId: string) => {
    setHrLoading(true);
    setHrError(null);
    try {
      const response = await getEmployeeProfile(userId);
      if (response.succeeded && response.data) {
        const profile: EmployeeProfile = response.data;
        setSalary(profile.salary);
        setBankAccountNo(profile.bankAccountNo || '');
        setBankIfsc(profile.bankIfsc || '');
        setPanNumber(profile.panNumber || '');
        setAadhaarLast4(profile.aadhaarLast4 || '');
        setEmergencyContactName(profile.emergencyContactName || '');
        setEmergencyContactPhone(profile.emergencyContactPhone || '');
        setBloodGroup(profile.bloodGroup || '');
        setAddress(profile.address || '');
      } else {
        setHrError(response.message || 'Failed to fetch HR profile.');
      }
    } catch (err: unknown) {
      setHrError(err instanceof Error ? err.message : 'An unexpected error occurred.');
    } finally {
      setHrLoading(false);
    }
  }, []);

  // Update/Save user's HR profile
  const handleUpdateHrProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user) return;

    setHrSaving(true);
    setHrError(null);
    setHrSuccess(null);
    try {
      const payload = {
        salary: Number(salary),
        bankAccountNo: bankAccountNo.trim() || undefined,
        bankIfsc: bankIfsc.trim() || undefined,
        panNumber: panNumber.trim() || undefined,
        aadhaarLast4: aadhaarLast4.trim() || undefined,
        emergencyContactName: emergencyContactName.trim() || undefined,
        emergencyContactPhone: emergencyContactPhone.trim() || undefined,
        bloodGroup: bloodGroup.trim() || undefined,
        address: address.trim() || undefined,
      };

      const response = await updateEmployeeProfile(user.id, payload);
      if (response.succeeded) {
        setHrSuccess('HR profile updated successfully.');
        await fetchUserHrProfile(user.id);
        setTimeout(() => setHrSuccess(null), 2500);
      } else {
        setHrError(response.message || 'Failed to update HR profile.');
      }
    } catch (err: unknown) {
      setHrError(err instanceof Error ? err.message : 'Network error occurred.');
    } finally {
      setHrSaving(false);
    }
  };

  // Trigger loads when user changes or modal opens
  useEffect(() => {
    if (isOpen && user) {
      fetchAllRoles();
      fetchUserRoles(user.id);
      fetchUserOverrides(user.id);
      if (canReadSalary) {
        fetchUserHrProfile(user.id);
      }
      setActiveTab('roles');
      setNewOverridePerm('');
      setNewOverrideReason('');
      setNewOverrideExpires('');
      setHrSuccess(null);
      setHrError(null);
    }
  }, [isOpen, user, fetchAllRoles, fetchUserRoles, fetchUserOverrides, fetchUserHrProfile, canReadSalary]);

  // Assign role handler
  const handleAssignRole = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user || !roleToAssign) return;

    setRolesLoading(true);
    setRolesError(null);
    try {
      const response = await assignUserRole(user.id, roleToAssign);
      if (response.succeeded) {
        setRoleToAssign('');
        await fetchUserRoles(user.id);
        onSuccess(); // Refresh main list
      } else {
        setRolesError(response.message || 'Failed to assign role to user.');
      }
    } catch (err: unknown) {
      setRolesError(err instanceof Error ? err.message : 'Network error occurred.');
    } finally {
      setRolesLoading(false);
    }
  };

  // Revoke role handler
  const handleRevokeRole = async (roleId: string) => {
    if (!user) return;

    setRolesLoading(true);
    setRolesError(null);
    try {
      const response = await removeUserRole(user.id, roleId);
      if (response.succeeded) {
        await fetchUserRoles(user.id);
        onSuccess(); // Refresh main list
      } else {
        setRolesError(response.message || 'Failed to revoke role from user.');
      }
    } catch (err: unknown) {
      setRolesError(err instanceof Error ? err.message : 'Network error occurred.');
    } finally {
      setRolesLoading(false);
    }
  };

  // Add permission override handler
  const handleAddOverride = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!user || !newOverridePerm) return;
    if (!canManageOverride(newOverridePerm)) {
      setOverridesError('You cannot manage this permission.');
      return;
    }

    setOverrideSubmitting(true);
    setOverridesError(null);
    try {
      const payload = {
        permissionCode: newOverridePerm,
        isGranted: newOverrideGranted,
        reason: newOverrideReason.trim() || undefined,
        expiresAt: newOverrideExpires ? new Date(newOverrideExpires).toISOString() : undefined
      };

      const response = await addPermissionOverride(user.id, payload);
      if (response.succeeded) {
        setNewOverridePerm('');
        setNewOverrideReason('');
        setNewOverrideExpires('');
        await fetchUserOverrides(user.id);
      } else {
        setOverridesError(response.message || 'Failed to apply permission override.');
      }
    } catch (err: unknown) {
      setOverridesError(err instanceof Error ? err.message : 'Network error occurred.');
    } finally {
      setOverrideSubmitting(false);
    }
  };

  // Remove permission override handler
  const handleRemoveOverride = async (permissionCode: string) => {
    if (!user) return;
    if (!canManageOverride(permissionCode)) {
      setOverridesError('You cannot manage this permission.');
      return;
    }

    setOverridesLoading(true);
    setOverridesError(null);
    try {
      const response = await removePermissionOverride(user.id, permissionCode);
      if (response.succeeded) {
        await fetchUserOverrides(user.id);
      } else {
        setOverridesError(response.message || 'Failed to remove override.');
      }
    } catch (err: unknown) {
      setOverridesError(err instanceof Error ? err.message : 'Network error occurred.');
    } finally {
      setOverridesLoading(false);
    }
  };

  // Roles that are NOT currently assigned to the user
  const assignableRoles = allRoles.filter(
    (role) => !userRoles.some((ur) => ur.name === role.name)
  );

  if (!user || !hasPermission(PERMISSIONS.USER_READ)) return null;

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={`Access Control: ${user.firstName} ${user.lastName}`}
      size="lg"
    >
      <div className="space-y-5">
        {/* User Card Summary */}
        <div className="flex items-center gap-3 p-3 rounded-lg bg-wms-hover border border-wms-border">
          <div className="h-10 w-10 rounded-full bg-gradient-to-br from-wms-indigo to-wms-cyan flex items-center justify-center font-bold text-white">
            {user.firstName[0]}{user.lastName[0]}
          </div>
          <div>
            <div className="flex items-center gap-2">
              <span className="font-bold text-sm text-wms-text">{user.firstName} {user.lastName}</span>
              <span className="text-[10px] text-wms-muted bg-wms-hover px-1.5 py-0.5 rounded">{user.employeeCode}</span>
            </div>
            <p className="text-xs text-wms-secondary mt-0.5">{user.designationName} • {user.departmentName}</p>
          </div>
        </div>

        {/* Custom Tab Bar */}
        <div className="flex border-b border-wms-border">
          <button
            onClick={() => setActiveTab('roles')}
            className={classNames(
              'flex items-center gap-2 px-4 py-2 text-sm font-semibold border-b-2 transition duration-150 cursor-pointer',
              activeTab === 'roles'
                ? 'border-wms-indigo text-wms-indigo'
                : 'border-transparent text-wms-muted hover:text-wms-text'
            )}
          >
            <Shield className="h-4 w-4" />
            Security Roles
          </button>
          {canUpdate && (
            <button
              onClick={() => setActiveTab('overrides')}
              className={classNames(
                'flex items-center gap-2 px-4 py-2 text-sm font-semibold border-b-2 transition duration-150 cursor-pointer',
                activeTab === 'overrides'
                  ? 'border-wms-indigo text-wms-indigo'
                  : 'border-transparent text-wms-muted hover:text-wms-text'
              )}
            >
              <Key className="h-4 w-4" />
              Permission Overrides
            </button>
          )}
          {canReadSalary && (
            <button
              onClick={() => setActiveTab('hrProfile')}
              className={classNames(
                'flex items-center gap-2 px-4 py-2 text-sm font-semibold border-b-2 transition duration-150 cursor-pointer',
                activeTab === 'hrProfile'
                  ? 'border-wms-indigo text-wms-indigo'
                  : 'border-transparent text-wms-muted hover:text-wms-text'
              )}
            >
              <FileText className="h-4 w-4" />
              HR Profile
            </button>
          )}
        </div>

        {/* ─── TAB CONTENT: SECURITY ROLES ─── */}
        {activeTab === 'roles' && (
          <div className="space-y-4">
            {rolesError && (
              <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 rounded-lg text-sm text-wms-danger flex items-center gap-2">
                <AlertCircle className="h-4 w-4" />
                {rolesError}
              </div>
            )}

            {/* Grant/Assign New Role Form */}
            {canUpdate && (
              <form onSubmit={handleAssignRole} className="flex flex-col sm:flex-row gap-3 items-end bg-wms-hover p-3 rounded-lg border border-wms-border">
                <div className="flex-1 space-y-1.5 w-full">
                  <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">
                    Grant Role
                  </label>
                  <select
                    value={roleToAssign}
                    onChange={(e) => setRoleToAssign(e.target.value)}
                    className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo"
                  >
                    <option value="" className="bg-wms-bg text-wms-muted">Select role to grant...</option>
                    {assignableRoles.map((role) => (
                      <option key={role.id} value={role.id} className="bg-wms-bg text-wms-text">
                        {role.name} {role.description ? `(${role.description})` : ''}
                      </option>
                    ))}
                  </select>
                </div>
                <button
                  type="submit"
                  disabled={!roleToAssign || rolesLoading}
                  className="w-full sm:w-auto inline-flex items-center justify-center gap-2 px-4 py-2 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition cursor-pointer disabled:opacity-50 h-9"
                >
                  <Plus className="h-4 w-4" />
                  Grant
                </button>
              </form>
            )}

            {/* Assigned Roles List */}
            <div className="space-y-2">
              <h4 className="text-xs font-semibold text-wms-muted uppercase tracking-wider">Assigned Roles</h4>
              {rolesLoading && userRoles.length === 0 ? (
                <div className="h-16 bg-wms-hover rounded-lg animate-pulse" />
              ) : userRoles.length === 0 ? (
                <div className="text-center py-6 text-wms-muted text-sm border border-dashed border-wms-border rounded-lg">
                  No roles are currently assigned to this user.
                </div>
              ) : (
                <div className="space-y-2">
                  {userRoles.map((role) => (
                    <div
                      key={role.id}
                      className="p-3 rounded-lg bg-wms-hover border border-wms-border flex items-center justify-between gap-3"
                    >
                      <div className="min-w-0">
                        <span className="font-semibold text-sm text-wms-text block">{role.name}</span>
                        {role.description && <span className="text-[10px] text-wms-muted block mt-0.5">{role.description}</span>}
                      </div>
                      {canUpdate && (
                        <button
                          type="button"
                          onClick={() => handleRevokeRole(role.id)}
                          disabled={rolesLoading}
                          className="p-1.5 rounded-lg bg-wms-danger/10 hover:bg-wms-danger/20 text-wms-danger hover:text-red-400 transition cursor-pointer"
                          title="Revoke Role"
                        >
                          <Trash2 className="h-4 w-4" />
                        </button>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}

        {/* ─── TAB CONTENT: PERMISSION OVERRIDES ─── */}
        {activeTab === 'overrides' && canUpdate && (
          <div className="space-y-4">
            {overridesError && (
              <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 rounded-lg text-sm text-wms-danger flex items-center gap-2">
                <AlertCircle className="h-4 w-4" />
                {overridesError}
              </div>
            )}

            {/* Add New Override Form */}
            {canUpdate && (
              <form onSubmit={handleAddOverride} className="space-y-3 bg-wms-hover p-4 rounded-lg border border-wms-border">
                <h4 className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Add or Update Override</h4>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
                  {/* Permission select */}
                  <div className="space-y-1">
                    <label className="text-[10px] font-bold text-wms-muted uppercase">Permission</label>
                    <select
                      value={newOverridePerm}
                      required
                      onChange={(e) => setNewOverridePerm(e.target.value)}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-xs text-wms-text outline-none focus:border-wms-indigo"
                    >
                      <option value="" className="bg-wms-bg text-wms-muted">Choose permission...</option>
                      {ALL_PERMISSIONS.map((code) => {
                        const manageable = canManageOverride(code);
                        return (
                          <option
                            key={code}
                            value={code}
                            disabled={!manageable}
                            title={!manageable ? 'You cannot manage this permission' : undefined}
                            className="bg-wms-bg text-wms-text"
                          >
                            {code}{!manageable ? ' (not manageable)' : ''}
                          </option>
                        );
                      })}
                    </select>
                    {newOverridePerm && !canManageOverride(newOverridePerm) && (
                      <span className="text-[10px] text-wms-danger block mt-1">You cannot manage this permission.</span>
                    )}
                  </div>

                  {/* Overrule state */}
                  <div className="space-y-1">
                    <label className="text-[10px] font-bold text-wms-muted uppercase">Override Action</label>
                    <div className="flex gap-2">
                      <button
                        type="button"
                        onClick={() => setNewOverrideGranted(true)}
                        className={classNames(
                          'flex-1 py-2 rounded-lg text-xs font-semibold border transition cursor-pointer',
                          newOverrideGranted
                            ? 'bg-wms-emerald/10 border-wms-emerald/40 text-wms-emerald'
                            : 'bg-wms-hover border-wms-border text-wms-secondary'
                        )}
                      >
                        Grant Override
                      </button>
                      <button
                        type="button"
                        onClick={() => setNewOverrideGranted(false)}
                        className={classNames(
                          'flex-1 py-2 rounded-lg text-xs font-semibold border transition cursor-pointer',
                          !newOverrideGranted
                            ? 'bg-wms-danger/10 border-wms-danger/40 text-wms-danger'
                            : 'bg-wms-hover border-wms-border text-wms-secondary'
                        )}
                      >
                        Deny Override
                      </button>
                    </div>
                  </div>

                  {/* Expiration date */}
                  <div className="space-y-1">
                    <label className="text-[10px] font-bold text-wms-muted uppercase flex items-center gap-1">
                      Expiration Date
                      <span className="text-[9px] text-wms-muted font-normal uppercase normal-case">(Optional)</span>
                    </label>
                    <input
                      type="datetime-local"
                      value={newOverrideExpires}
                      onChange={(e) => setNewOverrideExpires(e.target.value)}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-1.5 text-xs text-wms-text outline-none focus:border-wms-indigo"
                    />
                  </div>

                  {/* Reason */}
                  <div className="space-y-1">
                    <label className="text-[10px] font-bold text-wms-muted uppercase">Reason / Justification</label>
                    <input
                      type="text"
                      placeholder="e.g. Temporary project role"
                      value={newOverrideReason}
                      onChange={(e) => setNewOverrideReason(e.target.value)}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-1.5 text-xs text-wms-text outline-none focus:border-wms-indigo"
                    />
                  </div>
                </div>

                <div className="flex justify-end pt-2">
                  <button
                    type="submit"
                    disabled={!newOverridePerm || !canManageOverride(newOverridePerm) || overrideSubmitting}
                    className="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-xs font-semibold transition cursor-pointer disabled:opacity-50"
                  >
                    <Plus className="h-3.5 w-3.5" />
                    Apply Override
                  </button>
                </div>
              </form>
            )}

            {/* List active overrides */}
            <div className="space-y-2">
              <h4 className="text-xs font-semibold text-wms-muted uppercase tracking-wider">Active Overrides</h4>
              {overridesLoading && overrides.length === 0 ? (
                <div className="h-16 bg-wms-hover rounded-lg animate-pulse" />
              ) : overrides.length === 0 ? (
                <div className="text-center py-6 text-wms-muted text-sm border border-dashed border-wms-border rounded-lg">
                  No permission overrides exist for this employee.
                </div>
              ) : (
                <div className="space-y-2">
                  {overrides.map((override) => (
                    <div
                      key={override.permissionCode}
                      className={classNames(
                        'p-3 rounded-lg border flex items-start justify-between gap-3',
                        override.isGranted
                          ? 'bg-wms-emerald/5 border-wms-emerald/20'
                          : 'bg-wms-danger/5 border-wms-danger/20'
                      )}
                    >
                      <div className="min-w-0 flex-1">
                        <div className="flex items-center gap-2 flex-wrap">
                          <code className="text-xs font-bold text-wms-text">{override.permissionCode}</code>
                          <span className={classNames(
                            'text-[9px] uppercase font-bold px-1.5 py-0.5 rounded',
                            override.isGranted ? 'bg-wms-emerald/10 text-wms-emerald' : 'bg-wms-danger/10 text-wms-danger'
                          )}>
                            {override.isGranted ? 'Explicitly Granted' : 'Explicitly Denied'}
                          </span>
                        </div>
                        {override.reason && <p className="text-[10px] text-wms-secondary mt-1 font-medium">Reason: {override.reason}</p>}
                        {override.expiresAt && (
                          <span className="text-[9px] text-wms-muted mt-1 inline-flex items-center gap-1">
                            <Clock className="h-3 w-3" />
                            Expires: {new Date(override.expiresAt).toLocaleString()}
                          </span>
                        )}
                      </div>
                      {canUpdate && (
                        <button
                          type="button"
                          onClick={() => handleRemoveOverride(override.permissionCode)}
                          disabled={overridesLoading || !canManageOverride(override.permissionCode)}
                          className="p-1.5 rounded-lg bg-wms-hover hover:bg-wms-hover text-wms-secondary hover:text-wms-text transition cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed"
                          title={canManageOverride(override.permissionCode) ? 'Delete Override' : 'You cannot manage this permission'}
                        >
                          <X className="h-4 w-4" />
                        </button>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        )}

        {/* ─── TAB CONTENT: HR PROFILE ─── */}
        {activeTab === 'hrProfile' && canReadSalary && (
          <div className="space-y-4">
            {hrError && (
              <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 rounded-lg text-sm text-wms-danger flex items-center gap-2">
                <AlertCircle className="h-4 w-4" />
                {hrError}
              </div>
            )}
            {hrSuccess && (
              <div className="p-3 bg-wms-emerald/10 border border-wms-emerald/20 rounded-lg text-sm text-wms-emerald flex items-center gap-2">
                <Check className="h-4 w-4" />
                {hrSuccess}
              </div>
            )}

            {hrLoading ? (
              <div className="space-y-3 py-4">
                <div className="h-8 bg-wms-hover rounded animate-pulse" />
                <div className="grid grid-cols-2 gap-4">
                  <div className="h-8 bg-wms-hover rounded animate-pulse" />
                  <div className="h-8 bg-wms-hover rounded animate-pulse" />
                </div>
                <div className="h-20 bg-wms-hover rounded animate-pulse" />
              </div>
            ) : (
              <form onSubmit={handleUpdateHrProfile} className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {/* Salary */}
                  <div className="space-y-1.5">
                    <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Salary (Monthly)</label>
                    <input
                      type="number"
                      required
                      disabled={!canWriteSalary || hrSaving}
                      value={salary}
                      onChange={(e) => setSalary(Number(e.target.value))}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo disabled:opacity-50"
                    />
                  </div>

                  {/* PAN Number */}
                  <div className="space-y-1.5">
                    <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">PAN Card Number</label>
                    <input
                      type="text"
                      disabled={!canWriteSalary || hrSaving}
                      placeholder="e.g. ABCDE1234F"
                      value={panNumber}
                      onChange={(e) => setPanNumber(e.target.value)}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo disabled:opacity-50"
                    />
                  </div>

                  {/* Bank Account Number */}
                  <div className="space-y-1.5">
                    <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Bank Account Number</label>
                    <input
                      type="text"
                      disabled={!canWriteSalary || hrSaving}
                      value={bankAccountNo}
                      onChange={(e) => setBankAccountNo(e.target.value)}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo disabled:opacity-50"
                    />
                  </div>

                  {/* Bank IFSC */}
                  <div className="space-y-1.5">
                    <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Bank IFSC Code</label>
                    <input
                      type="text"
                      disabled={!canWriteSalary || hrSaving}
                      placeholder="e.g. SBIN0001234"
                      value={bankIfsc}
                      onChange={(e) => setBankIfsc(e.target.value)}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo disabled:opacity-50"
                    />
                  </div>

                  {/* Aadhaar Last 4 */}
                  <div className="space-y-1.5">
                    <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Aadhaar (Last 4 Digits)</label>
                    <input
                      type="text"
                      maxLength={4}
                      placeholder="e.g. 1234"
                      disabled={!canWriteSalary || hrSaving}
                      value={aadhaarLast4}
                      onChange={(e) => setAadhaarLast4(e.target.value.replace(/\D/g, ''))}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo disabled:opacity-50"
                    />
                  </div>

                  {/* Blood Group */}
                  <div className="space-y-1.5">
                    <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Blood Group</label>
                    <select
                      disabled={!canWriteSalary || hrSaving}
                      value={bloodGroup}
                      onChange={(e) => setBloodGroup(e.target.value)}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo disabled:opacity-50"
                    >
                      <option value="">Select blood group...</option>
                      <option value="A+">A+</option>
                      <option value="A-">A-</option>
                      <option value="B+">B+</option>
                      <option value="B-">B-</option>
                      <option value="AB+">AB+</option>
                      <option value="AB-">AB-</option>
                      <option value="O+">O+</option>
                      <option value="O-">O-</option>
                    </select>
                  </div>

                  {/* Emergency Contact Name */}
                  <div className="space-y-1.5">
                    <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Emergency Contact Name</label>
                    <input
                      type="text"
                      disabled={!canWriteSalary || hrSaving}
                      value={emergencyContactName}
                      onChange={(e) => setEmergencyContactName(e.target.value)}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo disabled:opacity-50"
                    />
                  </div>

                  {/* Emergency Contact Phone */}
                  <div className="space-y-1.5">
                    <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Emergency Contact Phone</label>
                    <input
                      type="text"
                      disabled={!canWriteSalary || hrSaving}
                      value={emergencyContactPhone}
                      onChange={(e) => setEmergencyContactPhone(e.target.value)}
                      className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo disabled:opacity-50"
                    />
                  </div>
                </div>

                {/* Address */}
                <div className="space-y-1.5">
                  <label className="text-xs font-semibold text-wms-secondary uppercase tracking-wider">Residential Address</label>
                  <textarea
                    rows={2}
                    disabled={!canWriteSalary || hrSaving}
                    value={address}
                    onChange={(e) => setAddress(e.target.value)}
                    className="w-full rounded-lg bg-wms-hover border border-wms-border px-3 py-2 text-sm text-wms-text outline-none focus:border-wms-indigo resize-none disabled:opacity-50"
                  />
                </div>

                {canWriteSalary && (
                  <div className="flex justify-end pt-2">
                    <button
                      type="submit"
                      disabled={hrSaving}
                      className="inline-flex items-center gap-1.5 px-4 py-2 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-xs font-semibold transition cursor-pointer disabled:opacity-50"
                    >
                      {hrSaving ? 'Saving...' : 'Save Profile'}
                    </button>
                  </div>
                )}
              </form>
            )}
          </div>
        )}

        <div className="flex justify-end pt-3 border-t border-wms-border">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 rounded-lg bg-wms-hover border border-wms-border text-sm font-semibold text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition cursor-pointer"
          >
            Close
          </button>
        </div>
      </div>
    </Modal>
  );
}
