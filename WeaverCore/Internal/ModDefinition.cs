using System;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Internal
{
    /// <summary>
    /// The mod class for WeaverCore
    /// </summary>
    public sealed class WeaverCore : WeaverMod
    {
        public override void Initialize()
        {
            base.Initialize();
            try
            {
                var shader = WeaverAssets.LoadWeaverAsset<Shader>("SpriteFlash");
                WeaverLog.Log("Shader = " + shader);
            }
            catch (Exception e)
            {
                WeaverLog.LogError("Weaver Init Error => " + e);
            }
        }


        public override string GetVersion()
        {
            return "0.1.0.0 Beta";
        }
    }
}
