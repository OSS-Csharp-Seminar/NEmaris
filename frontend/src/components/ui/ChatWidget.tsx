import { useEffect, useRef, useState } from "react";
import chatService, { type ChatMessage } from "../../services/chatService";

interface ChatWidgetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

const STORAGE_KEY = "nemaris.chat.messages";
const SESSION_KEY = "nemaris.chat.sessionId";
export const RESERVATIONS_CHANGED_EVENT = "nemaris:reservations-changed";

function loadMessages(): ChatMessage[] {
  if (typeof window === "undefined") return [];
  try {
    const raw = window.localStorage.getItem(STORAGE_KEY);
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? (parsed as ChatMessage[]) : [];
  } catch {
    return [];
  }
}

function loadSessionId(): string {
  if (typeof window === "undefined") return crypto.randomUUID();
  const existing = window.localStorage.getItem(SESSION_KEY);
  if (existing) return existing;
  const fresh = crypto.randomUUID();
  window.localStorage.setItem(SESSION_KEY, fresh);
  return fresh;
}

export default function ChatWidget({ open, onOpenChange }: ChatWidgetProps) {
  const [messages, setMessages] = useState<ChatMessage[]>(loadMessages);
  const [sessionId, setSessionId] = useState<string>(loadSessionId);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const scrollRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (typeof window === "undefined") return;
    if (messages.length === 0) {
      window.localStorage.removeItem(STORAGE_KEY);
    } else {
      window.localStorage.setItem(STORAGE_KEY, JSON.stringify(messages));
    }
  }, [messages]);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages, loading, open]);

  useEffect(() => {
    if (open) inputRef.current?.focus();
  }, [open]);

  useEffect(() => {
    if (!open) return;
    const onKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape") onOpenChange(false);
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [open, onOpenChange]);

  useEffect(() => {
    const onStorage = (e: StorageEvent) => {
      if (e.key === STORAGE_KEY) {
        setMessages(loadMessages());
      } else if (e.key === SESSION_KEY) {
        setSessionId(loadSessionId());
      }
    };
    window.addEventListener("storage", onStorage);
    return () => window.removeEventListener("storage", onStorage);
  }, []);

  const send = async () => {
    const trimmed = input.trim();
    if (!trimmed || loading) return;

    const nextMessages: ChatMessage[] = [
      ...messages,
      { role: "user", content: trimmed },
    ];
    setMessages(nextMessages);
    setInput("");
    setLoading(true);
    setError(null);

    try {
      const { data } = await chatService.send({ messages: nextMessages, sessionId });
      setMessages([...nextMessages, { role: "assistant", content: data.reply }]);
      if (data.reservationsChanged && typeof window !== "undefined") {
        window.dispatchEvent(new Event(RESERVATIONS_CHANGED_EVENT));
      }
    } catch (e: unknown) {
      const status =
        typeof e === "object" && e && "response" in e
          ? (e as { response?: { status?: number } }).response?.status
          : undefined;
      setMessages(messages);
      setInput(trimmed);
      if (status === 429) {
        setError("Too many requests. Try again in a minute.");
      } else {
        setError("Couldn't reach the assistant. Try again.");
      }
    } finally {
      setLoading(false);
    }
  };

  const onKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      send();
    }
  };

  const clearConversation = () => {
    setMessages([]);
    setError(null);
    const fresh = crypto.randomUUID();
    setSessionId(fresh);
    if (typeof window !== "undefined") {
      window.localStorage.setItem(SESSION_KEY, fresh);
    }
    inputRef.current?.focus();
  };

  if (!open) {
    return null;
  }

  return (
    <div className="fixed bottom-6 right-6 z-50 flex h-[32rem] w-[22rem] max-w-[calc(100vw-3rem)] flex-col rounded-2xl border border-border bg-card text-card-foreground shadow-xl">
      <div className="flex items-center justify-between border-b border-border p-3">
        <span className="font-medium">Reservations</span>
        <div className="flex items-center gap-3">
          <button
            type="button"
            onClick={clearConversation}
            disabled={loading || messages.length === 0}
            aria-label="Clear conversation"
            className="text-xs text-muted-foreground hover:text-foreground disabled:opacity-40 disabled:hover:text-muted-foreground"
          >
            Clear
          </button>
          <button
            type="button"
            onClick={() => onOpenChange(false)}
            aria-label="Close chat"
            className="text-muted-foreground hover:text-foreground"
          >
            ✕
          </button>
        </div>
      </div>

      <div ref={scrollRef} className="flex-1 space-y-2 overflow-y-auto p-3">
        {messages.length === 0 && (
          <p className="text-sm text-muted-foreground">
            Hi! Ask me to check availability, book, find, update, or cancel a
            reservation.
          </p>
        )}
        {messages.map((m, i) => (
          <div
            key={i}
            className={
              m.role === "user"
                ? "ml-auto max-w-[85%] rounded-2xl bg-primary px-3 py-2 text-sm text-primary-foreground"
                : "mr-auto max-w-[85%] whitespace-pre-line rounded-2xl bg-secondary px-3 py-2 text-sm text-secondary-foreground"
            }
          >
            {m.content}
          </div>
        ))}
        {loading && (
          <div className="mr-auto rounded-2xl bg-secondary px-3 py-2 text-sm text-muted-foreground">
            …
          </div>
        )}
        {error && (
          <div className="rounded-2xl bg-destructive/20 px-3 py-2 text-sm text-destructive">
            {error}
          </div>
        )}
      </div>

      <div className="flex gap-2 border-t border-border p-3">
        <input
          ref={inputRef}
          type="text"
          value={input}
          onChange={(e) => setInput(e.target.value)}
          onKeyDown={onKeyDown}
          disabled={loading}
          placeholder="Ask about a reservation…"
          className="flex-1 rounded-lg border border-input bg-background px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
        />
        <button
          type="button"
          onClick={send}
          disabled={loading || !input.trim()}
          className="rounded-lg bg-primary px-3 py-2 text-sm text-primary-foreground transition-opacity disabled:opacity-50"
        >
          Send
        </button>
      </div>
    </div>
  );
}
