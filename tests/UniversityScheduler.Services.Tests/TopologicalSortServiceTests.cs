using UniversityScheduler.Models;

namespace UniversityScheduler.Services.Tests;

public class TopologicalSortTests
{
  private readonly TopologicalSortService _service = new();

  [Fact]
  public void Sort_LinearDependency_ReturnsCorrectOrder() {
    var l1 = new Lesson(null!, null!, null!, 1, 0) { ID = Guid.NewGuid() };
    var l2 = new Lesson(null!, null!, null!, 1, 0) { ID = Guid.NewGuid(), Prerequisites = new() { l1.ID } };

    var result = _service.Sort(new() { l2, l1 });

    // У нас там типа разворот и поэтому возвращаем в таком порядке
    Assert.Equal(l2.ID, result[0].ID);
    Assert.Equal(l1.ID, result[1].ID);
  }

  [Fact]
  public void Sort_CyclicDependency_ThrowsException() {
    var id1 = Guid.NewGuid();
    var id2 = Guid.NewGuid();
    var l1 = new Lesson(null!, null!, null!, 1, 0) { ID = id1, Prerequisites = new() { id2 } };
    var l2 = new Lesson(null!, null!, null!, 1, 0) { ID = id2, Prerequisites = new() { id1 } };

    Assert.Throws<InvalidOperationException>(() => _service.Sort(new() { l1, l2 }));
  }

  [Fact]
  public void Sort_NoDependencies_ReturnsAll() {
    var lessons = new List<Lesson> {
      new(null!, null!, null!, 1, 0) { ID = Guid.NewGuid() },
      new(null!, null!, null!, 1, 0) { ID = Guid.NewGuid() }
    };
    var result = _service.Sort(lessons);
    Assert.Equal(2, result.Count);
  }

  [Fact]
  public void Sort_EmptyList_ReturnsEmpty() {
    var result = _service.Sort(new List<Lesson>());
    Assert.Empty(result);
  }

  [Fact]
  public void Sort_NullInput_ThrowsArgumentNullException() {
    Assert.Throws<ArgumentNullException>(() => _service.Sort(null!));
  }
}
