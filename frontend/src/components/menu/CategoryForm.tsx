import type { FormEvent } from "react";

interface CategoryFormProps {
  name: string;
  description: string;
  displayOrder: number;
  isSubmitting: boolean;
  onNameChange: (value: string) => void;
  onDescriptionChange: (value: string) => void;
  onDisplayOrderChange: (value: number) => void;
  onSubmit: (e: FormEvent) => void;
}

export default function CategoryForm({
  name,
  description,
  displayOrder,
  isSubmitting,
  onNameChange,
  onDescriptionChange,
  onDisplayOrderChange,
  onSubmit,
}: CategoryFormProps) {
  const inputClass =
    "w-full rounded-lg border border-gray-300 px-4 py-3 outline-none focus:border-blue-500 transition-colors";

  return (
    <form onSubmit={onSubmit} className="mb-6 flex flex-col gap-4">
      <input
        className={inputClass}
        placeholder="Category name"
        value={name}
        onChange={(e) => onNameChange(e.target.value)}
        required
      />

      <input
        className={inputClass}
        placeholder="Description"
        value={description}
        onChange={(e) => onDescriptionChange(e.target.value)}
      />

      <input
        className={inputClass}
        placeholder="Display order"
        type="number"
        min="1"
        value={displayOrder}
        onChange={(e) => onDisplayOrderChange(Number(e.target.value))}
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
  );
}