import { useEffect, useRef, useState, useCallback } from 'react';
import { HubConnectionBuilder, HubConnection, HubConnectionState } from '@microsoft/signalr';
import type { StatusChangeEvent, CommandEvent, ConversationEvent } from '../types';
import { useCoworkStore } from '../stores/useCoworkStore';

const SIGNALR_URL = import.meta.env.VITE_SIGNALR_URL || '/hub';

interface SignalRCallbacks {
  onStatusChange?: (ev: StatusChangeEvent) => void;
  onAgentEvent?: (ev: { type: string; agentId: string; message: string }) => void;
  onCommand?: (ev: CommandEvent) => void;
  onConversation?: (ev: ConversationEvent) => void;
  onBudgetWarning?: (ev: { level: string; current: number; cap: number }) => void;
  onAgentMessage?: (ev: { fromId: string; toId: string; content: string }) => void;
  onAgentSpawned?: (ev: { agentId: string; name: string; icon: string; department: string; spawnedBy: string }) => void;
  onAgentRetired?: (ev: { agentId: string; reason: string }) => void;
  onAgentMovement?: (ev: import('../types').MovementEvent) => void;
  onDocumentTransfer?: (ev: import('../types').DocumentTransferEvent) => void;
  onTaskEffect?: (ev: import('../types').TaskEffectEvent) => void;
}

export function useSignalR(
  callbacks: SignalRCallbacks,
  setConnectionState?: (state: 'connected' | 'reconnecting' | 'disconnected') => void,
) {
  const connRef = useRef<HubConnection | null>(null);
  const [connected, setConnected] = useState(false);
  const cbRef = useRef(callbacks);
  cbRef.current = callbacks;

  useEffect(() => {
    const conn = new HubConnectionBuilder()
      .withUrl(SIGNALR_URL)
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    conn.on('StatusChange', (ev) => cbRef.current.onStatusChange?.(ev));
    conn.on('AgentEvent', (ev) => cbRef.current.onAgentEvent?.(ev));
    conn.on('Command', (ev) => cbRef.current.onCommand?.(ev));
    conn.on('Conversation', (ev) => cbRef.current.onConversation?.(ev));
    conn.on('BudgetWarning', (ev) => cbRef.current.onBudgetWarning?.(ev));
    conn.on('AgentMessage', (ev) => cbRef.current.onAgentMessage?.(ev));
    conn.on('AgentSpawned', (ev) => cbRef.current.onAgentSpawned?.(ev));
    conn.on('AgentRetired', (ev) => cbRef.current.onAgentRetired?.(ev));
    conn.on('AgentMovement', (ev) => cbRef.current.onAgentMovement?.(ev));
    conn.on('DocumentTransfer', (ev) => cbRef.current.onDocumentTransfer?.(ev));
    conn.on('TaskEffect', (ev) => cbRef.current.onTaskEffect?.(ev));

    // Data Bridge feeds
    conn.on('TradeFeed', (feed: any) => {
      useCoworkStore.getState().setTradeFeed(feed);
    });
    conn.on('MedicalFeed', (feed: any) => {
      useCoworkStore.getState().setMedicalFeed(feed);
    });
    conn.on('HotelFeed', (feed: any) => {
      useCoworkStore.getState().setHotelFeed(feed);
    });

    // Chat events
    conn.on('ChatMessage', (ev: any) => {
      if (ev.role === 'assistant') {
        useCoworkStore.getState().addChatMessage(ev.agentId, {
          id: `msg-rt-${Date.now()}`,
          role: ev.role,
          content: ev.content,
          timestamp: ev.timestamp || new Date().toISOString(),
        });
      }
    });
    conn.on('ChatTyping', (ev: any) => {
      useCoworkStore.getState().setChatTyping(ev.agentId, ev.isTyping);
    });

    conn.onreconnecting(() => {
      setConnected(false);
      setConnectionState?.('reconnecting');
    });
    conn.onreconnected(() => {
      setConnected(true);
      setConnectionState?.('connected');
    });
    conn.onclose(() => {
      setConnected(false);
      setConnectionState?.('disconnected');
    });

    conn.start()
      .then(() => {
        setConnected(true);
        setConnectionState?.('connected');
      })
      .catch((err) => {
        console.warn('SignalR connection failed:', err);
        setConnectionState?.('disconnected');
      });

    connRef.current = conn;

    return () => {
      conn.stop();
    };
  }, []);

  const send = useCallback(async (method: string, ...args: unknown[]) => {
    if (connRef.current?.state === HubConnectionState.Connected) {
      await connRef.current.invoke(method, ...args);
    }
  }, []);

  return { connected, send };
}
