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
using static TrackTrek.Miscs.Searching;

namespace TrackTrek.Audio
{

    internal class Download
    {
        private static List<object[]> queue = new List<object[]>();
        private static bool downloading = false;

        private static async Task<string> ConvertAndDelete(string name, string path, ListViewItem item)
        {
            string outputPath = Path.Combine(Program.customPath, $"{name}.mp3");
            Form1.downloadProgress.Invoke(new MethodInvoker(() =>
            {
                item.SubItems[0].Text = outputPath;
            }));

            if (File.Exists(outputPath))
            {
                File.Delete(outputPath);
            }

            Sys.debug("Output path: " + outputPath);
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{path}\" -c:a libmp3lame -b:a 192k -ar 44100 -ac 2 -threads 4 -y \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();

                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();
                var waitTask = process.WaitForExitAsync();

                string output = await outputTask;
                string error = await errorTask;

                if (process.ExitCode != 0)
                {
                    Sys.debug($"FFMPEG ERROR: {error}");
                    throw new Exception($"FFmpeg conversion failed: {error}");
                }

                Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                {
                    Form1.downloadProgress.Value = 80;
                }));
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                {
                    item.SubItems[1].Text = "Completed!";
                    Form1.downloadProgress.Value = 100;
                }));
                return outputPath;
            }
        }


        private protected static async Task<string> DownloadPlaylist(VideoInfo[] playlistInfo)
        {
            Sys.debug(playlistInfo.ToString());
            foreach (VideoInfo video in playlistInfo)
            {
                // function not used :( 
                // Will delete in the future
            }
            return "";
        }

        private protected static async Task<string> DownloadAudio(string artist, string title, string query, ListViewItem item)
        {
            YoutubeClient youtube = new YoutubeClient();

            Sys.debug($"Step 1 download");
            Form1.downloadProgress.Invoke(new MethodInvoker(() =>
            {
                item.SubItems[0].Text = "Getting info...";
                item.SubItems[1].Text = "Fetching...";
            }));
            StreamManifest streamManifest;
            IStreamInfo streamInfo;

            try
            {
                streamManifest = await youtube.Videos.Streams.GetManifestAsync(query);
                streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            }
            catch
            {
                try
                {
                    await Task.Delay(5000);
                    streamManifest = await youtube.Videos.Streams.GetManifestAsync(query);
                    streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
                } catch (Exception e)
                {
                    Sys.debug("Error when downloading: " + e.Message.ToString());
                    Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                    {
                        item.SubItems[1].Text = "Error!";
                    }));
                    return "C:\\";
                }
                
            }

            Stream stream = await youtube.Videos.Streams.GetAsync(streamInfo);
            string name = title + " - " + Filter.FilterArtistName(artist);

            Sys.debug($"Step 2 Download");

            String path = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), $"{name}.{streamInfo.Container}");

            Sys.debug($"Path: {path}");

            Form1.downloadProgress.Invoke(new MethodInvoker(() =>
            {
                item.SubItems[0].Text = path;
                item.SubItems[1].Text = "Downloading...";

                Form1.downloadProgress.Maximum = 100;
                Form1.downloadProgress.Value = 20;
            }));

            try
            {
                await youtube.Videos.Streams.DownloadAsync(streamInfo, path);
                Sys.debug("Downloaded");
            } catch (Exception e)
            {
                Sys.debug("Error when downloading: " + e.Message.ToString());
                Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                {
                    item.SubItems[0].Text = path;
                    item.SubItems[1].Text = "Error!";
                }));
                return path;
            }

            Sys.debug($"Final downloading...");

            Form1.downloadProgress.Invoke(new MethodInvoker(() =>
            {
                Form1.downloadProgress.Value = 40;
            }));

            Sys.debug($"Starting \"ConvertAndDelete\"");

            Form1.downloadProgress.Invoke(new MethodInvoker(() =>
            {
                item.SubItems[1].Text = "Converting...";
            }));
            return await ConvertAndDelete(name, path, item);
        }

        public async static Task<string> EnqueueDownload(string artist, string title, string query, ListViewItem item)
        {
            var list = new object[] { artist, title, query, item };

            Form1.downloadProgress.Invoke(new MethodInvoker(() =>
            {
                item.SubItems[0].Text = "Loading...";
                item.SubItems[1].Text = "Enqueued!";
            }));

            lock (queue)
            {
                queue.Add(list);
            }

            while (true)
            {
                lock (queue)
                {
                    if (queue[0] == list && !downloading)
                        break;
                }
                await Task.Delay(500);
            }

            downloading = true;

            string outp = await DownloadAudio(artist, title, query, item);
            lock (queue)
            { 
                queue.RemoveAt(0); 
            }
                
            downloading = false;


            return outp;
        }
    }
}
