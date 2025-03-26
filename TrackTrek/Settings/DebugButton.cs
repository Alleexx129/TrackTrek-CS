using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TrackTrek.Settings
{
    internal class DebugButton : CheckBox
    {

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            string jsonString = File.ReadAllText("Settings.json");
            JsonNode json = JsonNode.Parse(jsonString);

            json["debug"] = !Program.debug;

            File.WriteAllText("Settings.json", json.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));

            Program.debug = !Program.debug;
            this.Text = $"Debug: {Program.debug.ToString()}";
        }
    }
}
