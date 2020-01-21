using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViridianLink.Implementations;
using ViridianLink.Implementations.Allocators;

namespace ViridianLink.Game.Implementations.Allocators
{
    public class GamePlayerAllocator : PlayerAllocator
    {
        public override PlayerImplementation Allocate()
        {
            if (HeroController.instance != null)
            {
                var impl = HeroController.instance.GetComponent<GamePlayerImplementation>();
                if (impl == null)
                {
                    impl = HeroController.instance.gameObject.AddComponent<GamePlayerImplementation>();
                    HeroController.instance.gameObject.AddComponent<Player>();
                }
                return impl;
            }
            else
            {
                throw new Exception("There is currently no player in the game right now");
            }
        }
    }
}
