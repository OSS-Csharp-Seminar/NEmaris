import { useState } from "react";
import FloorPlan from "./FloorPlan";
import type {
  RestaurantFloor,
  RestaurantTable,
  TableStatus,
} from "../types/floor";

interface TablePickerProps {
  floor: RestaurantFloor;
  onBack: () => void;
}

export default function TablePicker({ floor, onBack }: TablePickerProps) {
  const [selectedTable, setSelectedTable] = useState<RestaurantTable | null>(
    null
  );

  return (
    <section className="flex h-full flex-col gap-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <p className="text-sm font-medium text-primary">Odabir stolova</p>
          <h1 className="mt-1 text-2xl font-semibold text-foreground">
            {floor.label}
          </h1>
        </div>

        <button
          type="button"
          onClick={onBack}
          className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium text-card-foreground transition hover:bg-secondary focus:outline-none focus:ring-2 focus:ring-ring"
        >
          Nazad na katove
        </button>
      </div>

      <div className="grid min-h-0 flex-1 gap-4 lg:grid-cols-[1fr_280px]">
        <FloorPlan
          tables={floor.tables}
          selectedTableId={selectedTable?.id ?? null}
          onSelectTable={setSelectedTable}
        />

        <TableDetails table={selectedTable} />
      </div>
    </section>
  );
}

const statusLabel: Record<TableStatus, string> = {
  available: "Slobodan",
  reserved: "Rezerviran",
  occupied: "Zauzet",
};

function TableDetails({ table }: { table: RestaurantTable | null }) {
  if (!table) {
    return (
      <aside className="rounded-lg border border-border bg-card p-4 shadow-sm">
        <p className="text-sm font-medium text-card-foreground">
          Odaberi stol na planu kata.
        </p>
        <p className="mt-2 text-sm text-muted-foreground">
          Detalji odabranog stola prikazat ce se ovdje.
        </p>
      </aside>
    );
  }

  return (
    <aside className="rounded-lg border border-border bg-card p-4 shadow-sm">
      <p className="text-sm font-medium text-primary">Odabrani stol</p>
      <h2 className="mt-2 text-xl font-semibold text-card-foreground">
        {table.name}
      </h2>
      <div className="mt-4 h-1.5 rounded-full bg-secondary">
        <div
          className="h-full rounded-full bg-primary"
          style={{ width: `${Math.min(table.capacity * 10, 100)}%` }}
        />
      </div>
      <dl className="mt-4 space-y-3 text-sm">
        <div className="flex justify-between gap-3">
          <dt className="text-muted-foreground">Kapacitet</dt>
          <dd className="font-medium text-card-foreground">{table.capacity}</dd>
        </div>
        <div className="flex justify-between gap-3">
          <dt className="text-muted-foreground">Status</dt>
          <dd className="font-medium text-card-foreground">
            {statusLabel[table.status]}
          </dd>
        </div>
      </dl>
    </aside>
  );
}
