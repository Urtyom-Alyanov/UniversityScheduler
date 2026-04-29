using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using UniversityScheduler.Models;
using UniversityScheduler.Services;

namespace UniversityScheduler.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<Session> Sessions { get; } = new();
    public ObservableCollection<ScheduleRow> ScheduleTable { get; } = new();
    public ObservableCollection<Group> Groups { get; } = new();
    public ObservableCollection<Lector> Lectors { get; } = new();
    public ObservableCollection<Room> Rooms { get; } = new();
    public ObservableCollection<Session> FilteredSessions { get; } = new();
    public ObservableCollection<string> Subjects { get; } = new();

    private Group? _selectedGroup;
    public Group? SelectedGroup
    {
        get => _selectedGroup;
        set
        {
            if (SetField(ref _selectedGroup, value))
                FilterByGroup();
        }
    }

    private Lector? _selectedLector;
    public Lector? SelectedLector
    {
        get => _selectedLector;
        set
        {
            if (SetField(ref _selectedLector, value))
                FilterByLector();
        }
    }

    private string? _selectedSubject;
    public string? SelectedSubject
    {
        get => _selectedSubject;
        set
        {
            if (SetField(ref _selectedSubject, value))
                FilterBySubject();
        }
    }

    private string _statusText = "Готов";
    public string StatusText
    {
        get => _statusText;
        set => SetField(ref _statusText, value);
    }

    private string _roomUtilization = "0%";
    public string RoomUtilization
    {
        get => _roomUtilization;
        set => SetField(ref _roomUtilization, value);
    }

    private string _lectorWorkload = "Нормальная";
    public string LectorWorkload
    {
        get => _lectorWorkload;
        set => SetField(ref _lectorWorkload, value);
    }

    private int _gapsCount;
    public int GapsCount
    {
        get => _gapsCount;
        set => SetField(ref _gapsCount, value);
    }

    private int _groupGapsCount;
    public int GroupGapsCount
    {
        get => _groupGapsCount;
        set => SetField(ref _groupGapsCount, value);
    }

    private string _generateTime = "-";
    public string GenerateTime
    {
        get => _generateTime;
        set => SetField(ref _generateTime, value);
    }

    private string _optimizeTime = "-";
    public string OptimizeTime
    {
        get => _optimizeTime;
        set => SetField(ref _optimizeTime, value);
    }

    public ICommand GenerateScheduleCommand { get; }
    public ICommand CheckConflictsCommand { get; }
    public ICommand OptimizeScheduleCommand { get; }
    public ICommand RefreshStatisticsCommand { get; }

    private SchedulerEngine? _engine;

    public MainViewModel()
    {
        GenerateScheduleCommand = new RelayCommand(_ => GenerateSchedule());
        CheckConflictsCommand = new RelayCommand(_ => CheckConflicts());
        OptimizeScheduleCommand = new RelayCommand(_ => OptimizeSchedule());
        RefreshStatisticsCommand = new RelayCommand(_ => RefreshStatistics());
        LoadMockData();
    }

    private void FilterByGroup()
    {
        FilteredSessions.Clear();
        var sessions = SelectedGroup != null
            ? Sessions.Where(s => s.Group.Id == SelectedGroup.Id && s.TimeSlot > 0)
            : Sessions.Where(s => s.TimeSlot > 0);

        foreach (var s in sessions.OrderBy(s => s.TimeSlot))
            FilteredSessions.Add(s);

        StatusText = SelectedGroup != null
            ? $"Расписание группы {SelectedGroup.Name}: {FilteredSessions.Count} занятий"
            : "Выберите группу";
    }

    private void FilterByLector()
    {
        FilteredSessions.Clear();
        var sessions = SelectedLector != null
            ? Sessions.Where(s => s.Lector.Id == SelectedLector.Id && s.TimeSlot > 0)
            : Sessions.Where(s => s.TimeSlot > 0);

        foreach (var s in sessions.OrderBy(s => s.TimeSlot))
            FilteredSessions.Add(s);

        StatusText = SelectedLector != null
            ? $"Расписание преподавателя {SelectedLector.Name}: {FilteredSessions.Count} занятий"
            : "Выберите преподавателя";
    }

    private void FilterBySubject()
    {
        FilteredSessions.Clear();
        var sessions = SelectedSubject != null
            ? Sessions.Where(s => s.Subject == SelectedSubject && s.TimeSlot > 0)
            : Sessions.Where(s => s.TimeSlot > 0);

        foreach (var s in sessions.OrderBy(s => s.TimeSlot))
            FilteredSessions.Add(s);

        StatusText = SelectedSubject != null
            ? $"Занятия по предмету {SelectedSubject}: {FilteredSessions.Count}"
            : "Выберите предмет";
    }

    private void GenerateSchedule()
    {
        try
        {
            _engine = new SchedulerEngine(Sessions.ToList(), Rooms.ToList());

            var sw = System.Diagnostics.Stopwatch.StartNew();
            _engine.GenerateSchedule();
            sw.Stop();
            GenerateTime = $"{sw.Elapsed.TotalMilliseconds:F2} мс";

            StatusText = "Расписание сгенерировано";
            RefreshScheduleTable();
            RefreshStatistics();
            OnPropertyChanged(nameof(Sessions));
        }
        catch (Exception ex)
        {
            StatusText = $"Ошибка: {ex.Message}";
        }
    }

    private void RefreshScheduleTable()
    {
        ScheduleTable.Clear();

        for (int slot = 1; slot <= 6; slot++)
        {
            var row = new ScheduleRow
            {
                TimeLabel = SchedulerEngine.FormatTimeOnly(slot)
            };

            // Days: index 0=time label, 1=Mon, 2=Tue, 3=Wed, 4=Thu, 5=Fri
            for (int day = 0; day < 5; day++)
            {
                int timeSlot = day * 6 + slot;
                var session = Sessions.FirstOrDefault(s => s.TimeSlot == timeSlot && s.Room != null);
                row.Add(new ScheduleCell { Session = session, Duration = session?.Duration ?? 1 });
            }

            ScheduleTable.Add(row);
        }
    }

    private void CheckConflicts()
    {
        try
        {
            _engine ??= new SchedulerEngine(Sessions.ToList(), Rooms.ToList());
            var conflicts = _engine.CheckForConflicts();

            if (conflicts.Any())
            {
                StatusText = $"Конфликтов: {conflicts.Count}";
            }
            else
            {
                StatusText = "Конфликтов не найдено.";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Ошибка: {ex.Message}";
        }
    }

    private void OptimizeSchedule()
    {
        try
        {
            _engine ??= new SchedulerEngine(Sessions.ToList(), Rooms.ToList());

            var sw = System.Diagnostics.Stopwatch.StartNew();
            _engine.OptimizeSchedule();
            sw.Stop();
            OptimizeTime = $"{sw.Elapsed.TotalMilliseconds:F2} мс";

            StatusText = "Расписание оптимизировано";
            RefreshScheduleTable();
            RefreshStatistics();
            OnPropertyChanged(nameof(Sessions));
        }
        catch (Exception ex)
        {
            StatusText = $"Ошибка: {ex.Message}";
        }
    }

    private void RefreshStatistics()
    {
        try
        {
            _engine ??= new SchedulerEngine(Sessions.ToList(), Rooms.ToList());

            var utilization = _engine.GetRoomUtilization();
            if (utilization.Any())
            {
                var avg = utilization.Values.Average();
                RoomUtilization = $"{avg:F1}%";
            }

            var workload = _engine.GetLectorWorkload();
            if (workload.Any())
            {
                var totalHours = workload.Values.Sum();
                LectorWorkload = totalHours <= 20 ? "Нормальная" : (totalHours <= 30 ? "Высокая" : "Очень высокая");
            }

            GapsCount = _engine.CountGaps();
            GroupGapsCount = _engine.CountGapsForGroups();
        }
        catch (Exception ex)
        {
            StatusText = $"Ошибка: {ex.Message}";
        }
    }

    private void LoadMockData()
    {
        var subjectNames = new[]
        {
            "Математический анализ", "Линейная алгебра", "Дискретная математика",
            "Теория вероятностей", "Математическая статистика", "Физика",
            "Теоретическая механика", "Сопротивление материалов", "Информатика",
            "Программирование", "Алгоритмы и структуры данных", "Базы данных",
            "Операционные системы", "Компьютерные сети", "Теория автоматов",
            "Электротехника", "Электроника", "Микропроцессорная техника",
            "Техническая механика", "Гидравлика", "Теплотехника",
            "Химия", "Материаловедение", "Технология машиностроения",
            "Начертательная геометрия", "Инженерная графика", "Метрология",
            "Стандартизация", "Управление качеством", "Безопасность жизнедеятельности",
            "Экономика", "Менеджмент", "Маркетинг", "Правоведение",
            "Философия", "История", "Иностранный язык", "Психология"
        };

        var lectors = new[]
        {
            "Иванов И.И.", "Петров П.П.", "Сидоров С.С.", "Козлов К.К.", "Смирнов С.М.",
            "Васильев В.В.", "Попов П.О.", "Андреев А.А.", "Николаев Н.Н.", "Захаров З.З.",
            "Степанов С.Т.", "Григорьев Г.Г.", "Михайлов М.М.", "Макаров М.К.", "Орлов О.О.",
            "Леонов Л.Л.", "Кузнецов К.К.", "Тарасов Т.Т.", "Борисов Б.Б.", "Яковлев Я.Я.",
            "Фёдоров Ф.Ф.", "Волков В.В.", "Зайцев З.З.", "Егоров Е.Е.", "Соколов С.С.",
            "Титов Т.Т.", "Афанасьев А.А.", "Дмитриев Д.Д.", "Матвеев М.М.", "Никитин Н.Н.",
            "Румянцев Р.Р.", "Ковалёв К.К.", "Громов Г.Г.", "Поляков П.П.", "Данилов Д.Д.",
            "Жуков Ж.Ж.", "Воробьёв В.В.", "Трофимов Т.Т.", "Осипов О.О.", "Суворов С.С.",
            "Нестеров Н.Н.", "Приходько П.П.", "Морозов М.М.", "Шипов Ш.Ш.", "Петухов П.П.",
            "Виноградов В.В.", "Клюев К.К.", "Кондратьев К.К.", "Сидоренко С.С.", "Аксёнов А.А."
        };

        for (int i = 1; i <= 30; i++)
        {
            Groups.Add(new Group { Id = i, Name = $"ПИН-{i:D2}" });
        }

        for (int i = 0; i < lectors.Length; i++)
        {
            Lectors.Add(new Lector { Id = i + 1, Name = lectors[i] });
        }

        for (int i = 1; i <= 15; i++)
        {
            Rooms.Add(new Room { Id = i, Number = $"10{i}", Type = RoomType.Lecture });
        }
        for (int i = 16; i <= 25; i++)
        {
            Rooms.Add(new Room { Id = i, Number = $"20{i-15}", Type = RoomType.ComputerLab });
        }
        for (int i = 26; i <= 40; i++)
        {
            Rooms.Add(new Room { Id = i, Number = $"30{i-25}", Type = RoomType.Laboratory });
        }

        var random = new Random(42);
        for (int i = 0; i < subjectNames.Length; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                var groupId = random.Next(0, 30);
                var lectorId = random.Next(0, lectors.Length);
                var duration = random.Next(0, 2) + 1;
                var roomType = i < 15 ? RoomType.Lecture : (i < 25 ? RoomType.ComputerLab : RoomType.Laboratory);
                var sessionId = i * 5 + j + 1;

                Sessions.Add(new Session
                {
                    Id = sessionId,
                    Subject = subjectNames[i],
                    Group = Groups[groupId],
                    Lector = Lectors[lectorId],
                    RequiredType = roomType,
                    Duration = duration
                });
            }
        }

        foreach (var s in Sessions)
        {
            if (!Subjects.Contains(s.Subject))
                Subjects.Add(s.Subject);
        }
    }
}
