using System;
using System.Globalization;
using System.Windows.Data;
using UniversityScheduler.Services;

namespace UniversityScheduler.UI;

public class TimeSlotConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && values[0] is int slot && values[1] is int duration)
        {
            return SchedulerEngine.FormatTimeSlot(slot, duration);
        }
        if (values.Length >= 1 && values[0] is int slotOnly)
        {
            return SchedulerEngine.FormatTimeSlot(slotOnly, 1);
        }
        return "—";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}