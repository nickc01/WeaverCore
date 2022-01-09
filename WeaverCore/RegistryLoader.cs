using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Internal;

namespace WeaverCore
{
    /// <summary>
    /// Used to load the registries for WeaverCore Mods
    /// </summary>
    public static class RegistryLoader
    {
        static HashSet<Assembly> LoadedAssemblies = new HashSet<Assembly>();

        /// <summary>
        /// Loads all Registries that are a part of the specified mod
        /// </summary>
        /// <typeparam name="Mod">The mod to load the registries from</typeparam>
        public static void LoadAllRegistries<Mod>() where Mod : WeaverMod
        {
            LoadAllRegistries(typeof(Mod));
        }

        /// <summary>
        /// Loads all Registries that are a part of the specified mod
        /// </summary>
        /// <param name="modType">The mod to load the registries from</param>
        public static void LoadAllRegistries(Type modType)
        {
            LoadAllRegistries(modType.Assembly);
        }

        /// <summary>
        /// Loads all registries from an assembly
        /// </summary>
        /// <param name="assembly">The assembly to load the registries from</param>
        public static void LoadAllRegistries(Assembly assembly)
        {
            if (!LoadedAssemblies.Contains(assembly))
            {
                LoadedAssemblies.Add(assembly);
                var loader = ImplFinder.GetImplementation<RegistryLoader_I>();
                loader.LoadRegistries(assembly);
            }
        }

        /// <summary>
        /// Loads any registries that are embedded inside of the assembly as an embedded resource asset bundle
        /// </summary>
        /// <param name="assembly">The assembly to load from</param>
        public static void LoadEmbeddedRegistries(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            WeaverLog.Log("Loading Embedded Registries for [" + assemblyName + "]");
            string extension = null;
            if (SystemInfo.operatingSystem.Contains("Windows"))
            {
                extension = ".bundle.win";
            }
            else if (SystemInfo.operatingSystem.Contains("Mac"))
            {
                extension = ".bundle.mac";
            }
            else if (SystemInfo.operatingSystem.Contains("Linux"))
            {
                extension = ".bundle.unix";
            }

            try
            {
                if (assembly != typeof(WeaverMod).Assembly)
                {
                    List<AssetBundle> loadedBundles = new List<AssetBundle>();
                    foreach (var name in assembly.GetManifestResourceNames())
                    {
                        if (name.EndsWith(extension))
                        {
                            var bundle = AssetBundle.LoadFromStream(assembly.GetManifestResourceStream(name));

                            if (bundle != null)
                            {
                                loadedBundles.Add(bundle);
                            }
                        }
                    }

                    foreach (var bundle in loadedBundles)
                    {
                        if (!bundle.isStreamedSceneAssetBundle)
                        {
                            WeaverLog.Log("Loading bundle for Weaver Mod : " + bundle.name);
                            foreach (var registry in bundle.LoadAllAssets<Registry>())
                            {
                                if (registry.ModType.Assembly.GetName().Name == assemblyName)
                                {
                                    registry.EnableRegistry();
                                }
                            }
                        }
                        else
                        {
                            WeaverLog.Log("Loading scene bundle for Weaver Mod : " + bundle.name);
                        }
                    }
                }
            }
            catch (NotSupportedException error)
            {
                if (!error.Message.Contains("not supported in a dynamic module"))
                {
                    throw;
                }
            }
        }
    }
}
