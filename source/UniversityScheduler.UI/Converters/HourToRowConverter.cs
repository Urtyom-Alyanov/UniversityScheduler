using System.Globalization;
using System.Windows.Data;

namespace UniversityScheduler.UI.Converters;

public class HourToRowConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (value is uint hour) {
      return (int)hour - 7;
    }
    return 0;
  }
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
