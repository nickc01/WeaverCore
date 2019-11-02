using System;

namespace VoidCore.Hooks
{
    namespace Utility
    {
        public abstract class Allocator
        {
            public abstract object Allocate(Type originalType);
        }
    }
}
