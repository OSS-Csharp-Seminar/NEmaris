import { useEffect, useRef, useState } from "react";
import chatService, { type ChatMessage } from "../../services/chatService";

export default function ChatWidget() {
  const [open, setOpen] = useState(false);
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const scrollRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages, loading, open]);

  useEffect(() => {
    if (open) inputRef.current?.focus();
  }, [open]);

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
      const { data } = await chatService.send({ messages: nextMessages });
      setMessages([...nextMessages, { role: "assistant", content: data.reply }]);
    } catch (e: unknown) {
      const status =
        typeof e === "object" && e && "response" in e
          ? (e as { response?: { status?: number } }).response?.status
          : undefined;
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

  if (!open) {
    return (
      <button
        type="button"
        onClick={() => setOpen(true)}
        aria-label="Open reservations chat"
        className="fixed bottom-6 right-6 z-50 rounded-full bg-primary px-5 py-3 font-medium text-primary-foreground shadow-lg transition-opacity hover:opacity-90"
      >
        Chat
      </button>
    );
  }

  return (
    <div className="fixed bottom-6 right-6 z-50 flex h-[32rem] w-[22rem] max-w-[calc(100vw-3rem)] flex-col rounded-2xl border border-border bg-card text-card-foreground shadow-xl">
      <div className="flex items-center justify-between border-b border-border p-3">
        <span className="font-medium">Reservations</span>
        <button
          type="button"
          onClick={() => setOpen(false)}
          aria-label="Close chat"
          className="text-muted-foreground hover:text-foreground"
        >
          ✕
        </button>
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
