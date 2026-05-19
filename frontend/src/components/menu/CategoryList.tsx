import type { MenuCategory } from "../../services/menuCategoryService";

interface CategoryListProps {
  categories: MenuCategory[];
  selectedCategoryId: number | null;
  isLoading: boolean;
  onSelectCategory: (categoryId: number) => void;
}

export default function CategoryList({
  categories,
  selectedCategoryId,
  isLoading,
  onSelectCategory,
}: CategoryListProps) {
  if (isLoading) {
    return <p className="text-sm text-gray-500">Loading categories...</p>;
  }

  if (categories.length === 0) {
    return <p className="text-sm text-gray-500">No categories found.</p>;
  }

  return (
    <div className="flex flex-col gap-3">
      {categories.map((category) => (
        <button
          key={category.id}
          type="button"
          onClick={() => onSelectCategory(category.id)}
          className={`rounded-lg border px-4 py-3 text-left transition-colors ${
            selectedCategoryId === category.id
              ? "border-blue-500 bg-blue-50"
              : "border-gray-200 hover:bg-gray-50"
          }`}
        >
          <div className="flex items-center justify-between gap-4">
            <div>
              <h4 className="font-medium text-gray-800">{category.name}</h4>

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
  );
}