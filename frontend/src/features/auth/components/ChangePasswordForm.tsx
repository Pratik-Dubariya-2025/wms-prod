import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { Lock, Eye, EyeOff, ArrowLeft, Mail } from 'lucide-react';

import { changePasswordSchema, type ChangePasswordFormValues } from '../schemas/loginSchema';
import { useChangeFirstTimePassword } from '../hooks/useChangeFirstTimePassword';
import { Input } from '@/components/ui/Input/Input';
import { Button } from '@/components/ui/Button/Button';
import { ROUTES } from '@/constants/routes';

export function ChangePasswordForm() {
  const navigate = useNavigate();
  const location = useLocation();
  const emailFromState = (location.state as { email?: string })?.email || '';

  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const changeMutation = useChangeFirstTimePassword();

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<ChangePasswordFormValues>({
    resolver: zodResolver(changePasswordSchema),
    defaultValues: {
      email: emailFromState,
      currentPassword: '',
      newPassword: '',
      confirmPassword: '',
    },
  });

  function onSubmit(values: ChangePasswordFormValues) {
    changeMutation.mutate(
      {
        email: values.email,
        currentPassword: values.currentPassword,
        newPassword: values.newPassword,
        confirmNewPassword: values.confirmPassword,
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


  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
      <div className="text-center mb-2">
        <h3 className="text-lg font-semibold text-wms-text">First-Time Password Change</h3>
        <p className="text-sm text-wms-secondary mt-1">
          For security reasons, you must change your password before logging in for the first time.
        </p>
      </div>

      <Input
        {...register('email')}
        label="Email Address"
        type="email"
        placeholder="Enter your email"
        error={errors.email?.message}
        leftIcon={<Mail className="h-4 w-4" />}
        autoComplete="email"
        readOnly={!!emailFromState}
        className={emailFromState ? 'opacity-60 cursor-not-allowed focus:border-wms-border focus:ring-0' : ''}
        id="change-password-email"
      />


      <Input
        {...register('currentPassword')}
        label="Current Temporary Password"
        type={showCurrentPassword ? 'text' : 'password'}
        placeholder="Enter temporary password"
        error={errors.currentPassword?.message}
        leftIcon={<Lock className="h-4 w-4" />}
        rightIcon={
          <button
            type="button"
            onClick={() => setShowCurrentPassword(!showCurrentPassword)}
            className="text-wms-muted hover:text-wms-text transition cursor-pointer"
            tabIndex={-1}
          >
            {showCurrentPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
          </button>
        }
        autoComplete="current-password"
        id="change-password-current"
      />

      <Input
        {...register('newPassword')}
        label="New Password"
        type={showPassword ? 'text' : 'password'}
        placeholder="At least 8 characters with upper, lower, number, special"
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
        id="change-password-new"
      />

      <Input
        {...register('confirmPassword')}
        label="Confirm New Password"
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
        id="change-password-confirm"
      />

      <Button
        type="submit"
        isLoading={changeMutation.isPending}
        fullWidth
        size="lg"
        id="change-password-submit"
      >
        Change Password
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
