using TrackTrek.Avalonia.ViewModels;

namespace TrackTrek.Avalonia.Models;

public sealed class HistoryItem : ViewModelBase
{
    private string _downloadPath = string.Empty;
    private string _status = string.Empty;

    public string DownloadPath
    {
        get => _downloadPath;
        set => SetProperty(ref _downloadPath, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }
}