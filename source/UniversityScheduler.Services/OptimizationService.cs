using UniversityScheduler.Models;

namespace UniversityScheduler.Services;

public class OptimizationService(IConflictService conflictService) {
  /// <summary>
  /// Поиск аудитории без конфликтов для занятия
  /// </summary>
  /// <param name="lesson">Занятие</param>
  /// <param name="slot">Временной слот</param>
  /// <param name="schedule">Расписание</param>
  /// <param name="allRooms">Кабинеты</param>
  /// <returns>Свободная аудитория</returns>
  private Room? FindAvailableRoom(Lesson lesson, TimeSlot slot, List<ScheduledLesson> schedule, List<Room> allRooms) {
    return allRooms
      .Where(r => r.Type == lesson.RequiredRoomType)
      .FirstOrDefault(r => !conflictService.HasConflict(lesson, slot, r, schedule));
  }

  /// <summary>
  /// Оптимизация для определённой группы
  /// </summary>
  /// <param name="groupId">ID группы</param>
  /// <param name="schedule">Расписание</param>
  /// <param name="allRooms">аудитория</param>
  /// <param name="allSlots">Временные слоты</param>
  /// <returns>Было ли оптимизировано?</returns>
  private bool OptimizeForGroup(Guid groupId, List<ScheduledLesson> schedule, List<Room> allRooms, List<TimeSlot> allSlots) {
    bool anyMoveMade = false;
    var groupLessons = schedule
      .Where(s => s.Lesson.Group.ID == groupId)
      .OrderBy(s => s.Slot.Day)
      .ThenBy(s => s.Slot.StartHour)
      .ToList();

    foreach (var scheduled in groupLessons) {
      var earlierSlots = allSlots
        .Where(s => s.Day == scheduled.Slot.Day && s.StartHour < scheduled.Slot.StartHour)
        .OrderBy(s => s.StartHour)
        .ToList();

      foreach (var targetSlot in earlierSlots) {
        var bestRoom = FindAvailableRoom(scheduled.Lesson, targetSlot, schedule, allRooms);

        if (bestRoom != null) {
          scheduled.Slot = targetSlot;
          scheduled.Room = bestRoom;
          return true;
        }
      }
    }
    return anyMoveMade;
  }

  /// <summary>
  /// Оптимизирует расписание, переданное в <paramref name="schedule"/>
  /// </summary>
  /// <param name="schedule">Расписание, которое требуется оптимизировать</param>
  /// <param name="allRooms">Аудитории</param>
  /// <param name="allSlots">Временные слоты</param>
  public void Optimize(List<ScheduledLesson> schedule, List<Room> allRooms, List<TimeSlot> allSlots) {
    bool changed;
    do {
      changed = false;
      var groups = schedule.Select(s => s.Lesson.Group.ID).Distinct().ToList();

      foreach (var groupId in groups) {
        if (OptimizeForGroup(groupId, schedule, allRooms, allSlots))
          changed = true;
      }
    } while (changed);
  }
}
