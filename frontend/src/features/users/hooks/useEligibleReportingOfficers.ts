import { useQuery } from '@tanstack/react-query';
import { getEligibleReportingOfficers } from '@/api/usersApi';

/**
 * Eligible reporting officers (Team Leads, plus the manager as a fallback)
 * for the selected department / manager / invited level.
 */
export function useEligibleReportingOfficers(
  departmentId: string,
  invitedDesignationLevel: number,
  managerId: string,
  enabled: boolean,
) {
  return useQuery({
    queryKey: ['eligible-reporting-officers', departmentId, invitedDesignationLevel, managerId],
    queryFn: async () => {
      if (!departmentId || invitedDesignationLevel === 0 || !managerId) return [];
      const res = await getEligibleReportingOfficers(departmentId, invitedDesignationLevel, managerId);
      return res.data || [];
    },
    enabled: !!departmentId && !!managerId && invitedDesignationLevel > 0 && enabled,
    staleTime: 60 * 1000,
  });
}
