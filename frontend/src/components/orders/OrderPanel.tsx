import { useCallback, useEffect, useState } from "react";
import orderService from "../../services/orderService";
import type { Bill, Order } from "../../types/order";
import AddItemModal from "./AddItemModal";
import BillView from "./BillView";
import PaymentModal from "./PaymentModal";

interface Props {
  tableId: number;
  tableNumber: string;
  onClose: () => void;
  onOrderChange?: () => void;
}

export default function OrderPanel({
  tableId,
  tableNumber,
  onClose,
  onOrderChange,
}: Props) {
  const [order, setOrder] = useState<Order | null | undefined>(undefined); // undefined = loading
  const [bill, setBill] = useState<Bill | null>(null);
  const [showAddItem, setShowAddItem] = useState(false);
  const [showPayment, setShowPayment] = useState(false);
  const [paying, setPaying] = useState(false);
  const [opening, setOpening] = useState(false);
  const [cancelling, setCancelling] = useState(false);
  const [updatingItemId, setUpdatingItemId] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  const loadOrder = useCallback(async () => {
    const o = await orderService.getOpenOrderByTable(tableId);
    setOrder(o);
    if (o) {
      const b = await orderService.getBill(o.id);
      setBill(b);
    } else {
      setBill(null);
    }
  }, [tableId]);

  useEffect(() => {
    loadOrder();
  }, [loadOrder]);

  const handleOpenOrder = async () => {
    setOpening(true);
    setError(null);
    try {
      await orderService.createOrder({ tableId });
      await loadOrder();
      onOrderChange?.();
    } catch {
      setError("Nije moguće otvoriti narudžbu.");
    } finally {
      setOpening(false);
    }
  };

  const handleAddItem = async (menuItemId: number, quantity: number) => {
    if (!order) return;
    setError(null);
    await orderService.addItem(order.id, { menuItemId, quantity });
    const b = await orderService.getBill(order.id);
    setBill(b);
    const o = await orderService.getOrder(order.id);
    setOrder(o);
  };

  const handleRemoveItem = async (itemId: number) => {
    if (!order) return;
    setError(null);
    await orderService.removeItem(order.id, itemId);
    const b = await orderService.getBill(order.id);
    setBill(b);
    const o = await orderService.getOrder(order.id);
    setOrder(o);
  };

  const handleUpdateItemQuantity = async (itemId: number, newQuantity: number) => {
    if (!order) return;
    if (newQuantity < 1) return;
    setUpdatingItemId(itemId);
    setError(null);
    try {
      await orderService.updateItem(order.id, itemId, newQuantity);
      const b = await orderService.getBill(order.id);
      setBill(b);
      const o = await orderService.getOrder(order.id);
      setOrder(o);
    } catch (e) {
      const msg =
        (e as { response?: { data?: { error?: string } } })?.response?.data?.error ??
        "Nije moguće promijeniti količinu.";
      setError(msg);
    } finally {
      setUpdatingItemId(null);
    }
  };

  const handlePayment = async (amount: number) => {
    if (!order) return;
    setPaying(true);
    try {
      const b = await orderService.processPayment(order.id, {
        paymentMethod: 0, // Cash
        amount,
      });
      setBill(b);
      const o = await orderService.getOrder(order.id);
      setOrder(o);
      setShowPayment(false);
      onOrderChange?.();
    } finally {
      setPaying(false);
    }
  };

  const handleCancel = async () => {
    if (!order || !confirm("Poništiti narudžbu?")) return;
    setCancelling(true);
    setError(null);
    try {
      await orderService.cancelOrder(order.id);
      setOrder(null);
      setBill(null);
      onOrderChange?.();
    } catch {
      setError("Nije moguće poništiti narudžbu.");
    } finally {
      setCancelling(false);
    }
  };

  return (
    <>
      <div className="fixed inset-0 z-40 flex justify-end">
        <div className="absolute inset-0 bg-black/30" onClick={onClose} />
        <aside className="relative z-50 flex h-full w-full max-w-md flex-col bg-background shadow-2xl">
          {/* Header */}
          <div className="flex items-center justify-between border-b border-border px-5 py-4">
            <div>
              <p className="text-xs text-muted-foreground">Narudžba</p>
              <h2 className="text-lg font-semibold text-foreground">Stol {tableNumber}</h2>
            </div>
            <button
              onClick={onClose}
              className="rounded-md p-1.5 text-muted-foreground hover:bg-secondary"
              aria-label="Zatvori"
            >
              ✕
            </button>
          </div>

          {/* Body */}
          <div className="flex-1 overflow-y-auto p-5">
            {order === undefined && (
              <p className="text-sm text-muted-foreground">Učitavanje...</p>
            )}

            {order === null && (
              <div className="flex flex-col items-center gap-4 py-10">
                <p className="text-sm text-muted-foreground">Nema otvorene narudžbe za ovaj stol.</p>
                <button
                  onClick={handleOpenOrder}
                  disabled={opening}
                  className="rounded-lg bg-primary px-6 py-2.5 text-sm font-semibold text-primary-foreground hover:bg-primary/90 disabled:opacity-50"
                >
                  {opening ? "Otvaranje..." : "Otvori narudžbu"}
                </button>
              </div>
            )}

            {order && bill && (
              <div className="space-y-4">
                <div className="flex gap-2">
                  <button
                    onClick={() => setShowAddItem(true)}
                    disabled={order.status !== "open"}
                    className="flex-1 rounded-lg border border-primary py-2 text-sm font-medium text-primary hover:bg-primary/5 disabled:opacity-40"
                  >
                    + Dodaj stavku
                  </button>
                  {order.status === "open" && (
                    <button
                      onClick={handleCancel}
                      disabled={cancelling}
                      className="rounded-lg border border-rose-300 px-3 py-2 text-sm font-medium text-rose-600 hover:bg-rose-50 disabled:opacity-50"
                    >
                      {cancelling ? "..." : "Poništi"}
                    </button>
                  )}
                </div>

                {/* Inline item quantity controls */}
                {bill.items.length > 0 && (
                  <div className="rounded-lg border border-border bg-card">
                    <ul className="divide-y divide-border">
                      {bill.items.map((item) => {
                        const isBusy = updatingItemId === item.id;
                        const isOpen = order.status === "open";
                        return (
                          <li
                            key={item.id}
                            className="flex items-center justify-between gap-3 px-4 py-2.5"
                          >
                            <span className="flex-1 text-sm text-card-foreground">
                              {item.menuItemName}
                            </span>
                            {isOpen ? (
                              <div className="flex items-center gap-1">
                                <button
                                  onClick={() =>
                                    handleUpdateItemQuantity(item.id, item.quantity - 1)
                                  }
                                  disabled={isBusy || item.quantity <= 1}
                                  className="flex h-6 w-6 items-center justify-center rounded border border-border text-xs hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-40"
                                  aria-label="Smanji količinu"
                                >
                                  −
                                </button>
                                <span className="w-6 text-center text-xs font-medium text-card-foreground">
                                  {item.quantity}
                                </span>
                                <button
                                  onClick={() =>
                                    handleUpdateItemQuantity(item.id, item.quantity + 1)
                                  }
                                  disabled={isBusy}
                                  className="flex h-6 w-6 items-center justify-center rounded border border-border text-xs hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-40"
                                  aria-label="Povećaj količinu"
                                >
                                  +
                                </button>
                              </div>
                            ) : (
                              <span className="text-xs text-muted-foreground">
                                ×{item.quantity}
                              </span>
                            )}
                            <span className="w-16 text-right text-sm font-medium text-card-foreground">
                              {item.lineTotal.toFixed(2)} €
                            </span>
                            {isOpen && (
                              <button
                                onClick={() => handleRemoveItem(item.id)}
                                disabled={isBusy}
                                className="ml-1 rounded p-1 text-xs text-rose-500 hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-40"
                                aria-label="Ukloni stavku"
                              >
                                ✕
                              </button>
                            )}
                          </li>
                        );
                      })}
                    </ul>
                  </div>
                )}

                <BillView
                  bill={bill}
                  onPay={() => setShowPayment(true)}
                  paying={paying}
                />
              </div>
            )}

            {error && (
              <div className="mt-3 flex items-start justify-between gap-2 rounded-md border border-destructive/30 bg-destructive/10 px-3 py-2">
                <p className="text-sm text-destructive">{error}</p>
                <button
                  onClick={() => setError(null)}
                  className="text-xs text-destructive hover:opacity-70"
                  aria-label="Zatvori grešku"
                >
                  ✕
                </button>
              </div>
            )}
          </div>
        </aside>
      </div>

      {showAddItem && order && (
        <AddItemModal
          onAdd={handleAddItem}
          onClose={() => setShowAddItem(false)}
        />
      )}

      {showPayment && bill && (
        <PaymentModal
          total={bill.totalAmount}
          onConfirm={handlePayment}
          onClose={() => setShowPayment(false)}
        />
      )}
    </>
  );
}
