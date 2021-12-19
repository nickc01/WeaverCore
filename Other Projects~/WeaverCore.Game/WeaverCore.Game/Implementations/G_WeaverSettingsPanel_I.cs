using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Settings;

namespace WeaverCore.Game.Implementations
{
	public class G_WeaverSettingsPanel_I : WeaverSettingsPanel_I
    {
        public override void LoadSettings(GlobalSettings s)
        {
            SettingStorage.Load(s.GetType(), s);
        }

        public override void SaveSettings(GlobalSettings s)
        {
            SettingStorage.Save(s);
            foreach (var settings in GlobalSettings.GetAllSettings())
            {
                if (settings.GetType() == s.GetType() && settings != s)
                {
                    SettingStorage.Load(settings.GetType(), settings);
                }
            }
        }
    }
}
