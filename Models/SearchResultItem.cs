using TrackTrek.Avalonia.ViewModels;

namespace TrackTrek.Avalonia.Models;

public sealed class SearchResultItem : ViewModelBase
{
    private string _title = string.Empty;
    private string _artist = string.Empty;
    private string _thumbnailUrl = string.Empty;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Artist
    {
        get => _artist;
        set => SetProperty(ref _artist, value);
    }

    public string ThumbnailUrl
    {
        get => _thumbnailUrl;
        set => SetProperty(ref _thumbnailUrl, value);
    }
}