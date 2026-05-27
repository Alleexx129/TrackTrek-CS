using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace TrackTrek.UI
{
    internal class SearchBar : TextBox
    {
        public static String TextContent = ""; //"https://www.youtube.com/watch?v=9a_VFK0_vb0";

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            TextContent = this.Text; //"https://www.youtube.com/watch?v=9a_VFK0_vb0";
            //this.Text = "https://www.youtube.com/watch?v=9a_VFK0_vb0";
            if (!this.Text.Contains("?list="))
            {
                Form1.searchButton.Text = "Search";
                Program.searchingPlaylist = false;
            }
        }
    }
}
