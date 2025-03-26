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

                Video videoInfo = await youtube.Videos.GetAsync(query);

                Sys.debug($"Starting download...");

                string output = await Download.DownloadAudio(youtube, videoInfo, query);

                Sys.debug($"Audio downloaded!: {output}");

                var thumbnail = videoInfo.Thumbnails[videoInfo.Thumbnails.Count - 1];

                Sys.debug($"Adding metadata: {thumbnail.Url} {videoInfo}");

                await CustomMetaData.Add(output, thumbnail.Url, videoInfo);

                Form1.downloadProgress.Value = 100;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            base.OnClick(e);
        }
    };
}
