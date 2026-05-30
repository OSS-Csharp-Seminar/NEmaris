import { useState } from "react";
import FloorPlan from "./FloorPlan";
import type {
  RestaurantFloor,
  RestaurantTable,
  TableStatus,
} from "../types/floor";
import tableService from "../services/tableService";
import OrderPanel from "../../../components/orders/OrderPanel";

interface TablePickerProps {
  floor: RestaurantFloor;
  onBack: () => void;
  onTableStatusChange?: () => void;
}

export default function TablePicker({
  floor,
  onBack,
  onTableStatusChange,
}: TablePickerProps) {
  const [selectedTable, setSelectedTable] = useState<RestaurantTable | null>(null);
  const [orderPanelTable, setOrderPanelTable] = useState<RestaurantTable | null>(null);
  const [isUpdatingTable, setIsUpdatingTable] = useState(false);
  const [tableError, setTableError] = useState<string | null>(null);

  const updateSelectedTable = async (update: () => Promise<RestaurantTable>) => {
    setIsUpdatingTable(true);
    setTableError(null);
    try {
      const nextTable = await update();
      setSelectedTable(nextTable);
      onTableStatusChange?.();
    } catch {
      setTableError("Nije moguce azurirati stol.");
    } finally {
      setIsUpdatingTable(false);
    }
  };

  return (
    <section className="flex h-full min-h-0 flex-col gap-4">
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

      <div className="grid min-h-0 flex-1 gap-4 lg:grid-cols-[minmax(0,1fr)_280px]">
        <FloorPlan
          tables={floor.tables}
          selectedTableId={selectedTable?.id ?? null}
          onSelectTable={setSelectedTable}
        />

        <TableDetails
          table={selectedTable}
          onOpenOrder={() => setOrderPanelTable(selectedTable)}
          onChangeGuestCount={(change) =>
            selectedTable &&
            updateSelectedTable(() =>
              tableService.changeGuestCount(selectedTable.id, change),
            )
          }
          onMarkOccupied={() =>
            selectedTable &&
            updateSelectedTable(() => tableService.markOccupied(selectedTable.id))
          }
          isUpdating={isUpdatingTable}
          error={tableError}
        />
      </div>

      {orderPanelTable && (
        <OrderPanel
          tableId={Number(orderPanelTable.id)}
          tableNumber={orderPanelTable.name}
          onClose={() => setOrderPanelTable(null)}
          onOrderChange={onTableStatusChange}
        />
      )}
    </section>
  );
}

const statusLabel: Record<TableStatus, string> = {
  available: "Slobodan",
  reserved: "Rezerviran",
  occupied: "Zauzet",
};

function TableDetails({
  table,
  onOpenOrder,
  onChangeGuestCount,
  onMarkOccupied,
  isUpdating,
  error,
}: {
  table: RestaurantTable | null;
  onOpenOrder: () => void;
  onChangeGuestCount: (change: -1 | 1) => void;
  onMarkOccupied: () => void;
  isUpdating: boolean;
  error: string | null;
}) {
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
    <aside className="flex flex-col gap-4 rounded-lg border border-border bg-card p-4 shadow-sm">
      <div>
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
      </div>

      <div className="rounded-lg border border-border bg-secondary/40 p-3">
        <p className="text-sm font-medium text-card-foreground">Broj osoba</p>
        <div className="mt-3 flex items-center justify-between gap-3">
          <button
            type="button"
            onClick={() => onChangeGuestCount(-1)}
            disabled={isUpdating || table.guestCount === 0}
            aria-label="Oduzmi osobu"
            className="flex h-10 w-10 items-center justify-center rounded-lg border border-border bg-card text-xl font-semibold transition hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-40"
          >
            -
          </button>
          <strong className="text-2xl text-card-foreground">
            {table.guestCount}
          </strong>
          <button
            type="button"
            onClick={() => onChangeGuestCount(1)}
            disabled={isUpdating || table.guestCount === table.capacity}
            aria-label="Dodaj osobu"
            className="flex h-10 w-10 items-center justify-center rounded-lg border border-border bg-card text-xl font-semibold transition hover:bg-secondary disabled:cursor-not-allowed disabled:opacity-40"
          >
            +
          </button>
        </div>
      </div>

      {table.status === "reserved" && (
        <button
          type="button"
          onClick={onMarkOccupied}
          disabled={isUpdating}
          className="w-full rounded-lg bg-rose-500 py-2.5 text-sm font-semibold text-white transition hover:bg-rose-600 disabled:opacity-50"
        >
          Oznaci kao zauzet
        </button>
      )}

      {error && <p className="text-sm text-destructive">{error}</p>}

      {table.status === "occupied" && (
        <button
          type="button"
          onClick={onOpenOrder}
          className="w-full rounded-lg bg-primary py-2.5 text-sm font-semibold text-primary-foreground hover:bg-primary/90 transition"
        >
          Pregledaj ili otvori narudžbu
        </button>
      )}
    </aside>
  );
}
