using System.Drawing;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using TrackTrek.Audio;
using TrackTrek.Miscs;
using TrackTrek.UI;
using YoutubeExplode.Common;
using YoutubeExplode;

namespace TrackTrek
{
    internal static class Program
    {

        public static Boolean debug = false;
        public static string maxResults = "10";
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
                Program.maxResults = jsonNode["maxResults"].GetValue<string>();
            }
            catch
            {
                File.WriteAllText(path, "{\"debug\": true, \"maxResults\": \"10\"}");
                Program.debug = jsonNode["debug"].GetValue<bool>();
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
                Sys.debug(e.ToString());
                File.WriteAllText(DateTime.Today.ToString("yyyy-MM-dd") + "_error.txt", e.ToString());
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
                ListViewItem resultItem = resultsList.SelectedItems[0];
                ListViewItem newItem = new ListViewItem("Fetching...");

                string title = resultItem.SubItems[1].Text;
                string artist = resultItem.SubItems[2].Text;
                string album = resultItem.SubItems[3].Text;

                newItem.SubItems[1].Text = "Loading...";
                downloadQueue.Items.Add(newItem);

                YoutubeExplode.Videos.Video videoInfo = await Searching.GetVideo(title + " - " + artist);

                Sys.debug("Starting download...");

                string output = await Download.EnqueueDownload(artist, title, videoInfo.Url, newItem);

                Sys.debug("Audio downloaded!: " + output);

                Thumbnail thumbnail = videoInfo.Thumbnails[videoInfo.Thumbnails.Count - 1];

                Sys.debug("Adding metadata: " + thumbnail.Url);

                string albumImageUrl = await ImageUtils.GetAlbumImageUrl(album, artist);

                Sys.debug("Album Url: " + albumImageUrl);

                await CustomMetaData.Add(output, albumImageUrl, artist, title, album);

                Form1.downloadProgress.Value = 100;
            });
        }
    }
        
}