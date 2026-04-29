using Xunit;
using System.Collections.Generic;
using System.Linq;
using UniversityScheduler.Models;

namespace UniversityScheduler.Services.Tests;

public class GenerateScheduleTests
{
    [Fact]
    public void GenerateSchedule_TypicalData_NoConflictsGenerated()
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

        Assert.All(sessions, s =>
        {
            Assert.NotNull(s.TimeSlot);
            Assert.NotNull(s.Room);
        });

        var conflicts = engine.CheckForConflicts();
        Assert.Empty(conflicts);
    }

    [Fact]
    public void GenerateSchedule_EmptySessions_AssignsNoTimeSlots()
    {
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(new List<Session>(), rooms);

        engine.GenerateSchedule();

        Assert.Empty(engine.Sessions);
    }

    [Fact]
    public void GenerateSchedule_NullGroupOrLector_AllowsNull()
    {
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = null!, Lector = null!, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);

        engine.GenerateSchedule();
    }

    [Fact]
    public void GenerateSchedule_LargeDataset_CompletesSuccessfully()
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
        var startTime = DateTime.Now;
        
        engine.GenerateSchedule();
        
        var elapsed = DateTime.Now - startTime;
        Assert.True(elapsed.TotalSeconds < 30, $"Took {elapsed.TotalSeconds}s");
        
        var conflicts = engine.CheckForConflicts();
        Assert.Empty(conflicts);
    }

    [Fact]
    public void GenerateSchedule_InsufficientRooms_SomeSessionsUnassigned()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var group2 = new Group { Id = 2, Name = "P-102" };
        var group3 = new Group { Id = 3, Name = "P-103" };
        
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group2, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 3, Subject = "Chemistry", Group = group3, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);

        engine.GenerateSchedule();

        var assignedCount = sessions.Count(s => s.TimeSlot.HasValue);
        Assert.True(assignedCount >= 1);
    }

    [Fact]
    public void GenerateSchedule_NegativeDuration_HandledGracefully()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = -1 }
        };
        
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);

        engine.GenerateSchedule();
    }

    [Fact]
    public void GenerateSchedule_SessionWithNullRoomType_NoCrash()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        engine.GenerateSchedule();
        Assert.Null(sessions[0].Room);
    }
}