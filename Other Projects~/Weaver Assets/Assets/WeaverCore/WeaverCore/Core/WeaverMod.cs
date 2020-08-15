using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore
{
    public abstract class WeaverMod : Mod
    {
        //As soon as any WeaverCore mod loads, the init functions will be called
        static WeaverMod()
        {
            if (CoreInfo.LoadState == Enums.RunningState.Game)
            {
                InitRunner.RunInitFunctions();
            }
        }


        bool firstLoad = true;

        public IEnumerable<Registry> Registries
        {
            get
            {
                return Registry.FindModRegistries(GetType());
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
            if (firstLoad)
            {
                firstLoad = false;
                RegistryLoader.LoadAllRegistries(GetType());
            }
            else
            {
                EnableRegistries();
            }
        }

        public void EnableRegistries()
        {

            foreach (var registry in Registries)
            {
                registry.RegistryEnabled = true;
            }
        }

        public void DisableRegistries()
        {
            foreach (var registry in Registries)
            {
                registry.RegistryEnabled = false;
            }
        }
    }

    public abstract class TogglableWeaverMod : WeaverMod, ITogglableMod
    {
        public virtual void Unload()
        {
            DisableRegistries();
        }
    }
}
