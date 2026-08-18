import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from 'react-router-dom';
import { useState } from 'react';
import { Mail, ArrowLeft, CheckCircle2 } from 'lucide-react';

import { forgotPasswordSchema, type ForgotPasswordFormValues } from '../schemas/loginSchema';
import { useForgotPassword } from '../hooks/useForgotPassword';
import { Input } from '@/components/ui/Input/Input';
import { Button } from '@/components/ui/Button/Button';
import { ROUTES } from '@/constants/routes';

export function ForgotPasswordForm() {
  const [emailSentTo, setEmailSentTo] = useState<string | null>(null);
  const forgotMutation = useForgotPassword();

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
    forgotMutation.mutate(values, {
      onSuccess: (response) => {
        if (response.succeeded) {
          setEmailSentTo(values.email);
          reset();
        }
      },
    });
  }

  if (emailSentTo) {
    return (
      <div className="text-center py-4 flex flex-col items-center gap-4">
        <div className="h-12 w-12 rounded-full bg-emerald-500/10 flex items-center justify-center text-emerald-400">
          <CheckCircle2 className="h-6 w-6" />
        </div>
        <div>
          <h3 className="text-lg font-semibold text-wms-text">Reset link sent!</h3>
          <p className="text-sm text-wms-secondary mt-1 max-w-[300px] mx-auto">
            We've sent a password recovery email to <span className="text-wms-cyan font-medium">{emailSentTo}</span>.
          </p>
        </div>
        <div className="flex flex-col gap-2 w-full mt-4">
          <Link to={ROUTES.LOGIN} className="block w-full">
            <Button variant="secondary" fullWidth>
              Back to Sign In
            </Button>
          </Link>
        </div>
      </div>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
      <Input
        {...register('email')}
        label="Email Address"
        type="email"
        placeholder="Enter your email"
        error={errors.email?.message}
        leftIcon={<Mail className="h-4 w-4" />}
        autoComplete="email"
        id="forgot-email"
      />

      <Button
        type="submit"
        isLoading={forgotMutation.isPending}
        fullWidth
        size="lg"
        id="forgot-submit"
      >
        Send Reset Link
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
