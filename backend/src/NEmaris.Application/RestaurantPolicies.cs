namespace NEmaris.Application;

public static class RestaurantPolicies
{
    public static readonly TimeSpan WalkInDuration = TimeSpan.FromMinutes(90);

    public static readonly TimeSpan LiveStatusWindow = TimeSpan.FromHours(2);

    private static readonly TimeZoneInfo DisplayTimeZone = ResolveDisplayTimeZone();

    public static string FormatLocalTime(DateTime utc)
    {
        var asUtc = utc.Kind == DateTimeKind.Utc
            ? utc
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        var local = TimeZoneInfo.ConvertTimeFromUtc(asUtc, DisplayTimeZone);
        return local.ToString("HH:mm");
    }

    private static TimeZoneInfo ResolveDisplayTimeZone()
    {
        foreach (var id in new[] { "Europe/Zagreb", "Central European Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }
        return TimeZoneInfo.Utc;
    }
}
