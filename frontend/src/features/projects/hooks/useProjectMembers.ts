import { useState, useEffect, useCallback } from 'react';
import { getProjectMembers, addProjectMember, removeProjectMember } from '@/api/projectsApi';
import type { ProjectMember } from '@/types/project.types';

interface UseProjectMembersReturn {
  members: ProjectMember[];
  isLoading: boolean;
  error: string | null;
  addMember: (userId: string) => Promise<{ success: boolean; message?: string }>;
  removeMember: (userId: string) => Promise<{ success: boolean; message?: string }>;
  refresh: () => void;
}

export function useProjectMembers(projectId: string): UseProjectMembersReturn {
  const [members, setMembers] = useState<ProjectMember[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const fetchMembers = useCallback(async () => {
    if (!projectId) return;
    setIsLoading(true);
    setError(null);
    try {
      const response = await getProjectMembers(projectId);
      if (response.succeeded && response.data) {
        setMembers(response.data);
      } else {
        setError(response.message || 'Failed to fetch project members');
      }
    } catch (err: unknown) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred');
    } finally {
      setIsLoading(false);
    }
  }, [projectId]);

  useEffect(() => {
    fetchMembers();
  }, [fetchMembers]);

  const addMember = async (userId: string) => {
    try {
      const response = await addProjectMember(projectId, { userId });
      if (response.succeeded) {
        await fetchMembers();
        return { success: true, message: response.message || 'Member added successfully' };
      }
      return { success: false, message: response.message || 'Failed to add member' };
    } catch (err: unknown) {
      return { success: false, message: err instanceof Error ? err.message : 'An unexpected error occurred' };
    }
  };

  const removeMember = async (userId: string) => {
    try {
      const response = await removeProjectMember(projectId, userId);
      if (response.succeeded) {
        await fetchMembers();
        return { success: true, message: response.message || 'Member removed successfully' };
      }
      return { success: false, message: response.message || 'Failed to remove member' };
    } catch (err: unknown) {
      return { success: false, message: err instanceof Error ? err.message : 'An unexpected error occurred' };
    }
  };

  return {
    members,
    isLoading,
    error,
    addMember,
    removeMember,
    refresh: fetchMembers,
  };
}
