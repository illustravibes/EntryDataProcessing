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
            if (value is string str)
            {
                var s = str.Trim().ToLowerInvariant();
                if (s == "approve" || s == "approved" || s == "disetujui")
                    return new SolidColorBrush(Color.FromRgb(0x15, 0x80, 0x3D)); // Modern Green
                if (s == "reject" || s == "rejected" || s == "ditolak")
                    return new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C)); // Modern Red
                if (s == "draft")
                    return new SolidColorBrush(Color.FromRgb(0x43, 0x38, 0xCA)); // Modern Indigo
                return new SolidColorBrush(Color.FromRgb(0xB4, 0x53, 0x09)); // Modern Amber / Pending
            }
            if (value is int status)
            {
                return status switch
                {
                    1 => new SolidColorBrush(Color.FromRgb(0x15, 0x80, 0x3D)), // Approved
                    2 => new SolidColorBrush(Color.FromRgb(0xB9, 0x1C, 0x1C)), // Rejected
                    3 => new SolidColorBrush(Color.FromRgb(0x43, 0x38, 0xCA)), // Draft
                    _ => new SolidColorBrush(Color.FromRgb(0xB4, 0x53, 0x09))  // Pending
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
