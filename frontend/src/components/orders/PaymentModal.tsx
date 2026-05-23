import { useState } from "react";

interface Props {
  total: number;
  onConfirm: (amount: number) => Promise<void>;
  onClose: () => void;
}

export default function PaymentModal({ total, onConfirm, onClose }: Props) {
  const [received, setReceived] = useState<string>(total.toFixed(2));
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const receivedNum = parseFloat(received) || 0;
  const change = receivedNum - total;

  const handleConfirm = async () => {
    if (receivedNum < total) {
      setError("Iznos je manji od ukupnog računa.");
      return;
    }
    setLoading(true);
    setError(null);
    try {
      await onConfirm(receivedNum);
    } catch (e: unknown) {
      const msg = e instanceof Error ? e.message : "Greška pri naplati.";
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div className="w-full max-w-sm rounded-xl bg-background p-6 shadow-xl">
        <h2 className="mb-4 text-lg font-semibold text-foreground">Naplata — gotovina</h2>

        <div className="mb-4 rounded-lg border border-border bg-secondary/40 px-4 py-3">
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Iznos za naplatu</span>
            <span className="font-semibold text-foreground">{total.toFixed(2)} €</span>
          </div>
        </div>

        <div className="mb-4">
          <label className="mb-1.5 block text-sm font-medium text-foreground">
            Primljeno (€)
          </label>
          <input
            type="number"
            min={total}
            step="0.01"
            value={received}
            onChange={(e) => setReceived(e.target.value)}
            className="w-full rounded-lg border border-border bg-background px-3 py-2 text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </div>

        {receivedNum >= total && (
          <div className="mb-4 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-2.5 text-sm">
            <span className="text-muted-foreground">Kusur: </span>
            <span className="font-semibold text-emerald-700">{change.toFixed(2)} €</span>
          </div>
        )}

        {error && (
          <p className="mb-3 text-sm text-destructive">{error}</p>
        )}

        <div className="flex gap-3">
          <button
            onClick={onClose}
            disabled={loading}
            className="flex-1 rounded-lg border border-border py-2.5 text-sm font-medium text-foreground hover:bg-secondary disabled:opacity-50"
          >
            Odustani
          </button>
          <button
            onClick={handleConfirm}
            disabled={loading || receivedNum < total}
            className="flex-1 rounded-lg bg-primary py-2.5 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
          >
            {loading ? "..." : "Potvrdi"}
          </button>
        </div>
      </div>
    </div>
  );
}
