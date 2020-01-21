using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ViridianLink.Helpers;
using ViridianLink.Implementations;
using ViridianLink.Implementations.Allocators;

namespace ViridianLink
{
    public class Player : MonoBehaviour
    {
        public static Player GetPlayer()
        {
            return GetImplementation().GetComponent<Player>();
        }

        static PlayerImplementation GetImplementation()
        {
            var allocator = ImplInfo.GetImplementation<PlayerAllocator>();
            return allocator.Allocate();
        }
    }
}
