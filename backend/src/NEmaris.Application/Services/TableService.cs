using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.Services;

public class TableService : ITableService
{
    private readonly ITableRepository _tableRepository;
    private readonly IOrderService _orderService;
    private readonly IReservationRepository _reservationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TableService(
        ITableRepository tableRepository,
        IOrderService orderService,
        IReservationRepository reservationRepository,
        IUnitOfWork unitOfWork)
    {
        _tableRepository = tableRepository;
        _orderService = orderService;
        _reservationRepository = reservationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<TableDto>> GetAllAsync()
    {
        var now = DateTime.UtcNow;
        var tables = await _tableRepository.GetAllAsync();
        var liveReserved = await _reservationRepository.GetTableIdsWithLiveReservationAsync(now);
        var upcoming = await _reservationRepository.GetUpcomingReservationsByTableAsync(now, RestaurantPolicies.WalkInDuration);
        return tables.Select(t => MapToDto(t, liveReserved, upcoming)).ToList();
    }

    public async Task<TableDto> GetByIdAsync(long id)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table is null)
            throw new KeyNotFoundException("Table not found.");

        var now = DateTime.UtcNow;
        var liveReserved = await _reservationRepository.GetTableIdsWithLiveReservationAsync(now);
        var upcoming = await _reservationRepository.GetUpcomingReservationsByTableAsync(now, RestaurantPolicies.WalkInDuration);
        return MapToDto(table, liveReserved, upcoming);
    }

    public async Task<long> CreateTableAsync(CreateTableDto dto)
    {
        var tableNumberTaken = await _tableRepository.ExistsByTableNumberAsync(dto.TableNumber);
        if (tableNumberTaken)
            throw new InvalidOperationException("Table number already exists.");

        var table = new RestaurantTables
        {
            TableNumber = dto.TableNumber,
            Capacity = dto.Capacity,
            GuestCount = 0,
            Zone = dto.Zone,
            Status = dto.Status,
            Floor = dto.Floor,
            PositionX = dto.PositionX,
            PositionY = dto.PositionY,
            Shape = dto.Shape,
            Rotation = dto.Rotation,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _tableRepository.AddAsync(table);
        return table.Id;
    }

    public async Task UpdateTableAsync(long id, UpdateTableDto dto)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table is null)
            throw new KeyNotFoundException("Table not found.");

        var tableNumberTaken = await _tableRepository.ExistsByTableNumberAsync(dto.TableNumber, id);
        if (tableNumberTaken)
            throw new InvalidOperationException("Table number already exists.");

        table.TableNumber = dto.TableNumber;
        table.Capacity = dto.Capacity;
        table.Zone = dto.Zone;
        table.Status = dto.Status;
        table.Floor = dto.Floor;
        table.PositionX = dto.PositionX;
        table.PositionY = dto.PositionY;
        table.Shape = dto.Shape;
        table.Rotation = dto.Rotation;
        table.UpdatedAt = DateTime.UtcNow;

        await _tableRepository.UpdateAsync(table);
    }

    public async Task<TableDto> ChangeGuestCountAsync(long id, int change)
    {
        if (change is not (-1 or 1))
            throw new InvalidOperationException("Guest count can only be changed by one person at a time.");

        var table = await _tableRepository.GetByIdAsync(id);
        if (table is null)
            throw new KeyNotFoundException("Table not found.");

        if (table.Status == TableStatus.Available && change > 0)
            throw new InvalidOperationException(
                "Slobodan stol ne može primati goste izravno. Koristite akciju 'Walk-in' za neprijavljene goste.");

        var nextGuestCount = table.GuestCount + change;
        if (nextGuestCount < 0)
            throw new InvalidOperationException("Guest count cannot be less than zero.");

        if (nextGuestCount > table.Capacity)
            throw new InvalidOperationException("Guest count cannot exceed table capacity.");

        table.GuestCount = nextGuestCount;
        table.Status = nextGuestCount == 0
            ? TableStatus.Available
            : table.Status == TableStatus.Seated
                ? TableStatus.Seated
                : TableStatus.Reserved;
        table.UpdatedAt = DateTime.UtcNow;

        await _tableRepository.UpdateAsync(table);
        return MapToDto(table);
    }

    public async Task<TableDto> MarkOccupiedAsync(long id, string waiterUserId)
    {
        var (resultTable, needsOrderForTable) = await _unitOfWork.InSerializableTransactionAsync<(RestaurantTables, long?)>(async () =>
        {
            var table = await _tableRepository.GetByIdAsync(id);
            if (table is null)
                throw new KeyNotFoundException("Table not found.");

            if (table.Status != TableStatus.Reserved || table.GuestCount == 0)
                throw new InvalidOperationException("Only a reserved table with guests can be marked as occupied.");

            table.Status = TableStatus.Seated;
            table.UpdatedAt = DateTime.UtcNow;

            await _tableRepository.UpdateAsync(table);

            var now = DateTime.UtcNow;
            var reservation = await _reservationRepository.GetActiveReservationForTableCoveringAsync(id, now);
            if (reservation is not null)
            {
                reservation.Status = ReservationStatus.Seated;
                reservation.UpdatedAt = now;
                await _reservationRepository.UpdateReservationAsync(reservation);
            }

            return (table, (long?)id);
        });

        if (needsOrderForTable.HasValue)
        {
            try
            {
                var existing = await _orderService.GetOpenOrderByTableIdAsync(needsOrderForTable.Value);
                if (existing is null)
                {
                    await _orderService.CreateOrderAsync(new CreateOrderDto { TableId = needsOrderForTable.Value }, waiterUserId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[mark-occupied] order auto-open failed for table {needsOrderForTable.Value}: {ex.Message}");
            }
        }

        return MapToDto(resultTable);
    }

    public async Task<TableDto> SeatWalkInAsync(long id, int guestCount, string waiterUserId)
    {
        if (guestCount <= 0)
            throw new InvalidOperationException("Broj gostiju mora biti najmanje 1.");

        var (resultTable, needsOrderForTable) = await _unitOfWork.InSerializableTransactionAsync<(RestaurantTables, long?)>(async () =>
        {
            var table = await _tableRepository.GetByIdAsync(id);
            if (table is null)
                throw new KeyNotFoundException("Table not found.");

            if (table.Status != TableStatus.Available)
                throw new InvalidOperationException("Walk-in je moguć samo za slobodne stolove.");

            if (guestCount > table.Capacity)
                throw new InvalidOperationException(
                    $"Broj gostiju ({guestCount}) premašuje kapacitet stola ({table.Capacity}).");

            var nextStart = await _reservationRepository.GetNextActiveReservationStartAsync(
                id, DateTime.UtcNow, RestaurantPolicies.WalkInDuration);
            if (nextStart.HasValue)
            {
                var localTime = RestaurantPolicies.FormatLocalTime(nextStart.Value);
                throw new InvalidOperationException(
                    $"Stol ima rezervaciju u {localTime}. Pokušajte drugi stol ili pričekajte da rezervacija prođe.");
            }

            table.Status = TableStatus.Seated;
            table.GuestCount = guestCount;
            table.UpdatedAt = DateTime.UtcNow;

            await _tableRepository.UpdateAsync(table);

            return (table, (long?)id);
        });

        if (needsOrderForTable.HasValue)
        {
            try
            {
                var existing = await _orderService.GetOpenOrderByTableIdAsync(needsOrderForTable.Value);
                if (existing is null)
                {
                    await _orderService.CreateOrderAsync(
                        new CreateOrderDto { TableId = needsOrderForTable.Value },
                        waiterUserId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[walk-in] order auto-open failed for table {needsOrderForTable.Value}: {ex.Message}");
            }
        }

        return MapToDto(resultTable);
    }

    public async Task DeleteTableAsync(long id)
    {
        var table = await _tableRepository.GetByIdAsync(id);
        if (table is null)
            throw new KeyNotFoundException("Table not found.");

        await _tableRepository.DeleteAsync(table);
    }

    private static TableDto MapToDto(
        RestaurantTables table,
        IReadOnlySet<long>? liveReservedIds = null,
        IReadOnlyDictionary<long, DateTime>? upcomingByTable = null)
    {
        var effectiveStatus = table.Status;
        if (effectiveStatus == TableStatus.Available &&
            liveReservedIds is not null &&
            liveReservedIds.Contains(table.Id))
        {
            effectiveStatus = TableStatus.Reserved;
        }

        DateTime? upcomingAt = null;
        if (table.Status == TableStatus.Available &&
            upcomingByTable is not null &&
            upcomingByTable.TryGetValue(table.Id, out var startTime))
        {
            upcomingAt = DateTime.SpecifyKind(startTime, DateTimeKind.Utc);
        }

        return new TableDto
        {
            Id = table.Id,
            TableNumber = table.TableNumber,
            Capacity = table.Capacity,
            GuestCount = table.GuestCount,
            Zone = table.Zone,
            Status = effectiveStatus,
            Floor = table.Floor,
            PositionX = table.PositionX,
            PositionY = table.PositionY,
            Shape = table.Shape,
            Rotation = table.Rotation,
            CreatedAt = table.CreatedAt,
            UpdatedAt = table.UpdatedAt,
            UpcomingReservationAt = upcomingAt
        };
    }
}
