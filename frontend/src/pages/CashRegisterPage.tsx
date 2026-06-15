import { useCallback, useEffect, useState } from "react";
import { Link } from "react-router-dom";
import orderService from "../services/orderService";
import type { DailyStats } from "../types/order";

const METHOD_LABELS: Record<string, string> = {
  cash: "Gotovina",
  card: "Kartica",
  voucher: "Bon",
};

export default function CashRegisterPage() {
  const [stats, setStats] = useState<DailyStats | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await orderService.getTodayStats();
      setStats(data);
    } catch {
      setError("Nije moguće učitati statistiku.");
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const today = new Date().toLocaleDateString("hr-HR", {
    weekday: "long",
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });

  return (
    <div className="flex h-full flex-col gap-4 overflow-y-auto">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-foreground">Kasa</h1>
          <p className="text-sm text-muted-foreground capitalize">{today}</p>
        </div>
        <div className="flex gap-2">
          <button
            onClick={load}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm font-medium text-card-foreground hover:bg-secondary"
          >
            Osvježi
          </button>
          <Link
            to="/orders"
            className="rounded-lg bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:bg-primary/90"
          >
            Današnji računi
          </Link>
        </div>
      </div>

      {loading ? (
        <p className="text-sm text-muted-foreground">Učitavanje...</p>
      ) : error ? (
        <p className="text-sm text-rose-700">{error}</p>
      ) : !stats ? null : (
        <>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-5">
            <StatCard label="Promet (s PDV-om)" value={`${stats.revenue.toFixed(2)} €`} accent />
            <StatCard label="Osnovica" value={`${stats.subtotal.toFixed(2)} €`} />
            <StatCard label="Naplaćeni PDV" value={`${stats.taxCollected.toFixed(2)} €`} />
            <StatCard label="Napojnice" value={`${stats.tips.toFixed(2)} €`} />
            <StatCard label="Broj računa" value={`${stats.billCount}`} />
          </div>

          {stats.byPaymentMethod.length > 0 && (
            <Section title="Po načinu plaćanja">
              <div className="grid gap-2 sm:grid-cols-2 lg:grid-cols-3">
                {stats.byPaymentMethod.map((m) => (
                  <div
                    key={m.paymentMethod}
                    className="flex items-center justify-between rounded-lg border border-border bg-card px-4 py-3"
                  >
                    <div>
                      <p className="text-sm font-medium text-card-foreground">
                        {METHOD_LABELS[m.paymentMethod] ?? m.paymentMethod}
                      </p>
                      <p className="text-xs text-muted-foreground">{m.count} uplata</p>
                    </div>
                    <p className="text-base font-semibold text-primary">
                      {m.amount.toFixed(2)} €
                    </p>
                  </div>
                ))}
              </div>
            </Section>
          )}

          <div className="grid gap-4 lg:grid-cols-2">
            <Section title="Najprodavanije stavke">
              {stats.topItems.length === 0 ? (
                <EmptyRow />
              ) : (
                <ul className="divide-y divide-border rounded-lg border border-border bg-card">
                  {stats.topItems.map((it, idx) => (
                    <li
                      key={it.menuItemId}
                      className="flex items-center gap-3 px-4 py-2.5"
                    >
                      <span className="w-6 text-sm font-semibold text-muted-foreground">
                        {idx + 1}.
                      </span>
                      <span className="flex-1 text-sm text-card-foreground">
                        {it.menuItemName}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        ×{it.quantity}
                      </span>
                      <span className="w-20 text-right text-sm font-medium text-card-foreground">
                        {it.revenue.toFixed(2)} €
                      </span>
                    </li>
                  ))}
                </ul>
              )}
            </Section>

            <Section title="Po konobaru">
              {stats.byWaiter.length === 0 ? (
                <EmptyRow />
              ) : (
                <ul className="divide-y divide-border rounded-lg border border-border bg-card">
                  {stats.byWaiter.map((w) => (
                    <li
                      key={w.waiterUserId}
                      className="flex items-center gap-3 px-4 py-2.5"
                    >
                      <span className="flex-1 text-sm text-card-foreground">
                        {w.waiterName || w.waiterUserId}
                      </span>
                      <span className="text-xs text-muted-foreground">
                        {w.billCount} računa
                      </span>
                      <span className="w-20 text-right text-sm font-medium text-card-foreground">
                        {w.revenue.toFixed(2)} €
                      </span>
                    </li>
                  ))}
                </ul>
              )}
            </Section>
          </div>
        </>
      )}
    </div>
  );
}

function StatCard({
  label,
  value,
  accent,
}: {
  label: string;
  value: string;
  accent?: boolean;
}) {
  return (
    <div className="rounded-lg border border-border bg-card p-4">
      <p className="text-xs uppercase tracking-wide text-muted-foreground">{label}</p>
      <p
        className={`mt-1 text-2xl font-bold ${
          accent ? "text-primary" : "text-card-foreground"
        }`}
      >
        {value}
      </p>
    </div>
  );
}

function Section({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div className="space-y-2">
      <h2 className="text-sm font-semibold uppercase tracking-wide text-muted-foreground">
        {title}
      </h2>
      {children}
    </div>
  );
}

function EmptyRow() {
  return (
    <p className="rounded-lg border border-border bg-card px-4 py-6 text-center text-sm text-muted-foreground">
      Nema podataka.
    </p>
  );
}
