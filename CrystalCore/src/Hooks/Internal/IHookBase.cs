using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrystalCore.Hooks.Utility;

namespace CrystalCore.Hooks.Internal
{
    
    
    
    public interface IHookBase
    {
        
        
        
        
        void LoadHook(IMod mod);
        
        
        
        
        
        
        
        void UnloadHook(IMod mod);
    }
}
