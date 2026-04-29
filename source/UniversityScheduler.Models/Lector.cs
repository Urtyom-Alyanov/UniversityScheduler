namespace UniversityScheduler.Models;

/// <summary>
/// Преподаватель
/// </summary>
/// <param name="lastName">Фамилия</param>
/// <param name="firstName">Имя</param>
/// <param name="middleName">Отчество</param>
public class Lector(string lastName, string firstName, string middleName) {
  public Guid ID { get; set; }

  public string FullName => $"{lastName} {firstName} {middleName}";
  public string LastNameWithAliases => $"{lastName} {firstName[0]}. {middleName[0]}.";
}
