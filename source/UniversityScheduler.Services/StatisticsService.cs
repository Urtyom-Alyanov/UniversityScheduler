using UniversityScheduler.Models;

namespace UniversityScheduler.Services;

public class StatisticsService {
  /// <summary>
  /// Загруженность аудитории
  /// </summary>
  /// <param name="room">Аудитория</param>
  /// <param name="schedule">Расписание</param>
  /// <param name="totalAvailableSlots">Количество доступных временных слотов</param>
  /// <returns>Загруженность аудитории в процентах</returns>
  public double GetRoomUtilization(Room room, List<ScheduledLesson> schedule, int totalAvailableSlots) {
    var usedSlots = schedule.Count(s => s.Room.ID == room.ID);
    return (double)usedSlots / totalAvailableSlots * 100;
  }

  /// <summary>
  /// Получить загруженность каждого преподавателя
  /// </summary>
  /// <param name="schedule">Расписание</param>
  /// <returns>Загрузка каждого преподавателя по часам</returns>
  public Dictionary<string, long> GetLectorWorkload(List<ScheduledLesson> schedule) {
    return schedule
        .GroupBy(s => s.Lesson.Lector.LastNameWithAliases)
        .ToDictionary(g => g.Key, g => g.Sum(s => s.Lesson.Duration));
  }

  /// <summary>
  /// Подсчёт всех окон в расписании
  /// </summary>
  /// <param name="schedule">Расписание</param>
  /// <returns>Количество окон</returns>
  public uint CalculateTotalWindows(List<ScheduledLesson> schedule)
  {
    uint totalWindows = 0;
    var groupSchedules = schedule.GroupBy(s => new { s.Lesson.Group.ID, s.Slot.Day });

    foreach (var groupDay in groupSchedules)
    {
      var hours = groupDay.Select(s => s.Slot.StartHour).OrderBy(h => h).ToList();
      if (hours.Count > 1)
      {
        for (int i = 0; i < hours.Count - 1; i++)
        {
          totalWindows += (hours[i + 1] - hours[i] - 1);
        }
      }
    }
    return totalWindows;
  }
}
