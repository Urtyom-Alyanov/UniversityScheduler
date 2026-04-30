using System.Globalization;
using System.Windows.Data;

namespace UniversityScheduler.UI.Converters;

public class ViewModeToVisibilityConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    if (value != null && parameter != null && value.ToString() == parameter.ToString())
      return System.Windows.Visibility.Visible;
    return System.Windows.Visibility.Collapsed;
  }
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}
