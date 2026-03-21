import { useState, useEffect, useRef } from 'react';
import { useCoworkStore } from '../../stores/useCoworkStore';
import { sendAgentChat, getConversations } from '../../services/api';
import type { ChatMessage } from '../../types';

export default function ChatPanel() {
  const { panels, chatAgentId, chatMessages, chatTyping, chatConversationId,
    agents, addChatMessage, setChatTyping, setChatConversationId, closePanel } = useCoworkStore();

  const [input, setInput] = useState('');
  const [sending, setSending] = useState(false);
  const scrollRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  const agent = agents.find(a => a.id === chatAgentId);
  const messages = chatAgentId ? (chatMessages[chatAgentId] || []) : [];
  const isTyping = chatAgentId ? (chatTyping[chatAgentId] || false) : false;

  // Auto-scroll on new messages
  useEffect(() => {
    scrollRef.current?.scrollTo({ top: scrollRef.current.scrollHeight, behavior: 'smooth' });
  }, [messages.length, isTyping]);

  // Focus input when panel opens
  useEffect(() => {
    if (panels.chat.open && chatAgentId) {
      inputRef.current?.focus();
      // Load conversation history
      loadHistory(chatAgentId);
    }
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [panels.chat.open, chatAgentId]);

  const loadHistory = async (agentId: string) => {
    try {
      const convs = await getConversations(agentId);
      if (convs.length > 0 && !chatMessages[agentId]?.length) {
        setChatConversationId(convs[0].id);
        for (const msg of convs[0].messages) {
          addChatMessage(agentId, msg);
        }
      }
    } catch {
      // Conversations endpoint may not be available yet
    }
  };

  const handleSend = async () => {
    if (!chatAgentId || !input.trim() || sending) return;
    const message = input.trim();
    setInput('');
    setSending(true);

    // Optimistic user message
    const userMsg: ChatMessage = {
      id: `msg-${Date.now()}`,
      role: 'user',
      content: message,
      timestamp: new Date().toISOString(),
    };
    addChatMessage(chatAgentId, userMsg);
    setChatTyping(chatAgentId, true);

    try {
      const result = await sendAgentChat(chatAgentId, message, chatConversationId ?? undefined);
      if (result.conversationId) setChatConversationId(result.conversationId);
      if (result.assistantMessage) addChatMessage(chatAgentId, result.assistantMessage);
    } catch {
      addChatMessage(chatAgentId, {
        id: `err-${Date.now()}`,
        role: 'assistant',
        content: 'Bağlantı hatası. Tekrar deneyin.',
        timestamp: new Date().toISOString(),
      });
    } finally {
      setChatTyping(chatAgentId, false);
      setSending(false);
    }
  };

  if (!panels.chat.open || !chatAgentId || !agent) return null;

  return (
    <div className="w-[340px] h-full bg-[#0d0e1a]/90 backdrop-blur-xl border-l border-white/10 flex flex-col shrink-0">
      {/* Header */}
      <div className="px-3 py-2 border-b border-white/10 flex items-center gap-2">
        <span className="text-lg">{agent.icon}</span>
        <div className="flex-1 min-w-0">
          <div className="text-xs font-medium text-white truncate">{agent.name}</div>
          <div className="text-[10px] text-gray-500">{agent.department}</div>
        </div>
        <button onClick={() => closePanel('chat')} className="text-gray-500 hover:text-white text-xs p-1">✕</button>
      </div>

      {/* Messages */}
      <div ref={scrollRef} className="flex-1 overflow-y-auto p-3 space-y-3">
        {messages.length === 0 && (
          <div className="text-center text-gray-600 text-xs mt-8">
            {agent.icon} ile sohbet başlat...
          </div>
        )}
        {messages.map((msg, i) => (
          <div key={msg.id || i} className={`flex ${msg.role === 'user' ? 'justify-end' : 'justify-start'}`}>
            <div className={`max-w-[85%] rounded-xl px-3 py-2 text-xs leading-relaxed ${
              msg.role === 'user'
                ? 'bg-blue-500/15 text-blue-100 border border-blue-500/20'
                : 'bg-white/5 text-gray-300 border border-white/5'
            }`}>
              {msg.content}
            </div>
          </div>
        ))}

        {/* Typing indicator */}
        {isTyping && (
          <div className="flex justify-start">
            <div className="bg-white/5 rounded-xl px-3 py-2 border border-white/5">
              <div className="flex gap-1">
                <span className="w-1.5 h-1.5 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '0ms' }} />
                <span className="w-1.5 h-1.5 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '150ms' }} />
                <span className="w-1.5 h-1.5 bg-gray-400 rounded-full animate-bounce" style={{ animationDelay: '300ms' }} />
              </div>
            </div>
          </div>
        )}
      </div>

      {/* Input */}
      <div className="p-2 border-t border-white/10">
        <div className="flex gap-2">
          <input
            ref={inputRef}
            value={input}
            onChange={(e) => setInput(e.target.value)}
            onKeyDown={(e) => e.key === 'Enter' && !e.shiftKey && handleSend()}
            placeholder="Mesaj yaz..."
            disabled={sending}
            className="flex-1 bg-white/5 text-white text-xs rounded-lg px-3 py-2 outline-none border border-white/10 focus:border-blue-500/50 placeholder-gray-600 disabled:opacity-50"
          />
          <button
            onClick={handleSend}
            disabled={sending || !input.trim()}
            className="px-3 py-2 bg-blue-500/20 text-blue-400 rounded-lg text-xs hover:bg-blue-500/30 disabled:opacity-30 transition-colors"
          >
            ▶
          </button>
        </div>
      </div>
    </div>
  );
}
