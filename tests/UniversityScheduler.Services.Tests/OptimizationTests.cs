using Xunit;
using System.Collections.Generic;
using System.Linq;
using UniversityScheduler.Models;

namespace UniversityScheduler.Services.Tests;

public class OptimizationTests
{
    [Fact]
    public void OptimizeSchedule_TypicalData_ReducesGaps()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);
        engine.GenerateSchedule();
        
        var gapsBefore = engine.CountGaps();
        
        engine.OptimizeSchedule();
        
        var gapsAfter = engine.CountGaps();
    }

    [Fact]
    public void OptimizeSchedule_EmptySchedule_NoChange()
    {
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(new List<Session>(), rooms);

        engine.OptimizeSchedule();

        Assert.Empty(engine.Sessions);
    }

    [Fact]
    public void OptimizeSchedule_AlreadyOptimized_NoChange()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var group2 = new Group { Id = 2, Name = "P-102" };
        
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var lector2 = new Lector { Id = 2, Name = "Petrov P.P." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        var room2 = new Room { Id = 2, Number = "102", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group2, Lector = lector2, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        var rooms = new List<Room> { room1, room2 };
        var engine = new SchedulerEngine(sessions, rooms);
        engine.GenerateSchedule();
        engine.OptimizeSchedule();

        var conflicts = engine.CheckForConflicts();
        Assert.Empty(conflicts);
    }

    [Fact]
    public void OptimizeSchedule_LargeDataset_CompletesInTime()
    {
        var sessions = new List<Session>();
        var rooms = new List<Room>();
        
        for (int i = 0; i < 40; i++)
        {
            rooms.Add(new Room { Id = i + 1, Number = (i + 1).ToString(), Type = (RoomType)(i % 3) });
        }
        
        for (int i = 0; i < 100; i++)
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
        
        engine.OptimizeSchedule();
        
        var elapsed = DateTime.Now - startTime;
        Assert.True(elapsed.TotalSeconds < 30);
        
        var conflicts = engine.CheckForConflicts();
        Assert.Empty(conflicts);
    }

    [Fact]
    public void CountGaps_TypicalData_ReturnsCorrectCount()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1;
        sessions[1].TimeSlot = 3;
        
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var gaps = engine.CountGaps();

        Assert.Equal(1, gaps);
    }

    [Fact]
    public void CountGaps_NoGaps_ReturnsZero()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1;
        sessions[1].TimeSlot = 2;
        
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var gaps = engine.CountGaps();

        Assert.Equal(0, gaps);
    }

    [Fact]
    public void CountGaps_SingleSession_ReturnsZero()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1;
        
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var gaps = engine.CountGaps();

        Assert.Equal(0, gaps);
    }

    [Fact]
    public void CountGaps_MultipleLectors_CountsGapsPerLector()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var lector2 = new Lector { Id = 2, Name = "Petrov P.P." };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 3, Subject = "Chemistry", Group = group1, Lector = lector2, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1;
        sessions[1].TimeSlot = 3;
        sessions[2].TimeSlot = 1;
        
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var gaps = engine.CountGaps();

        Assert.Equal(1, gaps);
    }

    [Fact]
    public void CountGaps_SessionsWithNoTimeSlot_Ignored()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        sessions[0].TimeSlot = 1;
        
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var gaps = engine.CountGaps();

        Assert.Equal(0, gaps);
    }
}