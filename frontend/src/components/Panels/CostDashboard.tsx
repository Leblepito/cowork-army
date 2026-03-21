import { useState, useEffect } from 'react';
import { getBudgetStatus } from '../../services/api';
import { useTranslation, useCoworkStore } from '../../stores/useCoworkStore';

interface Props {
  onClose: () => void;
}

export function CostDashboard({ onClose }: Props) {
  const { t } = useTranslation();
  const agents = useCoworkStore((s) => s.agents);

  const [budget, setBudget] = useState<{
    globalTodayUsd: number;
    globalCapUsd: number;
    agentHourUsd: Record<string, number>;
    agentCapUsd: number;
  } | null>(null);

  useEffect(() => {
    getBudgetStatus().then(setBudget).catch(console.error);
    const interval = setInterval(() => {
      getBudgetStatus().then(setBudget).catch(console.error);
    }, 10000);
    return () => clearInterval(interval);
  }, []);

  if (!budget || budget.globalCapUsd === 0) {
    return (
      <div className="absolute bottom-14 left-3 w-full md:w-[300px] bg-[#0b0c14ee] border border-[#1a1f30] rounded-xl p-3.5 backdrop-blur-xl z-20">
        <div className="flex items-center justify-between mb-2">
          <span className="font-mono font-semibold text-[10px] tracking-[1.5px] text-gray-400">
            {t('cost_dashboard')}
          </span>
          <button onClick={onClose} className="text-gray-500 hover:text-white text-sm">✕</button>
        </div>
        <div className="text-[10px] text-gray-600 font-mono">{t('no_budget')}</div>
      </div>
    );
  }

  const globalPercent = Math.min(100, (budget.globalTodayUsd / budget.globalCapUsd) * 100);
  const topAgents = Object.entries(budget.agentHourUsd)
    .sort((a, b) => b[1] - a[1])
    .slice(0, 5);

  return (
    <div className="absolute bottom-14 left-3 w-full md:w-[300px] bg-[#0b0c14ee] border border-[#1a1f30] rounded-xl p-3.5 backdrop-blur-xl z-20">
      <div className="flex items-center justify-between mb-3">
        <span className="font-mono font-semibold text-[10px] tracking-[1.5px] text-gray-400">
          {t('cost_dashboard')}
        </span>
        <button onClick={onClose} className="text-gray-500 hover:text-white text-sm">
          ✕
        </button>
      </div>

      {/* Global budget bar */}
      <div className="mb-3">
        <div className="flex justify-between text-[9px] text-gray-400 mb-1">
          <span>{t('global_today')}</span>
          <span>
            ${budget.globalTodayUsd.toFixed(2)} / ${budget.globalCapUsd.toFixed(2)}
          </span>
        </div>
        <div className="w-full h-1.5 bg-gray-800 rounded-full overflow-hidden">
          <div
            className="h-full rounded-full transition-all"
            style={{
              width: `${globalPercent}%`,
              background:
                globalPercent > 80
                  ? '#ef4444'
                  : globalPercent > 50
                    ? '#f59e0b'
                    : '#22c55e',
            }}
          />
        </div>
      </div>

      {/* Top spending agents */}
      {topAgents.length > 0 && (
        <div>
          <div className="text-[8px] font-mono text-gray-500 tracking-[2px] mb-1.5">
            {t('top_agents')}
          </div>
          {topAgents.map(([id, cost]) => {
            const agentName = agents.find((a) => a.id === id)?.name || id;
            return (
              <div key={id} className="flex justify-between text-[9px] py-0.5">
                <span className="text-gray-400 truncate max-w-[60%]">{agentName}</span>
                <span
                  className={
                    cost > budget.agentCapUsd * 0.8 ? 'text-red-400' : 'text-gray-500'
                  }
                >
                  ${cost.toFixed(3)}
                </span>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
