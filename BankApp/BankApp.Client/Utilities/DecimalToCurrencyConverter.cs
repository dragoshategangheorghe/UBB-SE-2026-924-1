namespace BankApp.Client.Utilities;

using System;
using Microsoft.UI.Xaml.Data;
using System.Globalization;

/// <summary>
/// Converts a decimal value to a localized currency formatted string.
/// </summary>
public class DecimalToCurrencyConverter : IValueConverter
{
    /// <summary>
    /// Converts a decimal value to a currency string.
    /// </summary>
    /// <param name="value">The decimal value to convert.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="language">The language of the conversion.</param>
    /// <returns>A formatted currency string, or an empty string if the value is invalid.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal d)
        {
            return d.ToString("C2");
        }

        return string.Empty;
    }

    /// <summary>
    /// Converts a currency string back to a decimal. This method is not implemented.
    /// </summary>
    /// <param name="value">The value that is produced by the binding target.</param>
    /// <param name="targetType">The type to convert to.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="language">The language of the conversion.</param>
    /// <returns>The converted value.</returns>
    /// <exception cref="NotImplementedException">Always thrown as two-way binding is not supported.</exception>
    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}