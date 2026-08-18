import { useQuery } from '@tanstack/react-query';
import { getPolicyAttributes, type PolicyAttributesDto } from '@/api/policiesApi';

/**
 * Fetches the curated user-attribute list + module-specific resource-attribute
 * list used to populate the row-filter tree builder / conditions autocomplete.
 * Re-fetches whenever the module code changes; disabled entirely when no
 * module is selected yet.
 */
export function usePolicyAttributes(moduleCode?: string) {
  return useQuery<PolicyAttributesDto | null>({
    queryKey: ['policy-attributes', moduleCode],
    queryFn: async () => {
      if (!moduleCode) return null;
      const res = await getPolicyAttributes(moduleCode);
      return res.data ?? null;
    },
    enabled: !!moduleCode,
    staleTime: 5 * 60 * 1000,
  });
}
