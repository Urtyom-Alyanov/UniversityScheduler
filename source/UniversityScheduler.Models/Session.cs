using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UniversityScheduler.Models;

public class Session : INotifyPropertyChanged
{
    private int? _timeSlot;
    private Room _room;

    public int Id { get; set; }
    public string Subject { get; set; }
    public Group Group { get; set; }
    public Lector Lector { get; set; }
    public RoomType RequiredType { get; set; }
    public int Duration { get; set; } // 1 или 2 часа
        
    // Результат планирования
    public int? TimeSlot 
    { 
        get => _timeSlot; 
        set
        {
            _timeSlot = value;
            OnPropertyChanged();
        }
    }

    public Room Room 
    { 
        get => _room; 
        set
        {
            _room = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}