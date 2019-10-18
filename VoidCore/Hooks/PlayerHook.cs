using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VoidCore.Hooks.Utility;

namespace VoidCore.Hooks
{
    namespace Utility
    {
        /// <summary>
        /// Determines how the PlayerHooks are loaded into the game
        /// </summary>
        public class PlayerAllocator : Allocator
        {
            /// <summary>
            /// This is an internal method used to determine how the Hooks are created
            /// When the mod is loaded, this determines how the PlayerHook is created.
            /// This method returns null, meaning that the hook will be allocated later.
            /// They are instead allocated when the HeroController starts.
            /// </summary>
            /// <param name="originalType">The PlayerHook Type</param>
            /// <returns></returns>
            public override object Allocate(Type originalType)
            {
                PlayerHook.AvailableHooks.Add(originalType);
                return null;
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
    public abstract class PlayerHook : MonoBehaviour, IHook<PlayerAllocator>
    {
        internal static List<Type> AvailableHooks = new List<Type>();

        void IHook.LoadHook() { }
    }
}
