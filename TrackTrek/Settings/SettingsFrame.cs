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
        private protected TextBox maxResultText;
        private Label maxResultLabel;

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
                Width = 400,
                Top = 20,
                Left = 10,
                Text = $"Debug: {Program.debug.ToString()}",
                Checked = Program.debug
            };

            maxResultText = new TextBox
            {
                PlaceholderText = "Enter a number...",
                Text = Program.maxResults.ToString(),
                Width = 40,
                Top = 40,
                Left = 10
            };

            maxResultLabel = new Label
            {
                Text = "Max Results",
                Width = 100,
                Top = 45,
                Left = 50
            };


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
        }

        private void addControls()
        {
            Controls.Add(debugButton);
            Controls.Add(maxResultText);
            Controls.Add(maxResultLabel);
        }
    }
}
