using System;
using System.Globalization;
using System.Windows.Data;
using UniversityScheduler.Models;
using UniversityScheduler.Services;

namespace UniversityScheduler.UI;

public class TimeSlotConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Session session && session?.TimeSlot != null)
        {
            var timeStr = SchedulerEngine.FormatTimeOnly(session.TimeSlot, session.Duration);

            if (string.IsNullOrEmpty(timeStr) || timeStr == "—")
                return "";

            var details = new System.Text.StringBuilder();
            details.AppendLine(timeStr);

            if (!string.IsNullOrEmpty(session.Subject))
                details.AppendLine(session.Subject);

            if (session.Lector != null && !string.IsNullOrEmpty(session.Lector.Name))
                details.AppendLine(session.Lector.Name);

            if (session.Room != null && !string.IsNullOrEmpty(session.Room.Number))
                details.AppendLine($"Ауд. {session.Room.Number}");

            return details.ToString().TrimEnd();
        }

        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
