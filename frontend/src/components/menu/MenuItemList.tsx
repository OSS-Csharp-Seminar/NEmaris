import type { MenuItem } from "../../services/menuItemsService";
import MenuItemCard from "./MenuItemCard";

interface MenuItemListProps {
  items: MenuItem[];
}

export default function MenuItemList({ items }: MenuItemListProps) {
  if (items.length === 0) {
    return (
      <p className="text-sm text-gray-500">
        No menu items in this category.
      </p>
    );
  }

  return (
    <div className="flex flex-col gap-3">
      {items.map((item) => (
        <MenuItemCard key={item.id} item={item} />
      ))}
    </div>
  );
}