namespace UniversityScheduler.Models;

/// <summary>
/// Дисциплина aka предмет
/// </summary>
public class Subject(string name) {
  public Guid ID { get; set; }
  public string Name => name;
}
