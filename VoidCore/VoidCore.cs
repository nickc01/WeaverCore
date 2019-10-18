using System;
using System.Reflection;
using GlobalEnums;
using Modding;
using Harmony;

namespace VoidCore
{
    internal class VoidCore : Mod
    {
        static internal VoidCore Instance;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;

            var harmony = HarmonyInstance.Create("com.nccore.nickc01");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Hooks.SceneHook.OnGameLoad();
        }
    }

}
