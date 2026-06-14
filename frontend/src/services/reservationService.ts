import api from "./api";
import {
  RESERVATION_STATUS_BY_API_VALUE,
  RESERVATION_STATUS_TO_API_VALUE,
  type AvailableTable,
  type CreateReservationPayload,
  type Reservation,
  type ReservationStatus,
  type UpdateReservationPayload,
} from "../types/reservation";

interface ApiReservation {
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
  status: number;
  specialRequest?: string;
  reservedByUserId?: string;
}

function mapReservation(api: ApiReservation): Reservation {
  return {
    ...api,
    status: RESERVATION_STATUS_BY_API_VALUE[api.status] ?? "Active",
  };
}

const reservationService = {
  async createReservation(payload: CreateReservationPayload): Promise<Reservation> {
    const { data } = await api.post<ApiReservation>("/reservations", payload);
    return mapReservation(data);
  },

  async getReservations(fromDate?: string, toDate?: string): Promise<Reservation[]> {
    const params: Record<string, string> = {};
    if (fromDate) params.fromDate = fromDate;
    if (toDate) params.toDate = toDate;
    const { data } = await api.get<ApiReservation[]>("/reservations", { params });
    return data.map(mapReservation);
  },

  async cancelReservation(id: number, phone: string): Promise<Reservation> {
    const { data } = await api.delete<ApiReservation>(`/reservations/${id}`, {
      params: { phone },
    });
    return mapReservation(data);
  },

  async updateReservation(
    id: number,
    payload: UpdateReservationPayload,
  ): Promise<Reservation> {
    const { data } = await api.put<ApiReservation>(`/reservations/${id}`, payload);
    return mapReservation(data);
  },

  async updateStatus(id: number, status: ReservationStatus): Promise<Reservation> {
    const { data } = await api.patch<ApiReservation>(`/reservations/${id}/status`, {
      status: RESERVATION_STATUS_TO_API_VALUE[status],
    });
    return mapReservation(data);
  },

  async getAvailableTables(
    startTime: string,
    endTime: string,
    partySize: number,
  ): Promise<AvailableTable[]> {
    const { data } = await api.get<AvailableTable[]>("/reservations/available-tables", {
      params: { startTime, endTime, partySize },
    });
    return data;
  },
};

export default reservationService;
