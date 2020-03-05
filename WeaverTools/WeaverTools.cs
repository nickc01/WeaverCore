using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverTools.Patches;
using UnityEngine.SceneManagement;
using UnityEngine;
using WeaverCore.Helpers;

namespace WeaverTools
{
    public sealed class WeaverToolsMod : Mod
    {


        public WeaverToolsMod(string name) : base("Weaver Tools")
        {
        }

        public WeaverToolsMod() : base("Weaver Tools")
        {
        }

        public override void Initialize()
        {
            HutongGames.PlayMaker.FsmLog.LoggingEnabled = true;
            HutongGames.PlayMaker.FsmLog.EnableDebugFlow = true;
           // HutongGames.PlayMaker.FsmLog.
            On.HealthManager.Start += ObjectPrinterPatch.HealthManager_Start;
            On.HeroController.Start += ObjectPrinterPatch.HeroController_Start;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += ObjectPrinterPatch.SceneManager_sceneLoaded;

            var harmonyInstance = Harmony.Create($"com.{(nameof(WeaverTools).ToLower())}.nickc01");


            DebugPatches.ApplyPatches(harmonyInstance);
            //On.HealthManager.Awake += ObjectPrinterPatch.HealthManager_Awake;

            //ToolSettings.SetSettings(new ToolSettings());
            //var GameObjectDir = WeaverCore.WeaverDirectory.GetWeaverDirectory().CreateSubdirectory("Tools").CreateSubdirectory("Debug").CreateSubdirectory("GameObjects");
            //GameObjectDir.Create();




        }

        public override string GetVersion()
        {
            return "1.0.0.0";
        }
    }
}
