using System;
using System.Collections.Generic;
using System.Linq;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Attributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WeaverCore.Utilities
{
    /// <summary>
    /// Contains utility functions related to environment types.
    /// </summary>
    public static class EnvironmentTypeUtilities
    {
        static bool cacheDirty = true;
        static List<CustomEnvironmentType> cachedCustomEnvironmentTypes;
        static int[] cachedEnvironmentTypeIndexes;
        static string[] cachedEnvironmentTypeNames;

        static IEnumerable<CustomEnvironmentType> GetCustomEnvironmentTypesInternal()
        {
            IEnumerable<Registry> registries;

#if UNITY_EDITOR
            registries = AssetDatabase.FindAssets("t:Registry").Select(guid => AssetDatabase.LoadAssetAtPath<Registry>(AssetDatabase.GUIDToAssetPath(guid)));
#else
            registries = Registry.AllRegistries;
#endif
            return registries.Where(r => r != null).SelectMany(r => r.GetFeatures<CustomEnvironmentType>()).Where(t => t != null);
        }

        /// <summary>
        /// Gets a list of all custom environment types.
        /// </summary>
        public static List<CustomEnvironmentType> GetCustomEnvironmentTypes()
        {
            if (cacheDirty || cachedCustomEnvironmentTypes == null)
            {
                cachedCustomEnvironmentTypes = new List<CustomEnvironmentType>(GetCustomEnvironmentTypesInternal());
                cachedEnvironmentTypeIndexes = null;
                cachedEnvironmentTypeNames = null;
                cacheDirty = false;
            }

            return cachedCustomEnvironmentTypes;
        }

        /// <summary>
        /// Attempts to get a custom environment type by its ID.
        /// </summary>
        public static bool TryGetCustomEnvironmentType(int environmentTypeID, out CustomEnvironmentType customEnvironmentType)
        {
            customEnvironmentType = GetCustomEnvironmentTypes().LastOrDefault(t => t != null && t.EnvironmentID == environmentTypeID);
            return customEnvironmentType != null;
        }

        static string GetBuiltInName(EnvironmentType environmentType)
        {
            switch (environmentType)
            {
                case EnvironmentType.NoEffect:
                    return "No Effect";
                default:
                    return environmentType.ToString();
            }
        }

        static void BuildEnvironmentTypeLists(out int[] environmentTypeIndexes, out string[] environmentTypeNames)
        {
            if (cachedEnvironmentTypeIndexes != null && cachedEnvironmentTypeNames != null)
            {
                environmentTypeIndexes = cachedEnvironmentTypeIndexes;
                environmentTypeNames = cachedEnvironmentTypeNames;
                return;
            }

            var environmentIndexList = new List<int>();
            var environmentNameList = new List<string>();

            foreach (var value in Enum.GetValues(typeof(EnvironmentType)))
            {
                var environmentType = (EnvironmentType)value;

                environmentIndexList.Add((int)environmentType);
                environmentNameList.Add(GetBuiltInName(environmentType));
            }

            foreach (var customType in GetCustomEnvironmentTypes())
            {
                var existingIndex = environmentIndexList.IndexOf(customType.EnvironmentID);

                if (existingIndex >= 0)
                {
                    environmentIndexList[existingIndex] = customType.EnvironmentID;
                    environmentNameList[existingIndex] = customType.GetDisplayName();
                }
                else
                {
                    environmentIndexList.Add(customType.EnvironmentID);
                    environmentNameList.Add(customType.GetDisplayName());
                }
            }

            cachedEnvironmentTypeIndexes = environmentIndexList.ToArray();
            cachedEnvironmentTypeNames = environmentNameList.ToArray();

            environmentTypeIndexes = cachedEnvironmentTypeIndexes;
            environmentTypeNames = cachedEnvironmentTypeNames;
        }

        /// <summary>
        /// Gets all built-in and custom environment types.
        /// </summary>
        public static void GetAllEnvironmentTypes(out int[] environmentTypeIndexes, out string[] environmentTypeNames)
        {
            BuildEnvironmentTypeLists(out environmentTypeIndexes, out environmentTypeNames);
        }

        [OnRegistryLoad]
        static void OnRegistryLoad(Registry registry)
        {
            cacheDirty = true;
        }

        [OnRegistryUnload]
        static void OnRegistryUnload(Registry registry)
        {
            cacheDirty = true;
        }
    }
}
