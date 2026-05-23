import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import FloorSelector from "../features/floors/components/FloorSelector";
import TablePicker from "../features/floors/components/TablePicker";
import tableService from "../features/floors/services/tableService";
import type { RestaurantFloor } from "../features/floors/types/floor";
export default function HomePage() {
  const [floors, setFloors] = useState<RestaurantFloor[]>([]);
  const [selectedFloor, setSelectedFloor] = useState<RestaurantFloor | null>(
    null
  );
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    let ignore = false;

    async function loadFloors() {
      try {
        const nextFloors = await tableService.getFloors();
        if (!ignore) {
          setFloors(nextFloors);
          // Keep selected floor in sync with refreshed data
          setSelectedFloor((prev) =>
            prev ? (nextFloors.find((f) => f.id === prev.id) ?? null) : null,
          );
          setError(null);
        }
      } catch {
        if (!ignore) {
          setError("Nije moguce ucitati stolove.");
        }
      } finally {
        if (!ignore) {
          setIsLoading(false);
        }
      }
    }

    loadFloors();

    return () => {
      ignore = true;
    };
  }, [refreshKey]);

  return (
    <div className="h-full rounded-lg border border-border bg-background p-6">
      <div className="mb-4 flex justify-end">
        <Link
        to="/menu"
        className="rounded-xl bg-black px-6 py-3 text-white font-medium transition-opacity hover:opacity-80"
      >
        Open Menu Management
      </Link>
      </div>
      {isLoading ? (
        <PageState title="Ucitavanje stolova" />
      ) : error ? (
        <PageState title={error} />
      ) : selectedFloor ? (
        <TablePicker
          floor={selectedFloor}
          onBack={() => setSelectedFloor(null)}
          onTableStatusChange={() => setRefreshKey((k) => k + 1)}
        />
      ) : (
        <FloorSelector
          floors={floors}
          onSelectFloor={setSelectedFloor}
        />
      )}
    </div>
  );
}

function PageState({ title }: { title: string }) {
  return (
    <div className="flex h-full items-center justify-center">
      <p className="text-sm font-medium text-muted-foreground">{title}</p>
    </div>
  );
}