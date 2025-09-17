using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Threading.Tasks;
using TagLib.Id3v2;
using TrackTrek.UI;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Drawing;

namespace TrackTrek.Settings
{
    internal class SettingsFrame : Form
    {
        private DebugButton debugButton;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private protected TextBox maxResultText;
        private protected Button customPathText;
        private Label maxResultLabel;
        private Label customPathDisplay;
        private Label customPathLabel;

        public SettingsFrame()
        {

            this.Icon = new Icon("logo.ico");
            SetupUI();
            addControls();
        }
        private void SetupUI()
        {
            Text = "TrackTrek - Settings";
            BackColor = Color.FromArgb(30, 30, 30);
            ForeColor = Color.White;
            Size = new Size(400, 300);
            AutoScaleMode = AutoScaleMode.Font;

            debugButton = new DebugButton
            {
                Width = 200,
                Top = 20,
                ForeColor = Color.White,
                Left = 10,
                Text = $"Debug: {Program.debug.ToString()}",
                Checked = Program.debug
            };

            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();

            groupBox1.Size = new Size(400, 150);
            groupBox2.Top = 68;
            groupBox2.Size = new Size(260, 37);
            groupBox2.Left = 110;

            Size = new Size(450, 200);
            maxResultText = new TextBox
            {
                PlaceholderText = "Enter a number...",
                Text = Program.maxResults.ToString(),
                Width = 40,
                Top = 45,
                Left = 10,
            };

            maxResultLabel = new Label
            {
                Text = "Max Results",
                ForeColor = Color.White,
                Width = 100,
                Top = 45,
                Left = 50
            };

            customPathText = new Button
            {
                Text = "Select a Folder",
                BackColor = Color.White,
                ForeColor = Color.Black,
                Width = 100,
                Height = 30,
                Top = 75,
                Left = 10
            };

            customPathLabel = new Label
            {
                Text = "Custom Path",
                BackColor = Color.Transparent,
                ForeColor = Color.White,
                Width = 100,
                Top = 8,
                Height = 15,
                Left = 5
            };

            customPathDisplay = new Label
            {
                Text = Program.customPath.Replace("\\\\", "\\"),
                ForeColor = Color.White,
                Width = 250,
                Height = 15,
                Top = 20,
                Left = 5
            };

            if (Program.customPath.Length > 36)
            {
                customPathDisplay.Text = Program.customPath.Replace("\\\\", "\\").Replace(Program.customPath.Replace("\\\\", "\\").Substring(36), "") + "...";
            }
            ;


                maxResultText.TextChanged += (sender, e) =>
            {
                if (!Regex.IsMatch(maxResultText.Text, "^[0-9]+$"))
                {
                    maxResultText.Text = Program.maxResults;
                    if (!Regex.IsMatch(maxResultText.Text, "^[0-9]+$"))
                    {
                        maxResultText.Text = "10";
                    }
                }
                string mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrackTrek");
                string settingsPath = Path.Combine(mainPath, "Settings.json");

                Program.maxResults = maxResultText.Text;
                string jsonString = File.ReadAllText(settingsPath);
                JsonNode json = JsonNode.Parse(jsonString);

                json["maxResults"] = maxResultText.Text;

                File.WriteAllText(settingsPath, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            };

            customPathText.MouseClick += (sender, e) =>
            {
                FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                folderBrowser.ShowDialog();
                folderBrowser.Description = "Select a folder to save downloaded musics";
                folderBrowser.RootFolder = Environment.SpecialFolder.MyComputer;
                string mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrackTrek");
                string settingsPath = Path.Combine(mainPath, "Settings.json");

                if (folderBrowser.SelectedPath == "")
                {
                    return;
                }
                Program.customPath = folderBrowser.SelectedPath;
                if (Program.customPath.Length < 36)
                {
                    customPathDisplay.Text = Program.customPath;
                }
                else
                {
                    customPathDisplay.Text = folderBrowser.SelectedPath.Replace(folderBrowser.SelectedPath.Substring(36), "") + "...";
                }

                string jsonString = File.ReadAllText(settingsPath);
                JsonNode json = JsonNode.Parse(jsonString);

                json["customPath"] = Program.customPath;

                File.WriteAllText(settingsPath, json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
            };
        }

        private void addControls()
        {
            Controls.Add(groupBox1);
            groupBox1.Controls.Add(debugButton);
            groupBox1.Controls.Add(maxResultLabel);
            groupBox1.Controls.Add(groupBox2);
            groupBox1.Controls.Add(maxResultText);
            groupBox2.Controls.Add(customPathLabel);
            groupBox1.Controls.Add(customPathText);
            groupBox2.Controls.Add(customPathDisplay);
        }
        
    }
    
}
