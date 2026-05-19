import type { FormEvent } from "react";

interface MenuItemFormProps {
  itemName: string;
  itemDescription: string;
  itemPrice: number;
  itemSku: string;
  itemIsAvailable: boolean;
  onItemNameChange: (value: string) => void;
  onItemDescriptionChange: (value: string) => void;
  onItemPriceChange: (value: number) => void;
  onItemSkuChange: (value: string) => void;
  onItemIsAvailableChange: (value: boolean) => void;
  onSubmit: (e: FormEvent) => void;
}

export default function MenuItemForm({
  itemName,
  itemDescription,
  itemPrice,
  itemSku,
  itemIsAvailable,
  onItemNameChange,
  onItemDescriptionChange,
  onItemPriceChange,
  onItemSkuChange,
  onItemIsAvailableChange,
  onSubmit,
}: MenuItemFormProps) {
  const inputClass =
    "w-full rounded-lg border border-gray-300 px-4 py-3 outline-none focus:border-blue-500 transition-colors";

  return (
    <form
      onSubmit={onSubmit}
      className="mb-6 flex flex-col gap-4 rounded-xl border border-gray-200 p-4"
    >
      <h4 className="text-lg font-semibold text-gray-800">Add Menu Item</h4>

      <input
        className={inputClass}
        placeholder="Item name"
        value={itemName}
        onChange={(e) => onItemNameChange(e.target.value)}
        required
      />

      <input
        className={inputClass}
        placeholder="Description"
        value={itemDescription}
        onChange={(e) => onItemDescriptionChange(e.target.value)}
      />

      <input
        className={inputClass}
        placeholder="Price"
        type="number"
        step="0.01"
        min="0"
        value={itemPrice}
        onChange={(e) => onItemPriceChange(Number(e.target.value))}
        required
      />

      <input
        className={inputClass}
        placeholder="SKU"
        value={itemSku}
        onChange={(e) => onItemSkuChange(e.target.value)}
      />

      <label className="flex items-center gap-2 text-sm text-gray-700">
        <input
          type="checkbox"
          checked={itemIsAvailable}
          onChange={(e) => onItemIsAvailableChange(e.target.checked)}
        />
        Available
      </label>

      <button
        type="submit"
        className="rounded-lg bg-primary px-4 py-3 font-medium text-primary-foreground transition-opacity hover:opacity-90"
      >
        Create Menu Item
      </button>
    </form>
  );
}