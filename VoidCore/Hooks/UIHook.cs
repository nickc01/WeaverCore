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
        /// Determines how the UIHooks are loaded into the game.
        /// </summary>
        public class UIAllocator : Allocator
        {
            /// <summary>
            /// This is an internal method used to determine how the Hooks are created
            /// When the mod is loaded, this determines how the UIHook is created.
            /// This method returns null, meaning that the hook will be allocated later.
            /// They are instead allocated when the UIManager starts.
            /// </summary>
            /// <param name="hookType">The UIHook type</param>
            /// <returns></returns>
            public override object Allocate(Type hookType)
            {
                UIHook.AvailableHooks.Add(hookType);
                return null;
            }
        }
    }


    /// <summary>
    /// Hooks into the UI Manager. Inheriting from this will add the class as a component to the UI Manager GameObject.
    /// </summary>
    /// <example>
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
    ///</example>
public abstract class UIHook : MonoBehaviour, IHook<UIAllocator>
    {
        internal static List<Type> AvailableHooks = new List<Type>();
        void IHook.LoadHook() { }
    }
}
