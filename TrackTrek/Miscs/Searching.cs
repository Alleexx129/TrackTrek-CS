using FuzzySharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Frozen;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TrackTrek.Audio;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using static MediaToolkit.Model.Metadata;

// clean this sometime soon

namespace TrackTrek.Miscs
{
    internal class Searching
    {

        public static async Task<VideoInfo> FetchAndUpdateWithItune(VideoInfo videoInfo) // All infos are filtered, requires title and artist (using for yt searching)
        {
            HttpClient client = new HttpClient();

            HttpResponseMessage httpResponse = await client.GetAsync($"https://itunes.apple.com/search?term={videoInfo.Title} by {videoInfo.GetArtist()}&entity=song");
            while (!httpResponse.IsSuccessStatusCode)
            {
                await Task.Delay(1500);
                httpResponse = await client.GetAsync($"https://itunes.apple.com/search?term={videoInfo.Title} by {videoInfo.GetArtist()}&entity=song");
            }

            string response = await httpResponse.Content.ReadAsStringAsync();
            dynamic responseJson = JsonNode.Parse(response)["results"];

            string bestTitle = "";
            sbyte bestTitleConfidence = 0;
            string bestAuthor = "";
            sbyte bestAuthorConfidence = 0;
            string bestAlbum = "";

            foreach (JsonObject item in responseJson)
            {
                if (Filter.BlacklistedVideo(item["trackName"].ToString()))
                {
                    Sys.debug("Skipped");
                    continue;
                }
                string currentTitle = Filter.FilterTitle(item["trackName"].ToString());
                string currentArtist = Filter.FilterArtist(item["artistName"].ToString());
                string currentAlbum = Filter.FilterAlbum(item["collectionName"].ToString());

                string[] titleAndArtist = Filter.ToTitleAndArtist(videoInfo.Title, videoInfo.GetArtist());

                int currentTitleConfidence = Fuzz.Ratio(currentTitle.ToString(), titleAndArtist[0]);
                int currentAuthorConfidence = Fuzz.Ratio(currentArtist.ToString(), titleAndArtist[1]);

                if (currentTitleConfidence < Fuzz.Ratio(currentTitle.ToString(), titleAndArtist[1]))
                {
                    currentTitleConfidence = Fuzz.Ratio(currentTitle.ToString(), titleAndArtist[1]);
                    currentAuthorConfidence = Fuzz.Ratio(currentArtist.ToString(), titleAndArtist[0]);
                }

                if (currentAuthorConfidence > bestAuthorConfidence && currentAuthorConfidence > 30) // 30 is minimum confidence
                {
                    Sys.debug($"Found new artist with confidence of {currentAuthorConfidence}%");
                    bestAuthorConfidence = (sbyte)currentAuthorConfidence;
                    bestAuthor = currentArtist;
                    bestTitleConfidence = (sbyte)currentTitleConfidence;
                    bestTitle = currentTitle;
                    bestAlbum = currentAlbum;
                    Sys.debug($"Title: {currentTitle} Confidence: {currentTitleConfidence}\nAlbum: {currentAlbum}");
                } else if (currentAuthorConfidence == bestAuthorConfidence && currentTitleConfidence > bestTitleConfidence && currentTitleConfidence > 30)
                {
                    Sys.debug($"Found new title with confidence of {currentTitleConfidence}%");
                    bestTitle = currentTitle;
                    bestAlbum = currentAlbum;
                    bestTitleConfidence = (sbyte)currentTitleConfidence;
                }
            }

            if (bestTitle != "")
            {
                videoInfo.Title = bestTitle;
                videoInfo.Artist = bestAuthor.ToCapitalFirst();
                videoInfo.Album = bestAlbum;
            } else
            {
                videoInfo.Album = "Youtube";
            }
            return videoInfo;
        }
        public static async Task<VideoInfo> SearchImage(VideoInfo videoInfo) // does not check if it is Youtube or not, requires album and artist
        {
            string albumImageUrl = await ImageUtils.GetAlbumImageUrl(videoInfo.Album, videoInfo.GetArtist());

            videoInfo.AlbumImageUrl = albumImageUrl;

            return videoInfo;
        }

        public static async Task<VideoInfo> SearchWithItune(VideoInfo videoInfo)
        {
            return videoInfo;
        }

        public static async Task<VideoInfo> SearchAudioOnYt(VideoInfo videoInfo)
        {
            return videoInfo;
        }

        public static async Task<VideoInfo> SearchAndUpdateLyrics(VideoInfo videoInfo)
        {
            string lyricsUrl = Lyrics.ToGeniusLink(videoInfo.Title, videoInfo.GetArtist());

            string lyrics = await Lyrics.GetLyrics(lyricsUrl);

            videoInfo.Lyrics = lyrics;
            return videoInfo; 
        }

        public static async Task<YoutubeExplode.Videos.Video> GetVideo(string title, string artist, string album)
        {
            YoutubeClient youtube = new YoutubeClient();
            VideoSearchResult? found = null;
            byte bestRatio = 0;
            byte index = 0;

            await foreach(VideoSearchResult video in youtube.Search.GetVideosAsync($"{title} / {artist}"))
            {
                VideoSearchResult foundVideo = video;
                VideoSearchResult foundVideo2 = video;

                if (foundVideo.Title.Contains(" - "))
                {
                    FrozenSet<string> dividedTitle = foundVideo.Title.Split(" - ").ToFrozenSet();
                    
                    foundVideo = new VideoSearchResult(video.Id, dividedTitle.Last().ToLower().Replace("("+album.ToLower()+")", ""), new Author(video.Author.ChannelId, dividedTitle.First().ToLower().Replace("("+album.ToLower()+")", "")), video.Duration, video.Thumbnails);
                    foundVideo2 = new VideoSearchResult(video.Id, dividedTitle.First().ToLower().Replace("(" + album.ToLower() + ")", ""), new Author(video.Author.ChannelId, dividedTitle.Last().ToLower().Replace("(" + album.ToLower() + ")", "")), video.Duration, video.Thumbnails);
                    
                }
                index += 1;

                byte titleRatio = (byte)(Fuzz.Ratio(title.ToLower(), foundVideo.Title.ToLower().Replace("("+album.ToLower()+")", "")));
                byte artistRatio = (byte)(Fuzz.Ratio(artist.ToLower(), foundVideo.Author.ToString().ToLower()));
                byte titleRatio2 = (byte)(Fuzz.Ratio(title.ToLower(), foundVideo2.Title.ToLower().Replace("(" + album.ToLower() + ")", "")));
                byte artistRatio2 = (byte)(Fuzz.Ratio(artist.ToLower(), foundVideo2.Author.ToString().ToLower()));

                if (!Filter.BlacklistedVideo(foundVideo.Title))
                {
                    Sys.debug(foundVideo.Title);
                    if (titleRatio >= 90 && artistRatio >= 30)
                    {
                        found = foundVideo;
                        Sys.debug("Found video url: " + found.Url + " Title: " + found.Title.ToString() + " Author: " + found.Author.ToString() + " with accuracy of " + titleRatio + "%");
                        break;
                    } else if (titleRatio2 >= 90 && artistRatio2 >= 30)
                    {
                        found = foundVideo2;
                        Sys.debug("Found video url: " + found.Url + " Title: " + found.Title.ToString() + " Author: " + found.Author.ToString() + " with accuracy of " + titleRatio2 + "%");
                        break;
                    }
                    else if (bestRatio < titleRatio && artistRatio >= 30)
                    {
                        found = foundVideo;
                        bestRatio = titleRatio;
                    } else if (bestRatio < titleRatio2 && artistRatio2 >= 30)
                    {
                        found = foundVideo;
                        bestRatio = titleRatio2;
                    }
                    if (index > 15)
                    {
                        if (found == null)
                        {
                            MessageBox.Show("Error no sound found in search results on yt");
                            break;
                        }
                        Sys.debug("Found video url:" + found.Url + " Title: " + found.Title.ToString() + " Author: " + found.Author.ToString() + " with accuracy of " + bestRatio + "%");
                        break;
                    }
                } else {  }
            }
            if (found != null)
            {
                return await youtube.Videos.GetAsync(found.Url);
            } else
            {
                MessageBox.Show("ERROR: AUDIO NOT FOUND ON YT");
                return new YoutubeExplode.Videos.Video(new VideoId("21"), "", new Author(new YoutubeExplode.Channels.ChannelId(), ""), new DateTimeOffset((long)1, new TimeSpan((long)3)), "", new TimeSpan(), (IReadOnlyList<Thumbnail>)(new List<Thumbnail>()), (IReadOnlyList<string>)(new List<string>()), new Engagement(67, 2, 2));
            }
        }
        
        public class VideoInfo
        {
            public VideoInfo()
            {
                this.Title = string.Empty;
                this.Album = string.Empty;
                this.YoutubeArtist = string.Empty;
                this.Artist = string.Empty;
                this.Lyrics = string.Empty;
                this.AlbumImageUrl = string.Empty;
                this.AlbumImage = new byte[0];
                this.Path = string.Empty;
            }
            public string Title { get; set; }

            public string AlbumImageUrl { get; set; }
            public string Lyrics { get; set; }

            public string Path { get; set; }
            public string Artist { get; set; }
            public string YoutubeArtist { get; set; }
            public byte[] AlbumImage { get; set; }
            public string Album { get; set; }

            public string GetArtist()
            {
                if (Artist == string.Empty)
                {
                    return YoutubeArtist;
                }
                return Artist;
            }
        }
        public static async Task<string> FetchVideoInfos(string title, string uploader, byte[] thumbnail, bool bypass = false)
        {
            return "";
        }
        /*
        public static async Task<List<VideoInfo>> GetPlaylistVideos(string playlistUrl)
        {
            YoutubeClient youtube = new YoutubeClient();
            List<VideoInfo> videoInfos = new List<VideoInfo> { };
            var videos = youtube.Playlists.GetVideosAsync(playlistUrl);
            int index = 0;
            var countTask = Task.Run(async () =>
            {
                var playlist = await youtube.Playlists.GetAsync(playlistUrl);
                return playlist.Count ?? 1;
            });

            await foreach (var video in videos)
            {
                index++;
                Form1.downloadProgress.Value = index * 100 / await countTask;
                string title = video.Title;
                string author = video.Author.ToString();
                byte[]? imageByte = null;
                byte[]? resizedImage = null;

                VideoInfo videoInfo = await FetchVideoInfos(title, author, resizedImage, true);
                if (videoInfo.Album == "Unknown" || videoInfo.Album == "Youtube" || videoInfo.Album == string.Empty)
                {
                    imageByte = await CustomMetaData.DownloadThumbnailAsBytes(video.Thumbnails[video.Thumbnails.Count - 1].Url);
                    resizedImage = ImageUtils.ResizeImage(imageByte);

                    videoInfo.Album = "Youtube";
                    videoInfo.AlbumImage = resizedImage;
                    videoInfo.Title = title;
                    videoInfo.Artist = author;
                }

                videoInfos.Add(videoInfo);
                Sys.debug($"Added to playlist infos: Artist: {videoInfo.Artist} Title: {videoInfo.Title} Album: {videoInfo.Album}");
            }

            return videoInfos;
        }
        */
    }


}
