import { useState, useEffect } from 'react';
import { ShieldAlert, ShieldCheck, QrCode, Copy, Check, LogOut } from 'lucide-react';
import { setupMfa, verifyMfa, logoutAll } from '@/api/authApi';
import { Button } from '@/components/ui/Button/Button';
import { Input } from '@/components/ui/Input/Input';
import { toast } from '@/components/ui/Toast/Toast';
import { Modal } from '@/components/ui/Modal/Modal';
import { useCurrentUser } from '@/hooks/useCurrentUser';

interface SecuritySettingsDialogProps {
  isOpen: boolean;
  onClose: () => void;
}

export function SecuritySettingsDialog({ isOpen, onClose }: SecuritySettingsDialogProps) {
  const user = useCurrentUser();
  const [isMfaActive, setIsMfaActive] = useState(false);
  const [setupStep, setSetupStep] = useState<'idle' | 'showing_qr' | 'success'>('idle');
  const [qrCodeUrl, setQrCodeUrl] = useState('');
  const [sharedSecret, setSharedSecret] = useState('');
  const [verificationCode, setVerificationCode] = useState('');
  const [isVerifying, setIsVerifying] = useState(false);
  const [isSettingUp, setIsSettingUp] = useState(false);
  const [isLoggingOutAll, setIsLoggingOutAll] = useState(false);
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    if (user) {
      setIsMfaActive(!!(user as any).isMfaEnabled);
    }
  }, [user, isOpen]);

  async function handleSetupMfa() {
    setIsSettingUp(true);
    try {
      const response = await setupMfa();
      if (response.succeeded && response.data) {
        setQrCodeUrl(response.data.qrCodeUrl);
        setSharedSecret(response.data.sharedSecret);
        setSetupStep('showing_qr');
        toast.success('MFA registration initiated.');
      } else {
        toast.error(response.message || 'Failed to initiate MFA setup.');
      }
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to initiate MFA setup.');
    } finally {
      setIsSettingUp(false);
    }
  }

  async function handleVerifyMfa() {
    if (!verificationCode || verificationCode.length !== 6) {
      toast.error('Please enter a 6-digit code.');
      return;
    }
    setIsVerifying(true);
    try {
      const response = await verifyMfa(verificationCode);
      if (response.succeeded) {
        setIsMfaActive(true);
        setSetupStep('success');
        toast.success('Multi-Factor Authentication enabled successfully! Please log in again to apply settings.');
      } else {
        toast.error(response.message || 'Invalid verification code.');
      }
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Verification failed.');
    } finally {
      setIsVerifying(false);
    }
  }

  async function handleLogoutAll() {
    setIsLoggingOutAll(true);
    try {
      const response = await logoutAll();
      if (response.succeeded) {
        toast.success('Successfully logged out of all other active sessions.');
      } else {
        toast.error(response.message || 'Failed to logout of other sessions.');
      }
    } catch (err: any) {
      toast.error(err.response?.data?.message || 'Failed to perform session revocation.');
    } finally {
      setIsLoggingOutAll(false);
    }
  }

  function handleCopySecret() {
    navigator.clipboard.writeText(sharedSecret);
    setCopied(true);
    toast.success('Secret copied to clipboard');
    setTimeout(() => setCopied(false), 2000);
  }

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title="Security & MFA Settings"
      size="md"
    >
      <div className="flex flex-col gap-6 pt-2">
        {isMfaActive ? (
          <div className="flex flex-col items-center gap-4 text-center">
            <div className="inline-flex items-center justify-center h-16 w-16 rounded-full bg-wms-success/20 text-wms-success">
              <ShieldCheck className="h-10 w-10" />
            </div>
            <div>
              <h3 className="text-lg font-bold text-wms-text">MFA is Enabled</h3>
              <p className="text-xs text-wms-secondary mt-1 max-w-xs">
                Your account is protected with Two-Factor TOTP (Google Authenticator).
              </p>
            </div>

            <div className="w-full h-px bg-wms-hover my-2" />

            <div className="w-full flex flex-col gap-3">
              <h4 className="text-sm font-semibold text-wms-text text-left">Session Management</h4>
              <p className="text-xs text-wms-secondary text-left">
                Revoke authorization for all other browser instances, mobile logins, and active sessions.
              </p>
              <Button
                onClick={handleLogoutAll}
                isLoading={isLoggingOutAll}
                variant="secondary"
                className="w-full flex items-center justify-center gap-2 border-wms-border text-wms-text hover:bg-wms-danger/10 hover:text-wms-danger hover:border-wms-danger/20"
              >
                <LogOut className="h-4 w-4" />
                Logout From All Other Sessions
              </Button>
            </div>
          </div>
        ) : (
          <div className="flex flex-col gap-5">
            {setupStep === 'idle' && (
              <div className="flex flex-col items-center gap-4 text-center">
                <div className="inline-flex items-center justify-center h-16 w-16 rounded-full bg-wms-warning/20 text-wms-warning animate-pulse">
                  <ShieldAlert className="h-10 w-10" />
                </div>
                <div>
                  <h3 className="text-lg font-bold text-wms-text">MFA is Disabled</h3>
                  <p className="text-xs text-wms-secondary mt-1">
                    Improve your account security by requiring a 6-digit TOTP code during sign-in.
                  </p>
                </div>
                <Button
                  onClick={handleSetupMfa}
                  isLoading={isSettingUp}
                  fullWidth
                  size="lg"
                  className="mt-2 bg-gradient-to-r from-wms-indigo to-wms-cyan hover:opacity-90 transition duration-300"
                >
                  Set Up Authenticator
                </Button>
              </div>
            )}

            {setupStep === 'showing_qr' && (
              <div className="flex flex-col gap-4">
                <div className="text-center">
                  <h4 className="text-sm font-bold text-wms-text">Scan QR Code</h4>
                  <p className="text-xs text-wms-secondary mt-1">
                    Scan the QR code below with your authenticator app (Google Authenticator, Microsoft Authenticator, Authy).
                  </p>
                </div>

                {/* QR Code Frame */}
                <div className="flex flex-col items-center justify-center p-4 bg-white rounded-xl border border-wms-border self-center">
                  {qrCodeUrl ? (
                    <img
                      src={`https://api.qrserver.com/v1/create-qr-code/?data=${encodeURIComponent(qrCodeUrl)}&size=180x180`}
                      alt="TOTP Setup QR Code"
                      className="w-44 h-44"
                    />
                  ) : (
                    <div className="w-44 h-44 flex items-center justify-center text-wms-muted">
                      <QrCode className="h-12 w-12" />
                    </div>
                  )}
                </div>

                {/* Shared Secret display */}
                <div className="flex flex-col gap-1.5">
                  <span className="text-xs text-wms-secondary font-medium">Cannot scan? Enter code manually:</span>
                  <div className="flex items-center justify-between p-2.5 rounded-lg bg-wms-hover border border-wms-border font-mono text-sm text-wms-text">
                    <span className="select-all break-all">{sharedSecret}</span>
                    <button
                      onClick={handleCopySecret}
                      className="p-1 rounded text-wms-muted hover:text-wms-text transition cursor-pointer"
                      title="Copy to clipboard"
                    >
                      {copied ? <Check className="h-4 w-4 text-wms-success" /> : <Copy className="h-4 w-4" />}
                    </button>
                  </div>
                </div>

                <div className="w-full h-px bg-wms-hover" />

                {/* Verification code input */}
                <div className="flex flex-col gap-3">
                  <Input
                    label="Verify Authentication Code"
                    type="text"
                    placeholder="000000"
                    maxLength={6}
                    value={verificationCode}
                    onChange={(e) => setVerificationCode(e.target.value.replace(/\D/g, ''))}
                    className="text-center tracking-widest text-lg font-mono"
                    autoComplete="one-time-code"
                    id="mfa-verify-input"
                  />
                  <Button
                    onClick={handleVerifyMfa}
                    isLoading={isVerifying}
                    fullWidth
                    size="lg"
                  >
                    Verify & Enable MFA
                  </Button>
                </div>
              </div>
            )}

            {setupStep === 'success' && (
              <div className="flex flex-col items-center gap-4 text-center">
                <div className="inline-flex items-center justify-center h-16 w-16 rounded-full bg-wms-success/20 text-wms-success">
                  <ShieldCheck className="h-10 w-10" />
                </div>
                <div>
                  <h3 className="text-lg font-bold text-wms-text">Configuration Complete!</h3>
                  <p className="text-xs text-wms-secondary mt-1">
                    MFA is now successfully bound. Note that to complete enrollment on current login token, you will need to sign in again.
                  </p>
                </div>
                <Button
                  onClick={() => {
                    onClose();
                    setSetupStep('idle');
                  }}
                  fullWidth
                  size="lg"
                >
                  Done
                </Button>
              </div>
            )}
          </div>
        )}
      </div>
    </Modal>
  );
}
