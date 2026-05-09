using System;
using Microsoft.UI.Xaml.Data;

namespace BankApp.Client.Utilities
{
    public class DecimalToPercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            string.Format("{0:P2}", value ?? 0m);

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}