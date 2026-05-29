using AngleSharp.Media;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using TrackTrek.Miscs;
using TrackTrek.UI;
using static TrackTrek.Miscs.Searching;



namespace TrackTrek.Audio
{

    internal class Download
    {
        private static List<object[]> queue = new List<object[]>();
        private static bool downloading = false;

        private static async Task<string> ConvertAndDelete(string name, string path, ListViewItem item) // to delete in future
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

                await process.WaitForExitAsync();
                
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
                // meh maybe ill use it when I rewrite the code
            }
            return "";
        }
        public static async Task<VideoInfo> DownloadAudio(string link) // done
        {
            // MessageBox.Show($"""--print "Title: %(title|Unknown)s\nArtist: %(uploader|Unknown)s\nThumbnail: %(thumbnail|Unknown)s" --no-simulate -P "{Program.customPath}" -o "%(title)s - %(uploader)s.%(ext)s" -t mp3 "https://www.youtube.com/watch?v=nK9wM_WLjhA" """);
            // Arguments = $"--print \"Title: %(title|Unknown)s\nArtist: %(uploader|Unknown)s\nThumbnail: %(thumbnail|Unknown)s\" -o \"%(title)s-%(uploader)s.%(ext)s\" -P \"{Program.customPath}\" \"https://www.youtube.com/watch?v=nK9wM_WLjhA\"",

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "yt-dlp.exe",
                Arguments = $"""--print "Title: %(title|Unknown)s\nArtist: %(uploader|Unknown)s\nThumbnail: %(thumbnail|Unknown)s" --encoding utf-8 --no-simulate -P "{Program.customPath} " -o "%(title)s - %(uploader)s.%(ext)s" -t mp3 "{link}" """,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,

            };
            processStartInfo.Environment["PYTHONIOENCODING"] = "utf-8";

            using (var process = new Process { StartInfo = processStartInfo })
            {
                //process.Start();
                //await process.WaitForExitAsync();

                //var errorTask = process.StandardError.ReadToEndAsync();
                //MessageBox.Show(errorTask.Result);
                //var outputTask = process.StandardOutput.ReadToEndAsync();

                process.Start();

                Task<string> errorTask = process.StandardError.ReadToEndAsync();
                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();

                await process.WaitForExitAsync();

                string output = await outputTask;
                string error = await errorTask;

                //MessageBox.Show($"Output: {output}\nError: {error}");

                Sys.debug("Current output errors, these may be normal...\n" + error);
                Sys.debug("Output from console: \n" + output);

                string[] splitOut = output.Split("\\n");
                var map = new Dictionary<char, char> { { '/', '⧸' }, { '\\', '⧹' }, { ':', '：' }, { '*', '∗' }, { '?', '？' }, { '"', '＂' }, { '<', '〈' }, { '>', '〉' }, { '|', '｜' } }; // I hate windows

                string title = splitOut[0].Replace("Title: ", "");
                string author = splitOut[1].Replace("Artist: ", "");

                string sanatizedTitle = Regex.Replace(title ?? "", @"[\\/:*?""<>|]", m => map[m.Value[0]].ToString()).TrimEnd(' ');
                sanatizedTitle = sanatizedTitle.EndsWith('.') ? sanatizedTitle[..^1] + "．" : sanatizedTitle;
                string sanatizedAuthor = Regex.Replace(author ?? "", @"[\\/:*?""<>|]", m => map[m.Value[0]].ToString()).TrimEnd(' ');
                sanatizedAuthor = sanatizedAuthor.EndsWith('.') ? sanatizedAuthor[..^1] + "．" : sanatizedAuthor;
                string ThumbnailUrl = splitOut[2].Replace("Thumbnail: ", "");

                VideoInfo videoInfo = new VideoInfo();
                videoInfo.Title = title;
                videoInfo.YoutubeArtist = author;
                videoInfo.AlbumImageUrl = ThumbnailUrl;
                videoInfo.YoutubeImageUrl = ThumbnailUrl;

                videoInfo.Path = $"{Program.customPath}{sanatizedTitle} - {sanatizedAuthor}.mp3";

                return videoInfo;
            }
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

            //string outp = await DownloadAudio(artist, title, query, item);
            lock (queue)
            { 
                queue.RemoveAt(0); 
            }
                
            downloading = false;


            return "";
        }
    }
}

// future structure
/*
 * Add more folders
 * Classes folder
 * ...
*/