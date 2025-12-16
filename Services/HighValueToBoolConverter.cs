using System;
using System.Globalization;
using System.Windows.Data;

namespace SystemMonitorWPF.Services
{
    public class HighValueToBoolConverter : IValueConverter
    {
        public double Threshold { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double v)
                return v >= Threshold;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
