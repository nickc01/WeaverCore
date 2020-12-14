using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore
{
    [Serializable]
    public struct FeatureSet
    {
        [SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "<Pending>")]
        public UnityEngine.Object feature;
        public string TypeName;
        public string AssemblyName;
    }


    /// <summary>
    /// Used to store a variety of Features to be added to the game <seealso cref="IFeature"/>
    /// </summary>
    [CreateAssetMenu(fileName = "ModRegistry", menuName = "WeaverCore/Registry", order = 1)]
    public class Registry : ScriptableObject
    {
        static Dictionary<string, Assembly> assemblyNames;


        [SerializeField]
        string registryName;


        [SerializeField]
        string modName = "";

        [SerializeField]
        string modAssemblyName = "";

        [SerializeField]
        string modTypeName = "";

        [SerializeField]
        int modListHashCode = 0;

        [SerializeField]
        List<FeatureSet> features;

        [SerializeField]
        List<UnityEngine.Object> featuresRaw;

        [SerializeField]
        int selectedFeatureIndex = 0;

        [SerializeField]
        int selectedModIndex = 0;


        /// <summary>
        /// The name of the mod the registry is bound to <seealso cref="IWeaverMod"/> <seealso cref="WeaverMod"/>
        /// </summary>
        public string ModName
        {
            get
            {
                return modName;
            }
        }

        /// <summary>
        /// The name of the registry
        /// </summary>
        public string RegistryName
        {
            get
            {
                return registryName;
            }
        }

        /// <summary>
        /// The short name of the assembly the mod type resides in
        /// </summary>
        public string ModAssemblyName
        {
            get
            {
                return modAssemblyName;
            }
        }

        [SerializeField]
        bool registryEnabled = true;
        bool initialized
        {
            get
            {
                return AllRegistries.Contains(this);
            }
        }

        //bool initialized = false;

        private Type modType = null;

        //private string typeNameCache = "";

        /// <summary>
        /// Whether the registry is enabled or not. If the mod this registry is bound to is unloaded, the registry will automatically be disabled
        /// </summary>
        public bool RegistryEnabled
        {
            get { return registryEnabled; }
            set
            {
                if (registryEnabled != value)
                {
                    registryEnabled = value;
                    if (registryEnabled)
                    {
                        ActiveRegistries.Add(this);
                    }
                    else
                    {
                        ActiveRegistries.Remove(this);
                    }
                }

            }
        }

        /// <summary>
        /// The <see cref="Type"/> of the mod the registry is bound to <seealso cref="IWeaverMod"/> <seealso cref="WeaverMod"/>
        /// </summary>
        public Type ModType
        {
            get
            {
                if (assemblyNames == null)
                {
                    assemblyNames = new Dictionary<string, Assembly>();
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        assemblyNames.Add(assembly.GetName().Name, assembly);
                    }
                    AppDomain.CurrentDomain.AssemblyLoad += NewAssemblyLoaded;
                }
                if (modType == null || modType.FullName != modTypeName)
                {
                    modType = assemblyNames[modAssemblyName].GetType(modTypeName);
                }
                return modType;
            }
        }

        private static void NewAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            if (!assemblyNames.ContainsKey(args.LoadedAssembly.GetName().Name))
            {
                assemblyNames.Add(args.LoadedAssembly.GetName().Name, args.LoadedAssembly);
            }
        }

        /// <summary>
        /// Initializes the Registry. This is automatically called when the bound mod is loaded
        /// </summary>
        public void Initialize()
        {
            if (!initialized)
            {
                WeaverLog.Log("Loading Registry = " + RegistryName + " for mod = " + ModName);
                //initialized = true;
                AllRegistries.Add(this);
                if (RegistryEnabled)
                {
                    ActiveRegistries.Add(this);
                }
                foreach (var feature in featuresRaw)
                {
                    if (feature is IFeature && feature is IOnFeatureLoad)
                    {
                        try
                        {
                            ((IOnFeatureLoad)feature).OnFeatureLoad(this);
                        }
                        catch (Exception e)
                        {
                            WeaverLog.LogError("Registry Load Error: " + e);
                        }
                    }
                }

            }
        }

        public override bool Equals(object other)
        {
            if (other is Registry)
            {
                var otherReg = (Registry)other;
                return modName == otherReg.modName && modAssemblyName == otherReg.modAssemblyName && modTypeName == otherReg.modTypeName && registryName == otherReg.registryName;
            }
            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            int hash = 0;
            HashUtilities.AdditiveHash(ref hash, modName);
            HashUtilities.AdditiveHash(ref hash, modAssemblyName);
            HashUtilities.AdditiveHash(ref hash, modTypeName);
            HashUtilities.AdditiveHash(ref hash, registryName);
            return hash;
        }

        public override string ToString()
        {
            return registryName;
        }


        static HashSet<Registry> ActiveRegistries = new HashSet<Registry>();
        static HashSet<Registry> AllRegistries = new HashSet<Registry>();


        /// <summary>
        /// Goes through all of the loaded Registries, and find the specified features
        /// </summary>
        /// <typeparam name="T">The type of features to look for. Using <see cref="IFeature"/> retrieves all features</typeparam>
        /// <returns>Returns an Itereator with all the features in it</returns>
        public static IEnumerable<T> GetAllFeatures<T>() where T : class
        {
            return GetAllFeatures<T>(f => true);
        }

        /// <summary>
        /// Goes through all of the loaded Registries, and find the specified features
        /// </summary>
        /// <typeparam name="T">The type of features to look for. Using <see cref="IFeature"/> retrieves all features</typeparam>
        /// <param name="predicate">A predicate function used to narrow down the feature search even further</param>
        /// <returns>Returns an Itereator with all the features in it</returns>
        public static IEnumerable<T> GetAllFeatures<T>(Func<T, bool> predicate) where T : class
        {
            foreach (var registry in ActiveRegistries)
            {
                WeaverLog.Log("Active Registry = " + registry.registryName);
                if (registry != null && registry.registryEnabled)
                {
                    //Debugger.Log("B");
                    foreach (var result in registry.GetFeatures(predicate))
                    {
                        yield return result;
                    }
                }
            }
        }

        /// <summary>
        /// Searches the registry and finds the specifed features
        /// </summary>
        /// <typeparam name="T">The type of features to look for. Using <see cref="IFeature"/> retrieves all features</typeparam>
        /// <returns>Returns an Itereator with all the features in it</returns>
        public IEnumerable<T> GetFeatures<T>() where T : class
        {
            return GetFeatures<T>(f => true);
        }

        /// <summary>
        /// Searches the registry and finds the specifed features
        /// </summary>
        /// <typeparam name="T">The type of features to look for. Using <see cref="IFeature"/> retrieves all features</typeparam>
        /// <param name="predicate">A predicate function used to narrow down the feature search even further</param>
        /// <returns>Returns an Itereator with all the features in it</returns>
        public IEnumerable<T> GetFeatures<T>(Func<T, bool> predicate) where T : class
        {
            // Debugger.Log("Features Raw = " + featuresRaw);
            foreach (var rawFeature in featuresRaw)
            {
                //WeaverLog.Log("Raw Feature Type = " + rawFeature.GetType());
                //WeaverLog.Log("Raw Feature = " + rawFeature);
                if (rawFeature != null && rawFeature is IFeature)
                {
                    var feature = (IFeature)rawFeature;
                   // WeaverLog.Log("Feature 2 = " + feature);
                    //WeaverLog.Log("Destination Type = " + typeof(T).FullName);
                    //WeaverLog.Log("Can Convert to type = " + typeof(T).IsAssignableFrom(feature.GetType()));
                    if (typeof(T).IsAssignableFrom(feature.GetType()) && predicate(feature as T))
                    {
                        //WeaverLog.Log("Yielding Feature");
                        yield return feature as T;
                    }
                }
            }
        }

        /// <summary>
        /// Adds a new feature to the registry
        /// </summary>
        /// <typeparam name="T">The type of feature to add</typeparam>
        /// <param name="feature">The feature to be added</param>
        public void AddFeature<T>(T feature) where T : IFeature
        {
            if (!(feature is UnityEngine.Object))
            {
                return;
            }
            features.Add(new FeatureSet()
            {
                feature = feature as UnityEngine.Object,
                AssemblyName = typeof(T).Assembly.FullName,
                TypeName = typeof(T).FullName
            });
            featuresRaw.Add(feature as UnityEngine.Object);
        }

        /// <summary>
        /// Removes a feature from the registry
        /// </summary>
        /// <typeparam name="T">THe type of feature to add</typeparam>
        /// <param name="feature">The feature to be added</param>
        /// <returns>Returns whether the feature has been removed or not</returns>
        public bool Remove<T>(T feature) where T : IFeature
        {
            if (!(feature is UnityEngine.Object))
            {
                return false;
            }

            var realFeature = feature as UnityEngine.Object;

            for (int i = features.Count - 1; i >= 0; i--)
            {
                if (features[i].feature == realFeature)
                {
                    features.RemoveAt(i);
                    featuresRaw.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes a feature from the registry
        /// </summary>
        /// <param name="predicate">A predicate used to determine which feature to remove. Only 1 feature gets removed</param>
        /// <returns>Returns whether the feature has been removed or not</returns>
        public bool Remove(Func<IFeature, bool> predicate)
        {
            for (int i = features.Count - 1; i >= 0; i--)
            {
                if (predicate(features[i].feature as IFeature))
                {
                    features.RemoveAt(i);
                    featuresRaw.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Find a loaded registry pertaining to a mod
        /// </summary>
        /// <param name="ModType">The mod that is associated with the registry</param>
        /// <returns>Returns the registry that is bound to the mod. Returns null if no registry is found</returns>
        public static Registry FindModRegistry(Type ModType)
        {
            foreach (var registry in AllRegistries)
            {
                if (registry.ModType == ModType)
                {
                    return registry;
                }
            }
            return null;
        }

        /// <summary>
        /// Finds all loaded registries pertaining to a mod
        /// </summary>
        /// <param name="ModType">THe mod that is associated with the registry</param>
        /// <returns>Returns all the registries that are bound to the mod</returns>
        public static IEnumerable<Registry> FindModRegistries(Type ModType)
        {
            foreach (var registry in AllRegistries)
            {
                if (registry.ModType == ModType)
                {
                    yield return registry;
                }
            }
        }

        public static IEnumerable<Registry> FindModRegistries<Mod>() where Mod : WeaverMod
        {
            return FindModRegistries(typeof(Mod));
        }
    }
}