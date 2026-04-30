using System;
using System.Globalization;
using System.Windows.Data;
using UniversityScheduler.UI.ViewModels;

namespace UniversityScheduler.UI.Converters;

public class ViewModeToVisibilityConverter : IValueConverter {
  public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
    return value?.ToString() == parameter?.ToString();
  }
  public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
    if (value is bool b && b && parameter != null) {
      return Enum.Parse(typeof(ViewMode), parameter.ToString()!);
    }
    return Binding.DoNothing;
  }
}
