using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using WeaverCore.Assets;
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
            return "1.2.0.1";
        }

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Workshop", "GG_Statue_Mage_Knight")/*,
                ("End_Game_Completion", "credits object")*/
            };
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            if (preloadedObjects.TryGetValue("GG_Workshop", out var ggSceneDict) && ggSceneDict.TryGetValue("GG_Statue_Mage_Knight", out var mageKnightStatue))
            {
                GG_Internal.SetMageKnightStatue(mageKnightStatue);
            }

            /*if (preloadedObjects.TryGetValue("End_Game_Completion", out var endSceneDict) && endSceneDict.TryGetValue("credits object", out var creditsObject))
            {
                var completionTitle = creditsObject.transform.Find("game completion").Find("game completion title");

                var tmp = completionTitle.GetComponent<TextMeshPro>();
                var mainFont = tmp.font;

                var otherFonts = GameObject.FindObjectsOfType<TMP_FontAsset>();


                if (FontAssetContainer.InGameFonts == null)
                {
                    FontAssetContainer.InGameFonts = new HashSet<TMP_FontAsset>();
                }

                AddFont(mainFont);

                //WeaverLog.Log("CURRENT SCENE = " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().path);

                //UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneManager_sceneLoaded;

                foreach (var tmpComponent in GameObject.FindObjectsOfType<TextMeshPro>())
                {
                    AddFont(tmpComponent.font);
                }

                foreach (var font in FontAssetContainer.InGameFonts)
                {
                    WeaverLog.Log("FOUND FONT = " + font.name);
                }

                var assetContainer = FontAssetContainer.Load();
                assetContainer.ReplaceFonts();
            }*/

            base.Initialize(preloadedObjects);
        }

        /*private void SceneManager_sceneLoaded(UnityEngine.SceneManagement.Scene arg0, UnityEngine.SceneManagement.LoadSceneMode arg1)
        {
            WeaverLog.Log("NEW SCENE = " + arg0.path);

            if (arg0.path.Contains("Menu_Title"))
            {
                UnityEngine.SceneManagement.SceneManager.sceneLoaded -= SceneManager_sceneLoaded;

                foreach (var gm in arg0.GetRootGameObjects())
                {
                    CheckGMForFonts(gm);
                }

                foreach (var font in FontAssetContainer.InGameFonts)
                {
                    WeaverLog.Log("FOUND FONT = " + font.name);
                }

                var assetContainer = FontAssetContainer.Load();
                assetContainer.ReplaceFonts();
            }
        }*/
    }
}
