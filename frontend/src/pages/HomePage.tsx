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

  useEffect(() => {
    let ignore = false;

    async function loadFloors() {
      try {
        const nextFloors = await tableService.getFloors();
        if (!ignore) {
          setFloors(nextFloors);
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
  }, []);

  return (
    <div className="h-full rounded-lg border border-border bg-background p-6">
      {isLoading ? (
        <PageState title="Ucitavanje stolova" />
      ) : error ? (
        <PageState title={error} />
      ) : selectedFloor ? (
        <TablePicker
          floor={selectedFloor}
          onBack={() => setSelectedFloor(null)}
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
