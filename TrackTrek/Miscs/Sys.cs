using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackTrek.Miscs
{
    internal class Sys
    {
        public static void debug(string msg) {
            if (Program.debug == true)
            {
                Console.WriteLine(msg);
                string exist = "";

                try
                {
                    exist += $"{File.ReadAllText(DateTime.Today.ToString("yyyy-MM-dd") + "_debug.txt")}\n";
                } catch
                {
                }
                exist += $"[{DateTime.Now.Hour.ToString()}:{DateTime.Now.Minute.ToString()}:{DateTime.Now.Second.ToString()}] {msg.ToString()}";
                File.WriteAllText(DateTime.Today.ToString("yyyy-MM-dd") + "_debug.txt", exist);
            }
        }
    }
}

