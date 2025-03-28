using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using AngleSharp.Media;
using MediaToolkit;
using MediaToolkit.Model;
using TrackTrek.Miscs;
using TrackTrek.UI;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace TrackTrek.Audio
{

    internal class Download
    {
        private static List<object> queue = new List<object>
        {
            "artist",
            "title",
            "query",
            new ListViewItem()

        };
        private static Boolean downloading = false;

        private static async Task<string> ConvertAndDelete(string name, string path, ListViewItem item)
        {
            String outputPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), $"{name}.mp3");
            item.SubItems[0].Text = outputPath;

            Sys.debug("Output path: " + outputPath);
            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{path}\" -c:a libmp3lame -b:a 192k -ar 44100 -ac 2 -threads 4 -y \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                process.WaitForExit();


                if (process.ExitCode != 0)
                {
                    Sys.debug($"FFMPEG ERROR: {error}");
                    await ConvertAndDelete(name, path, item);
                    throw new Exception($"FFmpeg conversion failed: {error}");
                }

                Form1.downloadProgress.Value = 80;
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                item.SubItems[1].Text = "Completed!";
                return outputPath;
            }
        }

        private protected static async Task<string> DownloadAudio(string artist, string title, string query, ListViewItem item)
        {
            YoutubeClient youtube = new YoutubeClient();

            Sys.debug($"Step 1 download");
            item.SubItems[0].Text = "Getting info...";
            item.SubItems[1].Text = "Fetching...";

            StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(query);
            IStreamInfo streamInfo = streamManifest
                .GetAudioOnlyStreams()
                .GetWithHighestBitrate();

            Stream stream = await youtube.Videos.Streams.GetAsync(streamInfo);
            string name = title + " - " + Filter.FilterArtistName(artist);

            Sys.debug($"Step 2 Download");

            String path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), $"{name}.{streamInfo.Container}");

            Sys.debug($"Path: {path}");
            item.SubItems[0].Text = path;
            item.SubItems[1].Text = "Downloading...";

            Form1.downloadProgress.Maximum = 100;
            Form1.downloadProgress.Value = 20;

            await youtube.Videos.Streams.DownloadAsync(streamInfo, path);

            Sys.debug($"Final downloading...");

            Form1.downloadProgress.Value = 40;

            Sys.debug($"Starting \"ConvertAndDelete\"");

            item.SubItems[1].Text = "Converting...";
            return await ConvertAndDelete(name, path, item);
        }

        public async static Task<string> EnqueueDownload(string artist, string title, string query, ListViewItem item)
        {
            object[] list = new object[] { 
                artist, title, query, item
            };

            item.SubItems[0].Text = "Loading...";
            item.SubItems[1].Text = "Enqueued!";

            queue.AddRange(list);

            if (!downloading && queue[queue.Count - 1] == list)
            {
                downloading = true;
                queue.RemoveAt(0);

                await DownloadAudio(artist, title, query, item);
                downloading = false;
            } else
            {
                while (downloading && queue[queue.Count - 1] != list)
                {
                    if (queue[queue.Count - 1] == list)
                    {
                        await Task.Delay(500);
                    } else
                    {
                        await Task.Delay(1000);
                    }
                }
            }

            downloading = true;
            queue.RemoveAt(0);
            string outp = await DownloadAudio(artist, title, query, item);
            downloading = false;


            return outp;
        }
    }
}
