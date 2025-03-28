using System;
using System.Drawing;
using System.Windows.Forms;
using TrackTrek.Miscs;
using TrackTrek.Settings;
using TrackTrek.UI;

namespace TrackTrek
{
    public partial class Form1 : Form
    {
        private SearchBar searchBox;
        private SearchButton searchButton;
        public static ListView resultsList;
        public static ListView downloadQueue;
        private Button settingsButton;
        public static ProgressBar downloadProgress;

        public Form1()
        {
            try
            {
                InitializeComponent();
                this.Icon = new Icon("logo.ico");
                SetupUI();
            }
            catch (Exception e)
            {
                Sys.debug(e.ToString());
                File.WriteAllText(DateTime.Today.ToString("yyyy-MM-dd") + "_error.txt", e.ToString());
                MessageBox.Show("An unexpected error occurred: " + e.ToString());
            }
            ;
        }

        private void SetupUI()
        {
            this.Text = "TrackTrek - YouTube Music Downloader";
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            base.Size = new Size(800, 600);
            base.AutoScaleMode = AutoScaleMode.Font;
            this.searchBox = new SearchBar
            {
                Width = 400,
                Top = 20,
                Left = 10,
                PlaceholderText = "Search for songs...",
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right)
            };
            this.searchButton = new SearchButton
            {
                Text = "Search",
                Left = 420,
                Top = 18,
                Width = 80,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = (AnchorStyles.Top | AnchorStyles.Right)
            };
            this.settingsButton = new SettingsButton
            {
                Text = "⚙",
                Left = 510,
                Top = 18,
                Width = 40,
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Anchor = (AnchorStyles.Top | AnchorStyles.Right)
            };
            Form1.resultsList = new ListView
            {
                Top = 60,
                Left = 10,
                Width = 760,
                Height = 250,
                View = View.Details,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FullRowSelect = true,
                Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
            };
            Form1.resultsList.Columns.Add("Thumbnail", 100);
            Form1.resultsList.Columns.Add("Title", 250);
            Form1.resultsList.Columns.Add("Artist", 150);
            Form1.downloadQueue = new ListView
            {
                Top = 330,
                Left = 10,
                Width = 760,
                Height = 150,
                View = View.Details,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                FullRowSelect = true,
                Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
            };
            Form1.downloadQueue.Columns.Add("Path", 500);
            Form1.downloadQueue.Columns.Add("Status", 200);
            Form1.downloadProgress = new DownloadProgressBar
            {
                Top = 500,
                Left = 10,
                Width = 760,
                Height = 20,
                Anchor = (AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right)
            };
            Program.add_controls(base.Controls, this.searchBox, this.searchButton, this.settingsButton, Form1.resultsList, Form1.downloadQueue, Form1.downloadProgress);
            base.Resize += this.Form1_Resize;
        }

        private void SearchButton_Click(object sender, EventArgs e)
        {
            resultsList.Items.Clear();
            for (int i = 1; i <= 5; i++)
            {
                var item = new ListViewItem($"Thumbnail {i}");
                item.SubItems.Add($"Song Title {i}");
                item.SubItems.Add($"Artist {i}");
                item.SubItems.Add($"3:{i}0");
                resultsList.Items.Add(item);
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Settings menu will open here.", "Settings", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            int padding = 10;

            searchBox.Width = ClientSize.Width - 150;
            searchButton.Left = ClientSize.Width - 130;
            settingsButton.Left = ClientSize.Width - 50;
            resultsList.Width = ClientSize.Width - padding * 2;
            resultsList.Height = (ClientSize.Height - 120) / 2;
            downloadQueue.Top = resultsList.Bottom + padding;
            downloadQueue.Width = ClientSize.Width - padding * 2;
            downloadQueue.Height = (ClientSize.Height - 180) / 3;
            downloadProgress.Top = downloadQueue.Bottom + padding;
            downloadProgress.Width = ClientSize.Width - 30;
        }


        private void InitializeComponent()
        {
            SuspendLayout();
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 600);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }
    }
}