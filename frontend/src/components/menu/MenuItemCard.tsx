import type { MenuItem } from "../../services/menuItemsService";

interface MenuItemCardProps {
  item: MenuItem;
  onDelete: (itemId: number) => void;
  onEdit: (item: MenuItem) => void;
}

export default function MenuItemCard({
  item,
  onDelete,
  onEdit,
}: MenuItemCardProps) {
  return (
    <div className="rounded-lg border border-border px-4 py-3">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h4 className="font-medium text-card-foreground">{item.name}</h4>

          <p className="text-sm text-muted-foreground">
            {item.description || "Nema opisa"}
          </p>

          <p className="text-sm text-muted-foreground">
            SKU: {item.sku || "Nema SKU"}
          </p>

          <p className="text-sm text-muted-foreground">
            Skladiste: {item.stockQuantity}
          </p>
        </div>

        <div className="text-right">
          <p className="font-semibold text-card-foreground">€{item.price}</p>

          <span
            className={`text-xs ${
              item.isAvailable ? "text-success" : "text-destructive"
            }`}
          >
            {item.isAvailable ? "Dostupno" : "Nedostupno"}
          </span>
          <div className="mt-3 flex gap-2">
            <button
              type="button"
              onClick={() => onEdit(item)}
              className="rounded-lg bg-primary px-3 py-1 text-sm text-primary-foreground transition-opacity hover:opacity-90"
            >
              Uredi
            </button>
            <button
              type="button"
              onClick={() => onDelete(item.id)}
              className="rounded-lg bg-destructive px-3 py-1 text-sm text-destructive-foreground transition-opacity hover:opacity-90"
            >
              Obrisi
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
