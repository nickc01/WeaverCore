using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoidCore.Hooks
{
    public class PlayerAllocator : Allocator
    {
        public override object Allocate(Type originalType)
        {
            PlayerHook.AvailableHooks.Add(originalType);
            return null;
        }
    }



    public abstract class PlayerHook : MonoBehaviour, IHook<PlayerAllocator>
    {
        internal static List<Type> AvailableHooks = new List<Type>();

        void IHook.LoadHook()
        {
            
        }
    }
}
