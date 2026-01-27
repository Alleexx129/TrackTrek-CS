using AngleSharp.Common;
using System.Drawing;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrackTrek.Audio;
using TrackTrek.Miscs;
using TrackTrek.UI;
using YoutubeExplode;
using YoutubeExplode.Common;

namespace TrackTrek
{
    internal static class Program
    {
        public static bool searchingPlaylist = false;
        public static bool debug = false;
        public static string maxResults = "10";
        public static string customPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Sys.initialize();
            string path1 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrackTrek");
            string path = Path.Combine(path1, "Settings.json");
            JsonNode jsonNode = JsonNode.Parse(File.ReadAllText(path));
            
            try
            {
                Program.debug = jsonNode["debug"].GetValue<bool>();
                if (!jsonNode.AsObject().ContainsKey("customPath"))
                {
                    MessageBox.Show("test");
                    jsonNode["customPath"] = Program.customPath;
                    File.WriteAllText(path, jsonNode.ToJsonString());
                }
                Program.customPath = jsonNode["customPath"].GetValue<string>();
                Program.maxResults = jsonNode["maxResults"].GetValue<string>();
            }
            catch (Exception e)
            {
                File.WriteAllText(path, $"{{\"debug\": true, \"maxResults\": \"10\", \"customPath\": \"{(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")).Replace("\\", "\\\\")}\"}}");
                jsonNode = JsonNode.Parse(File.ReadAllText(path));
                Program.debug = jsonNode["debug"].GetValue<bool>();
                Sys.debug("Missing/Malformatted value in Settings.json, resetting to default values. Advanced: " + e.Message.ToString());
                Program.customPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                Program.maxResults = jsonNode["maxResults"].GetValue<string>();
            }

            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.

                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());
            }
            catch (Exception e)
            {
                Sys.debug(e.Message.ToString());
                File.WriteAllText(Path.Combine(path1, DateTime.Today.ToString("yyyy-MM-dd") + "_error.txt"), e.ToString());
                MessageBox.Show("An unexpected error occurred: " + e.ToString());
            }
            ;





        }
        public static void add_controls(Control.ControlCollection Controls, SearchBar searchBox, SearchButton searchButton, Button settingsButton, ListView resultsList, ListView downloadQueue, ProgressBar downloadProgress)
        {
            Controls.Add(searchBox);
            Controls.Add(searchButton);
            Controls.Add(settingsButton);
            Controls.Add(resultsList);
            Controls.Add(downloadQueue);
            Controls.Add(downloadProgress);

            //searchButton.Click += SearchButton_Click;
            //settingsButton.Click += SettingsButton_Click;
            resultsList.DoubleClick += (sender, e) => Task.Run(async () =>
            {
                ListViewItem resultItem = null;

                Form1.resultsList.Invoke(new MethodInvoker(() =>
                {
                    if (Form1.resultsList.SelectedItems.Count > 0)
                    {
                        resultItem = Form1.resultsList.SelectedItems[0];
                    }
                }));
                Sys.debug("Double click detected!");

                //ListViewItem resultItem = resultsList.SelectedItems[0];
                ListViewItem newItem = new ListViewItem("Loading...");

                string title = resultItem.SubItems[1].Text;
                string artist = resultItem.SubItems[2].Text;
                string album = resultItem.SubItems[3].Text;

                newItem.SubItems.Add("Loading...");
                lock (Form1.downloadQueue.Items)
                {
                    Form1.downloadQueue.Invoke(new MethodInvoker(() =>
                    {
                        Form1.downloadQueue.Items.Add(newItem);
                    }));
                }

                YoutubeExplode.Videos.Video videoInfo = await Searching.GetVideo(title, artist, album);

                Sys.debug("Starting download...");

                string output = await Download.EnqueueDownload(artist.Replace("/", "-"), title.Replace("/", "-"), videoInfo.Url, newItem);

                Form1.downloadQueue.Invoke(new MethodInvoker(() =>
                {
                    if (newItem.SubItems[1].Text == "Error!")
                    {
                        Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                        {
                            Form1.downloadProgress.Value = 100;
                        }));
                        return;
                    }
                }));
                

                Sys.debug("Audio downloaded!: " + output);

                string albumImage = await ImageUtils.GetAlbumImage(album, artist);

                Sys.debug("Adding album image: " + albumImage);

                string geniusLink = Lyrics.ToGeniusLink(title, artist);
                string lyrics = await Lyrics.GetLyrics(geniusLink);

                Sys.debug("Adding lyrics: " + geniusLink);

                await CustomMetaData.Add(output, albumImage, artist.toCapitalFirst(), title, lyrics, album);

                Form1.downloadProgress.Invoke(new MethodInvoker(() =>
                {
                    Form1.downloadProgress.Value = 100;
                }));
            });
        }
    }

}