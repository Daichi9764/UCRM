using System.Globalization;
using System.Windows.Data;

namespace KNXBoostDesktop
{
    /// <summary>
    /// Converts a width value to a boolean indicating whether it is below a specified threshold.
    /// <para>
    /// The <see cref="Convert"/> method checks if the provided width value is less than the defined threshold.
    /// If the width is below the threshold, it returns <c>true</c>; otherwise, it returns <c>false</c>.
    /// The threshold can be customized via the <see cref="Threshold"/> property, which defaults to 300.
    /// </para>
    /// </summary>
    public class WidthBelowThresholdConverter : IValueConverter
    {
        /// <summary>
        /// Gets or sets the threshold value for comparison. Defaults to 300.
        /// </summary>
        public double Threshold { get; set; } = 300;

        /// <summary>
        /// Converts a width value to a boolean indicating whether it is below the threshold.
        /// </summary>
        /// <param name="value">The width value to convert.</param>
        /// <param name="targetType">The type of the binding target property. This parameter is ignored in this implementation.</param>
        /// <param name="parameter">Additional parameters for the conversion. This parameter is ignored in this implementation.</param>
        /// <param name="culture">The culture to use in the conversion. This parameter is ignored in this implementation.</param>
        /// <returns><c>true</c> if the width is below the threshold; otherwise, <c>false</c>.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
            {
                return App.DisplayElements?.SettingsWindow != null && width < (Threshold*App.DisplayElements.SettingsWindow.AppScaleFactor/100f);
            }
            return false;
        }
        
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}