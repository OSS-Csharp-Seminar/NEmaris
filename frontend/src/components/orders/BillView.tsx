import type { Bill } from "../../types/order";

interface Props {
  bill: Bill;
  onPay: () => void;
  paying?: boolean;
}

export default function BillView({ bill, onPay, paying }: Props) {
  const isPaid = bill.status === "closed" || bill.paymentStatus === "paid";

  return (
    <div className="flex flex-col gap-4">
      <div className="rounded-lg border border-border bg-card">
        <div className="border-b border-border px-4 py-3">
          <p className="text-xs text-muted-foreground">{bill.orderNumber}</p>
          <p className="text-sm font-medium text-card-foreground">
            Stol {bill.tableNumber} · {bill.waiterName}
          </p>
        </div>

        {bill.items.length === 0 ? (
          <p className="px-4 py-6 text-center text-sm text-muted-foreground">
            Narudžba je prazna.
          </p>
        ) : (
          <ul className="divide-y divide-border">
            {bill.items.map((item) => (
              <li key={item.id} className="flex items-center justify-between gap-3 px-4 py-2.5">
                <span className="flex-1 text-sm text-card-foreground">
                  {item.menuItemName}
                </span>
                <span className="text-xs text-muted-foreground">×{item.quantity}</span>
                <span className="text-sm font-medium text-card-foreground w-16 text-right">
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
              <span className="text-card-foreground">-{bill.discountAmount.toFixed(2)} €</span>
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
          <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide">
            Plaćanja
          </p>
          {bill.payments.map((p) => (
            <div key={p.id} className="flex justify-between text-sm">
              <span className="capitalize text-card-foreground">{p.paymentMethod}</span>
              <span className="font-medium text-card-foreground">{p.amount.toFixed(2)} €</span>
            </div>
          ))}
        </div>
      )}

      {!isPaid && (
        <button
          onClick={onPay}
          disabled={paying || bill.items.length === 0}
          className="w-full rounded-lg bg-primary py-3 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-50 transition"
        >
          {paying ? "Obrađujem..." : `Naplati ${bill.totalAmount.toFixed(2)} €`}
        </button>
      )}

      {isPaid && (
        <div className="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-center text-sm font-medium text-emerald-700">
          Narudžba je plaćena
        </div>
      )}
    </div>
  );
}
