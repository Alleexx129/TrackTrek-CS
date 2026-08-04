using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using TrackTrek.Avalonia.Models;
using TrackTrek.Avalonia.ViewModels;
using TrackTrek.Avalonia.Views;

namespace TrackTrek.Avalonia;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var vm = new MainWindowViewModel();


            // 2) Hook your existing download code
            vm.DownloadHandler = (item, folder) =>
            {
                // Replace this with YOUR real queue/download call
                // Example:
                // myDownloader.Queue(item.Url, folder);

                vm.AddHistory(System.IO.Path.Combine(folder, item.Title + ".mp3"), "Queued");
            };

            var window = new MainWindow
            {
                DataContext = vm
            };

            desktop.MainWindow = window;
        }

        base.OnFrameworkInitializationCompleted();
    }
}