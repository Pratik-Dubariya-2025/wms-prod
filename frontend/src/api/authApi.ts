import axiosInstance from './axiosInstance';
import type { ApiResponse } from '@/types/api.types';

export interface LoginPayload {
  email: string;
  password: string;
  rememberMe?: boolean;
  mfaCode?: string;
}

export interface AuthResponse {
  accessToken: string | null;
  requiresMfa: boolean;
}

export interface MfaSetupDto {
  sharedSecret: string;
  qrCodeUrl: string;
}

//#region GET
/**
 * GET /auth/auth-check
 * Verify the current access token is valid.
 */
export async function authCheck(): Promise<ApiResponse> {
  const { data } = await axiosInstance.get<ApiResponse>('/auth/auth-check');
  return data;
}

/**
 * GET /auth/me
 * Retrieves the current authenticated user's effective permissions.
 */
export async function getMe(): Promise<ApiResponse<string[]>> {
  const { data } = await axiosInstance.get<ApiResponse<string[]>>('/auth/me');
  return data;
}
//#endregion

//#region POST
/**
 * POST /auth/login
 * Returns { data: AuthResponse } on success.
 * The refresh token is set as an httpOnly cookie by the backend.
 */
export async function login(payload: LoginPayload): Promise<ApiResponse<AuthResponse | string>> {
  const { data } = await axiosInstance.post<ApiResponse<AuthResponse | string>>('/auth/login', payload);
  return data;
}

/**
 * POST /auth/logout
 * Revokes the refresh token on the server and clears the cookie.
 */
export async function logout(): Promise<ApiResponse> {
  const { data } = await axiosInstance.post<ApiResponse>('/auth/logout');
  return data;
}

/**
 * POST /auth/refresh-token
 * Sends the expired access token in the Authorization header
 * and the refresh token via httpOnly cookie.
 * Returns a new access token.
 */
export async function refreshToken(): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/auth/refresh-token');
  return data;
}

export interface ForgotPasswordPayload {
  email: string;
}

export interface ResetPasswordPayload {
  token: string;
  newPassword: string;
  confirmPassword: string;
}

/**
 * POST /auth/forgot-password
 */
export async function forgotPassword(payload: ForgotPasswordPayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/auth/forgot-password', payload);
  return data;
}

/**
 * POST /auth/reset-password
 */
export async function resetPassword(payload: ResetPasswordPayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/auth/reset-password', payload);
  return data;
}

export interface ChangeFirstTimePasswordPayload {
  email: string;
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

/**
 * POST /auth/change-first-time-password
 */
export async function changeFirstTimePassword(payload: ChangeFirstTimePasswordPayload): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/auth/change-first-time-password', payload);
  return data;
}

/**
 * POST /auth/verify-first-time-login-email
 */
export async function verifyFirstTimeLoginEmail(payload: { email: string }): Promise<ApiResponse<string>> {
  const { data } = await axiosInstance.post<ApiResponse<string>>('/auth/verify-first-time-login-email', payload);
  return data;
}

/**
 * POST /auth/mfa/setup
 * Initiates MFA TOTP secret key generation and QR provisioning URL.
 */
export async function setupMfa(): Promise<ApiResponse<MfaSetupDto>> {
  const { data } = await axiosInstance.post<ApiResponse<MfaSetupDto>>('/auth/mfa/setup');
  return data;
}

/**
 * POST /auth/mfa/verify
 * Verifies initial TOTP code to enable MFA.
 */
export async function verifyMfa(code: string): Promise<ApiResponse<boolean>> {
  const { data } = await axiosInstance.post<ApiResponse<boolean>>('/auth/mfa/verify', { code });
  return data;
}

/**
 * POST /auth/logout-all
 * Revokes all active refresh tokens for the current user.
 */
export async function logoutAll(): Promise<ApiResponse> {
  const { data } = await axiosInstance.post<ApiResponse>('/auth/logout-all');
  return data;
}
//#endregion;