using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Text;

namespace TrackTrek.Miscs
{
    internal class Filter
    { // add custom keywords in settings
        private static List<string> blacklistedVideoKeywords = new List<string> { "Official HD Video", "Official Video", "】", "(Live", "( 4K Video)" }; // The program will ignore any video with these (Some live videos don't have the exact same sound as studio quality)
        private static List<string> deletedVideoKeywords = new List<string> { "(Deluxe Edition)", "HD", "lyrics", "Lyrics", "Official", "official", "(Clean Version)", "(Live)", "(Official Audio)", "(Remastered)", "(HQ)", "(  Video)", "( Audio)", "(Audio)", "[HQ]", "[Official Music Video]", "[Extended]", "[LYRICS]", "(Official Video)", "(official video)", "(Video Edit)", "( Video)", "( 4K Video)", "( Lyric Video)", "( Lyrics Video)", "(Official HD Video)" }; // The program will acccept videos with these keywords, but will delete these keywords in the title
        private static List<string> deletedAlbumKeywords = new List<string> { "(Deluxe Edition)", "HD", "Official", "official", "(Clean Version)", "(Live)", "(Official Audio)", "(Remastered)", "(HQ)", "[HQ]", "[Extended]", "[LYRICS]", "(Remix)", "(Remade)", "(Gold Edition)"}; // The program will acccept videos with these keywords, but will delete these keywords in the title

        public static string FilterArtist(string artistName)
        {
            return Regex.Replace(artistName, @"\([^\)]*\)|\s*- topic$|\s*- Topic$|\s* official$|\s* Official$", "").StripLeadingTrailingSpaces().ToCapitalFirst();
        }


        public static Boolean BlacklistedVideo(string title)
        {
            return blacklistedVideoKeywords.Any(title.Contains);
        }

        public static string FilterTitle(string title)
        {
            return deletedVideoKeywords.Aggregate(title, (current, word) => current.Replace(word, "").Replace("/", "-")).StripLeadingTrailingSpaces();
        }

        public static string FilterAlbum(string albumName)
        {
            return deletedAlbumKeywords.Aggregate(albumName, (current, word) => current.Replace(word, "", StringComparison.OrdinalIgnoreCase).Replace("/", "-", StringComparison.OrdinalIgnoreCase)).StripLeadingTrailingSpaces();
        }

        public static string[] ToTitleAndArtist(string title, string artist)
        {
            string newTitle = FilterTitle(title);
            string newArtist = FilterArtist(artist);

            if (newTitle.Contains(" - "))
            {
                newArtist = newTitle.Substring(0, newTitle.IndexOf(" - ")).Replace(" - ","");
                newTitle = newTitle.Replace(newArtist, "").Replace(" - ", "");
            }

            return new string[] {newTitle, newArtist };
        }
    }
}