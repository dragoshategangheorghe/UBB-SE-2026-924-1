using Microsoft.UI.Xaml.Data;
using System;

namespace BankApp.Client.Utilities
{
    public class DecimalToTrendSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            decimal val = (decimal)(value ?? 0m);
            return val >= 0 ? "▲" : "▼";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}