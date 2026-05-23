import { useCallback, useEffect, useState } from "react";
import orderService from "../services/orderService";
import type { Order } from "../types/order";
import OrderPanel from "../components/orders/OrderPanel";
import BillModal from "../components/orders/BillModal";

const STATUS_LABELS: Record<string, string> = {
  open: "Otvorena",
  closed: "Zatvorena",
  cancelled: "Poništena",
  voided: "Stornirana",
};

const STATUS_COLORS: Record<string, string> = {
  open: "bg-emerald-100 text-emerald-700",
  closed: "bg-slate-100 text-slate-600",
  cancelled: "bg-rose-100 text-rose-700",
  voided: "bg-amber-100 text-amber-700",
};

export default function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([]);
  const [filterStatus, setFilterStatus] = useState<string>("open");
  const [loading, setLoading] = useState(true);
  const [selectedOrder, setSelectedOrder] = useState<Order | null>(null);

  const loadOrders = useCallback(async () => {
    setLoading(true);
    try {
      const data = await orderService.getOrders(filterStatus || undefined);
      setOrders(data);
    } finally {
      setLoading(false);
    }
  }, [filterStatus]);

  useEffect(() => {
    loadOrders();
  }, [loadOrders]);

  return (
    <div className="flex h-full flex-col gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-semibold text-foreground">Narudžbe</h1>

        <div className="flex gap-2">
          {["open", "closed", "cancelled", ""].map((s) => (
            <button
              key={s || "all"}
              onClick={() => setFilterStatus(s)}
              className={`rounded-lg px-3 py-1.5 text-sm font-medium transition ${
                filterStatus === s
                  ? "bg-primary text-primary-foreground"
                  : "border border-border bg-card text-muted-foreground hover:bg-secondary"
              }`}
            >
              {s ? STATUS_LABELS[s] : "Sve"}
            </button>
          ))}
        </div>
      </div>

      {loading ? (
        <p className="text-sm text-muted-foreground">Učitavanje...</p>
      ) : orders.length === 0 ? (
        <p className="text-sm text-muted-foreground">Nema narudžbi.</p>
      ) : (
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          {orders.map((order) => (
            <button
              key={order.id}
              type="button"
              onClick={() => setSelectedOrder(order)}
              className="rounded-lg border border-border bg-card p-4 text-left shadow-sm transition hover:border-primary hover:shadow-md"
            >
              <div className="mb-2 flex items-center justify-between gap-2">
                <span className="text-sm font-semibold text-card-foreground">
                  Stol {order.tableNumber}
                </span>
                <span
                  className={`rounded-full px-2 py-0.5 text-xs font-medium ${STATUS_COLORS[order.status] ?? "bg-slate-100 text-slate-600"}`}
                >
                  {STATUS_LABELS[order.status] ?? order.status}
                </span>
              </div>
              <p className="text-xs text-muted-foreground">{order.orderNumber}</p>
              <div className="mt-3 flex items-end justify-between">
                <span className="text-xs text-muted-foreground">
                  {order.items.length} stavki
                </span>
                <span className="text-base font-bold text-primary">
                  {order.totalAmount.toFixed(2)} €
                </span>
              </div>
              <p className="mt-1 text-xs text-muted-foreground">
                {new Date(order.openedAt).toLocaleString("hr-HR")}
              </p>
            </button>
          ))}
        </div>
      )}

      {selectedOrder && selectedOrder.status === "open" && (
        <OrderPanel
          tableId={selectedOrder.tableId}
          tableNumber={selectedOrder.tableNumber}
          onClose={() => setSelectedOrder(null)}
          onOrderChange={loadOrders}
        />
      )}

      {selectedOrder && selectedOrder.status !== "open" && (
        <BillModal
          orderId={selectedOrder.id}
          onClose={() => setSelectedOrder(null)}
        />
      )}
    </div>
  );
}
