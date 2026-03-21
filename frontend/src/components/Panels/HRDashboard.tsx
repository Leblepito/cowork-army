import { useState, useEffect } from 'react';
import { getHRPerformance, getHRProposals, hrSpawnAgent, approveProposal } from '../../services/api';
import type { AgentPerformanceDto, HRProposal } from '../../types';
import { useTranslation, useCoworkStore } from '../../stores/useCoworkStore';
import { ConfirmDialog } from '../common/ConfirmDialog';

interface Props { onClose: () => void; }

export function HRDashboard({ onClose }: Props) {
  const { t } = useTranslation();
  const agents = useCoworkStore((s) => s.agents);

  const [performances, setPerformances] = useState<AgentPerformanceDto[]>([]);
  const [proposals, setProposals] = useState<HRProposal[]>([]);
  const [spawnReason, setSpawnReason] = useState('');
  const [loading, setLoading] = useState(false);
  const [confirmProposal, setConfirmProposal] = useState<string | null>(null);

  // Dynamic department list derived from agents in the store
  const depts = [...new Set(agents.map((a) => a.department))];
  const [spawnDept, setSpawnDept] = useState('');

  // Set default dept when agents load
  useEffect(() => {
    if (depts.length > 0 && !spawnDept) setSpawnDept(depts[0]);
  }, [depts, spawnDept]);

  const refresh = () => {
    getHRPerformance().then(setPerformances).catch(console.error);
    getHRProposals().then(setProposals).catch(console.error);
  };

  useEffect(() => {
    refresh();
    const i = setInterval(refresh, 15000);
    return () => clearInterval(i);
  }, []);

  const handleSpawn = async () => {
    if (!spawnReason.trim()) return;
    setLoading(true);
    try {
      await hrSpawnAgent(spawnReason, spawnDept);
      setSpawnReason('');
      refresh();
    } catch (e) {
      console.error(e);
    } finally {
      setLoading(false);
    }
  };

  const handleConfirmApprove = () => {
    if (!confirmProposal) return;
    approveProposal(confirmProposal)
      .then(refresh)
      .catch(console.error)
      .finally(() => setConfirmProposal(null));
  };

  const gradeColor = (g: string) =>
    ({ A: 'text-green-400', B: 'text-blue-400', C: 'text-yellow-400', D: 'text-orange-400', F: 'text-red-400' }[g] || 'text-gray-400');

  return (
    <>
      <div className="absolute top-3 right-3 w-full md:w-[380px] max-h-[80vh] bg-[#0b0c14ee] border border-[#1a1f30] rounded-xl backdrop-blur-xl z-20 flex flex-col overflow-hidden">
        <div className="flex items-center justify-between p-3 border-b border-gray-800">
          <span className="font-mono font-semibold text-[10px] tracking-[1.5px] text-emerald-400">
            🧑‍💼 {t('hr_dashboard')}
          </span>
          <button onClick={onClose} className="text-gray-500 hover:text-white text-sm">✕</button>
        </div>

        <div className="flex-1 overflow-y-auto p-3 space-y-3">
          {/* Spawn section */}
          <div>
            <div className="text-[8px] font-mono text-gray-500 tracking-[2px] mb-1">{t('spawn_agent')}</div>
            <div className="flex gap-1.5 mb-1.5">
              <input
                value={spawnReason}
                onChange={(e) => setSpawnReason(e.target.value)}
                placeholder={t('spawn_reason')}
                disabled={loading}
                className="flex-1 bg-[#0a0d16] border border-gray-800 rounded px-2 py-1 text-[10px] text-white outline-none disabled:opacity-50"
              />
              <select
                value={spawnDept}
                onChange={(e) => setSpawnDept(e.target.value)}
                className="bg-[#0a0d16] border border-gray-800 rounded px-1.5 py-1 text-[10px] text-white"
              >
                {depts.map((d) => (
                  <option key={d} value={d}>{d}</option>
                ))}
                {depts.length === 0 && <option value="">—</option>}
              </select>
            </div>
            <button
              onClick={handleSpawn}
              disabled={loading || !spawnReason.trim()}
              className="w-full font-mono text-[9px] py-1 rounded bg-emerald-500/15 text-emerald-400 border border-emerald-500/30 disabled:opacity-30"
            >
              {loading ? t('designing') : t('spawn')}
            </button>
          </div>

          {/* Proposals */}
          {proposals.length > 0 && (
            <div>
              <div className="text-[8px] font-mono text-gray-500 tracking-[2px] mb-1">
                {t('proposals')} ({proposals.length})
              </div>
              {proposals.map((p) => (
                <div key={p.id} className="flex items-center gap-2 bg-gray-900/50 rounded px-2 py-1.5 mb-1">
                  <span className="text-[9px] text-gray-300 flex-1">
                    {p.type}: {p.reason.slice(0, 40)}
                  </span>
                  <button
                    onClick={() => setConfirmProposal(p.id)}
                    className="text-[8px] text-green-400 hover:text-green-300"
                  >
                    {t('approve')}
                  </button>
                </div>
              ))}
            </div>
          )}

          {/* Performance */}
          <div>
            <div className="text-[8px] font-mono text-gray-500 tracking-[2px] mb-1">{t('performance')}</div>
            {performances.sort((a, b) => b.tasksCompleted - a.tasksCompleted).map((p) => (
              <div key={p.agentId} className="flex items-center gap-2 text-[9px] py-0.5">
                <span className="text-gray-400 w-24 truncate">{p.agentId}</span>
                <span className={`${gradeColor(p.grade)} font-bold w-4`}>{p.grade}</span>
                <span className="text-gray-500">✅{p.tasksCompleted} ❌{p.tasksFailed}</span>
                <span className="text-gray-600 ml-auto">${p.estimatedCost.toFixed(2)}</span>
                {p.warnings > 0 && <span className="text-red-400">⚠{p.warnings}</span>}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Confirm approve dialog */}
      <ConfirmDialog
        open={confirmProposal !== null}
        message={t('confirm.killAgent')}
        onConfirm={handleConfirmApprove}
        onCancel={() => setConfirmProposal(null)}
      />
    </>
  );
}
