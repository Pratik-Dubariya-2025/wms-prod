import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';

import { Modal } from '@/components/ui/Modal/Modal';
import { Input } from '@/components/ui/Input/Input';
import { Select } from '@/components/ui/Select/Select';
import { updateTask } from '@/api/tasksApi';
import { useTaskCreateMeta } from '@/features/tasks/hooks/useTaskCreateMeta';
import { toast } from '@/components/ui/Toast/Toast';
import { Spinner } from '@/components/ui/Spinner/Spinner';
import type { TaskDetail } from '@/features/tasks/types/task.types';

const editTaskSchema = z.object({
  title: z.string().min(1, 'Title is required').max(500, 'Max 500 characters'),
  description: z.string().max(5000, 'Max 5000 characters').optional().or(z.literal('')),
  status: z.string().min(1, 'Status is required'),
  priority: z.string().min(1, 'Priority is required'),
  estimatedHours: z.string().optional().or(z.literal('')),
  dueDate: z.string().optional().or(z.literal('')),
  assigneeId: z.string().optional().or(z.literal('')),
});

type EditTaskFormValues = z.infer<typeof editTaskSchema>;

interface ProjectEditTaskModalProps {
  isOpen: boolean;
  onClose: () => void;
  task: TaskDetail | null;
  onSuccess: () => void;
  projectId: string;
}

export function ProjectEditTaskModal({ isOpen, onClose, task, onSuccess, projectId }: ProjectEditTaskModalProps) {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { meta, isLoading: isLoadingMeta, error: metaError } = useTaskCreateMeta(projectId);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<EditTaskFormValues>({
    resolver: zodResolver(editTaskSchema),
  });

  // Reset/populate form when task is loaded
  useEffect(() => {
    if (task) {
      // Format date to YYYY-MM-DD for date inputs
      let formattedDate = '';
      if (task.dueDate) {
        const dateObj = new Date(task.dueDate);
        if (!isNaN(dateObj.getTime())) {
          formattedDate = dateObj.toISOString().split('T')[0];
        }
      }

      reset({
        title: task.title,
        description: task.description || '',
        status: task.status,
        priority: task.priority,
        estimatedHours: task.estimatedHours != null ? String(task.estimatedHours) : '',
        dueDate: formattedDate,
        assigneeId: task.assigneeId || '',
      });
    }
  }, [task, reset]);

  const onSubmit = async (values: EditTaskFormValues) => {
    if (!task) return;
    setIsSubmitting(true);
    try {
      const payload = {
        title: values.title,
        description: values.description || undefined,
        status: values.status,
        priority: values.priority,
        estimatedHours: values.estimatedHours ? parseFloat(values.estimatedHours) : null,
        dueDate: values.dueDate || null,
        projectId: projectId,
        assigneeId: values.assigneeId || null,
      };

      const response = await updateTask(projectId, task.id, payload);
      if (response.succeeded) {
        toast.success(response.message || 'Task updated successfully.');
        onSuccess();
        onClose();
      } else {
        toast.error(response.message || 'Failed to update task.');
      }
    } catch (err: any) {
      const msg = err?.response?.data?.message || err.message || 'An error occurred during submission.';
      toast.error(msg);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Edit Task" size="xl">
      {isLoadingMeta ? (
        <div className="flex flex-col items-center justify-center py-16 gap-3">
          <Spinner size="lg" />
          <p className="text-sm text-wms-text/50">Loading assignable users...</p>
        </div>
      ) : metaError ? (
        <div className="text-center py-8">
          <p className="text-sm text-red-400 font-medium">Failed to load edit options</p>
          <p className="text-xs text-wms-text/40 mt-1">Please close the modal and try again.</p>
        </div>
      ) : (
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <Input
            label="Task Title"
            placeholder="e.g. Implement user authentication flow"
            error={errors.title?.message}
            {...register('title')}
            id="edit-task-title"
          />

          <div className="space-y-1.5">
            <label className="text-xs font-semibold text-wms-text/50 uppercase tracking-wide">
              Description (Optional)
            </label>
            <textarea
              placeholder="Describe the task requirements, acceptance criteria, etc."
              {...register('description')}
              rows={3}
              className="w-full rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-text placeholder:text-wms-text/20
                px-4 py-2.5 transition duration-200 outline-none resize-none
                focus:border-wms-indigo focus:ring-2 focus:ring-wms-indigo/20 hover:border-wms-border"
              id="edit-task-description"
            />
            {errors.description?.message && (
              <p className="text-xs text-red-400 mt-1">{errors.description.message}</p>
            )}
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Select
              label="Status"
              options={[
                { label: 'Draft', value: 'Draft' },
                { label: 'In Progress', value: 'InProgress' },
                { label: 'In Review', value: 'InReview' },
                { label: 'Done', value: 'Done' },
                { label: 'Closed', value: 'Closed' },
              ]}
              error={errors.status?.message}
              {...register('status')}
              id="edit-task-status"
            />

            <Select
              label="Priority"
              options={[
                { label: 'Low', value: 'Low' },
                { label: 'Medium', value: 'Medium' },
                { label: 'High', value: 'High' },
                { label: 'Critical', value: 'Critical' },
              ]}
              error={errors.priority?.message}
              {...register('priority')}
              id="edit-task-priority"
            />
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              label="Estimated Hours"
              type="number"
              step="0.5"
              min="0"
              placeholder="e.g. 8"
              error={errors.estimatedHours?.message}
              {...register('estimatedHours')}
              id="edit-task-hours"
            />
            <Input
              label="Due Date"
              type="date"
              error={errors.dueDate?.message}
              {...register('dueDate')}
              id="edit-task-due-date"
            />
          </div>

          <Select
            label="Assignee (Optional)"
            options={[
              { label: 'Unassigned', value: '' },
              ...(meta?.users.map((u) => ({ label: u.fullName, value: u.id })) || []),
            ]}
            error={errors.assigneeId?.message}
            {...register('assigneeId')}
            id="edit-task-assignee"
          />

          <div className="flex justify-end gap-3 pt-4 border-t border-wms-border">
            <button
              type="button"
              onClick={onClose}
              className="px-5 py-2.5 rounded-lg bg-wms-hover border border-wms-border text-sm font-semibold text-wms-text/70 hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="inline-flex items-center justify-center px-5 py-2.5 rounded-lg bg-wms-indigo hover:bg-indigo-700 disabled:bg-wms-indigo/50 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-indigo/25"
            >
              {isSubmitting ? 'Saving...' : 'Save Changes'}
            </button>
          </div>
        </form>
      )}
    </Modal>
  );
}
