using UniversityScheduler.Models;

namespace UniversityScheduler.Services.Tests;

public class StatisticsServiceTests
{
    private readonly StatisticsService _service = new();

    [Fact]
    public void GetRoomUtilization_CalculatesCorrectPercentage() {
        var room = new Room("1", 1, 1) { ID = Guid.NewGuid() };
        var schedule = new List<ScheduledLesson> {
            new(null!, null!, room),
            new(null!, null!, room)
        };
        var result = _service.GetRoomUtilization(room, schedule, 10);
        Assert.Equal(20.0, result);
    }

    [Fact]
    public void GetLectorWorkload_SumDurations() {
        var lector = new Lector("Doe", "J", "J");
        var l1 = new Lesson(null!, null!, lector, 2, 0);
        var l2 = new Lesson(null!, null!, lector, 3, 0);
        var schedule = new List<ScheduledLesson> {
            new(l1, new TimeSlot(0, 0), null!),
            new(l2, new TimeSlot(0, 4), null!)
        };
        var workload = _service.GetLectorWorkload(schedule);
        Assert.Equal(5u, (uint)workload["Doe J. J."]);
    }

    [Fact]
    public void CalculateTotalWindows_NoWindows_ReturnsZero() {
        var group = new Group('B', "1", "1", "1", "1") { ID = Guid.NewGuid() };
        var lesson = new Lesson(null!, group, null!, 1, 0);
        var schedule = new List<ScheduledLesson> {
            new(lesson, new TimeSlot(DayOfWeek.Monday, 8), null!),
            new(lesson, new TimeSlot(DayOfWeek.Monday, 9), null!)
        };
        Assert.Equal(0u, _service.CalculateTotalWindows(schedule));
    }

    [Fact]
    public void CalculateTotalWindows_WithGap_ReturnsCount() {
        var group = new Group('B', "1", "1", "1", "1") { ID = Guid.NewGuid() };
        var lesson = new Lesson(null!, group, null!, 1, 0);
        var schedule = new List<ScheduledLesson> {
            new(lesson, new TimeSlot(DayOfWeek.Monday, 1), null!),
            new(lesson, new TimeSlot(DayOfWeek.Monday, 4), null!) // Окно в 2 часа (1 (заканчивается в 2) и 4)
        };
        Assert.Equal(2u, _service.CalculateTotalWindows(schedule));
    }

    [Fact]
    public void CalculateTotalWindows_DifferentDays_NoWindows() {
        var group = new Group('B', "1", "1", "1", "1") { ID = Guid.NewGuid() };
        var lesson = new Lesson(null!, group, null!, 1, 0);
        var schedule = new List<ScheduledLesson> {
            new(lesson, new TimeSlot(DayOfWeek.Monday, 8), null!),
            new(lesson, new TimeSlot(DayOfWeek.Tuesday, 11), null!)
        };
        Assert.Equal(0u, _service.CalculateTotalWindows(schedule));
    }
}
