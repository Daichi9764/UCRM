using System;
using System.Globalization;
using System.Windows.Data;

namespace KNXBoostDesktop
{
    public class WidthBelowThresholdConverter : IValueConverter
    {
        public double Threshold { get; set; } = 300;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return width < Threshold;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}