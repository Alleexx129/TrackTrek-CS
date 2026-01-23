using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using FuzzySharp;
using HtmlAgilityPack;
using System;
using System.Collections;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using TagLib.Riff;

namespace TrackTrek.Miscs
{
    internal class Lyrics
    { // add keybinds some day like enter
        private static readonly HttpClient client = new HttpClient();

        public async static Task<string> GetLyrics(string Link)
        {
            string[] list = {};

            // Filter.ToGeniusLink
            string html = await client.GetStringAsync(Link);
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            foreach (HtmlNode lyricsElement in doc.GetElementbyId("lyrics-root").ChildNodes)
            {
                foreach (string className in lyricsElement.GetClasses())
                {
                    if (className.Contains("Lyrics__Container") && lyricsElement.Descendants().Count() > 0) {
                        //list = list.Append<string>(lyricsElement.GetDirectInnerText()).ToArray();
                        var descen = lyricsElement.Descendants();
                        foreach (var ly in lyricsElement.Descendants().ToFrozenSet())
                        {
                            if (HtmlExtras.HasAttributeName("data-exclude-from-selection", ly))
                            {
                                ly.Remove();
                            }

                            if (ly.OriginalName == "br")
                            {
                                ly.InnerHtml = "\n";
                            };
                        }


                        foreach (var ly in descen)
                        {
                            if (ly.InnerText != null && ly.InnerText != "" && !ly.HasChildNodes)
                            {
                                list = list.Append(ly.GetDirectInnerText().Trim()+"\n").ToArray();
                            }
                        }
                        var last = list.Last() + "\n";
                        list[list.Length - 1] = last;
                    }
                }
            }

            string lyrics = String.Join(" ", list).Replace("&#x27;", "'").Trim();

            lyrics = lyrics.Replace("(\n", "(").Replace("( ", "(");
            lyrics = lyrics.Replace("\n )", ")");

            lyrics = lyrics.Replace("[\n", "[").Replace("[ ", "[");
            lyrics = lyrics.Replace("\n ]", "]");

            lyrics = lyrics.Replace("\n \n", "\n");
            lyrics = lyrics.Replace("\n ", "\n");
            lyrics = lyrics.Replace("&amp;", "&");
            lyrics = lyrics.Replace("&\n", "& ");
            //lyrics = Regex.Replace(lyrics, @"(\w)([A-Z])", "$1 $2");
            //lyrics = Regex.Replace(lyrics, @"\[", "\n\n[");
            //lyrics = Regex.Replace(lyrics, @"\]", "] ");
            //lyrics.Replace("")

            // might use regex later but not for now
            return lyrics;
        }

        public static string ToGeniusLink(string title, string artist)
        {
            string finalTitle = Regex.Replace(title, @"[^a-zA-Z0-9 ]", "").Replace(" ", "-");
            string finalArtist = Regex.Replace(artist, @"[^a-zA-Z0-9 ]", "").Replace(" ", "-");

            return $"https://genius.com/{finalArtist}-{finalTitle}-lyrics";
        }
    }
}
