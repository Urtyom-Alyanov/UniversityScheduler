namespace UniversityScheduler.Models;

/// <summary>
/// Временной слот
/// </summary>
/// <param name="Day">День недели</param>
/// <param name="StartHour">Начало пары</param>
public record TimeSlot(
    DayOfWeek Day,
    uint StartHour
);
