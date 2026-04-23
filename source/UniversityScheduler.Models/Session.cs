namespace UniversityScheduler.Models;

public class Session
{
    public int Id { get; set; }
    public string Subject { get; set; }
    public Group Group { get; set; }
    public Lector Lector { get; set; }
    public RoomType RequiredType { get; set; }
    public int Duration { get; set; } // 1 или 2 часа
        
    // Результат планирования
    public int? TimeSlot { get; set; } // "Цвет" в графе
    public Room Room { get; set; }
}