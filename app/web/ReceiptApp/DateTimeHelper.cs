using System.Globalization;

namespace ReceiptApp;

public static class DateTimeHelper
{
    public const string DateFormat = "yyyy-MM-dd";

    public static string GetToday() => DateTime.UtcNow.ToString(DateFormat);
    public static string DateToStringy(DateTime day) => day.ToString(DateFormat);

    public static DateTime Parse(string day)
    {
        CultureInfo provider = CultureInfo.InvariantCulture;
        return DateTime.ParseExact(day, DateFormat, provider);
    }
}