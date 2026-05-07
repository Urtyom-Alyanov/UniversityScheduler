using UniversityScheduler.Models;

namespace UniversityScheduler.Services;

public class ScheduleResult {
  public List<ScheduledLesson> ScheduledLessons { get; set; } = new();
  public List<Lesson> UnassignedLessons { get; set; } = new();
  public bool Success => UnassignedLessons.Count == 0;
}

public class ScheduleService(
    IConflictService conflictService,
    TopologicalSortService topologicalSortService
    ) {
  /// <summary>
  /// Генерация расписания
  /// </summary>
  /// <param name="lessons">Занятия</param>
  /// <param name="rooms">Кабинеты</param>
  /// <param name="availableSlots">Доступные временные слоты</param>
  /// <returns>Результат планировщика</returns>
  public ScheduleResult GenerateScheduledLessons(List<Lesson> lessons, List<Room> rooms, List<TimeSlot> availableSlots) {
    ScheduleResult result = new();
    var sortedLessons = topologicalSortService.Sort(lessons);

    foreach (var lesson in sortedLessons) {
      bool placed = false;

      var sortedSlots = availableSlots
        .OrderBy(s => s.Day)
        .ThenBy(s => s.StartHour);

      foreach (var slot in sortedSlots) {
        uint lessonEnd = (uint)slot.StartHour + lesson.Duration;
        bool fitsInDay = availableSlots.Any(s => s.Day == slot.Day && s.StartHour == lessonEnd - 1);

        if (!fitsInDay) continue;

        var suitableRoom = rooms.FirstOrDefault(r =>
            r.Type == lesson.RequiredRoomType &&
            !conflictService.HasConflict(lesson, slot, r, result.ScheduledLessons));

        if (suitableRoom != null) {
          result.ScheduledLessons.Add(new ScheduledLesson(lesson, slot, suitableRoom));
          placed = true;
          break;
        }
      }

      if (!placed) {
        result.UnassignedLessons.Add(lesson);
      }
    }

    return result;
  }
}
