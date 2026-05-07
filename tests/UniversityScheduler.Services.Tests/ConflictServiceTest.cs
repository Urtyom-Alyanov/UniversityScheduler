using UniversityScheduler.Models;
using UniversityScheduler.Services;
using Xunit;

namespace UniversityScheduler.Services.Tests;

public class ConflictServiceTests
{
    private readonly ConflictService _service = new();

    private (Lesson, TimeSlot, Room) CreateData(uint start = 10, uint duration = 2) {
        var l = new Lesson(new Subject("S"), new Group('B', "1", "1", "1", "1"), new Lector("L", "F", "M"), duration, RoomType.Lecture) { ID = Guid.NewGuid() };
        l.Lector.ID = Guid.NewGuid();
        l.Group.ID = Guid.NewGuid();
        var s = new TimeSlot(DayOfWeek.Monday, start);
        var r = new Room("1", 1, 1) { ID = Guid.NewGuid(), Type = RoomType.Lecture };
        return (l, s, r);
    }

    [Fact]
    public void HasConflict_SameRoom_ReturnsTrue() {
        var (l1, s1, r) = CreateData();
        var (l2, s2, _) = CreateData();
        var schedule = new List<ScheduledLesson> { new(l1, s1, r) };
        Assert.True(_service.HasConflict(l2, s1, r, schedule));
    }

    [Fact]
    public void HasConflict_SameLector_ReturnsTrue() {
        var (l1, s1, r1) = CreateData();
        var (l2, s2, r2) = CreateData();
        l2.Lector.ID = l1.Lector.ID;
        var schedule = new List<ScheduledLesson> { new(l1, s1, r1) };
        Assert.True(_service.HasConflict(l2, s1, r2, schedule));
    }

    [Fact]
    public void HasConflict_SameGroup_ReturnsTrue() {
        var (l1, s1, r1) = CreateData();
        var (l2, s2, r2) = CreateData();
        l2.Group.ID = l1.Group.ID;
        var schedule = new List<ScheduledLesson> { new(l1, s1, r1) };
        Assert.True(_service.HasConflict(l2, s1, r2, schedule));
    }

    [Fact]
    public void HasConflict_DifferentTime_ReturnsFalse() {
        var (l1, s1, r) = CreateData(10);
        var (l2, s2, _) = CreateData(12);
        var schedule = new List<ScheduledLesson> { new(l1, s1, r) };
        Assert.False(_service.HasConflict(l2, s2, r, schedule));
    }

    [Fact]
    public void GetConflictMessages_RoomTypeMismatch_ReturnsError() {
        var (l, s, r) = CreateData();
        r.Type = RoomType.Computer; // Требуется Lecture
        var messages = _service.GetConflictMessages(l, s, r, new List<ScheduledLesson>());
        Assert.Contains(messages, m => m.conflictType == ConflictType.RoomType);
    }
}
