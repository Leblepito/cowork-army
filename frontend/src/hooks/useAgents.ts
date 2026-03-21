import { useState, useEffect, useCallback } from 'react';
import type { Agent } from '../types';
import { getAgents, getAllStatuses } from '../services/api';

export function useAgents() {
  const [agents, setAgents] = useState<Agent[]>([]);
  const [statuses, setStatuses] = useState<Record<string, string>>({});
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([getAgents(), getAllStatuses()])
      .then(([agentList, statusMap]) => {
        setAgents(agentList);
        const stMap: Record<string, string> = {};
        Object.entries(statusMap).forEach(([id, st]) => {
          stMap[id] = st.status;
        });
        setStatuses(stMap);
      })
      .catch(console.error)
      .finally(() => setLoading(false));
  }, []);

  const updateStatus = useCallback((agentId: string, status: string) => {
    setStatuses((prev) => ({ ...prev, [agentId]: status }));
  }, []);

  const activeCount = Object.values(statuses).filter((s) => s !== 'idle').length;
  const idleCount = agents.length - activeCount;

  return { agents, statuses, updateStatus, loading, activeCount, idleCount };
}
