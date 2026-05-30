import api from "./api";
import type {
  Order,
  Bill,
  OrderItem,
  CreateOrderPayload,
  AddOrderItemPayload,
  CreatePaymentPayload,
  DailyStats,
} from "../types/order";

const orderService = {
  async createOrder(payload: CreateOrderPayload): Promise<Order> {
    const { data } = await api.post<Order>("/orders", payload);
    return data;
  },

  async getOrders(status?: string, todayOnly: boolean = true): Promise<Order[]> {
    const params: Record<string, string | boolean> = { todayOnly };
    if (status) params.status = status;
    const { data } = await api.get<Order[]>("/orders", { params });
    return data;
  },

  async getTodayStats(): Promise<DailyStats> {
    const { data } = await api.get<DailyStats>("/orders/stats/today");
    return data;
  },

  async getOrder(id: number): Promise<Order> {
    const { data } = await api.get<Order>(`/orders/${id}`);
    return data;
  },

  async getOpenOrderByTable(tableId: number): Promise<Order | null> {
    try {
      const { data } = await api.get<Order>(`/orders/by-table/${tableId}`);
      return data;
    } catch {
      return null;
    }
  },

  async getBill(orderId: number): Promise<Bill> {
    const { data } = await api.get<Bill>(`/orders/${orderId}/bill`);
    return data;
  },

  async addItem(orderId: number, payload: AddOrderItemPayload): Promise<OrderItem> {
    const { data } = await api.post<OrderItem>(`/orders/${orderId}/items`, payload);
    return data;
  },

  async updateItem(
    orderId: number,
    itemId: number,
    quantity: number,
  ): Promise<OrderItem> {
    const { data } = await api.put<OrderItem>(`/orders/${orderId}/items/${itemId}`, {
      quantity,
    });
    return data;
  },

  async removeItem(orderId: number, itemId: number): Promise<void> {
    await api.delete(`/orders/${orderId}/items/${itemId}`);
  },

  async processPayment(orderId: number, payload: CreatePaymentPayload): Promise<Bill> {
    const { data } = await api.post<Bill>(`/orders/${orderId}/pay`, payload);
    return data;
  },

  async cancelOrder(orderId: number): Promise<Order> {
    const { data } = await api.post<Order>(`/orders/${orderId}/cancel`);
    return data;
  },
};

export default orderService;
