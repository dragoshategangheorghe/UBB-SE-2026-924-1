namespace BankApp.Client.Utilities;

using System;
using Microsoft.UI.Xaml.Data;

public class DateToOpenedLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is DateTime date)
        {
            return $"Opened {date:dd MMM yyyy}";
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
