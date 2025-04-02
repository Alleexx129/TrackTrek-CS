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

namespace TrackTrek.UI
{
    internal class SearchButton : Button
    {
        protected override async void OnClick(EventArgs e)
        {
            await Searching.GetPlaylistVideos("https://youtube.com/playlist?list=PLYY_JhYfJvbWb8mpWoej-Uz05aR4i7zOk&si=Lnhnd6GBzQWMP_e5");
            Form1.downloadProgress.Value = 0;
            var youtube = new YoutubeClient();

            string query = this.Parent.Controls.OfType<SearchBar>().FirstOrDefault()?.Text;

            if (Searching.CheckIfLink(query))
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
            else
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
