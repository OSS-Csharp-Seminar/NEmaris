import type { SyntheticEvent } from "react";

interface CategoryFormProps {
  name: string;
  description: string;
  displayOrder: number;
  isSubmitting: boolean;
  editingCategoryId: number | null;
  onNameChange: (value: string) => void;
  onDescriptionChange: (value: string) => void;
  onDisplayOrderChange: (value: number) => void;
  onSubmit: (e: SyntheticEvent<HTMLFormElement>) => void | Promise<void>;
  onCancelEdit: () => void;
}

export default function CategoryForm({
  name,
  description,
  displayOrder,
  isSubmitting,
  editingCategoryId,
  onNameChange,
  onDescriptionChange,
  onDisplayOrderChange,
  onSubmit,
  onCancelEdit,
}: CategoryFormProps) {
  const inputClass =
    "w-full rounded-lg border border-input bg-background px-4 py-3 text-foreground outline-none transition-colors placeholder:text-muted-foreground focus:border-primary";

  return (
    <form onSubmit={onSubmit} className="mb-6 flex flex-col gap-4">
      <h4 className="text-lg font-semibold text-card-foreground">
        {editingCategoryId ? "Uredi kategoriju" : "Dodaj kategoriju"}
      </h4>

      <input
        className={inputClass}
        placeholder="Naziv kategorije"
        value={name}
        onChange={(e) => onNameChange(e.target.value)}
        required
      />

      <input
        className={inputClass}
        placeholder="Opis"
        value={description}
        onChange={(e) => onDescriptionChange(e.target.value)}
      />

      <input
        className={inputClass}
        placeholder="Redoslijed prikaza"
        type="number"
        min="1"
        step="1"
        value={displayOrder}
        onChange={(e) => onDisplayOrderChange(Number(e.target.value))}
        required
      />

      <button
        type="submit"
        disabled={isSubmitting}
        className="rounded-lg bg-primary px-4 py-3 font-medium text-primary-foreground transition-opacity hover:opacity-90 disabled:opacity-50"
      >
        {isSubmitting
          ? editingCategoryId
            ? "Azuriranje kategorije..."
            : "Dodavanje kategorije..."
          : editingCategoryId
          ? "Azuriraj kategoriju"
          : "Dodaj kategoriju"}
      </button>

      {editingCategoryId && (
        <button
          type="button"
          onClick={onCancelEdit}
          className="rounded-lg border border-input px-4 py-3 font-medium text-foreground transition-colors hover:bg-accent"
        >
          Odustani od uredjivanja
        </button>
      )}
    </form>
  );
}
