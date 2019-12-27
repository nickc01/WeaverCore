using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViridianCore.Hooks.Utility;

namespace ViridianCore.Hooks.Internal
{
    
    
    
    public interface IHookBase
    {
        
        
        
        
        void LoadHook(IMod mod);
        
        
        
        
        
        
        
        void UnloadHook(IMod mod);
    }
}
