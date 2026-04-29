using UniversityScheduler.Models;

namespace UniversityScheduler.Services;

public enum ConflictType
{
    Lector,
    Group,
    Room,
    RoomType
}

public record ConflictMessage(
    ConflictType conflictType,
    string entityId,
    string entityName);

public interface IConflictService
{
    /// <summary>
    /// Проверка на конфликт с существующими планами 
    /// </summary>
    /// <param name="lesson">Занятие</param>
    /// <param name="slot">Временной слот</param>
    /// <param name="room">Кабинет</param>
    /// <param name="existingSchedule">Запланированные занятия</param>
    /// <returns>Есть ли конфликты?</returns>
    bool HasConflict(Lesson lesson, TimeSlot slot, Room room, IEnumerable<ScheduledLesson> existingSchedule);
    
    /// <summary>
    /// Причины конфликтов
    /// </summary>
    /// <param name="lesson">Занятие</param>
    /// <param name="slot">Временной слот</param>
    /// <param name="room">Кабинет</param>
    /// <param name="existingSchedule">Запланированные занятия</param>
    /// <returns>Какие конфликты?</returns>
    IEnumerable<ConflictMessage> GetConflictMessages(Lesson lesson, TimeSlot slot, Room room, IEnumerable<ScheduledLesson> existingSchedule);
}

public class ConflictService : IConflictService
{
    public bool HasConflict(Lesson lesson, TimeSlot slot, Room room, IEnumerable<ScheduledLesson> existingSchedule)
    {
        return existingSchedule.Any(s => 
            s.Slot.Day == slot.Day && s.Slot.StartHour == slot.StartHour &&
            (s.Lesson.Lector.ID == lesson.Lector.ID || 
             s.Lesson.Group.ID == lesson.Group.ID || 
             s.Room.ID == room.ID));
    }

    public IEnumerable<ConflictMessage> GetConflictMessages(Lesson lesson, TimeSlot slot, Room room, IEnumerable<ScheduledLesson> existingSchedule)
    {
        var conflicts = existingSchedule.Where(s => s.Slot.Day == slot.Day && s.Slot.StartHour == slot.StartHour);
        
        if (conflicts.Any(s => s.Lesson.Lector.ID == lesson.Lector.ID))
            yield return new ConflictMessage(ConflictType.Lector, lesson.Lector.ID.ToString(), lesson.Lector.FullName);
        
        if (conflicts.Any(s => s.Lesson.Group.ID == lesson.Group.ID))
            yield return new ConflictMessage(ConflictType.Group, lesson.Group.ID.ToString(), lesson.Group.Name);
        
        if (conflicts.Any(s => s.Room.ID == room.ID))
            yield return new ConflictMessage(ConflictType.Room, room.ID.ToString(), room.FullNumber);
        
        if (room.Type != lesson.RequiredRoomType)
            yield return new ConflictMessage(ConflictType.RoomType, room.Type.ToString(), room.FullNumber);
    }
}