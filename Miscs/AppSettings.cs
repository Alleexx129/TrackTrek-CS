using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackTrek.Avalonia.Miscs
{
    public static class AppSettings
    {
        public static bool Debug = true;//{ get; set; }

        public static int MaxResults { get; set; }

        public static string DownloadFolder { get; set; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads");
    }
}
