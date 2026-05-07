namespace UniversityScheduler.Models;

/// <summary>
/// Университетская группа
/// </summary>
/// <param name="stage">Первая буква ступени образования (бакалавриат, магистратура и тд.)</param>
/// <param name="specialty">Специальность</param>
/// <param name="subspecialty">Подспециальность</param>
/// <param name="year">Год собрания группы</param>
/// <param name="number">Номер группы</param>
public class Group(
    char stage,
    string specialty,
    string subspecialty,
    string year,
    string number) {
  public Guid ID { get; set; } = Guid.NewGuid();

  /// <summary>
  /// Полное имя группы
  /// </summary>
  public string Name => $"{stage}.{specialty}.{subspecialty}.{year}.{number}";
}
