using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using CrystalCore.Hooks.Internal;
using CrystalCore.Hooks.Utility;

namespace CrystalCore.Hooks
{
    namespace Internal
    {
        /// <summary>
        /// An injector implementation for the <see cref="UIManager"/>
        /// </summary>
        public class UIInjector : HookInjector
        {
            /// <summary>
            /// Called when the UIInjector is initialized. Used to add the hooks into the <see cref="UIManager"/>
            /// </summary>
            public override void Initialize()
            {
                On.UIManager.Start += UIManagerStart;
            }

            private void UIManagerStart(On.UIManager.orig_Start orig, UIManager self)
            {
                orig(self);
                InjectInto(self);
            }
        }
    }


    /// <summary>
    /// Hooks into the UI Manager. Inheriting from this will add the class as a component to the UI Manager GameObject.
    /// </summary>
    /// <example>
    /// <code>
    /// public class ExampleHook : UIHook
    /// {
    ///     void Start()
    ///     {
    ///         //Called when the UI Manager first starts
    ///     }
    ///
    ///     void Update()
    ///     {
    ///         //Called each frame on the UI Manager GameObject
    ///     }
    /// }
    /// </code>
    ///</example>
    public abstract class UIHook<Mod> : MonoBehaviour, IHook<Mod, InjectionAllocator<UIInjector>> where Mod : IMod
    {
        /// <summary>
        /// Called when the PlayerHook is loading
        /// </summary>
        /// <param name="mod">The mod that the UIHook is bound to</param>
        public abstract void LoadHook(IMod mod);
        /// <summary>
        /// Called when the PlayerHook is unloading
        /// </summary>
        /// <param name="mod">The mod that the UIHook is bound to</param>
        public abstract void UnloadHook(IMod mod);
    }
}
