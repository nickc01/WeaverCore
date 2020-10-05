using UnityEngine;
using WeaverCore.Configuration;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class GlobalWeaverSettings_I : IImplementation
    {


        public abstract void SaveSettings(GlobalWeaverSettings settings);
        public abstract void LoadSettings(GlobalWeaverSettings settings);
    }
}
