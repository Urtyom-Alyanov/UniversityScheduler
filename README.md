<div align="center">

# Планировщик университетского расписание

![GitHub branch status](https://img.shields.io/github/check-runs/Urtyom-Alyanov/UniversityScheduler/master?style=for-the-badge&logo=githubactions&logoColor=FFFFFF)
![GitHub repo size](https://img.shields.io/github/repo-size/Urtyom-Alyanov/UniversityScheduler?style=for-the-badge&logo=github&logoColor=FFFFFF)

![GitHub top language](https://img.shields.io/github/languages/top/Urtyom-Alyanov/UniversityScheduler?style=for-the-badge&logo=dotnet&color=8B00FF&logoColor=FFFFFF)
![Last Commit](https://img.shields.io/github/last-commit/Urtyom-Alyanov/UniversityScheduler?style=for-the-badge&logo=git&logoColor=FFFFFF)

В условиях современного высшего образования эффективное управление ограниченными ресурсами — аудиторным фондом, временем преподавательского состава и учебной нагрузкой студентов — становится сложной логистической задачей. Традиционные методы ручного составления расписания для десятков учебных групп и сотен академических часов сопряжены с высоким риском возникновения конфликтов и нерациональным использованием пространства. Автоматизация этого процесса позволяет не только исключить пересечения занятий, но и значительно повысить эргономичность образовательной среды за счёт минимизации временных разрывов («окон»).

</div>

### Технологический стек

| Инструмент      | Версия | Для чего?                                                |
|:----------------|:------:|:---------------------------------------------------------|
| C#              |   14   | Язык программирования                                    |
| .NET            |   10   | Платформа и среда исполнения                             |
| WPF             |   -    | Фреймворк для графического пользовательского интерфейса  |
| xUnit           | 3.2.2  | Фреймворк для модульного тестирования                    |
| coverlet        | 6.0.4  | Сборщик отчёта по покрытию тестами                       |
| ReportGenerator | 5.4.1  | Конвертация отчёта по покрытию в человекочитаемый формат |

## Архитектура проекта

- `Models` - уровень домена, здесь содержатся все типы сущностей
  - `Group` - студенческая группа
  - `Lector` - преподаватель
  - `Lesson` и `ScheduledLesson` - занятие и спланированное занятие
  - `Room` и `RoomType` - аудитория и её тип
  - `Subject` - предмет
  - `TimeSlot` - Временной слот
- `Services` - уровень логики, здесь происходит всё взаимодействие с доменами
  - `ConflictService` - служба, ответственная за поиск конфликтов
  - `OptimizationService` - служба, ответственная за оптимизацию расписания, то есть за избавление от "окон" в расписании,
  - `StatisticsService` - статистика по "окнам", загруженности и т.д.
  - `TopologicalSortService` - топологическая сортировка (DAG).
  - `ScheduleService` - сервис, ответственный за составление расписания.
- `UI` - представление, он же графический интерфейс.


### UML блок-схема
Для большей наглядности, ниже представлена UML диаграма самого проекта.

```mermaid
classDiagram
    direction TB

    namespace Models {
        class Group {
            +Guid ID
            +char stage
            +string specialty
            +string subspecialty
            +string year
            +string number
            +string Name
        }
        class Lector {
            +Guid ID
            +string lastName
            +string firstName
            +string middleName
            +string FullName
            +string LastNameWithAliases
        }
        class Subject {
            +Guid ID
            +string Name
        }
        class Room {
            +Guid ID
            +RoomType Type
            +string building
            +uint stage
            +uint number
            +string FullNumber
        }
        class RoomType {
            <<enumeration>>
            Lecture
            Laboratory
            Computer
        }
        class TimeSlot {
            <<record>>
            +DayOfWeek Day
            +uint StartHour
        }
        class Lesson {
            +Guid ID
            +Subject Subject
            +Group Group
            +Lector Lector
            +uint Duration
            +RoomType RequiredRoomType
            +List~Guid~ Prerequisites
        }
        class ScheduledLesson {
            +Lesson Lesson
            +TimeSlot Slot
            +Room Room
            +uint EndTime
        }
    }

    namespace Services {
        class IConflictService {
            <<interface>>
            +bool HasConflict(...)
            +IEnumerable~ConflictMessage~ GetConflictMessages(...)
        }
        class ConflictService {
        }
        class TopologicalSortService {
            +List~Lesson~ Sort(List~Lesson~ lessons)
        }
        class ScheduleService {
            +ScheduleResult GenerateScheduledLessons(...)
        }
        class OptimizationService {
            +void Optimize(...)
        }
        class StatisticsService {
            +double GetRoomUtilization(...)
            +Dictionary~string, long~ GetLectorWorkload(...)
            +uint CalculateTotalWindows(...)
        }
    }

    namespace ViewModels {
        class ViewModelBase {
            <<abstract>>
            +event PropertyChanged
        }
        class MainWindowsViewModel {
            +ObservableCollection~Group~ Groups
            +ObservableCollection~Lector~ Lectors
            +ObservableCollection~Room~ Rooms
            +ObservableCollection~ScheduledLesson~ DisplayLessons
            +ICommand GenerateCommand
            +ICommand OptimizeCommand
            +void Generate()
            +void Optimize()
        }
        class RelayCommand {
            <<interface>>
            ICommand
        }
    }

    %% Relationships
    Lesson "1" --> "1" Subject
    Lesson "1" --> "1" Group
    Lesson "1" --> "1" Lector
    Lesson "1" --> "1" RoomType : requires

    ScheduledLesson "1" --> "1" Lesson
    ScheduledLesson "1" --> "1" TimeSlot
    ScheduledLesson "1" --> "1" Room

    Room "1" --> "1" RoomType

    ConflictService ..|> IConflictService

    ScheduleService --> IConflictService : uses
    ScheduleService --> TopologicalSortService : uses
    OptimizationService --> IConflictService : uses

    MainWindowsViewModel --|> ViewModelBase
    MainWindowsViewModel --> ScheduleService : depends
    MainWindowsViewModel --> OptimizationService : depends
    MainWindowsViewModel --> StatisticsService : depends
    MainWindowsViewModel --> IConflictService : depends

    MainWindowsViewModel "1" o-- "*" ScheduledLesson : displays
    MainWindowsViewModel "1" o-- "*" Group
    MainWindowsViewModel "1" o-- "*" Lector
    MainWindowsViewModel "1" o-- "*" Room
```

## Развёртка

### Скачать уже собранный проект
Отчёт по покрытию, сам отчёт по курсовому проекту и программа лежат во вкладке [Releases](https://github.com/tstu-artemos-projects/BaseOfProgLabs/releases)
(автоматически  собирается и тестируется)

### Сборка из исходного кода

1. Установите [.NET отсюда](https://dotnet.microsoft.com/en-us/download), а также [Git отсюда](https://git-scm.com/install).
2. Склонируйте репозиторий
  ```shell
    git clone https://github.com/Urtyom-Alyanov/UniversityScheduler.git
  ```
3. В директории проекта соберите и запустите его
    ```shell
      dotnet run
    ```
