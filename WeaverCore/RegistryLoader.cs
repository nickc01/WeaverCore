using Modding;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Internal;
using static Mono.Security.X509.X520;

namespace WeaverCore
{
    /// <summary>
    /// Used to load the registries for WeaverCore Mods
    /// </summary>
    public static class RegistryLoader
    {
        static HashSet<Assembly> LoadedAssemblies = new HashSet<Assembly>();

        static Dictionary<Assembly, List<AssetBundle>> loadedBundles = new Dictionary<Assembly, List<AssetBundle>>();

        /// <summary>
        /// Loads all Registries that are a part of the specified mod
        /// </summary>
        /// <typeparam name="Mod">The mod to load the registries from</typeparam>
        public static void LoadAllRegistries<Mod>() where Mod : IMod
        {    
            LoadAllRegistries(typeof(Mod));
        }

        /// <summary>
        /// Loads all Registries that are a part of the specified mod
        /// </summary>
        /// <param name="modType">The mod to load the registries from</param>
        public static void LoadAllRegistries(Type modType)
        {
            if (!Initialization.IsAssemblyExcluded(modType.Assembly))
            {
                Initialization.PerformanceLog($"Loading all Registries in Mod {modType.Name}");
                LoadAllRegistries(modType.Assembly);
                Initialization.PerformanceLog($"Finished loading all Registries in Mod {modType.Name}");
            }
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

        public static bool AreBundlesLoaded<T>() => AreBundlesLoaded(typeof(T));

        public static bool AreBundlesLoaded(Type modType) => AreBundlesLoaded(modType.Assembly);

        public static bool AreBundlesLoaded(Assembly assembly)
        {
            return loadedBundles.ContainsKey(assembly);
        }

        public static IEnumerable<AssetBundle> LoadBundlesOnly<T>() where T : IMod => LoadBundlesOnly(typeof(T));

        public static IEnumerable<AssetBundle> LoadBundlesOnly(Type modType) => LoadBundlesOnly(modType.Assembly);

        public static IEnumerable<AssetBundle> LoadBundlesOnly(Assembly assembly)
        {
            if (loadedBundles.TryGetValue(assembly, out var results))
            {
                return results;
            }

            var assemblyName = assembly.GetName().Name;
            //WeaverLog.Log("Loading Embedded Registries for [" + assemblyName + "]");
            Initialization.PerformanceLog($"Loading Embedding Registries for {assemblyName}");
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
                    results = new List<AssetBundle>();
                    foreach (var name in assembly.GetManifestResourceNames())
                    {
                        if (name.EndsWith(extension))
                        {
                            //WeaverLog.Log("Loading embedded bundle stream : " + name);
                            Initialization.PerformanceLog("Loading embedded bundle stream : " + name);
                            var bundle = AssetBundle.LoadFromStream(assembly.GetManifestResourceStream(name));

                            if (bundle != null)
                            {
                                results.Add(bundle);
                            }

                            Initialization.PerformanceLog("Finished Loading embedded bundle stream : " + name);
                        }
                    }

                    /*foreach (var bundle in loadedBundles)
                    {
                        if (!bundle.isStreamedSceneAssetBundle)
                        {
                            //WeaverLog.Log("Loading bundle for Weaver Mod : " + bundle.name);
                            Initialization.PerformanceLog("Loading bundle for Weaver Mod : " + bundle.name);
                            foreach (var registry in bundle.LoadAllAssets<Registry>())
                            {
                                if (registry.ModType.Assembly.GetName().Name == assemblyName)
                                {
                                    registry.EnableRegistry();
                                }
                            }

                            Initialization.PerformanceLog("Finished Loading bundle for Weaver Mod : " + bundle.name);
                        }
                        else
                        {
                            WeaverLog.Log("Loading scene bundle for Weaver Mod : " + bundle.name);
                        }
                    }*/

                    loadedBundles.Add(assembly, results);

                    return results;
                }
            }
            catch (NotSupportedException error)
            {
                if (!error.Message.Contains("not supported in a dynamic module"))
                {
                    throw;
                }
            }

            return new List<AssetBundle>();
        }

        /// <summary>
        /// Loads any registries that are embedded inside of the assembly as an embedded resource asset bundle
        /// </summary>
        /// <param name="assembly">The assembly to load from</param>
        public static void LoadEmbeddedRegistries(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;

            foreach (var bundle in LoadBundlesOnly(assembly))
            {
                if (!bundle.isStreamedSceneAssetBundle)
                {
                    Initialization.PerformanceLog("Loading bundle for Weaver Mod : " + bundle.name);
                    foreach (var registry in bundle.LoadAllAssets<Registry>())
                    {
                        if (registry.ModType.Assembly.GetName().Name == assemblyName)
                        {
                            registry.EnableRegistry();
                        }
                    }

                    Initialization.PerformanceLog("Finished Loading bundle for Weaver Mod : " + bundle.name);
                }
                else
                {
                    WeaverLog.Log("Loading scene bundle for Weaver Mod : " + bundle.name);
                }
            }


            /*var assemblyName = assembly.GetName().Name;
            //WeaverLog.Log("Loading Embedded Registries for [" + assemblyName + "]");
            Initialization.PerformanceLog($"Loading Embedding Registries for {assemblyName}");
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
            }*/

            /*try
            {
                if (assembly != typeof(WeaverMod).Assembly)
                {
                    List<AssetBundle> loadedBundles = new List<AssetBundle>();
                    foreach (var name in assembly.GetManifestResourceNames())
                    {
                        if (name.EndsWith(extension))
                        {
                            //WeaverLog.Log("Loading embedded bundle stream : " + name);
                            Initialization.PerformanceLog("Loading embedded bundle stream : " + name);
                            var bundle = AssetBundle.LoadFromStream(assembly.GetManifestResourceStream(name));

                            if (bundle != null)
                            {
                                loadedBundles.Add(bundle);
                            }

                            Initialization.PerformanceLog("Finished Loading embedded bundle stream : " + name);
                        }
                    }

                    foreach (var bundle in loadedBundles)
                    {
                        if (!bundle.isStreamedSceneAssetBundle)
                        {
                            //WeaverLog.Log("Loading bundle for Weaver Mod : " + bundle.name);
                            Initialization.PerformanceLog("Loading bundle for Weaver Mod : " + bundle.name);
                            foreach (var registry in bundle.LoadAllAssets<Registry>())
                            {
                                if (registry.ModType.Assembly.GetName().Name == assemblyName)
                                {
                                    registry.EnableRegistry();
                                }
                            }

                            Initialization.PerformanceLog("Finished Loading bundle for Weaver Mod : " + bundle.name);
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
            }*/

            Initialization.PerformanceLog($"Finished Loading Embedding Registries for {assemblyName}");
        }
    }
}
