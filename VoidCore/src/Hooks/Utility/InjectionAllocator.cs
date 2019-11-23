using System;
using System.Collections.Generic;
using Modding;
using VoidCore.Hooks.Internal;

namespace VoidCore.Hooks
{
    namespace Utility
    {
        
        
        
        
        public class InjectionAllocator<Injector> : IAllocator where Injector : HookInjector, new()
        {

            static Dictionary<IAllocator, (Type hook, IMod mod)> Hooks = new Dictionary<IAllocator, (Type, IMod)>();

            static bool started = false;

            static Injector injector;

            void Initialize()
            {
                injector = new Injector();
                injector.Initialize();
                foreach (var hook in Hooks)
                {
                    injector.Add(hook.Value.hook,hook.Value.mod);
                }
            }

            IHookBase IAllocator.Allocate(Type hook, IMod mod)
            {
                if (!started)
                {
                    started = true;
                    Initialize();
                }
                Hooks.Add(this, (hook, mod));
                injector.Add(hook, mod);
                return null;
            }

            void IAllocator.Deallocate(IHookBase hook, IMod mod)
            {
                injector.Remove(Hooks[this].hook,mod);
                Hooks.Remove(this);
            }
        }
    }
}
