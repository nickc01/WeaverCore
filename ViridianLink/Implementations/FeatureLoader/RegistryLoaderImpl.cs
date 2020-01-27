using System;
using ViridianLink.Core;

namespace ViridianLink.Implementations
{
    public abstract class RegistryLoaderImplementation : IImplementation
    {

        public Registry GetRegistry<Mod>() where Mod : IViridianMod
        {
            return GetRegistry(typeof(Mod));
        }

        public abstract Registry GetRegistry(Type modType);
    }
}
