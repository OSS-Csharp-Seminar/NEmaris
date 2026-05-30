import { useEffect, useState } from "react";
import menuItemService, { type MenuItem } from "../../services/menuItemsService";
import menuCategoryService, { type MenuCategory } from "../../services/menuCategoryService";

interface Props {
  onAdd: (menuItemId: number, quantity: number) => Promise<void>;
  onClose: () => void;
}

export default function AddItemModal({ onAdd, onClose }: Props) {
  const [categories, setCategories] = useState<MenuCategory[]>([]);
  const [items, setItems] = useState<MenuItem[]>([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);
  const [quantities, setQuantities] = useState<Record<number, number>>({});
  const [adding, setAdding] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    Promise.all([menuCategoryService.getAll(), menuItemService.getAll()]).then(
      ([catsResp, itemsResp]) => {
        const cats = catsResp.data;
        setCategories(cats);
        setItems(itemsResp.data);
        if (cats.length > 0) setSelectedCategoryId(cats[0].id);
      },
    );
  }, []);

  const filtered = selectedCategoryId
    ? items.filter((i) => i.categoryId === selectedCategoryId)
    : items;

  const handleAdd = async (item: MenuItem) => {
    const qty = Math.min(quantities[item.id] ?? 1, item.stockQuantity);
    if (!item.isAvailable || item.stockQuantity <= 0 || qty <= 0) return;

    setAdding(item.id);
    setError(null);
    try {
      await onAdd(item.id, qty);
      setItems((currentItems) =>
        currentItems.map((currentItem) =>
          currentItem.id === item.id
            ? {
                ...currentItem,
                stockQuantity: Math.max(0, currentItem.stockQuantity - qty),
              }
            : currentItem,
        ),
      );
      setQuantities((prev) => ({ ...prev, [item.id]: 1 }));
    } catch {
      setError("Greska pri dodavanju stavke.");
    } finally {
      setAdding(null);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <div className="flex h-[80vh] w-full max-w-2xl flex-col rounded-xl bg-background shadow-xl">
        <div className="flex items-center justify-between border-b border-border p-4">
          <h2 className="text-lg font-semibold text-foreground">Dodaj stavku</h2>
          <button
            onClick={onClose}
            className="rounded-md px-3 py-1.5 text-sm text-muted-foreground hover:bg-secondary"
          >
            Zatvori
          </button>
        </div>

        <div className="flex min-h-0 flex-1">
          <aside className="flex w-40 shrink-0 flex-col gap-1 overflow-y-auto border-r border-border p-3">
            {categories.map((cat) => (
              <button
                key={cat.id}
                onClick={() => setSelectedCategoryId(cat.id)}
                className={`rounded-md px-3 py-2 text-left text-sm transition ${
                  selectedCategoryId === cat.id
                    ? "bg-primary font-medium text-primary-foreground"
                    : "text-muted-foreground hover:bg-secondary"
                }`}
              >
                {cat.name}
              </button>
            ))}
          </aside>

          <div className="flex-1 space-y-2 overflow-y-auto p-3">
            {filtered.length === 0 && (
              <p className="p-2 text-sm text-muted-foreground">Nema stavki u ovoj kategoriji.</p>
            )}

            {filtered.map((item) => {
              const selectedQuantity = Math.min(
                quantities[item.id] ?? 1,
                Math.max(1, item.stockQuantity),
              );
              const isOutOfStock = item.stockQuantity <= 0;
              const canAdd = item.isAvailable && !isOutOfStock;

              return (
                <div
                  key={item.id}
                  className={`flex items-center justify-between gap-3 rounded-lg border border-border bg-card p-3 ${
                    canAdd ? "" : "opacity-60"
                  }`}
                >
                  <div className="min-w-0 flex-1">
                    <p className="truncate text-sm font-medium text-card-foreground">{item.name}</p>
                    <p className="text-sm font-semibold text-primary">{item.price.toFixed(2)} EUR</p>
                    <p className="text-xs text-muted-foreground">
                      Skladiste: {item.stockQuantity}
                    </p>
                    {!item.isAvailable && (
                      <p className="text-xs font-medium text-destructive">Nedostupno</p>
                    )}
                    {item.isAvailable && isOutOfStock && (
                      <p className="text-xs font-medium text-destructive">Nema na skladistu</p>
                    )}
                  </div>

                  <div className="flex shrink-0 items-center gap-2">
                    <button
                      onClick={() =>
                        setQuantities((p) => ({
                          ...p,
                          [item.id]: Math.max(1, (p[item.id] ?? 1) - 1),
                        }))
                      }
                      disabled={!canAdd}
                      className="flex h-7 w-7 items-center justify-center rounded border border-border text-sm hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      -
                    </button>
                    <span className="w-6 text-center text-sm font-medium">
                      {selectedQuantity}
                    </span>
                    <button
                      onClick={() =>
                        setQuantities((p) => ({
                          ...p,
                          [item.id]: Math.min(item.stockQuantity, (p[item.id] ?? 1) + 1),
                        }))
                      }
                      disabled={!canAdd || selectedQuantity >= item.stockQuantity}
                      className="flex h-7 w-7 items-center justify-center rounded border border-border text-sm hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      +
                    </button>

                    <button
                      onClick={() => handleAdd(item)}
                      disabled={!canAdd || adding === item.id}
                      className="rounded-md bg-primary px-3 py-1.5 text-xs font-medium text-primary-foreground hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-50"
                    >
                      {adding === item.id ? "..." : "Dodaj"}
                    </button>
                  </div>
                </div>
              );
            })}
          </div>
        </div>

        {error && (
          <p className="border-t border-border p-3 text-sm text-destructive">{error}</p>
        )}
      </div>
    </div>
  );
}
