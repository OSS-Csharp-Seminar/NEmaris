import api from "./api";

export interface MenuItem {
  id: number;
  categoryId: number;
  name: string;
  description?: string;
  price: number;
  status: number;
  isAvailable: boolean;
  sku?: string;
  createdAt?: string;
  updatedAt?: string;
}

export interface CreateMenuItemRequest {
  categoryId: number;
  name: string;
  description?: string;
  price: number;
  status: number;
  isAvailable: boolean;
  sku?: string;
}

export interface UpdateMenuItemRequest {
  categoryId: number;
  name: string;
  description?: string;
  price: number;
  status: number;
  isAvailable: boolean;
  sku?: string;
}

export interface CreateMenuItemResponse {
  id: number;
}

const menuItemService = {
  getAll: () =>
    api.get<MenuItem[]>("/menu-items"),

  getById: (id: number) =>
    api.get<MenuItem>(`/menu-items/${id}`),

  create: (data: CreateMenuItemRequest) =>
    api.post<CreateMenuItemResponse>("/menu-items", data),

  update: (id: number, data: UpdateMenuItemRequest) =>
    api.put(`/menu-items/${id}`, data),

  delete: (id: number) =>
    api.delete(`/menu-items/${id}`),
};

export default menuItemService;