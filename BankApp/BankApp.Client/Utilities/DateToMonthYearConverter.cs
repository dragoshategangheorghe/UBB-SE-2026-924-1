namespace BankApp.Client.Utilities;

using System;
using Microsoft.UI.Xaml.Data;

/// <summary>
/// Converts a <see cref="DateTime"/> object to a formatted month and year string.
/// </summary>
public class DateToMonthYearConverter : IValueConverter
{
    private const string MonthYearFormat = "MMM ''yy";

    /// <summary>
    /// Converts a date value to a short month and year string format.
    /// </summary>
    /// <param name="value">The date value to convert.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">The converter parameter to use.</param>
    /// <param name="language">The language of the conversion.</param>
    /// <returns>A formatted string representing the month and year, or an empty string if the value is invalid.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime date)
        {
            return date.ToString(MonthYearFormat);
        }

        return string.Empty;
    }

    /// <summary>
    /// Converts a formatted date string back to a <see cref="DateTime"/> object. This method is not implemented.
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