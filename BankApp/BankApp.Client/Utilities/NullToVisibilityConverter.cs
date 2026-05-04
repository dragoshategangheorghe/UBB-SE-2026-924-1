namespace BankApp.Client.Utilities;

using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

/// <summary>
/// Converts a null value to <see cref="Visibility.Collapsed"/> and a non-null value to <see cref="Visibility.Visible"/>.
/// Pass parameter "Invert" to reverse this logic.
/// </summary>
public class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a null or non-null value to a <see cref="Visibility"/> state.
    /// </summary>
    /// <param name="value">The value to check for null.</param>
    /// <param name="targetType">The type of the binding target property.</param>
    /// <param name="parameter">Optional parameter to invert the logic.</param>
    /// <param name="language">The language of the conversion.</param>
    /// <returns>A <see cref="Visibility"/> value based on the null state of the bound value.</returns>
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isNull = value == null;
        var invert = parameter?.ToString() == "Invert";
        return isNull == invert ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>
    /// Converts a <see cref="Visibility"/> back to an object. This method is not implemented.
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