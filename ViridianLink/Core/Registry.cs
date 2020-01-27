using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ViridianLink.Core
{
    [Serializable]
    public struct FeatureSet
    {
        public Feature feature;
        public string FullTypeName;
        public string FullAssemblyName;
    }


    [CreateAssetMenu(fileName = "ModRegistry", menuName = "ViridianLink/Registry", order = 1)]
    public class Registry : ScriptableObject
    {
        [SerializeField]
        string mod;

        [SerializeField]
        string modAssemblyName = "";

        [SerializeField]
        string modTypeName = "";

        [SerializeField]
        List<FeatureSet> features;

        [SerializeField]
        public int selectedFeatureIndex = 0;

        public string ModName => mod;


        [SerializeField]
        bool registryEnabled = true;

        public bool RegistryEnabled
        {
            get => registryEnabled;
            set => registryEnabled = value;
        }


        public Type ModType
        {
            get
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.FullName == modAssemblyName)
                    {
                        return assembly.GetType(modTypeName);
                    }
                }
                return null;
            }
        }


        static List<Registry> AllRegistries = new List<Registry>();

        public static IEnumerable<T> GetAllFeatures<T>() where T : Feature
        {
            return GetAllFeatures<T>(f => true);
        }

        public static IEnumerable<T> GetAllFeatures<T>(Func<T, bool> predicate) where T : Feature
        {
            foreach (var registry in AllRegistries)
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

        public IEnumerable<T> GetFeatures<T>() where T : Feature
        {
            return GetFeatures<T>(f => true);
        }

        public IEnumerable<T> GetFeatures<T>(Func<T,bool> predicate) where T : Feature
        {
            foreach (var feature in features)
            {
                if (feature.feature.FeatureEnabled && typeof(T).IsAssignableFrom(feature.feature.GetType()) && predicate((T)feature.feature))
                {
                    yield return (T)feature.feature;
                }
            }
        }

        public void AddFeature<T>(T feature) where T : Feature
        {
            features.Add(new FeatureSet()
            {
                feature = feature,
                FullAssemblyName = typeof(T).Assembly.FullName,
                FullTypeName = typeof(T).FullName
            });
        }

        public bool Remove<T>(T feature) where T : Feature
        {
            for (int i = features.Count - 1; i >= 0; i--)
            {
                if (features[i].feature == feature)
                {
                    features.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public bool Remove(Func<Feature,bool> predicate)
        {
            for (int i = features.Count - 1; i >= 0; i--)
            {
                if (predicate(features[i].feature))
                {
                    features.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

    }
}
