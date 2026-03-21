using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Entities;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;

    public ReservationService(IReservationRepository reservationRepository)
    {
        _reservationRepository = reservationRepository;
    }

    public async Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto, string? reservedByUserId)
    {
        if (dto.StartTime == default || dto.EndTime == default)
            throw new InvalidOperationException("Start time and end time are required.");

        if (dto.EndTime <= dto.StartTime)
            throw new InvalidOperationException("End time must be later than start time.");

        if (dto.PartySize <= 0)
            throw new InvalidOperationException("Party size must be greater than zero.");

        var firstName = (dto.FirstName ?? string.Empty).Trim();
        var lastName = (dto.LastName ?? string.Empty).Trim();
        var normalizedPhone = (dto.Phone ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new InvalidOperationException("First name and last name are required.");

        if (string.IsNullOrWhiteSpace(normalizedPhone))
            throw new InvalidOperationException("Phone number is required.");

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
