using Avalonia.Controls;
using Avalonia.Interactivity;
using TrackTrek.Avalonia.Miscs;
using TrackTrek.Avalonia.ViewModels;

namespace TrackTrek.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void OpenSettings_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel vm)
            return;

        var settingsVm = new SettingsViewModel
        {
            IsDebug = AppSettings.Debug,
            MaxResults = AppSettings.MaxResults,
            DownloadFolder = AppSettings.DownloadFolder
        };

        var window = new SettingsWindow
        {
            DataContext = settingsVm
        };

        var result = await window.ShowDialog<bool?>(this);

        if (result == true)
        {
            AppSettings.Debug = settingsVm.IsDebug;
            AppSettings.MaxResults = settingsVm.MaxResults;
            AppSettings.DownloadFolder = settingsVm.DownloadFolder;
            vm.GlobalStatus = "Settings updated.";
        }
    }
}