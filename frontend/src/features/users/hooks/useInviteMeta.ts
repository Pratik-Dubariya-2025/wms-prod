import { useQuery } from '@tanstack/react-query';
import { getInviteMeta } from '@/api/usersApi';

export function useInviteMeta(enabled = true) {
  return useQuery({
    queryKey: ['invite-meta'],
    queryFn: async () => {
      const response = await getInviteMeta();
      if (!response.succeeded || !response.data) {
        throw new Error(response.message || 'Failed to fetch metadata');
      }
      return response.data;
    },
    enabled,
    staleTime: 5 * 60 * 1000, // 5 minutes cache
  });
}
