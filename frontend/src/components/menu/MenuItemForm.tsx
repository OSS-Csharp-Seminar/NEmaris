import type { FormEvent } from "react";

interface MenuItemFormProps {
  itemName: string;
  itemDescription: string;
  itemPrice: number;
  itemSku: string;
  itemIsAvailable: boolean;
  itemStockQuantity: number;
  onItemNameChange: (value: string) => void;
  onItemDescriptionChange: (value: string) => void;
  onItemPriceChange: (value: number) => void;
  onItemSkuChange: (value: string) => void;
  onItemIsAvailableChange: (value: boolean) => void;
  onItemStockQuantityChange: (value: number) => void;
  onSubmit: (e: FormEvent) => void;
  editingItemId: number | null;
  onCancelEdit: () => void;
}

export default function MenuItemForm({
  itemName,
  itemDescription,
  itemPrice,
  itemSku,
  itemIsAvailable,
  itemStockQuantity,
  onItemNameChange,
  onItemDescriptionChange,
  onItemPriceChange,
  onItemSkuChange,
  onItemIsAvailableChange,
  onItemStockQuantityChange,
  onSubmit,
  editingItemId,
  onCancelEdit,
}: MenuItemFormProps) {
  const inputClass =
    "w-full rounded-lg border border-gray-300 px-4 py-3 outline-none focus:border-blue-500 transition-colors";

  return (
    <form
      onSubmit={onSubmit}
      className="mb-6 flex flex-col gap-4 rounded-xl border border-gray-200 p-4"
    >
      <h4 className="text-lg font-semibold text-gray-800">
        {editingItemId ? "Uredi stavku menija" : "Dodaj stavku menija"}
      </h4>

      <input
        className={inputClass}
        placeholder="Naziv stavke"
        value={itemName}
        onChange={(e) => onItemNameChange(e.target.value)}
        required
      />

      <input
        className={inputClass}
        placeholder="Opis"
        value={itemDescription}
        onChange={(e) => onItemDescriptionChange(e.target.value)}
      />

      <input
        className={inputClass}
        placeholder="Cijena"
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

      <input
        className={inputClass}
        placeholder="Kolicina u skladistu"
        type="number"
        step="1"
        min="0"
        value={itemStockQuantity}
        onChange={(e) => onItemStockQuantityChange(Number(e.target.value))}
        required
      />

      <label className="flex items-center gap-2 text-sm text-gray-700">
        <input
          type="checkbox"
          checked={itemIsAvailable}
          onChange={(e) => onItemIsAvailableChange(e.target.checked)}
        />
        Dostupno
      </label>

      <button
        type="submit"
        className="rounded-lg bg-primary px-4 py-3 font-medium text-primary-foreground transition-opacity hover:opacity-90"
      >
        {editingItemId ? "Azuriraj stavku" : "Dodaj stavku"}
      </button>

      {editingItemId && (
        <button
          type="button"
          onClick={onCancelEdit}
          className="rounded-lg border border-gray-300 px-4 py-3 font-medium text-gray-700 transition-colors hover:bg-gray-100"
        >
          Odustani od uredjivanja
        </button>
      )}
    </form>
  );
}
