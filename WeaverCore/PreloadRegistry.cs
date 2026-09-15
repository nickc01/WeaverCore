using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Internal;

namespace WeaverCore
{
    /// <summary>
    /// Registry feature for declaring and retrieving scene preloaded objects.
    /// </summary>
    [CreateAssetMenu(fileName = "PreloadRegistry", menuName = "WeaverCore/Preload Registry")]
    public class PreloadRegistry : ScriptableObject
    {
        public class PreloadEntryListView : IList<PreloadEntry>
        {
            readonly PreloadRegistry source;

            public PreloadEntryListView(PreloadRegistry source) => this.source = source;

            public PreloadEntry this[int index]
            {
                get => new PreloadEntry(source.sceneNames[index], source.objectPaths[index]);
                set
                {
                    source.sceneNames[index] = value.SceneName;
                    source.objectPaths[index] = value.ObjectPath;
                }
            }

            public int Count => Math.Min(source.sceneNames.Count, source.objectPaths.Count);
            public bool IsReadOnly => false;

            public void Add(PreloadEntry item)
            {
                source.sceneNames.Add(item.SceneName);
                source.objectPaths.Add(item.ObjectPath);
            }

            public void Clear()
            {
                source.sceneNames.Clear();
                source.objectPaths.Clear();
            }

            public bool Contains(PreloadEntry item) => IndexOf(item) >= 0;

            public void CopyTo(PreloadEntry[] array, int arrayIndex)
            {
                for (int i = 0; i < Count; i++)
                {
                    array[arrayIndex + i] = this[i];
                }
            }

            public IEnumerator<PreloadEntry> GetEnumerator()
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return this[i];
                }
            }

            public int IndexOf(PreloadEntry item)
            {
                for (int i = 0; i < Count; i++)
                {
                    if (source.sceneNames[i] == item.SceneName && source.objectPaths[i] == item.ObjectPath)
                    {
                        return i;
                    }
                }

                return -1;
            }

            public void Insert(int index, PreloadEntry item)
            {
                source.sceneNames.Insert(index, item.SceneName);
                source.objectPaths.Insert(index, item.ObjectPath);
            }

            public bool Remove(PreloadEntry item)
            {
                int index = IndexOf(item);
                if (index < 0)
                {
                    return false;
                }

                RemoveAt(index);
                return true;
            }

            public void RemoveAt(int index)
            {
                source.sceneNames.RemoveAt(index);
                source.objectPaths.RemoveAt(index);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public readonly struct PreloadEntry : IEquatable<PreloadEntry>
        {
            public readonly string SceneName;
            public readonly string ObjectPath;

            public PreloadEntry(string sceneName, string objectPath)
            {
                SceneName = sceneName;
                ObjectPath = objectPath;
            }

            public bool Equals(PreloadEntry other) => SceneName == other.SceneName && ObjectPath == other.ObjectPath;
            public override bool Equals(object obj) => obj is PreloadEntry other && Equals(other);
            public override int GetHashCode() => ((SceneName?.GetHashCode() ?? 0) * 397) ^ (ObjectPath?.GetHashCode() ?? 0);
            public override string ToString() => $"{SceneName}:{ObjectPath}";
        }

        static readonly List<PreloadRegistry> loadedRegistries = new List<PreloadRegistry>();
        static readonly Dictionary<(string scene, string path), GameObject> globalLoadedObjects = new Dictionary<(string scene, string path), GameObject>();
        static GameObject preloadContainer;

        [SerializeField]
        [HideInInspector]
        List<string> sceneNames = new List<string>();

        [SerializeField]
        [HideInInspector]
        List<string> objectPaths = new List<string>();

        readonly Dictionary<(string scene, string path), GameObject> loadedObjects = new Dictionary<(string scene, string path), GameObject>();
        PreloadEntryListView entries;

        public IList<PreloadEntry> Entries => entries ??= new PreloadEntryListView(this);
        public IEnumerable<GameObject> LoadedObjects => loadedObjects.Values.Where(o => o != null);
        public static IEnumerable<PreloadRegistry> LoadedRegistries => loadedRegistries;
        public static IEnumerable<GameObject> AllLoadedObjects => globalLoadedObjects.Values.Where(o => o != null);

        [OnRuntimeInit]
        static void OnRuntimeInit()
        {
            WeaverCore_ModClass.OnPreloadNames += CollectPreloadNames;
            WeaverCore_ModClass.OnPreloadedObjects += InitializeLoadedObjects;
        }

        static List<(string, string)> CollectPreloadNames()
        {
            loadedRegistries.Clear();

            var results = new List<(string, string)>();
            foreach (var registry in Registry.GetAllFeatures<PreloadRegistry>())
            {
                if (registry == null)
                {
                    continue;
                }

                loadedRegistries.Add(registry);
                registry.loadedObjects.Clear();

                foreach (var entry in registry.Entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.SceneName) || string.IsNullOrWhiteSpace(entry.ObjectPath))
                    {
                        continue;
                    }

                    var pair = (entry.SceneName, entry.ObjectPath);
                    if (!results.Contains(pair))
                    {
                        results.Add(pair);
                    }
                }
            }

            return results;
        }

        static void InitializeLoadedObjects(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            globalLoadedObjects.Clear();

            if (preloadedObjects == null || loadedRegistries.Count == 0)
            {
                return;
            }

            EnsurePreloadContainer();

            foreach (var registry in loadedRegistries)
            {
                registry.loadedObjects.Clear();

                foreach (var entry in registry.Entries)
                {
                    if (string.IsNullOrWhiteSpace(entry.SceneName) || string.IsNullOrWhiteSpace(entry.ObjectPath))
                    {
                        continue;
                    }

                    if (!preloadedObjects.TryGetValue(entry.SceneName, out var sceneObjects) ||
                        !sceneObjects.TryGetValue(entry.ObjectPath, out var loadedObject) ||
                        loadedObject == null)
                    {
                        WeaverLog.Log($"Didn't find Loaded Object for {entry.SceneName}:{entry.ObjectPath}");
                        continue;
                    }

                    var key = (entry.SceneName, entry.ObjectPath);
                    if (!globalLoadedObjects.TryGetValue(key, out var storedObject) || storedObject == null)
                    {
                        loadedObject.SetActive(false);
                        loadedObject.transform.SetParent(preloadContainer.transform);
                        loadedObject.SetActive(true);
                        globalLoadedObjects[key] = loadedObject;
                        storedObject = loadedObject;
                    }

                    registry.loadedObjects[key] = storedObject;
                }
            }
        }

        static void EnsurePreloadContainer()
        {
            if (preloadContainer != null)
            {
                return;
            }

            preloadContainer = new GameObject("PRELOAD_REGISTRY_CONTAINER");
            preloadContainer.hideFlags = HideFlags.HideAndDontSave;
            preloadContainer.SetActive(false);
            DontDestroyOnLoad(preloadContainer);
        }

        public GameObject GetObject(string sceneName, string objectPath)
        {
            loadedObjects.TryGetValue((sceneName, objectPath), out var result);
            return result;
        }

        public bool TryGetObject(string sceneName, string objectPath, out GameObject result)
        {
            return loadedObjects.TryGetValue((sceneName, objectPath), out result);
        }

        public IEnumerable<GameObject> GetAllObjects()
        {
            return LoadedObjects;
        }

        public IEnumerable<GameObject> GetObjectsByScene(string sceneName)
        {
            return loadedObjects.Where(pair => pair.Key.scene == sceneName).Select(pair => pair.Value).Where(o => o != null);
        }

        public IEnumerable<GameObject> GetObjectsByName(string objectName)
        {
            return loadedObjects.Where(pair => GetObjectName(pair.Key.path) == objectName).Select(pair => pair.Value).Where(o => o != null);
        }

        public IEnumerable<T> GetComponents<T>() where T : Component
        {
            foreach (var loadedObject in LoadedObjects)
            {
                if (loadedObject.TryGetComponent<T>(out var component))
                {
                    yield return component;
                }
            }
        }

        public IEnumerable<Component> GetComponents(Type componentType)
        {
            if (componentType == null || !typeof(Component).IsAssignableFrom(componentType))
            {
                yield break;
            }

            foreach (var loadedObject in LoadedObjects)
            {
                var component = loadedObject.GetComponent(componentType);
                if (component != null)
                {
                    yield return component;
                }
            }
        }

        public T GetFirstComponent<T>() where T : Component
        {
            return GetComponents<T>().FirstOrDefault();
        }

        public Component GetFirstComponent(Type componentType)
        {
            return GetComponents(componentType).FirstOrDefault();
        }

        public static GameObject GetObjectFromAnyRegistry(string sceneName, string objectPath)
        {
            globalLoadedObjects.TryGetValue((sceneName, objectPath), out var result);
            return result;
        }

        public static IEnumerable<GameObject> GetAllObjectsFromScene(string sceneName)
        {
            return globalLoadedObjects.Where(pair => pair.Key.scene == sceneName).Select(pair => pair.Value).Where(o => o != null);
        }

        public static IEnumerable<T> GetAllComponents<T>() where T : Component
        {
            return LoadedRegistries.SelectMany(registry => registry.GetComponents<T>());
        }

        public static IEnumerable<Component> GetAllComponents(Type componentType)
        {
            return LoadedRegistries.SelectMany(registry => registry.GetComponents(componentType));
        }

        public static string GetObjectName(string objectPath)
        {
            if (string.IsNullOrWhiteSpace(objectPath))
            {
                return null;
            }

            int separatorIndex = objectPath.LastIndexOf('/');
            return separatorIndex >= 0 && separatorIndex < objectPath.Length - 1 ? objectPath.Substring(separatorIndex + 1) : objectPath;
        }
    }
}
