using System.Globalization;
using System.Windows.Data;

namespace UniversityScheduler.UI.Converters;

public class DayToColumnConverter : IValueConverter {
   public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
     if (value is DayOfWeek day) {
       return (int)day - 1;
     }
     return 0;
   }
   public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => Binding.DoNothing;
}
