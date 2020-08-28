using UnityEngine;
using WeaverCore.Configuration;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	public class G_ModSettings_I : ModSettings_I
    {
        public override void LoadSettings(ModSettings settings)
        {
            SettingStorage.Load(settings.GetType(), settings);
        }

        public override void SaveSettings(ModSettings settings)
        {
            SettingStorage.Save(settings);
        }
    }
}
