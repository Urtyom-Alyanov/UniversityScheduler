using System.Windows;
using UniversityScheduler.UI.ViewModels;

namespace UniversityScheduler.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}