using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WeaverCore.Utilities;

namespace WeaverCore
{
    public abstract class WeaverMod : Mod
    {
        List<Registry> registries = null;


        public IEnumerable<Registry> Registries
        {
            get
            {
                return registries;
            }
        }

        static string Prettify(string input)
        {
            return Regex.Replace(input, @"(\S)([A-Z])", @"$1 $2");
        }

        public override string GetVersion()
        {
            return GetType().Assembly.GetName().Version.ToString();
        }

        public override void Initialize()
        {
            base.Initialize();
            if (registries == null)
            {
                registries = RegistryLoader.GetModRegistries(GetType()).ToList();
            }
            EnableRegistries();
        }

        public void EnableRegistries()
        {
            foreach (var registry in registries)
            {
                registry.RegistryEnabled = true;
            }
        }

        public void DisableRegistries()
        {
            foreach (var registry in registries)
            {
                registry.RegistryEnabled = false;
            }
        }
    }

    public abstract class TogglableWeaverMod : WeaverMod, ITogglableMod
    {
        public void Unload()
        {
            DisableRegistries();
        }
    }
}
