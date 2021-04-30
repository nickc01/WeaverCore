using UnityEngine;
using WeaverCore.Configuration;
using WeaverCore.Interfaces;
using WeaverCore.Settings;

namespace WeaverCore.Implementations
{
	public abstract class WeaverSettingsPanel_I : IImplementation
    {
        public abstract void SaveSettings(Panel settings);
        public abstract void LoadSettings(Panel settings);
    }
}
