import type { RestaurantTable, TableStatus } from "../types/floor";

interface FloorPlanProps {
  tables: RestaurantTable[];
  selectedTableId: string | null;
  onSelectTable: (table: RestaurantTable) => void;
}

const statusColor: Record<TableStatus, string> = {
  available: "border-emerald-500 bg-emerald-50 text-emerald-700",
  reserved: "border-amber-500 bg-amber-50 text-amber-700",
  occupied: "border-rose-500 bg-rose-50 text-rose-700",
};

interface TableSize {
  width: number;
  height: number;
}

const CHAIR_WIDTH = 28;
const CHAIR_HEIGHT = 16;
const MIN_CHAIR_SPACING = 1;
const CHAIR_TABLE_GAP = 3;

function getTableSize(table: RestaurantTable): TableSize {
  if (table.shape === "round") {
    const size = 44 + table.capacity * 4;
    return { width: size, height: size };
  }

  if (table.shape === "wide") {
    const sideChairCount = Math.ceil(table.capacity / 2);
    const minWidth =
      (sideChairCount + 1) * (CHAIR_WIDTH + MIN_CHAIR_SPACING);

    return {
      width: Math.max(68 + table.capacity * 8, minWidth),
      height: 40 + table.capacity * 3,
    };
  }

  return {
    width: 46 + table.capacity * 6,
    height: 46 + table.capacity * 6,
  };
}

function Chair({
  left,
  top,
  rotation,
}: {
  left: number;
  top: number;
  rotation: number;
}) {
  return (
    <span
      className="absolute rounded-full border border-slate-500 bg-white shadow-sm"
      style={{
        width: CHAIR_WIDTH,
        height: CHAIR_HEIGHT,
        left,
        top,
        transform: `translate(-50%, -50%) rotate(${rotation}deg)`,
      }}
    />
  );
}

function TableShape({
  table,
  size,
}: {
  table: RestaurantTable;
  size: TableSize;
}) {
  const tableClassName = `relative z-10 flex items-center justify-center border-2 text-xs font-semibold shadow-sm ${statusColor[table.status]}`;

  if (table.shape === "round") {
    return (
      <span
        className={`${tableClassName} rounded-full`}
        style={{ width: size.width, height: size.height }}
      />
    );
  }

  return (
    <span
      className={`${tableClassName} rounded-md`}
      style={{ width: size.width, height: size.height }}
    />
  );
}

function getChairPositions(table: RestaurantTable, size: TableSize) {
  if (table.shape === "round") {
    const centerX = size.width / 2;
    const centerY = size.height / 2;
    const radius = size.width / 2 + CHAIR_HEIGHT / 2 + CHAIR_TABLE_GAP;

    return Array.from({ length: table.capacity }, (_, index) => {
      const angle = (Math.PI * 2 * index) / table.capacity - Math.PI / 2;

      return {
        left: centerX + Math.cos(angle) * radius,
        top: centerY + Math.sin(angle) * radius,
        rotation: (angle * 180) / Math.PI + 90,
      };
    });
  }

  const topCount = Math.ceil(table.capacity / 2);
  const bottomCount = table.capacity - topCount;
  const top = -(CHAIR_HEIGHT / 2 + CHAIR_TABLE_GAP);
  const bottom = size.height + CHAIR_HEIGHT / 2 + CHAIR_TABLE_GAP;

  const topChairs = Array.from({ length: topCount }, (_, index) => ({
    left: ((index + 1) * size.width) / (topCount + 1),
    top,
    rotation: 0,
  }));

  const bottomChairs = Array.from({ length: bottomCount }, (_, index) => ({
    left: ((index + 1) * size.width) / (bottomCount + 1),
    top: bottom,
    rotation: 180,
  }));

  return [...topChairs, ...bottomChairs];
}

function formatUpcomingBadge(iso: string): string {
  const time = new Date(iso).toLocaleTimeString("hr-HR", {
    hour: "2-digit",
    minute: "2-digit",
  });
  return `Rez. ${time}`;
}

function DiningSet({ table }: { table: RestaurantTable }) {
  const size = getTableSize(table);
  const chairs = getChairPositions(table, size);
  const frameWidth = size.width + 40;
  const frameHeight = size.height + 40;
  const showUpcomingBadge =
    table.status === "available" && !!table.upcomingReservationAt;

  return (
    <span
      className="relative flex items-center justify-center"
      style={{ width: frameWidth, height: frameHeight }}
    >
      {showUpcomingBadge && (
        <span className="absolute inset-x-0 top-0 z-20 text-center text-[10px] font-semibold uppercase tracking-wide text-amber-700">
          <span className="inline-block rounded-full border border-amber-400 bg-amber-50 px-1.5 py-0.5">
            {formatUpcomingBadge(table.upcomingReservationAt!)}
          </span>
        </span>
      )}
      <span
        className="relative"
        style={{ width: size.width, height: size.height }}
      >
        {chairs.map((chair, index) => (
          <Chair key={index} {...chair} />
        ))}
        <TableShape table={table} size={size} />
      </span>

      <span className="absolute inset-x-0 bottom-0 text-center text-xs font-semibold text-slate-600">
        {table.name}
      </span>
    </span>
  );
}

export default function FloorPlan({
  tables,
  selectedTableId,
  onSelectTable,
}: FloorPlanProps) {
  return (
    <div className="relative h-full min-h-0 overflow-hidden rounded-lg border border-slate-300 bg-white shadow-sm">
      <div className="absolute inset-6 rounded-md border-2 border-slate-700 bg-slate-50" />
      <div className="absolute left-[8%] right-[8%] top-1/2 h-px bg-slate-200" />
      <div className="absolute bottom-[12%] top-[12%] left-1/2 w-px bg-slate-200" />
      <div className="absolute left-6 top-40 h-28 w-28 rounded-br-full border-b-2 border-r-2 border-slate-700 bg-slate-50" />
      <div className="absolute bottom-6 right-6 h-32 w-32 rounded-tl-full border-l-2 border-t-2 border-dashed border-slate-700" />

      {tables.map((table) => {
        const isSelected = selectedTableId === table.id;

        return (
          <button
            key={table.id}
            type="button"
            onClick={() => onSelectTable(table)}
            aria-label={`${table.name}, kapacitet ${table.capacity}`}
            className={`absolute flex items-center justify-center rounded-md focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 ${
              isSelected
                ? "z-20 scale-110 drop-shadow-md"
                : "z-10 hover:scale-105"
            } transition`}
            style={{
              left: `${table.x}%`,
              top: `${table.y}%`,
              transform: `translate(-50%, -50%) rotate(${table.rotation ?? 0}deg)`,
            }}
          >
            <DiningSet table={table} />
          </button>
        );
      })}
    </div>
  );
}
