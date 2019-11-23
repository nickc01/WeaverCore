using Modding;
using System;
using System.Collections;
using VoidCore.Hooks.Internal;

namespace VoidCore.Hooks.Utility
{
    
    
    
    public interface IAllocator
    {
        
        
        
        
        
        
        IHookBase Allocate(Type hook, IMod mod);
        
        
        
        
        
        void Deallocate(IHookBase hook, IMod mod);
    }
}
