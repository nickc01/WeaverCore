
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Game.Patches;
using WeaverCore.Utilities;

public static class SceneLoad_Patches
{
    // Field getters
    static Func<SceneLoad, MonoBehaviour> getRunner;
    static Func<SceneLoad, string> getTargetSceneName;

    // Property getters
    static Func<SceneLoad, bool> getIsFetchAllowed;
    static Func<SceneLoad, bool> getIsActivationAllowed;
    static Func<SceneLoad, bool> getIsUnloadAssetsRequired;
    static Func<SceneLoad, bool> getIsGarbageCollectRequired;

    // Property setters
    static Action<SceneLoad, bool> setIsFinished;

    // Method delegates
    static Action<SceneLoad, SceneLoad.Phases> recordBeginTime;
    static Action<SceneLoad, SceneLoad.Phases> recordEndTime;

    // Event field getters
    static Func<SceneLoad, SceneLoad.FetchCompleteDelegate> getFetchComplete;
    static Func<SceneLoad, SceneLoad.WillActivateDelegate> getWillActivate;
    static Func<SceneLoad, SceneLoad.ActivationCompleteDelegate> getActivationComplete;
    static Func<SceneLoad, SceneLoad.CompleteDelegate> getComplete;
    static Func<SceneLoad, SceneLoad.StartCalledDelegate> getStartCalled;
    static Func<SceneLoad, SceneLoad.BossLoadCompleteDelegate> getBossLoaded;
    static Func<SceneLoad, SceneLoad.FinishDelegate> getFinish;

    [OnHarmonyPatch]
    static void OnHarmonyPatch(HarmonyPatcher patcher)
    {
        // Initialize field accessors
        getRunner = ReflectionUtilities.CreateFieldGetter<SceneLoad, MonoBehaviour>("runner");
        getTargetSceneName = ReflectionUtilities.CreateFieldGetter<SceneLoad, string>("targetSceneName");

        // Initialize property accessors
        getIsFetchAllowed = ReflectionUtilities.CreatePropertyGetter<SceneLoad, bool>("IsFetchAllowed");
        getIsActivationAllowed = ReflectionUtilities.CreatePropertyGetter<SceneLoad, bool>("IsActivationAllowed");
        getIsUnloadAssetsRequired = ReflectionUtilities.CreatePropertyGetter<SceneLoad, bool>("IsUnloadAssetsRequired");
        getIsGarbageCollectRequired = ReflectionUtilities.CreatePropertyGetter<SceneLoad, bool>("IsGarbageCollectRequired");

        setIsFinished = ReflectionUtilities.CreatePropertySetter<SceneLoad, bool>("IsFinished");

        // Initialize method delegates
        recordBeginTime = ReflectionUtilities.MethodToDelegate<Action<SceneLoad, SceneLoad.Phases>, SceneLoad>("RecordBeginTime");
        recordEndTime = ReflectionUtilities.MethodToDelegate<Action<SceneLoad, SceneLoad.Phases>, SceneLoad>("RecordEndTime");

        // Initialize event field accessors
        getFetchComplete = ReflectionUtilities.CreateFieldGetter<SceneLoad, SceneLoad.FetchCompleteDelegate>("FetchComplete");
        getWillActivate = ReflectionUtilities.CreateFieldGetter<SceneLoad, SceneLoad.WillActivateDelegate>("WillActivate");
        getActivationComplete = ReflectionUtilities.CreateFieldGetter<SceneLoad, SceneLoad.ActivationCompleteDelegate>("ActivationComplete");
        getComplete = ReflectionUtilities.CreateFieldGetter<SceneLoad, SceneLoad.CompleteDelegate>("Complete");
        getStartCalled = ReflectionUtilities.CreateFieldGetter<SceneLoad, SceneLoad.StartCalledDelegate>("StartCalled");
        getBossLoaded = ReflectionUtilities.CreateFieldGetter<SceneLoad, SceneLoad.BossLoadCompleteDelegate>("BossLoaded");
        getFinish = ReflectionUtilities.CreateFieldGetter<SceneLoad, SceneLoad.FinishDelegate>("Finish");

        {
            WeaverLog.Log("APPLYING CUSTOM BEGIN ROUTINE PATCH!!!!!!");
            var orig = typeof(SceneLoad).GetMethod("BeginRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefix = typeof(SceneLoad_Patches).GetMethod(nameof(BeginRoutine_Prefix), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            patcher.Patch(orig, prefix, null);
        }

        {
            var orig = typeof(GameManager).GetMethod("BeginSceneTransitionRoutine", BindingFlags.NonPublic | BindingFlags.Instance);
            var prefix = typeof(SceneLoad_Patches).GetMethod(nameof(BeginSceneTransitionRoutine_Prefix), BindingFlags.Static | BindingFlags.NonPublic);
            patcher.Patch(orig, prefix, null);
        }
    }
    
    private static bool BeginSceneTransitionRoutine_Prefix(GameManager.SceneLoadInfo info)
    {
        //WeaverLog.Log("CHANGING SCENE LOAD SCENE FROM " + info.SceneName + " to " + UnitySceneManager_Patches.ReplaceScene(info.SceneName));
        //info.SceneName = UnitySceneManager_Patches.ReplaceScene(info.SceneName);
        return true;
    }

    private static bool BeginRoutine_Prefix(SceneLoad __instance, ref IEnumerator __result)
    {
        IEnumerator Func()
        {
            //WeaverLog.Log("BEGINNING CUSTOM ROUTINE!!! = " +  __instance);
            SceneAdditiveLoadConditional.loadInSequence = true;
            yield return getRunner(__instance).StartCoroutine(ScenePreloader.FinishPendingOperations());
            recordBeginTime(__instance, SceneLoad.Phases.FetchBlocked);
            while (!getIsFetchAllowed(__instance))
            {
                yield return null;
            }
            recordEndTime(__instance, SceneLoad.Phases.FetchBlocked);
            recordBeginTime(__instance, SceneLoad.Phases.Fetch);
            var targetSceneName = getTargetSceneName(__instance);
            var resolvedTargetSceneName = UnitySceneManager_Patches.ReplaceScene(targetSceneName, out _);
            // Inverse scene unions are baked into normal replacements during the build.
            AsyncOperation loadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(resolvedTargetSceneName, LoadSceneMode.Additive);
            //WeaverLog.Log("Load Operation = " + loadOperation);
            //WeaverLog.Log("Load Operation Is Null = " + (loadOperation == null));
            loadOperation.allowSceneActivation = false;
            while (loadOperation.progress < 0.9f)
            {
                yield return null;
            }

            recordEndTime(__instance, SceneLoad.Phases.Fetch);
            if (getFetchComplete(__instance) != null)
            {
                try
                {
                    getFetchComplete(__instance)();
                }
                catch (Exception exception)
                {
                    Debug.LogError("Exception in responders to SceneLoad.FetchComplete. Attempting to continue load regardless.");
                    Debug.LogException(exception);
                }
            }
            recordBeginTime(__instance, SceneLoad.Phases.ActivationBlocked);
            while (!getIsActivationAllowed(__instance))
            {
                yield return null;
            }
            recordEndTime(__instance, SceneLoad.Phases.ActivationBlocked);
            recordBeginTime(__instance, SceneLoad.Phases.Activation);
            if (getWillActivate(__instance) != null)
            {
                try
                {
                    getWillActivate(__instance)();
                }
                catch (Exception exception2)
                {
                    Debug.LogError("Exception in responders to SceneLoad.WillActivate. Attempting to continue load regardless.");
                    Debug.LogException(exception2);
                }
            }
            loadOperation.allowSceneActivation = true;
            yield return loadOperation;

            /*foreach (var listener in GameObject.FindObjectsOfType<AudioListener>())
            {
                listener.enabled = false;
                listener.enabled = true;
            }*/

            recordEndTime(__instance, SceneLoad.Phases.Activation);
            if (getActivationComplete(__instance) != null)
            {
                try
                {
                    getActivationComplete(__instance)();
                }
                catch (Exception exception3)
                {
                    Debug.LogError("Exception in responders to SceneLoad.ActivationComplete. Attempting to continue load regardless.");
                    Debug.LogException(exception3);
                }
            }
            recordBeginTime(__instance, SceneLoad.Phases.UnloadUnusedAssets);
            if (getIsUnloadAssetsRequired(__instance))
            {
                yield return Resources.UnloadUnusedAssets();
            }
            recordEndTime(__instance, SceneLoad.Phases.UnloadUnusedAssets);
            recordBeginTime(__instance, SceneLoad.Phases.GarbageCollect);
            if (getIsGarbageCollectRequired(__instance))
            {
                GCManager.Collect();
            }
            recordEndTime(__instance, SceneLoad.Phases.GarbageCollect);
            if (getComplete(__instance) != null)
            {
                try
                {
                    getComplete(__instance)();
                }
                catch (Exception exception4)
                {
                    Debug.LogError("Exception in responders to SceneLoad.Complete. Attempting to continue load regardless.");
                    Debug.LogException(exception4);
                }
            }
            recordBeginTime(__instance, SceneLoad.Phases.StartCall);
            yield return null;
            recordEndTime(__instance, SceneLoad.Phases.StartCall);
            if (getStartCalled(__instance) != null)
            {
                try
                {
                    getStartCalled(__instance)();
                }
                catch (Exception exception5)
                {
                    Debug.LogError("Exception in responders to SceneLoad.StartCalled. Attempting to continue load regardless.");
                    Debug.LogException(exception5);
                }
            }
            if (SceneAdditiveLoadConditional.ShouldLoadBoss)
            {
                recordBeginTime(__instance, SceneLoad.Phases.LoadBoss);
                yield return getRunner(__instance).StartCoroutine(SceneAdditiveLoadConditional.LoadAll());
                recordEndTime(__instance, SceneLoad.Phases.LoadBoss);
                try
                {
                    if (getBossLoaded(__instance) != null)
                    {
                        getBossLoaded(__instance)();
                    }
                    if ((bool)GameManager.instance)
                    {
                        GameManager.instance.LoadedBoss();
                    }
                }
                catch (Exception exception6)
                {
                    Debug.LogError("Exception in responders to SceneLoad.BossLoaded. Attempting to continue load regardless.");
                    Debug.LogException(exception6);
                }
            }
            try
            {
                ScenePreloader.Cleanup();
            }
            catch (Exception exception7)
            {
                Debug.LogError("Exception in responders to ScenePreloader.Cleanup. Attempting to continue load regardless.");
                Debug.LogException(exception7);
            }
            setIsFinished(__instance, true);
            if (getFinish(__instance) != null)
            {
                try
                {
                    getFinish(__instance)();
                }
                catch (Exception exception8)
                {
                    Debug.LogError("Exception in responders to SceneLoad.Finish. Attempting to continue load regardless.");
                    Debug.LogException(exception8);
                }
            }
        }

        //WeaverLog.Log("CUSTOM LOAD SCENE PREFIX CALLED!!!");

        __result = Func();
        return false;
    }
}
