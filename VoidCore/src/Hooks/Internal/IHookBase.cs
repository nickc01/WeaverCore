using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoidCore.Hooks.Utility;

namespace VoidCore.Hooks.Internal
{
    /// <summary>
    /// The base class for the hook interface. This is not mean't to be inherited directly. Use <see cref="IHook{TMod}"/> or <see cref="IHook{TMod, Alloc}"/>
    /// </summary>
    public interface IHookBase
    {
        /// <summary>
        /// Called when the Hook is loaded
        /// </summary>
        /// <param name="mod">The mod that the hook is bound to</param>
        void LoadHook(IMod mod);
        /// <summary>
        /// Called when the hook is unloaded.
        /// </summary>
        /// <param name="mod">The unloading mod that this hook is bound to</param>
        /// <remarks>
        /// NOTE: This method is only called when the mod is unloaded from the mod screen AND the hook is currently instantiated somewhere
        /// </remarks>
        void UnloadHook(IMod mod);
    }
}
