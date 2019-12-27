using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using UnityEngine;
using ViridianCore;
using ViridianCore.Hooks;
using ViridianCore.Hooks.Internal;
using ViridianCore.Hooks.Utility;

using HutongGames.PlayMaker;

namespace ViridianCore.Hooks
{
    namespace Internal
    {
        public class EnemyInjector : HookInjector
        {
            public override void Initialize()
            {
                On.HealthManager.Start += HealthManager_Start;
                //On.HutongGames.PlayMaker.ActionData.LoadActions += ActionData_LoadActions;
            }

            /*private FsmStateAction[] ActionData_LoadActions(On.HutongGames.PlayMaker.ActionData.orig_LoadActions orig, ActionData self, FsmState state)
            {

                return orig(self,state);
            }*/

            private void HealthManager_Start(On.HealthManager.orig_Start orig, HealthManager self)
            {
                orig(self);
                InjectInto(self);
            }
        }
    }


    public abstract class EnemyHook<Mod> : MonoBehaviour, IHook<Mod, InjectionAllocator<EnemyInjector>> where Mod : IMod
    {
        void IHookBase.LoadHook(IMod mod) { }

        void IHookBase.UnloadHook(IMod mod) { }
    }
}


/*public class GameObjectDebugger : GMTrackingHook<ViridianCore.ViridianCore>
{
    protected override void TrackingDisabled()
    {
        Events.GameObjectCreated -= GMCreated;
    }

    protected override void TrackingEnabled()
    {
        Events.GameObjectCreated += GMCreated;
    }

    private void GMCreated(UnityEngine.GameObject g, bool arg2)
    {
        Modding.Logger.Log("GM Name = " + g.name);
        foreach (var component in g.GetComponents<Component>())
        {
            Modding.Logger.Log("Component = " + component);
        }
    }
}*/