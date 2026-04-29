namespace UniversityScheduler.Models;

/// <summary>
/// Занятие, которое должно состояться
/// </summary>
/// <param name="subject">Дисциплина</param>
/// <param name="group">Группа</param>
/// <param name="lector">Преподаватель</param>
/// <param name="duration">Требуемая длительность занятия</param>
/// <param name="requiredRoomType">Тип занятия (требуемая аудитория)</param>
public class Lesson(
    Subject subject,
    Group group,
    Lector lector,
    uint duration,
    RoomType requiredRoomType)
{
    public Subject Subject => subject;
    public Group Group => group;
    public Lector Lector => lector;
    public RoomType RequiredRoomType => requiredRoomType;
    
    public Guid ID { get; set; }
    public uint Duration => duration;
    
    public List<Guid> Prerequisites { get; set; } = new();
}


/// <summary>
/// Запланированное занятие на время и кабинет
/// </summary>
/// <param name="lesson">Занятие</param>
/// <param name="timeSlot">Временной слот</param>
/// <param name="room">Выданный кабинет</param>
public class ScheduledLesson(
    Lesson lesson,
    TimeSlot timeSlot,
    Room room
)
{
    public Lesson Lesson => lesson;
    public TimeSlot Slot => timeSlot;
    public Room Room => room;
    
    public uint EndTime => timeSlot.StartHour + lesson.Duration;
}