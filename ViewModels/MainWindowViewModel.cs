using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TrackTrek.Audio;
using TrackTrek.Avalonia.Helpers;
using TrackTrek.Avalonia.Miscs;
using TrackTrek.Avalonia.Models;
using TrackTrek.Miscs;
using static TrackTrek.Miscs.Searching;

namespace TrackTrek.Avalonia.ViewModels;

public sealed class MainWindowViewModel : ViewModelBase
{
    private string _searchText = string.Empty;
    private bool _isDebug;
    public static MainWindowViewModel? Instance { get; private set; }
    private int _maxResults = 25;
    private string _downloadFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
    private double _globalProgress;
    private string _globalStatus = "Ready.";
    private SearchResultItem? _selectedResult;
    private HistoryItem? _selectedHistory;

    public MainWindowViewModel()
    {
        Sys.initialize();
        Instance = this;

        SearchCommand = new AsyncRelayCommand(PerformSearchAsync);
        QueueDownloadCommand = new RelayCommand(ExecuteQueueDownload, p => p is SearchResultItem);

        //SeedUiWithDemoData();
    }


    public ObservableCollection<SearchResultItem> SearchResults { get; } = new();
    public ObservableCollection<HistoryItem> HistoryItems { get; } = new();

    public ICommand SearchCommand { get; }
    public ICommand QueueDownloadCommand { get; }

    /// <summary>
    /// Plug your own search logic here.
    /// </summary>
    /// 

    private async Task SearchHandler()
    {
        SearchResults.Clear();
        string query = SearchText;

        string videoType = query.CheckStringType();

        // need to check if its Search or Download (for playlist)

        // "link" "playlist" "keyword"
        switch (videoType)
        {
            case "link":
                Sys.debug("Identified type \"Youtube Video Link\" to be the query");

                GlobalStatus = "Downloading audio from Youtube link...";
                GlobalProgress = 0;
                Sys.debug($"Downloading audio...");

                VideoInfo audioOutput = await Download.DownloadAudio(SearchText);

                Sys.debug($"Audio downloaded!: {audioOutput.Path}");
                GlobalProgress = 50;

                VideoInfo musicUpdatedInfo = await Searching.FetchAndUpdateWithItune(audioOutput);
                VideoInfo musicUpdatedInfoFinal;
                GlobalStatus = $"Detected {musicUpdatedInfo.Title} by {musicUpdatedInfo.GetArtist()}";

                if (musicUpdatedInfo.Album == "Youtube")
                {
                    musicUpdatedInfo.Lyrics = "No lyrics found";
                    if (musicUpdatedInfo.Title.Contains(" - "))
                    {
                        musicUpdatedInfo.Artist = musicUpdatedInfo.Title.ToArtistDashTitle(musicUpdatedInfo.GetArtist()).Split(" - ")[0];
                        musicUpdatedInfo.Title = musicUpdatedInfo.Title.ToArtistDashTitle(musicUpdatedInfo.GetArtist()).Split(" - ")[1];
                    }
                    musicUpdatedInfoFinal = musicUpdatedInfo;
                    Sys.debug("Couldn't find info from itune, trying to search image and update info with it");
                }
                else
                {
                    VideoInfo musicUpdatedInfoImage = await Searching.SearchImage(musicUpdatedInfo);

                    musicUpdatedInfoFinal = await SearchAndUpdateLyrics(musicUpdatedInfoImage);

                    Sys.debug("Lyrics added!");
                }
                GlobalProgress = 70;

                Sys.debug($"Video Infos:\n  Path:{musicUpdatedInfoFinal.Path}\n  Title:{musicUpdatedInfoFinal.Title}\n  Artist: {musicUpdatedInfoFinal.GetArtist()}\n  Album: {musicUpdatedInfoFinal.Album}\n  Album Image Found: {musicUpdatedInfoFinal.AlbumImageUrl != ""}");

                await CustomMetaData.Add(musicUpdatedInfo);

                GlobalProgress = 100;
                GlobalStatus = "Downloaded";

                break;
            case "playlist":
                GlobalStatus = "Unsupported, please download the latest beta instead of the current alpha";
                // same as link but foreach, not same for spotify
                break;
            case "keyword":
                // YOUR EXISTING SEARCH CODE HERE
                /*
                foreach (var song in results)
                {
                    SearchResults.Add(new SearchResultItem
                    {
                        Title = song.Title,
                        Artist = song.Artist,
                        ThumbnailUrl = song.Thumbnail
                    });
                }
                */
                // MessageBox.Show("Unsupported, please download the latest release instead of the current alpha");
                // first search itune, get info, get lyrics and all, then at the end search audio

                // await Searching.SearchOnYoutubeMusic(this.Parent.Controls.OfType<SearchBar>().FirstOrDefault().Text);

                // also need to update the double click in button list
                break;
            case "spotify":
                GlobalStatus = "Unsupported, this feature will only be available in the future";
                // temporary
                // use https://open.spotify.com/oembed?url=playlisturlhere
                break;
            default:
                GlobalStatus = "Please enter something in the typing box";
                break;
        }
    }

    /// <summary>
    /// Plug your own existing download/queue logic here.
    /// </summary>
    public Action<SearchResultItem, string>? DownloadHandler { get; set; }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }


    public double GlobalProgress
    {
        get => _globalProgress;
        set => SetProperty(ref _globalProgress, value);
    }

    public string GlobalStatus
    {
        get => _globalStatus;
        set => SetProperty(ref _globalStatus, value);
    }

    public SearchResultItem? SelectedResult
    {
        get => _selectedResult;
        set => SetProperty(ref _selectedResult, value);
    }

    public HistoryItem? SelectedHistory
    {
        get => _selectedHistory;
        set => SetProperty(ref _selectedHistory, value);
    }

    private async Task PerformSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            GlobalStatus = "Enter something to search.";
            return;
        }

        SearchResults.Clear();


        await SearchHandler();
        /*
        if (SearchHandler is not null)
        {
            var results = await SearchHandler(SearchText, MaxResults);
            foreach (var item in results)
                SearchResults.Add(item);

            GlobalStatus = $"Found {SearchResults.Count} result(s).";
            return;
        }

        // Demo UI-only fallback
        var count = Math.Min(MaxResults, 10);
        for (var i = 1; i <= count; i++)
        {
            SearchResults.Add(new SearchResultItem
            {
                Title = $"{SearchText} - Track {i}",
                Artist = $"Artist {i}",
                ThumbnailUrl = string.Empty
            });
        }

        GlobalStatus = $"Demo mode: generated {SearchResults.Count} result(s).";
        await Task.CompletedTask;
        */
    }

    private void ExecuteQueueDownload(object? parameter)
    {
        if (parameter is not SearchResultItem item)
            return;

        DownloadHandler?.Invoke(item, AppSettings.DownloadFolder);

        var fileName = $"{SanitizeFileName(item.Title)}.mp3";
        var path = Path.Combine(AppSettings.DownloadFolder, fileName);

        HistoryItems.Insert(0, new HistoryItem
        {
            DownloadPath = path,
            Status = "Queued"
        });

        GlobalStatus = $"Queued: {item.Title}";
    }

    public void SetProgress(double progress, string? status = null)
    {
        GlobalProgress = Math.Clamp(progress, 0, 100);
        if (!string.IsNullOrWhiteSpace(status))
            GlobalStatus = status;
    }

    public void AddHistory(string path, string status)
    {
        HistoryItems.Insert(0, new HistoryItem
        {
            DownloadPath = path,
            Status = status
        });
    }

    public void UpdateLatestHistoryStatus(string status)
    {
        if (HistoryItems.Any())
            HistoryItems[0].Status = status;
    }

    private void SeedUiWithDemoData()
    {
        SearchResults.Add(new SearchResultItem { Title = "Kill the Noise", Artist = "Papa Roach" });
        SearchResults.Add(new SearchResultItem { Title = "Scars", Artist = "Papa Roach" });
        SearchResults.Add(new SearchResultItem { Title = "Born for Greatness", Artist = "Papa Roach" });

        HistoryItems.Add(new HistoryItem
        {
            DownloadPath = Path.Combine(AppSettings.DownloadFolder, "Kill the Noise.mp3"),
            Status = "Completed"
        });

        HistoryItems.Add(new HistoryItem
        {
            DownloadPath = Path.Combine(AppSettings.DownloadFolder, "Scars.mp3"),
            Status = "Queued"
        });

        GlobalProgress = 42;
        GlobalStatus = "Waiting for next download...";
    }

    private static string SanitizeFileName(string input)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(input.Select(ch => invalid.Contains(ch) ? '_' : ch));
    }
}