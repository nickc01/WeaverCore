using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VoidCore.Hooks.Internal;
using VoidCore.Hooks.Utility;

namespace VoidCore.Hooks
{
    namespace Internal
    {
        /// <summary>
        /// An injector implementation for the <see cref="HeroController"/>
        /// </summary>
        public class PlayerInjector : HookInjector
        {
            /// <summary>
            /// Called when the HeroController is initialized. Used to add the hooks into the <see cref="HeroController"/>
            /// </summary>
            public override void Initialize()
            {
                On.HeroController.Start += PlayerStart;
            }

            private void PlayerStart(On.HeroController.orig_Start orig, HeroController self)
            {
                orig(self);
                InjectInto(self);
            }
        }
    }


    /// <summary>
    /// Hooks into the Hero Controller. Inheriting from this will have this class added as a component to the HeroController
    /// </summary>
    /// <example>
    /// <code>
    ///public class ExampleHook : PlayerHook
    ///{
    ///    void Start()
    ///    {
    ///        //Called when the HeroController Starts
    ///    }
    ///
    ///    void Update()
    ///    {
    ///        //Called each frame on the HeroController
    ///    }
    ///}
    /// </code>
    /// </example>
    public abstract class PlayerHook<Mod> : MonoBehaviour, IHook<Mod, InjectionAllocator<PlayerInjector>> where Mod : IMod
    {
        /// <summary>
        /// Called when the PlayerHook is loading
        /// </summary>
        /// <param name="mod">The mod that the PlayerHook is bound to</param>
        public virtual void LoadHook(IMod mod) { }
        /// <summary>
        /// Called when the PlayerHook is unloading
        /// </summary>
        /// <param name="mod">The mod that the PlayerHook is bound to</param>
        public virtual void UnloadHook(IMod mod) { }
    }
}
