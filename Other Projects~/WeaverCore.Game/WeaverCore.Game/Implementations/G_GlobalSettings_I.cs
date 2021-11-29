using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverCore.Implementations;
using WeaverCore.Settings;

namespace WeaverCore.Game.Implementations
{
    public class G_GlobalSettings_I : GlobalSettings_I
    {
        public override void LoadSettings(GlobalSettings settings)
        {
            SettingStorage.Load(settings.GetType(), settings);
        }

        public override void SaveSettings(GlobalSettings settings)
        {
            SettingStorage.Save(settings);
            foreach (var addedSettings in Registry.GetAllFeatures<GlobalSettings>())
            {
                if (addedSettings.GetType() == settings.GetType() && addedSettings != settings)
                {
                    SettingStorage.Load(settings.GetType(), settings);
                }
                //addedSettings.
            }
            /*foreach (var settings in Panel.GetAllSettings())
            {
                if (settings.GetType() == s.GetType() && settings != s)
                {
                    SettingStorage.Load(settings.GetType(), settings);
                }
            }*/
        }
    }
}
