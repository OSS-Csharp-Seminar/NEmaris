using NEmaris.Application.DTOs;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto, string? reservedByUserId);
    Task<IReadOnlyList<ReservationResponseDto>> GetReservationsAsync(DateOnly? fromDate, DateOnly? toDate);
    Task<IReadOnlyList<AvailableTableDto>> GetAvailableTablesAsync(GetAvailableTablesQueryDto query);
    Task<IReadOnlyList<ReservationResponseDto>> GetReservationsByPhoneAsync(string phone);
    Task<IReadOnlyList<ReservationResponseDto>> GetUpcomingReservationsForGuestAsync(string phone, string lastName);
    Task<ReservationResponseDto> CancelReservationAsync(long id, string phone);
    Task<ReservationResponseDto> UpdateReservationAsync(long id, UpdateReservationDto dto);
    Task<ReservationResponseDto> ChangeStatusAsync(long id, ReservationStatus newStatus, string? waiterUserId = null);
}
