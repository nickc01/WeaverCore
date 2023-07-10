using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.Attributes;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore
{
    /// <summary>
    /// The base class for all WeaverCore Mods
    /// </summary>
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

        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }

        [AfterModLoad(typeof(WeaverMod))]
        static void WeaverModLoaded(WeaverMod loadedMod)
        {
            if (loadedMod.firstLoad)
            {
                loadedMod.firstLoad = false;

                var modType = loadedMod.GetType();

                WeaverLog.Log("Loading Weaver Mod " + modType.Name);
                RegistryLoader.LoadAllRegistries(modType);
            }
            else
            {
                loadedMod.EnableRegistries();
            }
        }

        [AfterModUnload(typeof(WeaverMod))]
        static void WeaverModUnloaded(WeaverMod unloadedMod)
        {
            unloadedMod.DisableRegistries();
        }

        public void EnableRegistries()
        {
            foreach (var registry in Registries)
            {
                registry.EnableRegistry();
            }
        }

        public void DisableRegistries()
        {
            foreach (var registry in Registries)
            {
                registry.DisableRegistry();
            }
        }
    }
}
