using FuzzySharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackTrek.Miscs
{
    public static class StringExtras
    {
        public static string ToCapitalFirst(this string str)
        {
            string lower = str.ToLower();
            var final = new List<string>();

            foreach (string word in lower.Split(' '))
            {
                final.Add(word[0].ToString().ToUpper() + word.Remove(0, 1));
            }

            return string.Join(" ", final);
        }

        public static string CheckStringType(this string text)
        {
            if (text.Contains("?list=", StringComparison.OrdinalIgnoreCase))
            {
                return "playlist";
            }

            if (text.Contains("https", StringComparison.OrdinalIgnoreCase) || text.Contains("youtube.com", StringComparison.OrdinalIgnoreCase) || text .Contains("youtu.be", StringComparison.OrdinalIgnoreCase))
            {
                return "link";
            }
            if (text != "")
            {
                return "keyword";
            } else
            {
                return "empty";
            }
        }

        public static string ToArtistDashTitle(this string str, string artist)
        {
            string[] splitString = str.Split(" - ");

            if (Fuzz.Ratio(splitString[0].ToLower(), artist.ToLower()) > 70)
            {
                return splitString[0] + " - " + splitString[1];
            }
            else
            {
                 return splitString[1] + " - " + splitString[0];
            }
        }
    }
}
