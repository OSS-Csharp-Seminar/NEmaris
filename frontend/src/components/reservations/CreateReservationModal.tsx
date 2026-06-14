import { useEffect, useMemo, useState } from "react";
import reservationService from "../../services/reservationService";
import type {
  AvailableTable,
  CreateReservationPayload,
  Reservation,
} from "../../types/reservation";

interface CreateReservationModalProps {
  defaultDate: string;
  onClose: () => void;
  onCreated: (created: Reservation) => void;
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

function defaultStartLocal(dateOnly: string): string {
  const now = new Date();
  const target = new Date(dateOnly + "T00:00:00");
  if (
    target.getFullYear() === now.getFullYear() &&
    target.getMonth() === now.getMonth() &&
    target.getDate() === now.getDate()
  ) {
    const inOneHour = new Date(now.getTime() + 60 * 60_000);
    const minutes = inOneHour.getMinutes();
    const rounded = new Date(inOneHour);
    rounded.setMinutes(Math.ceil(minutes / 15) * 15, 0, 0);
    return isoToLocalInput(rounded.toISOString());
  }
  target.setHours(19, 0, 0, 0);
  return isoToLocalInput(target.toISOString());
}

function extractErrorMessage(e: unknown): string | undefined {
  if (typeof e !== "object" || !e) return undefined;
  const response = (e as { response?: { data?: { message?: string } } }).response;
  return response?.data?.message;
}

export default function CreateReservationModal({
  defaultDate,
  onClose,
  onCreated,
}: CreateReservationModalProps) {
  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [phone, setPhone] = useState("");
  const [startLocal, setStartLocal] = useState(() => defaultStartLocal(defaultDate));
  const [duration, setDuration] = useState(90);
  const [partySize, setPartySize] = useState(2);
  const [tableId, setTableId] = useState<number | null>(null);
  const [specialRequest, setSpecialRequest] = useState("");
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
        if (!ignore) {
          setAvailability(tables);
          setTableId((prev) => {
            if (prev && tables.some((t) => t.id === prev)) return prev;
            return tables[0]?.id ?? null;
          });
        }
      } catch (e: unknown) {
        if (!ignore) {
          setAvailability([]);
          setTableId(null);
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

  const tableOptions = useMemo(
    () =>
      [...availability].sort((a, b) => a.tableNumber.localeCompare(b.tableNumber)),
    [availability],
  );

  const canSave =
    firstName.trim().length >= 2 &&
    lastName.trim().length >= 2 &&
    phone.trim().length >= 7 &&
    partySize >= 1 &&
    duration >= 15 &&
    tableId !== null &&
    !saving;

  const handleSave = async () => {
    if (!canSave || tableId === null) return;
    setSaveError(null);
    setSaving(true);
    try {
      const startIso = localInputToIso(startLocal);
      const endIso = new Date(
        new Date(startLocal).getTime() + duration * 60_000,
      ).toISOString();

      const payload: CreateReservationPayload = {
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        phone: phone.trim(),
        tableId,
        startTime: startIso,
        endTime: endIso,
        partySize,
        specialRequest: specialRequest.trim() || undefined,
      };

      const created = await reservationService.createReservation(payload);
      onCreated(created);
    } catch (e: unknown) {
      setSaveError(
        extractErrorMessage(e) ?? "Nije moguće kreirati rezervaciju.",
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
          <h2 className="text-lg font-semibold">Nova rezervacija</h2>
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
            <span className="text-muted-foreground">Ime</span>
            <input
              type="text"
              value={firstName}
              onChange={(e) => setFirstName(e.target.value)}
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm">
            <span className="text-muted-foreground">Prezime</span>
            <input
              type="text"
              value={lastName}
              onChange={(e) => setLastName(e.target.value)}
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </label>

          <label className="flex flex-col gap-1 text-sm sm:col-span-2">
            <span className="text-muted-foreground">Telefon</span>
            <input
              type="tel"
              value={phone}
              onChange={(e) => setPhone(e.target.value)}
              placeholder="+385 ..."
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring"
            />
          </label>

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
              value={tableId ?? ""}
              onChange={(e) =>
                setTableId(e.target.value ? Number(e.target.value) : null)
              }
              disabled={tableOptions.length === 0}
              className="rounded-lg border border-input bg-background px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-ring disabled:opacity-50"
            >
              {tableOptions.length === 0 && <option value="">Nema dostupnih</option>}
              {tableOptions.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.tableNumber} · {t.capacity} mjesta
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
            disabled={!canSave}
            className="rounded-lg bg-primary px-4 py-2 text-sm font-medium text-primary-foreground transition hover:bg-primary/90 disabled:opacity-50"
          >
            {saving ? "Sprema..." : "Spremi"}
          </button>
        </div>
      </div>
    </div>
  );
}
