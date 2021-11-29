using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
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
                return ModHooks.GetAllMods();
#endif
            }
        }


        //As soon as any WeaverCore mod loads, the init functions will be called
        protected WeaverMod() : this(null)
		{

		}

        protected WeaverMod(string name) : base(name)
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

                WeaverLog.Log("Loading Weaver Mod " + modType.Name);
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
                //registry.RegistryEnabled = true;
                registry.EnableRegistry();
            }
        }

        public void DisableRegistries()
        {
            foreach (var registry in Registries)
            {
                //registry.RegistryEnabled = false;
                registry.DisableRegistry();
            }
        }
    }
}
