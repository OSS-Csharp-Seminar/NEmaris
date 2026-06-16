import { useCallback, useEffect, useMemo, useState } from "react";
import { useOutletContext } from "react-router-dom";
import type { AppOutletContext } from "../App";
import publicMenuService from "../services/publicMenuService";
import type { PublicMenuItem } from "../types/publicMenu";

export default function LandingPage() {
  const { openChat } = useOutletContext<AppOutletContext>();
  const [items, setItems] = useState<PublicMenuItem[]>([]);
  const [loading, setLoading] = useState(false);
  const [loaded, setLoaded] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [menuOpen, setMenuOpen] = useState(false);
  const [chatInput, setChatInput] = useState("");
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(
    () => new Set(),
  );

  const submitChat = () => {
    const trimmed = chatInput.trim();
    if (!trimmed) return;
    openChat("large", trimmed);
    setChatInput("");
  };

  const toggleCategory = (category: string) => {
    setExpandedCategories((prev) => {
      const next = new Set(prev);
      if (next.has(category)) next.delete(category);
      else next.add(category);
      return next;
    });
  };

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await publicMenuService.getMenu();
      setItems(data);
      setLoaded(true);
    } catch {
      setError("We can't fetch the menu right now.");
    } finally {
      setLoading(false);
    }
  }, []);

  const toggleMenu = () => {
    const next = !menuOpen;
    setMenuOpen(next);
    if (next && !loaded && !loading) load();
  };

  const groupedByCategory = useMemo(() => {
    const groups = new Map<string, PublicMenuItem[]>();
    for (const it of items) {
      const list = groups.get(it.category) ?? [];
      list.push(it);
      groups.set(it.category, list);
    }
    return Array.from(groups.entries());
  }, [items]);

  useEffect(() => {
    if (!menuOpen) return;
    const handler = (e: KeyboardEvent) => {
      if (e.key === "Escape") setMenuOpen(false);
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, [menuOpen]);

  return (
    <div className="h-full overflow-y-auto rounded-3xl bg-card text-foreground">
      <section className="flex flex-col items-center gap-6 px-6 py-16 text-center sm:py-20">
        <p className="text-xs font-semibold uppercase tracking-[0.3em] text-muted-foreground">
          NEmaris Restaurant
        </p>
        <h1 className="max-w-2xl text-4xl font-semibold leading-tight sm:text-5xl">
          Welcome. Book a table by chatting with us.
        </h1>
        <p className="max-w-xl text-base text-muted-foreground">
          Our assistant helps you find an open slot in just a few messages.
          Ask about availability for tonight, this weekend, or anything else
          you'd like to know.
        </p>
        <form
          onSubmit={(e) => {
            e.preventDefault();
            submitChat();
          }}
          className="flex w-full max-w-xl items-center gap-2 rounded-full border border-border bg-background p-1.5 shadow-sm focus-within:border-primary/60 focus-within:ring-2 focus-within:ring-primary/20"
        >
          <input
            type="text"
            value={chatInput}
            onChange={(e) => setChatInput(e.target.value)}
            placeholder="Book a table, ask about tonight, anything..."
            className="flex-1 bg-transparent px-4 py-2 text-base text-foreground placeholder:text-muted-foreground focus:outline-none"
          />
          <button
            type="submit"
            disabled={!chatInput.trim()}
            className="rounded-full bg-primary px-5 py-2 text-sm font-semibold text-primary-foreground transition hover:bg-primary/90 disabled:opacity-50"
          >
            Send
          </button>
        </form>

        <button
          type="button"
          onClick={toggleMenu}
          className="rounded-full border border-border bg-card px-8 py-3 text-base font-semibold text-card-foreground transition hover:bg-secondary"
        >
          {menuOpen ? "Hide menu" : "View menu"}
        </button>
      </section>

      {menuOpen && (
        <section className="border-t border-border px-6 py-12 sm:px-10">
          <div className="mx-auto max-w-4xl">
            <div className="flex items-end justify-between gap-3">
              <div>
                <h2 className="text-2xl font-semibold">Menu</h2>
                <p className="mt-1 text-sm text-muted-foreground">
                  Currently available dishes and drinks.
                </p>
              </div>
            </div>

            <div className="mt-8 space-y-10">
              {loading && (
                <p className="text-sm text-muted-foreground">Loading...</p>
              )}

              {error && !loading && (
                <p className="text-sm text-rose-700">{error}</p>
              )}

              {!loading && !error && loaded && items.length === 0 && (
                <p className="text-sm text-muted-foreground">
                  No dishes available right now.
                </p>
              )}

              {!loading &&
                !error &&
                groupedByCategory.map(([category, list]) => {
                  const isExpanded = expandedCategories.has(category);
                  return (
                    <div key={category}>
                      <button
                        type="button"
                        onClick={() => toggleCategory(category)}
                        aria-expanded={isExpanded}
                        className={`group flex w-full items-center justify-between gap-3 rounded-xl border px-5 py-3 text-left shadow-sm transition ${
                          isExpanded
                            ? "border-primary/30 bg-primary/5 hover:bg-primary/10"
                            : "border-border bg-card hover:bg-secondary"
                        }`}
                      >
                        <span className="flex items-baseline gap-2">
                          <h3 className="text-base font-semibold tracking-tight text-card-foreground">
                            {category}
                          </h3>
                          <span className="rounded-full bg-secondary px-2 py-0.5 text-xs font-medium text-muted-foreground">
                            {list.length}
                          </span>
                        </span>
                        <span
                          aria-hidden
                          className={`flex h-7 w-7 items-center justify-center rounded-full bg-secondary text-sm text-muted-foreground transition-transform group-hover:text-foreground ${
                            isExpanded ? "rotate-180" : ""
                          }`}
                        >
                          ▾
                        </span>
                      </button>
                      {isExpanded && (
                        <ul className="mt-3 divide-y divide-border rounded-lg border border-border">
                          {list.map((it) => (
                            <li
                              key={`${category}-${it.name}`}
                              className="flex items-start justify-between gap-4 px-4 py-3"
                            >
                              <div className="min-w-0">
                                <p className="font-medium text-card-foreground">
                                  {it.name}
                                </p>
                                {it.description && (
                                  <p className="mt-1 text-sm text-muted-foreground">
                                    {it.description}
                                  </p>
                                )}
                              </div>
                              <p className="shrink-0 text-base font-semibold text-primary">
                                {it.price.toFixed(2)} €
                              </p>
                            </li>
                          ))}
                        </ul>
                      )}
                    </div>
                  );
                })}
            </div>
          </div>
        </section>
      )}
    </div>
  );
}
