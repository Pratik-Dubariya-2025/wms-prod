import { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Modal } from '@/components/ui/Modal/Modal';
import { Input } from '@/components/ui/Input/Input';
import { createDepartment, updateDepartment } from '@/api/departmentApi';
import type { DepartmentListItem } from '../types/department.types';

const departmentSchema = z.object({
  name: z.string().min(1, 'Department name is required').max(150, 'Name must not exceed 150 characters'),
  code: z
    .string()
    .min(1, 'Department code is required')
    .max(50, 'Code must not exceed 50 characters')
    .regex(/^[a-zA-Z0-9_-]+$/, 'Code can only contain letters, numbers, hyphens, and underscores'),
  description: z.string().max(500, 'Description must not exceed 500 characters').optional(),
  isActive: z.boolean().default(true),
});

type DepartmentFormValues = z.infer<typeof departmentSchema>;

interface DepartmentModalProps {
  isOpen: boolean;
  departmentToEdit?: DepartmentListItem | null;
  onClose: () => void;
  onSuccess: () => void;
}

export function DepartmentModal({ isOpen, departmentToEdit, onClose, onSuccess }: DepartmentModalProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const isEditing = Boolean(departmentToEdit);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isValid },
  } = useForm<DepartmentFormValues>({
    resolver: zodResolver(departmentSchema) as any,
    mode: 'onChange',
    defaultValues: {
      name: '',
      code: '',
      description: '',
      isActive: true,
    },
  });

  useEffect(() => {
    if (departmentToEdit) {
      reset({
        name: departmentToEdit.name,
        code: departmentToEdit.code,
        description: departmentToEdit.description || '',
        isActive: departmentToEdit.isActive,
      });
    } else {
      reset({
        name: '',
        code: '',
        description: '',
        isActive: true,
      });
    }
  }, [departmentToEdit, reset]);

  const onSubmit = async (values: DepartmentFormValues) => {
    setIsSubmitting(true);
    setError(null);
    try {
      if (isEditing && departmentToEdit) {
        const response = await updateDepartment(departmentToEdit.id, {
          name: values.name.trim(),
          code: values.code.trim().toUpperCase(),
          description: values.description?.trim() || undefined,
          isActive: values.isActive,
        });

        if (response.succeeded) {
          reset();
          onSuccess();
          onClose();
        } else {
          setError(response.message || 'Failed to update department');
        }
      } else {
        const response = await createDepartment({
          name: values.name.trim(),
          code: values.code.trim().toUpperCase(),
          description: values.description?.trim() || undefined,
        });

        if (response.succeeded) {
          reset();
          onSuccess();
          onClose();
        } else {
          setError(response.message || 'Failed to create department');
        }
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
    <Modal
      isOpen={isOpen}
      onClose={handleClose}
      title={isEditing ? 'Edit Department' : 'Create Department'}
      size="md"
    >
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {error && (
          <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 rounded-lg text-sm text-wms-danger">
            {error}
          </div>
        )}

        <Input
          label="Department Name"
          placeholder="e.g. Engineering, Sales, Human Resources"
          error={errors.name?.message}
          {...register('name')}
          id="dept-name-input"
        />

        <Input
          label="Department Code"
          placeholder="e.g. ENG, SALES, HR"
          error={errors.code?.message}
          {...register('code', {
            onChange: (e) => {
              e.target.value = e.target.value.toUpperCase();
            },
          })}
          id="dept-code-input"
        />

        <div className="space-y-1.5">
          <label className="text-xs font-semibold text-wms-muted uppercase tracking-wider">
            Description
          </label>
          <textarea
            placeholder="Brief description of department responsibilities..."
            {...register('description')}
            className={`w-full h-24 rounded-lg bg-wms-hover border px-4 py-2.5 text-sm text-wms-text placeholder:text-wms-muted focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20 outline-none transition resize-none ${
              errors.description ? 'border-wms-danger/50 focus:border-wms-danger focus:ring-wms-danger/20' : 'border-wms-border'
            }`}
            id="dept-description-input"
          />
          {errors.description?.message && (
            <p className="text-xs text-wms-danger">{errors.description.message}</p>
          )}
        </div>

        {isEditing && (
          <div className="flex items-center gap-2 pt-1">
            <input
              type="checkbox"
              id="dept-active-toggle"
              {...register('isActive')}
              className="h-4 w-4 rounded border-wms-border text-wms-indigo focus:ring-wms-indigo bg-wms-hover cursor-pointer"
            />
            <label htmlFor="dept-active-toggle" className="text-sm font-medium text-wms-text cursor-pointer select-none">
              Active Status
            </label>
          </div>
        )}

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
            {isSubmitting
              ? isEditing ? 'Saving...' : 'Creating...'
              : isEditing ? 'Save Changes' : 'Create Department'}
          </button>
        </div>
      </form>
    </Modal>
  );
}
