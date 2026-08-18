import { useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import type { DepartmentListItem } from '../types/department.types';

interface DeleteDepartmentDialogProps {
  department: DepartmentListItem;
  onConfirm: (id: string) => Promise<{ succeeded: boolean; message?: string }>;
  onCancel: () => void;
}

export function DeleteDepartmentDialog({ department, onConfirm, onCancel }: DeleteDepartmentDialogProps) {
  const [isDeleting, setIsDeleting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const handleDelete = async () => {
    setIsDeleting(true);
    setError(null);
    const result = await onConfirm(department.id);
    if (!result.succeeded) {
      setError(result.message || 'Failed to delete department.');
      setIsDeleting(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
      <div className="w-full max-w-md bg-wms-surface border border-wms-border rounded-2xl p-6 shadow-2xl space-y-4">
        <div className="flex items-center gap-3 text-wms-danger">
          <div className="p-2.5 rounded-xl bg-wms-danger/10 border border-wms-danger/20">
            <AlertTriangle className="h-6 w-6" />
          </div>
          <h3 className="text-lg font-bold text-wms-text">Confirm Deletion</h3>
        </div>

        <p className="text-sm text-wms-secondary leading-relaxed">
          Are you sure you want to delete department{' '}
          <strong className="text-wms-text">
            {department.name} ({department.code})
          </strong>
          ?
        </p>

        {error && (
          <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 rounded-lg text-xs text-wms-danger">
            {error}
          </div>
        )}

        <div className="flex items-center justify-end gap-3 pt-3 border-t border-wms-border">
          <button
            type="button"
            onClick={onCancel}
            disabled={isDeleting}
            className="px-4 py-2 rounded-lg bg-wms-hover border border-wms-border text-sm font-medium text-wms-secondary hover:text-wms-text transition cursor-pointer"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={handleDelete}
            disabled={isDeleting}
            className="px-5 py-2 rounded-lg bg-wms-danger hover:bg-red-700 text-white text-sm font-semibold transition cursor-pointer disabled:opacity-50"
          >
            {isDeleting ? 'Deleting...' : 'Delete Department'}
          </button>
        </div>
      </div>
    </div>
  );
}
