using UniversityScheduler.Models;

namespace UniversityScheduler.Services.Tests;

using Moq;

public class ScheduleNOptimizationTests
{
    [Fact]
    public void ScheduleService_GeneratesSuccessfully() {
        var conflictMock = new Mock<IConflictService>();
        conflictMock.Setup(x => x.HasConflict(It.IsAny<Lesson>(), It.IsAny<TimeSlot>(), It.IsAny<Room>(), It.IsAny<IEnumerable<ScheduledLesson>>()))
                    .Returns(false);

        var topo = new TopologicalSortService();
        var service = new ScheduleService(conflictMock.Object, topo);

        var lessons = new List<Lesson> { new(new Subject("S"), new Group('B',"1","1","1","1"), new Lector("L","F","M"), 1, RoomType.Lecture) { ID = Guid.NewGuid() } };
        var rooms = new List<Room> { new Room("1", 1, 1) { Type = RoomType.Lecture } };
        var slots = new List<TimeSlot> { new TimeSlot(DayOfWeek.Monday, 8) };

        var result = service.GenerateScheduledLessons(lessons, rooms, slots);
        Assert.True(result.Success);
        Assert.Single(result.ScheduledLessons);
    }

    [Fact]
    public void ScheduleService_FailsWhenNoRooms() {
        var conflictMock = new Mock<IConflictService>();
        var service = new ScheduleService(conflictMock.Object, new TopologicalSortService());

        var lessons = new List<Lesson> { new(new Subject("S"), new Group('B',"1","1","1","1"), new Lector("L","F","M"), 1, RoomType.Lecture) { ID = Guid.NewGuid() } };
        var result = service.GenerateScheduledLessons(lessons, new List<Room>(), new List<TimeSlot>());

        Assert.False(result.Success);
        Assert.Single(result.UnassignedLessons);
    }

    [Fact]
    public void OptimizationService_MovesLessonToEarlierSlot() {
        var conflictMock = new Mock<IConflictService>();
        conflictMock.Setup(x => x.HasConflict(It.IsAny<Lesson>(), It.IsAny<TimeSlot>(), It.IsAny<Room>(), It.IsAny<List<ScheduledLesson>>()))
                    .Returns(false);

        var optService = new OptimizationService(conflictMock.Object);

        var group = new Group('B',"1","1","1","1") { ID = Guid.NewGuid() };
        var lesson = new Lesson(new Subject("S"), group, new Lector("L","F","M"), 1, RoomType.Lecture);
        var room = new Room("1", 1, 1) { Type = RoomType.Lecture };
        var scheduled = new ScheduledLesson(lesson, new TimeSlot(DayOfWeek.Monday, 10), room);

        var schedule = new List<ScheduledLesson> { scheduled };
        var allRooms = new List<Room> { room };
        var allSlots = new List<TimeSlot> { new TimeSlot(DayOfWeek.Monday, 8), new TimeSlot(DayOfWeek.Monday, 10) };

        optService.Optimize(schedule, allRooms, allSlots);

        Assert.Equal(8u, scheduled.Slot.StartHour); // Переместилось с 10 на 8
    }

    [Fact]
    public void OptimizationService_DoesNotMoveIfConflict() {
        var conflictMock = new Mock<IConflictService>();
        // Всегда конфликт
        conflictMock.Setup(x => x.HasConflict(It.IsAny<Lesson>(), It.IsAny<TimeSlot>(), It.IsAny<Room>(), It.IsAny<List<ScheduledLesson>>()))
                    .Returns(true);

        var optService = new OptimizationService(conflictMock.Object);
        var scheduled = new ScheduledLesson(new Lesson(null!, new Group('B',"1","1","1","1"), null!, 1, 0), new TimeSlot(DayOfWeek.Monday, 10), new Room("1",1,1));

        var schedule = new List<ScheduledLesson> { scheduled };
        optService.Optimize(schedule, new List<Room>(), new List<TimeSlot> { new TimeSlot(DayOfWeek.Monday, 8) });

        Assert.Equal(10u, scheduled.Slot.StartHour); // Не изменилось
    }

    [Fact]
    public void ScheduleResult_InitializesEmpty() {
        var result = new ScheduleResult();
        Assert.Empty(result.ScheduledLessons);
        Assert.Empty(result.UnassignedLessons);
        Assert.True(result.Success);
    }
}
