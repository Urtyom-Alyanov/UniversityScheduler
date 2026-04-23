namespace UniversityScheduler.Models;

public enum RoomType { Lecture, ComputerLab, Laboratory }

public class Room
{
    public int Id { get; set; }
    public string Number { get; set; }
    public RoomType Type { get; set; }
}