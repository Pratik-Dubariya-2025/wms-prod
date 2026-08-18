import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useSearchParams, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { Lock, Eye, EyeOff, ArrowLeft, AlertTriangle } from 'lucide-react';

import { resetPasswordSchema, type ResetPasswordFormValues } from '../schemas/loginSchema';
import { useResetPassword } from '../hooks/useResetPassword';
import { Input } from '@/components/ui/Input/Input';
import { Button } from '@/components/ui/Button/Button';
import { ROUTES } from '@/constants/routes';

export function ResetPasswordForm() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const tokenFromUrl = searchParams.get('token');

  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const resetMutation = useResetPassword();

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      newPassword: '',
      confirmPassword: '',
    },
  });

  function onSubmit(values: ResetPasswordFormValues) {
    if (!tokenFromUrl) return;

    resetMutation.mutate(
      {
        token: tokenFromUrl,
        newPassword: values.newPassword,
        confirmPassword: values.confirmPassword,
      },
      {
        onSuccess: (response) => {
          if (response.succeeded) {
            reset();
            navigate(ROUTES.LOGIN, { replace: true });
          }
        },
      }
    );
  }

  // If there is no token in the URL, display an elegant warning screen.
  if (!tokenFromUrl) {
    return (
      <div className="text-center py-4 flex flex-col items-center gap-4">
        <div className="h-12 w-12 rounded-full bg-red-500/10 flex items-center justify-center text-red-400">
          <AlertTriangle className="h-6 w-6" />
        </div>
        <div>
          <h3 className="text-lg font-semibold text-wms-text">Invalid Reset Link</h3>
          <p className="text-sm text-wms-secondary mt-1 max-w-[300px] mx-auto">
            This password reset link is invalid or has expired. Please request a new recovery link.
          </p>
        </div>
        <div className="flex flex-col gap-2 w-full mt-4">
          <Link to={ROUTES.FORGOT_PASSWORD} className="block w-full">
            <Button variant="secondary" fullWidth>
              Go to Forgot Password
            </Button>
          </Link>
          <Link
            to={ROUTES.LOGIN}
            className="text-sm text-wms-muted hover:text-wms-text transition flex items-center justify-center gap-1.5 mt-2 cursor-pointer"
          >
            <ArrowLeft className="h-4 w-4" /> Back to Sign In
          </Link>
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
      <Input
        {...register('newPassword')}
        label="New Password"
        type={showPassword ? 'text' : 'password'}
        placeholder="At least 6 characters"
        error={errors.newPassword?.message}
        leftIcon={<Lock className="h-4 w-4" />}
        rightIcon={
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            className="text-wms-muted hover:text-wms-text transition cursor-pointer"
            tabIndex={-1}
          >
            {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
          </button>
        }
        autoComplete="new-password"
        id="reset-new-password"
      />

      <Input
        {...register('confirmPassword')}
        label="Confirm Password"
        type={showConfirmPassword ? 'text' : 'password'}
        placeholder="Repeat new password"
        error={errors.confirmPassword?.message}
        leftIcon={<Lock className="h-4 w-4" />}
        rightIcon={
          <button
            type="button"
            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
            className="text-wms-muted hover:text-wms-text transition cursor-pointer"
            tabIndex={-1}
          >
            {showConfirmPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
          </button>
        }
        autoComplete="new-password"
        id="reset-confirm-password"
      />

      <Button
        type="submit"
        isLoading={resetMutation.isPending}
        fullWidth
        size="lg"
        id="reset-submit"
      >
        Reset Password
      </Button>

      <Link
        to={ROUTES.LOGIN}
        className="text-sm text-wms-muted hover:text-wms-text transition flex items-center justify-center gap-1.5 mt-1 cursor-pointer font-medium"
      >
        <ArrowLeft className="h-4 w-4" /> Cancel & Sign In
      </Link>
    </form>
  );
}
