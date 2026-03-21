import { useState, useRef, useEffect } from 'react';
import { useTranslation } from '../../stores/useCoworkStore';

const U2ALGO_EVENT_STYLES: Record<string, { icon: string; color: string }> = {
  trade_opened:  { icon: '📈', color: '#22c55e' },
  trade_closed:  { icon: '📉', color: '#ef4444' },
  trade_tp1:     { icon: '🎯', color: '#fbbf24' },
  hedge_opened:  { icon: '🛡️', color: '#f59e0b' },
  hedge_closed:  { icon: '🛡️', color: '#f59e0b' },
  bot_started:   { icon: '🤖', color: '#06b6d4' },
  bot_stopped:   { icon: '🤖', color: '#06b6d4' },
  agent_message: { icon: '💬', color: '#a855f7' },
  status_change: { icon: '🔄', color: '#6b7280' },
};

interface LogEntry {
  icon: string;
  message: string;
  time: string;
  source?: string;
}

interface Props {
  entries: LogEntry[];
}

export function EventLog({ entries }: Props) {
  const { t } = useTranslation();
  const [search, setSearch] = useState('');
  const [locked, setLocked] = useState(false);
  const [hasNew, setHasNew] = useState(false);
  const scrollRef = useRef<HTMLDivElement>(null);
  const prevLengthRef = useRef(entries.length);

  const filtered = search.trim()
    ? entries.filter((e) => e.message.toLowerCase().includes(search.toLowerCase()))
    : entries;

  const handleScroll = () => {
    const el = scrollRef.current;
    if (!el) return;
    const atBottom = el.scrollTop + el.clientHeight + 50 >= el.scrollHeight;
    setLocked(!atBottom);
    if (atBottom) setHasNew(false);
  };

  useEffect(() => {
    const el = scrollRef.current;
    if (!el) return;
    const newCount = entries.length;
    if (newCount > prevLengthRef.current) {
      if (locked) {
        setHasNew(true);
      } else {
        el.scrollTop = el.scrollHeight;
      }
    }
    prevLengthRef.current = newCount;
  }, [entries, locked]);

  const scrollToBottom = () => {
    const el = scrollRef.current;
    if (!el) return;
    el.scrollTo({ top: el.scrollHeight, behavior: 'smooth' });
    setHasNew(false);
    setLocked(false);
  };

  return (
    <div
      className="w-[200px] xl:w-[220px] flex flex-col shrink-0"
      style={{
        background: 'var(--bg-base)',
        borderLeft: '1px solid var(--border-dim)',
      }}
    >
      {/* Header */}
      <div className="panel-header">
        <span className="panel-title">{t('event_log')}</span>
      </div>

      {/* Search */}
      <div style={{ padding: 'var(--space-sm)', borderBottom: '1px solid var(--border-dim)' }}>
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('search_events')}
          className="input"
        />
      </div>

      {/* Entries */}
      <div
        ref={scrollRef}
        className="flex-1 overflow-y-auto relative"
        onScroll={handleScroll}
      >
        {filtered.length === 0 && (
          <div style={{ padding: 'var(--space-md)', fontSize: 'var(--text-body)', color: 'var(--text-dim)' }}>
            {entries.length === 0 ? t('waiting') : t('label.noEvents')}
          </div>
        )}
        {filtered.map((e, i) => {
          const isU2Algo = e.source === 'u2algo';
          // Try to extract event type from message for u2Algo styling
          const u2Style = isU2Algo
            ? Object.entries(U2ALGO_EVENT_STYLES).find(([key]) => e.message.includes(key))?.[1]
            : undefined;

          return (
            <div
              key={i}
              style={{
                padding: 'var(--space-sm) var(--space-md)',
                borderBottom: '1px solid var(--border-dim)',
                fontSize: 'var(--text-body)',
                borderLeft: isU2Algo ? `3px solid ${u2Style?.color ?? '#06b6d4'}` : undefined,
              }}
            >
              <span>{u2Style?.icon ?? e.icon}</span>{' '}
              <span style={{ color: isU2Algo ? (u2Style?.color ?? '#06b6d4') : 'var(--text-secondary)' }}>
                {e.message}
              </span>
              {isU2Algo && (
                <span style={{ fontSize: 'var(--text-caption)', color: '#06b6d4', marginLeft: '4px' }}>u2</span>
              )}
              <div style={{ fontSize: 'var(--text-caption)', color: 'var(--text-dim)', marginTop: '2px' }}>
                {e.time}
              </div>
            </div>
          );
        })}
      </div>

      {/* "New events" badge */}
      {hasNew && (
        <button
          onClick={scrollToBottom}
          className="btn btn-accent btn-compact"
          style={{
            margin: 'var(--space-sm)',
            justifyContent: 'center',
            width: 'calc(100% - var(--space-lg))',
          }}
        >
          {t('new_events')}
        </button>
      )}
    </div>
  );
}
