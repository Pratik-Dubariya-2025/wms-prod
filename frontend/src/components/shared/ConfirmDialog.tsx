import { Button } from '@/components/ui/Button/Button';
import { Modal } from '@/components/ui/Modal/Modal';
import { AlertTriangle } from 'lucide-react';

interface ConfirmDialogProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title?: string;
  message?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  isLoading?: boolean;
  variant?: 'danger' | 'warning';
}

/**
 * Reusable "Are you sure?" confirmation dialog.
 */
export function ConfirmDialog({
  isOpen,
  onClose,
  onConfirm,
  title = 'Confirm Action',
  message = 'Are you sure you want to proceed? This action cannot be undone.',
  confirmLabel = 'Confirm',
  cancelLabel = 'Cancel',
  isLoading = false,
  variant = 'danger',
}: ConfirmDialogProps) {
  return (
    <Modal isOpen={isOpen} onClose={onClose} size="sm">
      <div className="flex flex-col items-center text-center gap-4 py-2">
        <div
          className={`p-3 rounded-full ${
            variant === 'danger' ? 'bg-wms-danger/10' : 'bg-wms-warning/10'
          }`}
        >
          <AlertTriangle
            className={`h-6 w-6 ${
              variant === 'danger' ? 'text-wms-danger' : 'text-wms-warning'
            }`}
          />
        </div>

        <div>
          <h3 className="text-lg font-bold text-wms-text mb-1">{title}</h3>
          <p className="text-sm text-wms-secondary">{message}</p>
        </div>

        <div className="flex items-center gap-3 w-full pt-2">
          <Button variant="secondary" onClick={onClose} fullWidth disabled={isLoading}>
            {cancelLabel}
          </Button>
          <Button
            variant={variant === 'danger' ? 'danger' : 'primary'}
            onClick={onConfirm}
            fullWidth
            isLoading={isLoading}
          >
            {confirmLabel}
          </Button>
        </div>
      </div>
    </Modal>
  );
}
