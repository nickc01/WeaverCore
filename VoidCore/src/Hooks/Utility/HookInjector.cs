using System;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using VoidCore.Hooks.Internal;

namespace VoidCore.Hooks
{
    namespace Utility
    {

        /// <summary>
        /// The base class for implementing a hook injector. It is used to determine what gameObjects in the scene gets the hooks attached to it as Components
        /// </summary>
        /// <example>
        ///public class UIUser : HookInjector
        /// {
        ///     public override void Initialize()
        ///     {
        ///         On.UIManager.Start += UIManagerStart;
        ///     }
        ///
        ///     private void UIManagerStart(On.UIManager.orig_Start orig, UIManager self)
        ///     {
        ///         orig(self);
        ///         InjectInto(self);
        ///     }
        /// }
        /// </example>
        public abstract class HookInjector
        {
            List<HookInjection> Injections = new List<HookInjection>();
            List<(Type hook,IMod mod)> Hooks = new List<(Type hook,IMod mod)>();

            class HookInjection : MonoBehaviour
            {
                HookInjector Injector;
                Dictionary<Type, IHookBase> LoadedHooks = new Dictionary<Type, IHookBase>();

                internal void Initialize(HookInjector user)
                {
                    Injector = user;
                }

                internal void Add(Type hook,IMod mod)
                {
                    var component = (IHookBase)gameObject.AddComponent(hook);
                    component.LoadHook(mod);
                    LoadedHooks.Add(hook, component);
                }

                internal void Remove(Type hook,IMod mod)
                {
                    var instance = LoadedHooks[hook];
                    instance.UnloadHook(mod);
                    LoadedHooks.Remove(hook);
                    Destroy((Component)instance);
                }

                void OnDestroy()
                {
                    Injector.Injections.Remove(this);
                }
            }

            /// <summary>
            /// Call this method to inject the selected gameObject with the hooks
            /// </summary>
            /// <param name="gameObject">The gameObject to inject the hooks into</param>
            protected void InjectInto(GameObject gameObject)
            {
                if (gameObject.GetComponent<HookInjection>() == null)
                {
                    var monitor = gameObject.AddComponent<HookInjection>();
                    monitor.Initialize(this);
                    Injections.Add(monitor);
                    foreach (var hook in Hooks)
                    {
                        monitor.Add(hook.hook, hook.mod);
                    }
                }
            }

            /// <summary>
            /// Similar to <see cref="InjectInto(GameObject)"/>. Makes it easier to inject into the gameObject
            /// </summary>
            /// <typeparam name="T">The type of component</typeparam>
            /// <param name="component">The component with the gameObject to inject into</param>
            protected void InjectInto<T>(T component) where T : Component
            {
                InjectInto(component.gameObject);
            }

            internal void Add(Type hook,IMod mod)
            {
                Hooks.Add((hook,mod));
                foreach (var monitor in Injections)
                {
                    monitor.Add(hook,mod);
                }
            }

            internal void Remove(Type hook,IMod mod)
            {
                Hooks.Remove((hook,mod));
                foreach (var monitor in Injections)
                {
                    monitor.Remove(hook,mod);
                }
            }

            /// <summary>
            /// Called when the hooks are to be initialized. This is where you find the gameObjects you want to inject into. Call <see cref="InjectInto(GameObject)"/> to inject the gameObject with the hooks
            /// </summary>
            /// <example>
            /// public override void Initialize()
            /// {
            ///     On.HeroController.Start += PlayerStart;
            /// }
            /// 
            /// private void PlayerStart(On.HeroController.orig_Start orig, HeroController self)
            /// {
            ///     orig(self);
            ///     InjectInto(self);
            /// }
            /// </example>
            public abstract void Initialize();
        }
    }
}
