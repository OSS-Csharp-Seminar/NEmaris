import { useEffect, useState } from "react";
import tableService from "../services/tableService";
import type { RestaurantTable } from "../types/floor";

interface WalkInModalProps {
  table: RestaurantTable;
  onClose: () => void;
  onSeated: (next: RestaurantTable) => void;
}

function extractErrorMessage(e: unknown): string | undefined {
  if (typeof e !== "object" || !e) return undefined;
  const response = (e as { response?: { data?: { message?: string } } }).response;
  return response?.data?.message;
}

export default function WalkInModal({ table, onClose, onSeated }: WalkInModalProps) {
  const [guestCount, setGuestCount] = useState(2);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const handler = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    window.addEventListener("keydown", handler);
    return () => window.removeEventListener("keydown", handler);
  }, [onClose]);

  const handleSubmit = async () => {
    if (submitting) return;
    setSubmitting(true);
    setError(null);
    try {
      const next = await tableService.seatWalkIn(table.id, guestCount);
      onSeated(next);
    } catch (e: unknown) {
      setError(extractErrorMessage(e) ?? "Nije moguće zaprimiti walk-in goste.");
    } finally {
      setSubmitting(false);
    }
  };

  const canDecrease = guestCount > 1;
  const canIncrease = guestCount < table.capacity;

  return (
    <div
      className="fixed inset-0 z-40 flex items-center justify-center bg-black/40 p-4"
      onClick={onClose}
    >
      <div
        className="w-full max-w-sm rounded-lg border border-border bg-card p-5 shadow-lg"
        onClick={(e) => e.stopPropagation()}
      >
        <h2 className="text-lg font-semibold text-card-foreground">Walk-in</h2>
        <p className="mt-1 text-sm text-muted-foreground">
          Stol {table.name} · kapacitet {table.capacity}
        </p>

        <div className="mt-5 rounded-lg border border-border bg-secondary/40 p-3">
          <p className="text-sm font-medium text-card-foreground">Broj gostiju</p>
          <div className="mt-3 flex items-center justify-between gap-3">
            <button
              type="button"
              onClick={() => setGuestCount((g) => Math.max(1, g - 1))}
              disabled={!canDecrease || submitting}
              className="flex h-10 w-10 items-center justify-center rounded-lg border border-border bg-card text-xl font-semibold transition hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-40"
              aria-label="Oduzmi gosta"
            >
              -
            </button>
            <strong className="text-2xl text-card-foreground">{guestCount}</strong>
            <button
              type="button"
              onClick={() => setGuestCount((g) => Math.min(table.capacity, g + 1))}
              disabled={!canIncrease || submitting}
              className="flex h-10 w-10 items-center justify-center rounded-lg border border-border bg-card text-xl font-semibold transition hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-40"
              aria-label="Dodaj gosta"
            >
              +
            </button>
          </div>
        </div>

        {error && <p className="mt-3 text-sm text-destructive">{error}</p>}

        <div className="mt-5 flex justify-end gap-2">
          <button
            type="button"
            onClick={onClose}
            disabled={submitting}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm font-medium text-card-foreground hover:bg-secondary disabled:opacity-50"
          >
            Odustani
          </button>
          <button
            type="button"
            onClick={handleSubmit}
            disabled={submitting}
            className="rounded-lg bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
          >
            {submitting ? "Spremam..." : "Smjesti"}
          </button>
        </div>
      </div>
    </div>
  );
}
