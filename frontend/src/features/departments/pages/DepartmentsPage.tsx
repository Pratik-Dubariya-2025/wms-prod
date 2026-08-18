import { useState } from 'react';
import { FolderGit, Plus, RefreshCw, Building, AlertTriangle } from 'lucide-react';

import type { DepartmentListItem } from '../types/department.types';
import { PERMISSIONS } from '@/constants/permissions';
import { PermissionGate } from '@/components/shared/PermissionGate';
import { useModal } from '@/components/ui/Modal/useModal';
import { Spinner } from '@/components/ui/Spinner/Spinner';

import { useDepartments } from '../hooks/useDepartments';
import { DepartmentStatsBar } from '../components/DepartmentStatsBar';
import { DepartmentFilters } from '../components/DepartmentFilters';
import { DepartmentCard } from '../components/DepartmentCard';
import { DepartmentTable } from '../components/DepartmentTable';
import { DepartmentModal } from '../components/DepartmentModal';
import { DepartmentDetailDrawer } from '../components/DepartmentDetailDrawer';
import { DeleteDepartmentDialog } from '../components/DeleteDepartmentDialog';

export default function DepartmentsPage() {
  const {
    filteredDepartments,
    stats,
    isLoading,
    error,
    searchQuery,
    setSearchQuery,
    statusFilter,
    setStatusFilter,
    refresh,
    handleDelete,
  } = useDepartments();

  // Layout mode
  const [viewMode, setViewMode] = useState<'grid' | 'table'>('grid');

  // Modals & Drawers
  const createModal = useModal();
  const [departmentToEdit, setDepartmentToEdit] = useState<DepartmentListItem | null>(null);
  const [selectedDepartmentId, setSelectedDepartmentId] = useState<string | null>(null);
  const [departmentToDelete, setDepartmentToDelete] = useState<DepartmentListItem | null>(null);

  const openEditModal = (dept: DepartmentListItem) => {
    setDepartmentToEdit(dept);
    createModal.open();
  };

  const openCreateModal = () => {
    setDepartmentToEdit(null);
    createModal.open();
  };

  return (
    <div className="space-y-6 animate-fade-in pb-10">
      {/* ─── Header ─── */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-wms-border pb-5">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-xl bg-gradient-to-br from-wms-indigo to-wms-cyan flex items-center justify-center shadow-lg shadow-wms-indigo/20">
            <FolderGit className="h-5 w-5 text-wms-text" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-wms-text tracking-tight">Department Hierarchy</h1>
            <p className="text-sm text-wms-secondary mt-0.5">
              Organize organization structure, member assignments, and designation tiers.
            </p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={refresh}
            disabled={isLoading}
            className="inline-flex items-center gap-2 px-3 py-2.5 rounded-lg bg-wms-hover border border-wms-border text-sm text-wms-secondary hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer disabled:opacity-50"
            title="Refresh Departments"
          >
            <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
          </button>
          <PermissionGate permissions={PERMISSIONS.DEPARTMENT_CREATE}>
            <button
              onClick={openCreateModal}
              className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-indigo/25"
            >
              <Plus className="h-4 w-4" />
              Add Department
            </button>
          </PermissionGate>
        </div>
      </div>

      {/* ─── Stats ─── */}
      <DepartmentStatsBar {...stats} />

      {/* ─── Filters ─── */}
      <DepartmentFilters
        searchQuery={searchQuery}
        onSearchChange={setSearchQuery}
        statusFilter={statusFilter}
        onStatusChange={setStatusFilter}
        viewMode={viewMode}
        onViewModeChange={setViewMode}
      />

      {/* ─── Content ─── */}
      {isLoading ? (
        <div className="flex flex-col items-center justify-center py-20 gap-3 text-wms-muted">
          <Spinner size="lg" />
          <p className="text-sm font-medium">Fetching department structure...</p>
        </div>
      ) : error ? (
        <div className="glass-card p-8 rounded-xl text-center text-wms-danger border border-wms-danger/20">
          <AlertTriangle className="h-10 w-10 mx-auto mb-2 opacity-80" />
          <p className="font-semibold text-base">{error}</p>
          <button
            onClick={refresh}
            className="mt-4 px-4 py-2 bg-wms-hover border border-wms-border text-xs text-wms-text rounded-lg hover:bg-wms-border cursor-pointer"
          >
            Try Again
          </button>
        </div>
      ) : filteredDepartments.length === 0 ? (
        <div className="glass-card p-16 rounded-xl text-center text-wms-muted border border-wms-border">
          <Building className="h-12 w-12 mx-auto mb-3 opacity-40 text-wms-muted" />
          <h3 className="text-base font-bold text-wms-text">No Departments Found</h3>
          <p className="text-sm text-wms-muted max-w-sm mx-auto mt-1">
            {searchQuery
              ? `No departments match "${searchQuery}". Try adjusting your filter terms.`
              : 'Get started by creating your first organizational department.'}
          </p>
        </div>
      ) : viewMode === 'grid' ? (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {filteredDepartments.map((dept) => (
            <DepartmentCard
              key={dept.id}
              department={dept}
              onView={setSelectedDepartmentId}
              onEdit={openEditModal}
              onDelete={setDepartmentToDelete}
            />
          ))}
        </div>
      ) : (
        <DepartmentTable
          departments={filteredDepartments}
          onView={setSelectedDepartmentId}
          onEdit={openEditModal}
          onDelete={setDepartmentToDelete}
        />
      )}

      {/* ─── Modals & Drawers ─── */}
      <DepartmentModal
        isOpen={createModal.isOpen}
        departmentToEdit={departmentToEdit}
        onClose={createModal.close}
        onSuccess={refresh}
      />

      <DepartmentDetailDrawer
        departmentId={selectedDepartmentId}
        onClose={() => setSelectedDepartmentId(null)}
      />

      {departmentToDelete && (
        <DeleteDepartmentDialog
          department={departmentToDelete}
          onConfirm={async (id) => {
            const result = await handleDelete(id);
            if (result.succeeded) setDepartmentToDelete(null);
            return result;
          }}
          onCancel={() => setDepartmentToDelete(null)}
        />
      )}
    </div>
  );
}
