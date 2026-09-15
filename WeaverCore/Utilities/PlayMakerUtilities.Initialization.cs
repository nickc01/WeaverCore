using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Playmaker;

namespace WeaverCore.Utilities
{
    public static partial class PlayMakerUtilities
    {
        class PlayMakerFSMInitCallback
        {
            public MethodInfo Method;
            public OnPlayMakerFSMInitAttribute Attribute;
        }

        static readonly Dictionary<Component, List<Action>> fsmInitCallbacks = new Dictionary<Component, List<Action>>();
        static readonly Dictionary<Component, HashSet<MethodInfo>> invokedFsmInitMethods = new Dictionary<Component, HashSet<MethodInfo>>();
        static readonly Dictionary<string, List<PlayMakerFSMInitCallback>> exactObjectAndFsmCallbacks = new Dictionary<string, List<PlayMakerFSMInitCallback>>();
        static readonly Dictionary<string, List<PlayMakerFSMInitCallback>> exactObjectCallbacks = new Dictionary<string, List<PlayMakerFSMInitCallback>>();
        static readonly Dictionary<string, List<PlayMakerFSMInitCallback>> exactFsmCallbacks = new Dictionary<string, List<PlayMakerFSMInitCallback>>();
        static readonly List<PlayMakerFSMInitCallback> wildcardCallbacks = new List<PlayMakerFSMInitCallback>();
        static List<Assembly> indexedAssemblies;

        [OnHarmonyPatch]
        static void PatchPlayMakerFSMInit(HarmonyPatcher patcher)
        {
            if (!PlayMakerAvailable || PlayMakerFSMType == null)
            {
                return;
            }

            var initMethod = PlayMakerFSMType.GetMethod(
                "Init",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                null,
                Type.EmptyTypes,
                null
            );
            var postfix = typeof(PlayMakerUtilities).GetMethod(
                nameof(PlayMakerFSM_Init_Postfix),
                BindingFlags.NonPublic | BindingFlags.Static
            );

            if (initMethod != null && postfix != null)
            {
                patcher.Patch(initMethod, null, postfix);
                InvokeLoadedPlayMakerFSMs();
            }
        }

        static void InvokeLoadedPlayMakerFSMs()
        {
            if (PlayMakerFSMType == null)
            {
                return;
            }

            foreach (var obj in Resources.FindObjectsOfTypeAll(PlayMakerFSMType))
            {
                if (!(obj is Component playMakerFSM)
                    || playMakerFSM.gameObject == null
                    || !playMakerFSM.gameObject.scene.IsValid()
                    || !IsFsmInitialized(playMakerFSM))
                {
                    continue;
                }

                InvokePlayMakerFSMInitMethods(playMakerFSM);
            }
        }

        [OnRuntimeInit]
        static void InvokeLoadedPlayMakerFSMsAtRuntime()
        {
            InvokeLoadedPlayMakerFSMs();
        }

        // public static void WaitForInit(Component playMakerFSM, Action action)
        // {
        //     if (action == null || !IsPlayMakerFSM(playMakerFSM))
        //     {
        //         return;
        //     }
        //
        //     if (IsFsmInitialized(playMakerFSM))
        //     {
        //         action.Invoke();
        //         return;
        //     }
        //
        //     if (!fsmInitCallbacks.TryGetValue(playMakerFSM, out var callbacks))
        //     {
        //         callbacks = new List<Action>();
        //         fsmInitCallbacks.Add(playMakerFSM, callbacks);
        //     }
        //
        //     callbacks.Add(action);
        // }

        static bool IsFsmInitialized(Component playMakerFSM)
        {
            object fsm = GetFsm(playMakerFSM);
            if (!IsFsm(fsm))
            {
                return false;
            }

            if (!fsm.ReflectGetProperty<bool>("Initialized"))
            {
                return false;
            }

            foreach (var state in GetStates(fsm))
            {
                if (state == null || !ReferenceEquals(GetStateFsm(state), fsm))
                {
                    return false;
                }
            }

            return true;
        }

        static void PlayMakerFSM_Init_Postfix(object __instance)
        {
            if (!(__instance is Component playMakerFSM))
            {
                return;
            }

            if (fsmInitCallbacks.TryGetValue(playMakerFSM, out var callbacks))
            {
                fsmInitCallbacks.Remove(playMakerFSM);

                foreach (var callback in callbacks)
                {
                    try
                    {
                        callback?.Invoke();
                    }
                    catch (Exception e)
                    {
                        WeaverLog.LogError("Error running a PlayMaker FSM initialization callback");
                        WeaverLog.LogException(e);
                    }
                }
            }

            InvokePlayMakerFSMInitMethods(playMakerFSM);
        }

        static void InvokePlayMakerFSMInitMethods(Component playMakerFSM)
        {
            if (!IsFsmInitialized(playMakerFSM))
            {
                return;
            }

            var methods = GetMatchingPlayMakerFSMInitCallbacks(playMakerFSM);
            if (methods.Count == 0)
            {
                return;
            }

            var wrapper = new PlayMakerFsmWrapper(playMakerFSM);
            if (!invokedFsmInitMethods.TryGetValue(playMakerFSM, out var invokedMethods))
            {
                invokedMethods = new HashSet<MethodInfo>();
                invokedFsmInitMethods.Add(playMakerFSM, invokedMethods);
            }

            foreach (var callback in methods)
            {
                var method = callback.Method;
                if (!invokedMethods.Add(method))
                {
                    continue;
                }

                try
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 0)
                    {
                        method.Invoke(null, null);
                    }
                    else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(PlayMakerFsmWrapper))
                    {
                        method.Invoke(null, new object[] { wrapper });
                    }
                }
                catch (Exception e)
                {
                    WeaverLog.LogError($"Error running PlayMaker FSM init callback [{method.DeclaringType.FullName}:{method.Name}]");
                    WeaverLog.LogException(e);
                }
            }
        }

        static List<PlayMakerFSMInitCallback> GetMatchingPlayMakerFSMInitCallbacks(Component playMakerFSM)
        {
            EnsurePlayMakerFSMInitIndex();

            var matches = new HashSet<PlayMakerFSMInitCallback>();
            var objectKeys = GetObjectPathKeys(playMakerFSM.transform);
            var fsmName = GetFsmName(playMakerFSM);

            foreach (var objectKey in objectKeys)
            {
                AddCallbacks(matches, exactObjectCallbacks, objectKey);
                AddCallbacks(matches, exactObjectAndFsmCallbacks, MakeCallbackKey(objectKey, fsmName));
            }

            AddCallbacks(matches, exactFsmCallbacks, fsmName);
            matches.UnionWith(wildcardCallbacks);

            var result = matches.ToList();
            result.Sort((a, b) => a.Attribute.Priority.CompareTo(b.Attribute.Priority));
            return result;
        }

        static void EnsurePlayMakerFSMInitIndex()
        {
            var assemblies = Initialization.GetWeaverCoreAssemblies().ToList();
            if (indexedAssemblies != null
                && indexedAssemblies.Count == assemblies.Count
                && indexedAssemblies.SequenceEqual(assemblies))
            {
                return;
            }

            exactObjectAndFsmCallbacks.Clear();
            exactObjectCallbacks.Clear();
            exactFsmCallbacks.Clear();
            wildcardCallbacks.Clear();

            foreach (var (method, attribute) in ReflectionUtilities.GetMethodsWithAttribute<OnPlayMakerFSMInitAttribute>(assemblies))
            {
                var callback = new PlayMakerFSMInitCallback
                {
                    Method = method,
                    Attribute = attribute
                };
                var objectPath = NormalizeCallbackFilter(attribute.ObjectPath);
                var fsmName = NormalizeCallbackFilter(attribute.FsmName);

                if (objectPath == "*" && fsmName == "*")
                {
                    wildcardCallbacks.Add(callback);
                }
                else if (objectPath == "*")
                {
                    AddCallback(exactFsmCallbacks, fsmName, callback);
                }
                else if (fsmName == "*")
                {
                    AddCallback(exactObjectCallbacks, objectPath, callback);
                }
                else
                {
                    AddCallback(exactObjectAndFsmCallbacks, MakeCallbackKey(objectPath, fsmName), callback);
                }
            }

            indexedAssemblies = assemblies;
        }

        static string NormalizeCallbackFilter(string value)
        {
            return string.IsNullOrEmpty(value) || value == "*"
                ? "*"
                : value.Trim('/');
        }

        static string MakeCallbackKey(string objectPath, string fsmName)
        {
            return objectPath + "\u001f" + fsmName;
        }

        static void AddCallback(Dictionary<string, List<PlayMakerFSMInitCallback>> dictionary, string key, PlayMakerFSMInitCallback callback)
        {
            if (!dictionary.TryGetValue(key, out var callbacks))
            {
                callbacks = new List<PlayMakerFSMInitCallback>();
                dictionary.Add(key, callbacks);
            }

            callbacks.Add(callback);
        }

        static void AddCallbacks(HashSet<PlayMakerFSMInitCallback> matches, Dictionary<string, List<PlayMakerFSMInitCallback>> dictionary, string key)
        {
            if (!string.IsNullOrEmpty(key) && dictionary.TryGetValue(key, out var callbacks))
            {
                matches.UnionWith(callbacks);
            }
        }

        static HashSet<string> GetObjectPathKeys(Transform transform)
        {
            var names = new List<string>();
            for (var current = transform; current != null; current = current.parent)
            {
                names.Insert(0, current.name);
            }

            var keys = new HashSet<string> { transform.name };
            for (var start = 0; start < names.Count; start++)
            {
                keys.Add(string.Join("/", names.Skip(start)));
            }

            return keys;
        }
    }
}
