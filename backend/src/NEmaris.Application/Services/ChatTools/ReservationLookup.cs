using NEmaris.Application.DTOs;
using NEmaris.Application.Interfaces;
using NEmaris.Domain.Enums;

namespace NEmaris.Application.Services.ChatTools;

internal static class ReservationLookup
{
    public static async Task<ReservationResponseDto> ResolveByPhoneAndStartAsync(
        IReservationService reservationService,
        string phone,
        DateTime startTime)
    {
        var reservations = await reservationService.GetReservationsByPhoneAsync(phone);

        var live = reservations
            .Where(r =>
                r.Status == ReservationStatus.Active ||
                r.Status == ReservationStatus.Late ||
                r.Status == ReservationStatus.Seated)
            .ToList();

        var targetMinute = TruncateToMinute(startTime);
        var exactMatches = live
            .Where(r => TruncateToMinute(r.StartTime) == targetMinute)
            .ToList();

        if (exactMatches.Count == 1)
            return exactMatches[0];

        if (exactMatches.Count > 1)
            throw new InvalidOperationException(
                "Multiple reservations match that phone and start time. Ask the guest for more detail.");

        var now = DateTime.UtcNow;
        var upcoming = live
            .Where(r => r.EndTime > now)
            .OrderBy(r => r.StartTime)
            .ToList();

        if (upcoming.Count == 1)
            return upcoming[0];

        if (upcoming.Count == 0)
            throw new KeyNotFoundException(
                $"No active reservation found for that phone around {startTime:yyyy-MM-ddTHH:mm}.");

        var options = string.Join("; ", upcoming.Select(r =>
            $"table {r.TableNumber} at {r.StartTime:yyyy-MM-ddTHH:mm}Z"));
        throw new InvalidOperationException(
            "Multiple upcoming reservations exist for that phone and no single one matches the given start time. " +
            $"Ask the guest which one they mean. Options: {options}.");
    }

    private static DateTime TruncateToMinute(DateTime value) =>
        new(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0, value.Kind);
}
