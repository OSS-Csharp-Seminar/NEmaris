using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.Services.ChatTools;

internal static class ReservationLookup
{
    public static async Task<ReservationResponseDto> ResolveActiveByPhoneAndStartAsync(
        IReservationService reservationService,
        string phone,
        DateTime startTime)
    {
        var reservations = await reservationService.GetReservationsByPhoneAsync(phone);

        var matches = reservations
            .Where(r => r.Status == ReservationStatus.Active && r.StartTime == startTime)
            .ToList();

        if (matches.Count == 0)
            throw new KeyNotFoundException(
                $"No active reservation found for that phone at {startTime:yyyy-MM-ddTHH:mm:ss}.");

        if (matches.Count > 1)
            throw new InvalidOperationException(
                "Multiple reservations match that phone and start time. Ask the guest for more detail.");

        return matches[0];
    }
}
