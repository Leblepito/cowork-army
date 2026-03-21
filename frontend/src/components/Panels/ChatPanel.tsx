import { useState, useRef, useEffect } from 'react';
import type { Agent } from '../../types';
import { useChat } from '../../hooks/useChat';
import { useTranslation } from '../../stores/useCoworkStore';

interface Props {
  agent: Agent;
  onClose: () => void;
}

export function ChatPanel({ agent, onClose }: Props) {
  const { t } = useTranslation();
  const { messages, loading, send } = useChat(agent.id);
  const [input, setInput] = useState('');
  const [copied, setCopied] = useState<number | null>(null);
  const scrollRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: 'smooth' });
  }, [messages]);

  const handleSend = () => {
    if (!input.trim() || loading) return;
    send(input.trim());
    setInput('');
  };

  const handleCopy = (content: string, idx: number) => {
    navigator.clipboard.writeText(content).then(() => {
      setCopied(idx);
      setTimeout(() => setCopied(null), 1500);
    });
  };

  return (
    <div className="panel absolute top-3 right-3 w-full md:w-[320px] h-[calc(100vh-80px)] md:h-[400px] z-20 flex flex-col">
      {/* Header */}
      <div className="panel-header">
        <span style={{ fontSize: '18px' }}>{agent.icon}</span>
        <div className="flex-1">
          <div className="font-bold" style={{ fontSize: 'var(--text-body)', color: agent.color }}>
            {agent.name}
          </div>
          <div style={{ fontSize: 'var(--text-caption)', color: 'var(--text-dim)' }}>
            {agent.tier} &bull; {agent.department}
          </div>
        </div>
        <button
          onClick={onClose}
          className="btn btn-compact"
          style={{ padding: 'var(--space-xs)', fontSize: 'var(--text-body)' }}
        >
          &#x2715;
        </button>
      </div>

      {/* Messages */}
      <div
        ref={scrollRef}
        className="flex-1 overflow-y-auto"
        style={{ padding: 'var(--space-md)', display: 'flex', flexDirection: 'column', gap: 'var(--space-sm)' }}
      >
        {messages.length === 0 && (
          <div className="text-center" style={{ fontSize: 'var(--text-body)', color: 'var(--text-dim)', marginTop: 'var(--space-xl)' }}>
            {agent.icon} {t('start_chat')}
          </div>
        )}
        {messages.map((msg, i) => (
          <div key={i} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
            <div
              style={{
                maxWidth: '85%',
                borderRadius: '8px',
                padding: 'var(--space-sm) var(--space-md)',
                fontSize: 'var(--text-body)',
                lineHeight: '1.6',
                background: msg.role === 'user'
                  ? 'rgba(0, 212, 255, 0.1)'
                  : 'var(--bg-elevated)',
                color: msg.role === 'user'
                  ? 'var(--accent-cyan)'
                  : 'var(--text-secondary)',
                border: `1px solid ${msg.role === 'user'
                  ? 'rgba(0, 212, 255, 0.2)'
                  : 'var(--border-dim)'}`,
              }}
            >
              {msg.content}
              <div className="flex items-center" style={{ gap: 'var(--space-sm)', marginTop: 'var(--space-xs)' }}>
                <span style={{ fontSize: 'var(--text-caption)', color: 'var(--text-dim)' }}>
                  {new Date(msg.timestamp).toLocaleTimeString()}
                </span>
                {msg.tokens != null && (
                  <span style={{ fontSize: 'var(--text-caption)', color: 'var(--text-muted)' }}>
                    {msg.tokens} tokens &bull; ${msg.cost?.toFixed(4)}
                  </span>
                )}
                {msg.role === 'assistant' && (
                  <button
                    onClick={() => handleCopy(msg.content, i)}
                    className="btn-compact ml-auto"
                    style={{
                      fontSize: 'var(--text-caption)',
                      color: 'var(--text-dim)',
                      background: 'none',
                      border: 'none',
                      cursor: 'pointer',
                      padding: '2px',
                      minHeight: 'unset',
                      minWidth: 'unset',
                    }}
                    title="Copy"
                  >
                    {copied === i ? '\u2713' : '\u2398'}
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
        {loading && (
          <div className="flex justify-start">
            <div
              className="animate-pulse"
              style={{
                background: 'var(--bg-elevated)',
                border: '1px solid var(--border-dim)',
                borderRadius: '8px',
                padding: 'var(--space-sm) var(--space-md)',
                fontSize: 'var(--text-body)',
                color: 'var(--text-secondary)',
              }}
            >
              {agent.icon} &hellip;
            </div>
          </div>
        )}
      </div>

      {/* Input */}
      <div style={{ padding: 'var(--space-sm)', borderTop: '1px solid var(--border-dim)' }}>
        <div className="flex" style={{ gap: 'var(--space-sm)' }}>
          <input
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && handleSend()}
            placeholder={t('type_message')}
            disabled={loading}
            className="input flex-1"
            style={{ minHeight: '32px', opacity: loading ? 0.5 : 1 }}
          />
          <button
            onClick={handleSend}
            disabled={loading || !input.trim()}
            className="btn btn-accent"
            style={{ opacity: loading || !input.trim() ? 0.3 : 1 }}
          >
            &#x25B6;
          </button>
        </div>
      </div>
    </div>
  );
}
