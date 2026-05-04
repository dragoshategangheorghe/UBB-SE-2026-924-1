using Microsoft.UI.Xaml.Data;
using System;
using System.Globalization;

namespace BankApp.Client.Utilities
{
    public class DecimalToCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            string.Format(CultureInfo.CurrentCulture, "{0:C}", value ?? 0m);

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}