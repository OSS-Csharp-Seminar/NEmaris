import { useEffect, useState, type FormEvent } from "react";
import menuCategoryService, {
  type MenuCategory,
} from "../services/menuCategoryService";
import menuItemService, { type MenuItem } from "../services/menuItemsService";

import CategoryForm from "../components/menu/CategoryForm";
import CategoryList from "../components/menu/CategoryList";
import MenuItemForm from "../components/menu/MenuItemForm";
import MenuItemList from "../components/menu/MenuItemList";

export default function MenuManagementPage() {
  const [categories, setCategories] = useState<MenuCategory[]>([]);
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(
    null
  );

  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [displayOrder, setDisplayOrder] = useState(1);
  const [editingCategoryId, setEditingCategoryId] = useState<number | null>(
    null
  );

  const [itemName, setItemName] = useState("");
  const [itemDescription, setItemDescription] = useState("");
  const [itemPrice, setItemPrice] = useState(0);
  const [itemSku, setItemSku] = useState("");
  const [itemIsAvailable, setItemIsAvailable] = useState(true);
  const [itemStockQuantity, setItemStockQuantity] = useState(0);

  const [editingItemId, setEditingItemId] = useState<number | null>(null);

  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const normalizeCategories = (data: unknown): MenuCategory[] => {
    if (Array.isArray(data)) return data;

    if (
      data &&
      typeof data === "object" &&
      "items" in data &&
      Array.isArray((data as { items: unknown }).items)
    ) {
      return (data as { items: MenuCategory[] }).items;
    }

    if (
      data &&
      typeof data === "object" &&
      "data" in data &&
      Array.isArray((data as { data: unknown }).data)
    ) {
      return (data as { data: MenuCategory[] }).data;
    }

    return [];
  };

  const normalizeMenuItems = (data: unknown): MenuItem[] => {
    if (Array.isArray(data)) return data;

    if (
      data &&
      typeof data === "object" &&
      "items" in data &&
      Array.isArray((data as { items: unknown }).items)
    ) {
      return (data as { items: MenuItem[] }).items;
    }

    if (
      data &&
      typeof data === "object" &&
      "data" in data &&
      Array.isArray((data as { data: unknown }).data)
    ) {
      return (data as { data: MenuItem[] }).data;
    }

    return [];
  };

  const loadCategories = async () => {
    try {
      setIsLoading(true);
      setError(null);

      const response = await menuCategoryService.getAll();
      const loadedCategories = normalizeCategories(response.data);

      setCategories(loadedCategories);
      setSelectedCategoryId((currentCategoryId) => {
        if (loadedCategories.length === 0) return null;

        if (
          currentCategoryId &&
          loadedCategories.some((category) => category.id === currentCategoryId)
        ) {
          return currentCategoryId;
        }

        return loadedCategories[0].id;
      });
    } catch {
      setError("Nije moguce ucitati kategorije menija.");
    } finally {
      setIsLoading(false);
    }
  };

  const loadMenuItems = async () => {
    try {
      const response = await menuItemService.getAll();
      const loadedMenuItems = normalizeMenuItems(response.data);

      setMenuItems(loadedMenuItems);
    } catch {
      setError("Nije moguce ucitati stavke menija.");
    }
  };

  useEffect(() => {
    loadCategories();
    loadMenuItems();
  }, []);

  const handleSubmitCategory = async (e: FormEvent) => {
    e.preventDefault();

    setError(null);
    setMessage(null);

    if (!Number.isInteger(displayOrder) || displayOrder < 1) {
      setError("Redoslijed kategorije mora biti cijeli broj veci od 0.");
      return;
    }

    const displayOrderTaken = categories.some(
      (category) =>
        category.displayOrder === displayOrder &&
        category.id !== editingCategoryId
    );

    if (displayOrderTaken) {
      setError(`Redoslijed ${displayOrder} vec koristi druga kategorija menija.`);
      return;
    }

    setIsSubmitting(true);

    try {
      if (editingCategoryId) {
        await menuCategoryService.update(editingCategoryId, {
          name,
          description: description || undefined,
          displayOrder,
        });

        setMessage("Kategorija menija uspjesno je azurirana.");
        setSelectedCategoryId(editingCategoryId);
      } else {
        const response = await menuCategoryService.create({
          name,
          description: description || undefined,
          displayOrder,
        });

        setMessage("Kategorija menija uspjesno je dodana.");

        if (response.data.id) {
          setSelectedCategoryId(response.data.id);
        }
      }

      setEditingCategoryId(null);

      setName("");
      setDescription("");
      setDisplayOrder(1);

      await loadCategories();
    } catch {
      setError(
        editingCategoryId
          ? "Nije moguce azurirati kategoriju menija."
          : "Nije moguce dodati kategoriju menija."
      );
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelCategoryEdit = () => {
    setEditingCategoryId(null);

    setName("");
    setDescription("");
    setDisplayOrder(1);
  };

  const handleCreateMenuItem = async (e: FormEvent) => {
    e.preventDefault();

    if (!selectedCategoryId) return;

    setError(null);
    setMessage(null);

    if (!Number.isInteger(itemStockQuantity) || itemStockQuantity < 0) {
      setError("Kolicina u skladistu mora biti cijeli broj 0 ili veci.");
      return;
    }

    try {
      if (editingItemId) {
        await menuItemService.update(editingItemId, {
          categoryId: selectedCategoryId,
          name: itemName,
          description: itemDescription || undefined,
          price: itemPrice,
          status: 1,
          isAvailable: itemIsAvailable,
          stockQuantity: itemStockQuantity,
          sku: itemSku || undefined,
        });

        setMessage("Stavka menija uspjesno je azurirana.");
      } else {
        await menuItemService.create({
          categoryId: selectedCategoryId,
          name: itemName,
          description: itemDescription || undefined,
          price: itemPrice,
          status: 1,
          isAvailable: itemIsAvailable,
          stockQuantity: itemStockQuantity,
          sku: itemSku || undefined,
        });

        setMessage("Stavka menija uspjesno je dodana.");
      }

      setEditingItemId(null);

      setItemName("");
      setItemDescription("");
      setItemPrice(0);
      setItemSku("");
      setItemIsAvailable(true);
      setItemStockQuantity(0);

      await loadMenuItems();
    } catch {
      setError(
        editingItemId
          ? "Nije moguce azurirati stavku menija."
          : "Nije moguce dodati stavku menija."
      );
    }
  };

  const handleCancelEdit = () => {
    setEditingItemId(null);

    setItemName("");
    setItemDescription("");
    setItemPrice(0);
    setItemSku("");
    setItemIsAvailable(true);
    setItemStockQuantity(0);
  };

  const handleDeleteCategory = async (categoryId: number) => {
    try {
      setError(null);
      setMessage(null);

      await menuCategoryService.delete(categoryId);

      if (editingCategoryId === categoryId) {
        handleCancelCategoryEdit();
      }

      setMessage("Kategorija menija uspjesno je obrisana.");

      await Promise.all([loadCategories(), loadMenuItems()]);
    } catch {
      setError("Nije moguce obrisati kategoriju menija.");
    }
  };

  const handleEditCategory = (category: MenuCategory) => {
    setEditingCategoryId(category.id);
    setSelectedCategoryId(category.id);

    setName(category.name);
    setDescription(category.description || "");
    setDisplayOrder(category.displayOrder);
  };

  const handleDeleteMenuItem = async (itemId: number) => {
    try {
      setError(null);
      setMessage(null);

      await menuItemService.delete(itemId);

      setMessage("Stavka menija uspjesno je obrisana.");

      await loadMenuItems();
    } catch {
      setError("Nije moguce obrisati stavku menija.");
    }
  };

  const handleEditMenuItem = (item: MenuItem) => {
    setEditingItemId(item.id);

    setItemName(item.name);
    setItemDescription(item.description || "");
    setItemPrice(item.price);
    setItemSku(item.sku || "");
    setItemIsAvailable(item.isAvailable);
    setItemStockQuantity(item.stockQuantity);
  };

  const sortedCategories = [...categories].sort(
    (a, b) => a.displayOrder - b.displayOrder
  );

  const selectedCategory = categories.find(
    (category) => category.id === selectedCategoryId
  );

  const selectedCategoryItems = menuItems.filter(
    (item) => item.categoryId === selectedCategoryId
  );

  return (
    <div className="h-full overflow-y-auto bg-gray-50 p-6">
      <div className="mx-auto w-full max-w-7xl">
        <h2 className="mb-2 text-2xl font-bold text-gray-800">
          Upravljanje menijem
        </h2>

        <p className="mb-6 text-sm text-gray-500">
          Dodaj i uredi kategorije i stavke restoranskog menija.
        </p>

        {message && (
          <div className="mb-4 rounded-lg border border-green-200 bg-green-50 px-4 py-3 text-sm text-green-700">
            {message}
          </div>
        )}

        {error && (
          <div className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          <section className="rounded-2xl bg-white p-6 shadow-lg lg:col-span-1">
            <h3 className="mb-4 text-xl font-semibold text-gray-800">
              Kategorije
            </h3>

            <CategoryForm
              name={name}
              description={description}
              displayOrder={displayOrder}
              isSubmitting={isSubmitting}
              editingCategoryId={editingCategoryId}
              onNameChange={setName}
              onDescriptionChange={setDescription}
              onDisplayOrderChange={setDisplayOrder}
              onSubmit={handleSubmitCategory}
              onCancelEdit={handleCancelCategoryEdit}
            />

            <CategoryList
              categories={sortedCategories}
              selectedCategoryId={selectedCategoryId}
              isLoading={isLoading}
              onSelectCategory={setSelectedCategoryId}
              onDelete={handleDeleteCategory}
              onEdit={handleEditCategory}
            />
          </section>

          <section className="rounded-2xl bg-white p-6 shadow-lg lg:col-span-2">
            <div className="mb-4">
              <h3 className="text-xl font-semibold text-gray-800">
                {selectedCategory
                  ? `Stavke kategorije ${selectedCategory.name}`
                  : "Stavke menija"}
              </h3>

              <p className="text-sm text-gray-500">
                {selectedCategory
                  ? "Stavke dodijeljene odabranoj kategoriji."
                  : "Odaberi kategoriju za pregled stavki."}
              </p>
            </div>

            {selectedCategory && (
              <MenuItemForm
                itemName={itemName}
                itemDescription={itemDescription}
                itemPrice={itemPrice}
                itemSku={itemSku}
                itemIsAvailable={itemIsAvailable}
                itemStockQuantity={itemStockQuantity}
                onItemNameChange={setItemName}
                onItemDescriptionChange={setItemDescription}
                onItemPriceChange={setItemPrice}
                onItemSkuChange={setItemSku}
                onItemIsAvailableChange={setItemIsAvailable}
                onItemStockQuantityChange={setItemStockQuantity}
                onSubmit={handleCreateMenuItem}
                editingItemId={editingItemId}
                onCancelEdit={handleCancelEdit}
              />
            )}

            {!selectedCategory && (
              <p className="text-sm text-gray-500">
                Odaberi kategoriju s lijeve strane.
              </p>
            )}

            {selectedCategory && (
              <MenuItemList
                items={selectedCategoryItems}
                onDelete={handleDeleteMenuItem}
                onEdit={handleEditMenuItem}
              />
            )}
          </section>
        </div>
      </div>
    </div>
  );
}
