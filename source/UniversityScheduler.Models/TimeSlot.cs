namespace UniversityScheduler.Models;

/// <summary>
/// Временной слот
/// </summary>
/// <param name="Day">День недели</param>
/// <param name="StartHour">Начальная пара (или просто какая пара)</param>
public record TimeSlot(
    DayOfWeek Day,
    uint StartHour
);
