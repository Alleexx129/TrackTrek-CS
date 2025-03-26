using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackTrek.Miscs;

namespace TrackTrek.Settings
{
    internal class SettingsButton : Button
    {
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            new SettingsFrame().Show();
        }
    }
}
