using Modding;
using System;
using System.Collections;
using VoidCore.Hooks.Internal;

namespace VoidCore.Hooks.Utility
{
    /// <summary>
    /// Used to customize how the hooks are allocated and deallocated
    /// </summary>
    public interface IAllocator
    {
        /// <summary>
        /// Determines how the hook is allocated
        /// </summary>
        /// <param name="hook">The hook type to be allocated</param>
        /// <param name="mod">The mod that this hook came from</param>
        /// <returns>Returns the allocated object. Note that allocators can return null for this if the hook is going to be allocated later</returns>
        IHookBase Allocate(Type hook, IMod mod);
        /// <summary>
        /// Determines how the hook is deallocated. This is called when the mod is being unloaded
        /// </summary>
        /// <param name="hook">The hook to be deallocated</param>
        /// <param name="mod">THe mod that this hook came from</param>
        void Deallocate(IHookBase hook, IMod mod);
    }
}
