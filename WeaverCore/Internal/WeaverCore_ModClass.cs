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
            return "1.2.0.2";
        }

        public override List<(string, string)> GetPreloadNames()
        {
            return new List<(string, string)>
            {
                ("GG_Workshop", "GG_Statue_Mage_Knight"),/*,
                ("End_Game_Completion", "credits object")*/
                ("Tutorial_01", "_Enemies/Crawler 1"),
                ("Tutorial_01", "_Props/Health Cocoon")
            };
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects)
        {
            if (preloadedObjects.TryGetValue("GG_Workshop", out var ggSceneDict) && ggSceneDict.TryGetValue("GG_Statue_Mage_Knight", out var mageKnightStatue))
            {
                GG_Internal.SetMageKnightStatue(mageKnightStatue);
            }

            if (preloadedObjects.TryGetValue("Tutorial_01", out var tutorialSceneDict))
            {
                foreach (var pair in tutorialSceneDict)
                {
                    WeaverLog.Log($"DICT CONTENTS = {pair.Key}:{pair.Value}");
                }

                if (tutorialSceneDict.TryGetValue("_Enemies/Crawler 1", out var crawler))
                {
                    var enemyDeathEffects = crawler.GetComponent("EnemyDeathEffects");

                    var journalPrefab = enemyDeathEffects.ReflectGetField("journalUpdateMessagePrefab") as GameObject;

                    Other_Preloads.JournalUpdateMsg = journalPrefab;

                    WeaverLog.Log("JOURNAL PREFAB = " + journalPrefab);

                    GameObject.Destroy(crawler);
                }

                if (tutorialSceneDict.TryGetValue("_Props/Health Cocoon", out var healthCocoonObj))
                {
                    var cocoon = healthCocoonObj.GetComponent<HealthCocoon>();
                    foreach (var fling in cocoon.flingPrefabs)
                    {
                        if (fling.prefab.name == "Health Scuttler")
                        {
                            var scuttlerControl = fling.prefab.GetComponent<ScuttlerControl>();
                            Other_Preloads.HealthCocoonFlashPrefab = scuttlerControl.screenFlash;
                            WeaverLog.Log("FLASH PREFAB = " + scuttlerControl.screenFlash);
                            break;
                        }
                    }

                    GameObject.Destroy(healthCocoonObj);
                }
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
