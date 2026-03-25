import { Suspense, useEffect } from 'react';
import { useCoworkStore } from './stores/useCoworkStore';
import { useSignalR } from './hooks/useSignalR';
import { useDataBridge } from './hooks/useDataBridge';
import { getAgents, getAllStatuses } from './services/api';
import { Sidebar } from './components/Sidebar/Sidebar';
import { EventLog } from './components/EventLog/EventLog';
import { DetailPanel } from './components/DetailPanel/DetailPanel';
import { ChatPanel } from './components/Panels/ChatPanel';
import { CostDashboard } from './components/Panels/CostDashboard';
import { HRDashboard } from './components/Panels/HRDashboard';
import { MiniMap } from './components/Scene3D/UI/MiniMap';
import { CoworkScene } from './components/Scene3D/CoworkScene';
import { CeoPlatform } from './components/Scene3D/Buildings/CeoPlatform';
import { CargoHub } from './components/Scene3D/Buildings/CargoHub';
import { DepartmentBuilding } from './components/Scene3D/Buildings/DepartmentBuilding';
import { AgentCharacter } from './components/Scene3D/Characters/AgentCharacter';
import { LoadingSpinner } from './components/common/LoadingSpinner';
import { ConnectionBanner } from './components/common/ConnectionBanner';
import { ToastContainer } from './components/common/ToastContainer';
import { BUILDINGS } from './constants/colors';
import LiveTicker from './components/StatusBar/LiveTicker';

export default function App() {
  const store = useCoworkStore();
  const panels = store.panels;

  // ── Bootstrap: load agents + statuses ───────────────────────────────────────
  useEffect(() => {
    Promise.all([getAgents(), getAllStatuses()])
      .then(([agents, statusMap]) => {
        store.setAgents(agents);
        const stMap: Record<string, string> = {};
        Object.entries(statusMap).forEach(([id, st]) => {
          stMap[id] = (st as { status: string }).status;
        });
        store.setStatuses(stMap);
      })
      .catch((err: Error) => store.addToast('error', `Failed to load: ${err.message}`));
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // ── Esc key closes the topmost open panel ───────────────────────────────────
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key !== 'Escape') return;
      // Priority order: chat > detail > cost > hr > eventLog > sidebar
      const order: Array<'chat' | 'detail' | 'cost' | 'hr' | 'eventLog' | 'sidebar'> = [
        'chat', 'detail', 'cost', 'hr', 'eventLog', 'sidebar',
      ];
      for (const name of order) {
        if (panels[name].open) {
          store.closePanel(name);
          break;
        }
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [panels, store]);

  // ── SignalR real-time events ─────────────────────────────────────────────────
  useSignalR({
    onStatusChange: (ev) => store.updateStatus(ev.agentId, ev.status),
    onAgentEvent: (ev) => store.addLog('📡', `${ev.agentId}: ${ev.message}`),
    onCommand: (ev) => store.addLog('👑', `${ev.fromId}→${ev.toId}: ${ev.message}`),
    onConversation: (ev) => store.addLog('🗣️', `${ev.fromIcon} ${ev.message}`),
    onAgentMessage: (ev) => {
      store.addMessage(ev);
      store.addLog('💬', `${ev.fromId}→${ev.toId}: ${ev.content.slice(0, 40)}`);
    },
    onAgentSpawned: (ev) => {
      store.addAgent({
        id: ev.agentId,
        name: ev.name,
        icon: ev.icon,
        tier: 'WRK',
        color: '#6b7280',
        department: ev.department,
        description: ev.name,
        skills: '[]',
        isBase: false,
        isActive: true,
        isImmortal: false,
        tools: '[]',
        createdBy: ev.spawnedBy,
        createdAt: new Date().toISOString(),
      });
      store.addLog('\u{1F9D1}\u200D\u{1F4BC}', `New agent spawned: ${ev.name}`);
    },
    onAgentRetired: (ev) => {
      store.removeAgent(ev.agentId);
      store.addLog('🔴', `Agent retired: ${ev.agentId} — ${ev.reason}`);
    },
    onAgentMovement: (ev) => {
      store.addMovement(ev);
      store.addLog('🚶', `${ev.agentId} → ${ev.targetAgentId}`);
    },
    onDocumentTransfer: (ev) => {
      store.addEffect({
        id: `doc-${Date.now()}`,
        type: 'document',
        data: ev,
        createdAt: Date.now(),
      });
      store.addLog('📄', `${ev.fromId} → ${ev.toId} [${ev.docType}]`);
    },
    onTaskEffect: (ev) => {
      store.addEffect({
        id: `task-${Date.now()}`,
        type: 'taskComplete',
        data: ev,
        createdAt: Date.now(),
      });
      store.addLog(ev.effect === 'complete' ? '✅' : '❌', `${ev.agentId} task ${ev.effect}`);
    },
    onClaudeAction: (ev) => {
      store.addClaudeEvent(ev);
      store.addLog('🤖', `Claude [${ev.tool}]: ${ev.summary.slice(0, 50)}`);
    },
    onClaudeTaskStart: (ev) => {
      store.startClaudeTask(ev);
      store.addLog('🚀', `Claude task started: ${ev.title}`);
    },
    onClaudeTaskComplete: (ev) => {
      store.completeClaudeTask(ev.taskId, ev.status);
      store.addLog('🏁', `Claude task ${ev.status}: ${ev.taskId}`);
    },
  }, store.setConnectionState);

  // ── Data Bridge polling ────────────────────────────────────────────────────
  useDataBridge();

  // ── Derived state ────────────────────────────────────────────────────────────
  const selectedAgentId = panels.detail.agentId ?? null;
  const selectedAgent = store.agents.find((a) => a.id === selectedAgentId) ?? null;

  const chatAgentId = panels.chat.agentId ?? null;
  const chatAgent = store.agents.find((a) => a.id === chatAgentId) ?? null;

  return (
    <div
      className="flex h-screen overflow-hidden text-gray-50"
      style={{ background: 'var(--bg-primary, #060710)' }}
    >
      <ConnectionBanner />
      <ToastContainer />
      {/* ── Sidebar (left) ────────────────────────────────────────────────── */}
      <div
        className={`${panels.sidebar.open ? 'flex' : 'hidden'} md:flex shrink-0`}
      >
        <Sidebar
          agents={store.agents}
          statuses={store.statuses}
          selectedId={selectedAgentId}
          onSelect={(id) => store.openPanel('detail', id)}
          activeCount={store.activeCount()}
        />
      </div>

      {/* ── Main viewport ─────────────────────────────────────────────────── */}
      <div className="flex-1 relative overflow-hidden flex flex-col">
        {/* ── Live data ticker ──────────────────────────────────────────── */}
        <LiveTicker />

        {/* ── Scene + overlays container ───────────────────────────────── */}
        <div className="flex-1 relative overflow-hidden">
        {/* Mobile hamburger — visible only below md */}
        <button
          className="md:hidden absolute top-3 left-3 z-30 flex items-center justify-center w-8 h-8 rounded-md bg-[#0a0b14cc] border border-gray-800 text-gray-400 hover:text-white"
          aria-label="Toggle sidebar"
          onClick={() => store.togglePanel('sidebar')}
        >
          ☰
        </button>

        {/* 3D Scene wrapped in Suspense */}
        <Suspense
          fallback={
            <div className="absolute inset-0 flex items-center justify-center">
              <LoadingSpinner />
            </div>
          }
        >
          <CoworkScene>
            <CeoPlatform />
            <CargoHub />
            {/* Department buildings from BUILDINGS constant */}
            {BUILDINGS.map((b) => (
              <DepartmentBuilding
                key={b.id}
                position={[b.x, 0, b.z]}
                size={[b.width, b.depth, b.height]}
                color={b.color}
              />
            ))}
            {store.agents.map((agent) => (
              <AgentCharacter
                key={agent.id}
                agentId={agent.id}
                icon={agent.icon}
                color={agent.color}
                status={store.statuses[agent.id] || 'idle'}
                isSelected={selectedAgentId === agent.id}
                onClick={() => store.openPanel('detail', agent.id)}
              />
            ))}
          </CoworkScene>
        </Suspense>

        {/* ── Detail panel ────────────────────────────────────────────────── */}
        {selectedAgent && panels.detail.open && (
          <DetailPanel
            agent={selectedAgent}
            status={store.statuses[selectedAgent.id] || 'idle'}
            onClose={() => store.closePanel('detail')}
            onChat={() => store.openPanel('chat', selectedAgent.id)}
          />
        )}

        {/* ── Chat panel ──────────────────────────────────────────────────── */}
        {chatAgent && panels.chat.open && (
          <ChatPanel
            agent={chatAgent}
            onClose={() => store.closePanel('chat')}
          />
        )}

        {/* ── Cost dashboard ──────────────────────────────────────────────── */}
        {panels.cost.open && (
          <CostDashboard onClose={() => store.closePanel('cost')} />
        )}

        {/* ── HR dashboard ────────────────────────────────────────────────── */}
        {panels.hr.open && (
          <HRDashboard onClose={() => store.closePanel('hr')} />
        )}

        {/* ── MiniMap (always visible) ────────────────────────────────────── */}
        <MiniMap />

        {/* ── Bottom toolbar ──────────────────────────────────────────────── */}
        <div className="absolute bottom-3 left-3 z-10 flex items-center gap-1.5">
          {/* Cost toggle */}
          <button
            onClick={() => store.togglePanel('cost')}
            className={`font-mono text-[9px] rounded px-2 py-1 border transition-colors
              ${panels.cost.open
                ? 'bg-amber-500/20 border-amber-500/50 text-amber-300'
                : 'bg-[#0a0b14cc] border-gray-800 text-gray-400 hover:text-white'
              }`}
          >
            💰 Cost
          </button>

          {/* HR toggle */}
          <button
            onClick={() => store.togglePanel('hr')}
            className={`font-mono text-[9px] rounded px-2 py-1 border transition-colors
              ${panels.hr.open
                ? 'bg-emerald-500/20 border-emerald-500/50 text-emerald-300'
                : 'bg-[#0a0b14cc] border-gray-800 text-gray-400 hover:text-white'
              }`}
          >
            🧑‍💼 HR
          </button>

          {/* EventLog toggle (visible only below xl) */}
          <button
            onClick={() => store.togglePanel('eventLog')}
            className={`xl:hidden font-mono text-[9px] rounded px-2 py-1 border transition-colors
              ${panels.eventLog.open
                ? 'bg-blue-500/20 border-blue-500/50 text-blue-300'
                : 'bg-[#0a0b14cc] border-gray-800 text-gray-400 hover:text-white'
              }`}
          >
            📋 Log
          </button>
        </div>
        </div>{/* end scene + overlays container */}
      </div>

      {/* ── Event log (right) ─────────────────────────────────────────────── */}
      <div
        className={`${panels.eventLog.open ? 'flex' : 'hidden'} xl:flex shrink-0`}
      >
        <EventLog entries={store.eventLog} />
      </div>
    </div>
  );
}
