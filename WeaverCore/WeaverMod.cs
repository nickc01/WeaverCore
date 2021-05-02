using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using WeaverCore.Attributes;
using WeaverCore.Configuration;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore
{
    public abstract class WeaverMod : Mod
    {
#if UNITY_EDITOR
        static List<IMod> _loadedMods = new List<IMod>();
#else
        static Type modLoaderType;
        static FieldInfo loadedMods;
#endif

        public static IEnumerable<IMod> LoadedMods
		{
            get
			{
#if UNITY_EDITOR
                return _loadedMods;
#else
				if (modLoaderType == null)
				{
                    modLoaderType = typeof(IMod).Assembly.GetType("Modding.ModLoader");
                    loadedMods = modLoaderType.GetField("LoadedMods", BindingFlags.Public | BindingFlags.Static);
                }
                return (List<IMod>)loadedMods.GetValue(null);
#endif
            }
		}


        //As soon as any WeaverCore mod loads, the init functions will be called
        protected WeaverMod()
		{
#if UNITY_EDITOR
            _loadedMods.Add(this);
#endif
            Initialization.Initialize();
		}


        bool firstLoad = true;

        public IEnumerable<Registry> Registries
        {
            get
            {
                return Registry.FindModRegistries(GetType());
            }
        }

        /*static string Prettify(string input)
        {
            return Regex.Replace(input, @"(\S)([A-Z])", @"$1 $2");
        }*/

        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (firstLoad)
            {
                firstLoad = false;

                var modType = GetType();

                RegistryLoader.LoadAllRegistries(modType);

                //WeaverLog.Log(modType.FullName + "___LOADED!!!");

                ReflectionUtilities.ExecuteMethodsWithAttribute<AfterModLoadAttribute>((_,a) => a.ModType.IsAssignableFrom(modType));

                //Load all global mod settings pertaining to this mod
                /*foreach (var registry in Registry.FindModRegistries(modType))
                {
                    var settingTypes = registry.GetFeatureTypes<GlobalWeaverSettings>();
                    foreach (var settingsType in settingTypes)
                    {
                        settingsType.Load();
                    }
                }*/
            }
            else
            {
                EnableRegistries();
            }
        }

        public void EnableRegistries()
        {

            foreach (var registry in Registries)
            {
                registry.RegistryEnabled = true;
            }
        }

        public void DisableRegistries()
        {
            foreach (var registry in Registries)
            {
                registry.RegistryEnabled = false;
            }
        }
    }
}
