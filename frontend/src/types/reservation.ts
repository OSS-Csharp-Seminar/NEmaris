export type ReservationStatus =
  | "Active"
  | "Completed"
  | "Cancelled"
  | "NoShow"
  | "Late"
  | "Seated";

export const RESERVATION_STATUS_BY_API_VALUE: Record<number, ReservationStatus> = {
  0: "Active",
  1: "Completed",
  2: "Cancelled",
  3: "NoShow",
  4: "Late",
  5: "Seated",
};

export const RESERVATION_STATUS_TO_API_VALUE: Record<ReservationStatus, number> = {
  Active: 0,
  Completed: 1,
  Cancelled: 2,
  NoShow: 3,
  Late: 4,
  Seated: 5,
};

export interface Reservation {
  id: number;
  guestId: number;
  guestFullName: string;
  guestPhone: string;
  tableId: number;
  tableNumber: string;
  reservationDate: string;
  startTime: string;
  endTime: string;
  partySize: number;
  status: ReservationStatus;
  specialRequest?: string;
  reservedByUserId?: string;
}

export interface AvailableTable {
  id: number;
  tableNumber: string;
  capacity: number;
  zone?: string | null;
}

export interface UpdateReservationPayload {
  phone: string;
  startTime?: string;
  endTime?: string;
  partySize?: number;
  tableNumber?: string;
  specialRequest?: string;
}

export interface CreateReservationPayload {
  firstName: string;
  lastName: string;
  phone: string;
  tableId: number;
  startTime: string;
  endTime: string;
  partySize: number;
  specialRequest?: string;
}
