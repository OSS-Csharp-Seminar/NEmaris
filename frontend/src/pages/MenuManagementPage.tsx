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

  const [itemName, setItemName] = useState("");
  const [itemDescription, setItemDescription] = useState("");
  const [itemPrice, setItemPrice] = useState(0);
  const [itemSku, setItemSku] = useState("");
  const [itemIsAvailable, setItemIsAvailable] = useState(true);

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

      if (loadedCategories.length > 0 && selectedCategoryId === null) {
        setSelectedCategoryId(loadedCategories[0].id);
      }
    } catch {
      setError("Failed to load menu categories.");
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
      setError("Failed to load menu items.");
    }
  };

  useEffect(() => {
    loadCategories();
    loadMenuItems();
  }, []);

  const handleCreateCategory = async (e: FormEvent) => {
    e.preventDefault();

    setError(null);
    setMessage(null);
    setIsSubmitting(true);

    try {
      const response = await menuCategoryService.create({
        name,
        description: description || undefined,
        displayOrder,
      });

      setMessage("Menu category created successfully.");

      setName("");
      setDescription("");
      setDisplayOrder(1);

      await loadCategories();

      if (response.data.id) {
        setSelectedCategoryId(response.data.id);
      }
    } catch {
      setError("Failed to create menu category.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCreateMenuItem = async (e: FormEvent) => {
    e.preventDefault();

    if (!selectedCategoryId) return;

    setError(null);
    setMessage(null);

    try {
      await menuItemService.create({
        categoryId: selectedCategoryId,
        name: itemName,
        description: itemDescription || undefined,
        price: itemPrice,
        status: 1,
        isAvailable: itemIsAvailable,
        sku: itemSku || undefined,
      });

      setMessage("Menu item created successfully.");

      setItemName("");
      setItemDescription("");
      setItemPrice(0);
      setItemSku("");
      setItemIsAvailable(true);

      await loadMenuItems();
    } catch {
      setError("Failed to create menu item.");
    }
  };

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
          Menu Management
        </h2>

        <p className="mb-6 text-sm text-gray-500">
          Create and manage restaurant menu categories and menu items.
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
              Categories
            </h3>

            <CategoryForm
              name={name}
              description={description}
              displayOrder={displayOrder}
              isSubmitting={isSubmitting}
              onNameChange={setName}
              onDescriptionChange={setDescription}
              onDisplayOrderChange={setDisplayOrder}
              onSubmit={handleCreateCategory}
            />

            <CategoryList
              categories={categories}
              selectedCategoryId={selectedCategoryId}
              isLoading={isLoading}
              onSelectCategory={setSelectedCategoryId}
            />
          </section>

          <section className="rounded-2xl bg-white p-6 shadow-lg lg:col-span-2">
            <div className="mb-4">
              <h3 className="text-xl font-semibold text-gray-800">
                {selectedCategory
                  ? `${selectedCategory.name} Items`
                  : "Menu Items"}
              </h3>

              <p className="text-sm text-gray-500">
                {selectedCategory
                  ? "Items assigned to the selected category."
                  : "Select a category to view its items."}
              </p>
            </div>

            {selectedCategory && (
              <MenuItemForm
                itemName={itemName}
                itemDescription={itemDescription}
                itemPrice={itemPrice}
                itemSku={itemSku}
                itemIsAvailable={itemIsAvailable}
                onItemNameChange={setItemName}
                onItemDescriptionChange={setItemDescription}
                onItemPriceChange={setItemPrice}
                onItemSkuChange={setItemSku}
                onItemIsAvailableChange={setItemIsAvailable}
                onSubmit={handleCreateMenuItem}
              />
            )}

            {!selectedCategory && (
              <p className="text-sm text-gray-500">
                Select a category from the left side.
              </p>
            )}

            {selectedCategory && (<MenuItemList items={selectedCategoryItems} />
        )}
          </section>
        </div>
      </div>
    </div>
  );
}