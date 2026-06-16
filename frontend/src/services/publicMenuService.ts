import api from "./api";
import type { PublicMenuItem } from "../types/publicMenu";

const publicMenuService = {
  async getMenu(): Promise<PublicMenuItem[]> {
    const response = await api.get<PublicMenuItem[]>("/public/menu");
    return response.data;
  },
};

export default publicMenuService;
