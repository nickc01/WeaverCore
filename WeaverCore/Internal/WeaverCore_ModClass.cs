using System;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Internal
{

    /// <summary>
    /// The mod class for WeaverCore
    /// </summary>
    public sealed class WeaverCore_ModClass : WeaverMod
    {
        public WeaverCore_ModClass() : base("WeaverCore") { }
        public override void Initialize()
        {
            base.Initialize();
        }


        public override string GetVersion()
        {
            return "1.0.1.1";
        }
    }
}
