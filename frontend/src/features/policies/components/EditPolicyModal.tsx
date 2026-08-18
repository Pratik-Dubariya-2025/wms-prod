import { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { HelpCircle, BookOpen } from 'lucide-react';
import { Modal } from '@/components/ui/Modal/Modal';
import { Switch } from '@/components/ui/Switch';
import type { ModuleActionDto, PolicyDetailDto } from '@/api/policiesApi';
import type { RoleListItem } from '@/api/rolesApi';
import type { UserListItem } from '@/features/users/types/user.types';
import { editPolicySchema, type EditPolicyFormValues } from '../schemas/policy.schema';
import { RowFilterTreeBuilder } from './RowFilterTreeBuilder';
import { PolicyGuideModal } from './PolicyGuideModal';
import { createEmptyGroup, parseRowFilterExpression, serializeRowFilterNode } from '../utils/rowFilterTree';
import { GUIDE_HEADINGS } from '../content/policyGuideContent';

interface EditPolicyModalProps {
  isOpen: boolean;
  onClose: () => void;
  policy: PolicyDetailDto | null;
  authorizedModules: ModuleActionDto[];
  allRoles: RoleListItem[];
  allUsers: UserListItem[];
  onSubmit: (payload: {
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
  }) => Promise<void>;
  isSubmitting: boolean;
}

export function EditPolicyModal({
  isOpen,
  onClose,
  policy,
  authorizedModules,
  allRoles,
  allUsers,
  onSubmit,
  isSubmitting
}: EditPolicyModalProps) {
  const [isGuideOpen, setIsGuideOpen] = useState(false);

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    control,
    formState: { errors, isValid },
  } = useForm<EditPolicyFormValues>({
    resolver: zodResolver(editPolicySchema),
    mode: 'onChange',
    defaultValues: {
      name: '',
      description: '',
      targetType: 'Role',
      targetId: '',
      moduleId: '',
      action: '',
      effect: 'Allow',
      priority: 10,
      rowFilterExpression: createEmptyGroup(),
      isActive: true,
    },
  });

  const targetType = watch('targetType');
  const moduleId = watch('moduleId');
  const isActive = watch('isActive');

  // Initialize/Reset form values when the modal is opened or the active policy changes
  useEffect(() => {
    if (isOpen && policy) {
      reset({
        name: policy.name || '',
        description: policy.description || '',
        targetType: policy.userId ? 'User' : 'Role',
        targetId: policy.userId || policy.roleId || '',
        moduleId: policy.moduleId || '',
        action: policy.action || '',
        effect: (policy.effect as 'Allow' | 'Deny') || 'Allow',
        priority: policy.priority ?? 10,
        rowFilterExpression: parseRowFilterExpression(policy.rowFilterExpression) ?? createEmptyGroup(),
        isActive: policy.isActive ?? true,
      });
    }
  }, [isOpen, policy, reset]);

  const selectedModule = authorizedModules.find(m => m.id === moduleId);

  const onFormSubmit = async (values: EditPolicyFormValues) => {
    await onSubmit({
      name: values.name.trim(),
      description: values.description?.trim() || undefined,
      moduleId: values.moduleId,
      action: values.action,
      effect: values.effect,
      roleId: values.targetType === 'Role' ? values.targetId : undefined,
      userId: values.targetType === 'User' ? values.targetId : undefined,
      rowFilterExpression: serializeRowFilterNode(values.rowFilterExpression),
      priority: values.priority,
      isActive: values.isActive,
    });
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="lg">
      <div className="space-y-4">
        <div className="flex items-center justify-between border-b border-wms-border pb-3">
          <h2 className="text-lg font-bold text-wms-text">Update Access Policy Details</h2>
          <button
            type="button"
            onClick={() => setIsGuideOpen(true)}
            className="inline-flex items-center gap-1.5 px-2.5 py-1.5 rounded-lg bg-wms-hover border border-wms-border text-xs font-semibold text-wms-secondary hover:text-wms-text hover:border-wms-indigo/40 transition cursor-pointer"
            title="Open the Policy Module Guide"
          >
            <BookOpen className="h-3.5 w-3.5" />
            Guide
          </button>
        </div>

        <form onSubmit={handleSubmit(onFormSubmit)} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-1.5">
              <label className="text-xs font-bold text-wms-secondary uppercase">Policy Name *</label>
              <input
                type="text"
                {...register('name')}
                className={`w-full rounded-lg bg-wms-hover border px-4 py-2 text-sm text-wms-text focus:border-wms-indigo outline-none transition ${
                  errors.name ? 'border-wms-danger/50 focus:border-wms-danger' : 'border-wms-border hover:border-wms-border'
                }`}
              />
              {errors.name?.message && (
                <p className="text-xs text-wms-danger mt-1">{errors.name.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-bold text-wms-secondary uppercase">Description</label>
              <input
                type="text"
                {...register('description')}
                className={`w-full rounded-lg bg-wms-hover border px-4 py-2 text-sm text-wms-text focus:border-wms-indigo outline-none transition ${
                  errors.description ? 'border-wms-danger/50 focus:border-wms-danger' : 'border-wms-border hover:border-wms-border'
                }`}
              />
              {errors.description?.message && (
                <p className="text-xs text-wms-danger mt-1">{errors.description.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-bold text-wms-secondary uppercase">Target Subject *</label>
              <select
                {...register('targetType', {
                  onChange: () => setValue('targetId', '', { shouldValidate: true })
                })}
                className="w-full rounded-lg bg-wms-hover border border-wms-border px-4 py-2 text-sm text-wms-text focus:border-wms-indigo outline-none"
              >
                <option value="Role">Security Role</option>
                <option value="User">Individual User</option>
              </select>
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-bold text-wms-secondary uppercase">Select Role/User *</label>
              <select
                {...register('targetId')}
                className={`w-full rounded-lg bg-wms-hover border px-4 py-2 text-sm text-wms-text focus:border-wms-indigo outline-none transition ${
                  errors.targetId ? 'border-wms-danger/50 focus:border-wms-danger' : 'border-wms-border hover:border-wms-border'
                }`}
              >
                <option value="">-- Choose Target --</option>
                {targetType === 'Role'
                  ? allRoles.map(r => <option key={r.id} value={r.id}>{r.name}</option>)
                  : allUsers.map(u => <option key={u.id} value={u.id}>{u.firstName} {u.lastName} ({u.email})</option>)}
              </select>
              {errors.targetId?.message && (
                <p className="text-xs text-wms-danger mt-1">{errors.targetId.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-bold text-wms-secondary uppercase">Resource Module *</label>
              <select
                {...register('moduleId', {
                  onChange: () => setValue('action', '', { shouldValidate: true })
                })}
                className={`w-full rounded-lg bg-wms-hover border px-4 py-2 text-sm text-wms-text focus:border-wms-indigo outline-none transition ${
                  errors.moduleId ? 'border-wms-danger/50 focus:border-wms-danger' : 'border-wms-border hover:border-wms-border'
                }`}
              >
                <option value="">-- Choose Module --</option>
                {authorizedModules.map(m => <option key={m.id} value={m.id}>{m.name} ({m.code})</option>)}
              </select>
              {errors.moduleId?.message && (
                <p className="text-xs text-wms-danger mt-1">{errors.moduleId.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-bold text-wms-secondary uppercase">Allowed Action *</label>
              <select
                {...register('action')}
                className={`w-full rounded-lg bg-wms-hover border px-4 py-2 text-sm text-wms-text focus:border-wms-indigo outline-none transition ${
                  errors.action ? 'border-wms-danger/50 focus:border-wms-danger' : 'border-wms-border hover:border-wms-border'
                }`}
              >
                <option value="">-- Choose Action --</option>
                {selectedModule
                  ? selectedModule.availableActions.map(act => <option key={act} value={act}>{act.toUpperCase()}</option>)
                  : ['create', 'read', 'update', 'delete'].map(act => <option key={act} value={act}>{act.toUpperCase()}</option>)}
              </select>
              {errors.action?.message && (
                <p className="text-xs text-wms-danger mt-1">{errors.action.message}</p>
              )}
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-bold text-wms-secondary uppercase">Effect *</label>
              <select
                {...register('effect')}
                className="w-full rounded-lg bg-wms-hover border border-wms-border px-4 py-2 text-sm text-wms-text focus:border-wms-indigo outline-none"
              >
                <option value="Allow">Allow access</option>
                <option value="Deny">Deny access</option>
              </select>
            </div>

            <div className="space-y-1.5">
              <label className="text-xs font-bold text-wms-secondary uppercase flex items-center gap-1">
                Evaluation Priority (Weight) *
                <span title="Determines evaluation order (lowest evaluates first). Explicit Deny rules override Allow. If multiple policies apply to the same field, priority decides which column restriction (Hide > Mask > ReadOnly) is chosen.">
                  <HelpCircle className="h-3.5 w-3.5 text-wms-muted cursor-help" />
                </span>
              </label>
              <input
                type="number"
                {...register('priority', { valueAsNumber: true })}
                className={`w-full rounded-lg bg-wms-hover border px-4 py-2 text-sm text-wms-text focus:border-wms-indigo outline-none transition ${
                  errors.priority ? 'border-wms-danger/50 focus:border-wms-danger' : 'border-wms-border hover:border-wms-border'
                }`}
              />
              {errors.priority?.message && (
                <p className="text-xs text-wms-danger mt-1">{errors.priority.message}</p>
              )}
              <span className="text-[11px] text-wms-muted block mt-1 leading-normal">
                Lowest number evaluates first (1-1000). Explicit <strong>Deny</strong> overrides <strong>Allow</strong>. Priority resolves conflicts and merges column field restrictions.
              </span>
            </div>

            <div className="space-y-1.5 flex items-center gap-3 mt-4">
              <div className="space-y-1">
                <label className="text-xs font-bold text-wms-secondary uppercase block">Is Active *</label>
                <Switch
                  checked={isActive}
                  onChange={(val) => setValue('isActive', val, { shouldValidate: true })}
                  ariaLabel="Toggle active"
                />
              </div>
              <span className="text-sm font-semibold text-wms-text mt-4">
                {isActive ? 'Active / Enabled' : 'Disabled'}
              </span>
            </div>
          </div>

          {/* Row Filter Tree Builder */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <label className="text-xs font-bold text-wms-secondary uppercase flex items-center gap-1">
                Row-Level Filter (AND/OR Expression Tree)
                <span title="Allows narrowing down the table records a user can view (e.g. only records belonging to the user's department, or owned by their org-chart subordinates).">
                  <HelpCircle className="h-3.5 w-3.5 text-wms-muted cursor-help" />
                </span>
              </label>
              <button
                type="button"
                onClick={() => setIsGuideOpen(true)}
                className="inline-flex items-center gap-1 text-[11px] font-semibold text-wms-indigo hover:underline cursor-pointer"
              >
                <BookOpen className="h-3 w-3" />
                How do I write this?
              </button>
            </div>
            <Controller
              control={control}
              name="rowFilterExpression"
              render={({ field }) => (
                <RowFilterTreeBuilder
                  value={field.value ?? createEmptyGroup()}
                  onChange={field.onChange}
                  moduleCode={selectedModule?.code}
                />
              )}
            />
          </div>

          <div className="flex justify-end gap-2 border-t border-wms-border pt-4">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 rounded-lg bg-wms-hover border border-wms-border text-sm font-semibold text-wms-text hover:bg-wms-hover cursor-pointer"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting || !isValid}
              className="px-5 py-2 rounded-lg bg-wms-indigo text-white text-sm font-semibold hover:bg-indigo-700 cursor-pointer shadow-lg shadow-wms-indigo/25 disabled:opacity-50"
            >
              {isSubmitting ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      </div>

      <PolicyGuideModal
        isOpen={isGuideOpen}
        onClose={() => setIsGuideOpen(false)}
        initialSectionHeading={GUIDE_HEADINGS.rowFilter}
      />
    </Modal>
  );
}
