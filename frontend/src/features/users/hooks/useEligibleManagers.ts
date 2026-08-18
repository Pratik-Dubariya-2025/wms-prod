import { useQuery } from '@tanstack/react-query';
import { getEligibleManagers } from '@/api/usersApi';

/**
 * Eligible managers (department heads) for the selected department.
 * Only fetches when a department is chosen and the invited designation
 * actually needs a manager.
 */
export function useEligibleManagers(departmentId: string, enabled: boolean) {
  return useQuery({
    queryKey: ['eligible-managers', departmentId],
    queryFn: async () => {
      if (!departmentId) return [];
      const res = await getEligibleManagers(departmentId);
      return res.data || [];
    },
    enabled: !!departmentId && enabled,
    staleTime: 60 * 1000,
  });
}
