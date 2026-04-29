using UniversityScheduler.Models;
using System.Linq;

namespace UniversityScheduler.Services;

public class SchedulerEngine
{
    private List<Session> _sessions;
    private List<Room> _rooms;
    
    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();
    public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();
    
private const int SlotsPerDay = 6;
    private const int DaysPerWeek = 5;
    private const int MaxSlots = SlotsPerDay * DaysPerWeek;
    
    public SchedulerEngine(List<Session> sessions, List<Room> rooms)
    {
        _sessions = sessions;
        _rooms = rooms;
    }
    
    public static (int Day, int SlotInDay) ConvertSlotToDayAndSlot(int slot)
    {
        if (slot <= 0) return (0, 0);
        int day = (slot - 1) / SlotsPerDay + 1;
        int slotInDay = (slot - 1) % SlotsPerDay + 1;
        return (day, slotInDay);
    }
    
    public static string FormatTimeSlot(int? slot, int duration = 1)
    {
        if (!slot.HasValue || slot.Value <= 0) return "—";
        var (day, slotInDay) = ConvertSlotToDayAndSlot(slot.Value);
        var dayNames = new[] { "", "Пн", "Вт", "Ср", "Чт", "Пт" };
        
        var startTimes = new[] { 0, 8*60+30, 10*60+15, 12*60+15, 14*60, 15*60+45, 17*60+30 };
        var endTime = startTimes[slotInDay] + duration * 90;
        var endHour = endTime / 60;
        var endMinute = endTime % 60;
        
        var startTime = startTimes[slotInDay];
        var startHour = startTime / 60;
        var startMinute = startTime % 60;
        
        return $"{dayNames[day]}, {startHour}:{startMinute:D2}-{endHour:D2}:{endMinute:D2}";
    }

    public static string FormatTimeOnly(int? slot, int duration = 1)
    {
        if (!slot.HasValue || slot.Value <= 0) return "—";
        var (day, slotInDay) = ConvertSlotToDayAndSlot(slot.Value);

        var startTimes = new[] { 0, 8*60+30, 10*60+15, 12*60+15, 14*60, 15*60+45, 17*60+30 };
        var endTime = startTimes[slotInDay] + duration * 90;
        var endHour = endTime / 60;
        var endMinute = endTime % 60;

        var startTime = startTimes[slotInDay];
        var startHour = startTime / 60;
        var startMinute = startTime % 60;

        return $"{startHour}:{startMinute:D2}-{endHour:D2}:{endMinute:D2}";
    }
    
    private bool AreConflicting(Session s1, Session s2)
    {
        if (s1.Id == s2.Id) return false;
        if (s1.Lector == null || s2.Lector == null || s1.Group == null || s2.Group == null) return false;

        // Базовый конфликт: один преподаватель или одна группа
        return s1.Lector.Id == s2.Lector.Id || s1.Group.Id == s2.Group.Id;
    }

    private bool SessionsOverlap(Session s1, Session s2)
    {
        if (!s1.TimeSlot.HasValue || !s2.TimeSlot.HasValue) return false;

        int start1 = s1.TimeSlot.Value;
        int end1 = start1 + s1.Duration - 1;

        int start2 = s2.TimeSlot.Value;
        int end2 = start2 + s2.Duration - 1;

        // Проверка пересечения интервалов в пределах одного дня
        var day1 = (start1 - 1) / SlotsPerDay;
        var day2 = (start2 - 1) / SlotsPerDay;

        if (day1 != day2) return false;

        return Math.Max(start1, start2) <= Math.Min(end1, end2);
    }

    public int CountGapsForGroups()
    {
        var gaps = 0;
        var groups = _sessions.Select(s => s.Group?.Id).Where(id => id.HasValue).Select(id => id.Value).Distinct().ToList();

        foreach (var groupId in groups)
        {
            var groupSessions = _sessions
                .Where(s => s.Group?.Id == groupId && s.TimeSlot.HasValue)
                .OrderBy(s => s.TimeSlot)
                .Select(s => s.TimeSlot!.Value)
                .ToList();

            for (int i = 1; i < groupSessions.Count; i++)
            {
                if (groupSessions[i] - groupSessions[i - 1] > 1)
                    gaps++;
            }
        }

        return gaps;
    }

    private bool CanAssign(Session session, int slot, Room room)
    {
        if (slot <= 0 || slot + session.Duration - 1 > MaxSlots) return false;
        
        // Проверка, что занятие не выходит за границы дня
        int dayStart = (slot - 1) / SlotsPerDay;
        int dayEnd = (slot + session.Duration - 2) / SlotsPerDay;
        if (dayStart != dayEnd) return false;

        // Проверка типа аудитории
        if (room.Type != session.RequiredType) return false;

        foreach (var other in _sessions)
        {
            if (other.Id == session.Id || !other.TimeSlot.HasValue) continue;

            int otherStart = other.TimeSlot.Value;
            int otherEnd = otherStart + other.Duration - 1;
            int currentEnd = slot + session.Duration - 1;

            // Если в это же время
            if (Math.Max(slot, otherStart) <= Math.Min(currentEnd, otherEnd))
            {
                // Конфликт ресурсов
                if (AreConflicting(session, other)) return false;
                // Конфликт аудитории
                if (other.Room?.Id == room.Id) return false;
            }
        }
        return true;
    }

    public (bool IsDag, List<Session> SortedSessions) TopologicalSort()
    {
        var graph = new Dictionary<int, List<int>>();
        var inDegree = _sessions.ToDictionary(s => s.Id, _ => 0);

        foreach (var s in _sessions)
            graph[s.Id] = new List<int>();

        foreach (var s in _sessions)
        {
            if (s.PrerequisiteSessionId.HasValue && graph.ContainsKey(s.PrerequisiteSessionId.Value))
            {
                graph[s.PrerequisiteSessionId.Value].Add(s.Id);
                inDegree[s.Id]++;
            }
        }

        var queue = new Queue<int>(_sessions.Where(s => inDegree[s.Id] == 0).Select(s => s.Id));
        var sorted = new List<Session>();

        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            var session = _sessions.First(s => s.Id == id);
            sorted.Add(session);

            foreach (var neighborId in graph[id])
            {
                inDegree[neighborId]--;
                if (inDegree[neighborId] == 0)
                    queue.Enqueue(neighborId);
            }
        }

        bool isDag = sorted.Count == _sessions.Count;
        if (!isDag)
        {
            // Если есть циклы, возвращаем все сессии в исходном порядке
            return (false, _sessions.ToList());
        }

        return (true, sorted);
    }

    public void GenerateSchedule()
    {
        // 1. Очистка текущего расписания
        foreach (var s in _sessions)
        {
            s.TimeSlot = null;
            s.Room = null;
        }

        // 2. Топологическая сортировка
        var (isDag, orderedSessions) = TopologicalSort();

        // 3. Вычисляем уровень (глубину) для каждой сессии в DAG
        var sessionLevel = new Dictionary<int, int>();
        foreach (var s in orderedSessions)
        {
            if (s.PrerequisiteSessionId.HasValue && sessionLevel.ContainsKey(s.PrerequisiteSessionId.Value))
            {
                sessionLevel[s.Id] = sessionLevel[s.PrerequisiteSessionId.Value] + 1;
            }
            else
            {
                sessionLevel[s.Id] = 0;
            }
        }

        // 4. Сортируем: сначала по уровню (топологический порядок), затем по количеству конфликтов
        var sortedSessions = orderedSessions
            .OrderBy(s => sessionLevel[s.Id])
            .ThenByDescending(s => _sessions.Count(other => AreConflicting(s, other)))
            .ToList();

        foreach (var session in sortedSessions)
        {
            bool assigned = false;

            // Если есть пререквизит, начинаем поиск слота ПОСЛЕ него
            int startSlot = 1;
            if (session.PrerequisiteSessionId.HasValue)
            {
                var prereq = _sessions.FirstOrDefault(s => s.Id == session.PrerequisiteSessionId);
                if (prereq?.TimeSlot.HasValue == true)
                {
                    startSlot = prereq.TimeSlot.Value + prereq.Duration;
                }
            }

            for (int slot = startSlot; slot <= MaxSlots; slot++)
            {
                foreach (var room in _rooms.Where(r => r.Type == session.RequiredType))
                {
                    if (CanAssign(session, slot, room))
                    {
                        session.TimeSlot = slot;
                        session.Room = room;
                        assigned = true;
                        break;
                    }
                }
                if (assigned) break;
            }
        }
    }

    public void OptimizeSchedule()
    {
        // Минимизация окон: стараемся сдвинуть все пары к началу дня
        var sessionsByGroup = _sessions
            .Where(s => s.TimeSlot.HasValue)
            .GroupBy(s => s.Group.Id)
            .ToList();

        foreach (var groupSessions in sessionsByGroup)
        {
            var sorted = groupSessions.OrderBy(s => s.TimeSlot).ToList();
            foreach (var session in sorted)
            {
                int currentSlot = session.TimeSlot.Value;
                Room currentRoom = session.Room;
                
                // Пробуем найти более ранний слот в тот же день
                int day = (currentSlot - 1) / SlotsPerDay;
                for (int earlierSlot = day * SlotsPerDay + 1; earlierSlot < currentSlot; earlierSlot++)
                {
                    if (CanAssign(session, earlierSlot, currentRoom))
                    {
                        session.TimeSlot = earlierSlot;
                        break;
                    }
                }
            }
        }
    }

    public List<string> CheckForConflicts()
    {
        var conflicts = new List<string>();

        for (int i = 0; i < _sessions.Count; i++)
        {
            var s1 = _sessions[i];
            if (!s1.TimeSlot.HasValue || s1.Room == null) continue;

            for (int j = i + 1; j < _sessions.Count; j++)
            {
                var s2 = _sessions[j];
                if (!s2.TimeSlot.HasValue || s2.Room == null) continue;

                if (SessionsOverlap(s1, s2))
                {
                    if (s1.Group.Id == s2.Group.Id)
                        conflicts.Add($"Конфликт: Группа {s1.Group.Name} — пересечение '{s1.Subject}' и '{s2.Subject}'");
                    
                    if (s1.Lector.Id == s2.Lector.Id)
                        conflicts.Add($"Конфликт: Преподаватель {s1.Lector.Name} — пересечение '{s1.Subject}' и '{s2.Subject}'");
                    
                    if (s1.Room.Id == s2.Room.Id)
                        conflicts.Add($"Конфликт: Аудитория {s1.Room.Number} занята одновременно '{s1.Subject}' и '{s2.Subject}'");
                }
            }
        }

        return conflicts;
    }

    public Dictionary<int, double> GetRoomUtilization()
    {
        var utilization = new Dictionary<int, double>();

        foreach (var room in _rooms)
        {
            var usedSlots = _sessions.Count(s => s.Room?.Id == room.Id && s.TimeSlot.HasValue && s.TimeSlot.Value > 0);
            utilization[room.Id] = (double)usedSlots / MaxSlots * 100;
        }

        return utilization;
    }

    public Dictionary<int, double> GetLectorWorkload()
    {
        var workload = new Dictionary<int, double>();
        
        foreach (var session in _sessions.Where(s => s.TimeSlot.HasValue && s.TimeSlot.Value > 0))
        {
            var hours = session.Duration;
            if (workload.ContainsKey(session.Lector.Id))
                workload[session.Lector.Id] += hours;
            else
                workload[session.Lector.Id] = hours;
        }
        
        return workload;
    }

    public int CountGaps()
    {
        var gaps = 0;
        var lectors = _sessions.Select(s => s.Lector.Id).Distinct().ToList();
        
        foreach (var lectorId in lectors)
        {
            var lectorSessions = _sessions
                .Where(s => s.Lector.Id == lectorId && s.TimeSlot.HasValue)
                .OrderBy(s => s.TimeSlot)
                .Select(s => s.TimeSlot!.Value)
                .ToList();
            
            for (int i = 1; i < lectorSessions.Count; i++)
            {
                if (lectorSessions[i] - lectorSessions[i-1] > 1)
                    gaps++;
            }
        }
        
        return gaps;
    }

    public (bool IsValid, string Message) ValidateSessionPlacement(Session session, int newSlot, Room newRoom)
    {
        if (CanAssign(session, newSlot, newRoom))
        {
            return (true, "Допустимо");
        }
        
        // Более детальная проверка для сообщения об ошибке
        if (newSlot <= 0 || newSlot + session.Duration - 1 > MaxSlots)
            return (false, "Слот вне допустимого диапазона");

        int dayStart = (newSlot - 1) / SlotsPerDay;
        int dayEnd = (newSlot + session.Duration - 2) / SlotsPerDay;
        if (dayStart != dayEnd)
            return (false, "Занятие выходит за пределы одного дня");

        if (newRoom.Type != session.RequiredType)
            return (false, $"Неверный тип аудитории (требуется {session.RequiredType})");

        return (false, "Конфликт ресурсов (группа/преподаватель) или аудитория занята");
    }

    public void AssignSession(Session session, int slot, Room room)
    {
        var validation = ValidateSessionPlacement(session, slot, room);
        if (validation.IsValid)
        {
            session.TimeSlot = slot;
            session.Room = room;
        }
    }
}