using System.Globalization;

namespace DashboardOrders.Models;

public static class DisplayFormatter
{
    private static readonly CultureInfo ItalianCulture = CultureInfo.GetCultureInfo("it-IT");

    public static string FormatEuro(decimal value, string numericFormat = "N2")
    {
        return $"{value.ToString(numericFormat, ItalianCulture)} \u20ac";
    }

    public static string FormatNumber(int value)
    {
        return value.ToString("N0", ItalianCulture);
    }
}
