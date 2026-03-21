import { useState, useCallback } from 'react';
import type { ChatMessage } from '../types';
import { sendChat } from '../services/api';

export function useChat(agentId: string) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [loading, setLoading] = useState(false);

  const send = useCallback(async (text: string) => {
    const userMsg: ChatMessage = { id: `msg-${Date.now()}`, role: 'user', content: text, timestamp: new Date().toISOString() };
    setMessages((prev) => [...prev, userMsg]);
    setLoading(true);
    try {
      const res = await sendChat(agentId, text);
      const assistantMsg: ChatMessage = {
        id: `msg-${Date.now()}-a`, role: 'assistant', content: res.response,
        timestamp: new Date().toISOString(), tokens: res.tokens, cost: res.cost,
      };
      setMessages((prev) => [...prev, assistantMsg]);
    } catch (err) {
      const errorMsg: ChatMessage = {
        id: `msg-err-${Date.now()}`, role: 'assistant',
        content: `Hata: ${err instanceof Error ? err.message : 'Bilinmeyen hata'}`,
        timestamp: new Date().toISOString(),
      };
      setMessages((prev) => [...prev, errorMsg]);
    } finally {
      setLoading(false);
    }
  }, [agentId]);

  const clear = useCallback(() => setMessages([]), []);
  return { messages, loading, send, clear };
}
