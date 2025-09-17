using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.VisualBasic;

namespace TrackTrek.Miscs
{
    internal class Sys
    {
        public static void debug<T>(T msg) {
            
            if (Program.debug == true)
            {
                string debugMessage = "";
                string mainPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TrackTrek");

                Console.WriteLine(msg);
                string exist = "";

                if (msg is byte[] byteArray)
                {
                    debugMessage = Encoding.UTF8.GetString(byteArray);
                }
                else
                {
                    debugMessage = Convert.ToString(msg);
                }

                try
                {
                    exist += $"{File.ReadAllText(Path.Combine(mainPath, DateTime.Today.ToString("yyyy-MM-dd") + "_debug.txt"))}\n";
                } catch
                {
                }
                exist += $"[{DateTime.Now.Hour.ToString()}:{DateTime.Now.Minute.ToString()}:{DateTime.Now.Second.ToString()}] {debugMessage}";
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

