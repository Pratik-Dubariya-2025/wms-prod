import { useAuthStore } from '@/store/authStore';
import { getUserFromToken } from '@/services/tokenService';

export function useCurrentUser() {
  const accessToken = useAuthStore((s) => s.accessToken);

  if (!accessToken) {
    return null;
  }

  return getUserFromToken(accessToken);
}
