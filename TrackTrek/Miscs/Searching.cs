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
    }
}
