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
        static System.Collections.Generic.List<IMod> _loadedMods = new System.Collections.Generic.List<IMod>();
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


        static Dictionary<IMod, bool> firstLoadedMods = new Dictionary<IMod, bool>();
        //bool firstLoad = true;

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

        [AfterModLoad(typeof(IMod))]
        static void WeaverModLoaded(IMod loadedMod)
        {
            if (!firstLoadedMods.ContainsKey(loadedMod))
            {
                firstLoadedMods.Add(loadedMod, true);

                var modType = loadedMod.GetType();

                if (loadedMod is WeaverMod)
                {
                    WeaverLog.Log("Loading Weaver Mod " + modType.Name);
                }
                RegistryLoader.LoadAllRegistries(modType);
            }
            else
            {
                EnableRegistries(loadedMod);
            }
        }

        [AfterModUnload(typeof(IMod))]
        static void WeaverModUnloaded(IMod unloadedMod)
        {
            DisableRegistries(unloadedMod);
        }

        static IEnumerable<Registry> GetModRegistries(IMod mod)
        {
            return Registry.FindModRegistries(mod.GetType());
        }

        public static void EnableRegistries(IMod mod)
        {
            foreach (var registry in GetModRegistries(mod))
            {
                registry.EnableRegistry();
            }
        }

        public static void DisableRegistries(IMod mod)
        {
            foreach (var registry in GetModRegistries(mod))
            {
                registry.DisableRegistry();
            }
        }
    }
}
