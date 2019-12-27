using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViridianCore.Helpers
{
    /// <summary>
    /// Similar to an event, but can be called internally from the assembly, rather than being limited to just the class it is defined in
    /// </summary>
    /// <typeparam name="FuncT">The </typeparam>
    public struct InternalEvent<FuncT> where FuncT : MulticastDelegate
    {
        internal FuncT Invoker { get; private set; }

        public static InternalEvent<FuncT> operator +(InternalEvent<FuncT> self, FuncT function)
        {
            self.Invoker = (FuncT)Delegate.Combine(self.Invoker, function);
            return self;
        }

        public static InternalEvent<FuncT> operator -(InternalEvent<FuncT> self, FuncT function)
        {
            self.Invoker = (FuncT)Delegate.Remove(self.Invoker, function);
            return self;
        }

        internal void Clear()
        {
            var invoList = Invoker.GetInvocationList();
            for (int i = 0; i < invoList.GetLength(0); i++)
            {
                Invoker = (FuncT)Delegate.Remove(Invoker, invoList[i]);
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is InternalEvent<FuncT> func)
            {
                return Invoker == func.Invoker;
            }
            return false;
        }

        public override int GetHashCode()
        {
            if (Invoker != null)
            {
                return Invoker.GetHashCode();
            }
            return 0;
        }

        public static bool operator ==(InternalEvent<FuncT> left, InternalEvent<FuncT> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InternalEvent<FuncT> left, InternalEvent<FuncT> right)
        {
            return !(left == right);
        }
    }
}
