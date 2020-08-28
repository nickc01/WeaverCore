using UnityEngine;
using WeaverCore.Configuration;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class ModSettings_I : IImplementation
    {


        public abstract void SaveSettings(ModSettings settings);
        public abstract void LoadSettings(ModSettings settings);
    }
}
