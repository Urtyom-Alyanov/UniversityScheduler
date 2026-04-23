using UniversityScheduler.Models;

namespace UniversityScheduler.Services;

/// <summary>
/// Движок планировщика расписания
/// </summary>
public class SchedulerEngine
{
    private List<Session> _sessions;
    private List<Room> _rooms;
    
    /// <summary>
    /// Занятия
    /// </summary>
    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();
    /// <summary>
    /// Кабинеты
    /// </summary>
    public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();
    
    private int _slotsPerDay = 6;
    private int _daysPerWeek = 5;
    
    public SchedulerEngine(List<Session> sessions, List<Room> rooms)
    {
        _sessions = sessions;
        _rooms = rooms;
    }
    
    /// <summary>
    /// Проверка на конфликт меж двумя занятиями, то есть:
    /// 
    /// 1. Один преподаватель на две группы в одно время
    /// 2. Одна группа на два преподавателя
    /// </summary>
    /// <param name="sessionFirst">Первое занятие для сравнения</param>
    /// <param name="sessionSecond">Второе занятие</param>
    /// <returns>Конфликтуют ли?</returns>
    private bool AreConflicting(Session sessionFirst, Session sessionSecond)
    {
        if (sessionFirst.Id == sessionSecond.Id) return false;
        
        return sessionFirst.Lector.Id == sessionSecond.Lector.Id || 
               sessionFirst.Group.Id == sessionSecond.Group.Id;
    }

    /// <summary>
    /// Сколько у занятия конфликтов
    /// </summary>
    /// <param name="session">занятие</param>
    /// <returns>Количество конфликтов</returns>
    private int GetDegree(Session session) => _sessions.Count(other => AreConflicting(session, other));
    
    /// <summary>
    /// Может ли занятие занять слот в определённое время
    /// </summary>
    /// <param name="session">занятие</param>
    /// <param name="slot">слот</param>
    /// <returns>Может ли занять</returns>
    private bool CanAssign(Session session, int slot)
    {
        return !_sessions.Any(s => s.TimeSlot == slot && AreConflicting(session, s));
    }

    /// <summary>
    /// Свободная ли комната в это время?
    /// </summary>
    /// <param name="room">комната</param>
    /// <param name="slot">слот</param>
    /// <returns>Может ли занять</returns>
    private bool IsRoomFree(Room room, int slot)
    {
        return !_sessions.Any(s => s.TimeSlot == slot && s.Room?.Id == room.Id);
    }
    
    /// <summary>
    /// Генерирует расписание
    /// </summary>
    public void GenerateSchedule()
    {
        var sortedSessions = _sessions
            .OrderByDescending(s => GetDegree(s))
            .ToList();

        foreach (var session in sortedSessions)
        {
            int color = 0;
            bool assigned = false;

            while (!assigned)
            {
                if (CanAssign(session, color))
                {
                    var availableRoom = _rooms.FirstOrDefault(r => 
                        r.Type == session.RequiredType && 
                        IsRoomFree(r, color));

                    if (availableRoom != null)
                    {
                        session.TimeSlot = color;
                        session.Room = availableRoom;
                        assigned = true;
                    }
                }
                color++;
                if (color > 100) break;
            }
        }
    }
}