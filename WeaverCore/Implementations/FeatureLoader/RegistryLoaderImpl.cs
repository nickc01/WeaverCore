using System;

namespace WeaverCore.Implementations
{
	public abstract class RegistryLoaderImplementation : IImplementation
    {

        public Registry GetRegistry<Mod>() where Mod : IWeaverMod
        {
            return GetRegistry(typeof(Mod));
        }

        public abstract Registry GetRegistry(Type modType);
    }
}
