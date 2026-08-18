import { useMutation } from '@tanstack/react-query';
import { resetPassword, type ResetPasswordPayload } from '@/api/authApi';
import { toast } from '@/components/ui/Toast/Toast';
import type { AxiosError } from 'axios';
import type { ApiResponse } from '@/types/api.types';

export function useResetPassword() {
  return useMutation({
    mutationFn: (payload: ResetPasswordPayload) => resetPassword(payload),
    onSuccess: (response) => {
      if (response.succeeded) {
        toast.success(response.message || 'Password reset successfully.');
      } else {
        toast.error(response.message || 'Failed to reset password.');
      }
    },
    onError: (error: AxiosError<ApiResponse>) => {
      const message = error.response?.data?.message || 'Failed to reset password. Please try again.';
      toast.error(message);
    },
  });
}
