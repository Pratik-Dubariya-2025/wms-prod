import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link, useNavigate } from 'react-router-dom';
import { Mail, ArrowLeft } from 'lucide-react';

import { forgotPasswordSchema, type ForgotPasswordFormValues } from '../schemas/loginSchema';
import { useVerifyFirstTimeLoginEmail } from '../hooks/useVerifyFirstTimeLoginEmail';
import { Input } from '@/components/ui/Input/Input';
import { Button } from '@/components/ui/Button/Button';
import { ROUTES } from '@/constants/routes';

export function VerifyEmailForm() {
  const navigate = useNavigate();
  const verifyMutation = useVerifyFirstTimeLoginEmail();

  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      email: '',
    },
  });

  function onSubmit(values: ForgotPasswordFormValues) {
    verifyMutation.mutate(
      { email: values.email },
      {
        onSuccess: (response) => {
          if (response.succeeded) {
            reset();
            // Redirect to setup password with email passed in history state
            navigate(ROUTES.CHANGE_PASSWORD, { state: { email: values.email } });
          }
        },
      }
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
      <div className="text-center mb-2">
        <h3 className="text-lg font-semibold text-wms-text">Verify Your Account</h3>
        <p className="text-sm text-wms-secondary mt-1">
          To complete your first-time login setup, please verify your registered email address.
        </p>
      </div>

      <Input
        {...register('email')}
        label="Email Address"
        type="email"
        placeholder="Enter your registered email"
        error={errors.email?.message}
        leftIcon={<Mail className="h-4 w-4" />}
        autoComplete="email"
        id="verify-email-input"
      />

      <Button
        type="submit"
        isLoading={verifyMutation.isPending}
        fullWidth
        size="lg"
        id="verify-email-submit"
      >
        Verify Email
      </Button>

      <Link
        to={ROUTES.LOGIN}
        className="text-sm text-wms-muted hover:text-wms-text transition flex items-center justify-center gap-1.5 mt-1 cursor-pointer font-medium"
      >
        <ArrowLeft className="h-4 w-4" /> Back to Sign In
      </Link>
    </form>
  );
}
