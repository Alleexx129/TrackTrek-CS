namespace TrackTrek.Avalonia.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private bool _isDebug;
    private int _maxResults;
    private string _downloadFolder = string.Empty;

    public bool IsDebug
    {
        get => _isDebug;
        set => SetProperty(ref _isDebug, value);
    }

    public int MaxResults
    {
        get => _maxResults;
        set => SetProperty(ref _maxResults, value);
    }

    public string DownloadFolder
    {
        get => _downloadFolder;
        set => SetProperty(ref _downloadFolder, value);
    }
}