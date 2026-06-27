import type { MenuItem } from "../../services/menuItemsService";
import MenuItemCard from "./MenuItemCard";

interface MenuItemListProps {
  items: MenuItem[];
  onDelete: (itemId: number) => void;
  onEdit: (item: MenuItem) => void;
}

export default function MenuItemList({
  items,
  onDelete,
  onEdit,
}: MenuItemListProps) {
  if (items.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">
        Nema stavki menija u ovoj kategoriji.
      </p>
    );
  }

  return (
    <div className="flex flex-col gap-3">
      {items.map((item) => (
        <MenuItemCard
          key={item.id}
          item={item}
          onDelete={onDelete}
          onEdit={onEdit}
        />
      ))}
    </div>
  );
}
