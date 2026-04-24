using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UniversityScheduler.Models;

public enum RoomType { Lecture, ComputerLab, Laboratory }

public class Room : INotifyPropertyChanged
{
    private string _number;
    private RoomType _type;

    public int Id { get; set; }
    
    public string Number
    {
        get => _number;
        set
        {
            _number = value;
            OnPropertyChanged();
        }
    }

    public RoomType Type
    {
        get => _type;
        set
        {
            _type = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}