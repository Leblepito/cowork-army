import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import type { Agent, MovementEvent, ActiveEffect, TradeFeed, MedicalFeed, HotelFeed, ChatMessage } from '../types';
import { translate, type Language } from '../constants/i18n';

export const EVENT_LOG_MAX = 80;

// ─── Existing Types ────────────────────────────────────────────────────────────
interface LogEntry {
  icon: string;
  message: string;
  time: string;
  source?: string;
}

// ─── Panel Types ───────────────────────────────────────────────────────────────
export type PanelName = 'sidebar' | 'eventLog' | 'detail' | 'chat' | 'cost' | 'hr';

interface PanelState {
  open: boolean;
  /** Only used by the 'detail' panel — which agent to show */
  agentId?: string;
}

type PanelsMap = Record<PanelName, PanelState>;

const DEFAULT_PANELS: PanelsMap = {
  sidebar:  { open: true },
  eventLog: { open: true },
  detail:   { open: false },
  chat:     { open: false },
  cost:     { open: false },
  hr:       { open: false },
};

// ─── Store Interface ───────────────────────────────────────────────────────────
interface CoworkState {
  // ── Existing state ──────────────────────────────────────────────────────────
  agents: Agent[];
  statuses: Record<string, string>;
  eventLog: LogEntry[];
  selectedId: string | null;
  messages: { fromId: string; toId: string; content: string; timestamp: string }[];
  connectionState: 'connected' | 'reconnecting' | 'disconnected';
  setConnectionState: (state: 'connected' | 'reconnecting' | 'disconnected') => void;

  // Toast state
  toasts: { id: string; type: 'error' | 'success' | 'info'; message: string }[];
  addToast: (type: 'error' | 'success' | 'info', message: string) => void;
  removeToast: (id: string) => void;

  // ── Existing actions ────────────────────────────────────────────────────────
  setAgents: (agents: Agent[]) => void;
  updateStatus: (agentId: string, status: string) => void;
  setStatuses: (statuses: Record<string, string>) => void;
  addLog: (icon: string, message: string, source?: string) => void;
  setSelectedId: (id: string | null) => void;
  addMessage: (msg: { fromId: string; toId: string; content: string }) => void;
  activeCount: () => number;
  idleCount: () => number;
  addAgent: (agent: Agent) => void;
  removeAgent: (agentId: string) => void;

  // ── Animation state ───────────────────────────────────────────────────────
  agentPositions: Record<string, { x: number; z: number }>;
  activeEffects: ActiveEffect[];
  movements: MovementEvent[];
  setAgentPosition: (agentId: string, x: number, z: number) => void;
  addEffect: (effect: ActiveEffect) => void;
  removeEffect: (id: string) => void;
  addMovement: (event: MovementEvent) => void;
  removeMovement: (agentId: string) => void;

  // ── Panel state ─────────────────────────────────────────────────────────────
  panels: PanelsMap;

  /** Open a panel. On mobile (<1024 px) all other panels close first. */
  openPanel: (name: PanelName, agentId?: string) => void;
  closePanel: (name: PanelName) => void;
  togglePanel: (name: PanelName, agentId?: string) => void;

  // ── Data Bridge state ──────────────────────────────────────────────────────
  tradeFeed: TradeFeed | null;
  medicalFeed: MedicalFeed | null;
  hotelFeed: HotelFeed | null;
  setTradeFeed: (feed: TradeFeed) => void;
  setMedicalFeed: (feed: MedicalFeed) => void;
  setHotelFeed: (feed: HotelFeed) => void;

  // ── Chat state ────────────────────────────────────────────────────────────
  chatMessages: Record<string, ChatMessage[]>;
  chatTyping: Record<string, boolean>;
  chatConversationId: string | null;
  chatAgentId: string | null;
  addChatMessage: (agentId: string, message: ChatMessage) => void;
  setChatTyping: (agentId: string, typing: boolean) => void;
  setChatConversationId: (convId: string | null) => void;
  setChatAgentId: (agentId: string | null) => void;
  openChat: (agentId: string) => void;

  // ── i18n state ──────────────────────────────────────────────────────────────
  language: Language;
  setLanguage: (lang: Language) => void;
}

// ─── Helpers ──────────────────────────────────────────────────────────────────
function isMobile(): boolean {
  return typeof window !== 'undefined' && window.innerWidth < 1024;
}

// ─── Store ─────────────────────────────────────────────────────────────────────
export const useCoworkStore = create<CoworkState>()(
  persist(
    (set, get) => ({
      // ── Existing state ────────────────────────────────────────────────────────
      agents: [],
      statuses: {},
      eventLog: [],
      selectedId: null,
      messages: [],
      connectionState: 'disconnected' as const,
      setConnectionState: (connectionState) => set({ connectionState }),

      toasts: [],
      addToast: (type, message) => {
        const id = Date.now().toString(36);
        set((state) => ({
          toasts: [...state.toasts, { id, type, message }].slice(-5),
        }));
        setTimeout(() => {
          set((state) => ({
            toasts: state.toasts.filter((t) => t.id !== id),
          }));
        }, 5000);
      },
      removeToast: (id) =>
        set((state) => ({ toasts: state.toasts.filter((t) => t.id !== id) })),

      // ── Existing actions ──────────────────────────────────────────────────────
      setAgents: (agents) => set({ agents }),

      updateStatus: (agentId, status) =>
        set((state) => ({ statuses: { ...state.statuses, [agentId]: status } })),

      setStatuses: (statuses) => set({ statuses }),

      addLog: (icon, message, source?) =>
        set((state) => ({
          eventLog: [
            { icon, message, time: new Date().toLocaleTimeString('tr-TR'), source },
            ...state.eventLog.slice(0, EVENT_LOG_MAX - 1),
          ],
        })),

      setSelectedId: (id) => set({ selectedId: id }),

      addMessage: (msg) =>
        set((state) => ({
          messages: [
            { ...msg, timestamp: new Date().toISOString() },
            ...state.messages.slice(0, 99),
          ],
        })),

      activeCount: () =>
        Object.values(get().statuses).filter((s) => s !== 'idle').length,

      idleCount: () => get().agents.length - get().activeCount(),

      addAgent: (agent) =>
        set((state) => ({ agents: [...state.agents, agent] })),

      removeAgent: (agentId) =>
        set((state) => ({
          agents: state.agents.filter((a) => a.id !== agentId),
        })),

      // ── Animation state ───────────────────────────────────────────────────
      agentPositions: {},
      activeEffects: [],
      movements: [],

      setAgentPosition: (agentId, x, z) =>
        set((state) => ({
          agentPositions: { ...state.agentPositions, [agentId]: { x, z } },
        })),

      addEffect: (effect) =>
        set((state) => ({
          activeEffects: [...state.activeEffects, effect].slice(-20),
        })),

      removeEffect: (id) =>
        set((state) => ({
          activeEffects: state.activeEffects.filter((e) => e.id !== id),
        })),

      addMovement: (event) =>
        set((state) => ({
          movements: [...state.movements.filter((m) => m.agentId !== event.agentId), event],
        })),

      removeMovement: (agentId) =>
        set((state) => ({
          movements: state.movements.filter((m) => m.agentId !== agentId),
        })),

      // ── Panel state ───────────────────────────────────────────────────────────
      panels: DEFAULT_PANELS,

      openPanel: (name, agentId) =>
        set((state) => {
          const mobile = isMobile();
          const updated: PanelsMap = mobile
            ? // On mobile: close all panels, then open the requested one
              (Object.fromEntries(
                Object.keys(state.panels).map((k) => [k, { open: false }]),
              ) as PanelsMap)
            : { ...state.panels };

          updated[name] = { open: true, ...(agentId !== undefined ? { agentId } : {}) };
          return { panels: updated };
        }),

      closePanel: (name) =>
        set((state) => ({
          panels: {
            ...state.panels,
            [name]: { ...state.panels[name], open: false },
          },
        })),

      togglePanel: (name, agentId) => {
        const { panels, openPanel, closePanel } = get();
        if (panels[name].open) {
          closePanel(name);
        } else {
          openPanel(name, agentId);
        }
      },

      // ── Data Bridge state ────────────────────────────────────────────────────
      tradeFeed: null,
      medicalFeed: null,
      hotelFeed: null,
      setTradeFeed: (feed) => set({ tradeFeed: feed }),
      setMedicalFeed: (feed) => set({ medicalFeed: feed }),
      setHotelFeed: (feed) => set({ hotelFeed: feed }),

      // ── Chat state ──────────────────────────────────────────────────────────
      chatMessages: {},
      chatTyping: {},
      chatConversationId: null,
      chatAgentId: null,

      addChatMessage: (agentId, message) =>
        set((state) => ({
          chatMessages: {
            ...state.chatMessages,
            [agentId]: [...(state.chatMessages[agentId] || []), message],
          },
        })),

      setChatTyping: (agentId, typing) =>
        set((state) => ({
          chatTyping: { ...state.chatTyping, [agentId]: typing },
        })),

      setChatConversationId: (convId) => set({ chatConversationId: convId }),

      setChatAgentId: (agentId) => set({ chatAgentId: agentId }),

      openChat: (agentId) =>
        set((state) => {
          const panels = { ...state.panels };
          panels.chat = { ...panels.chat, open: true };
          return { panels, chatAgentId: agentId };
        }),

      // ── i18n state ────────────────────────────────────────────────────────────
      language: 'en' as Language,

      setLanguage: (lang) => {
        set({ language: lang });
      },
    }),
    {
      name: 'cowork-store',
      partialize: (state) => ({
        panels: state.panels,
        language: state.language,
      }),
    },
  ),
);

// ─── useTranslation hook ───────────────────────────────────────────────────────
/**
 * Returns a `t(key)` helper bound to the current language from the store.
 *
 * @example
 *   const { t } = useTranslation();
 *   <button>{t('action.spawn')}</button>
 */
export function useTranslation() {
  const language = useCoworkStore((s) => s.language);
  const t = (key: string) => translate(key, language);
  return { t, language };
}
