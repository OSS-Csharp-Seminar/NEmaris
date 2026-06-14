import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import reservationService from "../services/reservationService";
import type {
  Reservation,
  ReservationStatus,
} from "../types/reservation";
import { RESERVATIONS_CHANGED_EVENT } from "../components/ui/ChatWidget";
import EditReservationModal from "../components/reservations/EditReservationModal";
import CreateReservationModal from "../components/reservations/CreateReservationModal";

const STATUS_LABEL: Record<ReservationStatus, string> = {
  Active: "Aktivna",
  Late: "Kasni",
  Seated: "Smještena",
  Completed: "Završena",
  Cancelled: "Otkazana",
  NoShow: "Nije došla",
};

const STATUS_COLOR: Record<ReservationStatus, string> = {
  Active: "bg-emerald-100 text-emerald-700",
  Late: "bg-amber-100 text-amber-700",
  Seated: "bg-blue-100 text-blue-700",
  Completed: "bg-slate-100 text-slate-600",
  Cancelled: "bg-rose-100 text-rose-700",
  NoShow: "bg-rose-100 text-rose-700",
};

const ALL_STATUSES: ReservationStatus[] = [
  "Active",
  "Late",
  "Seated",
  "Completed",
  "Cancelled",
  "NoShow",
];

function formatDateOnly(d: Date): string {
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const dd = String(d.getDate()).padStart(2, "0");
  return `${yyyy}-${mm}-${dd}`;
}

function formatTime(iso: string): string {
  const d = new Date(iso);
  return d.toLocaleTimeString("hr-HR", { hour: "2-digit", minute: "2-digit" });
}

function extractErrorMessage(e: unknown): string | undefined {
  if (typeof e !== "object" || !e) return undefined;
  const response = (e as { response?: { data?: { message?: string } } }).response;
  return response?.data?.message;
}

export default function ReservationsPage() {
  const [reservations, setReservations] = useState<Reservation[]>([]);
  const [date, setDate] = useState<string>(() => formatDateOnly(new Date()));
  const [statusFilter, setStatusFilter] = useState<ReservationStatus | "All">("All");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [editing, setEditing] = useState<Reservation | null>(null);
  const [creating, setCreating] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);
  const [refreshKey, setRefreshKey] = useState(0);

  useEffect(() => {
    let ignore = false;
    async function load() {
      setLoading(true);
      try {
        const data = await reservationService.getReservations(date, date);
        if (!ignore) {
          setReservations(data);
          setError(null);
        }
      } catch {
        if (!ignore) setError("Nije moguće učitati rezervacije.");
      } finally {
        if (!ignore) setLoading(false);
      }
    }
    load();
    return () => {
      ignore = true;
    };
  }, [date, refreshKey]);

  useEffect(() => {
    const handler = () => setRefreshKey((k) => k + 1);
    window.addEventListener(RESERVATIONS_CHANGED_EVENT, handler);
    return () => window.removeEventListener(RESERVATIONS_CHANGED_EVENT, handler);
  }, []);

  useEffect(() => {
    const POLL_MS = 10_000;
    const tick = () => {
      if (!document.hidden) setRefreshKey((k) => k + 1);
    };
    const id = window.setInterval(tick, POLL_MS);
    const onVisible = () => {
      if (!document.hidden) setRefreshKey((k) => k + 1);
    };
    document.addEventListener("visibilitychange", onVisible);
    return () => {
      window.clearInterval(id);
      document.removeEventListener("visibilitychange", onVisible);
    };
  }, []);

  const filtered = useMemo(() => {
    const sorted = [...reservations].sort(
      (a, b) => new Date(a.startTime).getTime() - new Date(b.startTime).getTime(),
    );
    if (statusFilter === "All") return sorted;
    return sorted.filter((r) => r.status === statusFilter);
  }, [reservations, statusFilter]);

  const applyUpdate = (updated: Reservation) => {
    setReservations((prev) => {
      if (updated.reservationDate !== date) {
        return prev.filter((r) => r.id !== updated.id);
      }
      const existing = prev.findIndex((r) => r.id === updated.id);
      if (existing === -1) return [...prev, updated];
      const next = [...prev];
      next[existing] = updated;
      return next;
    });
  };

  const handleStatusChange = async (r: Reservation, newStatus: ReservationStatus) => {
    setActionError(null);
    try {
      const updated = await reservationService.updateStatus(r.id, newStatus);
      applyUpdate(updated);
    } catch (e: unknown) {
      setActionError(extractErrorMessage(e) ?? "Nije moguće promijeniti status.");
    }
  };

  const handleCancel = async (r: Reservation) => {
    setActionError(null);
    if (!window.confirm(`Otkazati rezervaciju za ${r.guestFullName}?`)) return;
    try {
      const updated = await reservationService.cancelReservation(r.id, r.guestPhone);
      applyUpdate(updated);
    } catch (e: unknown) {
      setActionError(extractErrorMessage(e) ?? "Nije moguće otkazati rezervaciju.");
    }
  };

  const handleEdited = (updated: Reservation) => {
    setEditing(null);
    applyUpdate(updated);
  };

  const handleCreated = (created: Reservation) => {
    setCreating(false);
    applyUpdate(created);
    window.dispatchEvent(new Event(RESERVATIONS_CHANGED_EVENT));
  };

  return (
    <div className="flex h-full min-h-0 flex-col gap-4 overflow-hidden rounded-lg border border-border bg-background p-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-semibold text-foreground">Rezervacije</h1>
          <p className="text-sm text-muted-foreground">
            {filtered.length}{" "}
            {filtered.length === 1 ? "rezervacija" : "rezervacija(e)"} za odabrani datum
          </p>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          <Link
            to="/home"
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm font-medium text-foreground transition hover:bg-secondary"
          >
            Natrag na stolove
          </Link>
          <button
            type="button"
            onClick={() => setCreating(true)}
            className="rounded-lg bg-primary px-3 py-1.5 text-sm font-medium text-primary-foreground transition hover:bg-primary/90"
          >
            Nova rezervacija
          </button>
          <label className="flex items-center gap-2 text-sm">
            <span className="text-muted-foreground">Datum:</span>
            <input
              type="date"
              value={date}
              onChange={(e) => setDate(e.target.value)}
              className="rounded-lg border border-input bg-background px-3 py-1.5 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </label>
          <button
            type="button"
            onClick={() => setDate(formatDateOnly(new Date()))}
            className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm font-medium text-foreground transition hover:bg-secondary"
          >
            Danas
          </button>
        </div>
      </div>

      <div className="flex flex-wrap gap-2">
        <FilterChip
          label="Sve"
          active={statusFilter === "All"}
          onClick={() => setStatusFilter("All")}
        />
        {ALL_STATUSES.map((s) => (
          <FilterChip
            key={s}
            label={STATUS_LABEL[s]}
            active={statusFilter === s}
            onClick={() => setStatusFilter(s)}
          />
        ))}
      </div>

      {actionError && (
        <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
          {actionError}
        </div>
      )}

      <div className="min-h-0 flex-1 overflow-auto rounded-lg border border-border bg-card">
        {loading && reservations.length === 0 ? (
          <div className="flex h-full items-center justify-center">
            <p className="text-sm text-muted-foreground">Učitavanje rezervacija...</p>
          </div>
        ) : error ? (
          <div className="flex h-full flex-col items-center justify-center gap-3">
            <p className="text-sm text-destructive">{error}</p>
            <button
              type="button"
              onClick={() => setRefreshKey((k) => k + 1)}
              className="rounded-lg border border-border bg-card px-3 py-1.5 text-sm font-medium text-foreground transition hover:bg-secondary"
            >
              Pokušaj ponovno
            </button>
          </div>
        ) : filtered.length === 0 ? (
          <div className="flex h-full items-center justify-center">
            <p className="text-sm text-muted-foreground">Nema rezervacija.</p>
          </div>
        ) : (
          <table className="w-full text-left text-sm">
            <thead className="sticky top-0 bg-card text-xs uppercase text-muted-foreground">
              <tr>
                <th className="px-4 py-2 font-medium">Vrijeme</th>
                <th className="px-4 py-2 font-medium">Stol</th>
                <th className="px-4 py-2 font-medium">Gosti</th>
                <th className="px-4 py-2 font-medium">Ime</th>
                <th className="px-4 py-2 font-medium">Telefon</th>
                <th className="px-4 py-2 font-medium">Status</th>
                <th className="px-4 py-2 font-medium text-right">Akcije</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((r) => (
                <ReservationRow
                  key={r.id}
                  reservation={r}
                  onStatusChange={handleStatusChange}
                  onCancel={handleCancel}
                  onEdit={() => setEditing(r)}
                />
              ))}
            </tbody>
          </table>
        )}
      </div>

      {editing && (
        <EditReservationModal
          reservation={editing}
          onClose={() => setEditing(null)}
          onSaved={handleEdited}
        />
      )}

      {creating && (
        <CreateReservationModal
          defaultDate={date}
          onClose={() => setCreating(false)}
          onCreated={handleCreated}
        />
      )}
    </div>
  );
}

function FilterChip({
  label,
  active,
  onClick,
}: {
  label: string;
  active: boolean;
  onClick: () => void;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`rounded-lg px-3 py-1.5 text-sm font-medium transition ${
        active
          ? "bg-primary text-primary-foreground"
          : "border border-border bg-card text-muted-foreground hover:bg-secondary"
      }`}
    >
      {label}
    </button>
  );
}

function ReservationRow({
  reservation: r,
  onStatusChange,
  onCancel,
  onEdit,
}: {
  reservation: Reservation;
  onStatusChange: (r: Reservation, status: ReservationStatus) => void;
  onCancel: (r: Reservation) => void;
  onEdit: () => void;
}) {
  const canMutate = r.status === "Active" || r.status === "Late";
  const canComplete = r.status === "Seated";

  return (
    <tr className="border-t border-border hover:bg-secondary/30">
      <td className="whitespace-nowrap px-4 py-3 font-medium text-card-foreground">
        {formatTime(r.startTime)} – {formatTime(r.endTime)}
      </td>
      <td className="px-4 py-3 text-card-foreground">{r.tableNumber}</td>
      <td className="px-4 py-3 text-card-foreground">{r.partySize}</td>
      <td className="px-4 py-3 text-card-foreground">{r.guestFullName}</td>
      <td className="px-4 py-3 text-muted-foreground">{r.guestPhone}</td>
      <td className="px-4 py-3">
        <span
          className={`rounded-full px-2 py-0.5 text-xs font-medium ${STATUS_COLOR[r.status]}`}
        >
          {STATUS_LABEL[r.status]}
        </span>
      </td>
      <td className="px-4 py-3">
        <div className="flex flex-wrap justify-end gap-1.5">
          {canMutate && (
            <>
              <ActionButton
                onClick={() => onStatusChange(r, "Seated")}
                tone="primary"
              >
                Smjesti
              </ActionButton>
              <ActionButton onClick={() => onStatusChange(r, "NoShow")}>
                Nije došla
              </ActionButton>
              <ActionButton onClick={onEdit}>Uredi</ActionButton>
              <ActionButton onClick={() => onCancel(r)} tone="danger">
                Otkaži
              </ActionButton>
            </>
          )}
          {canComplete && (
            <ActionButton
              onClick={() => onStatusChange(r, "Completed")}
              tone="primary"
            >
              Završi
            </ActionButton>
          )}
        </div>
      </td>
    </tr>
  );
}

function ActionButton({
  children,
  onClick,
  tone = "default",
}: {
  children: React.ReactNode;
  onClick: () => void;
  tone?: "default" | "primary" | "danger";
}) {
  const toneClass =
    tone === "primary"
      ? "bg-primary text-primary-foreground hover:bg-primary/90"
      : tone === "danger"
        ? "border border-rose-300 bg-white text-rose-700 hover:bg-rose-50"
        : "border border-border bg-card text-foreground hover:bg-secondary";

  return (
    <button
      type="button"
      onClick={onClick}
      className={`rounded-md px-2.5 py-1 text-xs font-medium transition ${toneClass}`}
    >
      {children}
    </button>
  );
}
