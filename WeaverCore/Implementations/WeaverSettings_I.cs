using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Settings;

namespace WeaverCore.Implementations
{
	public abstract class WeaverSettingsPanel_I : IImplementation
    {
        public abstract void SaveSettings(GlobalSettings settings);
        public abstract void LoadSettings(GlobalSettings settings);
    }
}
