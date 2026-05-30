import { useEffect, useState } from "react";
import FloorSelector from "../features/floors/components/FloorSelector";
import TablePicker from "../features/floors/components/TablePicker";
import OrderPanel from "../components/orders/OrderPanel";
import tableService from "../features/floors/services/tableService";
import orderService from "../services/orderService";
import type { RestaurantFloor } from "../features/floors/types/floor";
import type { Order } from "../types/order";
export default function HomePage() {
  const [floors, setFloors] = useState<RestaurantFloor[]>([]);
  const [openOrders, setOpenOrders] = useState<Order[]>([]);
  const [selectedFloor, setSelectedFloor] = useState<RestaurantFloor | null>(
    null
  );
  const [selectedOrderTable, setSelectedOrderTable] = useState<{
    id: number;
    name: string;
  } | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    let ignore = false;

    async function loadFloors() {
      try {
        const [nextFloors, nextOpenOrders] = await Promise.all([
          tableService.getFloors(),
          orderService.getOrders("open"),
        ]);
        if (!ignore) {
          setFloors(nextFloors);
          setOpenOrders(nextOpenOrders);
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
    <div className="flex h-full min-h-0 flex-col rounded-lg border border-border bg-background p-4">
      <div className="min-h-0 flex-1">
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
            openOrders={openOrders}
            onSelectFloor={setSelectedFloor}
            onSelectOrder={(order) =>
              setSelectedOrderTable({
                id: order.tableId,
                name: order.tableNumber,
              })
            }
          />
        )}
      </div>

      {selectedOrderTable && (
        <OrderPanel
          tableId={selectedOrderTable.id}
          tableNumber={selectedOrderTable.name}
          onClose={() => setSelectedOrderTable(null)}
          onOrderChange={() => setRefreshKey((k) => k + 1)}
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
