import { useEffect, useState, type FormEvent } from "react";
import menuCategoryService, {
  type MenuCategory,
} from "../services/menuCategoryService";
import menuItemService, { type MenuItem } from "../services/menuItemsService";

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

  const inputClass =
    "w-full rounded-lg border border-gray-300 px-4 py-3 outline-none focus:border-blue-500 transition-colors";

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
          <div className="mb-4 rounded-lg bg-green-50 border border-green-200 px-4 py-3 text-sm text-green-700">
            {message}
          </div>
        )}

        {error && (
          <div className="mb-4 rounded-lg bg-red-50 border border-red-200 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}

        <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
          <section className="rounded-2xl bg-white p-6 shadow-lg lg:col-span-1">
            <h3 className="mb-4 text-xl font-semibold text-gray-800">
              Categories
            </h3>

            <form
              onSubmit={handleCreateCategory}
              className="mb-6 flex flex-col gap-4"
            >
              <input
                className={inputClass}
                placeholder="Category name"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
              />

              <input
                className={inputClass}
                placeholder="Description"
                value={description}
                onChange={(e) => setDescription(e.target.value)}
              />

              <input
                className={inputClass}
                placeholder="Display order"
                type="number"
                min="1"
                value={displayOrder}
                onChange={(e) => setDisplayOrder(Number(e.target.value))}
                required
              />

              <button
                type="submit"
                disabled={isSubmitting}
                className="rounded-lg bg-primary px-4 py-3 font-medium text-primary-foreground transition-opacity hover:opacity-90 disabled:opacity-50"
              >
                {isSubmitting ? "Creating category..." : "Create Category"}
              </button>
            </form>

            {isLoading && (
              <p className="text-sm text-gray-500">Loading categories...</p>
            )}

            {!isLoading && categories.length === 0 && (
              <p className="text-sm text-gray-500">No categories found.</p>
            )}

            {!isLoading && categories.length > 0 && (
              <div className="flex flex-col gap-3">
                {categories.map((category) => (
                  <button
                    key={category.id}
                    type="button"
                    onClick={() => setSelectedCategoryId(category.id)}
                    className={`rounded-lg border px-4 py-3 text-left transition-colors ${
                      selectedCategoryId === category.id
                        ? "border-blue-500 bg-blue-50"
                        : "border-gray-200 hover:bg-gray-50"
                    }`}
                  >
                    <div className="flex items-center justify-between gap-4">
                      <div>
                        <h4 className="font-medium text-gray-800">
                          {category.name}
                        </h4>

                        <p className="text-sm text-gray-500">
                          {category.description || "No description"}
                        </p>
                      </div>

                      <span className="rounded-full bg-gray-100 px-3 py-1 text-xs text-gray-600">
                        Order: {category.displayOrder}
                      </span>
                    </div>
                  </button>
                ))}
              </div>
            )}
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
              <form
                onSubmit={handleCreateMenuItem}
                className="mb-6 flex flex-col gap-4 rounded-xl border border-gray-200 p-4"
              >
                <h4 className="text-lg font-semibold text-gray-800">
                  Add Menu Item
                </h4>

                <input
                  className={inputClass}
                  placeholder="Item name"
                  value={itemName}
                  onChange={(e) => setItemName(e.target.value)}
                  required
                />

                <input
                  className={inputClass}
                  placeholder="Description"
                  value={itemDescription}
                  onChange={(e) => setItemDescription(e.target.value)}
                />

                <input
                  className={inputClass}
                  placeholder="Price"
                  type="number"
                  step="0.01"
                  min="0"
                  value={itemPrice}
                  onChange={(e) => setItemPrice(Number(e.target.value))}
                  required
                />

                <input
                  className={inputClass}
                  placeholder="SKU"
                  value={itemSku}
                  onChange={(e) => setItemSku(e.target.value)}
                />

                <label className="flex items-center gap-2 text-sm text-gray-700">
                  <input
                    type="checkbox"
                    checked={itemIsAvailable}
                    onChange={(e) => setItemIsAvailable(e.target.checked)}
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
            )}

            {!selectedCategory && (
              <p className="text-sm text-gray-500">
                Select a category from the left side.
              </p>
            )}

            {selectedCategory && selectedCategoryItems.length === 0 && (
              <p className="text-sm text-gray-500">
                No menu items in this category.
              </p>
            )}

            {selectedCategory && selectedCategoryItems.length > 0 && (
              <div className="flex flex-col gap-3">
                {selectedCategoryItems.map((item) => (
                  <div
                    key={item.id}
                    className="rounded-lg border border-gray-200 px-4 py-3"
                  >
                    <div className="flex items-center justify-between gap-4">
                      <div>
                        <h4 className="font-medium text-gray-800">
                          {item.name}
                        </h4>

                        <p className="text-sm text-gray-500">
                          {item.description || "No description"}
                        </p>

                        <p className="text-sm text-gray-400">
                          SKU: {item.sku || "No SKU"}
                        </p>
                      </div>

                      <div className="text-right">
                        <p className="font-semibold text-gray-800">
                          €{item.price}
                        </p>

                        <span
                          className={`text-xs ${
                            item.isAvailable
                              ? "text-green-600"
                              : "text-red-600"
                          }`}
                        >
                          {item.isAvailable ? "Available" : "Unavailable"}
                        </span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </section>
        </div>
      </div>
    </div>
  );
}