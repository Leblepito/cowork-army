import { useTranslation } from '../../stores/useCoworkStore';

interface LoadingSpinnerProps {
  /** Override the displayed text */
  label?: string;
  /** Additional wrapper class names */
  className?: string;
}

export function LoadingSpinner({ label, className = '' }: LoadingSpinnerProps) {
  const { t } = useTranslation();
  const text = label ?? t('scene.loading');

  return (
    <div
      className={`flex flex-col items-center justify-center gap-4 select-none ${className}`}
      role="status"
      aria-label={text}
    >
      {/* Crown + pulse ring */}
      <div className="relative flex items-center justify-center">
        {/* Animated ring */}
        <span className="absolute inline-flex h-16 w-16 rounded-full bg-blue-500/20 animate-ping" />
        {/* Crown emoji */}
        <span className="relative text-4xl leading-none" aria-hidden="true">
          👑
        </span>
      </div>

      {/* Label */}
      <p className="text-gray-400 text-sm font-medium tracking-wide animate-pulse">
        {text}
      </p>
    </div>
  );
}

export default LoadingSpinner;
