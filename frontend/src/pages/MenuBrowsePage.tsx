import { useEffect, useState } from "react";
import menuCategoryService, {
  type MenuCategory,
} from "../services/menuCategoryService";
import menuItemService, { type MenuItem } from "../services/menuItemsService";

export default function MenuBrowsePage() {
  const [categories, setCategories] = useState<MenuCategory[]>([]);
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | null>(null);
  const [selectedItemIds, setSelectedItemIds] = useState<number[]>([]);

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

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

  useEffect(() => {
    const loadMenu = async () => {
      try {
        setIsLoading(true);
        setError(null);

        const categoriesResponse = await menuCategoryService.getAll();
        const itemsResponse = await menuItemService.getAll();

        const loadedCategories = normalizeCategories(categoriesResponse.data);
        const loadedItems = normalizeMenuItems(itemsResponse.data).filter(
          (item) => item.isAvailable
        );

        setCategories(loadedCategories);
        setMenuItems(loadedItems);

        const sortedLoadedCategories = [...loadedCategories].sort(
          (a, b) => a.displayOrder - b.displayOrder
        );

        if (sortedLoadedCategories.length > 0) {
          setSelectedCategoryId(sortedLoadedCategories[0].id);
        }
      } catch {
        setError("Failed to load menu.");
      } finally {
        setIsLoading(false);
      }
    };

    loadMenu();
  }, []);

  const toggleSelectedItem = (itemId: number) => {
    setSelectedItemIds((currentIds) =>
      currentIds.includes(itemId)
        ? currentIds.filter((id) => id !== itemId)
        : [...currentIds, itemId]
    );
  };

  const selectedCategory = categories.find(
    (category) => category.id === selectedCategoryId
  );

  const sortedCategories = [...categories].sort(
    (a, b) => a.displayOrder - b.displayOrder
  );

  const selectedCategoryItems = menuItems.filter(
    (item) => item.categoryId === selectedCategoryId
  );

  const selectedItems = menuItems.filter((item) =>
    selectedItemIds.includes(item.id)
  );

  return (
    <div className="h-full overflow-y-auto bg-gray-50 p-6">
      <div className="mx-auto w-full max-w-7xl">
        <h2 className="mb-2 text-2xl font-bold text-gray-800">Menu</h2>

        <p className="mb-6 text-sm text-gray-500">
          Browse available menu items by category.
        </p>

        {error && (
          <div className="mb-4 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
            {error}
          </div>
        )}

        {isLoading && <p className="text-sm text-gray-500">Loading menu...</p>}

        {!isLoading && !error && (
          <div className="grid grid-cols-1 gap-6 lg:grid-cols-4">
            <section className="rounded-2xl bg-white p-6 shadow-lg lg:col-span-1">
              <h3 className="mb-4 text-xl font-semibold text-gray-800">
                Categories
              </h3>

              {sortedCategories.length === 0 && (
                <p className="text-sm text-gray-500">No categories found.</p>
              )}

              <div className="flex flex-col gap-3">
                {sortedCategories.map((category) => (
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
                    <h4 className="font-medium text-gray-800">
                      {category.name}
                    </h4>

                    <p className="text-sm text-gray-500">
                      {category.description || "No description"}
                    </p>
                  </button>
                ))}
              </div>
            </section>

            <section className="rounded-2xl bg-white p-6 shadow-lg lg:col-span-3">
              <h3 className="mb-4 text-xl font-semibold text-gray-800">
                {selectedCategory
                  ? `${selectedCategory.name} Items`
                  : "Menu Items"}
              </h3>

              {!selectedCategory && (
                <p className="text-sm text-gray-500">
                  Select a category to view items.
                </p>
              )}

              {selectedCategory && selectedCategoryItems.length === 0 && (
                <p className="text-sm text-gray-500">
                  No available items in this category.
                </p>
              )}

              {selectedCategory && selectedCategoryItems.length > 0 && (
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                  {selectedCategoryItems.map((item) => {
                    const isSelected = selectedItemIds.includes(item.id);

                    return (
                      <button
                        key={item.id}
                        type="button"
                        onClick={() => toggleSelectedItem(item.id)}
                        className={`rounded-xl border p-4 text-left transition-colors ${
                          isSelected
                            ? "border-blue-500 bg-blue-50"
                            : "border-gray-200 hover:bg-gray-50"
                        }`}
                      >
                        <div className="flex items-start justify-between gap-4">
                          <div>
                            <div className="flex items-center gap-2">
                              <span
                                className={`flex h-5 w-5 items-center justify-center rounded border text-xs ${
                                  isSelected
                                    ? "border-blue-500 bg-blue-500 text-white"
                                    : "border-gray-300 bg-white text-transparent"
                                }`}
                              >
                                ✓
                              </span>

                              <h4 className="font-semibold text-gray-800">
                                {item.name}
                              </h4>
                            </div>

                            <p className="mt-1 text-sm text-gray-500">
                              {item.description || "No description"}
                            </p>

                            <p className="mt-2 text-xs text-gray-400">
                              SKU: {item.sku || "No SKU"}
                            </p>
                          </div>

                          <p className="font-semibold text-gray-800">
                            €{item.price}
                          </p>
                        </div>
                      </button>
                    );
                  })}
                </div>
              )}
            </section>
          </div>
        )}

        <div className="fixed bottom-6 right-6 w-80 rounded-2xl bg-white p-4 shadow-xl">
          <p className="mb-2 text-sm font-medium text-gray-800">
            Selected items: {selectedItems.length}
          </p>

          {selectedItems.length > 0 && (
            <div className="mb-3 max-h-32 overflow-y-auto text-sm text-gray-600">
              {selectedItems.map((item) => (
                <div key={item.id} className="flex justify-between gap-3">
                  <span>{item.name}</span>
                  <span>€{item.price}</span>
                </div>
              ))}
            </div>
          )}

          <button
            type="button"
            disabled={selectedItems.length === 0}
            className="w-full rounded-xl bg-primary px-6 py-3 font-medium text-primary-foreground transition-opacity hover:opacity-90 disabled:opacity-50"
          >
            Add to Order
          </button>
        </div>
      </div>
    </div>
  );
}
