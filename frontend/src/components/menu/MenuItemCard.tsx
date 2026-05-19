import type { MenuItem } from "../../services/menuItemsService";

interface MenuItemCardProps {
  item: MenuItem;
}

export default function MenuItemCard({ item }: MenuItemCardProps) {
  return (
    <div className="rounded-lg border border-gray-200 px-4 py-3">
      <div className="flex items-center justify-between gap-4">
        <div>
          <h4 className="font-medium text-gray-800">{item.name}</h4>

          <p className="text-sm text-gray-500">
            {item.description || "No description"}
          </p>

          <p className="text-sm text-gray-400">
            SKU: {item.sku || "No SKU"}
          </p>
        </div>

        <div className="text-right">
          <p className="font-semibold text-gray-800">€{item.price}</p>

          <span
            className={`text-xs ${
              item.isAvailable ? "text-green-600" : "text-red-600"
            }`}
          >
            {item.isAvailable ? "Available" : "Unavailable"}
          </span>
        </div>
      </div>
    </div>
  );
}