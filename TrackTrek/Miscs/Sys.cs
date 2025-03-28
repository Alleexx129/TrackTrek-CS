using System;
using System.Collections.Generic;
using System.IO;
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
                string mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrackTrek");

                Console.WriteLine(msg);
                string exist = "";

                try
                {
                    exist += $"{File.ReadAllText(Path.Combine(mainPath, DateTime.Today.ToString("yyyy-MM-dd") + "_debug.txt"))}\n";
                } catch
                {
                }
                exist += $"[{DateTime.Now.Hour.ToString()}:{DateTime.Now.Minute.ToString()}:{DateTime.Now.Second.ToString()}] {msg.ToString()}";
                File.WriteAllText(Path.Combine(mainPath, DateTime.Today.ToString("yyyy-MM-dd") + "_debug.txt"), exist);
            }
        }

        public static void initialize()
        {
            string mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrackTrek");
            string settingsPath = Path.Combine(mainPath, "Settings.json");

            if (!Directory.Exists(mainPath))
            {
                Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrackTrek"));
            }

            if (!File.Exists(settingsPath))
            {
                File.WriteAllText(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrackTrek"), "Settings.json"),  "{\"debug\": true, \"maxResults\": \"10\"}");
            }
        }
    }
}

