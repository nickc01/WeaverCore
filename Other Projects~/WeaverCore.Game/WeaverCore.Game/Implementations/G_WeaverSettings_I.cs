using UnityEngine;
using WeaverCore.Configuration;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class G_WeaverSettings_I : GlobalWeaverSettings_I
    {
        public override void LoadSettings(GlobalWeaverSettings s)
        {
            SettingStorage.Load(s.GetType(), s);
        }

        public override void SaveSettings(GlobalWeaverSettings s)
        {
            SettingStorage.Save(s);
            foreach (var settings in GlobalWeaverSettings.GetAllSettings())
            {
                if (settings.GetType() == s.GetType() && settings != s)
                {
                    SettingStorage.Load(settings.GetType(), settings);
                }
            }
        }
    }
}
