using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore
{
	/// <summary>
	/// Used to store a variety of features to be added to the game
	/// </summary>
	[CreateAssetMenu(fileName = "Registry", menuName = "WeaverCore/Registry", order = 1)]
    public class Registry : ScriptableObject
	{
        static List<Registry> allRegistries = new List<Registry>();

        public static IEnumerable<Registry> AllRegistries => allRegistries;

        [NonSerialized]
        bool initialized = false;

        [SerializeField]
        string modTypeName = "";

        [SerializeField]
        string modAssemblyName = "";

        [SerializeField]
        List<UnityEngine.Object> features;

        [SerializeField]
        List<string> featureTypeNames;

        [SerializeField]
        List<string> featureAssemblyNames;

        [NonSerialized]
        Type _modeTypeCached = null;
        public Type ModType
		{
			get
			{
				if (_modeTypeCached == null || (_modeTypeCached != null && _modeTypeCached.FullName != modTypeName))
				{
                    _modeTypeCached = TypeUtilities.NameToType(modTypeName, modAssemblyName);
                }
                return _modeTypeCached;
			}
		}
        public string ModName => ModType.Name;
        public string ModAssemblyName => modAssemblyName;
        public bool Enabled => initialized && ModType != null;
        public IEnumerable<UnityEngine.Object> Features => features;
        public int FeatureCount => features.Count;


        public static Registry Create<TMod>() where TMod : WeaverMod
		{
            return Create(typeof(TMod));
		}

        public static Registry Create(WeaverMod mod)
		{
            return Create(mod.GetType());
		}

        public static Registry Create(Type modType)
		{
            var newRegistry = ScriptableObject.CreateInstance<Registry>();
            newRegistry.modAssemblyName = modType.Assembly.GetName().Name;
            newRegistry.modTypeName = modType.FullName;
            newRegistry._modeTypeCached = modType;
            newRegistry.EnableRegistry();
            return newRegistry;
		}

        public void EnableRegistry()
		{
            if (initialized)
            {
                return;
            }
            initialized = true;
            allRegistries.Add(this);
            ExecuteAttribute<OnRegistryLoadAttribute>(this);

            foreach (var feature in features)
            {
                ExecuteAttribute<OnFeatureLoadAttribute>(feature);
            }
        }

        public void DisableRegistry()
		{
            if (!initialized)
            {
                return;
            }
            try
            {
                foreach (var feature in features)
                {
                    ExecuteAttribute<OnFeatureUnloadAttribute>(feature);
                }
                ExecuteAttribute<OnRegistryUnloadAttribute>(this);
                allRegistries.Remove(this);
            }
            finally
			{
                initialized = false;
            }
        }

        static void ExecuteAttribute<AttrType>(object parameter) where AttrType : PriorityAttribute
        {
            var methods = ReflectionUtilities.GetMethodsWithAttribute<AttrType>().ToList();

            methods.Sort(new PriorityAttribute.PairSorter<AttrType>());

            var param = new object[] { parameter };

            foreach (var method in methods)
            {
                var parameters = method.Item1.GetParameters();

                try
                {
                    var paramCount = parameters.GetLength(0);
                    if (paramCount == 0)
                    {
                        method.Item1.Invoke(null,null);
                    }
					else if (paramCount == 1)
					{
                        var paramType = parameters[0].ParameterType;
						if (paramType.IsAssignableFrom(parameter.GetType()))
						{
                            method.Item1.Invoke(null, param);
                        }
                    }
                }
                catch (Exception e)
                {
                    WeaverLog.LogError("Error Running Function: " + method.Item1.DeclaringType.FullName + ":" + method.Item1.Name);
                    WeaverLog.LogException(e);
                }
            }
        }

        /// <summary>
        /// Goes through all of the loaded Registries, and find the specified features
        /// </summary>
        /// <typeparam name="T">The type of features to look for</typeparam>
        /// <returns>Returns an iterator with all the features in it</returns>
        public static IEnumerable<T> GetAllFeatures<T>() where T : class
        {
            return GetAllFeatures<T>(f => true);
        }

        /// <summary>
        /// Goes through all of the loaded Registries, and find the specified features
        /// </summary>
        /// <typeparam name="T">The type of features to look for</typeparam>
        /// <param name="predicate">A predicate used to only return the features that satisfy the predicate condition</param>
        /// <param name="onlyEnabledRegistries">If set to true, then only registries that are enabled will be searched through. If false, then all registries will be searched through</param>
        /// <returns>Returns an iterator with all the features in it</returns>
        public static IEnumerable<T> GetAllFeatures<T>(Func<T, bool> predicate, bool onlyEnabledRegistries = true) where T : class
        {
            if (predicate == null)
            {
                predicate = f => true;
            }
            foreach (var registry in AllRegistries)
            {
                if (registry != null && (!onlyEnabledRegistries || (onlyEnabledRegistries && registry.Enabled)))
                {
                    foreach (var result in registry.GetFeatures(predicate))
                    {
                        yield return result;
                    }
                }
            }
        }

        /// <summary>
        /// Searches the registry and finds the specified features
        /// </summary>
        /// <typeparam name="T">The type of features to look for</typeparam>
        /// <returns>Returns an iterator with all the features in it</returns>
        public IEnumerable<T> GetFeatures<T>() where T : class
        {
            return GetFeatures<T>(f => true);
        }

        /// <summary>
        /// Searches the registry and finds the specifed features
        /// </summary>
        /// <typeparam name="T">The type of features to look for</typeparam>
        /// <param name="predicate">A predicate used to only return the features that satisfy the predicate condition</param>
        /// <returns>Returns an Itereator with all the features in it</returns>
        public IEnumerable<T> GetFeatures<T>(Func<T, bool> predicate) where T : class
        {
            foreach (var feature in features)
            {
				if (feature != null)
				{
                    if (typeof(T).IsAssignableFrom(feature.GetType()) && predicate(feature as T))
                    {
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
        public void AddFeature<T>(T feature) where T : UnityEngine.Object
        {
            features.Add(feature);
            ExecuteAttribute<OnFeatureLoadAttribute>(feature);
        }


		/// <summary>
		/// Removes a feature from the registry
		/// </summary>
		/// <typeparam name="T">The type of feature to remove</typeparam>
		/// <param name="feature">The feature to be removed</param>
		/// <returns>Returns whether the feature has been removed or not</returns>
		public bool Remove<T>(T feature) where T : UnityEngine.Object
		{
            ExecuteAttribute<OnFeatureUnloadAttribute>(feature);
            return features.Remove(feature);
        }

		/// <summary>
		/// Removes some features from the registry
		/// </summary>
		/// <param name="predicate">A predicate used to determine which feature to remove</param>
		/// <returns>Returns how many features where removed</returns>
		public int Remove(Predicate<UnityEngine.Object> predicate)
		{
            int removedAmount = 0;
			for (int i = features.Count - 1; i >= 0; i--)
			{
				if (predicate(features[i]))
				{
                    removedAmount++;
                    ExecuteAttribute<OnFeatureUnloadAttribute>(features[i]);
                    features.RemoveAt(i);
				}
			}
            return removedAmount;
        }

		/// <summary>
		/// Removes all features of the specified type
		/// </summary>
		/// <typeparam name="T">The type of features to remove</typeparam>
		/// <returns>Returns how many features where removed</returns>
		public int RemoveAllFeatures<T>()
		{
            int removedAmount = 0;
            for (int i = features.Count - 1; i >= 0; i--)
            {
                if (features[i] is T)
                {
                    removedAmount++;
                    ExecuteAttribute<OnFeatureUnloadAttribute>(features[i]);
                    features.RemoveAt(i);
                }
            }
            return removedAmount;
        }

        /// <summary>
        /// Removes all features from the registry
        /// </summary>
		public void RemoveAllFeatures()
        {
			for (int i = features.Count - 1; i >= 0; i--)
			{
                ExecuteAttribute<OnFeatureUnloadAttribute>(features[i]);
            }
            features.Clear();
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