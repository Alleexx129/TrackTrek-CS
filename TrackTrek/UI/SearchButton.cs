using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using MediaToolkit;
using MediaToolkit.Model;
using TrackTrek.Audio;
using YoutubeExplode.Common;
using System.IO;
using System.Xml.Linq;
using TrackTrek.Miscs;
using YoutubeExplode.Search;
using AngleSharp.Media;
using AngleSharp.Common;
using System.ComponentModel;

namespace TrackTrek.UI
{
    internal class SearchButton : Button
    {
        protected override async void OnClick(EventArgs e)
        {
            Form1.downloadProgress.Value = 0;
            try
            {
                var youtube = new YoutubeClient();

                string query = this.Parent.Controls.OfType<SearchBar>().FirstOrDefault()?.Text;

                if (Searching.CheckIfLink(query))
                {
                    Video videoInfo = await youtube.Videos.GetAsync(query);

                    Sys.debug($"Starting download...");

                    string output = await Download.DownloadAudio(youtube, videoInfo, query);

                    Sys.debug($"Audio downloaded!: {output}");

                    var thumbnail = videoInfo.Thumbnails[videoInfo.Thumbnails.Count - 1];

                    Sys.debug($"Adding metadata: {thumbnail.Url} {videoInfo}");

                    await CustomMetaData.Add(output, thumbnail.Url, videoInfo);

                    Form1.downloadProgress.Value = 100;
                } else
                {
                    await foreach (ISearchResult result in youtube.Search.GetResultsAsync(query))
                    {
                       switch (result)
                        {
                            case VideoSearchResult video:
                                {
                                    if (Form1.resultsList.SmallImageList == null)
                                    {
                                        Form1.resultsList.SmallImageList = new ImageList();
                                        Form1.resultsList.SmallImageList.ImageSize = new Size(70, 70);
                                    }


                                    string imageUrl = video.Thumbnails[video.Thumbnails.Count - 1].Url;
                                    
                                    ListViewItem item = new ListViewItem("", Form1.resultsList.SmallImageList.Images.Count);
                                    byte[] imageByte = await CustomMetaData.DownloadThumbnailAsBytes(imageUrl);
                                    byte[] resizedImage = ImageUtils.ResizeImage(imageByte);
                                    MemoryStream imageStream = new MemoryStream(resizedImage);
                                    Form1.resultsList.SmallImageList.Images.Add(Image.FromStream(imageStream));

                                    item.SubItems.Add(video.Title);
                                    item.SubItems.Add(Filter.FilterArtistName(video.Author));
                                    item.SubItems.Add(video.Duration.ToString());
                                    Form1.resultsList.Items.Add(item);

                                    break;
                                }
                            default:
                                {
                                    break;
                                }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Sys.debug(ex.Message);
            }
            base.OnClick(e);
        }
    };
}
