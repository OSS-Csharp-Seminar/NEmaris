namespace NEmaris.Application.Services;

public interface IRequestTimeZoneContext
{
    TimeZoneInfo TimeZone { get; }
    void Set(string? ianaOrWindowsId);
}

public class RequestTimeZoneContext : IRequestTimeZoneContext
{
    public TimeZoneInfo TimeZone { get; private set; } = TimeZoneInfo.Utc;

    public void Set(string? ianaOrWindowsId)
    {
        if (string.IsNullOrWhiteSpace(ianaOrWindowsId))
        {
            TimeZone = TimeZoneInfo.Utc;
            return;
        }

        try
        {
            TimeZone = TimeZoneInfo.FindSystemTimeZoneById(ianaOrWindowsId);
        }
        catch (TimeZoneNotFoundException)
        {
            TimeZone = TimeZoneInfo.Utc;
        }
        catch (InvalidTimeZoneException)
        {
            TimeZone = TimeZoneInfo.Utc;
        }
    }
}
