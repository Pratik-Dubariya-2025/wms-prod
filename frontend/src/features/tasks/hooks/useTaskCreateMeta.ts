import { useState, useEffect } from 'react';
import { getTaskCreateMeta } from '@/api/tasksApi';
import type { TaskCreateMeta } from '@/features/tasks/types/task.types';

interface UseTaskCreateMetaReturn {
  meta: TaskCreateMeta | null;
  isLoading: boolean;
  error: string | null;
}

export function useTaskCreateMeta(projectId: string): UseTaskCreateMetaReturn {
  const [meta, setMeta] = useState<TaskCreateMeta | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!projectId) return;
    let cancelled = false;

    async function fetchMeta() {
      setIsLoading(true);
      setError(null);
      try {
        const response = await getTaskCreateMeta(projectId);
        if (!cancelled) {
          if (response.succeeded && response.data) {
            setMeta(response.data);
          } else {
            setError(response.message || 'Failed to fetch task creation metadata');
          }
        }
      } catch (err: unknown) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : 'An unexpected error occurred');
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false);
        }
      }
    }

    fetchMeta();
    return () => { cancelled = true; };
  }, [projectId]);

  return { meta, isLoading, error };
}
