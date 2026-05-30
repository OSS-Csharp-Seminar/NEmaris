import api from "./api";

export interface PublicOverview {
  totalTables: number;
  occupiedTables: number;
  reservedTables: number;
  availableTables: number;
  reservationsToday: number;
  upcomingReservations: number;
}

const publicOverviewService = {
  getOverview: () => api.get<PublicOverview>("/public/overview"),
};

export default publicOverviewService;
