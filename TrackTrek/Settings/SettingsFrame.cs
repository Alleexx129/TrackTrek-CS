using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TagLib.Id3v2;
using TrackTrek.UI;

namespace TrackTrek.Settings
{
    internal class SettingsFrame : Form
    {
        private DebugButton debugButton;

        public SettingsFrame()
        {
            try
            {
                this.Icon = new Icon("logo.ico");
                SetupUI();
                addControls();
            }
            catch (Exception e)
            {
                File.WriteAllText(DateTime.Today.ToString("yyyy-MM-dd") + "_error.txt", e.ToString());
                MessageBox.Show("An unexpected error occurred: " + e.ToString());
            };
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
        }

        private void addControls()
        {
            Controls.Add(debugButton);
        }
    }
}
