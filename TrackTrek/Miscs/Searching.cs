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

        public static async Task<Video> GetVideo(string title, string artist)
        {
            YoutubeClient youtube = new YoutubeClient();
            VideoSearchResult? found = null;
            byte bestRatio = 0;
            byte index = 0;
            
            await foreach(VideoSearchResult video in youtube.Search.GetVideosAsync($"{title} / {artist}"))
            {
                VideoSearchResult foundVideo = video;

                if (foundVideo.Title.Contains(" - "))
                {
                    foundVideo = new VideoSearchResult(video.Id, foundVideo.Title.Split(" - ")[1], new Author(video.Author.ChannelId, foundVideo.Title.Split(" - ")[0]), video.Duration, video.Thumbnails);
                }
                index += 1;

                byte titleRatio = (byte)(Fuzz.Ratio(title, foundVideo.Title));
                byte artistRatio = (byte)(Fuzz.PartialRatio(artist, foundVideo.Author.ToString()));

                if (!Filter.BlacklistedVideo(foundVideo.Title))
                {
                    if (titleRatio >= 90 && artistRatio >= 30)
                    {
                        found = foundVideo;
                        Sys.debug("Found video url:" + found.Url + " Title: " + found.Title.ToString() + " Author: " + found.Author.ToString() + " with accuracy of " + titleRatio + "%");
                        break;
                    }
                    else if (bestRatio < titleRatio && artistRatio >= 30)
                    {
                        found = foundVideo;
                        bestRatio = titleRatio;
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
                }
            }
            if (found != null)
            {
                return await youtube.Videos.GetAsync(found.Url);
            } else
            {
                MessageBox.Show("ERROR: AUDIO NOT FOUND ON YT");
                return new Video(new VideoId("21"), "", new Author(new YoutubeExplode.Channels.ChannelId(), ""), new DateTimeOffset((long)1, new TimeSpan((long)3)), "", new TimeSpan(), (IReadOnlyList<Thumbnail>)(new List<Thumbnail>()), (IReadOnlyList<string>)(new List<string>()), new Engagement(67, 2, 2));
            }
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

            string bestArtist = "";
            int bestArtistN = 0;
            string bestTitle = "";
            int bestTitleN = 0;
            string bestAlbum = "";

            foreach (JsonObject item in responseJson)
            {
                if (Filter.BlacklistedVideo(item["trackName"].ToString()))
                {
                    Sys.debug("skipped");
                    continue;
                }

                if (bestArtistN < Fuzz.Ratio(Filter.FilterArtistName(item["artistName"].ToString()), Filter.FilterArtistName(newVideoInfo.Artist.ToLower())))
                {
                    bestArtistN = Fuzz.Ratio(Filter.FilterArtistName(item["artistName"].ToString()), Filter.FilterArtistName(newVideoInfo.Artist.ToLower()));
                    bestArtist = Filter.FilterArtistName(item["artistName"].ToString());
                    Sys.debug("Detected artist: \"" + bestArtist + "\" Confidence " + bestArtistN + "%");
                };

                if (bestTitleN < Fuzz.Ratio(Filter.FilterTitle(item["trackName"].ToString()), Filter.FilterTitle(newVideoInfo.Title.ToLower())))
                {
                    bestTitle = Filter.FilterTitle(item["trackName"].ToString());
                    bestAlbum = item["collectionName"].ToString();
                    bestTitleN = Fuzz.Ratio(Filter.FilterTitle(item["trackName"].ToString()), Filter.FilterTitle(newVideoInfo.Title.ToLower()));
                    Sys.debug("Detected title: \"" + bestTitle + "\" Confidence " + bestTitleN + "%");
                    Sys.debug("Detected album: \"" + bestAlbum + "\"");
                }

                if (Fuzz.PartialRatio(Filter.FilterArtistName(item["artistName"].ToString()), Filter.FilterArtistName(newVideoInfo.Artist.ToLower())) > 80 && Fuzz.PartialRatio(Filter.FilterTitle(item["trackName"].ToString()), Filter.FilterTitle(newVideoInfo.Title.ToLower())) > 80)
                {
                    newVideoInfo.Album = item["collectionName"].ToString();
                    newVideoInfo.Title = Filter.FilterTitle(item["trackName"].ToString());
                    newVideoInfo.Artist = Filter.FilterArtistName(item["artistName"].ToString());;

                    break;
                }
            }

            newVideoInfo.Album = bestAlbum;
            newVideoInfo.Title = bestTitle;
            newVideoInfo.Artist = bestArtist;
            string albumImage = await ImageUtils.GetAlbumImageUrl(newVideoInfo.Album, newVideoInfo.Artist);

            newVideoInfo.AlbumImage = await CustomMetaData.DownloadThumbnailAsBytes(albumImage);
            return newVideoInfo;
        }
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
