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
                                    string imageUrl = video.Thumbnails[video.Thumbnails.Count - 1].Url;
                                    
                                    ListViewItem item = new ListViewItem();
                                    byte[] imageByte = await CustomMetaData.DownloadThumbnailAsBytes(imageUrl);
                                    Sys.debug(imageByte.Length.ToString());
                                    byte[] resizedImage = ImageUtils.ResizeImage(imageByte);
                                    Sys.debug(resizedImage.Length.ToString());
                                    MemoryStream imageStream = new MemoryStream(resizedImage);
                                    Sys.debug(imageStream.Length.ToString());

                                    imageStream.CopyTo(imageStream);
                                    Form1.resultsList.SmallImageList.Images.Add("Image", Image.FromStream(imageStream));
                                    Sys.debug("After");
                                    item.SubItems.Add(video.Title);
                                    item.SubItems.Add(FilterArtistName.filter(video.Author));
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
