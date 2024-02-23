using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore
{
	public interface ICachedPrefab
	{
        void Clear();
	}

    static class CachedPrefab_Common
    {
        public static HashSet<ICachedPrefab> loadedPrefabs = new HashSet<ICachedPrefab>();

        [OnRuntimeInit]
        static void OnRuntimeInit()
        {
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
        }

        private static void SceneManager_sceneUnloaded(UnityEngine.SceneManagement.Scene arg0)
        {
            foreach (var prefab in loadedPrefabs)
            {
                prefab.Clear();
            }
            loadedPrefabs.Clear();
        }
    }

    public sealed class CachedPrefab<T> : ICachedPrefab where T : UnityEngine.Object
    {
        T _value = default;

        public T Value
        {
            get => _value;

            set
            {
                _value = value;
                CachedPrefab_Common.loadedPrefabs.Add(this);
            }
        }

        public void Clear()
        {
            _value = default;
        }
    }
}
