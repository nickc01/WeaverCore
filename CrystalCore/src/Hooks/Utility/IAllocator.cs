using Modding;
using System;
using System.Collections;
using CrystalCore.Hooks.Internal;

namespace CrystalCore.Hooks.Utility
{
    
    
    
    public interface IAllocator
    {
        
        
        
        
        
        
        IHookBase Allocate(Type hook, IMod mod);
        
        
        
        
        
        void Deallocate(IHookBase hook, IMod mod);
    }
}
