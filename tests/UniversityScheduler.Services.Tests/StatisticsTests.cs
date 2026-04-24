using Xunit;
using System.Collections.Generic;
using System.Linq;
using UniversityScheduler.Models;

namespace UniversityScheduler.Services.Tests;

public class StatisticsTests
{
    [Fact]
    public void GetRoomUtilization_TypicalData_ReturnsValidPercentages()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        var room2 = new Room { Id = 2, Number = "102", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        var rooms = new List<Room> { room1, room2 };
        var engine = new SchedulerEngine(sessions, rooms);
        engine.GenerateSchedule();

        var utilization = engine.GetRoomUtilization();

        Assert.Equal(2, utilization.Count);
        Assert.All(utilization.Values, v => Assert.True(v >= 0));
    }

    [Fact]
    public void GetRoomUtilization_EmptyRooms_ReturnsEmptyDict()
    {
        var sessions = new List<Session>();
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var utilization = engine.GetRoomUtilization();

        Assert.Empty(utilization);
    }

    [Fact]
    public void GetLectorWorkload_TypicalData_ReturnsCorrectHours()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var sessions = new List<Session>
        {
            new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 2 },
            new Session { Id = 2, Subject = "Physics", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 }
        };
        
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);
        engine.GenerateSchedule();

        var workload = engine.GetLectorWorkload();

        Assert.Equal(3, workload[lector1.Id]);
    }

    [Fact]
    public void GetLectorWorkload_NoAssignedSessions_ReturnsZero()
    {
        var sessions = new List<Session>();
        var rooms = new List<Room>();
        var engine = new SchedulerEngine(sessions, rooms);

        var workload = engine.GetLectorWorkload();

        Assert.Empty(workload);
    }

    [Fact]
    public void ValidateSessionPlacement_TypicalData_ValidatesCorrectly()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var session = new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 };
        
        var sessions = new List<Session> { session };
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);

        var (isValid, message) = engine.ValidateSessionPlacement(session, 1, room1);

        Assert.True(isValid);
    }

    [Fact]
    public void ValidateSessionPlacement_WrongRoomType_ReturnsInvalid()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        var room2 = new Room { Id = 2, Number = "102", Type = RoomType.ComputerLab };
        
        var session = new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 };
        
        var sessions = new List<Session> { session };
        var rooms = new List<Room> { room1, room2 };
        var engine = new SchedulerEngine(sessions, rooms);

        var (isValid, message) = engine.ValidateSessionPlacement(session, 1, room2);

        Assert.False(isValid);
        Assert.Contains("Room type", message);
    }

    [Fact]
    public void ValidateSessionPlacement_ConflictWithGroup_ReturnsInvalid()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        var lector2 = new Lector { Id = 2, Name = "Petrov P.P." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var session1 = new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 };
        var session2 = new Session { Id = 2, Subject = "Physics", Group = group1, Lector = lector2, RequiredType = RoomType.Lecture, Duration = 1 };
        
        var sessions = new List<Session> { session1, session2 };
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);
        
        session1.TimeSlot = 1;
        session1.Room = room1;

        var (isValid, message) = engine.ValidateSessionPlacement(session2, 1, room1);

        Assert.False(isValid);
    }

    [Fact]
    public void ValidateSessionPlacement_RoomOccupied_ReturnsInvalid()
    {
        var group1 = new Group { Id = 1, Name = "P-101" };
        var group2 = new Group { Id = 2, Name = "P-102" };
        var lector1 = new Lector { Id = 1, Name = "Ivanov I.I." };
        
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        var session1 = new Session { Id = 1, Subject = "Math", Group = group1, Lector = lector1, RequiredType = RoomType.Lecture, Duration = 1 };
        var session2 = new Session { Id = 2, Subject = "Physics", Group = group2, Lector = new Lector { Id = 2, Name = "Petrov" }, RequiredType = RoomType.Lecture, Duration = 1 };
        
        var sessions = new List<Session> { session1, session2 };
        var rooms = new List<Room> { room1 };
        var engine = new SchedulerEngine(sessions, rooms);
        
        session1.TimeSlot = 1;
        session1.Room = room1;

        var (isValid, message) = engine.ValidateSessionPlacement(session2, 1, room1);

        Assert.False(isValid);
        Assert.Contains("Room", message);
    }

    [Fact]
    public void FormatTimeSlot_NullSlot_ReturnsDash()
    {
        var result = SchedulerEngine.FormatTimeSlot(null);
        
        Assert.Equal("—", result);
    }

    [Fact]
    public void FormatTimeSlot_ZeroSlot_ReturnsDash()
    {
        var result = SchedulerEngine.FormatTimeSlot(0);
        
        Assert.Equal("—", result);
    }

    [Fact]
    public void FormatTimeSlot_NegativeSlot_ReturnsDash()
    {
        var result = SchedulerEngine.FormatTimeSlot(-1);
        
        Assert.Equal("—", result);
    }

    [Fact]
    public void FormatTimeSlot_ValidSlot_ReturnsFormattedTime()
    {
        var result = SchedulerEngine.FormatTimeSlot(1);
        
        Assert.Contains("Пн", result);
    }

    [Fact]
    public void FormatTimeSlot_WithDuration2_ReturnsExtendedTime()
    {
        var result = SchedulerEngine.FormatTimeSlot(1, 2);
        
        Assert.Contains("Пн", result);
    }

    [Fact]
    public void ConvertSlotToDayAndSlot_InvalidSlot_ReturnsZero()
    {
        var (day, slotInDay) = SchedulerEngine.ConvertSlotToDayAndSlot(0);
        
        Assert.Equal(0, day);
        Assert.Equal(0, slotInDay);
    }

    [Fact]
    public void ConvertSlotToDayAndSlot_ValidSlot_ReturnsCorrectValues()
    {
        var (day, slotInDay) = SchedulerEngine.ConvertSlotToDayAndSlot(7);
        
        Assert.Equal(2, day);
        Assert.Equal(1, slotInDay);
    }
}