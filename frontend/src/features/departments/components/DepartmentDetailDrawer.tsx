import { useEffect, useState } from 'react';
import { getDepartmentById } from '@/api/departmentApi';
import type { DepartmentDetail } from '../types/department.types';
import { X, Users, Award, Building, CheckCircle2, XCircle } from 'lucide-react';
import { Spinner } from '@/components/ui/Spinner/Spinner';
import { DepartmentUserList } from './DepartmentUserList';
import { DepartmentDesignationList } from './DepartmentDesignationList';

interface DepartmentDetailDrawerProps {
  departmentId: string | null;
  onClose: () => void;
}

export function DepartmentDetailDrawer({ departmentId, onClose }: DepartmentDetailDrawerProps) {
  const [detail, setDetail] = useState<DepartmentDetail | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState<'users' | 'designations'>('users');

  useEffect(() => {
    if (!departmentId) {
      setDetail(null);
      return;
    }

    let isMounted = true;
    setIsLoading(true);
    setError(null);

    getDepartmentById(departmentId)
      .then((res) => {
        if (!isMounted) return;
        if (res.succeeded && res.data) {
          setDetail(res.data);
        } else {
          setError(res.message || 'Failed to load department details.');
        }
      })
      .catch((err: Error) => {
        if (isMounted) {
          setError(err.message || 'An error occurred while fetching department details.');
        }
      })
      .finally(() => {
        if (isMounted) setIsLoading(false);
      });

    return () => {
      isMounted = false;
    };
  }, [departmentId]);

  if (!departmentId) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-hidden bg-black/60 backdrop-blur-sm animate-fade-in flex justify-end">
      <div
        className="w-full max-w-xl bg-wms-surface border-l border-wms-border h-full flex flex-col shadow-2xl transition-transform duration-300"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="p-5 border-b border-wms-border flex items-center justify-between bg-wms-hover/50">
          <div className="flex items-center gap-3">
            <div className="p-2.5 rounded-xl bg-wms-indigo/10 text-wms-indigo">
              <Building className="h-6 w-6" />
            </div>
            <div>
              <h2 className="text-lg font-bold text-wms-text">
                {detail?.name || 'Department Details'}
              </h2>
              {detail?.code && (
                <span className="text-xs font-mono font-semibold px-2 py-0.5 rounded bg-wms-hover text-wms-secondary border border-wms-border">
                  {detail.code}
                </span>
              )}
            </div>
          </div>
          <button
            onClick={onClose}
            className="p-2 rounded-lg text-wms-muted hover:text-wms-text hover:bg-wms-hover transition cursor-pointer"
          >
            <X className="h-5 w-5" />
          </button>
        </div>

        {/* Content Body */}
        <div className="flex-1 overflow-y-auto p-6 space-y-6">
          {isLoading ? (
            <div className="flex flex-col items-center justify-center h-64 gap-3 text-wms-muted">
              <Spinner size="lg" />
              <p className="text-sm font-medium">Loading department details...</p>
            </div>
          ) : error ? (
            <div className="p-4 bg-wms-danger/10 border border-wms-danger/20 rounded-xl text-wms-danger text-sm">
              {error}
            </div>
          ) : detail ? (
            <>
              {/* Status & Overview */}
              <div className="grid grid-cols-2 gap-4">
                <div className="p-4 rounded-xl bg-wms-hover border border-wms-border flex items-center gap-3">
                  {detail.isActive ? (
                    <CheckCircle2 className="h-5 w-5 text-emerald-500" />
                  ) : (
                    <XCircle className="h-5 w-5 text-wms-danger" />
                  )}
                  <div>
                    <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider block">Status</span>
                    <span className={`text-sm font-semibold ${detail.isActive ? 'text-emerald-600 dark:text-emerald-400' : 'text-wms-danger'}`}>
                      {detail.isActive ? 'Active Department' : 'Inactive'}
                    </span>
                  </div>
                </div>

                <div className="p-4 rounded-xl bg-wms-hover border border-wms-border flex items-center gap-3">
                  <Users className="h-5 w-5 text-wms-indigo" />
                  <div>
                    <span className="text-xs font-semibold text-wms-muted uppercase tracking-wider block">Members</span>
                    <span className="text-sm font-bold text-wms-text">{detail.users.length} Active Users</span>
                  </div>
                </div>
              </div>

              {detail.description && (
                <div className="space-y-1.5">
                  <h4 className="text-xs font-semibold text-wms-muted uppercase tracking-wider">Description</h4>
                  <p className="text-sm text-wms-text bg-wms-hover/50 p-3 rounded-lg border border-wms-border">
                    {detail.description}
                  </p>
                </div>
              )}

              {/* Tabs */}
              <div className="border-b border-wms-border flex gap-4">
                <button
                  onClick={() => setActiveTab('users')}
                  className={`pb-2.5 text-sm font-semibold transition border-b-2 cursor-pointer flex items-center gap-2 ${
                    activeTab === 'users'
                      ? 'border-wms-indigo text-wms-indigo'
                      : 'border-transparent text-wms-muted hover:text-wms-text'
                  }`}
                >
                  <Users className="h-4 w-4" />
                  Assigned Users ({detail.users.length})
                </button>
                <button
                  onClick={() => setActiveTab('designations')}
                  className={`pb-2.5 text-sm font-semibold transition border-b-2 cursor-pointer flex items-center gap-2 ${
                    activeTab === 'designations'
                      ? 'border-wms-indigo text-wms-indigo'
                      : 'border-transparent text-wms-muted hover:text-wms-text'
                  }`}
                >
                  <Award className="h-4 w-4" />
                  Designations ({detail.designations.length})
                </button>
              </div>

              {activeTab === 'users' && <DepartmentUserList users={detail.users} />}
              {activeTab === 'designations' && (
                <DepartmentDesignationList
                  departmentId={departmentId}
                  designations={detail.designations}
                  onRefresh={() => {
                    getDepartmentById(departmentId).then((res) => {
                      if (res.succeeded && res.data) setDetail(res.data);
                    });
                  }}
                />
              )}
            </>
          ) : null}
        </div>
      </div>
    </div>
  );
}
