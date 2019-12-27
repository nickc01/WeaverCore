using Modding;
using System;
using System.Collections;
using ViridianCore.Hooks.Internal;

namespace ViridianCore.Hooks.Utility
{
    
    
    
    public interface IAllocator
    {
        
        
        
        
        
        
        IHookBase Allocate(Type hook, IMod mod);
        
        
        
        
        
        void Deallocate(IHookBase hook, IMod mod);
    }
}
