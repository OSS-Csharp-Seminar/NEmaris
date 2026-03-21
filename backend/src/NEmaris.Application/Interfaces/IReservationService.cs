using NEmaris.Application.DTOs;

namespace NEmaris.Application.Interfaces;

public interface IReservationService
{
    Task<ReservationResponseDto> CreateReservationAsync(CreateReservationDto dto, string? reservedByUserId);
    Task<IReadOnlyList<ReservationResponseDto>> GetReservationsAsync(DateOnly? fromDate, DateOnly? toDate);
    Task<IReadOnlyList<AvailableTableDto>> GetAvailableTablesAsync(GetAvailableTablesQueryDto query);
}
