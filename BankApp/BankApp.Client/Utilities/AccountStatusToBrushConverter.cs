namespace BankApp.Client.Utilities;

using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;

/// <summary>
/// Converts an account status string to a corresponding <see cref="SolidColorBrush"/> for UI styling.
/// </summary>
public class AccountStatusToBrushConverter : IValueConverter
{
    private const byte FullAlpha = 255;

    private static readonly Color ActiveColor = Color.FromArgb(FullAlpha, 29, 185, 84);
    private static readonly Color ClosedColor = Color.FromArgb(FullAlpha, 229, 57, 53);
    private static readonly Color MaturedColor = Color.FromArgb(FullAlpha, 30, 136, 229);

    private static readonly SolidColorBrush ActiveBrush = new(ActiveColor);
    private static readonly SolidColorBrush ClosedBrush = new(ClosedColor);
    private static readonly SolidColorBrush MaturedBrush = new(MaturedColor);
    private static readonly SolidColorBrush DefaultBrush = new(Colors.Gray);

    /// <summary>
    /// Converts an account status value to a <see cref="SolidColorBrush"/>.
    /// </summary>
    /// <param name="value">The account status string (e.g., "Active", "Closed", "Matured").</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="language">The language of the conversion.</param>
    /// <returns>A <see cref="SolidColorBrush"/> corresponding to the account status.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value?.ToString() switch
        {
            "Active" => ActiveBrush,
            "Closed" => ClosedBrush,
            "Matured" => MaturedBrush,
            _ => DefaultBrush,
        };
    }

    /// <summary>
    /// Converts a <see cref="SolidColorBrush"/> back to an account status. This method is not implemented.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="language">The language of the conversion.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotImplementedException">Always thrown as two-way binding is not supported for this converter.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}