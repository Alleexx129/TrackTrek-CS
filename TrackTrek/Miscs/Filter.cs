using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp.Text;
using YoutubeExplode.Videos;

namespace TrackTrek.Miscs
{
    internal class Filter
    {
        private static List<string> blacklistedVideoKeywords = new List<string> { "(Clean Version)", "Official HD Video", "Official Video", "】", "(HD", "(Live", "(Deluxe", "( Video)", "( 4K Video)" }; // The program will ignore any video with these (Some live videos don't have the exact same sound as studio quality)
        private static List<string> deletedVideoKeywords = new List<string> { "HD", "lyrics", "Lyrics", "Official", "official", "(Clean Version)", "(Live)", "(Official Audio)", "(Remastered)", "(HQ)", "(  Video)", "( Audio)", "(Audio)", "[HQ]", "[Official Music Video]", "[Extended]", "[LYRICS]", "(Official Video)", "(official video)", "(Video Edit)", "( Video)", "( 4K Video)", "( Lyric Video)", "( Lyrics Video)" }; // The program will acccept videos with these keywords, but will delete these keywords in the title

        public static string FilterArtistName(string artistName)
        {
            return Regex.Replace(artistName, @"\([^\)]*\)|\s*- topic$|\s*- Topic$|\s* official$|\s* Official$", "");
        }


        public static Boolean BlacklistedVideo(string title)
        {
            return blacklistedVideoKeywords.Any(title.Contains);
        }

        public static string FilterTitle(string title)
        {
            return deletedVideoKeywords.Aggregate(title, (current, word) => current.Replace(word, "").Replace("/", "-"));
        }

        public static string ToGeniusLink(string title, string artist)
        {
            string finalTitle = Regex.Replace(title, @"[^a-zA-Z0-9 ]", "").Replace(" ", "-");
            string finalArtist = Regex.Replace(artist, @"[^a-zA-Z0-9 ]", "").Replace(" ", "-");

            return $"https://genius.com/{finalArtist}-{finalTitle}-lyrics";
        }

        public static string[] ToTitleAndArtist(string title, string artist)
        {
            string newTitle = FilterTitle(title);
            string newArtist = FilterArtistName(artist);

            if (newTitle.Contains(" - "))
            {
                newArtist = newTitle.Substring(0, newTitle.IndexOf(" - ")).Replace(" - ","");
                newTitle = newTitle.Replace(newArtist, "").Replace(" - ", "");
            }

            return new string[] {newTitle, newArtist };
        }
    }
}