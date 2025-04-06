using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using TrackTrek.Audio;
using YoutubeExplode.Common;
using System.IO;
using System.Xml.Linq;
using TrackTrek.Miscs;
using YoutubeExplode.Search;
using AngleSharp.Media;
using AngleSharp.Common;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json.Nodes;
using static MediaToolkit.Model.Metadata;
using static TrackTrek.Miscs.Searching;
using System.Buffers.Text;

namespace TrackTrek.UI
{
    public class SearchButton : Button
    {
        protected override async void OnClick(EventArgs e)
        {
            Form1.downloadProgress.Value = 0;
            var youtube = new YoutubeClient();

            string query = this.Parent.Controls.OfType<SearchBar>().FirstOrDefault()?.Text;

            if (Searching.CheckIfLink(query) == "link")
            {
                ListViewItem newItem = new ListViewItem("Fetching...");

                newItem.SubItems.Add("Loading...");
                Form1.downloadQueue.Items.Add(newItem);

                YoutubeExplode.Videos.Video videoInfo = await youtube.Videos.GetAsync(query);
            
                Sys.debug($"Starting download...");

                string output = await Download.EnqueueDownload(videoInfo.Author.ToString(), videoInfo.Title.ToString(), query, newItem);

                Sys.debug($"Audio downloaded!: {output}");

                var thumbnail = videoInfo.Thumbnails[videoInfo.Thumbnails.Count - 1];

                Sys.debug($"Adding metadata: {thumbnail.Url} {videoInfo}");

                await CustomMetaData.Add(output, thumbnail.Url, videoInfo.Author.ToString(), videoInfo.Title.ToString());

                Form1.downloadProgress.Value = 100;
            }
            else if (Searching.CheckIfLink(query) == "playlist")
            {
                Form1.searchButton.Text = "Download Playlist";
                Form1.searchButton.Click += async (sender, e) =>
                {
                    if (Program.searchingPlaylist == true)
                    {
                        Form1.searchButton.Enabled = false;
                        foreach (ListViewItem item in Form1.resultsList.Items)
                        {
                            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

                            Task processTask = Task.Run(async () =>
                            {
                                ListViewItem resultItem = item;
                                ListViewItem newItem = new ListViewItem("Loading...");

                                string title = resultItem.SubItems[1].Text;
                                string artist = resultItem.SubItems[2].Text;
                                string album = resultItem.SubItems[3].Text;

                                newItem.SubItems.Add("Loading...");
                                Form1.downloadQueue.Items.Add(newItem);

                                YoutubeExplode.Videos.Video videoInfo = await Searching.GetVideo(title + " - " + artist);

                                Sys.debug("Starting download...");

                                string output = null;
                                Task processTask = Task.Run(async () =>
                                {
                                    output = await Download.EnqueueDownload(artist.Replace("/", "-"), title.Replace("/", "-"), videoInfo.Url, newItem);
                                });

                                for (int i = 0; i < 60; i++)
                                {
                                    await Task.Delay(1000);

                                    if (output != null && output != "")
                                    {
                                        break;
                                    }
                                }

                                Sys.debug("Audio downloaded!: " + output);

                                if (output != null && output != "")
                                {
                                    await CustomMetaData.Add(output, Convert.FromBase64String(resultItem.SubItems[4].Text), artist, title, album);
                                }


                                Form1.downloadProgress.Value = 100;
                                taskCompletionSource.SetResult(true);
                            });
                            /*
                            bool isDone = await Task.WhenAny(taskCompletionSource.Task, Task.Delay(15000)) == taskCompletionSource.Task;

                            if (!isDone)
                            {
                                Sys.debug("Couldn't download: Timeout");
                            }
                            */
                        }
                        Form1.searchButton.Text = "Search";
                        Form1.searchButton.Enabled = true;
                    }
                };
                Program.searchingPlaylist = true;
                Form1.searchButton.TextChanged += (sender, e) =>
                {
                    if (!Form1.searchButton.Text.Contains("?list="))
                    {
                        Form1.searchButton.Text = "Search";
                        Program.searchingPlaylist = false;
                    }
                };



                List<VideoInfo> videoInfos = await Searching.GetPlaylistVideos(query);
                int index = 0;

                Form1.resultsList.Items.Clear();
                foreach (var video in videoInfos)
                {
                    index++;
                    Form1.downloadProgress.Value = index / videoInfos.Count * 100;
                    if (Form1.resultsList.SmallImageList == null)
                    {
                        Form1.resultsList.SmallImageList = new ImageList();
                        Form1.resultsList.SmallImageList.ImageSize = new Size(70, 70);
                    }

                    ListViewItem listItem = new ListViewItem("", Form1.resultsList.SmallImageList.Images.Count);
                    byte[] resizedImage = video.AlbumImage;
                    MemoryStream imageStream = new MemoryStream(resizedImage);
                    Form1.resultsList.SmallImageList.Images.Add(Image.FromStream(imageStream));

                    listItem.SubItems.Add(video.Title);
                    listItem.SubItems.Add(video.Artist);
                    listItem.SubItems.Add(video.Album);
                    listItem.SubItems.Add(Convert.ToBase64String(video.AlbumImage));

                    Form1.resultsList.Items.Add(listItem);
                }


                Form1.searchButton.Enabled = true;
            } else
            {
                Form1.resultsList.Items.Clear();
                HttpClient client = new HttpClient();

                HttpResponseMessage httpResponse = await client.GetAsync($"https://itunes.apple.com/search?term={query}&entity=song");
                string response = await httpResponse.Content.ReadAsStringAsync();
                dynamic responseJson = JsonNode.Parse(response)["results"];
                int index = 0;

                foreach (JsonObject item in responseJson)
                {
                    index++;
                    if (Form1.resultsList.SmallImageList == null)
                    {
                        Form1.resultsList.SmallImageList = new ImageList();
                        Form1.resultsList.SmallImageList.ImageSize = new Size(70, 70);
                    }

                    var imageUrl = item["artworkUrl100"].ToString();

                    ListViewItem listItem = new ListViewItem("", Form1.resultsList.SmallImageList.Images.Count);
                    byte[] imageByte = await CustomMetaData.DownloadThumbnailAsBytes(imageUrl);
                    byte[] resizedImage = ImageUtils.ResizeImage(imageByte);
                    MemoryStream imageStream = new MemoryStream(resizedImage);
                    Form1.resultsList.SmallImageList.Images.Add(Image.FromStream(imageStream));

                    listItem.SubItems.Add(item["trackName"].ToString());
                    listItem.SubItems.Add(item["artistName"].ToString());
                    listItem.SubItems.Add(item["collectionName"].ToString());

                    // Here you can see I've removed duration it was NOT because I could do it, it did made the whole thing lag since I needed to request youtube for duration (ITunes does not support this)

                    Form1.resultsList.Items.Add(listItem);
                    if (index >= Int32.Parse(Program.maxResults))
                    {
                        break;
                    }
                }

            }
            base.OnClick(e);
        }
    };
}
