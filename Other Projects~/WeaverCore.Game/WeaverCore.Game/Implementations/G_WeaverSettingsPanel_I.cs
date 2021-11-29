using Modding;
using Modding.Menu;
using Modding.Menu.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Attributes;
using WeaverCore.Implementations;
using WeaverCore.Settings;

namespace WeaverCore.Game.Implementations
{
    class GlobalSettingsModMenuDisplay : IMenuMod
    {
        static ModVersionDraw versionDrawer;

        public bool ToggleButtonInsideMenu => false;


        public GlobalSettings Settings { get; private set; }

        public GlobalSettingsModMenuDisplay(GlobalSettings settings)
        {
            Settings = settings;
        }

        /*public MenuScreen GetMenuScreen(MenuScreen modListMenu, ModToggleDelegates? toggleDelegates)
        {
            if (versionDrawer == null)
            {
                versionDrawer = GameObject.FindObjectOfType<ModVersionDraw>();
                versionDrawer.drawString = Regex.Replace(versionDrawer.drawString,@"\{TEMP.+?TEMP\}","");
            }


        }*/

        public List<IMenuMod.MenuEntry> GetMenuData(IMenuMod.MenuEntry? toggleButtonEntry)
        {
            
        }


        public string GetName()
        {
            return "{TEMP";
        }

        public List<(string, string)> GetPreloadNames()
        {
            return null;
        }

        public string GetVersion()
        {
            return "TEMP}";
        }

        public void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            
        }

        public int LoadPriority()
        {
            return 0;
        }

        public void Log(string message)
        {
            Modding.Logger.Log(message);
        }

        public void Log(object message)
        {
            Modding.Logger.Log(message);
        }

        public void LogDebug(string message)
        {
            Modding.Logger.LogDebug(message);
        }

        public void LogDebug(object message)
        {
            Modding.Logger.LogDebug(message);
        }

        public void LogError(string message)
        {
            Modding.Logger.LogError(message);
        }

        public void LogError(object message)
        {
            Modding.Logger.LogError(message);
        }

        public void LogFine(string message)
        {
            Modding.Logger.LogFine(message);
        }

        public void LogFine(object message)
        {
            Modding.Logger.LogFine(message);
        }

        public void LogWarn(string message)
        {
            Modding.Logger.LogWarn(message);
        }

        public void LogWarn(object message)
        {
            Modding.Logger.LogWarn(message);
        }
    }


    public class G_WeaverSettingsPanel_I : WeaverSettingsPanel_I
    {
        static bool loaded = false;

        [AfterModLoad(typeof(WeaverMod))]
        static void AfterModLoaded()
        {
            if (!loaded)
            {
                loaded = true;
                foreach (var settings in Registry.GetAllFeatures<GlobalSettings>(null,false))
                {
                    AddMenuMod(settings);
                }
            }
        }

        static Type ModLoaderType;
        static Type ModInstanceType;

        static FieldInfo ModInstance_Mod;
        static FieldInfo ModInstance_Name;
        static FieldInfo ModInstance_Enabled;

        static FieldInfo ModLoader_ModInstanceTypeMap;
        static FieldInfo ModLoader_ModInstanceNameMap;
        static FieldInfo ModLoader_ModInstances;

        static void AddMenuMod(GlobalSettings settings)
        {
            if (ModLoaderType == null)
            {
                ModLoaderType = typeof(IMod).Assembly.GetType("Modding.ModLoader");
                ModInstanceType = ModLoaderType.GetNestedType("ModInstance");

                ModInstance_Mod = ModInstanceType.GetField("Mod");
                ModInstance_Name = ModInstanceType.GetField("Name");
                ModInstance_Enabled = ModInstanceType.GetField("Enabled");

                //AddModInstance = ModLoaderType.GetMethod("AddModInstance");
                ModLoader_ModInstanceTypeMap = ModLoaderType.GetField("ModInstanceTypeMap");
                ModLoader_ModInstanceNameMap = ModLoaderType.GetField("ModInstanceNameMap");
                ModLoader_ModInstances = ModLoaderType.GetField("ModInstances");
            }

            var display = new GlobalSettingsModMenuDisplay(settings);
            var modInstance = CreateBasicModInstance(display);

            var typeMap = (IDictionary)ModLoader_ModInstanceTypeMap.GetValue(null);
            if (!typeMap.Contains(display.GetType()))
            {
                typeMap.Add(display.GetType(), modInstance);
            }

            var nameMap = (IDictionary)ModLoader_ModInstanceNameMap.GetValue(null);
            if (!nameMap.Contains(display.GetType()))
            {
                nameMap.Add(display.GetType(), modInstance);
            }

            var hashSet = ModLoader_ModInstances.GetValue(null);

            hashSet.GetType().GetMethod("Add").Invoke(hashSet,new object[] { modInstance });

        }

        static object CreateBasicModInstance(IMod mod)
        {
            var instance = Activator.CreateInstance(ModInstanceType);
            ModInstance_Mod.SetValue(instance, mod);
            ModInstance_Name.SetValue(instance, mod.GetName());
            ModInstance_Enabled.SetValue(instance, true);
            return instance;
        }
    }
}
