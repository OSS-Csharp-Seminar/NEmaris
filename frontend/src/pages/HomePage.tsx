import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import FloorSelector from "../features/floors/components/FloorSelector";
import TablePicker from "../features/floors/components/TablePicker";
import OrderPanel from "../components/orders/OrderPanel";
import tableService from "../features/floors/services/tableService";
import orderService from "../services/orderService";
import type { RestaurantFloor } from "../features/floors/types/floor";
import type { Order } from "../types/order";
import { useAuth } from "../context/useAuth";

export default function HomePage() {
  const { isAdmin } = useAuth();
  const [floors, setFloors] = useState<RestaurantFloor[]>([]);
  const [openOrders, setOpenOrders] = useState<Order[]>([]);
  const [selectedFloor, setSelectedFloor] = useState<RestaurantFloor | null>(
    null,
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
      <div className="mb-4 flex justify-end gap-3">
        <Link
          to="/cash-register"
          className="rounded-xl border border-border bg-card px-6 py-3 font-medium text-card-foreground transition-opacity hover:opacity-80"
        >
          Kasa
        </Link>
        {isAdmin && (
          <Link
            to="/menu"
            className="rounded-xl bg-black px-6 py-3 font-medium text-white transition-opacity hover:opacity-80"
          >
            Upravljanje menijem
          </Link>
        )}
      </div>

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
