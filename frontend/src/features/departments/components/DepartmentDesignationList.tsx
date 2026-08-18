import { useState } from 'react';
import { Plus, Edit2, Trash2, AlertTriangle } from 'lucide-react';
import type { DepartmentDesignation } from '../types/department.types';
import { deleteDesignation } from '@/api/departmentApi';
import { PERMISSIONS } from '@/constants/permissions';
import { PermissionGate } from '@/components/shared/PermissionGate';
import { DesignationModal } from './DesignationModal';

interface DepartmentDesignationListProps {
  departmentId: string;
  designations: DepartmentDesignation[];
  onRefresh: () => void;
}

export function DepartmentDesignationList({
  departmentId,
  designations,
  onRefresh,
}: DepartmentDesignationListProps) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [designationToEdit, setDesignationToEdit] = useState<DepartmentDesignation | null>(null);

  // Deletion State
  const [designationToDelete, setDesignationToDelete] = useState<DepartmentDesignation | null>(null);
  const [isDeleting, setIsDeleting] = useState(false);
  const [deleteError, setDeleteError] = useState<string | null>(null);

  const handleOpenAdd = () => {
    setDesignationToEdit(null);
    setIsModalOpen(true);
  };

  const handleOpenEdit = (des: DepartmentDesignation) => {
    setDesignationToEdit(des);
    setIsModalOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!designationToDelete) return;
    setIsDeleting(true);
    setDeleteError(null);
    try {
      const response = await deleteDesignation(designationToDelete.id);
      if (response.succeeded) {
        setDesignationToDelete(null);
        onRefresh();
      } else {
        setDeleteError(response.message || 'Failed to delete designation.');
      }
    } catch (err: unknown) {
      setDeleteError(err instanceof Error ? err.message : 'An error occurred while deleting designation.');
    } finally {
      setIsDeleting(false);
    }
  };

  return (
    <div className="space-y-4">
      {/* Header bar with Add Designation button */}
      <div className="flex items-center justify-between pb-1">
        <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider">
          Department Designations ({designations.length})
        </span>
        <PermissionGate permissions={PERMISSIONS.DEPARTMENT_CREATE}>
          <button
            onClick={handleOpenAdd}
            className="inline-flex items-center gap-1.5 text-xs font-semibold px-3 py-1.5 rounded-lg bg-wms-indigo/10 text-wms-indigo hover:bg-wms-indigo hover:text-white transition cursor-pointer"
          >
            <Plus className="h-3.5 w-3.5" />
            Add Designation
          </button>
        </PermissionGate>
      </div>

      {designations.length === 0 ? (
        <div className="text-center py-8 text-wms-muted text-sm border border-dashed border-wms-border rounded-xl space-y-3">
          <p>No designations defined under this department yet.</p>
          <PermissionGate permissions={PERMISSIONS.DEPARTMENT_CREATE}>
            <button
              onClick={handleOpenAdd}
              className="inline-flex items-center gap-1.5 text-xs font-semibold px-3 py-1.5 rounded-lg bg-wms-indigo text-white hover:bg-indigo-700 transition cursor-pointer"
            >
              <Plus className="h-3.5 w-3.5" />
              Create First Designation
            </button>
          </PermissionGate>
        </div>
      ) : (
        <div className="space-y-3">
          {designations.map((des) => (
            <div
              key={des.id}
              className="p-3.5 rounded-xl bg-wms-hover/50 border border-wms-border flex items-center justify-between hover:border-wms-indigo/30 transition group"
            >
              <div className="flex items-center gap-3">
                <div className="h-9 w-9 rounded-lg bg-wms-indigo/10 text-wms-indigo font-bold text-xs flex items-center justify-center">
                  L{des.level}
                </div>
                <div>
                  <div className="flex items-center gap-2">
                    <span className="font-semibold text-wms-text text-sm">{des.name}</span>
                    {des.code && (
                      <span className="text-[11px] font-mono font-semibold px-1.5 py-0.5 rounded bg-wms-hover text-wms-secondary border border-wms-border">
                        {des.code}
                      </span>
                    )}
                  </div>
                  <span className="text-xs text-wms-muted">
                    Hierarchy Level {des.level}
                    {des.description ? ` • ${des.description}` : ''}
                  </span>
                </div>
              </div>

              <div className="flex items-center gap-3">
                <span
                  className={`text-xs px-2.5 py-1 rounded-full font-semibold border ${
                    des.isActive
                      ? 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400 border-emerald-500/20'
                      : 'bg-wms-muted/10 text-wms-muted border-wms-muted/20'
                  }`}
                >
                  {des.isActive ? 'Active' : 'Inactive'}
                </span>

                <div className="flex items-center gap-1 opacity-90 group-hover:opacity-100 transition">
                  <PermissionGate permissions={PERMISSIONS.DEPARTMENT_UPDATE}>
                    <button
                      onClick={() => handleOpenEdit(des)}
                      className="p-1.5 rounded-lg text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition cursor-pointer"
                      title="Edit Designation"
                    >
                      <Edit2 className="h-3.5 w-3.5" />
                    </button>
                  </PermissionGate>

                  <PermissionGate permissions={PERMISSIONS.DEPARTMENT_DELETE}>
                    <button
                      onClick={() => setDesignationToDelete(des)}
                      className="p-1.5 rounded-lg text-wms-muted hover:text-wms-danger hover:bg-wms-danger/10 transition cursor-pointer"
                      title="Delete Designation"
                    >
                      <Trash2 className="h-3.5 w-3.5" />
                    </button>
                  </PermissionGate>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Designation Create/Edit Modal */}
      <DesignationModal
        isOpen={isModalOpen}
        departmentId={departmentId}
        designationToEdit={designationToEdit}
        onClose={() => setIsModalOpen(false)}
        onSuccess={onRefresh}
      />

      {/* Delete Confirmation Modal */}
      {designationToDelete && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
          <div className="w-full max-w-md bg-wms-surface border border-wms-border rounded-2xl p-6 shadow-2xl space-y-4">
            <div className="flex items-center gap-3 text-wms-danger">
              <div className="p-2.5 rounded-xl bg-wms-danger/10 border border-wms-danger/20">
                <AlertTriangle className="h-6 w-6" />
              </div>
              <h3 className="text-lg font-bold text-wms-text">Delete Designation</h3>
            </div>

            <p className="text-sm text-wms-secondary leading-relaxed">
              Are you sure you want to delete designation{' '}
              <strong className="text-wms-text">{designationToDelete.name}</strong>?
            </p>

            {deleteError && (
              <div className="p-3 bg-wms-danger/10 border border-wms-danger/20 rounded-lg text-xs text-wms-danger">
                {deleteError}
              </div>
            )}

            <div className="flex items-center justify-end gap-3 pt-3 border-t border-wms-border">
              <button
                type="button"
                onClick={() => {
                  setDesignationToDelete(null);
                  setDeleteError(null);
                }}
                disabled={isDeleting}
                className="px-4 py-2 rounded-lg bg-wms-hover border border-wms-border text-sm font-medium text-wms-secondary hover:text-wms-text transition cursor-pointer"
              >
                Cancel
              </button>
              <button
                type="button"
                onClick={handleDeleteConfirm}
                disabled={isDeleting}
                className="px-5 py-2 rounded-lg bg-wms-danger hover:bg-red-700 text-white text-sm font-semibold transition cursor-pointer disabled:opacity-50"
              >
                {isDeleting ? 'Deleting...' : 'Delete Designation'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
