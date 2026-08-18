import { useState, useEffect, useCallback } from 'react';
import { getProjectById } from '@/api/projectsApi';
import type { ProjectDetail } from '@/types/project.types';

interface UseProjectDetailReturn {
  project: ProjectDetail | null;
  isLoading: boolean;
  error: string | null;
  refresh: () => void;
}

export function useProjectDetail(id: string): UseProjectDetailReturn {
  const [project, setProject] = useState<ProjectDetail | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchProject = useCallback(async () => {
    if (!id) return;
    setIsLoading(true);
    setError(null);
    try {
      const response = await getProjectById(id);
      if (response.succeeded && response.data) {
        setProject(response.data);
      } else {
        setError(response.message || 'Failed to fetch project details');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred');
    } finally {
      setIsLoading(false);
    }
  }, [id]);

  useEffect(() => {
    fetchProject();
  }, [fetchProject]);

  return {
    project,
    isLoading,
    error,
    refresh: fetchProject,
  };
}
