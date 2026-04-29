namespace UniversityScheduler.Models;

/// <summary>
/// Кабинет
/// </summary>
public class Room(string building, uint stage, uint number) {
  public Guid ID { get; set; }
  public RoomType Type { get; set; } = RoomType.Lecture;
  public uint Capacity { get; set; }

  public string FullNumber => $"{building}-{stage}{(number > 9 ? number : $"0{number}")}";
}
