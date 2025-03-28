using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YoutubeExplode.Videos;

namespace TrackTrek.Miscs
{
    internal class Filter
    {
        private static List<string> blacklistedVideoKeywords = new List<string> { "video", "Video", "live", "Live", "(Live)", "(Deluxe" }; // The program will ignore any video with these (Some live videos don't have the exact same sound as studio quality)
        private static List<string> deletedVideoKeywords = new List<string> { "HD", "lyrics", "Lyrics", "Official", "official", "(clean version)", "(Live)"}; // The program will acccept videos with these keywords, but will delete these keywords in the title

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
            return deletedVideoKeywords.Aggregate(title, (current, word) => current.Replace(word, ""));
        }

        public static string ToGeniusLink(string title, string artist)
        {
            string finalTitle = Regex.Replace(title, @"[^a-zA-Z0-9 ]", "").Replace(" ", "-");
            string finalArtist = Regex.Replace(artist, @"[^a-zA-Z0-9 ]", "").Replace(" ", "-");

            return $"https://genius.com/{finalArtist}-{finalTitle}-lyrics";
        }
    }
}