using System;
using System.Collections.Generic;
using Modding;
using UnityEngine;
using CrystalCore.Hooks.Internal;

namespace CrystalCore.Hooks
{
    namespace Utility
    {

        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
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

            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            public abstract void Initialize();
        }
    }
}
