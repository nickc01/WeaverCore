using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using ViridianCore.Hooks.Internal;
using ViridianCore.Hooks.Utility;

namespace ViridianCore.Hooks
{
    public abstract class DebugHook<Mod> : IHook<Mod> where Mod : IMod
    {
        protected virtual void Initialize() { }
        protected virtual void Uninitialize() { }

        protected abstract void DebugEnabled();
        protected abstract void DebugDisabled();

        void DebugEvent(bool enabled)
        {
            if (enabled)
            {
                DebugEnabled();
            }
            else
            {
                DebugDisabled();
            }
        }



        void IHookBase.LoadHook(IMod mod)
        {
            Initialize();
            if (Settings.DebugMode)
            {
                DebugEnabled();
            }
            Events.DebugEvent += DebugEvent;
        }

        void IHookBase.UnloadHook(IMod mod)
        {
            Uninitialize();
            if (Settings.DebugMode)
            {
                DebugDisabled();
            }
            Events.DebugEvent -= DebugEvent;
        }
    }
}
