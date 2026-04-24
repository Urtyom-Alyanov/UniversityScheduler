using Xunit;
using UniversityScheduler.Models;

namespace UniversityScheduler.Models.Tests;

public class SessionTests
{
    [Fact]
    public void Session_Constructor_InitializesProperties()
    {
        var group = new Group { Id = 1, Name = "P-101" };
        var lector = new Lector { Id = 1, Name = "Ivanov" };
        
        var session = new Session 
        { 
            Id = 1, 
            Subject = "Math", 
            Group = group, 
            Lector = lector, 
            RequiredType = RoomType.Lecture,
            Duration = 2
        };

        Assert.Equal(1, session.Id);
        Assert.Equal("Math", session.Subject);
        Assert.Equal(group, session.Group);
        Assert.Equal(lector, session.Lector);
        Assert.Equal(RoomType.Lecture, session.RequiredType);
        Assert.Equal(2, session.Duration);
    }

    [Fact]
    public void Session_TimeSlot_ChangesNotifyPropertyChanged()
    {
        var session = new Session { Id = 1, Subject = "Math" };
        bool notified = false;
        session.PropertyChanged += (s, e) => notified = true;
        
        session.TimeSlot = 5;
        
        Assert.True(notified);
        Assert.Equal(5, session.TimeSlot);
    }

    [Fact]
    public void Session_Room_ChangesNotifyPropertyChanged()
    {
        var session = new Session { Id = 1, Subject = "Math" };
        var room = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        bool notified = false;
        session.PropertyChanged += (s, e) => notified = true;
        
        session.Room = room;
        
        Assert.True(notified);
        Assert.Equal(room, session.Room);
    }

    [Fact]
    public void Session_Duration_ValidValues_AreAccepted()
    {
        var session1 = new Session { Duration = 1 };
        var session2 = new Session { Duration = 2 };
        
        Assert.Equal(1, session1.Duration);
        Assert.Equal(2, session2.Duration);
    }

    [Fact]
    public void Session_NullGroupOrLector_AllowsNull()
    {
        var session = new Session { Id = 1, Subject = "Math", Group = null!, Lector = null! };
        
        Assert.Null(session.Group);
        Assert.Null(session.Lector);
    }
}