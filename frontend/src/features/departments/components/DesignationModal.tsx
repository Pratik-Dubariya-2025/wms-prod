import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Award, X } from 'lucide-react';

import { Modal } from '@/components/ui/Modal/Modal';
import { Spinner } from '@/components/ui/Spinner/Spinner';
import { createDesignation, updateDesignation } from '@/api/departmentApi';
import type { DepartmentDesignation } from '../types/department.types';

const designationSchema = z.object({
  name: z.string().min(1, 'Name is required').max(100, 'Name must not exceed 100 characters'),
  code: z
    .string()
    .min(1, 'Code is required')
    .max(20, 'Code must not exceed 20 characters')
    .regex(/^[a-zA-Z0-9_-]+$/, 'Code can only contain letters, numbers, hyphens, and underscores'),
  description: z.string().optional(),
  level: z.coerce.number().min(1, 'Level must be at least 1'),
  isActive: z.boolean().default(true),
});

type DesignationFormValues = z.infer<typeof designationSchema>;

interface DesignationModalProps {
  isOpen: boolean;
  departmentId: string;
  designationToEdit?: DepartmentDesignation | null;
  onClose: () => void;
  onSuccess: () => void;
}

export function DesignationModal({
  isOpen,
  departmentId,
  designationToEdit,
  onClose,
  onSuccess,
}: DesignationModalProps) {
  const isEditing = Boolean(designationToEdit);

  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<DesignationFormValues>({
    resolver: zodResolver(designationSchema) as any,
    defaultValues: {
      name: '',
      code: '',
      description: '',
      level: 1,
      isActive: true,
    },
  });

  useEffect(() => {
    if (designationToEdit) {
      reset({
        name: designationToEdit.name,
        code: designationToEdit.code || '',
        description: designationToEdit.description || '',
        level: designationToEdit.level || 1,
        isActive: designationToEdit.isActive,
      });
    } else {
      reset({
        name: '',
        code: '',
        description: '',
        level: 1,
        isActive: true,
      });
    }
  }, [designationToEdit, reset, isOpen]);

  const onSubmit = async (values: DesignationFormValues) => {
    try {
      if (isEditing && designationToEdit) {
        const response = await updateDesignation(designationToEdit.id, {
          name: values.name.trim(),
          code: values.code.trim().toUpperCase(),
          description: values.description?.trim(),
          level: values.level,
          isActive: values.isActive,
        });

        if (response.succeeded) {
          onSuccess();
          onClose();
        } else {
          setError('root', { message: response.message || 'Failed to update designation.' });
        }
      } else {
        const response = await createDesignation(departmentId, {
          name: values.name.trim(),
          code: values.code.trim().toUpperCase(),
          description: values.description?.trim(),
          level: values.level,
        });

        if (response.succeeded) {
          onSuccess();
          onClose();
        } else {
          setError('root', { message: response.message || 'Failed to create designation.' });
        }
      }
    } catch (err: unknown) {
      setError('root', {
        message: err instanceof Error ? err.message : 'An error occurred while saving designation.',
      });
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} size="md">
      <div className="space-y-5">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-wms-border pb-4">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-xl bg-amber-500/10 text-amber-500">
              <Award className="h-5 w-5" />
            </div>
            <div>
              <h2 className="text-lg font-bold text-wms-text">
                {isEditing ? 'Edit Designation' : 'Add Designation'}
              </h2>
              <p className="text-xs text-wms-muted">
                {isEditing ? 'Modify designation title and level' : 'Create a new designation level for this department'}
              </p>
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-1.5 rounded-lg text-wms-muted hover:text-wms-text hover:bg-wms-hover transition cursor-pointer"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* Form Error */}
        {errors.root && (
          <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 rounded-lg text-xs text-wms-danger">
            {errors.root.message}
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Name & Code */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold text-wms-secondary uppercase tracking-wider mb-1.5">
                Designation Name <span className="text-wms-danger">*</span>
              </label>
              <input
                type="text"
                placeholder="e.g. Senior Software Engineer"
                {...register('name')}
                className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text placeholder:text-wms-muted px-3.5 py-2 transition outline-none focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
              />
              {errors.name && (
                <p className="text-xs text-wms-danger mt-1">{errors.name.message}</p>
              )}
            </div>

            <div>
              <label className="block text-xs font-semibold text-wms-secondary uppercase tracking-wider mb-1.5">
                Code <span className="text-wms-danger">*</span>
              </label>
              <input
                type="text"
                placeholder="e.g. SSE"
                {...register('code')}
                className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text uppercase placeholder:normal-case placeholder:text-wms-muted px-3.5 py-2 transition outline-none focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
              />
              {errors.code && (
                <p className="text-xs text-wms-danger mt-1">{errors.code.message}</p>
              )}
            </div>
          </div>

          {/* Level */}
          <div>
            <label className="block text-xs font-semibold text-wms-secondary uppercase tracking-wider mb-1.5">
              Hierarchy Level <span className="text-wms-danger">*</span>
            </label>
            <input
              type="number"
              min={1}
              placeholder="1"
              {...register('level')}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text px-3.5 py-2 transition outline-none focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20"
            />
            <p className="text-[11px] text-wms-muted mt-1">
              Lower number indicates higher seniority level (e.g. Level 1 = Manager/Lead).
            </p>
            {errors.level && (
              <p className="text-xs text-wms-danger mt-1">{errors.level.message}</p>
            )}
          </div>

          {/* Description */}
          <div>
            <label className="block text-xs font-semibold text-wms-secondary uppercase tracking-wider mb-1.5">
              Description
            </label>
            <textarea
              rows={2}
              placeholder="Brief description of designation responsibilities..."
              {...register('description')}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text placeholder:text-wms-muted px-3.5 py-2 transition outline-none focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20 resize-none"
            />
          </div>

          {/* IsActive (when editing) */}
          {isEditing && (
            <div className="flex items-center gap-2 pt-1">
              <input
                type="checkbox"
                id="designation-is-active"
                {...register('isActive')}
                className="rounded border-wms-border text-wms-indigo focus:ring-wms-indigo h-4 w-4"
              />
              <label htmlFor="designation-is-active" className="text-sm font-medium text-wms-text cursor-pointer">
                Active Designation
              </label>
            </div>
          )}

          {/* Footer buttons */}
          <div className="flex items-center justify-end gap-3 pt-4 border-t border-wms-border">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="px-4 py-2 rounded-lg bg-wms-hover border border-wms-border text-sm font-medium text-wms-secondary hover:text-wms-text transition cursor-pointer"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="inline-flex items-center gap-2 px-5 py-2 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition cursor-pointer disabled:opacity-50"
            >
              {isSubmitting && <Spinner size="sm" />}
              {isEditing ? 'Save Changes' : 'Create Designation'}
            </button>
          </div>
        </form>
      </div>
    </Modal>
  );
}
