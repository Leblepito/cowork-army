import { useState } from 'react';
import type { Agent } from '../../types';
import { STATUS_COLORS } from '../../constants/colors';
import { useTranslation } from '../../stores/useCoworkStore';
import { spawnAgent, killAgent } from '../../services/api';

interface Props {
  agent: Agent;
  status: string;
  onClose: () => void;
  onChat: () => void;
}

const MAX_TASK_LENGTH = 500;

export function DetailPanel({ agent, status, onClose, onChat }: Props) {
  const { t } = useTranslation();
  const [taskInput, setTaskInput] = useState('');
  const [assigning, setAssigning] = useState(false);
  const isActive = status !== 'idle';
  const skills: string[] = (() => {
    try {
      const parsed = JSON.parse(agent.skills || '[]');
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  })();

  const handleAssign = async () => {
    const cleaned = taskInput.trim().replace(/<[^>]*>/g, '');
    if (!cleaned || cleaned.length > MAX_TASK_LENGTH || assigning) return;
    setAssigning(true);
    try {
      await spawnAgent(agent.id, cleaned);
      setTaskInput('');
    } finally {
      setAssigning(false);
    }
  };

  return (
    <div
      className="panel absolute top-3 left-3 w-full md:w-[280px] z-20"
      style={{ padding: 'var(--space-lg)' }}
    >
      {/* Header */}
      <div className="flex items-center" style={{ gap: 'var(--space-md)', marginBottom: 'var(--space-md)' }}>
        <span style={{ fontSize: '28px' }}>{agent.icon}</span>
        <div className="flex-1">
          <div className="font-bold" style={{ fontSize: 'var(--text-heading)', color: agent.color }}>
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

      {/* Description */}
      <div style={{ fontSize: 'var(--text-body)', color: 'var(--text-secondary)', marginBottom: 'var(--space-sm)', lineHeight: '1.6' }}>
        {agent.description}
      </div>

      {/* Status bar */}
      <div
        className="flex items-center"
        style={{
          gap: 'var(--space-sm)',
          background: 'var(--bg-surface)',
          borderRadius: '6px',
          padding: 'var(--space-sm) var(--space-md)',
          marginBottom: 'var(--space-md)',
        }}
      >
        <div
          className={`status-dot ${isActive ? 'active' : ''}`}
          style={{ background: STATUS_COLORS[status] ?? STATUS_COLORS['idle'], color: STATUS_COLORS[status] ?? STATUS_COLORS['idle'] }}
        />
        <span className="font-medium" style={{ fontSize: 'var(--text-body)', color: 'var(--text-primary)' }}>
          {status}
        </span>
        {isActive && (
          <span className="ml-auto" style={{ fontSize: 'var(--text-caption)', color: 'var(--accent-green)' }}>
            &#x25CF; LIVE
          </span>
        )}
      </div>

      {/* Task input */}
      <div className="panel-title" style={{ marginBottom: 'var(--space-xs)' }}>
        {t('assign_task')}
      </div>
      <div className="flex" style={{ gap: 'var(--space-xs)', marginBottom: 'var(--space-md)' }}>
        <input
          value={taskInput}
          onChange={(e) => setTaskInput(e.target.value)}
          onKeyDown={(e) => e.key === 'Enter' && handleAssign()}
          placeholder={t('write_task')}
          disabled={assigning}
          className="input flex-1"
          style={{ minHeight: '32px' }}
          maxLength={MAX_TASK_LENGTH}
        />
        <button
          onClick={handleAssign}
          disabled={assigning || !taskInput.trim()}
          className="btn btn-accent"
          style={{ opacity: assigning || !taskInput.trim() ? 0.3 : 1 }}
        >
          {assigning ? '\u23F3' : '\u25B6'}
        </button>
      </div>

      {/* Chat button */}
      <button
        onClick={onChat}
        className="btn btn-success w-full"
        style={{ justifyContent: 'center', marginBottom: 'var(--space-md)' }}
      >
        {t('chat')}
      </button>

      {/* Controls */}
      <div className="flex" style={{ gap: 'var(--space-sm)', marginBottom: 'var(--space-md)' }}>
        <button
          onClick={() => spawnAgent(agent.id)}
          className="btn btn-success"
        >
          {t('start')}
        </button>
        <button
          onClick={() => killAgent(agent.id)}
          className="btn btn-danger"
        >
          {t('stop')}
        </button>
      </div>

      {/* Skills */}
      <div className="panel-title" style={{ marginBottom: 'var(--space-xs)' }}>
        {t('skills')}
      </div>
      <div className="flex flex-wrap" style={{ gap: 'var(--space-xs)', marginBottom: 'var(--space-md)' }}>
        {skills.map((s) => (
          <span
            key={s}
            style={{
              fontSize: 'var(--text-caption)',
              padding: '2px var(--space-sm)',
              borderRadius: '4px',
              background: 'rgba(168, 85, 247, 0.1)',
              color: 'var(--accent-purple)',
              border: '1px solid rgba(168, 85, 247, 0.2)',
            }}
          >
            {s}
          </span>
        ))}
      </div>
    </div>
  );
}
