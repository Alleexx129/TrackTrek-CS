using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackTrek.Miscs
{
    internal class HtmlExtras
    {
        public static bool HasAttributeName(string name, HtmlNode node)
        {
            IEnumerable<HtmlAttribute> attributes = node.GetAttributes();
            foreach (HtmlAttribute attr in attributes)
            {
                if (attr.Name == name)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
