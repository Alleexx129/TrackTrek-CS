using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackTrek.Miscs
{
    public static class StringExtras
    {
        public static string toCapitalFirst(this string str)
        {
            string lower = str.ToLower();
            var final = new List<string>();

            foreach (string word in lower.Split(' '))
            {
                final.Add(word[0].ToString().ToUpper() + word.Remove(0, 1));
            }

            return string.Join(" ", final);
        }
    }
}
