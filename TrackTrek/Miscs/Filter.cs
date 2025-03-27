using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TrackTrek.Miscs
{
    internal class Filter
    {
        public static string FilterArtistName(YoutubeExplode.Common.Author artistName)
        {

            return Regex.Replace(artistName.ToString(), @"\([^\)]*\)|\s*- topic$|\s*- Topic$|\s* official$|\s* Official$", "");
        }
    }
}