using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;
using WeaverCore.Interfaces;
using System.Reflection;
using Modding;

namespace WeaverCore.Implementations
{
	public abstract class RegistryLoader_I : IImplementation
    {
        public abstract void LoadRegistries(Assembly assembly);

        public void LoadRegistries<Mod>() where Mod : IMod
        {
            LoadRegistries(typeof(Mod));
        }

        public void LoadRegistries(Type modType)
        {
            LoadRegistries(modType.Assembly);
        }
    }
}
