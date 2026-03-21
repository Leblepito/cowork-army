import { useCoworkStore } from '../../stores/useCoworkStore';

export function ConnectionBanner() {
  const connectionState = useCoworkStore((s) => s.connectionState);

  if (connectionState === 'connected') return null;

  const config = {
    reconnecting: {
      bg: 'bg-amber-500/90',
      text: 'Reconnecting to server...',
      icon: '⟳',
    },
    disconnected: {
      bg: 'bg-red-500/90',
      text: 'Disconnected from server',
      icon: '⚠',
    },
  }[connectionState];

  return (
    <div
      className={`fixed top-0 left-0 right-0 z-50 ${config.bg} text-white text-center py-1.5 text-xs font-mono`}
    >
      {config.icon} {config.text}
    </div>
  );
}
