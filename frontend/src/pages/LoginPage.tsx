import { useEffect, useState } from "react";
import LoginForm from "../components/ui/LoginForm";
import publicOverviewService, {
  type PublicOverview,
} from "../services/publicOverviewService";

const emptyOverview: PublicOverview = {
  totalTables: 0,
  occupiedTables: 0,
  reservedTables: 0,
  availableTables: 0,
  reservationsToday: 0,
  upcomingReservations: 0,
};

export default function LoginPage() {
  const [overview, setOverview] = useState<PublicOverview>(emptyOverview);
  const [isLoading, setIsLoading] = useState(true);
  const [hasError, setHasError] = useState(false);

  useEffect(() => {
    const loadOverview = async () => {
      try {
        const { data } = await publicOverviewService.getOverview();
        setOverview(data);
      } catch {
        setHasError(true);
      } finally {
        setIsLoading(false);
      }
    };

    void loadOverview();
  }, []);

  const displayValue = (value: number) => (isLoading ? "--" : value);

  return (
    <div className="flex h-full items-center justify-center bg-gray-50">
      <div className="flex min-h-[590px] w-full max-w-7xl flex-row gap-8 rounded-3xl border border-gray-200 bg-white p-8 shadow-lg">
        <div className="flex w-[440px]">
          <LoginForm />
        </div>

        <div className="flex-1 rounded-3xl bg-slate-50 p-8">
          <h1 className="text-3xl font-bold text-gray-800 mb-2">Overview</h1>

          <p className="text-sm text-gray-500 mb-6">
            Manage reservations, tables and guests from one place.
          </p>

          <div className="grid grid-cols-2 gap-4">
            <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
              <p className="text-sm text-gray-500">Total tables</p>
              <h2 className="mt-2 text-3xl font-bold text-gray-800">
                {displayValue(overview.totalTables)}
              </h2>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
              <p className="text-sm text-gray-500">Occupied tables</p>
              <h2 className="mt-2 text-3xl font-bold text-red-500">
                {displayValue(overview.occupiedTables)}
              </h2>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
              <p className="text-sm text-gray-500">Reservations today</p>
              <h2 className="mt-2 text-3xl font-bold text-blue-500">
                {displayValue(overview.reservationsToday)}
              </h2>
            </div>

            <div className="rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
              <p className="text-sm text-gray-500">Upcoming reservations</p>
              <h2 className="mt-2 text-3xl font-bold text-yellow-500">
                {displayValue(overview.upcomingReservations)}
              </h2>
            </div>
          </div>

          <div className="mt-6 rounded-2xl border border-gray-200 bg-white p-5 shadow-sm">
            <h3 className="mb-3 text-lg font-semibold text-gray-800">
              Current table availability
            </h3>
            <div className="flex flex-wrap gap-3 text-sm">
              <span className="rounded-full bg-green-50 px-4 py-2 font-medium text-green-700">
                Available: {displayValue(overview.availableTables)}
              </span>
              <span className="rounded-full bg-yellow-50 px-4 py-2 font-medium text-yellow-700">
                Reserved: {displayValue(overview.reservedTables)}
              </span>
              <span className="rounded-full bg-red-50 px-4 py-2 font-medium text-red-700">
                Occupied: {displayValue(overview.occupiedTables)}
              </span>
            </div>
            {hasError && (
              <p className="mt-4 text-sm text-red-600">
                Live overview is temporarily unavailable.
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
