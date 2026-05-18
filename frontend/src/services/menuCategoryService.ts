import api from "./api";

export interface MenuCategory {
  id: number;
  name: string;
  description?: string;
  displayOrder: number;
}

export interface CreateMenuCategoryRequest {
  name: string;
  description?: string;
  displayOrder: number;
}

export interface UpdateMenuCategoryRequest {
  name: string;
  description?: string;
  displayOrder: number;
}

export interface CreateMenuCategoryResponse {
  id: number;
}

const menuCategoryService = {
  getAll: () =>
    api.get<MenuCategory[]>("/menu-categories"),

  getById: (id: number) =>
    api.get<MenuCategory>(`/menu-categories/${id}`),

  create: (data: CreateMenuCategoryRequest) =>
    api.post<CreateMenuCategoryResponse>("/menu-categories", data),

  update: (id: number, data: UpdateMenuCategoryRequest) =>
    api.put(`/menu-categories/${id}`, data),

  delete: (id: number) =>
    api.delete(`/menu-categories/${id}`),
};

export default menuCategoryService;