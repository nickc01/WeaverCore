using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Playmaker;



namespace WeaverCore.Utilities
{
    public class CustomHatchlingSpawner : MonoBehaviour
    {
        public CustomHatchling HatchlingPrefab { get; private set; }
        public HashSet<CustomHatchling> SpawnedHatchlings { get; private set; } = new HashSet<CustomHatchling>();
        public int MaxCount { get; private set; } = 4;
        public Charm ActiveCharm { get; private set; } = Charm.GlowingWomb;
        public int SpawnedCount => SpawnedHatchlings.Where(h => h != null && h.isActiveAndEnabled).Count();
        public float SpawnDelayTime { get; set; } = 4;
        public int SoulCost { get; set; } = 8;

        public float LastSpawnTimeStamp { get; private set; } = -1;

        [NonSerialized]
        string currentSceneName = null;

        [OnRuntimeInit]
        static void OnRuntimeInit()
        {
            EventManager.OnEventTriggered += OnEventTriggered;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoad;
        }

        public static CustomHatchlingSpawner AddToPlayer(CustomHatchling hatchlingPrefab, int maxCount = 4, float spawnDelayTime = 4f, Charm activeCharm = Charm.GlowingWomb)
        {
            return AddToPlayer<CustomHatchlingSpawner>(hatchlingPrefab, maxCount, spawnDelayTime, activeCharm);
        }

        public static T AddToPlayer<T>(CustomHatchling hatchlingPrefab, int maxCount = 4, float spawnDelayTime = 4f, Charm activeCharm = Charm.GlowingWomb) where T : CustomHatchlingSpawner
        {
            var instance = Player.Player1.gameObject.AddComponent<T>();
            instance.HatchlingPrefab = hatchlingPrefab;
            instance.MaxCount = maxCount;
            instance.SpawnDelayTime = spawnDelayTime;
            instance.ActiveCharm = activeCharm;

            return instance;
        }

        protected virtual void Awake()
        {
            currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            LastSpawnTimeStamp = Time.time;
            StartCoroutine(MainRoutine());
        }

        static void OnEventTriggered(string eventName, GameObject source, GameObject destination, EventManager.EventType eventType)
        {
            if (eventName == "LEAVING SCENE" || eventName == "CHARM EQUIP CHECK" || eventName == "ALL CHARMS END")
            {
                if (HeroController.instance != null)
                {
                    foreach (var spawner in HeroController.instance.GetComponents<CustomHatchlingSpawner>())
                    {
                        if (eventName == "LEAVING SCENE")
                        {
                            spawner.OnLeaveScene();
                        }

                        if (eventName == "CHARM EQUIP CHECK" || eventName == "ALL CHARMS END")
                        {
                            spawner.OnCharmsUpdated();
                        }
                    }
                }
            }
        }

        static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (HeroController.instance != null)
            {
                foreach (var spawner in HeroController.instance.GetComponents<CustomHatchlingSpawner>())
                {
                    spawner.currentSceneName = scene.name;
                }
            }
        }

        IEnumerator MainRoutine()
        {
            yield return null;
            while (true)
            {
                if (CanSpawn())
                {
                    SpawnedHatchlings.RemoveWhere(h => h != null && h.isActiveAndEnabled);
                    var instance = SpawnObj(true);
                    instance.OnSpawn(this);
                    SpawnedHatchlings.Add(instance);
                }
                yield return null;
            }
        }

        protected virtual bool CanSpawn()
        {
            return HeroController.instance.isHeroInPosition &&
            !PlayMakerUtilities.GetFsmBool(HeroController.instance.gameObject, "ProxyFSM", "No Charms") &&
            PlayerData.instance.GetBool($"equippedCharm_{(int)ActiveCharm}") &&
            Time.time >= LastSpawnTimeStamp + SpawnDelayTime &&
            SpawnedCount < MaxCount &&
            PlayerData.instance.GetInt("MPCharge") >= SoulCost;

        }

        protected virtual CustomHatchling SpawnObj(bool takeSoul)
        {
            if (takeSoul)
            {
                HeroController.instance.TakeMP(SoulCost);
            }
            LastSpawnTimeStamp = Time.time;

            if (HatchlingPrefab.GetComponent<PoolableObject>() != null)
            {
                return Pooling.Instantiate(HatchlingPrefab, HeroController.instance.transform.position, Quaternion.identity);
            }
            else
            {
                return GameObject.Instantiate(HatchlingPrefab, HeroController.instance.transform.position, Quaternion.identity);
            }
        }

        protected virtual void OnLeaveScene()
        {
            StopAllCoroutines();
            StartCoroutine(OnLeaveSceneRoutine());
        }

        IEnumerator OnLeaveSceneRoutine()
        {
            if (SpawnedCount <= 0)
            {
                yield return MainRoutine();
                yield break;
            }

            var oldSpawnCount = SpawnedCount;

            var currentScene = currentSceneName;
            yield return new WaitUntil(() => currentScene != currentSceneName);

            if (PlayMakerUtilities.GetFsmBool(HeroController.instance.gameObject, "ProxyFSM", "No Charms") || GameManager.instance.IsGameplayScene())
            {
                foreach (var instance in SpawnedHatchlings.Where(h => h != null && h.isActiveAndEnabled))
                {
                    instance.OnDeath(this);
                }

                SpawnedHatchlings.Clear();
                yield break;
            }

            yield return new WaitUntil(() => HeroController.instance.isHeroInPosition);

            for (int i = 0; i < oldSpawnCount; i++)
            {
                var instance = SpawnObj(false);
                instance.OnSpawn(this);
                SpawnedHatchlings.Add(instance);
            }

            yield return MainRoutine();
        }

        protected virtual void OnCharmsUpdated()
        {
            if (PlayerData.instance.GetBool($"equippedCharm_{(int)ActiveCharm}"))
            {
                LastSpawnTimeStamp = Time.time;
            }
            else
            {
                foreach (var instance in SpawnedHatchlings.Where(h => h != null && h.isActiveAndEnabled))
                {
                    instance.OnDeath(this);
                }

                SpawnedHatchlings.Clear();
            }
        }

        static PlayMakerFsmWrapper hatchlingFsm;

        public static bool DefaultSpawnerEnabled => ((MonoBehaviour)hatchlingFsm.InternalComponent)?.enabled ?? false;

        static int defaultSpawnerRefCount = 0;

        [OnPlayerInit]
        static void OnPlayerLoad()
        {
            defaultSpawnerRefCount = 0;
            if (Initialization.Environment == RunningState.Editor)
            {
                return;
            }
            hatchlingFsm = PlayMakerUtilities.FindPlayMakerFSMWrapper(Player.Player1.gameObject, "Hatchling Spawn");
        }

        public static void EnableDefaultSpawner()
        {
            if (defaultSpawnerRefCount == 1)
            {
                defaultSpawnerRefCount--;
                ((MonoBehaviour)hatchlingFsm.InternalComponent).enabled = true;
            }
            else if (defaultSpawnerRefCount > 1)
            {
                defaultSpawnerRefCount--;
            }
        }

        public static void DisableDefaultSpawner()
        {
            if (defaultSpawnerRefCount++ == 0)
            {
                EventManager.BroadcastEvent("K HATCHLING END", null);
                ((MonoBehaviour)hatchlingFsm.InternalComponent).enabled = false;
            }
        }

    }
}