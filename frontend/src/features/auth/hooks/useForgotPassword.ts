import { useMutation } from '@tanstack/react-query';
import { forgotPassword, type ForgotPasswordPayload } from '@/api/authApi';
import { toast } from '@/components/ui/Toast/Toast';
import type { AxiosError } from 'axios';
import type { ApiResponse } from '@/types/api.types';

export function useForgotPassword() {
  return useMutation({
    mutationFn: (payload: ForgotPasswordPayload) => forgotPassword(payload),
    onSuccess: (response) => {
      if (response.succeeded) {
        toast.success(response.message || 'Reset link sent to your email.');
      } else {
        toast.error(response.message || 'Failed to send reset link.');
      }
    },
    onError: (error: AxiosError<ApiResponse>) => {
      const message = error.response?.data?.message || 'Failed to send reset link. Please try again.';
      toast.error(message);
    },
  });
}
