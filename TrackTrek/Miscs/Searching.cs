using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;

namespace TrackTrek.Miscs
{
    internal class Searching
    {
        public static Boolean CheckIfLink(string text)
        {
            return text.ToLower().Contains("https") || text.ToLower().Contains("youtube.com") || text.ToLower().Contains("youtu.be");
        }

        public static async Task<Video> GetVideo(string query)
        {
            YoutubeClient youtube = new YoutubeClient();
            ISearchResult found = null;

            await foreach(var video in youtube.Search.GetResultsAsync(query)) {
                if (!Filter.BlacklistedVideo(video.Title))
                {
                    found = video;
                    break;
                }
            }
            return await youtube.Videos.GetAsync(found.Url);
        }
    }   
    
}
