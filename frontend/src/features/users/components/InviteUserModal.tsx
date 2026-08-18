import { useState, useEffect, useMemo } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Eye, EyeOff } from 'lucide-react';

import { Modal } from '@/components/ui/Modal/Modal';
import { Input } from '@/components/ui/Input/Input';
import { Select } from '@/components/ui/Select/Select';
import { Spinner } from '@/components/ui/Spinner/Spinner';
import { toast } from '@/components/ui/Toast/Toast';
import { inviteUser } from '@/api/usersApi';
import { useInviteMeta } from '@/features/users/hooks/useInviteMeta';
import { useEligibleManagers } from '@/features/users/hooks/useEligibleManagers';
import { useEligibleReportingOfficers } from '@/features/users/hooks/useEligibleReportingOfficers';
import { resolveHierarchyTier } from '@/features/users/utils/hierarchy';
import { inviteUserSchema, type InviteUserFormValues } from '@/features/users/schemas/inviteUser.schema';
import { RoleCheckboxGroup } from './RoleCheckboxGroup';

interface InviteUserModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export function InviteUserModal({ isOpen, onClose, onSuccess }: InviteUserModalProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [showPassword, setShowPassword] = useState(false);

  const { data: metaData, isLoading: isLoadingMeta, error: metaError } = useInviteMeta(isOpen);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    setError,
    control,
    reset,
    formState: { errors },
  } = useForm<InviteUserFormValues>({
    resolver: zodResolver(inviteUserSchema),
    defaultValues: {
      employeeCode: '',
      firstName: '',
      lastName: '',
      email: '',
      username: '',
      password: '',
      phoneNumber: '',
      departmentId: '',
      designationId: '',
      roleIds: [],
      managerId: '',
      reportingOfficerId: '',
    },
  });

  const selectedDepartmentId = watch('departmentId');
  const selectedDesignationId = watch('designationId');
  const selectedManagerId = watch('managerId');

  // Cascade resets when a parent selection changes.
  useEffect(() => {
    setValue('designationId', '');
    setValue('managerId', '');
    setValue('reportingOfficerId', '');
  }, [selectedDepartmentId, setValue]);

  useEffect(() => {
    setValue('managerId', '');
    setValue('reportingOfficerId', '');
  }, [selectedDesignationId, setValue]);

  useEffect(() => {
    setValue('reportingOfficerId', '');
  }, [selectedManagerId, setValue]);

  const filteredDesignations = useMemo(
    () => metaData?.designations.filter((d) => d.departmentId === selectedDepartmentId) ?? [],
    [metaData, selectedDepartmentId],
  );

  const tier = useMemo(
    () => resolveHierarchyTier(metaData, selectedDepartmentId, selectedDesignationId),
    [metaData, selectedDepartmentId, selectedDesignationId],
  );

  const { data: eligibleManagers = [], isLoading: isLoadingManagers } = useEligibleManagers(
    selectedDepartmentId,
    tier.needsManager && isOpen,
  );

  const { data: eligibleReportingOfficers = [], isLoading: isLoadingReportingOfficers } =
    useEligibleReportingOfficers(
      selectedDepartmentId,
      tier.designationLevel,
      selectedManagerId || '',
      tier.needsManager && !!selectedManagerId && isOpen,
    );

  const onSubmit = async (values: InviteUserFormValues) => {
    if (tier.needsManager && !values.managerId) {
      setError('managerId', { type: 'manual', message: 'Manager is required' });
      return;
    }
    if (tier.needsReportingOfficer && !values.reportingOfficerId) {
      setError('reportingOfficerId', { type: 'manual', message: 'Reporting officer is required' });
      return;
    }

    setIsSubmitting(true);
    try {
      const payload = {
        ...values,
        phoneNumber: values.phoneNumber || undefined,
        managerId: values.managerId || undefined,
        reportingOfficerId: values.reportingOfficerId || undefined,
      };

      const response = await inviteUser(payload);
      if (response.succeeded) {
        toast.success(response.message || 'User invited successfully.');
        reset();
        onSuccess();
        onClose();
      } else {
        toast.error(response.message || 'Failed to invite user.');
      }
    } catch (err: any) {
      const msg = err?.response?.data?.message || err.message || 'An error occurred during submission.';
      toast.error(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Invite New Employee" size="xl">
      {isLoadingMeta ? (
        <div className="flex flex-col items-center justify-center py-16 gap-3">
          <Spinner size="lg" />
          <p className="text-sm text-wms-secondary">Loading department and role options...</p>
        </div>
      ) : metaError ? (
        <div className="text-center py-8">
          <p className="text-sm text-wms-danger font-medium">Failed to load registration options</p>
          <p className="text-xs text-wms-muted mt-1">Please close the modal and try again.</p>
        </div>
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input label="Employee Code" placeholder="e.g. EMP-005" error={errors.employeeCode?.message} {...register('employeeCode')} id="invite-employee-code" />
            <Input label="Username" placeholder="e.g. johndoe" error={errors.username?.message} {...register('username')} id="invite-username" />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input label="First Name" placeholder="e.g. John" error={errors.firstName?.message} {...register('firstName')} id="invite-first-name" />
            <Input label="Last Name" placeholder="e.g. Doe" error={errors.lastName?.message} {...register('lastName')} id="invite-last-name" />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input label="Email Address" type="email" placeholder="e.g. john.doe@wms.com" error={errors.email?.message} {...register('email')} id="invite-email" />
            <Input
              label="Password"
              type={showPassword ? 'text' : 'password'}
              placeholder="Min 8 chars, 1 upper, 1 lower, 1 num, 1 special"
              error={errors.password?.message}
              {...register('password')}
              id="invite-password"
              rightIcon={
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="text-wms-muted hover:text-wms-text transition cursor-pointer"
                  tabIndex={-1}
                >
                  {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                </button>
              }
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input label="Phone Number (Optional)" placeholder="e.g. +1234567890" error={errors.phoneNumber?.message} {...register('phoneNumber')} id="invite-phone" />
            <Select
              label="Department"
              options={[
                { label: 'Select Department', value: '', disabled: true },
                ...(metaData?.departments.map((d) => ({ label: d.name, value: d.id })) || []),
              ]}
              error={errors.departmentId?.message}
              {...register('departmentId')}
              id="invite-department"
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Select
              label="Designation"
              options={[
                { label: 'Select Designation', value: '', disabled: true },
                ...filteredDesignations.map((d) => ({ label: d.name, value: d.id })),
              ]}
              disabled={!selectedDepartmentId}
              error={errors.designationId?.message}
              {...register('designationId')}
              id="invite-designation"
            />
            {tier.needsManager && (
              <Select
                label="Manager (Department Head) *"
                options={[
                  { label: 'Select Manager', value: '', disabled: true },
                  ...eligibleManagers.map((m) => ({
                    label: `${m.firstName} ${m.lastName} (${m.designationName})`,
                    value: m.id,
                  })),
                ]}
                disabled={isLoadingManagers || eligibleManagers.length === 0}
                error={errors.managerId?.message}
                {...register('managerId')}
                id="invite-manager"
              />
            )}
          </div>

          {tier.needsReportingOfficer && selectedManagerId && (
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Select
                label="Reporting Officer / Team Lead *"
                options={[
                  { label: 'Select Reporting Officer', value: '', disabled: true },
                  ...eligibleReportingOfficers.map((ro) => ({
                    label: `${ro.firstName} ${ro.lastName} (${ro.designationName})`,
                    value: ro.id,
                  })),
                ]}
                disabled={isLoadingReportingOfficers || eligibleReportingOfficers.length === 0}
                error={errors.reportingOfficerId?.message}
                {...register('reportingOfficerId')}
                id="invite-reporting-officer"
              />
            </div>
          )}

          <Controller
            control={control}
            name="roleIds"
            render={({ field }) => (
              <RoleCheckboxGroup
                roles={metaData?.roles ?? []}
                value={field.value ?? []}
                onChange={field.onChange}
                error={errors.roleIds?.message}
              />
            )}
          />

          <div className="flex justify-end gap-3 pt-4 border-t border-wms-border">
            <button
              type="button"
              onClick={onClose}
              className="px-5 py-2.5 rounded-lg bg-wms-hover border border-wms-border text-sm font-semibold text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg bg-wms-indigo hover:bg-indigo-700 disabled:bg-wms-indigo/50 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-indigo/25"
            >
              {isSubmitting ? 'Inviting...' : 'Invite Employee'}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
