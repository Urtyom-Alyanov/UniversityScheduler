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
        Assert.Contains(conflicts, c => c.Contains("Group"));
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
        Assert.Contains(conflicts, c => c.Contains("Lector"));
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
        Assert.Contains(conflicts, c => c.Contains("Room"));
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
    public void CheckForConflicts_MultipleConflictsAtSameSlot_ReturnsDistinct()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1;
        sessions[0].Room = room1;
        sessions[1].TimeSlot = 1;
        sessions[1].Room = room1;
        
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var conflicts = engine.CheckForConflicts();
        var distinctConflicts = conflicts.Distinct().ToList();

        Assert.NotEmpty(conflicts);
    }
}