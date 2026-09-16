using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Entry_Data_Processing.Shared.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                return status switch
                {
                    1 => new SolidColorBrush(Colors.Green), // Approved
                    2 => new SolidColorBrush(Colors.Red),   // Rejected
                    _ => new SolidColorBrush(Colors.Orange) // Pending
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
