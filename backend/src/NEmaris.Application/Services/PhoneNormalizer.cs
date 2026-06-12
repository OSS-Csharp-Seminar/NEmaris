using System.Text;

namespace NEmaris.Application.Services;

internal static class PhoneNormalizer
{
    public static string Normalize(string? raw)
    {
        if (string.IsNullOrEmpty(raw)) return string.Empty;

        var sb = new StringBuilder(raw.Length);
        foreach (var ch in raw)
        {
            if (ch == '+' && sb.Length == 0)
                sb.Append('+');
            else if (char.IsDigit(ch))
                sb.Append(ch);
        }
        return sb.ToString();
    }
}
