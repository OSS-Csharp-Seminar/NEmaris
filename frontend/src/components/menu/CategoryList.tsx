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
    return <p className="text-sm text-muted-foreground">Ucitavanje kategorija...</p>;
  }

  if (categories.length === 0) {
    return <p className="text-sm text-muted-foreground">Nema pronadjenih kategorija.</p>;
  }

  return (
    <div className="flex flex-col gap-3">
      {categories.map((category) => (
        <div
          key={category.id}
          className={`rounded-lg border px-4 py-3 transition-colors ${
            selectedCategoryId === category.id
              ? "border-primary bg-accent"
              : "border-border hover:bg-accent"
          }`}
        >
          <div className="flex items-center justify-between gap-4">
            <button
              type="button"
              onClick={() => onSelectCategory(category.id)}
              className="flex-1 text-left"
            >
              <div>
                <h4 className="font-medium text-card-foreground">{category.name}</h4>

                <p className="text-sm text-muted-foreground">
                  {category.description || "Nema opisa"}
                </p>
              </div>
            </button>

            <div className="flex flex-col items-end gap-3">
              <div className="flex gap-2">
                <button
                  type="button"
                  onClick={() => onEdit(category)}
                  className="rounded-lg bg-primary px-3 py-1 text-sm text-primary-foreground transition-opacity hover:opacity-90"
                >
                  Uredi
                </button>

                <button
                  type="button"
                  onClick={() => onDelete(category.id)}
                  className="rounded-lg bg-destructive px-3 py-1 text-sm text-destructive-foreground transition-opacity hover:opacity-90"
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
