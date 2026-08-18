import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Link } from 'react-router-dom';
import { useState } from 'react';
import { Mail, Lock, Eye, EyeOff } from 'lucide-react';

import { loginSchema, type LoginFormValues } from '../schemas/loginSchema';
import { useLogin } from '../hooks/useLogin';
import { Input } from '@/components/ui/Input/Input';
import { Button } from '@/components/ui/Button/Button';
import { ROUTES } from '@/constants/routes';

import { Shield, ArrowLeft } from 'lucide-react';

export function LoginForm() {
  const [showPassword, setShowPassword] = useState(false);
  const [requiresMfa, setRequiresMfa] = useState(false);
  const loginMutation = useLogin();

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
      mfaCode: '',
    },
  });

  async function onSubmit(values: LoginFormValues) {
    try {
      const response = await loginMutation.mutateAsync(values);
      const requiresMfa = response.succeeded && 
        response.data && 
        typeof response.data === 'object' && 
        !!(response.data as any).requiresMfa;
      if (requiresMfa) {
        setRequiresMfa(true);
      }
    } catch {
      // Error handled by mutation hook
    }
  }

  if (requiresMfa) {
    return (
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
        <div className="text-center mb-2">
          <div className="inline-flex items-center justify-center h-12 w-12 rounded-xl bg-wms-indigo/20 text-wms-indigo mb-3">
            <Shield className="h-6 w-6" />
          </div>
          <h2 className="text-lg font-bold text-wms-text">Two-Factor Authentication</h2>
          <p className="text-xs text-wms-secondary mt-1">
            Enter the 6-digit verification code from your authenticator app.
          </p>
        </div>

        <Input
          {...register('mfaCode')}
          label="Verification Code"
          type="text"
          placeholder="000000"
          error={errors.mfaCode?.message}
          maxLength={6}
          className="text-center tracking-widest text-lg font-mono"
          leftIcon={<Shield className="h-4 w-4" />}
          autoComplete="one-time-code"
          id="login-mfa-code"
        />

        <Button
          type="submit"
          isLoading={loginMutation.isPending}
          fullWidth
          size="lg"
          id="login-mfa-submit"
        >
          Verify & Sign In
        </Button>

        <button
          type="button"
          onClick={() => {
            setRequiresMfa(false);
            setValue('mfaCode', '');
          }}
          className="inline-flex items-center justify-center gap-2 text-sm text-wms-secondary hover:text-wms-text transition cursor-pointer"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to credentials
        </button>
      </form>
    );
  }

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-5">
      <Input
        {...register('email')}
        label="Email"
        type="email"
        placeholder="Enter your email"
        error={errors.email?.message}
        leftIcon={<Mail className="h-4 w-4" />}
        autoComplete="email"
        id="login-email"
      />

      <Input
        {...register('password')}
        label="Password"
        type={showPassword ? 'text' : 'password'}
        placeholder="Enter your password"
        error={errors.password?.message}
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
        autoComplete="current-password"
        id="login-password"
      />

      <div className="flex items-center justify-between">
        <label className="flex items-center gap-2 cursor-pointer select-none">
          <input
            {...register('rememberMe')}
            type="checkbox"
            className="h-4 w-4 rounded border-wms-border bg-wms-hover text-wms-indigo focus:ring-wms-indigo/50 cursor-pointer"
            id="login-remember"
          />
          <span className="text-sm text-wms-secondary">Remember me</span>
        </label>
        
        <Link
          to={ROUTES.FORGOT_PASSWORD}
          className="text-sm text-wms-indigo hover:text-wms-cyan hover:underline transition font-medium"
        >
          Forgot password?
        </Link>
      </div>

      <Button
        type="submit"
        isLoading={loginMutation.isPending}
        fullWidth
        size="lg"
        id="login-submit"
      >
        Sign In
      </Button>
    </form>
  );
}
