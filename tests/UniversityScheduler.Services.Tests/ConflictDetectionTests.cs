using Xunit;
using System.Collections.Generic;
using System.Linq;
using UniversityScheduler.Models;

namespace UniversityScheduler.Services.Tests;

public class ConflictDetectionTests
{
    [Fact]
    public void CheckForConflicts_TypicalData_ReturnsEmptyList()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var group2 = new Group { Id = 2, Name = "P-102" };
        
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var lector2 = new Lector { Id = 2, Name = "Petrov P.P." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        var room2 = new Room { Id = 2, Number = "102", Type = RoomType.ComputerLab };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 2 },
            new Session { Id = 2, Subject = "Programming", Group = group1, Lector = lector2, RequiredType = RoomType.ComputerLab, Duration = 1 },
            new Session { Id = 3, Subject = "Physics", Group = group2, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 2 },
            new Session { Id = 4, Subject = "Databases", Group = group2, Lector = lector2, RequiredType = RoomType.ComputerLab, Duration = 1 }
        };
        
        var rooms = new List<Room> { room1, room2 };
        var engine = new SchedulerEngine(sessions, rooms);
        engine.GenerateSchedule();

        var conflicts = engine.CheckForConflicts();

        Assert.Empty(conflicts);
    }

    [Fact]
    public void CheckForConflicts_EmptySessions_ReturnsEmptyList()
    {
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(new List<Session>(), rooms);

        var conflicts = engine.CheckForConflicts();

        Assert.Empty(conflicts);
    }

    [Fact]
    public void CheckForConflicts_DetectsGroupConflict()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = new Lector { Id = 2, Name = "Petrov" }, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1;
        sessions[0].Room = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        sessions[1].TimeSlot = 1;
        sessions[1].Room = new Room { Id = 2, Number = "102", Type = RoomType.Lecture };
        
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var conflicts = engine.CheckForConflicts();

        Assert.NotEmpty(conflicts);
        Assert.Contains(conflicts, c => c.Contains("Группа"));
    }

    [Fact]
    public void CheckForConflicts_DetectsLectorConflict()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var group2 = new Group { Id = 2, Name = "P-102" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group2, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1;
        sessions[0].Room = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        sessions[1].TimeSlot = 1;
        sessions[1].Room = new Room { Id = 2, Number = "102", Type = RoomType.Lecture };
        
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var conflicts = engine.CheckForConflicts();

        Assert.NotEmpty(conflicts);
        Assert.Contains(conflicts, c => c.Contains("Преподаватель"));
    }

    [Fact]
    public void CheckForConflicts_DetectsRoomConflict()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var group2 = new Group { Id = 2, Name = "P-102" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var lector2 = new Lector { Id = 2, Name = "Petrov P.P." };
        
        var room = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group2, Lector = lector2, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1;
        sessions[0].Room = room;
        sessions[1].TimeSlot = 1;
        sessions[1].Room = room;
        
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var conflicts = engine.CheckForConflicts();

        Assert.NotEmpty(conflicts);
        Assert.Contains(conflicts, c => c.Contains("Аудитория"));
    }

    [Fact]
    public void CheckForConflicts_SessionsWithNoTimeSlot_Ignored()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = new Lector { Id = 2, Name = "Petrov" }, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1;
        sessions[0].Room = room1;
        
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);

        var conflicts = engine.CheckForConflicts();

        Assert.Empty(conflicts);
    }

    [Fact]
    public void CheckForConflicts_DetectsDurationOverlap()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Long Math", Group = group1, Lector = new Lector{Id=1, Name="L1"}, RequiredType = RoomType.Lecture, Duration = 2 },
            new Session { Id = 2, Subject = "Short Physics", Group = group1, Lector = new Lector{Id=2, Name="L2"}, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1; // Занимает 1 и 2
        sessions[0].Room = room1;
        sessions[1].TimeSlot = 2; // Пересекается с хвостом первой пары
        sessions[1].Room = new Room { Id = 2, Number = "102", Type = RoomType.Lecture };
        
        var engine = new SchedulerEngine(sessions, new List<Room>());

        var conflicts = engine.CheckForConflicts();

        Assert.NotEmpty(conflicts);
        Assert.Contains(conflicts, c => c.Contains("Группа"));
    }

    [Fact]
    public void CheckForConflicts_LargeDataset_CompletesInTime()
    {
        var sessions = new List<Session>();
        var rooms = new List<Room>();

        for (int i = 0; i < 40; i++)
        {
            rooms.Add(new Room { Id = i + 1, Number = (i + 1).ToString(), Type = (RoomType)(i % 3) });
        }

        for (int i = 0; i < 120; i++)
        {
            var groupId = (i % 30) + 1;
            var lectorId = (i % 50) + 1;
            sessions.Add(new Session
            {
                Id = i + 1,
                Subject = $"Subject{i}",
                Group = new Group { Id = groupId, Name = $"G-{groupId}" },
                Lector = new Lector { Id = lectorId, Name = $"L-{lectorId}" },
                RequiredType = (RoomType)(i % 3),
                Duration = (i % 2) + 1
            });
        }

        var engine = new SchedulerEngine(sessions, rooms);
        engine.GenerateSchedule();

        var startTime = DateTime.Now;
        var conflicts = engine.CheckForConflicts();
        var elapsed = DateTime.Now - startTime;

        Assert.True(elapsed.TotalSeconds < 10, $"Took {elapsed.TotalSeconds}s");
    }
}
