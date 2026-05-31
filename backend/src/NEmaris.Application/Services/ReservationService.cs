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

    public ReservationService(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
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
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());

        if (digits.Length < 7)
            throw new InvalidOperationException($"Phone number '{trimmed}' is too short. Ask the guest for a real phone number.");

        if (digits.Distinct().Count() == 1)
            throw new InvalidOperationException($"Phone number '{trimmed}' looks like a placeholder. Ask the guest for a real phone number.");

        if (IsSequentialDigits(digits))
            throw new InvalidOperationException($"Phone number '{trimmed}' looks like a placeholder. Ask the guest for a real phone number.");

        return trimmed;
    }

    private static bool IsSequentialDigits(string digits)
    {
        if (digits.Length < 7) return false;
        var ascending = true;
        var descending = true;
        for (var i = 1; i < digits.Length; i++)
        {
            if (digits[i] - digits[i - 1] != 1) ascending = false;
            if (digits[i - 1] - digits[i] != 1) descending = false;
        }
        return ascending || descending;
    }

    private static void RejectPlaceholderNamePair(string firstName, string lastName)
    {
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

        if (dto.PartySize <= 0)
            throw new InvalidOperationException("Party size must be greater than zero.");

        var firstName = RequireRealName(dto.FirstName, "First name");
        var lastName = RequireRealName(dto.LastName, "Last name");
        RejectPlaceholderNamePair(firstName, lastName);
        var normalizedPhone = RequireRealPhone(dto.Phone);

        var normalizedEmail = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim();
        var normalizedNotes = string.IsNullOrWhiteSpace(dto.Notes) ? null : dto.Notes.Trim();
        var normalizedSpecialRequest = string.IsNullOrWhiteSpace(dto.SpecialRequest) ? null : dto.SpecialRequest.Trim();

        var table = await _reservationRepository.GetTableByIdAsync(dto.TableId);
        if (table is null)
            throw new KeyNotFoundException("Selected table does not exist.");

        if (dto.PartySize > table.Capacity)
            throw new InvalidOperationException("Party size exceeds selected table capacity.");

        var isOverlapping = await _reservationRepository.HasOverlappingReservationAsync(dto.TableId, dto.StartTime, dto.EndTime);
        if (isOverlapping)
            throw new InvalidOperationException("Selected table is already reserved for that time slot.");

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
        var normalizedPhone = (phone ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedPhone))
            throw new InvalidOperationException("Phone number is required.");

        var reservations = await _reservationRepository.GetReservationsByPhoneAsync(normalizedPhone);
        return reservations.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<ReservationResponseDto>> GetUpcomingReservationsForGuestAsync(string phone, string lastName)
    {
        var normalizedPhone = (phone ?? string.Empty).Trim();
        var normalizedLastName = (lastName ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(normalizedPhone) || string.IsNullOrWhiteSpace(normalizedLastName) ||
            IsPlaceholder(normalizedPhone) || IsPlaceholder(normalizedLastName))
            return Array.Empty<ReservationResponseDto>();

        var reservations = await _reservationRepository.GetUpcomingReservationsByPhoneAndLastNameAsync(
            normalizedPhone, normalizedLastName, DateTime.UtcNow);

        return reservations.Select(MapToDto).ToList();
    }

    public async Task<ReservationResponseDto> CancelReservationAsync(long id, string phone)
    {
        var normalizedPhone = RequireRealValue(phone, "Phone number");

        var reservation = await _reservationRepository.GetReservationByIdAsync(id);
        if (reservation is null)
            throw new KeyNotFoundException("Reservation not found.");

        if (!string.Equals(reservation.Guest.Phone, normalizedPhone, StringComparison.Ordinal))
            throw new InvalidOperationException("Phone number does not match the reservation.");

        if (reservation.Status == ReservationStatus.Cancelled)
            return MapToDto(reservation);

        if (reservation.Status == ReservationStatus.Completed)
            throw new InvalidOperationException("Cannot cancel a completed reservation.");

        reservation.Status = ReservationStatus.Cancelled;
        reservation.UpdatedAt = DateTime.UtcNow;
        await _reservationRepository.UpdateReservationAsync(reservation);

        return MapToDto(reservation);
    }

    public async Task<ReservationResponseDto> UpdateReservationAsync(long id, UpdateReservationDto dto)
    {
        var normalizedPhone = RequireRealValue(dto.Phone, "Phone number");

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
        }

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
            StartTime = reservation.StartTime,
            EndTime = reservation.EndTime,
            PartySize = reservation.PartySize,
            Status = reservation.Status,
            SpecialRequest = reservation.SpecialRequest,
            ReservedByUserId = reservation.ReservedByUserId
        };
    }
}
