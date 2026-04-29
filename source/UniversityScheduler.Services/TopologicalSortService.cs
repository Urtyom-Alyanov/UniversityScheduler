using UniversityScheduler.Models;

namespace UniversityScheduler.Services;

/// <summary>
/// Сервис топологической сортировки (Ориентированный ациклический граф).
///
/// Каждое занятие является вершиной графа.
/// </summary>
public class TopologicalSortService
{
    /// <summary>
    /// Отсортировать занятия с помощью топологической сортировки
    /// </summary>
    /// <param name="lessons">Занятия</param>
    /// <returns>Отсортированные занятия</returns>
    /// <exception cref="ArgumentNullException">Не переданы занятия</exception>
    /// <exception cref="InvalidOperationException">Есть цикл в зависимостях</exception>
    public static List<Lesson> Sort(List<Lesson> lessons)
    {
        if (lessons == null) throw new ArgumentNullException(nameof(lessons));
        
        var result = new List<Lesson>();
        var visited = new Dictionary<Guid, bool>();
        var stack = new HashSet<Guid>();

        foreach (var lesson in lessons)
        {
            if (!visited.ContainsKey(lesson.ID))
                Visit(lesson, lessons, visited, stack, result);
        }

        result.Reverse();
        return result;
    }
    
    private static void Visit(Lesson lesson, List<Lesson> all, Dictionary<Guid, bool> visited, HashSet<Guid> stack, List<Lesson> result)
    {
        if (stack.Contains(lesson.ID))
            throw new InvalidOperationException("Обнаружен цикл в зависимостях предметов!");

        if (!visited.ContainsKey(lesson.ID))
        {
            stack.Add(lesson.ID);
            foreach (var preId in lesson.Prerequisites)
            {
                var preLesson = all.FirstOrDefault(l => l.ID == preId);
                if (preLesson != null)
                    Visit(preLesson, all, visited, stack, result);
            }
            visited[lesson.ID] = true;
            stack.Remove(lesson.ID);
            result.Add(lesson);
        }
    }
}