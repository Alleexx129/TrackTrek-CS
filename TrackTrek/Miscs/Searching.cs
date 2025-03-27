using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace TrackTrek.Miscs
{
    internal class Searching
    {
        public static Boolean CheckIfLink(string text)
        {
            return text.ToLower().Contains("https") || text.ToLower().Contains("youtube.com") || text.ToLower().Contains("youtu.be");
        }

        private static List<string> BlacklistedVideoKeywords = new List<string> {"video", "Video", "live", "Live"}; // The program will ignore any video with these (Some official videos don't have the exact same sound)
        private static List<string> DeletedVideoKeywords = new List<string> { "HD", "lyrics", "Lyrics", "Official", "official"}; // The program will acccept videos with these keywords, but will delete these keywords in the title
    
        public static Boolean BlacklistedVideo(string title)
        {
            return BlacklistedVideoKeywords.Any(title.Contains);
        }

        public static string FilterTitle(string title)
        {
            return DeletedVideoKeywords.Aggregate(title, (current, word) => current.Replace(word, ""));
        }
    }   
    
}
