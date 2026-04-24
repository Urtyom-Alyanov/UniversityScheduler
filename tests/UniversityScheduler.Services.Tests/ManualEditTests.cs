using Xunit;
using System.Collections.Generic;
using UniversityScheduler.Models;

namespace UniversityScheduler.Services.Tests;

public class ManualEditTests
{
    [Fact]
    public void AssignSession_TypicalData_AssignsSuccessfully()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var session = new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 };
        
        var sessions = new List<Session> { session };
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);

        engine.AssignSession(session, 1, room1);

        Assert.Equal(1, session.TimeSlot);
        Assert.Equal(room1, session.Room);
    }

    [Fact]
    public void AssignSession_ConflictDetected_DoesNotAssign()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var session1 = new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 };
        var session2 = new Session { Id = 2, Subject = "Physics", Group = group1, Lector = new Lector { Id = 2, Name = "Petrov" }, RequiredType = RoomType.Lecture, Duration = 1 };
        
        var sessions = new List<Session> { session1, session2 };
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);
        
        session1.TimeSlot = 1;
        session1.Room = room1;

        engine.AssignSession(session2, 1, room1);

        Assert.Null(session2.TimeSlot);
    }

    [Fact]
    public void AssignSession_LectorConflict_DoesNotAssign()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var group2 = new Group { Id = 2, Name = "P-102" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var session1 = new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 };
        var session2 = new Session { Id = 2, Subject = "Physics", Group = group2, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 };
        
        var sessions = new List<Session> { session1, session2 };
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);
        
        session1.TimeSlot = 1;
        session1.Room = room1;

        engine.AssignSession(session2, 1, room1);

        Assert.Null(session2.TimeSlot);
    }

    [Fact]
    public void AssignSession_DifferentTimeSlot_AssignsSuccessfully()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var session1 = new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 };
        var session2 = new Session { Id = 2, Subject = "Physics", Group = group1, Lector = new Lector { Id = 2, Name = "Petrov" }, RequiredType = RoomType.Lecture, Duration = 1 };
        
        var sessions = new List<Session> { session1, session2 };
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);
        
        session1.TimeSlot = 1;
        session1.Room = room1;

        engine.AssignSession(session2, 2, room1);

        Assert.Equal(2, session2.TimeSlot);
        Assert.Equal(room1, session2.Room);
    }

    [Fact]
    public void AssignSession_RoomTypeMismatch_DoesNotAssign()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var session = new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 };
        
        var sessions = new List<Session> { session };
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);

        engine.AssignSession(session, 1, room1);

        Assert.Equal(1, session.TimeSlot);
    }
}