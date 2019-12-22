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
        
        
        
        public class PlayerInjector : HookInjector
        {
            
            
            
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


    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    public abstract class PlayerHook<Mod> : MonoBehaviour, IHook<Mod, InjectionAllocator<PlayerInjector>> where Mod : IMod
    {
        
        
        
        
        public virtual void LoadHook(IMod mod) { }
        
        
        
        
        public virtual void UnloadHook(IMod mod) { }
    }
}
