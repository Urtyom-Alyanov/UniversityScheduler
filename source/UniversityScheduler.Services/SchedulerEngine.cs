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
        
        var timeStr = $"{startTimes[slotInDay]/60}:{startTimes[slotInDay]%60:D2}-{endHour:D2}:{endMinute:D2}";
        return $"{dayNames[day]}, {timeStr}";
    }
    
    private bool AreConflicting(Session sessionFirst, Session sessionSecond)
    {
        if (sessionFirst.Id == sessionSecond.Id) return false;
        
        return sessionFirst.Lector.Id == sessionSecond.Lector.Id || 
               sessionFirst.Group.Id == sessionSecond.Group.Id;
    }

    private int GetDegree(Session session) => _sessions.Count(other => AreConflicting(session, other));
    
    private bool CanAssign(Session session, int slot)
    {
        if (slot <= 0 || slot > MaxSlots) return false;
        return !_sessions.Any(s => s.TimeSlot == slot && s.TimeSlot.HasValue && AreConflicting(session, s));
    }

    private bool IsRoomFree(Room room, int slot)
    {
        if (slot <= 0) return true;
        return !_sessions.Any(s => s.TimeSlot == slot && s.TimeSlot.HasValue && s.Room?.Id == room.Id);
    }

    private (bool IsValidDay, bool IsValidSlot) ValidateSlot(int slot)
    {
        if (slot <= 0) return (false, false);
        var (day, slotInDay) = ConvertSlotToDayAndSlot(slot);
        return (day >= 1 && day <= DaysPerWeek, slotInDay >= 1 && slotInDay <= SlotsPerDay);
    }

    private Dictionary<int, List<int>> BuildConflictGraph()
    {
        var graph = new Dictionary<int, List<int>>();
        foreach (var s in _sessions)
            graph[s.Id] = new List<int>();
        
        foreach (var s1 in _sessions)
        {
            foreach (var s2 in _sessions)
            {
                if (s1.Id != s2.Id && AreConflicting(s1, s2) && !graph[s1.Id].Contains(s2.Id))
                {
                    graph[s1.Id].Add(s2.Id);
                }
            }
        }
        
        return graph;
    }

    public (bool IsDag, List<Session> SortedSessions) TopologicalSort()
    {
        var graph = BuildConflictGraph();
        var inDegree = _sessions.ToDictionary(s => s.Id, _ => 0);
        foreach (var s in _sessions)
        {
            inDegree[s.Id] = graph[s.Id].Count;
        }

        var queue = new Queue<int>();
        foreach (var s in _sessions.Where(s => inDegree[s.Id] == 0))
            queue.Enqueue(s.Id);

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

        var isDag = sorted.Count == _sessions.Count;
        
        if (!isDag)
        {
            sorted = _sessions
                .OrderByDescending(s => GetDegree(s))
                .ToList();
        }
        
        return (isDag, sorted);
    }

    public void GenerateSchedule()
    {
        var (_, sortedSessions) = TopologicalSort();
        var sessionQueue = new Queue<Session>(sortedSessions);

        while (sessionQueue.Count > 0)
        {
            for (int day = 0; day < DaysPerWeek; day++)
            {
                if (sessionQueue.Count == 0) break;
                
                for (int slotInDay = 1; slotInDay <= SlotsPerDay; slotInDay++)
                {
                    if (sessionQueue.Count == 0) break;
                    
                    var slot = day * SlotsPerDay + slotInDay;
                    var session = sessionQueue.Peek();
                    
                    var availableRoom = _rooms.FirstOrDefault(r => 
                        r.Type == session.RequiredType && 
                        IsRoomFree(r, slot));

                    if (availableRoom != null && CanAssign(session, slot))
                    {
                        sessionQueue.Dequeue();
                        session.TimeSlot = slot;
                        session.Room = availableRoom;
                    }
                }
            }
        }
    }

    public void OptimizeSchedule()
    {
        var scheduledSessions = _sessions
            .Where(s => s.TimeSlot.HasValue && s.TimeSlot.Value > 0)
            .ToList();

        if (!scheduledSessions.Any()) return;

        var sortedSessions = scheduledSessions
            .OrderBy(s => s.Group.Id)
            .ThenBy(s => s.Lector.Id)
            .ToList();

        foreach (var session in sortedSessions)
        {
            session.TimeSlot = null;
            session.Room = null;
        }

        var daySlotCounts = new int[DaysPerWeek];

        foreach (var session in sortedSessions)
        {
            int bestDay = -1;
            int bestSlot = -1;
            Room? bestRoom = null;
            int minCount = int.MaxValue;

            var daysWithMinCount = Enumerable.Range(0, DaysPerWeek)
                .Where(d => daySlotCounts[d] <= minCount)
                .ToList();

            foreach (var day in daysWithMinCount.OrderBy(d => daySlotCounts[d]))
            {
                for (int slotInDay = 0; slotInDay < SlotsPerDay; slotInDay++)
                {
                    int slot = day * SlotsPerDay + slotInDay + 1;

                    var availableRoom = _rooms.FirstOrDefault(r =>
                        r.Type == session.RequiredType &&
                        IsRoomFree(r, slot));

                    if (availableRoom != null && CanAssign(session, slot))
                    {
                        bestDay = day;
                        bestSlot = slot;
                        bestRoom = availableRoom;
                        break;
                    }
                }
                if (bestSlot > 0)
                {
                    daySlotCounts[bestDay]++;
                    break;
                }
            }

            if (bestSlot > 0 && bestRoom != null)
            {
                session.TimeSlot = bestSlot;
                session.Room = bestRoom;
            }
            else
            {
                for (int slot = 1; slot <= MaxSlots; slot++)
                {
                    var availableRoom = _rooms.FirstOrDefault(r =>
                        r.Type == session.RequiredType &&
                        IsRoomFree(r, slot));

                    if (availableRoom != null && CanAssign(session, slot))
                    {
                        session.TimeSlot = slot;
                        session.Room = availableRoom;
                        break;
                    }
                }
            }
        }
    }

    public List<string> CheckForConflicts()
    {
        var conflicts = new List<string>();

        foreach (var session1 in _sessions)
        {
            if (!session1.TimeSlot.HasValue || session1.Room == null) continue;

            foreach (var session2 in _sessions)
            {
                if (session1.Id == session2.Id || !session2.TimeSlot.HasValue || session2.Room == null) continue;

                if (session1.TimeSlot == session2.TimeSlot)
                {
                    if (session1.Group.Id == session2.Group.Id)
                    {
                        conflicts.Add($"Conflict: Group {session1.Group.Name} has two sessions (\"{session1.Subject}\" and \"{session2.Subject}\") at time slot {session1.TimeSlot}.");
                    }
                    if (session1.Lector.Id == session2.Lector.Id)
                    {
                        conflicts.Add($"Conflict: Lector {session1.Lector.Name} has two sessions (\"{session1.Subject}\" and \"{session2.Subject}\") at time slot {session1.TimeSlot}.");
                    }
                    if (session1.Room.Id == session2.Room.Id)
                    {
                        conflicts.Add($"Conflict: Room {session1.Room.Number} has two sessions (\"{session1.Subject}\" and \"{session2.Subject}\") at time slot {session1.TimeSlot}.");
                    }
                }
            }
        }

        return conflicts.Distinct().ToList();
    }

    public Dictionary<int, double> GetRoomUtilization()
    {
        var utilization = new Dictionary<int, double>();
        var maxSlot = _sessions.Where(s => s.TimeSlot.HasValue && s.TimeSlot.Value > 0)
            .Select(s => s.TimeSlot!.Value)
            .DefaultIfEmpty(0)
            .Max();
        
        if (maxSlot == 0) maxSlot = MaxSlots;
        
        foreach (var room in _rooms)
        {
            var usedSlots = _sessions.Count(s => s.Room?.Id == room.Id && s.TimeSlot.HasValue && s.TimeSlot.Value > 0);
            utilization[room.Id] = (double)usedSlots / maxSlot * 100;
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
        if (newRoom.Type != session.RequiredType)
            return (false, "Room type does not match required type");
        
        var otherSessions = _sessions.Where(s => s.Id != session.Id && s.TimeSlot == newSlot).ToList();
        
        foreach (var other in otherSessions)
        {
            if (AreConflicting(session, other))
                return (false, $"Conflict with session {other.Subject}");
            
            if (other.Room?.Id == newRoom.Id)
                return (false, "Room is already occupied");
        }
        
        return (true, "Valid");
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