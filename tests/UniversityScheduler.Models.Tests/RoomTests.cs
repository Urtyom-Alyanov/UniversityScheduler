using Xunit;
using UniversityScheduler.Models;

namespace UniversityScheduler.Models.Tests;

public class RoomTests
{
    [Fact]
    public void Room_Constructor_InitializesProperties()
    {
        var room = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        
        Assert.Equal(1, room.Id);
        Assert.Equal("101", room.Number);
        Assert.Equal(RoomType.Lecture, room.Type);
    }

    [Fact]
    public void Room_Type_ChangesNotifyPropertyChanged()
    {
        var room = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        bool notified = false;
        room.PropertyChanged += (s, e) => notified = true;
        
        room.Type = RoomType.ComputerLab;
        
        Assert.True(notified);
        Assert.Equal(RoomType.ComputerLab, room.Type);
    }

    [Fact]
    public void Room_Number_ChangesNotifyPropertyChanged()
    {
        var room = new Room { Id = 1, Number = "101" };
        bool notified = false;
        room.PropertyChanged += (s, e) => notified = true;
        
        room.Number = "102";
        
        Assert.True(notified);
        Assert.Equal("102", room.Number);
    }

    [Fact]
    public void Room_AllTypes_AreValid()
    {
        Assert.True(Enum.IsDefined(typeof(RoomType), RoomType.Lecture));
        Assert.True(Enum.IsDefined(typeof(RoomType), RoomType.ComputerLab));
        Assert.True(Enum.IsDefined(typeof(RoomType), RoomType.Laboratory));
    }

    [Fact]
    public void Room_SameId_AreEqualForEquality()
    {
        var room1 = new Room { Id = 1, Number = "101", Type = RoomType.Lecture };
        var room2 = new Room { Id = 1, Number = "102", Type = RoomType.ComputerLab };
        
        Assert.Equal(room1.Id, room2.Id);
    }
}