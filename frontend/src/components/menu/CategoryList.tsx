import type { MenuCategory } from "../../services/menuCategoryService";

interface CategoryListProps {
  categories: MenuCategory[];
  selectedCategoryId: number | null;
  isLoading: boolean;
  onSelectCategory: (categoryId: number) => void;
  onDelete: (categoryId: number) => void;
  onEdit: (category: MenuCategory) => void;
}

export default function CategoryList({
  categories,
  selectedCategoryId,
  isLoading,
  onSelectCategory,
  onDelete,
  onEdit,
}: CategoryListProps) {
  if (isLoading) {
    return <p className="text-sm text-gray-500">Ucitavanje kategorija...</p>;
  }

  if (categories.length === 0) {
    return <p className="text-sm text-gray-500">Nema pronadjenih kategorija.</p>;
  }

  return (
    <div className="flex flex-col gap-3">
      {categories.map((category) => (
        <div
          key={category.id}
          className={`rounded-lg border px-4 py-3 transition-colors ${
            selectedCategoryId === category.id
              ? "border-blue-500 bg-blue-50"
              : "border-gray-200 hover:bg-gray-50"
          }`}
        >
          <div className="flex items-center justify-between gap-4">
            <button
              type="button"
              onClick={() => onSelectCategory(category.id)}
              className="flex-1 text-left"
            >
              <div>
                <h4 className="font-medium text-gray-800">{category.name}</h4>

                <p className="text-sm text-gray-500">
                  {category.description || "Nema opisa"}
                </p>
              </div>
            </button>

            <div className="flex flex-col items-end gap-3">
              <span className="rounded-full bg-gray-100 px-3 py-1 text-xs text-gray-600">
                Redoslijed: {category.displayOrder}
              </span>

              <div className="flex gap-2">
                <button
                  type="button"
                  onClick={() => onEdit(category)}
                  className="rounded-lg bg-blue-500 px-3 py-1 text-sm text-white transition-opacity hover:opacity-90"
                >
                  Uredi
                </button>

                <button
                  type="button"
                  onClick={() => onDelete(category.id)}
                  className="rounded-lg bg-red-500 px-3 py-1 text-sm text-white transition-opacity hover:opacity-90"
                >
                  Obrisi
                </button>
              </div>
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}
