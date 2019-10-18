using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoidCore.Hooks
{
    public abstract class Allocator
    {
        public abstract object Allocate(Type originalType);
    }


    public interface IHook
    {
        void LoadHook();
    }

    public interface IHook<Alloc>: IHook where Alloc : Allocator
    {
        
    }


    public abstract class HookBase : IHook
    {
        //internal abstract void LoadHook();
        //public abstract void LoadHook();
        public abstract void LoadHook();
    }
}
