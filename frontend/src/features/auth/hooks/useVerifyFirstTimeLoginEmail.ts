import { useMutation } from '@tanstack/react-query';
import { verifyFirstTimeLoginEmail } from '@/api/authApi';
import { toast } from '@/components/ui/Toast/Toast';
import type { AxiosError } from 'axios';

export function useVerifyFirstTimeLoginEmail() {
  return useMutation({
    mutationFn: (payload: { email: string }) => verifyFirstTimeLoginEmail(payload),
    onSuccess: (response) => {
      if (response.succeeded) {
        toast.success(response.message || 'Email verified successfully.');
      } else {
        toast.error(response.message || 'Verification failed.');
      }
    },
    onError: (error: AxiosError<any>) => {
      const data = error.response?.data;
      const message = data?.message || data?.Message || 'Email verification failed. Please try again.';
      toast.error(message);
    },

  });
}
