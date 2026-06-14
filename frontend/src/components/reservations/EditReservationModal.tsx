import { useEffect, useMemo, useState } from "react";
import reservationService from "../../services/reservationService";
import type { AvailableTable, Reservation } from "../../types/reservation";

interface EditReservationModalProps {
  reservation: Reservation;
  onClose: () => void;
  onSaved: (updated: Reservation) => void;
}

function isoToLocalInput(iso: string): string {
  const d = new Date(iso);
  const yyyy = d.getFullYear();
  const mm = String(d.getMonth() + 1).padStart(2, "0");
  const dd = String(d.getDate()).padStart(2, "0");
  const hh = String(d.getHours()).padStart(2, "0");
  const mi = String(d.getMinutes()).padStart(2, "0");
  return `${yyyy}-${mm}-${dd}T${hh}:${mi}`;
}

function localInputToIso(local: string): string {
  return new Date(local).toISOString();
}

function durationMinutes(startIso: string, endIso: string): number {
  return Math.round(
    (new Date(endIso).getTime() - new Date(startIso).getTime()) / 60_000,
  );
}

function extractErrorMessage(e: unknown): string | undefined {
  if (typeof e !== "object" || !e) return undefined;
  const response = (e as { response?: { data?: { message?: string } } }).response;
  return response?.data?.message;
}

export default function EditReservationModal({
  reservation,
  onClose,
  onSaved,
}: EditReservationModalProps) {
  const initialDuration = useMemo(
    () => durationMinutes(reservation.startTime, reservation.endTime),
    [reservation],
  );

  const [startLocal, setStartLocal] = useState(() =>
    isoToLocalInput(reservation.startTime),
  );
  const [duration, setDuration] = useState(initialDuration);
  const [partySize, setPartySize] = useState(reservation.partySize);
  const [tableNumber, setTableNumber] = useState(reservation.tableNumber);
  const [specialRequest, setSpecialRequest] = useState(
    reservation.specialRequest ?? "",
  );
  const [availability, setAvailability] = useState<AvailableTable[]>([]);
  const [availabilityLoading, setAvailabilityLoading] = useState(false);
  const [availabilityError, setAvailabilityError] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [saveError, setSaveError] = useState<string | null>(null);

  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (e.key === "Escape") onClose();
    };
    window.addEventListener("keydown", onKey);
    return () => window.removeEventListener("keydown", onKey);
  }, [onClose]);

  useEffect(() => {
    let ignore = false;

    async function fetchAvailability() {
      setAvailabilityError(null);
      setAvailabilityLoading(true);
      try {
        const startIso = localInputToIso(startLocal);
        const endIso = new Date(
          new Date(startLocal).getTime() + duration * 60_000,
        ).toISOString();
        const tables = await reservationService.getAvailableTables(
          startIso,
          endIso,
          partySize,
        );
        if (!ignore) setAvailability(tables);
      } catch (e: unknown) {
        if (!ignore) {
          setAvailability([]);
          setAvailabilityError(
            extractErrorMessage(e) ?? "Nije moguće dohvatiti dostupne stolove.",
          );
        }
      } finally {
        if (!ignore) setAvailabilityLoading(false);
      }
    }

    const timer = window.setTimeout(fetchAvailability, 300);
    return () => {
      ignore = true;
      window.clearTimeout(timer);
    };
  }, [startLocal, duration, partySize]);

  const tableOptions = useMemo(() => {
    const map = new Map<string, AvailableTable>();
    for (const t of availability) map.set(t.tableNumber, t);
    if (!map.has(reservation.tableNumber)) {
      map.set(reservation.tableNumber, {
        id: reservation.tableId,
        tableNumber: reservation.tableNumber,
        capacity: 0,
        zone: null,
      });
    }
    return Array.from(map.values()).sort((a, b) =>
      a.tableNumber.localeCompare(b.tableNumber),
    );
  }, [availability, reservation]);

  const handleSave = async () => {
    setSaveError(null);
    setSaving(true);
    try {
      const startIso = localInputToIso(startLocal);
      const endIso = new Date(
        new Date(startLocal).getTime() + duration * 60_000,
      ).toISOString();

      const updated = await reservationService.updateReservation(reservation.id, {
        phone: reservation.guestPhone,
        startTime: startIso,
        endTime: endIso,
        partySize,
        tableNumber,
        specialRequest: specialRequest.trim() || "",
      });
      onSaved(updated);
    } catch (e: unknown) {
      setSaveError(
        extractErrorMessage(e) ?? "Nije moguće spremiti promjene.",
      );
    } finally {
      setSaving(false);
    }
  };

  return (
    <div
      className="fixed inset-0 z-40 flex items-center justify-center bg-black/40 p-4"
      onClick={onClose}
    >
      <div
        className="flex w-full max-w-lg flex-col gap-4 rounded-2xl border border-border bg-card p-6 text-card-foreground shadow-xl"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex items-start justify-between gap-3">
          <div>
            <h2 className="text-lg font-semibold">Uredi rezervaciju</h2>
            <p className="text-sm text-muted-foreground">
              {reservation.guestFullName} · {reservation.guestPhone}
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            aria-label="Zatvori"
            className="text-muted-foreground hover:text-foreground"
          >
            ✕
          </button>
        </div>

        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
          <label className="flex flex-col gap-1 text-sm">
            <span className="text-muted-foreground">Početak</span>
            <input
              type="datetime-local"
              value={startLocal}
              onChange={(e) => setStartLocal(e.target.value)}
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-muted-foreground">Trajanje (min)</span>
            <input
              type="number"
              min={15}
              max={480}
              step={15}
              value={duration}
              onChange={(e) => setDuration(Number(e.target.value))}
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-muted-foreground">Broj gostiju</span>
            <input
              type="number"
              min={1}
              max={100}
              value={partySize}
              onChange={(e) => setPartySize(Number(e.target.value))}
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-muted-foreground">Stol</span>
            <select
              value={tableNumber}
              onChange={(e) => setTableNumber(e.target.value)}
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            >
              {tableOptions.map((t) => (
                <option key={t.tableNumber} value={t.tableNumber}>
                  {t.tableNumber}
                  {t.tableNumber === reservation.tableNumber ? " (trenutni)" : ""}
                  {t.capacity > 0 ? ` · ${t.capacity} mjesta` : ""}
                </option>
              ))}
            </select>
          </label>
        </div>

        <label className="flex flex-col gap-1 text-sm">
          <span className="text-muted-foreground">Posebna napomena</span>
          <textarea
            value={specialRequest}
            onChange={(e) => setSpecialRequest(e.target.value)}
            rows={2}
            className="resize-none rounded-lg border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
          />
        </label>

        <div className="text-xs text-muted-foreground">
          {availabilityLoading
            ? "Provjera dostupnosti..."
            : availabilityError
              ? availabilityError
              : `Dostupno: ${availability.length} stol(ov)a`}
        </div>

        {saveError && (
          <div className="rounded-lg border border-destructive/30 bg-destructive/10 px-3 py-2 text-sm text-destructive">
            {saveError}
          </div>
        )}

        <div className="flex justify-end gap-2">
          <button
            type="button"
            onClick={onClose}
            disabled={saving}
            className="rounded-lg border border-border bg-card px-4 py-2 text-sm font-medium text-foreground transition hover:bg-secondary disabled:opacity-50"
          >
            Odustani
          </button>
          <button
            type="button"
            onClick={handleSave}
            disabled={saving}
            className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition hover:bg-primary/90 disabled:opacity-50"
          >
            {saving ? "Sprema..." : "Spremi"}
          </button>
        </div>
      </div>
    </div>
  );
}
