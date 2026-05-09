using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace BankApp.Client.Utilities
{
    public class DecimalToGainLossBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            decimal val = (decimal)(value ?? 0m);
            return val >= 0
                ? new SolidColorBrush(Windows.UI.Color.FromArgb(255, 78, 205, 196)) // Teal/Green
                : new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 107, 107)); // Red
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }
}