import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import LoginForm from "../components/ui/LoginForm";
import publicOverviewService, {
  type PublicOverview,
} from "../services/publicOverviewService";
import { useAuth } from "../context/useAuth";

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
  const { isAuthenticated, isLoading: isAuthLoading } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    if (!isAuthLoading && isAuthenticated) {
      navigate("/home", { replace: true });
    }
  }, [isAuthLoading, isAuthenticated, navigate]);

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
    <div className="flex h-full items-center justify-center bg-background">
      <div className="flex min-h-[590px] w-full max-w-7xl flex-row gap-8 rounded-3xl border border-border bg-card p-8 shadow-lg">
        <div className="flex w-[440px]">
          <LoginForm />
        </div>

        <div className="flex-1 rounded-3xl bg-secondary/40 p-8">
          <h1 className="text-3xl font-bold text-card-foreground mb-2">Overview</h1>

          <p className="text-sm text-muted-foreground mb-6">
            Manage reservations, tables and guests from one place.
          </p>

          <div className="grid grid-cols-2 gap-4">
            <div className="rounded-2xl border border-border bg-card p-5 shadow-sm">
              <p className="text-sm text-muted-foreground">Total tables</p>
              <h2 className="mt-2 text-3xl font-bold text-card-foreground">
                {displayValue(overview.totalTables)}
              </h2>
            </div>

            <div className="rounded-2xl border border-border bg-card p-5 shadow-sm">
              <p className="text-sm text-muted-foreground">Occupied tables</p>
              <h2 className="mt-2 text-3xl font-bold text-rose-500">
                {displayValue(overview.occupiedTables)}
              </h2>
            </div>

            <div className="rounded-2xl border border-border bg-card p-5 shadow-sm">
              <p className="text-sm text-muted-foreground">Reservations today</p>
              <h2 className="mt-2 text-3xl font-bold text-primary">
                {displayValue(overview.reservationsToday)}
              </h2>
            </div>

            <div className="rounded-2xl border border-border bg-card p-5 shadow-sm">
              <p className="text-sm text-muted-foreground">Upcoming reservations</p>
              <h2 className="mt-2 text-3xl font-bold text-amber-500">
                {displayValue(overview.upcomingReservations)}
              </h2>
            </div>
          </div>

          <div className="mt-6 rounded-2xl border border-border bg-card p-5 shadow-sm">
            <h3 className="mb-3 text-lg font-semibold text-card-foreground">
              Current table availability
            </h3>
            <div className="flex flex-wrap gap-3 text-sm">
              <span className="rounded-full bg-emerald-500/15 px-4 py-2 font-medium text-emerald-600 dark:text-emerald-300">
                Available: {displayValue(overview.availableTables)}
              </span>
              <span className="rounded-full bg-amber-500/15 px-4 py-2 font-medium text-amber-700 dark:text-amber-300">
                Reserved: {displayValue(overview.reservedTables)}
              </span>
              <span className="rounded-full bg-rose-500/15 px-4 py-2 font-medium text-rose-600 dark:text-rose-300">
                Occupied: {displayValue(overview.occupiedTables)}
              </span>
            </div>
            {hasError && (
              <p className="mt-4 text-sm text-destructive">
                Live overview is temporarily unavailable.
              </p>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
