import { useEffect, useCallback } from 'react';
import { useTranslation } from '../../stores/useCoworkStore';

interface ConfirmDialogProps {
  /** Whether the dialog is visible */
  open: boolean;
  /** Dialog heading */
  title?: string;
  /** Body text */
  message: string;
  /** Called when the user confirms */
  onConfirm: () => void;
  /** Called when the user cancels or presses Esc */
  onCancel: () => void;
  /** Override the confirm button label */
  confirmLabel?: string;
  /** Override the cancel button label */
  cancelLabel?: string;
  /** Make the confirm button red (destructive action) */
  danger?: boolean;
}

export function ConfirmDialog({
  open,
  title,
  message,
  onConfirm,
  onCancel,
  confirmLabel,
  cancelLabel,
  danger = false,
}: ConfirmDialogProps) {
  const { t } = useTranslation();

  const resolvedTitle   = title         ?? t('confirm.title');
  const resolvedConfirm = confirmLabel  ?? t('action.confirm');
  const resolvedCancel  = cancelLabel   ?? t('action.cancel');

  // Keyboard: Esc → cancel, Enter → confirm
  const handleKeyDown = useCallback(
    (e: KeyboardEvent) => {
      if (!open) return;
      if (e.key === 'Escape') { e.preventDefault(); onCancel(); }
      if (e.key === 'Enter')  { e.preventDefault(); onConfirm(); }
    },
    [open, onCancel, onConfirm],
  );

  useEffect(() => {
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown]);

  if (!open) return null;

  return (
    /* Backdrop */
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm"
      onClick={onCancel}
      role="dialog"
      aria-modal="true"
      aria-labelledby="confirm-dialog-title"
    >
      {/* Panel — stop click propagation so backdrop click doesn't fire inside */}
      <div
        className="bg-gray-900 border border-gray-700 rounded-xl shadow-2xl w-full max-w-sm mx-4 p-6 flex flex-col gap-4"
        onClick={(e) => e.stopPropagation()}
      >
        {/* Title */}
        <h2 id="confirm-dialog-title" className="text-white text-lg font-semibold">
          {resolvedTitle}
        </h2>

        {/* Message */}
        <p className="text-gray-300 text-sm leading-relaxed">{message}</p>

        {/* Actions */}
        <div className="flex gap-3 justify-end pt-1">
          <button
            type="button"
            onClick={onCancel}
            className="
              px-4 py-2 rounded-lg text-sm font-medium
              bg-gray-700 text-gray-200
              hover:bg-gray-600 active:bg-gray-500
              focus-visible:outline focus-visible:outline-2 focus-visible:outline-blue-500
              transition-colors
            "
          >
            {resolvedCancel}
          </button>

          <button
            type="button"
            onClick={onConfirm}
            autoFocus
            className={`
              px-4 py-2 rounded-lg text-sm font-medium
              focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2
              transition-colors
              ${danger
                ? 'bg-red-600 text-white hover:bg-red-500 active:bg-red-700 focus-visible:outline-red-500'
                : 'bg-blue-600 text-white hover:bg-blue-500 active:bg-blue-700 focus-visible:outline-blue-500'
              }
            `}
          >
            {resolvedConfirm}
          </button>
        </div>
      </div>
    </div>
  );
}

export default ConfirmDialog;
