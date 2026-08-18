import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { 
  FolderGit, 
  Search, 
  RefreshCw, 
  Plus, 
  User, 
  Calendar, 
  Users, 
  CheckSquare, 
  Filter 
} from 'lucide-react';

import { useProjects } from '../hooks/useProjects';
import { usePermissions } from '@/hooks/usePermissions';
import { PERMISSIONS } from '@/constants/permissions';
import { Badge } from '@/components/ui/Badge/Badge';
import { Pagination } from '@/components/ui/Pagination/Pagination';
import { useModal } from '@/components/ui/Modal/useModal';
import { CreateProjectModal } from '../components/CreateProjectModal';
import { createProject } from '@/api/projectsApi';
import type { CreateProjectPayload } from '@/types/project.types';

const STATUS_COLOR_MAP: Record<string, 'indigo' | 'purple' | 'cyan' | 'emerald' | 'warning' | 'danger'> = {
  Planning: 'purple',
  Active: 'emerald',
  OnHold: 'warning',
  Completed: 'indigo',
  Cancelled: 'danger',
};

export default function ProjectsPage() {
  const navigate = useNavigate();
  const {
    projects,
    pagination,
    isLoading,
    error,
    params,
    setSearch,
    setStatus,
    setPage,
    refresh,
  } = useProjects(9);

  const { hasPermission } = usePermissions();
  const createModal = useModal();
  const [filterStatus, setFilterStatus] = useState('');

  const handleCreateSubmit = async (payload: CreateProjectPayload) => {
    try {
      const response = await createProject(payload);
      if (response.succeeded) {
        refresh();
        return true;
      }
      return false;
    } catch {
      return false;
    }
  };

  const handleStatusFilterChange = (status: string) => {
    setFilterStatus(status);
    setStatus(status);
  };

  const isManagerOrAdmin = hasPermission(PERMISSIONS.PROJECT_CREATE);

  return (
    <div className="space-y-6 animate-fade-in">
      {/* Page Header */}
      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4 border-b border-wms-border pb-5">
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-xl bg-gradient-to-br from-wms-indigo to-wms-cyan flex items-center justify-center shadow-lg shadow-wms-indigo/20">
            <FolderGit className="h-5 w-5 text-wms-text" />
          </div>
          <div>
            <h1 className="text-2xl font-bold text-wms-text tracking-tight">Projects</h1>
            <p className="text-sm text-wms-text/60 mt-0.5">
              {pagination ? (
                <>
                  <span className="text-wms-indigo font-semibold">{pagination.totalCount}</span> active projects
                </>
              ) : (
                'Manage workspace projects'
              )}
            </p>
          </div>
        </div>

        <div className="flex items-center gap-2">
          <button
            onClick={refresh}
            disabled={isLoading}
            className="inline-flex items-center justify-center p-2.5 rounded-lg bg-wms-hover border border-wms-border text-wms-text/70 hover:text-wms-text hover:bg-wms-hover transition duration-200 cursor-pointer disabled:opacity-50"
            title="Refresh"
          >
            <RefreshCw className={`h-4 w-4 ${isLoading ? 'animate-spin' : ''}`} />
          </button>
          {isManagerOrAdmin && (
            <button
              onClick={createModal.open}
              className="inline-flex items-center gap-2 px-5 py-2.5 rounded-lg bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold transition duration-200 cursor-pointer shadow-lg shadow-wms-indigo/25"
            >
              <Plus className="h-4 w-4" />
              New Project
            </button>
          )}
        </div>
      </div>

      {/* Search and Filters */}
      <div className="flex flex-col md:flex-row gap-4">
        {/* Search */}
        <div className="relative flex-1">
          <Search className="absolute left-3.5 top-1/2 -translate-y-1/2 h-4 w-4 text-wms-text/30" />
          <input
            type="text"
            placeholder="Search projects..."
            defaultValue={params.search || ''}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full pl-10 pr-4 py-2.5 bg-wms-hover border border-wms-border rounded-lg text-wms-text placeholder-white/25 focus:outline-none focus:border-wms-indigo focus:ring-1 focus:ring-wms-indigo text-sm transition duration-200"
          />
        </div>

        {/* Filters */}
        <div className="flex items-center gap-3">
          <div className="flex items-center gap-2 text-wms-text/50 text-sm">
            <Filter className="h-4 w-4" />
            <span>Status:</span>
          </div>
          <select
            value={filterStatus}
            onChange={(e) => handleStatusFilterChange(e.target.value)}
            className="px-4 py-2.5 bg-wms-hover border border-wms-border rounded-lg text-wms-text/80 focus:outline-none focus:border-wms-indigo focus:ring-1 focus:ring-wms-indigo text-sm"
          >
            <option value="" className="bg-wms-surface text-wms-text">All Statuses</option>
            <option value="Planning" className="bg-wms-surface text-wms-text">Planning</option>
            <option value="Active" className="bg-wms-surface text-wms-text">Active</option>
            <option value="OnHold" className="bg-wms-surface text-wms-text">On Hold</option>
            <option value="Completed" className="bg-wms-surface text-wms-text">Completed</option>
            <option value="Cancelled" className="bg-wms-surface text-wms-text">Cancelled</option>
          </select>
        </div>
      </div>

      {/* Projects Grid */}
      {isLoading ? (
        <div className="flex justify-center items-center py-24">
          <RefreshCw className="h-8 w-8 text-wms-indigo animate-spin" />
        </div>
      ) : error ? (
        <div className="p-4 bg-red-500/10 border border-red-500/20 text-red-400 rounded-xl text-center">
          {error}
        </div>
      ) : projects.length === 0 ? (
        <div className="p-12 text-center border border-dashed border-wms-border rounded-2xl">
          <FolderGit className="h-12 w-12 text-wms-text/20 mx-auto mb-3" />
          <h3 className="text-wms-text font-medium text-lg">No Projects Found</h3>
          <p className="text-wms-text/40 text-sm mt-1">Try resetting search filters or create a new project.</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {projects.map((project) => (
            <div
              key={project.id}
              onClick={() => navigate(`/projects/${project.id}`)}
              className="group relative bg-wms-hover hover:bg-wms-hover border border-wms-border hover:border-wms-border rounded-2xl p-5 shadow-lg transition-all duration-300 cursor-pointer flex flex-col justify-between overflow-hidden"
            >
              {/* Glow decoration */}
              <div className="absolute -right-20 -top-20 w-40 h-40 rounded-full bg-wms-indigo/10 blur-3xl opacity-0 group-hover:opacity-100 transition-opacity duration-500 pointer-events-none" />

              <div className="space-y-4">
                {/* Title & Status */}
                <div className="flex justify-between items-start gap-3">
                  <div>
                    <h3 className="text-lg font-bold text-wms-text group-hover:text-wms-indigo transition-colors duration-200 line-clamp-1">
                      {project.name}
                    </h3>
                    <span className="text-xs text-wms-text/40 font-medium">
                      {project.departmentName}
                    </span>
                  </div>
                  <Badge variant={STATUS_COLOR_MAP[project.status] || 'indigo'}>
                    {project.status}
                  </Badge>
                </div>

                {/* Description */}
                <p className="text-sm text-wms-text/50 line-clamp-2 h-10">
                  {project.description || 'No description provided.'}
                </p>

                {/* People roles */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 text-xs pt-2 border-t border-wms-border">
                  <div className="flex items-center gap-1.5 text-wms-text/60">
                    <User className="h-3.5 w-3.5 text-wms-cyan" />
                    <span className="truncate" title={`Owner: ${project.ownerName}`}>
                      Owner: {project.ownerName}
                    </span>
                  </div>
                  <div className="flex items-center gap-1.5 text-wms-text/60">
                    <User className="h-3.5 w-3.5 text-wms-indigo" />
                    <span className="truncate" title={`TL: ${project.teamLeadName || 'Unassigned'}`}>
                      Lead: {project.teamLeadName || 'None'}
                    </span>
                  </div>
                </div>
              </div>

              {/* Footer details */}
              <div className="flex items-center justify-between mt-5 pt-3 border-t border-wms-border text-xs text-wms-text/40">
                <div className="flex items-center gap-3">
                  <span className="flex items-center gap-1">
                    <Users className="h-3.5 w-3.5" />
                    {project.memberCount}
                  </span>
                  <span className="flex items-center gap-1">
                    <CheckSquare className="h-3.5 w-3.5" />
                    {project.taskCount}
                  </span>
                </div>
                {project.startDate && (
                  <span className="flex items-center gap-1">
                    <Calendar className="h-3.5 w-3.5" />
                    {new Date(project.startDate).toLocaleDateString()}
                  </span>
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Pagination */}
      {pagination && (
        <Pagination
          pageNumber={pagination.pageNumber}
          pageSize={pagination.pageSize}
          totalCount={pagination.totalCount}
          totalPages={pagination.totalPages}
          hasPreviousPage={pagination.hasPreviousPage}
          hasNextPage={pagination.hasNextPage}
          onPageChange={setPage}
          onPageSizeChange={() => {}}
        />
      )}

      {/* Create Project Modal */}
      <CreateProjectModal
        isOpen={createModal.isOpen}
        onClose={createModal.close}
        onSubmit={handleCreateSubmit}
      />
    </div>
  );
}
