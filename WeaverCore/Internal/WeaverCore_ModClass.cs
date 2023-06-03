using System;
using System.Collections.Generic;
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
        /*public override void Initialize()
        {
            base.Initialize();
        }*/


        public override string GetVersion()
        {
            return "1.0.1.3";
        }

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Workshop", "GG_Statue_Mage_Knight")
            };
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            if (preloadedObjects.TryGetValue("GG_Workshop",out var sceneDict) && sceneDict.TryGetValue("GG_Statue_Mage_Knight", out var mageKnightStatue))
            {
                GG_Internal.SetMageKnightStatue(mageKnightStatue);
            }

            base.Initialize(preloadedObjects);
        }
    }
}
