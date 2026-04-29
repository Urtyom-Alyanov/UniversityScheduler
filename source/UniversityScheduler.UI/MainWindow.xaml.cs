using System.Windows;
using System.Windows.Controls;
using UniversityScheduler.Models;
using UniversityScheduler.Services;
using UniversityScheduler.UI.ViewModels;

namespace UniversityScheduler.UI;

public partial class MainWindow : Window
{
    private MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }

    private void ScheduleGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is DataGrid grid && grid.SelectedItem is ScheduleRow row)
        {
            // Find which column was selected
            if (grid.CurrentColumn != null)
            {
                int dayIndex = grid.CurrentColumn.Header switch
                {
                    "Пн" => 0,
                    "Вт" => 1,
                    "Ср" => 2,
                    "Чт" => 3,
                    "Пт" => 4,
                    _ => -1
                };

                if (dayIndex >= 0 && dayIndex < row.Count)
                {
                    var cell = row[dayIndex]; // dayIndex 0=Пн, 1=Вт, 2=Ср, 3=Чт, 4=Пт
                    if (cell?.Session != null)
                    {
                        MessageBox.Show(
                            $"Редактирование:\nПредмет: {cell.Session.Subject}\n" +
                            $"Время: {SchedulerEngine.FormatTimeSlot(cell.Session.TimeSlot, cell.Session.Duration)}\n" +
                            $"Аудитория: {cell.Session.Room?.Number}\n" +
                            $"Группа: {cell.Session.Group?.Name}\n" +
                            $"Преподаватель: {cell.Session.Lector?.Name}",
                            "Информация о занятии");
                    }
                }
            }
        }
    }
}
