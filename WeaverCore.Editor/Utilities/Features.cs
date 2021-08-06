/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using WeaverCore.Attributes;
using WeaverCore.Interfaces;

namespace WeaverCore.Editor.Utilities
{
	public static class Features
	{
		public static List<Type> GetFeatures()
		{
            List<Type> features = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (typeof(IFeature).IsAssignableFrom(type) && type.IsDefined(typeof(ShowFeatureAttribute), false))
                    {
                        features.Add(type);
                    }
                }
            }
            features.Sort(new TypeComparer());
            return features;
        }

        public static string[] GetFeatureNames(List<Type> Features)
        {
            var FeatureNames = new string[Features.Count];
            for (int i = 0; i < Features.Count; i++)
            {
                FeatureNames[i] = ObjectNames.NicifyVariableName(Features[i].Name);
            }
            return FeatureNames;
        }

        public static Type FindFeatureType(string assemblyName, string typeName, List<Type> Features)
        {
            foreach (var feature in Features)
            {
                if (feature.FullName == typeName && (assemblyName == "" || feature.Assembly.GetName().Name == assemblyName))
                {
                    return feature;
                }
            }
            return null;
        }

        public static Type FindFeatureType(FeatureSet featureSet, List<Type> Features)
        {
            foreach (var feature in Features)
            {
                if (feature.FullName == featureSet.TypeName && (featureSet.AssemblyName == "" || feature.Assembly.GetName().Name == featureSet.AssemblyName))
                {
                    return feature;
                }
            }
            if (featureSet.feature != null)
            {
                return featureSet.feature.GetType();
            }
            return null;
        }

        public static bool FindFeatureType(string assemblyName, string typeName, List<Type> Features, out Type result)
        {
            result = FindFeatureType(assemblyName, typeName, Features);
            return result != null;
        }

        public static bool FindFeatureType(FeatureSet featureSet, List<Type> Features, out Type result)
        {
            result = FindFeatureType(featureSet, Features);
            return result != null;
        }
    }
}
*/