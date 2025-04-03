using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace TrackTrek.Miscs
{
    internal class Searching
    {
        public static Boolean CheckIfLink(string text)
        {
            return text.ToLower().Contains("https") || text.ToLower().Contains("youtube.com") || text.ToLower().Contains("youtu.be");
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
                this.AlbumImage = string.Empty;
            }
            public string Title { get; set; }
            public string Artist { get; set; }
            public string AlbumImage { get; set; }
            public string Album { get; set; }
        }

        public static async Task<VideoInfo> FetchVideoInfos(string title, string uploader, string thumbnail)
        {
            VideoInfo newVideoInfo = new VideoInfo{ Title = "", Artist = "", AlbumImage = "", Album = thumbnail, };

            string[] filteredUploaderAndTitle = Filter.ToTitleAndArtist(title, uploader);

            newVideoInfo.Title = filteredUploaderAndTitle[0];
            newVideoInfo.Artist = filteredUploaderAndTitle[1];

            HttpClient client = new HttpClient();

            HttpResponseMessage httpResponse = await client.GetAsync($"https://itunes.apple.com/search?term={newVideoInfo.Title} by {newVideoInfo.Artist}&entity=song");
            string response = await httpResponse.Content.ReadAsStringAsync();
            dynamic responseJson = JsonNode.Parse(response)["results"];

            foreach (JsonObject item in responseJson)
            {
                if (Filter.BlacklistedVideo(item["trackName"].ToString()))
                {
                    continue;
                }

                if (Regex.IsMatch(Filter.FilterArtistName(item["artistName"].ToString()).ToLower(), Regex.Escape(newVideoInfo.Artist.ToLower())))
                {
                    newVideoInfo.Album = item["collectionName"].ToString();
                    newVideoInfo.Title = item["trackName"].ToString();
                    newVideoInfo.Artist = item["artistName"].ToString();
                    newVideoInfo.AlbumImage = await ImageUtils.GetAlbumImageUrl(newVideoInfo.Album, newVideoInfo.Artist);

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

                VideoInfo videoInfo = await FetchVideoInfos(title, author, ImageUtils.ResizeImage(video.Thumbnails[0].Url));
                Sys.debug($"Artist: {videoInfo.Artist} Title: {videoInfo.Title} Album: {videoInfo.Album} AlbumImage: {videoInfo.AlbumImage}");
            }

            return videoInfos;
        }
    }   
    
    
}
