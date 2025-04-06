using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackTrek.UI
{
    internal class SearchBar : TextBox
    {
        public static String TextContent;

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            TextContent = this.Text;

            if (!this.Text.Contains("?list="))
            {
                Form1.searchButton.Text = "Search";
                Program.searchingPlaylist = false;
            }
        }
    }
}
