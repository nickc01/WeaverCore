//#undef UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
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

    public struct FeatureType<T> where T : class
    {
        public readonly Type Type;
        public readonly Registry Registry;
#if UNITY_EDITOR
        T Instance;

        internal FeatureType(T instance, Registry registry)
        {
            Instance = instance;
            Registry = registry;
            Type = instance.GetType();
        }

        public T Load()
        {
            if (Instance is IOnFeatureLoad)
            {
                var onFeatureLoad = (IOnFeatureLoad)Instance;
                onFeatureLoad.OnFeatureLoad(Registry);
            }
            return Instance;
        }
#else
        string AssetName;

        internal FeatureType(Registry registry, Type type, string assetName)
        {
            Type = type;
            AssetName = assetName;
            Registry = registry;

            //Instance = instance;
            //Registry = registry;
            //Type = instance.GetType();
        }

        public T Load()
        {
            WeaverLog.Log("AssetName = " + AssetName + ", Type = " + Type.FullName + ", Registry = " + Registry.RegistryName + ", conversion to = " + typeof(T).FullName);
            WeaverLog.Log("Bundle = " + Registry.RegistryBundle);
            var asset = Registry.RegistryBundle.LoadAsset<UnityEngine.Object>(AssetName);

            WeaverLog.Log("Asset = " + asset);

            if (asset == null)
            {
                return null;
            }

            T instance = asset as T;

            if (instance == null && asset is GameObject)
            {
                instance = (asset as GameObject).GetComponent<T>();
            }
            WeaverLog.Log("Instance A = " + (asset == null ? "null" : asset.GetType().FullName));
            WeaverLog.Log("Instance = " + instance);
            WeaverLog.Log("Instance B = " + Registry.RegistryBundle.LoadAsset(AssetName.ToLower(),Type));
            //var instance = Registry.RegistryBundle.LoadAsset(AssetName.ToLower(), Type) as T;
            if (instance is IOnFeatureLoad)
            {
                var onFeatureLoad = (IOnFeatureLoad)instance;
                onFeatureLoad.OnFeatureLoad(Registry);
            }
            return instance;
        }
#endif
    }


    /// <summary>
    /// Used to store a variety of Features to be added to the game <seealso cref="IFeature"/>
    /// </summary>
    [CreateAssetMenu(fileName = "ModRegistry", menuName = "WeaverCore/Registry", order = 1)]
    public class Registry : ScriptableObject
    {
        //static Dictionary<string, Assembly> assemblyNames;


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

#if !GAME_BUILD
        [SerializeField]
        List<FeatureSet> features;
#endif

        //[SerializeField]
        //List<UnityEngine.Object> featuresRaw;

        [SerializeField]
        List<string> featureBundlePaths;

        [SerializeField]
        List<string> featureTypesRaw;
#if !UNITY_EDITOR
        List<Type> FeatureTypes;
#endif

        [SerializeField]
        int selectedFeatureIndex = 0;

        [SerializeField]
        int selectedModIndex = 0;

        /// <summary>
        /// The assetbundle that this registry is loaded from. This is set to null if loaded in the Editor
        /// </summary>
        public AssetBundle RegistryBundle { get; internal set; }


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
                if (modType == null || modType.FullName != modTypeName)
                {
                    var assembly = Assembly.Load(modAssemblyName);
                    if (assembly != null)
                    {
                        modType = assembly.GetType(modTypeName);
                    }
                }
                return modType;
            }
        }

        /*private static void NewAssemblyLoaded(object sender, AssemblyLoadEventArgs args)
        {
            if (!assemblyNames.ContainsKey(args.LoadedAssembly.GetName().Name))
            {
                assemblyNames.Add(args.LoadedAssembly.GetName().Name, args.LoadedAssembly);
            }
        }*/

        /// <summary>
        /// Initializes the Registry. This is automatically called when the registry is loaded
        /// </summary>
        public void Initialize(AssetBundle bundle)
        {
            if (!initialized)
            {
                RegistryBundle = bundle;

                WeaverLog.Log("Loading Registry = " + RegistryName + " for mod = " + ModName);
                //initialized = true;
                AllRegistries.Add(this);
                if (RegistryEnabled)
                {
                    ActiveRegistries.Add(this);
                }
                //WeaverLog.Log("-----Registry = " + RegistryName);
                /*for (int i = 0; i < featureTypesRaw.Count; i++)
                {
                    //Debug.Log("Registry = " + registryName + " , Type = " + featureTypesRaw[i]);
                    WeaverLog.Log("Feature = " + featureTypesRaw[i]);
                    WeaverLog.Log("Bundle Path = " + featureBundlePaths[i]);
                }

                if (RegistryBundle != null)
                {
                    WeaverLog.Log("RegistryBundle = " + RegistryBundle);

                    foreach (var name in RegistryBundle.GetAllAssetNames())
                    {
                        WeaverLog.Log("Asset Name = " + name);
                    }
                }*/




#if !UNITY_EDITOR
                FeatureTypes = new List<Type>();
                foreach (var modTypePath in featureTypesRaw)
                {
                    var split = modTypePath.Split(':');
                    var assemblyName = split[0];
                    var typeName = split[1];
                    var assembly = Assembly.Load(assemblyName);
                    if (assembly != null)
                    {
                        var type = assembly.GetType(typeName);
                        if (type != null)
                        {
                            FeatureTypes.Add(type);
                            continue;
                        }
                    }
                    FeatureTypes.Add(null);
                }
#endif
                /*foreach (var set in features)
                {
                    if (set.feature is IFeature && ((IFeature)set.feature).FeatureEnabled && set.feature is IOnFeatureLoad)
                    {
                        try
                        {
                            ((IOnFeatureLoad)set.feature).OnRegistryLoad(this);
                        }
                        catch (Exception e)
                        {
                            WeaverLog.LogError("Registry Load Error: " + e);
                        }
                    }
                }
                foreach (var feature in featuresRaw)
                {
                    if (feature is IFeature && ((IFeature)feature).FeatureEnabled && feature is IOnFeatureLoad)
                    {
                        try
                        {
                            ((IOnFeatureLoad)feature).OnRegistryLoad(this);
                        }
                        catch (Exception e)
                        {
                            WeaverLog.LogError("Registry Load Error: " + e);
                        }
                    }
                }*/

            }
        }

        /*public override bool Equals(object other)
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
        }*/


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

        public static IEnumerable<FeatureType<T>> GetAllFeatureTypes<T>() where T : class
        {
            return GetAllFeatureTypes<T>(f => true);
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

        public static IEnumerable<FeatureType<T>> GetAllFeatureTypes<T>(Func<FeatureType<T>, bool> predicate) where T : class
        {
            foreach (var registry in ActiveRegistries)
            {
                if (registry != null && registry.registryEnabled)
                {
                    //Debugger.Log("B");
                    foreach (var result in registry.GetFeatureTypes(predicate))
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

        public IEnumerable<FeatureType<T>> GetFeatureTypes<T>() where T : class
        {
            return GetFeatureTypes<T>(f => true);
        }

        /// <summary>
        /// Searches the registry and finds the specifed features
        /// </summary>
        /// <typeparam name="T">The type of features to look for. Using <see cref="IFeature"/> retrieves all features</typeparam>
        /// <param name="predicate">A predicate function used to narrow down the feature search even further</param>
        /// <returns>Returns an Itereator with all the features in it</returns>
        public IEnumerable<T> GetFeatures<T>(Func<T, bool> predicate) where T : class
        {
            foreach (var types in GetFeatureTypes<T>())
            {
                var instance = types.Load();
                if (predicate(instance))
                {
                    yield return instance;
                }
            }
        }

        public IEnumerable<FeatureType<T>> GetFeatureTypes<T>(Func<FeatureType<T>, bool> predicate) where T : class
        {
#if UNITY_EDITOR
            foreach (var set in features)
            {
                if (set.feature != null && set.feature is IFeature)
                {
                    var feature = (IFeature)set.feature;

                    if (typeof(T).IsAssignableFrom(feature.GetType()))
                    {
                        var featureType = new FeatureType<T>(feature as T, this);
                        if (predicate(featureType))
                        {
                            yield return featureType;
                        }
                        /*if (feature is IOnFeatureLoad)
                        {
                            var onFeatureLoad = (IOnFeatureLoad)feature;
                            onFeatureLoad.OnFeatureLoad(this);
                        }*/
                    }
                }
            }
#else
            for (int i = 0; i < featureBundlePaths.Count; i++)
            {
                var type = FeatureTypes[i];
                if (type != null && typeof(T).IsAssignableFrom(type))
                {
                    var assetName = featureBundlePaths[i];

                    var featureType = new FeatureType<T>(this,type, assetName);

                    //var instance = RegistryBundle.LoadAsset(assetName, type) as T;
                    if (predicate(featureType))
                    {
                        yield return featureType;
                    }
                }
            }

#endif
        }

        /*/// <summary>
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
        }*/

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
