using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverCore.Interfaces;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Implementations
{
    public abstract class GlobalSettings_I : IImplementation
    {
        static GlobalSettings_I _impl;
        public static GlobalSettings_I Impl
        {
            get
            {
                if (_impl == null)
                {
                    _impl = ImplFinder.GetImplementation<GlobalSettings_I>();
                }
                return _impl;
            }
        }

        //List<GlobalSettings> loadedSettings = new List<GlobalSettings>();
        //public bool Loaded { get; private set; } = false;


        /*void InitAllSettings()
        {
            if (!Loaded)
            {
                Loaded = true;
                foreach (var settingsData in Registry.GetAllFeatures<GlobalSettings>())
                {
                    try
                    {
                        Init(settingsData);
                        loadedSettings.Add(settingsData);
                    }
                    catch (Exception e)
                    {
                        WeaverLog.LogError($"Error Loading Mod Settings {settingsData?.GetType().FullName.ToString() ?? "Null Type"}");
                        WeaverLog.LogException(e);
                    }
                }
            }
        }

        void DestroyAllSettings()
        {
            if (Loaded)
            {
                Loaded = false;
                foreach (var settingsData in loadedSettings)
                {
                    try
                    {
                        Destroy(settingsData);
                    }
                    catch (Exception e)
                    {
                        WeaverLog.LogError($"Error Unloading Mod Settings {settingsData?.GetType().FullName.ToString() ?? "Null Type"}");
                        WeaverLog.LogException(e);
                    }
                }
                loadedSettings.Clear();
            }
        }


        public abstract void Init(GlobalSettings settings);
        public abstract void Destroy(GlobalSettings settings);*/


        public abstract void LoadSettings(GlobalSettings settings);
        public abstract void SaveSettings(GlobalSettings settings);
    }
}
