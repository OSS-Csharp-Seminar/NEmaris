export interface OrderItem {
  id: number;
  menuItemId: number;
  menuItemName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface PaymentRecord {
  id: number;
  orderId: number;
  paymentMethod: string;
  amount: number;
  referenceNumber: string;
  paidAt: string;
}

export interface Order {
  id: number;
  orderNumber: string;
  tableId: number;
  tableNumber: string;
  waiterUserId: string;
  waiterName: string;
  guestId?: number;
  reservationId?: number;
  status: "open" | "closed" | "cancelled" | "voided";
  paymentStatus: "unpaid" | "paid" | "partiallypaid" | "voided";
  subtotal: number;
  discountAmount: number;
  taxRate: number;
  taxAmount: number;
  totalAmount: number;
  openedAt: string;
  closedAt?: string;
  items: OrderItem[];
}

export interface Bill {
  orderId: number;
  orderNumber: string;
  tableNumber: string;
  waiterName: string;
  status: string;
  paymentStatus: string;
  items: OrderItem[];
  subtotal: number;
  discountAmount: number;
  taxRate: number;
  taxAmount: number;
  totalAmount: number;
  payments: PaymentRecord[];
  openedAt: string;
  closedAt?: string;
}

export interface PaymentMethodTotal {
  paymentMethod: string;
  amount: number;
  count: number;
}

export interface TopItem {
  menuItemId: number;
  menuItemName: string;
  quantity: number;
  revenue: number;
}

export interface WaiterTotal {
  waiterUserId: string;
  waiterName: string;
  billCount: number;
  revenue: number;
}

export interface DailyStats {
  date: string;
  billCount: number;
  revenue: number;
  taxCollected: number;
  subtotal: number;
  tips: number;
  byPaymentMethod: PaymentMethodTotal[];
  topItems: TopItem[];
  byWaiter: WaiterTotal[];
}

export interface CreateOrderPayload {
  tableId: number;
  guestId?: number;
  reservationId?: number;
}

export interface AddOrderItemPayload {
  menuItemId: number;
  quantity: number;
}

export interface CreatePaymentPayload {
  paymentMethod: 0 | 1 | 2; // 0=Cash, 1=Card, 2=Voucher
  amount: number;
}
