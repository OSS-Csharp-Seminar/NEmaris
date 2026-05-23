import { useEffect, useState } from "react";
import orderService from "../../services/orderService";
import type { Bill } from "../../types/order";

interface Props {
  orderId: number;
  onClose: () => void;
}

const STATUS_LABELS: Record<string, string> = {
  open: "Otvorena",
  closed: "Zatvorena",
  cancelled: "Poništena",
  voided: "Stornirana",
};

export default function BillModal({ orderId, onClose }: Props) {
  const [bill, setBill] = useState<Bill | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    orderService.getBill(orderId).then((b) => {
      setBill(b);
      setLoading(false);
    });
  }, [orderId]);

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div className="flex max-h-[90vh] w-full max-w-md flex-col rounded-xl bg-background shadow-xl">
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <div>
            <p className="text-xs text-muted-foreground">
              {bill ? STATUS_LABELS[bill.status] ?? bill.status : ""}
            </p>
            <h2 className="text-lg font-semibold text-foreground">
              {bill ? `Stol ${bill.tableNumber} · ${bill.orderNumber}` : "Račun"}
            </h2>
          </div>
          <button
            onClick={onClose}
            className="rounded-md p-1.5 text-muted-foreground hover:bg-secondary"
          >
            ✕
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-5">
          {loading && <p className="text-sm text-muted-foreground">Učitavanje...</p>}

          {bill && (
            <div className="space-y-4">
              <div className="rounded-lg border border-border bg-card">
                {bill.items.length === 0 ? (
                  <p className="px-4 py-6 text-center text-sm text-muted-foreground">
                    Narudžba je prazna.
                  </p>
                ) : (
                  <ul className="divide-y divide-border">
                    {bill.items.map((item) => (
                      <li
                        key={item.id}
                        className="flex items-center justify-between gap-3 px-4 py-2.5"
                      >
                        <span className="flex-1 text-sm text-card-foreground">
                          {item.menuItemName}
                        </span>
                        <span className="text-xs text-muted-foreground">×{item.quantity}</span>
                        <span className="w-16 text-right text-sm font-medium text-card-foreground">
                          {item.lineTotal.toFixed(2)} €
                        </span>
                      </li>
                    ))}
                  </ul>
                )}

                <div className="space-y-1 border-t border-border px-4 py-3">
                  {bill.discountAmount > 0 && (
                    <div className="flex justify-between text-sm">
                      <span className="text-muted-foreground">Popust</span>
                      <span>-{bill.discountAmount.toFixed(2)} €</span>
                    </div>
                  )}
                  <div className="flex justify-between text-base font-semibold">
                    <span className="text-card-foreground">Ukupno</span>
                    <span className="text-primary">{bill.totalAmount.toFixed(2)} €</span>
                  </div>
                </div>
              </div>

              {bill.payments.length > 0 && (
                <div className="rounded-lg border border-border bg-card px-4 py-3 space-y-1">
                  <p className="text-xs font-medium uppercase tracking-wide text-muted-foreground">
                    Plaćanja
                  </p>
                  {bill.payments.map((p) => (
                    <div key={p.id} className="flex justify-between text-sm">
                      <span className="capitalize text-card-foreground">{p.paymentMethod}</span>
                      <span className="font-medium">{p.amount.toFixed(2)} €</span>
                    </div>
                  ))}
                </div>
              )}

              <div className="text-xs text-muted-foreground space-y-0.5">
                <p>Otvorio: {new Date(bill.openedAt).toLocaleString("hr-HR")}</p>
                {bill.closedAt && (
                  <p>Zatvorio: {new Date(bill.closedAt).toLocaleString("hr-HR")}</p>
                )}
                <p>Konobar: {bill.waiterName}</p>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
