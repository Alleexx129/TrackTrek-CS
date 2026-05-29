using FuzzySharp;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TrackTrek.Audio;
using static System.Windows.Forms.LinkLabel;

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

        //if (bestTitle != "")
        //{
        //videoInfo.Title = bestTitle;
        //videoInfo.Artist = bestAuthor.ToCapitalFirst();
        //videoInfo.Album = bestAlbum;
        //} else
        //{
        //videoInfo.Album = "Youtube";
        //}
        //return videoInfo;
        //}
        public static async Task<VideoInfo> GetVideo(string title, string artist, string album = "Youtube")
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp.exe",
                Arguments = $"""ytsearch5:"system of a down sugar" --get-id --get-title """,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = false,
            };

            using (var process = new System.Diagnostics.Process { StartInfo = processStartInfo })
            {
                //process.Start();
                //await process.WaitForExitAsync();

                //var errorTask = process.StandardError.ReadToEndAsync();
                //MessageBox.Show(errorTask.Result);
                //var outputTask = process.StandardOutput.ReadToEndAsync();
                process.Start();
                await process.WaitForExitAsync();


                var error = await process.StandardError.ReadToEndAsync();
                var output = await process.StandardOutput.ReadToEndAsync();

                Sys.debug("Current output errors, these may be normal...\n" + error);
                Sys.debug("Output from console: \n" + output);


                MessageBox.Show(output);
                MessageBox.Show(error);
                return new VideoInfo();
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
                this.YoutubeImageUrl = string.Empty;
                this.AlbumImage = new byte[0];
                this.Path = string.Empty;
            }
            public string Title { get; set; }

            public string AlbumImageUrl { get; set; }
            public string YoutubeImageUrl { get; set; }
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
