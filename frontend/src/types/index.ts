export interface Agent {
  id: string;
  name: string;
  icon: string;
  tier: 'CEO' | 'DIR' | 'WRK' | 'SUP';
  color: string;
  department: string;
  description: string;
  skills: string;
  isBase: boolean;
  isActive: boolean;
  isImmortal: boolean;
  tools: string;
  createdBy: string;
  createdAt: string;
  retiredAt?: string;
  modelOverride?: string;
}

export interface AgentStatus {
  agentId: string;
  status: string;
  alive: boolean;
  lines: string[];
  startedAt?: string;
}

export interface AgentTask {
  id: string;
  title: string;
  description: string;
  assignedTo: string;
  createdBy: string;
  priority: string;
  status: string;
  createdAt: string;
  completedAt?: string;
}

export interface AgentEvent {
  id: number;
  type: string;
  agentId: string;
  message: string;
  timestamp: string;
}

// SignalR event payloads
export interface StatusChangeEvent {
  agentId: string;
  status: string;
  timestamp: string;
}

export interface CommandEvent {
  phase: string;
  fromId: string;
  toId: string;
  message: string;
  timestamp: string;
}

export interface ConversationEvent {
  fromId: string;
  fromIcon: string;
  toId: string;
  message: string;
  timestamp: string;
}

// Chat
export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  timestamp: string;
  tokens?: number;
  cost?: number;
}

export interface ChatResponse {
  response: string;
  tokens: number;
  cost: number;
}

// Budget
export interface BudgetStatus {
  globalTodayUsd: number;
  globalCapUsd: number;
  deptTodayUsd: Record<string, number>;
  deptCapUsd: number;
  agentHourUsd: Record<string, number>;
  agentCapUsd: number;
}

// Tools
export interface ToolInfo {
  name: string;
  description: string;
  permission: string;
  requiredParams: string[];
}

// Agent Messages
export interface AgentMessageDto {
  id: number;
  fromId: string;
  toId: string;
  type: string;
  content: string;
  priority: string;
  timestamp: string;
}

// Cost
export interface CostSummary {
  globalTodayUsd: number;
  globalCapUsd: number;
  agentHourUsd: Record<string, number>;
  agentCapUsd: number;
}

// HR
export interface AgentPerformanceDto {
  agentId: string; tasksCompleted: number; tasksFailed: number;
  avgResponseMs: number; totalTokens: number; estimatedCost: number;
  warnings: number; grade: string; lastActiveAt?: string;
}
export interface HRProposal {
  id: string; type: string; agentId?: string; reason: string;
  status: string; createdAt: string;
}
export interface SpawnResult {
  agentId: string; name: string; icon: string; department: string; designedBy: string;
}

// ═══ Animation ═══
export interface AnimationProps {
  armRotation: number;
  legRotation: number;
  headRotation: number;
  mouthOpen: boolean;
}

export interface MovementEvent {
  agentId: string;
  targetAgentId: string;
  duration: number;
}

export interface DocumentTransferEvent {
  fromId: string;
  toId: string;
  docType: string;
}

export interface TaskEffectEvent {
  agentId: string;
  effect: 'complete' | 'fail';
}

export interface ActiveEffect {
  id: string;
  type: 'document' | 'taskComplete';
  data: DocumentTransferEvent | TaskEffectEvent;
  createdAt: number;
}

// ═══ Data Bridge Feeds ═══
export interface TradeFeed {
  btcPrice: number; ethPrice: number;
  btcChange24h: number; ethChange24h: number;
  openPositions: number; totalPnl: number;
  activeSignals: number; fetchedAt: string;
}

export interface MedicalFeed {
  patientsToday: number; surgeryQueue: number;
  vipPipeline: number; monthlyRevenue: number;
  partnerHospitals: number; countriesServed: number;
  fetchedAt: string;
}

export interface HotelFeed {
  occupancyPercent: number; totalRooms: number;
  checkInsToday: number; checkOutsToday: number;
  newReservations: number; revPar: number;
  tours: number; transfers: number;
  spaBookings: number; restaurantReservations: number;
  fetchedAt: string;
}

// ═══ Chat Conversation ═══
export interface ChatConversation {
  id: string;
  agentId: string;
  title: string;
  messages: ChatMessage[];
  updatedAt: string;
}
