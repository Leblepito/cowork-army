import type { Agent, AgentStatus, AgentTask, AgentEvent, TradeFeed, MedicalFeed, HotelFeed, ChatMessage, ChatConversation } from '../types';

const API = import.meta.env.VITE_API_URL || '';

let authToken: string | null = localStorage.getItem('cowork_token');

export function setAuthToken(token: string | null) {
  authToken = token;
  if (token) localStorage.setItem('cowork_token', token);
  else localStorage.removeItem('cowork_token');
}

export function getAuthToken(): string | null {
  return authToken;
}

function headers(extra?: Record<string, string>): Record<string, string> {
  const h: Record<string, string> = { 'Content-Type': 'application/json', ...extra };
  if (authToken) h['Authorization'] = `Bearer ${authToken}`;
  return h;
}

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${API}${path}`, { headers: headers() });
  if (!res.ok) throw new Error(`API ${res.status}: ${path}`);
  return res.json();
}

async function post<T>(path: string, body?: unknown): Promise<T> {
  const res = await fetch(`${API}${path}`, {
    method: 'POST',
    headers: headers(),
    body: body ? JSON.stringify(body) : undefined,
  });
  if (!res.ok) throw new Error(`API ${res.status}: ${path}`);
  return res.json();
}

async function del<T>(path: string): Promise<T> {
  const res = await fetch(`${API}${path}`, { method: 'DELETE', headers: headers() });
  if (!res.ok) throw new Error(`API ${res.status}: ${path}`);
  return res.json();
}

// ═══ Agents ═══
export const getAgents = () => get<Agent[]>('/api/agents');
export const getAgent = (id: string) => get<Agent>(`/api/agents/${id}`);
export const createAgent = (data: Partial<Agent>) => post<Agent>('/api/agents', data);
export const deleteAgent = (id: string) => del<{ deleted: boolean }>(`/api/agents/${id}`);
export const getAgentStatus = (id: string) => get<AgentStatus>(`/api/agents/${id}/status`);
export const getAgentOutput = (id: string) => get<{ lines: string[] }>(`/api/agents/${id}/output`);
export const spawnAgent = (id: string, task?: string) =>
  post<AgentStatus>(`/api/agents/${id}/spawn${task ? `?task=${encodeURIComponent(task)}` : ''}`);
export const killAgent = (id: string) => post<{ status: string }>(`/api/agents/${id}/kill`);

// ═══ Statuses ═══
export const getAllStatuses = () => get<Record<string, AgentStatus>>('/api/statuses');

// ═══ Tasks ═══
export const getTasks = () => get<AgentTask[]>('/api/tasks');
export const createTask = (data: { title: string; description?: string; assignedTo?: string; priority?: string }) =>
  post<AgentTask>('/api/tasks', data);
export const delegateTask = (data: { title: string; description?: string }) =>
  post<{ routed_to: string; title: string }>('/api/commander/delegate', data);

// ═══ Events ═══
export const getEvents = (limit = 50) => get<AgentEvent[]>(`/api/events?limit=${limit}`);

// ═══ Autonomous ═══
export const getAutonomousStatus = () =>
  get<{ running: boolean; tick_count: number; agents_tracked: number }>('/api/autonomous/status');
export const startAutonomous = () => post<{ status: string }>('/api/autonomous/start');
export const stopAutonomous = () => post<{ status: string }>('/api/autonomous/stop');

// ═══ Health ═══
export const getHealth = () => get<{ status: string; version: string }>('/health');
export const getInfo = () => get<{ name: string; version: string; agents: number }>('/api/info');

// ═══ Chat ═══
export const sendChat = (agentId: string, message: string) =>
  post<{ response: string; tokens: number; cost: number }>(`/api/chat/${agentId}`, { message });

// ═══ Budget ═══
export const getBudgetStatus = () => get<import('../types').BudgetStatus>('/api/budget/status');

// ═══ Tools ═══
export const getTools = () => get<import('../types').ToolInfo[]>('/api/tools');

// ═══ Agent Messages ═══
export const getAgentMessages = (id: string, limit = 50) =>
  get<import('../types').AgentMessageDto[]>(`/api/agents/${id}/messages?limit=${limit}`);

// ═══ Orchestrate ═══
export const orchestrateAgent = (agentId: string, message: string) =>
  post<{ response: string }>(`/api/orchestrate/${agentId}`, { message });

export const startCommandChain = (message: string) =>
  post<{ chainId: string; status: string }>('/api/commander/chain', { message });

// ═══ HR ═══
export const getHRPerformance = () => get<import('../types').AgentPerformanceDto[]>('/api/hr/performance');
export const hrSpawnAgent = (reason: string, department: string) =>
  post<import('../types').SpawnResult>('/api/hr/spawn', { reason, department });
export const hrRetireAgent = (agentId: string, reason: string) =>
  post<{ retired: boolean }>(`/api/hr/retire/${agentId}`, { reason });
export const hrWarnAgent = (agentId: string, reason: string) =>
  post<{ warnings: number }>(`/api/hr/warn/${agentId}`, { reason });
export const getHRProposals = () => get<import('../types').HRProposal[]>('/api/hr/proposals');
export const approveProposal = (id: string) => post<{ executed: boolean }>(`/api/hr/proposals/${id}/approve`);

// ═══ Data Bridge ═══
export const getBridgeAll = () => get<{ trade: TradeFeed; medical: MedicalFeed; hotel: HotelFeed }>('/api/bridge/all');
export const getBridgeTrade = () => get<TradeFeed>('/api/bridge/trade');
export const getBridgeMedical = () => get<MedicalFeed>('/api/bridge/medical');
export const getBridgeHotel = () => get<HotelFeed>('/api/bridge/hotel');

// ═══ Agent Chat (with conversations) ═══
export const sendAgentChat = (agentId: string, message: string, conversationId?: string) =>
  post<{ conversationId: string; userMessage: ChatMessage; assistantMessage: ChatMessage; tokens: number; cost: number }>(
    `/api/agents/${agentId}/chat`, { message, conversationId });

export const getConversations = (agentId: string) =>
  get<ChatConversation[]>(`/api/agents/${agentId}/conversations`);

export const getConversation = (convId: string) =>
  get<ChatConversation>(`/api/conversations/${convId}`);
