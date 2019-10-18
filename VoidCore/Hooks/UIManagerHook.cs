using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace VoidCore.Hooks
{
    public class UIManagerAllocator : Allocator
    {
        public override object Allocate(Type originalType)
        {
            UIManagerHook.AvailableHooks.Add(originalType);
            return null;
        }
    }



    public abstract class UIManagerHook : MonoBehaviour, IHook<UIManagerAllocator>
    {
        internal static List<Type> AvailableHooks = new List<Type>();

        //private AudioManager


        void IHook.LoadHook()
        {

        }
    }
}
