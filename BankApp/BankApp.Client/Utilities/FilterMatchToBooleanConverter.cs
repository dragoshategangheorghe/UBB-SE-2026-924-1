using Microsoft.UI.Xaml.Data;
using System;

namespace BankApp.Client.Utilities
{
    public class FilterMatchToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            value?.ToString() == parameter?.ToString();

        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            value is bool b && b ? parameter : null;
    }
}