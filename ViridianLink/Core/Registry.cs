using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;
using ViridianLink.Extras;

namespace ViridianLink.Core
{
    [Serializable]
    struct FeatureSet
    {
        [SuppressMessage("Usage", "CA2235:Mark all non-serializable fields", Justification = "<Pending>")]
        public Feature feature;
        public string FullTypeName;
        public string FullAssemblyName;
    }


    /// <summary>
    /// Used to store a variety of Features to be added to the game <seealso cref="Feature"/>
    /// </summary>
    [CreateAssetMenu(fileName = "ModRegistry", menuName = "ViridianLink/Registry", order = 1)]
    public class Registry : ScriptableObject
    {
        [SerializeField]
        string mod = "";

        [SerializeField]
        string modAssemblyName = "";

        [SerializeField]
        string modTypeName = "";

        [SerializeField]
        List<FeatureSet> features;

        [SerializeField]
        List<Feature> featuresRaw;

        [SerializeField]
        
        int selectedFeatureIndex = 0;


        /// <summary>
        /// The name of the mod the registry is bound to <seealso cref="IViridianMod"/> <seealso cref="ViridianMod"/>
        /// </summary>
        public string ModName => mod;

        [SerializeField]
        bool registryEnabled = true;

        bool started = false;

        private Type modType = null;

        private string typeNameCache = "";

        /// <summary>
        /// Whether the registry is enabled or not. If the mod this registry is bound to is unloaded, the registry will automatically be disabled
        /// </summary>
        public bool RegistryEnabled
        {
            get => registryEnabled;
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
        /// The <see cref="Type"/> of the mod the registry is bound to <seealso cref="IViridianMod"/> <seealso cref="ViridianMod"/>
        /// </summary>
        public Type ModType
        {
            get
            {
                if (modType == null || typeNameCache != modTypeName)
                {
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var type = assembly.GetType(modTypeName);
                        if (type != null)
                        {
                            modType = type;
                            typeNameCache = modTypeName;
                            break;
                        }
                    }
                }
                return modType;
            }
        }

        /// <summary>
        /// Initializes the Registry. This is automatically called when the bound mod is loaded
        /// </summary>
        public void Start()
        {
            if (!started)
            {
                started = true;
                AllRegistries.Add(this);
                if (RegistryEnabled)
                {
                    ActiveRegistries.Add(this);
                }
            }
        }


        static HashSet<Registry> ActiveRegistries = new HashSet<Registry>();
        static HashSet<Registry> AllRegistries = new HashSet<Registry>();


        /// <summary>
        /// Goes through all of the loaded Registries, and find the specified features
        /// </summary>
        /// <typeparam name="T">The type of features to look for. Using <see cref="Feature"/> retrieves all features</typeparam>
        /// <returns>Returns an Itereator with all the features in it</returns>
        public static IEnumerable<T> GetAllFeatures<T>() where T : Feature
        {
            return GetAllFeatures<T>(f => true);
        }

        /// <summary>
        /// Goes through all of the loaded Registries, and find the specified features
        /// </summary>
        /// <typeparam name="T">The type of features to look for. Using <see cref="Feature"/> retrieves all features</typeparam>
        /// <param name="predicate">A predicate function used to narrow down the feature search even further</param>
        /// <returns>Returns an Itereator with all the features in it</returns>
        public static IEnumerable<T> GetAllFeatures<T>(Func<T, bool> predicate) where T : Feature
        {
            foreach (var registry in ActiveRegistries)
            {
                if (registry.registryEnabled)
                {
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
        /// <typeparam name="T">The type of features to look for. Using <see cref="Feature"/> retrieves all features</typeparam>
        /// <returns>Returns an Itereator with all the features in it</returns>
        public IEnumerable<T> GetFeatures<T>() where T : Feature
        {
            return GetFeatures<T>(f => true);
        }

        /// <summary>
        /// Searches the registry and finds the specifed features
        /// </summary>
        /// <typeparam name="T">The type of features to look for. Using <see cref="Feature"/> retrieves all features</typeparam>
        /// <param name="predicate">A predicate function used to narrow down the feature search even further</param>
        /// <returns>Returns an Itereator with all the features in it</returns>
        public IEnumerable<T> GetFeatures<T>(Func<T,bool> predicate) where T : Feature
        {
            foreach (var feature in featuresRaw)
            {
                if (feature.FeatureEnabled && typeof(T).IsAssignableFrom(feature.GetType()) && predicate((T)feature))
                {
                    yield return (T)feature;
                }
            }
        }

        //Returns a list of all the features
        /*public IEnumerable<Feature> AllFeatures
        {
            get
            {
                Debugger.Log("Features = " + featuresRaw);
                foreach (var feature in featuresRaw)
                {
                    Debugger.Log("Feature = " + feature);
                    yield return feature;
                }
            }
        }*/

        /// <summary>
        /// Adds a new feature to the registry
        /// </summary>
        /// <typeparam name="T">The type of feature to add</typeparam>
        /// <param name="feature">The feature to be added</param>
        public void AddFeature<T>(T feature) where T : Feature
        {
            features.Add(new FeatureSet()
            {
                feature = feature,
                FullAssemblyName = typeof(T).Assembly.FullName,
                FullTypeName = typeof(T).FullName
            });
            featuresRaw.Add(feature);
        }

        /// <summary>
        /// Removes a feature from the registry
        /// </summary>
        /// <typeparam name="T">THe type of feature to add</typeparam>
        /// <param name="feature">The feature to be added</param>
        /// <returns>Returns whether the feature has been removed or not</returns>
        public bool Remove<T>(T feature) where T : Feature
        {
            for (int i = features.Count - 1; i >= 0; i--)
            {
                if (features[i].feature == feature)
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
        public bool Remove(Func<Feature,bool> predicate)
        {
            for (int i = features.Count - 1; i >= 0; i--)
            {
                if (predicate(features[i].feature))
                {
                    features.RemoveAt(i);
                    featuresRaw.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Finds an already loaded Registry
        /// </summary>
        /// <param name="ModType">The mod that is associated with the registry</param>
        /// <returns>Returns the registry that is bound to the mod. Returns null if no registry is found</returns>
        public static Registry Find(Type ModType)
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
    }
}
