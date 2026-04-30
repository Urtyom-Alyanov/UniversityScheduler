using System;
using System.Globalization;
using System.Windows.Data;
using UniversityScheduler.Models;

namespace UniversityScheduler.UI.Converters
{
    public class EntityToNameConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Group g) return g.Name;
            if (value is Lector l) return l.FullName;
            if (value is Room r) return r.FullNumber;
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
