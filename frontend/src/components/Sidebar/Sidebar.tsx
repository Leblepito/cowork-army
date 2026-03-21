import { useState } from 'react';
import type { Agent } from '../../types';
import { STATUS_COLORS, DEPT_META } from '../../constants/colors';
import { useTranslation, useCoworkStore } from '../../stores/useCoworkStore';

interface Props {
  agents: Agent[];
  statuses: Record<string, string>;
  selectedId: string | null;
  onSelect: (id: string) => void;
  activeCount: number;
}

export function Sidebar({ agents, statuses, selectedId, onSelect, activeCount }: Props) {
  const { t } = useTranslation();
  const setLanguage = useCoworkStore((s) => s.setLanguage);
  const language = useCoworkStore((s) => s.language);
  const [search, setSearch] = useState('');

  const depts = [...new Set(agents.map((a) => a.department))];

  const filteredAgents = search.trim()
    ? agents.filter((a) => a.name.toLowerCase().includes(search.toLowerCase()))
    : agents;

  return (
    <div
      className="w-[200px] lg:w-[250px] flex flex-col shrink-0"
      style={{
        background: 'var(--bg-base)',
        borderRight: '1px solid var(--border-dim)',
      }}
    >
      {/* Header */}
      <div
        className="flex flex-col"
        style={{
          padding: 'var(--space-md) var(--space-lg)',
          borderBottom: '1px solid var(--border-dim)',
          borderTop: '2px solid rgba(0, 212, 255, 0.15)',
        }}
      >
        <h1
          className="font-semibold tracking-widest"
          style={{ fontSize: 'var(--text-heading)', color: 'var(--accent-cyan)' }}
        >
          COWORK.ARMY
        </h1>
        <p style={{ fontSize: 'var(--text-caption)', color: 'var(--text-dim)', marginTop: '2px', letterSpacing: '2px' }}>
          v9 &bull; {agents.length} {t('agents')} &bull; {activeCount} {t('active')}
        </p>
      </div>

      {/* Search */}
      <div style={{ padding: 'var(--space-sm) var(--space-md)', borderBottom: '1px solid var(--border-dim)' }}>
        <input
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder={t('search_agents')}
          className="input"
        />
      </div>

      {/* Agent list */}
      <div className="flex-1 overflow-y-auto" style={{ padding: 'var(--space-xs) 0' }}>
        {depts.map((dept) => {
          const deptAgents = filteredAgents.filter((a) => a.department === dept);
          if (!deptAgents.length) return null;
          const meta = DEPT_META[dept] || { label: dept, color: '#6b7280', colorInt: 0x6b7280, icon: '\u{1F916}' };

          return (
            <div key={dept}>
              <div
                className="flex items-center"
                style={{
                  padding: 'var(--space-xs) var(--space-lg)',
                  gap: 'var(--space-sm)',
                  marginTop: 'var(--space-sm)',
                }}
              >
                <div className="status-dot" style={{ background: meta.color }} />
                <span
                  className="font-semibold tracking-wider"
                  style={{ fontSize: 'var(--text-caption)', color: meta.color }}
                >
                  {meta.icon} {meta.label.toUpperCase()}
                </span>
                <span style={{ fontSize: 'var(--text-caption)', color: 'var(--text-dim)' }}>
                  {deptAgents.length}
                </span>
              </div>
              {deptAgents.map((agent) => {
                const st = statuses[agent.id] || 'idle';
                const isActive = st !== 'idle';
                const isSel = selectedId === agent.id;

                return (
                  <button
                    key={agent.id}
                    onClick={() => onSelect(agent.id)}
                    className="btn-compact gradient-border w-full flex items-center text-left transition-all"
                    style={{
                      padding: 'var(--space-sm) var(--space-lg)',
                      gap: 'var(--space-sm)',
                      borderLeft: isSel ? `2px solid ${agent.color}` : '2px solid transparent',
                      background: isSel ? 'var(--bg-surface)' : 'transparent',
                      color: isSel ? agent.color : 'var(--text-primary)',
                      borderRadius: 0,
                      border: 'none',
                      borderLeftStyle: 'solid',
                      borderLeftWidth: '2px',
                      borderLeftColor: isSel ? agent.color : 'transparent',
                      cursor: 'pointer',
                      minHeight: 'unset',
                      minWidth: 'unset',
                    }}
                  >
                    <span style={{ fontSize: '15px' }}>{agent.icon}</span>
                    <div className="flex-1 min-w-0">
                      <div
                        className="font-medium truncate"
                        style={{ fontSize: 'var(--text-body)' }}
                      >
                        {agent.name}
                      </div>
                      <div style={{ fontSize: 'var(--text-caption)', color: 'var(--text-dim)' }}>
                        {agent.tier}
                      </div>
                    </div>
                    <div className="flex items-center" style={{ gap: 'var(--space-xs)' }}>
                      <div
                        className={`status-dot ${isActive ? 'active' : ''}`}
                        style={{ background: STATUS_COLORS[st] ?? STATUS_COLORS['idle'], color: STATUS_COLORS[st] ?? STATUS_COLORS['idle'] }}
                      />
                      <span style={{ fontSize: 'var(--text-caption)', color: 'var(--text-dim)' }}>
                        {st}
                      </span>
                    </div>
                  </button>
                );
              })}
            </div>
          );
        })}

        {/* Empty state */}
        {filteredAgents.length === 0 && (
          <div className="text-center" style={{ padding: 'var(--space-md)', fontSize: 'var(--text-body)', color: 'var(--text-dim)', marginTop: 'var(--space-lg)' }}>
            {t('label.noAgents')}
          </div>
        )}
      </div>

      {/* Footer -- Language toggle */}
      <div
        className="flex items-center justify-between"
        style={{ padding: 'var(--space-sm)', borderTop: '1px solid var(--border-dim)' }}
      >
        <span style={{ fontSize: 'var(--text-caption)', color: 'var(--text-dim)' }}>
          {t('label.language')}
        </span>
        <div className="flex" style={{ gap: 'var(--space-xs)' }}>
          <button
            onClick={() => setLanguage('en')}
            className="btn btn-compact"
            style={{
              padding: '2px var(--space-sm)',
              fontSize: 'var(--text-caption)',
              ...(language === 'en'
                ? { background: 'rgba(0, 212, 255, 0.12)', borderColor: 'rgba(0, 212, 255, 0.3)', color: 'var(--accent-cyan)' }
                : {}),
            }}
          >
            EN
          </button>
          <button
            onClick={() => setLanguage('tr')}
            className="btn btn-compact"
            style={{
              padding: '2px var(--space-sm)',
              fontSize: 'var(--text-caption)',
              ...(language === 'tr'
                ? { background: 'rgba(239, 68, 68, 0.12)', borderColor: 'rgba(239, 68, 68, 0.3)', color: 'var(--accent-red)' }
                : {}),
            }}
          >
            TR
          </button>
        </div>
      </div>
    </div>
  );
}
