import { useCoworkStore } from '../../stores/useCoworkStore';

const TOAST_STYLES = {
  error: 'bg-red-500/90 border-red-400',
  success: 'bg-emerald-500/90 border-emerald-400',
  info: 'bg-blue-500/90 border-blue-400',
};

export function ToastContainer() {
  const toasts = useCoworkStore((s) => s.toasts);
  const removeToast = useCoworkStore((s) => s.removeToast);

  if (toasts.length === 0) return null;

  return (
    <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2 max-w-sm">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          className={`${TOAST_STYLES[toast.type]} border rounded-lg px-4 py-2.5 text-white text-xs font-mono shadow-lg flex items-start gap-2`}
        >
          <span className="flex-1">{toast.message}</span>
          <button
            onClick={() => removeToast(toast.id)}
            className="text-white/70 hover:text-white shrink-0"
          >
            ✕
          </button>
        </div>
      ))}
    </div>
  );
}
