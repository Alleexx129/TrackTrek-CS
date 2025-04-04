using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FuzzySharp;
using TrackTrek.Audio;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace TrackTrek.Miscs
{
    internal class Searching
    {
        public static string CheckIfLink(string text)
        {
            string ctype = "";
            if (text.ToLower().Contains("https") || text.ToLower().Contains("youtube.com") || text.ToLower().Contains("youtu.be"))
            {
                ctype = "link";

                if (text.ToLower().Contains("?list="))
                {
                    ctype = "playlist";
                }
            }

            return ctype;
        }

        public static async Task<Video> GetVideo(string query)
        {
            YoutubeClient youtube = new YoutubeClient();
            ISearchResult found = null;

            await foreach(var video in youtube.Search.GetResultsAsync(query)) {
                if (!Filter.BlacklistedVideo(video.Title))
                {
                    found = video;
                    break;
                }
            }
            return await youtube.Videos.GetAsync(found.Url);
        }
        
        public class VideoInfo
        {
            public VideoInfo()
            {
                this.Title = string.Empty;
                this.Album = string.Empty;
                this.Artist = string.Empty;
                this.AlbumImage = new byte[0];
            }
            public string Title { get; set; }
            public string Artist { get; set; }
            public byte[] AlbumImage { get; set; }
            public string Album { get; set; }
        }

        public static async Task<VideoInfo> FetchVideoInfos(string title, string uploader, byte[] thumbnail)
        {
            VideoInfo newVideoInfo = new VideoInfo{ Title = "", Artist = "", AlbumImage = thumbnail, Album = "Unknown", };

            string[] filteredUploaderAndTitle = Filter.ToTitleAndArtist(title, uploader);

            newVideoInfo.Title = filteredUploaderAndTitle[0];
            newVideoInfo.Artist = filteredUploaderAndTitle[1];

            HttpClient client = new HttpClient();

            HttpResponseMessage httpResponse = await client.GetAsync($"https://itunes.apple.com/search?term={newVideoInfo.Title} by {newVideoInfo.Artist}&entity=song");
            while (!httpResponse.IsSuccessStatusCode)
            {
                httpResponse = await client.GetAsync($"https://itunes.apple.com/search?term={newVideoInfo.Title} by {newVideoInfo.Artist}&entity=song");
            }
            string response = await httpResponse.Content.ReadAsStringAsync();
            dynamic responseJson = JsonNode.Parse(response)["results"];

            foreach (JsonObject item in responseJson)
            {
                if (Filter.BlacklistedVideo(item["trackName"].ToString()))
                {
                    continue;
                }

                Sys.debug(Fuzz.PartialRatio(Filter.FilterArtistName(item["artistName"].ToString()), newVideoInfo.Artist.ToLower()).ToString());
                if (Fuzz.PartialRatio(Filter.FilterArtistName(item["artistName"].ToString()), newVideoInfo.Artist.ToLower()) >= 45);
                {
                    newVideoInfo.Album = item["collectionName"].ToString();
                    newVideoInfo.Title = item["trackName"].ToString();
                    newVideoInfo.Artist = item["artistName"].ToString();
                    string albumImage = await ImageUtils.GetAlbumImageUrl(newVideoInfo.Album, newVideoInfo.Artist);

                    newVideoInfo.AlbumImage = await CustomMetaData.DownloadThumbnailAsBytes(albumImage);

                    break;
                }
            }

            return newVideoInfo;
        }
        public static async Task<List<VideoInfo>> GetPlaylistVideos(string playlistUrl)
        {
            YoutubeClient youtube = new YoutubeClient();
            List<VideoInfo> videoInfos = new List<VideoInfo> { };
            var videos = await youtube.Playlists.GetVideosAsync(playlistUrl);

            await foreach (var video in youtube.Playlists.GetVideosAsync(playlistUrl))
            {
                string title = video.Title;
                string author = video.Author.ToString();
                byte[] imageByte = await CustomMetaData.DownloadThumbnailAsBytes(video.Thumbnails[video.Thumbnails.Count - 1].Url);
                byte[] resizedImage = ImageUtils.ResizeImage(imageByte);

                VideoInfo videoInfo = await FetchVideoInfos(title, author, resizedImage);
                if (videoInfo.Artist != string.Empty)
                {
                    videoInfos.Add(videoInfo);
                }
                Sys.debug($"Artist: {videoInfo.Artist} Title: {videoInfo.Title} Album: {videoInfo.Album}");
            }

            return videoInfos;
        }
    }   
    
    
}
