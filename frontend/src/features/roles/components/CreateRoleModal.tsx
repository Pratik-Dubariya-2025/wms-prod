import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Modal } from '@/components/ui/Modal/Modal';
import { Input } from '@/components/ui/Input/Input';
import { createRole } from '@/api/rolesApi';
import { createRoleSchema, type CreateRoleFormValues } from '../schemas/createRole.schema';

interface CreateRoleModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export function CreateRoleModal({ isOpen, onClose, onSuccess }: CreateRoleModalProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isValid },
  } = useForm<CreateRoleFormValues>({
    resolver: zodResolver(createRoleSchema),
    mode: 'onChange',
    defaultValues: {
      name: '',
      code: '',
      priority: 100,
      description: '',
    },
  });

  const onSubmit = async (values: CreateRoleFormValues) => {
    setIsSubmitting(true);
    setError(null);
    try {
      const response = await createRole({
        name: values.name.trim(),
        code: values.code.trim().toUpperCase(),
        priority: Number(values.priority),
        description: values.description?.trim() || undefined,
      });

      if (response.succeeded) {
        reset();
        onSuccess();
        onClose();
      } else {
        setError(response.message || 'Failed to create role');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred');
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleClose = () => {
    reset();
    setError(null);
    onClose();
  };

  return (
    <Modal isOpen={isOpen} onClose={handleClose} title="Create Custom Role" size="md">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {error && (
          <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 rounded-lg text-sm text-wms-danger">
            {error}
          </div>
        )}

        <Input
          label="Role Name"
          placeholder="e.g. WAREHOUSE_LEAD"
          error={errors.name?.message}
          {...register('name')}
          id="role-name-input"
        />

        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <Input
            label="Role Code"
            placeholder="e.g. WH_LEAD"
            error={errors.code?.message}
            {...register('code', {
              onChange: (e) => {
                e.target.value = e.target.value.toUpperCase();
              }
            })}
            id="role-code-input"
          />

          <Input
            label="Priority (1-100)"
            type="number"
            placeholder="100"
            error={errors.priority?.message}
            {...register('priority', { valueAsNumber: true })}
            id="role-priority-input"
          />
        </div>

        <div className="space-y-1.5">
          <label className="text-xs font-semibold text-wms-muted uppercase tracking-wider">
            Description
          </label>
          <textarea
            placeholder="Provide a clear description of what users with this role can do..."
            {...register('description')}
            className={`w-full h-24 rounded-lg bg-wms-hover border px-4 py-2.5 text-sm text-wms-text placeholder:text-wms-muted focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20 outline-none transition resize-none ${
              errors.description ? 'border-wms-danger/50 focus:border-wms-danger focus:ring-wms-danger/20' : 'border-wms-border'
            }`}
            id="role-description-input"
          />
          {errors.description?.message && (
            <p className="text-xs text-wms-danger">{errors.description.message}</p>
          )}
        </div>

        <div className="flex justify-end gap-3 pt-3 border-t border-wms-border">
          <button
            type="button"
            onClick={handleClose}
            disabled={isSubmitting}
            className="px-4 py-2 rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={isSubmitting || !isValid}
            className="px-5 py-2 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition duration-200 cursor-pointer disabled:opacity-50"
          >
            {isSubmitting ? 'Creating...' : 'Create Role'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
