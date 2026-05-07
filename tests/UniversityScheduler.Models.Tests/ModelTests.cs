using UniversityScheduler.Models;
using Xunit;

namespace UniversityScheduler.Models.Tests;

public class ModelTests
{
  [Fact]
  public void Subject_ShouldFormatCorrectlty()
  {
    var subj = new Subject("Метаматематика");

    Assert.Equal("Метаматематика", subj.Name);
  }

  [Fact]
  public void Lesson_CorrectSubject()
  {
    var subj = new Subject("Метаматематика");
    var lesson = new Lesson(subj, new Group('B', "1", "1", "1", "1"),
      new Lector("R", "M", "S"), 2, RoomType.Lecture);

    Assert.Equal(lesson.Subject, subj);
  }

  [Fact]
  public void Room_FullNumber_ShouldFormatCorrectlty()
  {
    var room = new Room("A", 2, 5); // Корпус А, этаж 2, каб 5
    Assert.Equal("A-205", room.FullNumber);

    var room2 = new Room("B", 1, 12);
    Assert.Equal("B-112", room2.FullNumber);
  }

  [Fact]
  public void Lector_Names_ShouldFormatCorrectly()
  {
    var lector = new Lector("Ivanov", "Ivan", "Ivanovich");
    Assert.Equal("Ivanov Ivan Ivanovich", lector.FullName);
    Assert.Equal("Ivanov I. I.", lector.LastNameWithAliases);
  }

  [Fact]
  public void Group_Name_ShouldFormatCorrectly()
  {
    var group = new Group('Б', "ПИН", "ИИ", "25", "16");
    Assert.Equal("Б.ПИН.ИИ.25.16", group.Name);
  }

  [Fact]
  public void ScheduledLesson_EndTime_ShouldCalculateCorrectly()
  {
    var lesson = new Lesson(new Subject("Math"), new Group('B', "1", "1", "1", "1"),
      new Lector("R", "M", "S"), 2, RoomType.Lecture);
    var slot = new TimeSlot(DayOfWeek.Monday, 1);
    var room = new Room("1", 1, 1);
    var scheduled = new ScheduledLesson(lesson, slot, room);

    Assert.Equal(3u, scheduled.EndTime); // 1 + 2 = 3
  }
}
