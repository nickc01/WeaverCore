using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;

namespace WeaverCore.Implementations
{
	public abstract class RegistryLoader_I : IImplementation
    {
        //static bool loaded = false;

        public IEnumerable<Registry> GetRegistries<Mod>() where Mod : WeaverMod
        {
            return GetRegistries(typeof(Mod));
        }

        public abstract IEnumerable<Registry> GetRegistries(Type modType);
    }
}
