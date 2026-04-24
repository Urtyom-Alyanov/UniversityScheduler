using Xunit;
using System.Collections.Generic;
using System.Linq;
using UniversityScheduler.Models;

namespace UniversityScheduler.Services.Tests;

public class TopologicalSortTests
{
    [Fact]
    public void TopologicalSort_TypicalData_ReturnsDagAndSortedSessions()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var group2 = new Group { Id = 2, Name = "P-102" };
        
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var lector2 = new Lector { Id = 2, Name = "Petrov P.P." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 2 },
            new Session { Id = 2, Subject = "Physics", Group = group2, Lector = lector2, RequiredType = RoomType.Lecture, Duration = 2 }
        };
        
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);

        var (isDag, sortedSessions) = engine.TopologicalSort();

        Assert.True(isDag);
        Assert.Equal(2, sortedSessions.Count);
    }

    [Fact]
    public void TopologicalSort_SingleSession_ReturnsValidSortedList()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);

        var (_, sortedSessions) = engine.TopologicalSort();

        Assert.Single(sortedSessions);
        Assert.Equal(1, sortedSessions[0].Id);
    }

    [Fact]
    public void TopologicalSort_EmptySessions_ReturnsEmptyList()
    {
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(new List<Session>(), rooms);

        var (isDag, sortedSessions) = engine.TopologicalSort();

        Assert.True(isDag);
        Assert.Empty(sortedSessions);
    }

    [Fact]
    public void TopologicalSort_LargeDataset_CompletesInReasonableTime()
    {
        var sessions = new List<Session>();
        var rooms = new List<Room>();
        
        for (int i = 0; i < 40; i++)
        {
            rooms.Add(new Room { Id = i + 1, Number = (i + 1).ToString(), Type = (RoomType)(i % 3) });
        }
        
        for (int i = 0; i < 150; i++)
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
        
        var (_, sortedSessions) = engine.TopologicalSort();
        
        var elapsed = DateTime.Now - startTime;
        Assert.True(elapsed.TotalSeconds < 10, $"Took {elapsed.TotalSeconds}s");
        Assert.Equal(150, sortedSessions.Count);
    }

    [Fact]
    public void TopologicalSort_ConflictingSessions_FallsBackToGreedy()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 2 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 2 }
        };
        
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);

        var (isDag, sortedSessions) = engine.TopologicalSort();

        Assert.False(isDag);
        Assert.Equal(2, sortedSessions.Count);
    }
}