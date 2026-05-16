import type { RestaurantFloor } from "../types/floor";

interface FloorSelectorProps {
  floors: RestaurantFloor[];
  onSelectFloor: (floor: RestaurantFloor) => void;
}

export default function FloorSelector({
  floors,
  onSelectFloor,
}: FloorSelectorProps) {
  return (
    <section className="flex h-full flex-col justify-center gap-6">
      <div>
        <h1 className="text-2xl font-semibold text-foreground">
          Odaberi kat
        </h1>
        <p className="mt-2 text-sm text-muted-foreground">
          Nakon odabira kata prikazat ce se dostupni stolovi.
        </p>
      </div>

      <div className="grid gap-4 sm:grid-cols-3">
        {floors.map((floor) => (
          <button
            key={floor.id}
            type="button"
            onClick={() => onSelectFloor(floor)}
            className="rounded-lg border border-border bg-card p-6 text-left shadow-sm transition hover:border-primary hover:bg-accent focus:outline-none focus:ring-2 focus:ring-ring"
          >
            <span className="text-lg font-semibold text-card-foreground">
              {floor.label}
            </span>
            <span className="mt-2 block text-sm text-muted-foreground">
              {floor.tables.length} stolova
            </span>
          </button>
        ))}
      </div>
    </section>
  );
}
