import { useMutation } from '@tanstack/react-query';
import { changeFirstTimePassword, type ChangeFirstTimePasswordPayload } from '@/api/authApi';
import { toast } from '@/components/ui/Toast/Toast';
import type { AxiosError } from 'axios';

export function useChangeFirstTimePassword() {
  return useMutation({
    mutationFn: (payload: ChangeFirstTimePasswordPayload) => changeFirstTimePassword(payload),
    onSuccess: (response) => {
      if (response.succeeded) {
        toast.success(response.message || 'Password changed successfully. Please sign in.');
      } else {
        toast.error(response.message || 'Failed to change password.');
      }
    },
    onError: (error: AxiosError<any>) => {
      const data = error.response?.data;
      const message = data?.message || data?.Message || 'Failed to change password. Please try again.';
      toast.error(message);
    },

  });
}
