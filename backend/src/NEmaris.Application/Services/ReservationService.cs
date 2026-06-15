using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.Services;

public class ReservationService : IReservationService
{
    private static readonly HashSet<string> PlaceholderValues = new(StringComparer.OrdinalIgnoreCase)
    {
        "<nil>", "nil", "null", "undefined", "n/a", "na", "none", "unknown",
        "tbd", "tba", "?", "??", "???", "-", "--", "...", "string"
    };

    private static readonly HashSet<string> PlaceholderNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "doe", "john doe", "jane doe", "test", "tester", "testuser", "test user",
        "sample", "example", "anonymous", "anon", "guest", "customer", "demo",
        "firstname", "lastname", "first name", "last name", "fname", "lname",
        "foo", "bar", "baz", "asdf", "qwerty"
    };

    private readonly IReservationRepository _reservationRepository;
    private readonly ITableRepository _tableRepository;
    private readonly IOrderService _orderService;
    private readonly IUnitOfWork _unitOfWork;

    public ReservationService(
        IReservationRepository reservationRepository,
        ITableRepository tableRepository,
        IOrderService orderService,
        IUnitOfWork unitOfWork)
    {
        _reservationRepository = reservationRepository;
        _tableRepository = tableRepository;
        _orderService = orderService;
        _unitOfWork = unitOfWork;
    }

    private static bool IsPlaceholder(string value) =>
        PlaceholderValues.Contains(value.Trim());

    private static string RequireRealValue(string? value, string fieldName)
    {
        var trimmed = (value ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed) || IsPlaceholder(trimmed))
            throw new InvalidOperationException($"{fieldName} is required and must be a real value, not '{trimmed}'.");
        return trimmed;
    }

    private static string RequireRealName(string? value, string fieldName)
    {
        var trimmed = RequireRealValue(value, fieldName);
        if (PlaceholderNames.Contains(trimmed))
            throw new InvalidOperationException($"{fieldName} '{trimmed}' looks like a placeholder. Ask the guest for their real name.");
        return trimmed;
    }

    private static string RequireRealPhone(string? value)
    {
        var trimmed = RequireRealValue(value, "Phone number");
        var normalized = PhoneNormalizer.Normalize(trimmed);
        var digits = new string(normalized.Where(char.IsDigit).ToArray());

        if (digits.Length < 7)
            throw new InvalidOperationException($"Phone number '{trimmed}' is too short. Ask the guest for a real phone number.");

        if (digits.Distinct().Count() == 1)
            throw new InvalidOperationException($"Phone number '{trimmed}' looks like a placeholder. Ask the guest for a real phone number.");

        if (IsSequentialDigits(digits))
            throw new InvalidOperationException($"Phone number '{trimmed}' looks like a placeholder. Ask the guest for a real phone number.");

        return normalized;
    }

    private static bool IsSequentialDigits(string digits)
    {
        const int window = 7;
        if (digits.Length < window) return false;
        for (var start = 0; start <= digits.Length - window; start++)
        {
            var ascending = true;
            var descending = true;
            for (var i = start + 1; i < start + window; i++)
            {
                if (digits[i] - digits[i - 1] != 1) ascending = false;
                if (digits[i - 1] - digits[i] != 1) descending = false;
            }
            if (ascending || descending) return true;
        }
        return false;
    }

    private static void RejectPlaceholderNamePair(string firstName, string lastName)
    {
        if (string.Equals(firstName, lastName, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"First name '{firstName}' and last name '{lastName}' are identical. " +
                "This looks like a name was duplicated instead of asking the guest for their first name. " +
                "Ask the guest for their first name explicitly.");

        var combined = $"{firstName} {lastName}".Trim();
        if (PlaceholderNames.Contains(combined))
            throw new InvalidOperationException($"Name '{combined}' looks like a placeholder. Ask the guest for their real name.");
    }

    public async Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto, string? reservedByUserId)
    {
        if (dto.StartTime == default || dto.EndTime == default)
            throw new InvalidOperationException("Start time and end time are required.");

        if (dto.EndTime <= dto.StartTime)
            throw new InvalidOperationException("End time must be later than start time.");

        if (dto.StartTime < DateTime.UtcNow.AddMinutes(-1))
            throw new InvalidOperationException("Start time is already in the past. Ask the guest for a future time (for example the same time tomorrow).");

        if (dto.PartySize <= 0)
            throw new InvalidOperationException("Party size must be greater than zero.");

        var firstName = RequireRealName(dto.FirstName, "First name");
        var lastName = RequireRealName(dto.LastName, "Last name");
        RejectPlaceholderNamePair(firstName, lastName);
        var normalizedPhone = RequireRealPhone(dto.Phone);

        var normalizedEmail = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        var normalizedNotes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        var normalizedSpecialRequest = string.IsNullOrWhiteSpace(dto.SpecialRequest) ? null : dto.SpecialRequest.Trim();

        return await _unitOfWork.InSerializableTransactionAsync(async () =>
        {
            var table = await _reservationRepository.GetTableByIdAsync(dto.TableId);
            if (table is null)
                throw new KeyNotFoundException("Selected table does not exist.");

            if (dto.PartySize > table.Capacity)
                throw new InvalidOperationException("Party size exceeds selected table capacity.");

            var isOverlapping = await _reservationRepository.HasOverlappingReservationAsync(dto.TableId, dto.StartTime, dto.EndTime);
            if (isOverlapping)
                throw new InvalidOperationException("Selected table is already reserved for that time slot.");

            await EnsureNoWalkInClashAsync(dto.TableId, dto.StartTime, dto.EndTime);

            var guest = await _reservationRepository.GetGuestByPhoneAsync(normalizedPhone);
            if (guest is null)
            {
                guest = new Guests
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = normalizedPhone,
                    Email = normalizedEmail,
                    Notes = normalizedNotes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _reservationRepository.AddGuestAsync(guest);
            }
            else
            {
                guest.FirstName = firstName;
                guest.LastName = lastName;
                guest.Email = normalizedEmail;
                guest.Notes = normalizedNotes;
                guest.UpdatedAt = DateTime.UtcNow;

                await _reservationRepository.UpdateGuestAsync(guest);
            }

            var reservation = new Reservations
            {
                GuestId = guest.Id,
                TableId = table.Id,
                ReservedByUserId = reservedByUserId,
                ReservationDate = DateOnly.FromDateTime(dto.StartTime),
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                PartySize = dto.PartySize,
                Status = ReservationStatus.Active,
                SpecialRequest = normalizedSpecialRequest,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _reservationRepository.AddReservationAsync(reservation);
            reservation.Guest = guest;
            reservation.Table = table;

            return MapToDto(reservation);
        });
    }

    public async Task<IReadOnlyList<ReservationResponseDto>> GetReservationsAsync(DateOnly? fromDate, DateOnly? toDate)
    {
        var reservations = await _reservationRepository.GetReservationsAsync(fromDate, toDate);
        return reservations.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<AvailableTableDto>> GetAvailableTablesAsync(GetAvailableTablesQueryDto query)
    {
        if (query.StartTime == default || query.EndTime == default)
            throw new InvalidOperationException("Start time and end time are required.");

        if (query.EndTime <= query.StartTime)
            throw new InvalidOperationException("End time must be later than start time.");

        if (query.StartTime < DateTime.UtcNow.AddMinutes(-1))
            throw new InvalidOperationException("Start time is already in the past. Ask the guest for a future time (for example the same time tomorrow).");

        if (query.PartySize <= 0)
            throw new InvalidOperationException("Party size must be greater than zero.");

        var tables = await _reservationRepository.GetAvailableTablesAsync(query.StartTime, query.EndTime, query.PartySize);
        return tables
            .Select(t => new AvailableTableDto
            {
                Id = t.Id,
                TableNumber = t.TableNumber,
                Capacity = t.Capacity,
                Zone = t.Zone
            })
            .ToList();
    }

    public async Task<IReadOnlyList<ReservationResponseDto>> GetReservationsByPhoneAsync(string phone)
    {
        var normalizedPhone = PhoneNormalizer.Normalize(phone);
        if (string.IsNullOrWhiteSpace(normalizedPhone))
            throw new InvalidOperationException("Phone number is required.");

        var reservations = await _reservationRepository.GetReservationsByPhoneAsync(normalizedPhone);
        return reservations.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ReservationResponseDto>> GetUpcomingReservationsForGuestAsync(string phone, string lastName)
    {
        var normalizedPhone = PhoneNormalizer.Normalize(phone);
        var normalizedLastName = (lastName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalizedPhone) || string.IsNullOrWhiteSpace(normalizedLastName) ||
            IsPlaceholder(normalizedLastName))
            return Array.Empty<ReservationResponseDto>();

        var reservations = await _reservationRepository.GetUpcomingReservationsByPhoneAndLastNameAsync(
            normalizedPhone, normalizedLastName, DateTime.UtcNow);

        return reservations.Select(MapToDto).ToList();
    }

    public async Task<ReservationResponseDto> CancelReservationAsync(long id, string phone)
    {
        var normalizedPhone = PhoneNormalizer.Normalize(RequireRealValue(phone, "Phone number"));

        return await _unitOfWork.InSerializableTransactionAsync(async () =>
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(id);
            if (reservation is null)
                throw new KeyNotFoundException("Reservation not found.");

            if (!string.Equals(reservation.Guest.Phone, normalizedPhone, StringComparison.Ordinal))
                throw new InvalidOperationException("Phone number does not match the reservation.");

            switch (reservation.Status)
            {
                case ReservationStatus.Cancelled:
                    return MapToDto(reservation);
                case ReservationStatus.Active:
                case ReservationStatus.Late:
                    break;
                case ReservationStatus.Seated:
                    throw new InvalidOperationException("Cannot cancel a reservation once the guests have been seated.");
                case ReservationStatus.Completed:
                    throw new InvalidOperationException("Cannot cancel a completed reservation.");
                case ReservationStatus.NoShow:
                    throw new InvalidOperationException("Cannot cancel a reservation that was already marked no-show.");
                default:
                    throw new InvalidOperationException($"Cannot cancel a reservation with status {reservation.Status}.");
            }

            reservation.Status = ReservationStatus.Cancelled;
            reservation.UpdatedAt = DateTime.UtcNow;
            await _reservationRepository.UpdateReservationAsync(reservation);

            return MapToDto(reservation);
        });
    }

    public async Task<ReservationResponseDto> UpdateReservationAsync(long id, UpdateReservationDto dto)
    {
        var normalizedPhone = PhoneNormalizer.Normalize(RequireRealValue(dto.Phone, "Phone number"));

        return await _unitOfWork.InSerializableTransactionAsync(async () =>
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(id);
            if (reservation is null)
                throw new KeyNotFoundException("Reservation not found.");

            if (!string.Equals(reservation.Guest.Phone, normalizedPhone, StringComparison.Ordinal))
                throw new InvalidOperationException("Phone number does not match the reservation.");

            if (reservation.Status != ReservationStatus.Active)
                throw new InvalidOperationException($"Cannot update a reservation with status {reservation.Status}.");

            var hasAnyChange =
                dto.StartTime.HasValue ||
                dto.EndTime.HasValue ||
                dto.PartySize.HasValue ||
                !string.IsNullOrWhiteSpace(dto.TableNumber) ||
                dto.SpecialRequest is not null;

            if (!hasAnyChange)
                throw new InvalidOperationException("No fields provided to update.");

            var newStartTime = dto.StartTime ?? reservation.StartTime;
            var newEndTime = dto.EndTime ?? reservation.EndTime;
            var newPartySize = dto.PartySize ?? reservation.PartySize;

            if (newEndTime <= newStartTime)
                throw new InvalidOperationException("End time must be later than start time.");

            var newTableId = reservation.TableId;
            var newTableForCapacityCheck = reservation.Table;

            if (!string.IsNullOrWhiteSpace(dto.TableNumber))
            {
                var trimmed = dto.TableNumber.Trim();
                var newTable = await _reservationRepository.GetTableByNumberAsync(trimmed)
                    ?? throw new KeyNotFoundException($"No table named '{trimmed}' exists.");
                newTableId = newTable.Id;
                newTableForCapacityCheck = newTable;
            }

            if (newPartySize > newTableForCapacityCheck.Capacity)
                throw new InvalidOperationException("Party size exceeds the chosen table's capacity.");

            var scheduleOrTableChanged =
                dto.StartTime.HasValue ||
                dto.EndTime.HasValue ||
                !string.IsNullOrWhiteSpace(dto.TableNumber);

            if (scheduleOrTableChanged)
            {
                var clash = await _reservationRepository.HasOverlappingReservationAsync(
                    newTableId, newStartTime, newEndTime, excludeReservationId: id);
                if (clash)
                    throw new InvalidOperationException("Selected table is already reserved for that time slot.");

                await EnsureNoWalkInClashAsync(newTableId, newStartTime, newEndTime);
            }

            return await CompleteUpdateAsync(reservation, dto, newTableId, newTableForCapacityCheck, newStartTime, newEndTime, newPartySize);
        });
    }

    private async Task<ReservationResponseDto> CompleteUpdateAsync(
        Reservations reservation,
        UpdateReservationDto dto,
        long newTableId,
        RestaurantTables newTableForCapacityCheck,
        DateTime newStartTime,
        DateTime newEndTime,
        int newPartySize)
    {

        reservation.TableId = newTableId;
        reservation.Table = newTableForCapacityCheck;
        reservation.StartTime = newStartTime;
        reservation.EndTime = newEndTime;
        reservation.ReservationDate = DateOnly.FromDateTime(newStartTime);
        reservation.PartySize = newPartySize;

        if (dto.SpecialRequest is not null)
            reservation.SpecialRequest = string.IsNullOrWhiteSpace(dto.SpecialRequest)
                ? null
                : dto.SpecialRequest.Trim();

        reservation.UpdatedAt = DateTime.UtcNow;
        await _reservationRepository.UpdateReservationAsync(reservation);

        return MapToDto(reservation);
    }

    public async Task<ReservationResponseDto> ChangeStatusAsync(long id, ReservationStatus newStatus, string? waiterUserId = null)
    {
        var (result, openOrderForTableId, openOrderForReservationId) = await _unitOfWork.InSerializableTransactionAsync<(ReservationResponseDto, long?, long?)>(async () =>
        {
            var reservation = await _reservationRepository.GetReservationByIdAsync(id);
            if (reservation is null)
                throw new KeyNotFoundException("Reservation not found.");

            if (reservation.Status == newStatus)
                return (MapToDto(reservation), (long?)null, (long?)null);

            if (!IsLegalTransition(reservation.Status, newStatus))
                throw new InvalidOperationException(
                    $"Cannot transition reservation from {reservation.Status} to {newStatus}.");

            if (newStatus == ReservationStatus.Completed &&
                await _orderService.HasOpenOrderForReservationAsync(reservation.Id))
            {
                throw new InvalidOperationException(
                    "Nije moguće završiti rezervaciju dok je račun za stol još otvoren. " +
                    "Najprije naplatite račun.");
            }

            var previousStatus = reservation.Status;
            reservation.Status = newStatus;
            reservation.UpdatedAt = DateTime.UtcNow;
            await _reservationRepository.UpdateReservationAsync(reservation);

            long? tableNeedingOrder = null;
            long? reservationIdForOrder = null;

            if (newStatus == ReservationStatus.Seated)
            {
                var table = await _tableRepository.GetByIdAsync(reservation.TableId);
                if (table is not null)
                {
                    table.Status = TableStatus.Seated;
                    table.GuestCount = Math.Min(reservation.PartySize, table.Capacity);
                    table.UpdatedAt = DateTime.UtcNow;
                    await _tableRepository.UpdateAsync(table);
                }

                tableNeedingOrder = reservation.TableId;
                reservationIdForOrder = reservation.Id;
            }
            else if (previousStatus == ReservationStatus.Seated && newStatus == ReservationStatus.Completed)
            {
                var table = await _tableRepository.GetByIdAsync(reservation.TableId);
                if (table is not null)
                {
                    table.Status = TableStatus.Available;
                    table.GuestCount = 0;
                    table.UpdatedAt = DateTime.UtcNow;
                    await _tableRepository.UpdateAsync(table);
                }
            }

            return (MapToDto(reservation), tableNeedingOrder, reservationIdForOrder);
        });

        if (openOrderForTableId.HasValue && !string.IsNullOrWhiteSpace(waiterUserId))
        {
            try
            {
                var existingOrder = await _orderService.GetOpenOrderByTableIdAsync(openOrderForTableId.Value);
                if (existingOrder is null)
                {
                    await _orderService.CreateOrderAsync(
                        new CreateOrderDto { TableId = openOrderForTableId.Value, ReservationId = openOrderForReservationId },
                        waiterUserId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[change-status] order auto-open failed for table {openOrderForTableId.Value}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task EnsureNoWalkInClashAsync(long tableId, DateTime startTime, DateTime endTime)
    {
        var walkInOpenedAt = await _orderService.GetOpenWalkInStartTimeForTableAsync(tableId);
        if (!walkInOpenedAt.HasValue) return;

        var walkInStart = DateTime.SpecifyKind(walkInOpenedAt.Value, DateTimeKind.Utc);
        var walkInEnd = walkInStart + RestaurantPolicies.WalkInDuration;

        if (walkInStart < endTime && startTime < walkInEnd)
        {
            var freeAt = RestaurantPolicies.FormatLocalTime(walkInEnd);
            throw new InvalidOperationException(
                $"Stol je trenutno zauzet walk-in gostima. Slobodan najranije u {freeAt}.");
        }
    }

    private static bool IsLegalTransition(ReservationStatus from, ReservationStatus to) => from switch
    {
        ReservationStatus.Active => to is ReservationStatus.Seated
            or ReservationStatus.NoShow
            or ReservationStatus.Cancelled
            or ReservationStatus.Late
            or ReservationStatus.Completed,
        ReservationStatus.Late => to is ReservationStatus.Seated
            or ReservationStatus.NoShow
            or ReservationStatus.Cancelled
            or ReservationStatus.Completed,
        ReservationStatus.Seated => to is ReservationStatus.Completed,
        _ => false
    };

    private static ReservationResponseDto MapToDto(Reservations reservation)
    {
        return new ReservationResponseDto
        {
            Id = reservation.Id,
            GuestId = reservation.GuestId,
            GuestFullName = $"{reservation.Guest.FirstName} {reservation.Guest.LastName}",
            GuestPhone = reservation.Guest.Phone,
            TableId = reservation.TableId,
            TableNumber = reservation.Table.TableNumber,
            ReservationDate = reservation.ReservationDate,
            StartTime = DateTime.SpecifyKind(reservation.StartTime, DateTimeKind.Utc),
            EndTime = DateTime.SpecifyKind(reservation.EndTime, DateTimeKind.Utc),
            PartySize = reservation.PartySize,
            Status = reservation.Status,
            SpecialRequest = reservation.SpecialRequest,
            ReservedByUserId = reservation.ReservedByUserId
        };
    }
}
