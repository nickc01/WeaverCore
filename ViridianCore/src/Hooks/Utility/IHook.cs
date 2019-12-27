using Modding;
using ViridianCore.Hooks.Internal;

namespace ViridianCore.Hooks.Utility
{
    /// <summary>
    /// The base interface for implementing hooks. Hooks are ways in which you can hook into existing systems of the game.
    /// </summary>
    /// <typeparam name="TMod">The mod that this hook is bound to. The hook is loaded when the mod is loaded, and the hook is unloaded when the mod is unloaded</typeparam>
    public interface IHook<TMod> : IHookBase where TMod : IMod { }

    /// <summary>
    /// Similar to <see cref="IHook{TMod}"/>, but has the ability to change how the hook is created and destroyed. This can be used to greatly customize the hooking process
    /// </summary>
    /// <typeparam name="TMod">The mod that this hook is bound to</typeparam>
    /// <typeparam name="Alloc">The allocator that determines how the hook is created and destroyed</typeparam>
    public interface IHook<TMod, Alloc> : IHook<TMod> where Alloc : IAllocator where TMod : IMod { }
}
