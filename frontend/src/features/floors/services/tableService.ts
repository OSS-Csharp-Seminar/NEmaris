import api from "../../../services/api";
import type {
  ApiRestaurantTable,
  RestaurantFloor,
  RestaurantTable,
  TableStatus,
} from "../types/floor";

const statusByApiValue: Record<number, TableStatus> = {
  0: "available",
  1: "reserved",
  2: "occupied",
};

const shapeByApiValue: Record<number, RestaurantTable["shape"]> = {
  0: "square",
  1: "round",
  2: "wide",
};

const fallbackLayoutByTableNumber: Record<
  string,
  Pick<ApiRestaurantTable, "floor" | "positionX" | "positionY" | "shape" | "rotation">
> = {
  "F1-T1": { floor: 1, positionX: 20, positionY: 22, shape: 0, rotation: 0 },
  "F1-T2": { floor: 1, positionX: 42, positionY: 22, shape: 0, rotation: 0 },
  "F1-T3": { floor: 1, positionX: 64, positionY: 22, shape: 1, rotation: 0 },
  "F1-T4": { floor: 1, positionX: 84, positionY: 22, shape: 0, rotation: 0 },
  "F1-T5": { floor: 1, positionX: 23, positionY: 48, shape: 2, rotation: 0 },
  "F1-T6": { floor: 1, positionX: 52, positionY: 48, shape: 1, rotation: 0 },
  "F1-T7": { floor: 1, positionX: 80, positionY: 48, shape: 2, rotation: 0 },
  "F1-T8": { floor: 1, positionX: 23, positionY: 76, shape: 0, rotation: 0 },
  "F1-T9": { floor: 1, positionX: 54, positionY: 76, shape: 2, rotation: 0 },
  "F1-T10": { floor: 1, positionX: 84, positionY: 76, shape: 1, rotation: 0 },
  "F2-T1": { floor: 2, positionX: 22, positionY: 24, shape: 0, rotation: 0 },
  "F2-T2": { floor: 2, positionX: 52, positionY: 24, shape: 2, rotation: 0 },
  "F2-T3": { floor: 2, positionX: 82, positionY: 24, shape: 1, rotation: 0 },
  "F2-T4": { floor: 2, positionX: 24, positionY: 52, shape: 1, rotation: 0 },
  "F2-T5": { floor: 2, positionX: 58, positionY: 52, shape: 2, rotation: 0 },
  "F2-T6": { floor: 2, positionX: 86, positionY: 52, shape: 0, rotation: 0 },
  "F2-T7": { floor: 2, positionX: 28, positionY: 78, shape: 0, rotation: 0 },
  "F2-T8": { floor: 2, positionX: 58, positionY: 78, shape: 1, rotation: 0 },
  "F2-T9": { floor: 2, positionX: 84, positionY: 78, shape: 0, rotation: 0 },
  "F3-T1": { floor: 3, positionX: 22, positionY: 24, shape: 0, rotation: 0 },
  "F3-T2": { floor: 3, positionX: 45, positionY: 24, shape: 0, rotation: 0 },
  "F3-T3": { floor: 3, positionX: 72, positionY: 24, shape: 1, rotation: 0 },
  "F3-T4": { floor: 3, positionX: 34, positionY: 54, shape: 2, rotation: 0 },
  "F3-T5": { floor: 3, positionX: 68, positionY: 54, shape: 1, rotation: 0 },
  "F3-T6": { floor: 3, positionX: 86, positionY: 54, shape: 2, rotation: 90 },
  "F3-T7": { floor: 3, positionX: 27, positionY: 80, shape: 1, rotation: 0 },
  "F3-T8": { floor: 3, positionX: 62, positionY: 80, shape: 2, rotation: 0 },
};

function getTableName(tableNumber: string) {
  return tableNumber.replace(/^F\d-/, "");
}

function withLayout(table: ApiRestaurantTable): Required<ApiRestaurantTable> {
  const fallback = fallbackLayoutByTableNumber[table.tableNumber];

  return {
    ...table,
    guestCount: table.guestCount ?? 0,
    floor: table.floor ?? fallback?.floor ?? 1,
    positionX: table.positionX ?? fallback?.positionX ?? 50,
    positionY: table.positionY ?? fallback?.positionY ?? 50,
    shape: table.shape ?? fallback?.shape ?? 0,
    rotation: table.rotation ?? fallback?.rotation ?? 0,
    zone: table.zone ?? null,
  };
}

function mapTable(table: ApiRestaurantTable): RestaurantTable {
  const tableWithLayout = withLayout(table);

  return {
    id: String(tableWithLayout.id),
    name: getTableName(tableWithLayout.tableNumber),
    capacity: tableWithLayout.capacity,
    guestCount: tableWithLayout.guestCount ?? 0,
    status: statusByApiValue[tableWithLayout.status] ?? "available",
    x: Number(tableWithLayout.positionX),
    y: Number(tableWithLayout.positionY),
    shape: shapeByApiValue[tableWithLayout.shape] ?? "square",
    rotation: tableWithLayout.rotation,
  };
}

function mapFloors(tables: ApiRestaurantTable[]): RestaurantFloor[] {
  const tablesWithLayout = tables.map(withLayout);

  return [1, 2, 3].map((floorNumber) => ({
    id: `floor-${floorNumber}`,
    label: `${floorNumber}. kat`,
    tables: tablesWithLayout
      .filter((table) => table.floor === floorNumber)
      .map(mapTable),
  }));
}

const tableService = {
  async getFloors(): Promise<RestaurantFloor[]> {
    const response = await api.get<ApiRestaurantTable[]>("/tables");
    return mapFloors(response.data);
  },

  async changeGuestCount(tableId: string, change: -1 | 1): Promise<RestaurantTable> {
    const response = await api.patch<ApiRestaurantTable>(
      `/tables/${tableId}/guest-count`,
      { change },
    );
    return mapTable(response.data);
  },

  async markOccupied(tableId: string): Promise<RestaurantTable> {
    const response = await api.post<ApiRestaurantTable>(`/tables/${tableId}/occupy`);
    return mapTable(response.data);
  },
};

export default tableService;
