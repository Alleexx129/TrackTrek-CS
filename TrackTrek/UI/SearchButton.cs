using AngleSharp.Common;
using AngleSharp.Dom;
using AngleSharp.Media;
using HtmlAgilityPack;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.Arm;
using System.Security.Policy;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using TrackTrek.Audio;
using TrackTrek.Miscs;
using static System.Net.Mime.MediaTypeNames;
using static TrackTrek.Miscs.Searching;

namespace TrackTrek.UI
{
    public class SearchButton : Button
    {
        /*
        protected override async void OnClick(EventArgs e)
        {
            //string geniusLink = Lyrics.ToGeniusLink()
            //var lyrics = await Lyrics.GetLyrics("https://genius.com/System-of-a-down-sugar-lyrics");

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

                VideoInfo videoInfoFromITune = await Searching.FetchVideoInfos(videoInfo.Title.ToString(), videoInfo.Author.ToString(), new byte[0]);
                string geniusLink;
                string lyrics;
                string output;

                try
                {
                    geniusLink = Lyrics.ToGeniusLink(videoInfoFromITune.Title.ToString(), videoInfoFromITune.Artist.ToString());

                    lyrics = await Lyrics.GetLyrics(geniusLink);
                }
                catch (Exception)
                {
                    lyrics = "";
                }
                if (lyrics == "")
                {
                    output = await Download.EnqueueDownload(Filter.FilterArtistName(videoInfo.Author.ToString()), Filter.FilterTitle(videoInfo.Title.ToString()), query, newItem);
<<<<<<< HEAD
                } else
=======
                }
                else
>>>>>>> b9fa676d63c9bdb7c2413ca6f305a4957e916700
                {
                    output = await Download.EnqueueDownload(videoInfoFromITune.Artist, videoInfoFromITune.Title, query, newItem);
                }

                try
                {
                    geniusLink = Lyrics.ToGeniusLink(videoInfoFromITune.Title.ToString(), videoInfoFromITune.Artist.ToString());

                    lyrics = await Lyrics.GetLyrics(geniusLink);
                }
                catch (Exception)
                {
                    lyrics = "";
                }
                if (lyrics == "")
                {
                    output = await Download.EnqueueDownload(Filter.FilterArtistName(videoInfo.Author.ToString()), Filter.FilterTitle(videoInfo.Title.ToString()), query, newItem);
                } else
                {
                    output = await Download.EnqueueDownload(videoInfoFromITune.Artist, videoInfoFromITune.Title, query, newItem);
                }

                if (newItem.SubItems[1].Text == "Error!")
                {
                    Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                    {
                        Form1.downloadProgress.Value = 100;
                    }));
                    return;
                }

                Sys.debug($"Audio downloaded!: {output}");

                var thumbnail = videoInfo.Thumbnails[videoInfo.Thumbnails.Count - 1];

                Sys.debug($"Adding metadata: {thumbnail.Url} {videoInfo}");

                if (await ImageUtils.GetAlbumImage(videoInfoFromITune.Album.ToString(), videoInfoFromITune.Artist.ToString()) != string.Empty)
                {
                    await CustomMetaData.Add(output, await ImageUtils.GetAlbumImage(videoInfoFromITune.Album.ToString(), videoInfoFromITune.Artist.ToString()), videoInfoFromITune.Artist.ToString(), videoInfoFromITune.Title.ToString(), lyrics, videoInfoFromITune.Album.ToString());
<<<<<<< HEAD
                } else
=======
                }
                else
>>>>>>> b9fa676d63c9bdb7c2413ca6f305a4957e916700
                {
                    await CustomMetaData.Add(output, thumbnail.Url, videoInfo.Author.ToString(), videoInfo.Title.ToString(), "", "Youtube");
                }


                Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                {
                    Form1.downloadProgress.Value = 100;
                }));
            }
            else if (Searching.CheckIfLink(query) == "playlist")
            {
                if (Form1.searchButton.Enabled == true && Form1.searchButton.Text == "Download")
                {
                    Form1.searchButton.Invoke(new MethodInvoker(() =>
                    {
                        Form1.searchButton.Enabled = false;
                    }));

                    List<ListViewItem> results = Form1.resultsList.Items.Cast<ListViewItem>().ToList();

                    Task processTask = Task.Run(async () =>
                    {
<<<<<<< HEAD
                        try
                        {
                            foreach (ListViewItem resultItem in results)
=======
                        foreach (ListViewItem resultItem in results)
                        {
                            try
>>>>>>> b9fa676d63c9bdb7c2413ca6f305a4957e916700
                            {
                                TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();
                                await Task.Delay(100);
                                ListViewItem newItem = new ListViewItem("Loading...");

                                string title = resultItem.SubItems[1].Text;
                                string artist = resultItem.SubItems[2].Text;
                                string album = resultItem.SubItems[3].Text;

                                Form1.downloadQueue.Invoke(new MethodInvoker(() =>
                                {
                                    newItem.SubItems.Add("Loading...");
                                    Form1.downloadQueue.Items.Add(newItem);
                                }));

                                YoutubeExplode.Videos.Video videoInfo = await Searching.GetVideo(title, artist, album);


                                Sys.debug("Starting download...");

                                string output = await Download.EnqueueDownload(artist.Replace("/", "-"), title.Replace("/", "-"), videoInfo.Url, newItem);

                                Form1.downloadQueue.Invoke(new MethodInvoker(() =>
                                {
                                    if (newItem.SubItems[1].Text == "Error!")
                                    {
                                        Form1.downloadProgress.Value = 100;
                                        taskCompletionSource.SetResult(true);
                                        return;
                                    }

                                }));

                                Sys.debug("Audio downloaded!: " + output);

                                if (album != "Youtube")
                                {
                                    string geniusLink = Lyrics.ToGeniusLink(title, artist);
                                    var lyrics = await Lyrics.GetLyrics(geniusLink);
                                    Sys.debug(resultItem.SubItems[4].Text + " is supposed to be image");

                                    await CustomMetaData.Add(output, Convert.FromBase64String(resultItem.SubItems[4].Text), artist.toCapitalFirst(), title, lyrics, album);
                                }
                                else
                                {
                                    byte[] imageByte = await CustomMetaData.DownloadThumbnailAsBytes(videoInfo.Thumbnails[videoInfo.Thumbnails.Count - 1].Url);
                                    byte[] resizedImage = ImageUtils.ResizeImage(imageByte);
                                    await CustomMetaData.Add(output, resizedImage, artist.toCapitalFirst(), title, "Unknown", album);
                                }



                                Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                                {
                                    Form1.downloadProgress.Value = 100;
                                }));
                                taskCompletionSource.SetResult(true);
                            }
<<<<<<< HEAD
                        } catch (Exception e)
                        {
                            MessageBox.Show(e.ToString());
=======
                            catch (Exception e)
                            {
                                MessageBox.Show("Error (fix will be implemented soon) \n Audio " + resultItem.SubItems[1].Text + " won't be downloaded\nClick to dismiss... " + e.ToString()); // note to myself, do better than that and fix the bug causing it not to show
                                Sys.debug(e.ToString());
                            }
>>>>>>> b9fa676d63c9bdb7c2413ca6f305a4957e916700
                        }
                    });



                    /*
                    bool isDone = await Task.WhenAny(taskCompletionSource.Task, Task.Delay(15000)) == taskCompletionSource.Task;

                    if (!isDone)
                    {
                        Sys.debug("Couldn't download: Timeout");
                    }
                    *\/

                    Form1.searchButton.Invoke(new MethodInvoker(() =>
                    {
                        Form1.searchButton.Text = "Search";
                        Form1.searchButton.Enabled = true;
                    }));
                    base.OnClick(e);
                    return;
                }
                Program.searchingPlaylist = true;
                Form1.searchButton.Invoke(new MethodInvoker(() =>
                {
                    Form1.searchButton.Text = "Download";
                    Form1.searchButton.Enabled = false;
                }));



                List<VideoInfo> videoInfos = await Searching.GetPlaylistVideos(query);
                int index = 0;

                Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                {
                    Form1.resultsList.Items.Clear();
                }));

                foreach (var video in videoInfos)
                {
                    try
                    {

                        index++;
                        ListViewItem listItem;
                        Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                        {
                            Form1.downloadProgress.Value = 100;
                        }));

                        if (Form1.resultsList.SmallImageList == null)
                        {
                            Form1.resultsList.SmallImageList = new ImageList();
                            Form1.resultsList.SmallImageList.ImageSize = new Size(70, 70);
                        }

                        Bitmap bitmap;
                        byte[] resizedImage = video.AlbumImage;

                        string header = BitConverter.ToString(resizedImage.Take(12).ToArray());

                        Stream imageStream = new MemoryStream(resizedImage);

                        Form1.resultsList.Invoke(new MethodInvoker(() =>
                        {
                            listItem = new ListViewItem("", Form1.resultsList.SmallImageList.Images.Count);

                            try
                            {
                                Form1.resultsList.SmallImageList.Images.Add(Image.FromStream(imageStream));
                            }
                            catch (Exception)
                            {

                                Task processTask = Task.Run(async () =>
                                {
                                    var thum = await CustomMetaData.DownloadThumbnailAsBytes("https://r2.image-upload.app/uploads/permanent/image/1771522735192-i59pdc6gfmd.png");
                                    Form1.resultsList.SmallImageList.Images.Add(Image.FromStream(new MemoryStream(thum)));
                                });
                            }

                            listItem.SubItems.Add(video.Title);
                            listItem.SubItems.Add(video.Artist);
                            listItem.SubItems.Add(video.Album);
                            listItem.SubItems.Add(Convert.ToBase64String(video.AlbumImage));

                            Form1.resultsList.Items.Add(listItem);
                        }));
                    }
                    catch (Exception err)
                    {
<<<<<<< HEAD
                        listItem = new ListViewItem("", Form1.resultsList.SmallImageList.Images.Count);

                        try
                        {
                            Form1.resultsList.SmallImageList.Images.Add(Image.FromStream(imageStream));
                        } catch(Exception)
                        {

                            Task processTask = Task.Run(async () =>
                            {
                                var thum = await CustomMetaData.DownloadThumbnailAsBytes("https://r2.image-upload.app/uploads/permanent/image/1771522735192-i59pdc6gfmd.png");
                                Form1.resultsList.SmallImageList.Images.Add(Image.FromStream(new MemoryStream(thum)));
                            });
                        }

                        listItem.SubItems.Add(video.Title);
                        listItem.SubItems.Add(video.Artist);
                        listItem.SubItems.Add(video.Album);
                        listItem.SubItems.Add(Convert.ToBase64String(video.AlbumImage));

                        Form1.resultsList.Items.Add(listItem);
                    }));
=======
                        MessageBox.Show("Error (fix will be implemented soon) click to dismiss... " + err.ToString()); // note to myself, do better than that and fix the bug causing it not to show
                        Sys.debug(err.ToString());
                    }
>>>>>>> b9fa676d63c9bdb7c2413ca6f305a4957e916700
                }

                Form1.searchButton.Invoke(new MethodInvoker(() =>
                {
                    Form1.searchButton.Enabled = true;
                }));
            }
            else
            {
                Form1.resultsList.Invoke(new MethodInvoker(() =>
                {
                    Form1.resultsList.Items.Clear();
                }));
                HttpClient client = new HttpClient();

                HttpResponseMessage httpResponse = await client.GetAsync($"https://itunes.apple.com/search?term={query}&entity=song");
                string response = await httpResponse.Content.ReadAsStringAsync();
                dynamic responseJson = JsonNode.Parse(response)["results"];
                int index = 0;

                foreach (JsonObject item in responseJson)
                {
                    index++;
                    Form1.resultsList.Invoke(new MethodInvoker(() =>
                    {
                        if (Form1.resultsList.SmallImageList == null)
                        {
                            Form1.resultsList.SmallImageList = new ImageList();
                            Form1.resultsList.SmallImageList.ImageSize = new Size(70, 70);
                        }
                    }));


                    var imageUrl = item["artworkUrl100"].ToString();
                    byte[] imageByte = await CustomMetaData.DownloadThumbnailAsBytes(imageUrl);
                    Form1.resultsList.Invoke(new MethodInvoker(() =>
                    {
                        ListViewItem listItem = new ListViewItem("", Form1.resultsList.SmallImageList.Images.Count);

                        byte[] resizedImage = ImageUtils.ResizeImage(imageByte);
                        MemoryStream imageStream = new MemoryStream(resizedImage);
                        Form1.resultsList.SmallImageList.Images.Add(Image.FromStream(imageStream));

                        listItem.SubItems.Add(Filter.FilterTitle(item["trackName"].ToString()));
                        listItem.SubItems.Add(item["artistName"].ToString());
                        listItem.SubItems.Add(item["collectionName"].ToString());

                        Form1.resultsList.Items.Add(listItem);
                    }));

                    // Here you can see I've removed duration it was NOT because I could do it, it did made the whole thing was delaying since I needed to request youtube for duration (ITunes does not support this)

                    if (index >= Int32.Parse(Program.maxResults))
                    {
                        break;
                    }
                }
            }
            base.OnClick(e);
        }
        */
        protected override async void OnClick(EventArgs e)
        {
            string? query = this.Parent?.Controls.OfType<SearchBar>().FirstOrDefault()?.Text;

            if (query == string.Empty || query == null)
            {
                Sys.debug("No input");
                return;
            }

            string videoType = query.CheckStringType();

            // need to check if its Search or Download (for playlist)

            // "link" "playlist" "keyword"
            switch(videoType)
            {
                case "link":
                    Sys.debug("Identified type \"Youtube Video Link\" to be the query");

                    Sys.debug($"Downloading audio...");

                    VideoInfo audioOutput = await Download.DownloadAudio(this.Parent.Controls.OfType<SearchBar>().FirstOrDefault().Text);

                    Sys.debug($"Audio downloaded!: {audioOutput.Path}");

                    VideoInfo musicUpdatedInfo = await Searching.FetchAndUpdateWithItune(audioOutput);
                    VideoInfo musicUpdatedInfoFinal;

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
                    } else
                    {
                        VideoInfo musicUpdatedInfoImage = await Searching.SearchImage(musicUpdatedInfo);

                        musicUpdatedInfoFinal = await SearchAndUpdateLyrics(musicUpdatedInfoImage);
                    }

                    Sys.debug($"Video Infos:\n  Path:{musicUpdatedInfoFinal.Path}\n  Title:{musicUpdatedInfoFinal.Title}\n  Artist: {musicUpdatedInfoFinal.GetArtist()}\n  Album: {musicUpdatedInfoFinal.Album}\n  Album Image Found: {musicUpdatedInfoFinal.AlbumImageUrl != ""}");
                    
                    await CustomMetaData.Add(musicUpdatedInfo);
                    
                    break;
                case "playlist":
                    MessageBox.Show("Unsupported, please download the latest release instead of the current alpha");
                    // same as link but foreach, not same for spotify
                    break;
                case "keyword":
                    MessageBox.Show("Unsupported, please download the latest release instead of the current alpha");
                    // first search itune, get info, get lyrics and all, then at the end search audio
                    await Searching.GetVideo("test", "test", "test");
                    // also need to update the double click in button list
                    break;
                case "spotify":
                    MessageBox.Show("Unsupported, this feature will only be available in the future");
                    // temporary
                    // use https://open.spotify.com/oembed?url=playlisturlhere
                    break;
                default:
                    MessageBox.Show("Please enter something in the typing box");
                    break;
            }
            
            base.OnClick(e);
        }
    };
}
