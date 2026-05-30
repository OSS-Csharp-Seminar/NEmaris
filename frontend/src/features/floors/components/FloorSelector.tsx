import type { RestaurantFloor } from "../types/floor";
import type { Order } from "../../../types/order";

interface FloorSelectorProps {
  floors: RestaurantFloor[];
  openOrders: Order[];
  onSelectFloor: (floor: RestaurantFloor) => void;
  onSelectOrder: (order: Order) => void;
}

export default function FloorSelector({
  floors,
  openOrders,
  onSelectFloor,
  onSelectOrder,
}: FloorSelectorProps) {
  const totalTables = floors.reduce((sum, floor) => sum + floor.tables.length, 0);
  const totalCapacity = floors.reduce(
    (sum, floor) =>
      sum + floor.tables.reduce((floorSum, table) => floorSum + table.capacity, 0),
    0,
  );
  const availableTables = floors.reduce(
    (sum, floor) =>
      sum + floor.tables.filter((table) => table.status === "available").length,
    0,
  );

  return (
    <section className="flex h-full min-h-0 flex-col gap-4">
      <div className="rounded-2xl border border-border bg-card px-6 py-5 shadow-sm">
        <div className="flex flex-wrap items-end justify-between gap-4">
          <div>
            <p className="text-sm font-medium text-primary">Pregled restorana</p>
            <h1 className="mt-1 text-3xl font-semibold text-foreground">
              Odaberi kat
            </h1>
            <p className="mt-2 text-sm text-muted-foreground">
              Otvori plan kata za pregled stolova i upravljanje narudžbama.
            </p>
          </div>

          <div className="flex flex-wrap gap-2">
            <SummaryPill label="Katova" value={floors.length} />
            <SummaryPill label="Stolova" value={totalTables} />
            <SummaryPill label="Mjesta" value={totalCapacity} />
            <SummaryPill
              label="Slobodno"
              value={availableTables}
              className="bg-emerald-50 text-emerald-700"
            />
          </div>
        </div>
      </div>

      <div className="grid min-h-0 flex-1 gap-4 lg:grid-cols-3">
        {floors.map((floor) => {
          const available = floor.tables.filter(
            (table) => table.status === "available",
          ).length;
          const reserved = floor.tables.filter(
            (table) => table.status === "reserved",
          ).length;
          const occupied = floor.tables.filter(
            (table) => table.status === "occupied",
          ).length;
          const capacity = floor.tables.reduce(
            (sum, table) => sum + table.capacity,
            0,
          );
          const floorTableIds = new Set(
            floor.tables.map((table) => Number(table.id)),
          );
          const floorOpenOrders = openOrders.filter((order) =>
            floorTableIds.has(order.tableId),
          );

          return (
            <article
              key={floor.id}
              className="flex min-h-0 flex-col rounded-2xl border border-border bg-card p-6 text-left shadow-sm"
            >
              <div>
                <div className="flex items-start justify-between gap-3">
                  <div>
                    <span className="text-sm font-medium text-primary">
                      Plan stolova
                    </span>
                    <h2 className="mt-1 text-3xl font-semibold text-card-foreground">
                      {floor.label}
                    </h2>
                  </div>
                  <span className="rounded-full bg-secondary px-3 py-1 text-xs font-semibold text-secondary-foreground">
                    {floor.tables.length} stolova
                  </span>
                </div>

                <div className="mt-7 grid grid-cols-3 gap-2">
                  <StatusCount label="Slobodno" value={available} tone="green" />
                  <StatusCount label="Rezervirano" value={reserved} tone="yellow" />
                  <StatusCount label="Zauzeto" value={occupied} tone="red" />
                </div>
              </div>

              <div className="mt-5 min-h-0 flex-1 border-t border-border pt-4">
                <div className="mb-3 flex items-center justify-between gap-2">
                  <h3 className="text-sm font-semibold text-card-foreground">
                    Otvoreni računi
                  </h3>
                  <span className="rounded-full bg-secondary px-2 py-0.5 text-xs font-semibold text-secondary-foreground">
                    {floorOpenOrders.length}
                  </span>
                </div>

                <div className="max-h-36 space-y-2 overflow-y-auto pr-1">
                  {floorOpenOrders.length === 0 ? (
                    <p className="rounded-lg bg-secondary/40 px-3 py-3 text-xs text-muted-foreground">
                      Nema otvorenih računa.
                    </p>
                  ) : (
                    floorOpenOrders.map((order) => (
                      <button
                        key={order.id}
                        type="button"
                        onClick={() => onSelectOrder(order)}
                        className="flex w-full items-center justify-between gap-3 rounded-lg border border-border bg-background px-3 py-2 text-left transition hover:border-primary hover:bg-accent"
                      >
                        <span>
                          <strong className="block text-sm text-card-foreground">
                            Stol {order.tableNumber}
                          </strong>
                          <span className="block text-xs text-muted-foreground">
                            {order.items.length} stavki
                          </span>
                        </span>
                        <strong className="text-sm text-primary">
                          {order.totalAmount.toFixed(2)} €
                        </strong>
                      </button>
                    ))
                  )}
                </div>
              </div>

              <div className="mt-4 flex items-center justify-between border-t border-border pt-4">
                <span className="text-sm text-muted-foreground">
                  Ukupni kapacitet:{" "}
                  <strong className="text-card-foreground">{capacity}</strong>
                </span>
                <button
                  type="button"
                  onClick={() => onSelectFloor(floor)}
                  className="text-sm font-semibold text-primary transition hover:translate-x-1"
                >
                  Otvori kat &rarr;
                </button>
              </div>
            </article>
          );
        })}
      </div>
    </section>
  );
}

function SummaryPill({
  label,
  value,
  className = "bg-secondary text-secondary-foreground",
}: {
  label: string;
  value: number;
  className?: string;
}) {
  return (
    <span className={`rounded-full px-3 py-2 text-sm font-medium ${className}`}>
      {label}: <strong>{value}</strong>
    </span>
  );
}

const statusTone = {
  green: "bg-emerald-50 text-emerald-700",
  yellow: "bg-amber-50 text-amber-700",
  red: "bg-rose-50 text-rose-700",
};

function StatusCount({
  label,
  value,
  tone,
}: {
  label: string;
  value: number;
  tone: keyof typeof statusTone;
}) {
  return (
    <span className={`rounded-xl px-3 py-4 text-center ${statusTone[tone]}`}>
      <strong className="block text-2xl">{value}</strong>
      <span className="mt-1 block text-xs font-medium">{label}</span>
    </span>
  );
}
