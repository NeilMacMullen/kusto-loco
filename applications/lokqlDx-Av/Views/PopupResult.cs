using NotNullStrings;

namespace LokqlDx.Views;

internal readonly record struct PopupResult(string Series, double X, double Y, double V, bool InvertAxes)
{
    internal static readonly PopupResult None = new(string.Empty, 0, 0, 0, false);
    public bool IsValid => Series.IsNotBlank();
}
