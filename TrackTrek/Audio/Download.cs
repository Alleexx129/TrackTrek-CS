using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

    internal class Download {
        private static ListViewItem item;
        private static async Task<string> Convert_and_Delete(string name, string path)
        {

            String outputPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), $"{name}.mp3");

            Sys.debug("Output path: " + outputPath);
            ProcessStartInfo processStartInfo = new ProcessStartInfo { 
                FileName = "ffmpeg",
                Arguments = $"-i \"{path}\" -c:a libmp3lame -b:a 192k -ar 44100 -ac 2 -threads 4 -y \"{outputPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(processStartInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                Console.WriteLine(output);
                Console.WriteLine(error);

                if (process.ExitCode != 0)
                {
                    throw new Exception($"FFmpeg conversion failed: {error}");
                }
                Form1.downloadProgress.Value = 80;

                File.Delete(path);
                item.SubItems[1].Text = "Completed!";
                return outputPath;
            }

        }
        public static async Task<string> DownloadAudio(YoutubeClient youtube, YoutubeExplode.Videos.Video videoInfo, string query)
        {
            Sys.debug($"Step 1 download");
            item = new ListViewItem("Getting info...");
            item.SubItems.Add("Fetching...");
            Form1.downloadQueue.Items.Add(item);

            StreamManifest streamManifest = await youtube.Videos.Streams.GetManifestAsync(query);
            IStreamInfo streamInfo = streamManifest
                .GetAudioOnlyStreams()
                .GetWithHighestBitrate();

            Stream stream = await youtube.Videos.Streams.GetAsync(streamInfo);
            string name = videoInfo.Title + " - " + FilterArtistName.filter(videoInfo.Author);

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

            Sys.debug($"Starting \"Convert_and_Delete\"");

            item.SubItems[1].Text = "Converting...";
            return await Convert_and_Delete(name, path);
        }
    }
}
