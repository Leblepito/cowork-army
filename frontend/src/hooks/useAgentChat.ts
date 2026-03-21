import { useCallback, useRef } from 'react';
import { useCoworkStore } from '../stores/useCoworkStore';
import { sendAgentChat, getConversations } from '../services/api';
import type { ChatMessage } from '../types';

export function useAgentChat() {
  const store = useCoworkStore();
  const sendingRef = useRef(false);

  const sendMessage = useCallback(async (agentId: string, message: string) => {
    if (sendingRef.current || !message.trim()) return;
    sendingRef.current = true;

    // Optimistic: add user message immediately
    const userMsg: ChatMessage = {
      id: `msg-${Date.now()}`,
      role: 'user',
      content: message,
      timestamp: new Date().toISOString(),
    };
    store.addChatMessage(agentId, userMsg);
    store.setChatTyping(agentId, true);

    try {
      const result = await sendAgentChat(agentId, message, store.chatConversationId ?? undefined);

      // Set conversation ID for follow-up messages
      if (result.conversationId) {
        store.setChatConversationId(result.conversationId);
      }

      // Add assistant message
      if (result.assistantMessage) {
        store.addChatMessage(agentId, result.assistantMessage);
      }
    } catch (e) {
      const errMsg: ChatMessage = {
        id: `msg-err-${Date.now()}`,
        role: 'assistant',
        content: 'Bağlantı hatası. Tekrar deneyin.',
        timestamp: new Date().toISOString(),
      };
      store.addChatMessage(agentId, errMsg);
    } finally {
      store.setChatTyping(agentId, false);
      sendingRef.current = false;
    }
  }, [store]);

  const loadHistory = useCallback(async (agentId: string) => {
    try {
      const convs = await getConversations(agentId);
      if (convs.length > 0) {
        const latest = convs[0];
        store.setChatConversationId(latest.id);
        // Load messages if not already loaded
        if (!store.chatMessages[agentId]?.length && latest.messages.length > 0) {
          for (const msg of latest.messages) {
            store.addChatMessage(agentId, msg);
          }
        }
      }
    } catch (e) {
      console.warn('[Chat] Failed to load history:', e);
    }
  }, [store]);

  return { sendMessage, loadHistory };
}
