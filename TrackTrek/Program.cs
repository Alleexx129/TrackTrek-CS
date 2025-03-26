using System.Drawing;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using TrackTrek.UI;

namespace TrackTrek
{
    internal static class Program
    {

        public static Boolean debug = false;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!File.Exists("Settings.json"))
            {
                File.WriteAllText("Settings.json", "{\"debug\": true}");
            }
            
            string jsonString = File.ReadAllText("Settings.json");
            JsonNode json = JsonNode.Parse(jsonString);

            debug = json["debug"].GetValue<bool>() || false;
            try
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());
                }
                catch (Exception e)
                {
                    File.WriteAllText(DateTime.Today.ToString("yyyy-MM-dd") + "_error.txt", e.ToString());
                    MessageBox.Show("An unexpected error occurred: " + e.ToString());
                };
            


        

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
        }
    }
}