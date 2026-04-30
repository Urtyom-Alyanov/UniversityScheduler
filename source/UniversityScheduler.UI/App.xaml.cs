using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using UniversityScheduler.Services;
using UniversityScheduler.UI.ViewModels;

namespace UniversityScheduler.UI;

public partial class App : Application {
  private ServiceProvider? _serviceProvider;
  private static readonly string LogPath = "app_log.txt";

  public App() {
    try {
      File.AppendAllText(LogPath, $"[{DateTime.Now}] App constructor started\n");
      ServiceCollection services = new();
      ConfigureServices(services);
      _serviceProvider = services.BuildServiceProvider();
      File.AppendAllText(LogPath, $"[{DateTime.Now}] DI built successfully\n");
    } catch (Exception ex) {
      File.AppendAllText(LogPath, $"[{DateTime.Now}] ERROR in constructor: {ex.Message}\n{ex.StackTrace}\n");
      MessageBox.Show($"Ошибка при создании DI: {ex.Message}\n\n{ex.InnerException?.Message}",
        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
      Shutdown();
    }
  }

  private void ConfigureServices(ServiceCollection services) {
    services.AddSingleton<IConflictService, ConflictService>();
    services.AddSingleton<TopologicalSortService>();
    services.AddSingleton<ScheduleService>();
    services.AddSingleton<OptimizationService>();
    services.AddSingleton<StatisticsService>();

    services.AddSingleton<MainWindowsViewModel>();
    services.AddSingleton<MainWindow>(s => {
      File.AppendAllText(LogPath, $"[{DateTime.Now}] Creating MainWindow with DataContext\n");
      return new MainWindow {
        DataContext = s.GetRequiredService<MainWindowsViewModel>()
      };
    });
  }

  protected override void OnStartup(StartupEventArgs e) {
    try {
      File.AppendAllText(LogPath, $"[{DateTime.Now}] OnStartup called\n");
      var mainWindow = _serviceProvider?.GetRequiredService<MainWindow>()
        ?? throw new InvalidOperationException("ServiceProvider не инициализирован");
      File.AppendAllText(LogPath, $"[{DateTime.Now}] MainWindow resolved, initialising...\n");
      mainWindow.InitializeComponent();
      File.AppendAllText(LogPath, $"[{DateTime.Now}] MainWindow initialized, showing...\n");
      mainWindow.Show();
      File.AppendAllText(LogPath, $"[{DateTime.Now}] MainWindow shown\n");
      base.OnStartup(e);
    } catch (Exception ex) {
      File.AppendAllText(LogPath, $"[{DateTime.Now}] ERROR in OnStartup: {ex.Message}\n{ex.StackTrace}\n");
      MessageBox.Show($"Ошибка при запуске: {ex.Message}\n\n{ex.InnerException?.Message}",
        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
      Shutdown();
    }
  }
}
