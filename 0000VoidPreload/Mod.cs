using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preloader
{
    internal class _0000PreloadMod : Mod
    {
        public override void Initialize()
        {
            Log("LOADING PRELOADER");
            base.Initialize();
        }
        public override string GetVersion()
        {
            return "0.0.2";
        }
        public override int LoadPriority()
        {
            return -1000;
        }
    }


    [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class PreloaderAttribute : Attribute
    {

    }

}

