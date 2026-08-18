import { useState, useEffect } from 'react';
import { X } from 'lucide-react';
import { getProjectCreateMeta } from '@/api/projectsApi';
import type { CreateProjectPayload, TeamLeadOption } from '@/types/project.types';

interface CreateProjectModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (payload: CreateProjectPayload) => Promise<boolean>;
}

export function CreateProjectModal({ isOpen, onClose, onSubmit }: CreateProjectModalProps) {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [status, setStatus] = useState('Planning');
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [teamLeadId, setTeamLeadId] = useState('');

  const [departmentName, setDepartmentName] = useState('');
  const [teamLeads, setTeamLeads] = useState<TeamLeadOption[]>([]);
  const [isLoadingMeta, setIsLoadingMeta] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!isOpen) return;

    // Reset fields
    setName('');
    setDescription('');
    setStatus('Planning');
    setStartDate('');
    setEndDate('');
    setTeamLeadId('');
    setDepartmentName('');
    setTeamLeads([]);
    setError(null);

    async function loadMeta() {
      setIsLoadingMeta(true);
      try {
        const metaRes = await getProjectCreateMeta();
        if (metaRes.succeeded && metaRes.data) {
          setDepartmentName(metaRes.data.departmentName);
          setTeamLeads(metaRes.data.teamLeads);
        } else {
          setError(metaRes.message || 'Failed to load project metadata.');
        }
      } catch {
        setError('Failed to load project metadata.');
      } finally {
        setIsLoadingMeta(false);
      }
    }

    loadMeta();
  }, [isOpen]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!name.trim()) {
      setError('Project name is required.');
      return;
    }

    setIsSubmitting(true);
    setError(null);

    const payload: CreateProjectPayload = {
      name,
      description: description.trim() || undefined,
      status,
      startDate: startDate || null,
      endDate: endDate || null,
      teamLeadId: teamLeadId || null,
    };

    const success = await onSubmit(payload);
    setIsSubmitting(false);
    if (success) {
      onClose();
    } else {
      setError('Failed to create project. Please check inputs.');
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm animate-fade-in p-4">
      <div className="relative w-full max-w-lg bg-wms-surface backdrop-blur-xl border border-wms-border rounded-2xl shadow-2xl p-4 sm:p-6 max-h-[90vh] overflow-y-auto">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-wms-border pb-4 mb-4">
          <h2 className="text-xl font-bold text-wms-text tracking-tight">Create New Project</h2>
          <button
            onClick={onClose}
            className="text-wms-text/40 hover:text-wms-text transition duration-200"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {error && (
          <div className="mb-4 p-3 bg-red-500/10 border border-red-500/20 text-red-400 text-xs rounded-lg">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="space-y-4">
          {/* Project Name */}
          <div>
            <label className="block text-xs font-semibold text-wms-text/60 uppercase tracking-wider mb-1.5">
              Project Name *
            </label>
            <input
              type="text"
              required
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-4 py-2.5 bg-wms-hover border border-wms-border rounded-lg text-wms-text placeholder-white/25 focus:outline-none focus:border-wms-indigo focus:ring-1 focus:ring-wms-indigo text-sm"
              placeholder="e.g. WMS Phase 2"
            />
          </div>

          {/* Description */}
          <div>
            <label className="block text-xs font-semibold text-wms-text/60 uppercase tracking-wider mb-1.5">
              Description
            </label>
            <textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              rows={3}
              className="w-full px-4 py-2.5 bg-wms-hover border border-wms-border rounded-lg text-wms-text placeholder-white/25 focus:outline-none focus:border-wms-indigo focus:ring-1 focus:ring-wms-indigo text-sm resize-none"
              placeholder="Provide a brief summary of the project scope..."
            />
          </div>

          {/* Department & Status */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold text-wms-text/60 uppercase tracking-wider mb-1.5">
                Department
              </label>
              <div
                className="w-full px-4 py-2.5 bg-wms-hover/60 border border-wms-border rounded-lg text-wms-text/70 text-sm flex items-center"
                title="Projects are created in your department"
              >
                {isLoadingMeta ? 'Loading…' : departmentName || '—'}
              </div>
            </div>

            <div>
              <label className="block text-xs font-semibold text-wms-text/60 uppercase tracking-wider mb-1.5">
                Status
              </label>
              <select
                value={status}
                onChange={(e) => setStatus(e.target.value)}
                className="w-full px-4 py-2.5 bg-wms-hover border border-wms-border rounded-lg text-wms-text focus:outline-none focus:border-wms-indigo focus:ring-1 focus:ring-wms-indigo text-sm"
              >
                <option value="Planning" className="bg-wms-surface text-wms-text">Planning</option>
                <option value="Active" className="bg-wms-surface text-wms-text">Active</option>
                <option value="OnHold" className="bg-wms-surface text-wms-text">On Hold</option>
                <option value="Completed" className="bg-wms-surface text-wms-text">Completed</option>
                <option value="Cancelled" className="bg-wms-surface text-wms-text">Cancelled</option>
              </select>
            </div>
          </div>

          {/* Dates */}
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold text-wms-text/60 uppercase tracking-wider mb-1.5">
                Start Date
              </label>
              <input
                type="date"
                value={startDate}
                onChange={(e) => setStartDate(e.target.value)}
                className="w-full px-4 py-2.5 bg-wms-hover border border-wms-border rounded-lg text-wms-text focus:outline-none focus:border-wms-indigo focus:ring-1 focus:ring-wms-indigo text-sm"
              />
            </div>

            <div>
              <label className="block text-xs font-semibold text-wms-text/60 uppercase tracking-wider mb-1.5">
                End Date
              </label>
              <input
                type="date"
                value={endDate}
                onChange={(e) => setEndDate(e.target.value)}
                className="w-full px-4 py-2.5 bg-wms-hover border border-wms-border rounded-lg text-wms-text focus:outline-none focus:border-wms-indigo focus:ring-1 focus:ring-wms-indigo text-sm"
              />
            </div>
          </div>

          {/* Team Lead Assignment — the project inherits the selected Tech Lead's team */}
          <div>
            <label className="block text-xs font-semibold text-wms-text/60 uppercase tracking-wider mb-1.5">
              Assign Tech Lead
            </label>
            <select
              value={teamLeadId}
              onChange={(e) => setTeamLeadId(e.target.value)}
              className="w-full px-4 py-2.5 bg-wms-hover border border-wms-border rounded-lg text-wms-text focus:outline-none focus:border-wms-indigo focus:ring-1 focus:ring-wms-indigo text-sm"
            >
              <option value="" className="bg-wms-surface text-wms-text">-- Unassigned --</option>
              {teamLeads.map((tl) => (
                <option key={tl.id} value={tl.id} className="bg-wms-surface text-wms-text">
                  {tl.fullName}{tl.teamName ? ` — ${tl.teamName}` : ''}
                </option>
              ))}
            </select>
            {!isLoadingMeta && teamLeads.length === 0 && (
              <p className="text-xs text-wms-muted mt-1.5">
                No Tech Leads with a team in your department yet. Invite a Tech Lead first.
              </p>
            )}
          </div>

          {/* Action buttons */}
          <div className="flex justify-end gap-3 border-t border-wms-border pt-4 mt-6">
            <button
              type="button"
              onClick={onClose}
              disabled={isSubmitting}
              className="px-4 py-2 bg-wms-hover hover:bg-wms-hover border border-wms-border rounded-lg text-sm text-wms-text/70 hover:text-wms-text transition duration-200"
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="px-5 py-2 bg-wms-indigo hover:bg-indigo-700 text-white text-sm font-semibold rounded-lg shadow-lg shadow-wms-indigo/25 transition duration-200 disabled:opacity-50"
            >
              {isSubmitting ? 'Creating...' : 'Create Project'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
