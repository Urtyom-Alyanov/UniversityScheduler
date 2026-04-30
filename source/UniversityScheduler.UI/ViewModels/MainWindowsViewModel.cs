using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
using System.Linq;
using UniversityScheduler.Models;
using UniversityScheduler.Services;

namespace UniversityScheduler.UI.ViewModels;

public enum ViewMode {
  Group,
  Lector,
  Room
}

public class MainWindowsViewModel : ViewModelBase {
  private readonly ScheduleService _scheduler;
  private readonly OptimizationService _optimizer;
  private readonly StatisticsService _statisticsService;

  private ObservableCollection<ScheduledLesson> _allScheduledLessons = new();
  private ObservableCollection<ScheduledLesson> _displayLessons = new();

  private Group? _selectedGroup;
  private Lector? _selectedLector;
  private Room? _selectedRoom;
  private ViewMode _viewMode = ViewMode.Group;

  private string _statisticsText = string.Empty;

  public ObservableCollection<Group> Groups { get; } = new();
  public ObservableCollection<Lector> Lectors { get; } = new();
  public ObservableCollection<Room> Rooms { get; } = new();
  public ObservableCollection<TimeSlot> TimeSlots { get; } = new();

  public ObservableCollection<ScheduledLesson> DisplayLessons {
    get => _displayLessons;
    set => SetProperty(ref _displayLessons, value);
  }

  public Group? SelectedGroup {
    get => _selectedGroup;
    set {
      if (SetProperty(ref _selectedGroup, value) && _viewMode == ViewMode.Group)
        UpdateDisplay();
    }
  }

  public Lector? SelectedLector {
    get => _selectedLector;
    set {
      if (SetProperty(ref _selectedLector, value) && _viewMode == ViewMode.Lector)
        UpdateDisplay();
    }
  }

  public Room? SelectedRoom {
    get => _selectedRoom;
    set {
      if (SetProperty(ref _selectedRoom, value) && _viewMode == ViewMode.Room)
        UpdateDisplay();
    }
  }

  public ViewMode SelectedViewMode {
    get => _viewMode;
    set {
      if (SetProperty(ref _viewMode, value)) {
        OnPropertyChanged(nameof(SelectionItems));
        SelectedEntity = SelectionItems.Cast<object?>().FirstOrDefault();
        UpdateDisplay();
      }
    }
  }

  public IEnumerable<object> SelectionItems {
    get {
      return _viewMode switch {
        ViewMode.Group => Groups.Cast<object>(),
        ViewMode.Lector => Lectors.Cast<object>(),
        ViewMode.Room => Rooms.Cast<object>(),
        _ => Groups.Cast<object>()
      };
    }
  }

  private object? _selectedEntity;
  public object? SelectedEntity {
    get => _selectedEntity;
    set {
      if (SetProperty(ref _selectedEntity, value)) {
        if (value is Group g) SelectedGroup = g;
        else if (value is Lector l) SelectedLector = l;
        else if (value is Room r) SelectedRoom = r;
      }
    }
  }

  public string StatisticsText {
    get => _statisticsText;
    set => SetProperty(ref _statisticsText, value);
  }

  private string _logsText = string.Empty;
  private string _debugText = string.Empty;

  public string LogsText {
    get => _logsText;
    set => SetProperty(ref _logsText, value);
  }

  public string DebugText {
    get => _debugText;
    set => SetProperty(ref _debugText, value);
  }

  private ScheduledLesson? _selectedScheduledLesson;

  public ScheduledLesson? SelectedScheduledLesson {
    get => _selectedScheduledLesson;
    set {
      if (SetProperty(ref _selectedScheduledLesson, value)) {
        OnPropertyChanged(nameof(IsLessonSelected));
        UpdateConflictMessages();
      }
    }
  }

  public bool IsLessonSelected => _selectedScheduledLesson != null;

  private string _conflictMessages = string.Empty;
  public string ConflictMessages {
    get => _conflictMessages;
    set => SetProperty(ref _conflictMessages, value);
  }

  public ICommand GenerateCommand { get; }
  public ICommand OptimizeCommand { get; }
  public ICommand ClearCommand { get; }
  public ICommand MoveUpCommand { get; }
  public ICommand MoveDownCommand { get; }
  public ICommand MoveLeftCommand { get; }
  public ICommand MoveRightCommand { get; }

  public MainWindowsViewModel(
      ScheduleService scheduler,
      OptimizationService optimizer,
      StatisticsService statisticsService
  ) {
    _scheduler = scheduler;
    _optimizer = optimizer;
    _statisticsService = statisticsService;

    GenerateCommand = new RelayCommand(_ => Generate());
    OptimizeCommand = new RelayCommand(_ => Optimize(), _ => _allScheduledLessons.Count > 0);
    ClearCommand = new RelayCommand(_ => Clear());

    MoveUpCommand = new RelayCommand(_ => MoveLesson(0, -1), _ => IsLessonSelected);
    MoveDownCommand = new RelayCommand(_ => MoveLesson(0, 1), _ => IsLessonSelected);
    MoveLeftCommand = new RelayCommand(_ => MoveLesson(-1, 0), _ => IsLessonSelected);
    MoveRightCommand = new RelayCommand(_ => MoveLesson(1, 0), _ => IsLessonSelected);

    InitializeData();
  }

  private void MoveLesson(int dayDelta, int hourDelta) {
    if (SelectedScheduledLesson == null) return;

    var currentSlot = SelectedScheduledLesson.Slot;
    var newDay = (DayOfWeek)(((int)currentSlot.Day + dayDelta - 1 + 7) % 7 + 1);
    if (newDay == DayOfWeek.Sunday) newDay = DayOfWeek.Monday;

     var newHour = (uint)Math.Clamp((int)currentSlot.StartHour + hourDelta, 1, 6);
    var newSlot = new TimeSlot(newDay, newHour);

    var otherLessons = _allScheduledLessons.Where(l => l != SelectedScheduledLesson).ToList();
    var conflictService = new ConflictService();

    if (!conflictService.HasConflict(SelectedScheduledLesson.Lesson, newSlot, SelectedScheduledLesson.Room, otherLessons)) {
      SelectedScheduledLesson.Slot = newSlot;
      UpdateDisplay();
      UpdateStatistics();
      ConflictMessages = string.Empty;
    } else {
      var messages = conflictService.GetConflictMessages(SelectedScheduledLesson.Lesson, newSlot, SelectedScheduledLesson.Room, otherLessons);
      ConflictMessages = "Конфликт: " + string.Join(", ", messages.Select(m => $"{m.conflictType}: {m.entityName}"));
    }
  }

  private void UpdateConflictMessages() {
    if (SelectedScheduledLesson == null) {
      ConflictMessages = string.Empty;
      return;
    }
  }

  private readonly int GROUP_COUNT = 30;
  private readonly int LECTOR_COUNT = 50;
  private readonly int ROOM_COUNT = 40;

  private void InitializeData() {
    // Генерация групп
    for (int i = 1; i <= GROUP_COUNT; i++)
      Groups.Add(new Group('Б', "ПИН", "ИИ", "22", i.ToString("D2")) { ID = Guid.NewGuid() });

    // Генерация преподавателей
    string[] firstNames = { "Иван", "Петр", "Сергей", "Алексей", "Дмитрий", "Андрей", "Михаил" };
    string[] lastNames = { "Иванов", "Петров", "Сидоров", "Кузнецов", "Смирнов", "Попов", "Васильев" };
    for (int i = 1; i <= LECTOR_COUNT; i++)
      Lectors.Add(new Lector(lastNames[i % lastNames.Length], firstNames[i % firstNames.Length], "Олегович") { ID = Guid.NewGuid() });

    // Генерация аудиторий
    RoomType[] roomTypes = { RoomType.Lecture, RoomType.Computer, RoomType.Laboratory };
    for (int i = 1; i <= ROOM_COUNT; i++) {
        var room = new Room("ГК", (uint)(i / 10 + 1), (uint)(i % 10 + 1)) {
            ID = Guid.NewGuid(),
            Type = roomTypes[i % roomTypes.Length]
        };
        Rooms.Add(room);
    }

    // Генерация временных слотов
    for (int day = 1; day <= 6; day++) {
      for (uint hour = 1; hour <= 6; hour++) {
        TimeSlots.Add(new TimeSlot((DayOfWeek)day, hour));
      }
    }

    try {
      var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app_log.txt");
      if (!File.Exists(logPath)) {
        // попробовать найти в рабочей папке проекта (при запуске из IDE)
        var candidate = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!.FullName, "app_log.txt");
        if (File.Exists(candidate)) logPath = candidate;
      }

      LogsText = File.Exists(logPath) ? File.ReadAllText(logPath) : "Журнала не найдено: " + logPath;
    } catch {
      LogsText = "Ошибка при чтении журнала";
    }

    var sb = new System.Text.StringBuilder();
    sb.AppendLine($"Групп: {Groups.Count}");
    foreach (var g in Groups.Take(10)) sb.AppendLine($" - {g.Name}");
    sb.AppendLine();
    sb.AppendLine($"Преподавателей: {Lectors.Count}");
    foreach (var l in Lectors.Take(10)) sb.AppendLine($" - {l.FullName}");
    sb.AppendLine();
    sb.AppendLine($"Аудиторий: {Rooms.Count}");
    foreach (var r in Rooms.Take(10)) sb.AppendLine($" - {r.FullNumber} ({r.Type})");

    DebugText = sb.ToString();

    SelectedGroup = Groups.FirstOrDefault();

    try {
      Generate();
    } catch {
      StatisticsText = "Ошибка при генерации расписания";
    }
  }

  private void Generate() {
    List<Lesson> lessons = new();
    Random rnd = new();
    Subject[] subjects = {
        new("Математика"), new("Физика"), new("Программирование"),
        new("История"), new("Экономика"), new("Базы данных")
    };

    // Генерируем по 10 занятий на группу
    foreach (var group in Groups) {
      for (int i = 0; i < 10; i++) {
        var lector = Lectors[rnd.Next(Lectors.Count)];
        var subject = subjects[rnd.Next(subjects.Length)];
        uint duration = (uint)(rnd.Next(1, 3)); // 1 или 2 часа
        var roomType = (RoomType)rnd.Next(1, 4); // RoomType: 1, 2, 3
        lessons.Add(new Lesson(subject, group, lector, duration, roomType) { ID = Guid.NewGuid() });
      }
    }

    var result = _scheduler.GenerateScheduledLessons(lessons, Rooms.ToList(), TimeSlots.ToList());
    _allScheduledLessons = new ObservableCollection<ScheduledLesson>(result.ScheduledLessons);

    UpdateDisplay();
    UpdateStatistics();
  }

  private void Optimize() {
    var scheduleList = _allScheduledLessons.ToList();
    _optimizer.Optimize(scheduleList, Rooms.ToList(), TimeSlots.ToList());
    _allScheduledLessons = new ObservableCollection<ScheduledLesson>(scheduleList);
    UpdateDisplay();
    UpdateStatistics();
  }

  private void Clear() {
    _allScheduledLessons.Clear();
    UpdateDisplay();
    UpdateStatistics();
  }

  private void UpdateDisplay() {
    IEnumerable<ScheduledLesson> filtered = _allScheduledLessons;

    if (SelectedViewMode == ViewMode.Group && SelectedGroup != null)
      filtered = _allScheduledLessons.Where(l => l.Lesson.Group.ID == SelectedGroup.ID);
    else if (SelectedViewMode == ViewMode.Lector && SelectedLector != null)
      filtered = _allScheduledLessons.Where(l => l.Lesson.Lector.ID == SelectedLector.ID);
    else if (SelectedViewMode == ViewMode.Room && SelectedRoom != null)
      filtered = _allScheduledLessons.Where(l => l.Room.ID == SelectedRoom.ID);

    DisplayLessons = new ObservableCollection<ScheduledLesson>(filtered);
  }

  private void UpdateStatistics() {
    if (_allScheduledLessons.Count == 0) {
      StatisticsText = "Расписание пусто";
      return;
    }

    uint windows = _statisticsService.CalculateTotalWindows(_allScheduledLessons.ToList());
    var roomUtil = Rooms.Select(r => _statisticsService.GetRoomUtilization(r, _allScheduledLessons.ToList(), TimeSlots.Count)).Average();
    var workload = _statisticsService.GetLectorWorkload(_allScheduledLessons.ToList());
    var avgWorkload = workload.Values.Count > 0 ? workload.Values.Average() : 0;

    StatisticsText = $"Общее количество окон: {windows}\n" +
                     $"Средняя загрузка аудиторий: {roomUtil:F1}%\n" +
                     $"Средняя нагрузка преп.: {avgWorkload:F1} ч.";
  }
}
