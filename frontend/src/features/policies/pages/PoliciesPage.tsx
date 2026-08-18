import { useState, useEffect, useCallback, useRef } from 'react';
import {
  ShieldAlert,
  Search,
  Plus,
  RefreshCw,
  AlertCircle,
  HelpCircle,
  Filter,
  UserCheck,
  BookOpen
} from 'lucide-react';

import {
  getPolicies,
  getPolicyById,
  createPolicy,
  updatePolicy,
  deletePolicy,
  addPolicyCondition,
  removePolicyCondition,
  addFieldRestriction,
  removeFieldRestriction,
  getModulesWithActions,
  type PolicyListDto,
  type PolicyDetailDto,
  type ModuleActionDto
} from '@/api/policiesApi';
import { getRoles, type RoleListItem } from '@/api/rolesApi';
import { getUsers } from '@/api/usersApi';
import type { UserListItem } from '@/features/users/types/user.types';
import { classNames } from '@/utils/classNames';
import { Pagination } from '@/components/ui/Pagination/Pagination';
import { useModal } from '@/components/ui/Modal/useModal';
import { ConfirmDialog } from '@/components/shared/ConfirmDialog';
import { Modal } from '@/components/ui/Modal/Modal';
import { toast } from '@/components/ui/Toast/Toast';
import { usePermissions } from '@/hooks/usePermissions';
import { PERMISSIONS } from '@/constants/permissions';

// Extracted Subcomponents
import { CreatePolicyModal } from '../components/CreatePolicyModal';
import { EditPolicyModal } from '../components/EditPolicyModal';
import { PolicyDetailPanel } from '../components/PolicyDetailPanel';
import { PolicyGuideModal } from '../components/PolicyGuideModal';

export default function PoliciesPage({ hideHeader = false, compactHeader = false }: { hideHeader?: boolean; compactHeader?: boolean }) {
  const [policies, setPolicies] = useState<PolicyListDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Pagination & Filters
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [search, setSearch] = useState('');
  const [debouncedSearch, setDebouncedSearch] = useState('');
  const [totalCount, setTotalCount] = useState(0);
  const [totalPages, setTotalPages] = useState(0);
  const [hasPreviousPage, setHasPreviousPage] = useState(false);
  const [hasNextPage, setHasNextPage] = useState(false);

  // Policy detail
  const [selectedPolicy, setSelectedPolicy] = useState<PolicyDetailDto | null>(null);
  const [isLoadingDetail, setIsLoadingDetail] = useState(false);
  const [detailError, setDetailError] = useState<string | null>(null);

  // Dropdown lists
  const [modules, setModules] = useState<ModuleActionDto[]>([]);
  const [allRoles, setAllRoles] = useState<RoleListItem[]>([]);
  const [allUsers, setAllUsers] = useState<UserListItem[]>([]);

  // Modals state
  const createModal = useModal();
  const editModal = useModal();
  const guideModal = useModal();
  const [isSubmittingCreate, setIsSubmittingCreate] = useState(false);
  const [isSubmittingEdit, setIsSubmittingEdit] = useState(false);

  // Custom Confirmation and Alert modals
  const { hasPermission, hasAnyPermission } = usePermissions();
  const canCreate = hasPermission(PERMISSIONS.POLICY_WRITE);
  const canUpdate = hasPermission(PERMISSIONS.POLICY_WRITE);
  const canDelete = hasPermission(PERMISSIONS.POLICY_WRITE);
  const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
  const [condIdToDelete, setCondIdToDelete] = useState<string | null>(null);
  const [restIdToDelete, setRestIdToDelete] = useState<string | null>(null);
  const [alertModalOpen, setAlertModalOpen] = useState(false);
  const [alertModalTitle, setAlertModalTitle] = useState('');
  const [alertModalMessage, setAlertModalMessage] = useState('');
  const [alertModalType, setAlertModalType] = useState<'info' | 'success' | 'error'>('info');

  const showAlert = (title: string, message: string, type: 'info' | 'success' | 'error' = 'info') => {
    setAlertModalTitle(title);
    setAlertModalMessage(message);
    setAlertModalType(type);
    setAlertModalOpen(true);
  };

  const modulePermissionMap: Record<string, string[]> = {
    USER_MGMT: ['user.read', 'user.create', 'user.update', 'user.delete'],
    DEPT_MGMT: ['department.read', 'department.create', 'department.update', 'department.delete'],
    ROLE_MGMT: ['role.read', 'role.create', 'role.update', 'role.delete'],
    TASK_MGMT: ['task.read', 'task.create', 'task.update', 'task.delete'],
    PROJECT_MGMT: ['project.read', 'project.create', 'project.update', 'project.delete'],
    HR_MGMT: ['salary.read', 'salary.write'],
    LEAVE_MGMT: ['leave.read', 'leave.write', 'leave.approve'],
    CRM: ['leads.read', 'leads.write'],
    ACCOUNTS: ['invoices.read', 'invoices.write'],
    POLICY_MGMT: ['policy.read', 'policy.manage']
  };

  const authorizedModules = modules.filter(m => {
    const required = modulePermissionMap[m.code];
    if (!required) return true;
    return hasAnyPermission(required);
  });

  // Search Debounce
  const searchTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  useEffect(() => {
    if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    searchTimerRef.current = setTimeout(() => {
      setDebouncedSearch(search);
      setPage(1);
    }, 400);
    return () => {
      if (searchTimerRef.current) clearTimeout(searchTimerRef.current);
    };
  }, [search]);

  // Fetch Policies
  const fetchPolicies = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const response = await getPolicies({
        pageNumber: page,
        pageSize: pageSize,
        search: debouncedSearch
      });
      if (response.succeeded && response.data) {
        setPolicies(response.data.items);
        setTotalCount(response.data.totalCount);
        setTotalPages(response.data.totalPages);
        setHasPreviousPage(response.data.hasPreviousPage);
        setHasNextPage(response.data.hasNextPage);
      } else {
        setError(response.message || 'Failed to fetch policies');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred');
    } finally {
      setIsLoading(false);
    }
  }, [page, pageSize, debouncedSearch]);

  // Fetch policy detail
  const fetchPolicyDetail = useCallback(async (id: string) => {
    setIsLoadingDetail(true);
    setDetailError(null);
    try {
      const response = await getPolicyById(id);
      if (response.succeeded && response.data) {
        setSelectedPolicy(response.data);
      } else {
        setDetailError(response.message || 'Failed to fetch policy details');
      }
    } catch (err: unknown) {
      setDetailError(err instanceof Error ? err.message : 'Failed to fetch policy details');
    } finally {
      setIsLoadingDetail(false);
    }
  }, []);

  // Fetch metadata dropdowns (modules, roles, users)
  const fetchMetadata = useCallback(async () => {
    try {
      const [modulesRes, rolesRes, usersRes] = await Promise.all([
        getModulesWithActions(),
        getRoles({ pageNumber: 1, pageSize: 100 }),
        getUsers({ pageNumber: 1, pageSize: 100 })
      ]);

      if (modulesRes.succeeded && modulesRes.data) {
        setModules(modulesRes.data);
      }
      if (rolesRes.succeeded && rolesRes.data) {
        setAllRoles(rolesRes.data.items);
      }
      if (usersRes.succeeded && usersRes.data) {
        setAllUsers(usersRes.data.items);
      }
    } catch (e) {
      console.error('Failed to load metadata', e);
    }
  }, []);

  useEffect(() => {
    fetchPolicies();
  }, [fetchPolicies]);

  useEffect(() => {
    fetchMetadata();
  }, [fetchMetadata]);

  // Handle auto-selecting first policy
  useEffect(() => {
    if (policies.length > 0 && !selectedPolicy) {
      fetchPolicyDetail(policies[0].id);
    }
  }, [policies, selectedPolicy, fetchPolicyDetail]);

  const handleSelectPolicy = (policy: PolicyListDto) => {
    fetchPolicyDetail(policy.id);
  };

  const handleCreatePolicy = async (payload: {
    name: string;
    description?: string;
    moduleId: string;
    action: string;
    effect: 'Allow' | 'Deny';
    roleId?: string;
    userId?: string;
    rowFilterExpression?: string;
    priority: number;
  }) => {
    setIsSubmittingCreate(true);
    try {
      const res = await createPolicy(payload);
      if (res.succeeded && res.data) {
        createModal.close();
        fetchPolicies();
        fetchPolicyDetail(res.data);
        toast.success('Policy created successfully');
      } else {
        showAlert('Error', res.message || 'Failed to create policy', 'error');
      }
    } catch (err: unknown) {
      showAlert('Error', err instanceof Error ? err.message : 'Error occurred', 'error');
    } finally {
      setIsSubmittingCreate(false);
    }
  };

  const handleUpdatePolicy = async (payload: {
    name: string;
    description?: string;
    moduleId: string;
    action: string;
    effect: 'Allow' | 'Deny';
    roleId?: string;
    userId?: string;
    rowFilterExpression?: string;
    priority: number;
    isActive: boolean;
  }) => {
    if (!selectedPolicy) return;
    setIsSubmittingEdit(true);
    try {
      const res = await updatePolicy(selectedPolicy.id, payload);
      if (res.succeeded) {
        editModal.close();
        fetchPolicies();
        fetchPolicyDetail(selectedPolicy.id);
        toast.success('Policy updated successfully');
      } else {
        showAlert('Error', res.message || 'Failed to update policy', 'error');
      }
    } catch (err: unknown) {
      showAlert('Error', err instanceof Error ? err.message : 'Error occurred', 'error');
    } finally {
      setIsSubmittingEdit(false);
    }
  };

  const handleDeletePolicy = () => {
    if (!selectedPolicy) return;
    setShowDeleteConfirm(true);
  };

  const handleConfirmDelete = async () => {
    if (!selectedPolicy) return;
    setShowDeleteConfirm(false);
    try {
      const res = await deletePolicy(selectedPolicy.id);
      if (res.succeeded) {
        setSelectedPolicy(null);
        fetchPolicies();
        toast.success('Policy deleted successfully');
      } else {
        showAlert('Error', res.message || 'Failed to delete policy', 'error');
      }
    } catch (err: unknown) {
      showAlert('Error', err instanceof Error ? err.message : 'Error occurred', 'error');
    }
  };

  const handleAddCondition = async (group: number, attribute: string, operator: string, value: string) => {
    if (!selectedPolicy) return;
    const res = await addPolicyCondition(selectedPolicy.id, {
      conditionGroup: group,
      attribute,
      operator,
      value
    });
    if (res.succeeded) {
      fetchPolicyDetail(selectedPolicy.id);
      toast.success('Condition added successfully');
    } else {
      showAlert('Error', res.message || 'Failed to add condition', 'error');
      throw new Error(res.message || undefined);
    }
  };

  const handleRemoveCondition = (condId: string) => {
    setCondIdToDelete(condId);
  };

  const handleConfirmRemoveCondition = async () => {
    if (!selectedPolicy || !condIdToDelete) return;
    const condId = condIdToDelete;
    setCondIdToDelete(null);
    try {
      const res = await removePolicyCondition(selectedPolicy.id, condId);
      if (res.succeeded) {
        fetchPolicyDetail(selectedPolicy.id);
        toast.success('Condition removed successfully');
      } else {
        showAlert('Error', res.message || 'Failed to remove condition', 'error');
      }
    } catch (err: unknown) {
      showAlert('Error', err instanceof Error ? err.message : 'Error occurred', 'error');
    }
  };

  const handleAddFieldRest = async (fieldName: string, restrictionType: string, maskPattern: string) => {
    if (!selectedPolicy) return;
    const res = await addFieldRestriction(selectedPolicy.id, {
      fieldName,
      restrictionType,
      maskPattern: maskPattern || undefined
    });
    if (res.succeeded) {
      fetchPolicyDetail(selectedPolicy.id);
      toast.success('Field restriction added successfully');
    } else {
      showAlert('Error', res.message || 'Failed to add field restriction', 'error');
      throw new Error(res.message || undefined);
    }
  };

  const handleRemoveFieldRest = (restId: string) => {
    setRestIdToDelete(restId);
  };

  const handleConfirmRemoveFieldRest = async () => {
    if (!selectedPolicy || !restIdToDelete) return;
    const restId = restIdToDelete;
    setRestIdToDelete(null);
    try {
      const res = await removeFieldRestriction(selectedPolicy.id, restId);
      if (res.succeeded) {
        fetchPolicyDetail(selectedPolicy.id);
        toast.success('Field restriction removed successfully');
      } else {
        showAlert('Error', res.message || 'Failed to remove field restriction', 'error');
      }
    } catch (err: unknown) {
      showAlert('Error', err instanceof Error ? err.message : 'Error occurred', 'error');
    }
  };

  const handleToggleActive = async (isActive: boolean) => {
    if (!selectedPolicy) return;
    try {
      const payload = {
        name: selectedPolicy.name,
        description: selectedPolicy.description,
        moduleId: selectedPolicy.moduleId,
        action: selectedPolicy.action,
        effect: selectedPolicy.effect,
        roleId: selectedPolicy.roleId,
        userId: selectedPolicy.userId,
        rowFilterExpression: selectedPolicy.rowFilterExpression,
        priority: selectedPolicy.priority,
        isActive: isActive
      };
      const res = await updatePolicy(selectedPolicy.id, payload);
      if (res.succeeded) {
        fetchPolicies();
        fetchPolicyDetail(selectedPolicy.id);
      }
    } catch (e) {
      console.error(e);
    }
  };

  return (
    <div className="space-y-6 animate-fade-in">
      {/* ─── Page Header ─── */}
      {!hideHeader && (
        <div className={classNames("flex flex-col sm:flex-row sm:items-center justify-between gap-4 pb-5", !compactHeader && "border-b border-wms-border")}>
          {!compactHeader && (
            <div className="flex items-center gap-3">
              <div className="h-10 w-10 rounded-xl bg-linear-to-br from-wms-indigo to-wms-purple flex items-center justify-center shadow-lg shadow-wms-indigo/20">
                <ShieldAlert className="h-5 w-5 text-wms-text" />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-wms-text tracking-tight">Access Control Policies (PBAC)</h1>
                <p className="text-sm text-wms-secondary mt-0.5">
                  Configure fine-grained Attribute & Policy-Based Access Control, conditions, and column restrictions.
                </p>
              </div>
            </div>
          )}
          <div className={classNames("flex items-center gap-2", compactHeader ? "w-full justify-end border-b border-wms-border pb-4" : "ml-auto")}>
            <button
              onClick={guideModal.open}
              className="inline-flex items-center gap-2 px-3 py-2.5 rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer"
              title="Open the Policy Module Guide"
            >
              <BookOpen className="h-4 w-4" />
              <span className="hidden sm:inline">Guide</span>
            </button>
            <button
              onClick={fetchPolicies}
              disabled={isLoading}
              className="inline-flex items-center gap-2 px-3 py-2.5 rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer disabled:opacity-50"
              title="Refresh policies"
            >
              <RefreshCw className={classNames('h-4 w-4', isLoading && 'animate-spin')} />
            </button>
            {canCreate && (
              <button
                onClick={createModal.open}
                className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-indigo/25"
              >
                <Plus className="h-4 w-4" />
                Create Access Policy
              </button>
            )}
          </div>
        </div>
      )}

      {/* ─── Layout Grid ─── */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">
        {/* Left: Policy List (5 Cols) */}
        <div className="lg:col-span-5 space-y-4">
          <div className="glass-card rounded-xl p-4 space-y-4">
            <h2 className="text-sm font-bold text-wms-text uppercase tracking-wider flex items-center gap-2">
              <Filter className="h-4 w-4 text-wms-indigo" />
              Access Policies
            </h2>

            {/* Search */}
            <div className="relative">
              <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-wms-muted" />
              <input
                type="text"
                placeholder="Search policies..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text placeholder:text-wms-muted
                  pl-10 pr-4 py-2 transition duration-200 outline-none
                  focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20 hover:border-wms-border"
              />
            </div>

            {error && (
              <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 rounded-lg text-wms-danger text-xs flex items-center gap-2">
                <AlertCircle className="h-4 w-4 shrink-0" />
                <span>{error}</span>
              </div>
            )}

            {/* List */}
            <div className="space-y-2 max-h-120 overflow-y-auto pr-1">
              {isLoading ? (
                Array.from({ length: 4 }).map((_, i) => (
                  <div key={i} className="h-16 rounded-lg bg-wms-hover animate-pulse" />
                ))
              ) : policies.length === 0 ? (
                <div className="text-center py-8 text-wms-muted text-sm">No policies found.</div>
              ) : (
                policies.map((policy) => {
                  const isSelected = selectedPolicy?.id === policy.id;
                  return (
                    <button
                      key={policy.id}
                      onClick={() => handleSelectPolicy(policy)}
                      className={classNames(
                        'w-full text-left p-4 rounded-lg border transition duration-200 cursor-pointer flex justify-between items-start gap-3',
                        isSelected
                          ? 'bg-wms-indigo/10 border-wms-indigo/40 text-wms-text'
                          : 'bg-wms-hover border-wms-border text-wms-secondary hover:bg-wms-hover hover:border-wms-border'
                      )}
                    >
                      <div className="min-w-0 flex-1">
                        <div className="flex items-center gap-2 flex-wrap">
                          <span className="font-semibold text-sm truncate">{policy.name}</span>
                          <span className={classNames(
                            'text-[9px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded border',
                            policy.effect === 'Allow'
                              ? 'bg-wms-success/10 text-wms-success border-wms-success/20'
                              : 'bg-wms-danger/10 text-wms-danger border-wms-danger/20'
                          )}>
                            {policy.effect}
                          </span>
                          {!policy.isActive && (
                            <span className="text-[9px] uppercase font-bold tracking-wider px-1.5 py-0.5 rounded bg-wms-muted/15 text-wms-muted border border-wms-border">
                              Disabled
                            </span>
                          )}
                        </div>
                        <div className="flex items-center gap-2 mt-2 text-xs text-wms-muted">
                          <span className="bg-wms-hover px-1.5 py-0.5 rounded text-wms-indigo font-medium">{policy.moduleCode}.{policy.action}</span>
                          <span>•</span>
                          <span className="truncate">For: {policy.targetName} ({policy.targetType})</span>
                        </div>
                      </div>
                    </button>
                  );
                })
              )}
            </div>

            {/* Pagination */}
            {totalCount > 0 && (
              <div className="pt-2 border-t border-wms-border">
                <Pagination
                  pageNumber={page}
                  pageSize={pageSize}
                  totalCount={totalCount}
                  totalPages={totalPages}
                  hasPreviousPage={hasPreviousPage}
                  hasNextPage={hasNextPage}
                  onPageChange={setPage}
                  onPageSizeChange={setPageSize}
                  isLoading={isLoading}
                />
              </div>
            )}
          </div>
        </div>

        {/* Right: Policy details, conditions & restrictions (7 Cols) */}
        <div className="lg:col-span-7">
          <PolicyDetailPanel
            policy={selectedPolicy}
            isLoadingDetail={isLoadingDetail}
            detailError={detailError}
            onOpenEdit={editModal.open}
            onDelete={handleDeletePolicy}
            onToggleActive={handleToggleActive}
            onAddCondition={handleAddCondition}
            onRemoveCondition={handleRemoveCondition}
            onAddFieldRestriction={handleAddFieldRest}
            onRemoveFieldRestriction={handleRemoveFieldRest}
            canUpdate={canUpdate}
            canDelete={canDelete}
          />
        </div>
      </div>

      {/* ─── Policy Guide Modal ─── */}
      <PolicyGuideModal isOpen={guideModal.isOpen} onClose={guideModal.close} />

      {/* ─── Create Policy Modal ─── */}
      <CreatePolicyModal
        isOpen={createModal.isOpen}
        onClose={createModal.close}
        authorizedModules={authorizedModules}
        allRoles={allRoles}
        allUsers={allUsers}
        onSubmit={handleCreatePolicy}
        isSubmitting={isSubmittingCreate}
      />

      {/* ─── Edit Policy Modal ─── */}
      <EditPolicyModal
        isOpen={editModal.isOpen}
        onClose={editModal.close}
        policy={selectedPolicy}
        authorizedModules={authorizedModules}
        allRoles={allRoles}
        allUsers={allUsers}
        onSubmit={handleUpdatePolicy}
        isSubmitting={isSubmittingEdit}
      />

      <ConfirmDialog
        isOpen={showDeleteConfirm}
        onClose={() => setShowDeleteConfirm(false)}
        onConfirm={handleConfirmDelete}
        title="Confirm Policy Deletion"
        message={`Are you sure you want to delete the policy "${selectedPolicy?.name}"? This action cannot be undone.`}
        confirmLabel="Delete"
        cancelLabel="Cancel"
        variant="danger"
      />

      <ConfirmDialog
        isOpen={!!condIdToDelete}
        onClose={() => setCondIdToDelete(null)}
        onConfirm={handleConfirmRemoveCondition}
        title="Confirm Condition Removal"
        message="Are you sure you want to remove this condition from the policy?"
        confirmLabel="Remove"
        cancelLabel="Cancel"
        variant="danger"
      />

      <ConfirmDialog
        isOpen={!!restIdToDelete}
        onClose={() => setRestIdToDelete(null)}
        onConfirm={handleConfirmRemoveFieldRest}
        title="Confirm Restriction Removal"
        message="Are you sure you want to remove this field restriction from the policy?"
        confirmLabel="Remove"
        cancelLabel="Cancel"
        variant="danger"
      />

      <Modal isOpen={alertModalOpen} onClose={() => setAlertModalOpen(false)} size="sm">
        <div className="flex flex-col items-center text-center gap-4 py-2">
          <div className={classNames(
            'p-3 rounded-full',
            alertModalType === 'error' && 'bg-wms-danger/10 text-wms-danger',
            alertModalType === 'success' && 'bg-wms-success/10 text-wms-success',
            alertModalType === 'info' && 'bg-wms-indigo/10 text-wms-indigo'
          )}>
            {alertModalType === 'error' && <AlertCircle className="h-6 w-6" />}
            {alertModalType === 'success' && <UserCheck className="h-6 w-6" />}
            {alertModalType === 'info' && <HelpCircle className="h-6 w-6" />}
          </div>

          <div>
            <h3 className="text-lg font-bold text-wms-text mb-1">{alertModalTitle}</h3>
            <p className="text-sm text-wms-secondary">{alertModalMessage}</p>
          </div>

          <div className="w-full pt-2">
            <button
              onClick={() => setAlertModalOpen(false)}
              className="w-full px-4 py-2.5 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-indigo/25 text-center block"
            >
              OK
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
