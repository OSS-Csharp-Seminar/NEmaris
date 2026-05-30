export type TableStatus = "available" | "reserved" | "occupied";

export interface RestaurantTable {
  id: string;
  name: string;
  capacity: number;
  guestCount: number;
  status: TableStatus;
  x: number;
  y: number;
  shape: "square" | "round" | "wide";
  rotation?: number;
}

export interface RestaurantFloor {
  id: string;
  label: string;
  tables: RestaurantTable[];
}

export interface ApiRestaurantTable {
  id: number;
  tableNumber: string;
  capacity: number;
  guestCount?: number;
  zone?: string | null;
  status: number;
  floor?: number;
  positionX?: number;
  positionY?: number;
  shape?: number;
  rotation?: number;
}
