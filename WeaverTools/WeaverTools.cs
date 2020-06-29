using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WeaverTools.Patches;
using UnityEngine.SceneManagement;
using UnityEngine;
using WeaverCore.Utilities;
using System.Collections;
using WeaverCore.Interfaces;
using System.Diagnostics;
using WeaverCore;

namespace WeaverTools
{
    public sealed class WeaverToolsMod : Mod
    {
        [Serializable]
        struct IDHolder<T> where T : UnityEngine.Object
        {
            public IDHolder(T obj)
            {
                OBJ = obj;
            }

            public T OBJ;

            public static IDHolder<T> Transform(long fileID, long pathID)
            {
                string json = $"{{\"OBJ\":{{ \"m_FileID\":{fileID},\"m_PathID\":{pathID}}}}}";
                //WeaverLog.Log("Json = " + json);
                return JsonUtility.FromJson<IDHolder<T>>(json);
            }
        }


        public WeaverToolsMod(string name) : base("Weaver Tools")
        {
        }

        public WeaverToolsMod() : base("Weaver Tools")
        {
        }

        public override void Initialize()
        {
            /*HutongGames.PlayMaker.FsmLog.LoggingEnabled = true;
            HutongGames.PlayMaker.FsmLog.EnableDebugFlow = true;
           // HutongGames.PlayMaker.FsmLog.
            On.HealthManager.Start += ObjectPrinterPatch.HealthManager_Start;
            On.HeroController.Start += ObjectPrinterPatch.HeroController_Start;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += ObjectPrinterPatch.SceneManager_sceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoad;

            var harmonyInstance = HarmonyPatcher.Create($"com.{nameof(WeaverTools).ToLower()}.nickc01");
            */

            //DebugPatches.ApplyPatches(harmonyInstance);
            //On.HealthManager.Awake += ObjectPrinterPatch.HealthManager_Awake;

            //ToolSettings.SetSettings(new ToolSettings());
            //var GameObjectDir = WeaverCore.WeaverDirectory.GetWeaverDirectory().CreateSubdirectory("Tools").CreateSubdirectory("Debug").CreateSubdirectory("GameObjects");
            //GameObjectDir.Create();

            //URoutine.Start(PrintObjectIDs());


        }

        private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
        {
            //WeaverRoutine.Start(PrintObjectIDs());
        }

        static IEnumerator<IWeaverAwaiter> PrintObjectIDs()
        {
            WeaverLog.Log("-------------Printing IDS!!!");
            yield return null;
            ToolSettings settings = ToolSettings.GetSettings();
            if (settings == null)
            {
                yield break;
            }

            WeaverLog.Log("A");
            if (settings.PrintIDs)
            {
                WeaverLog.Log("B");

                settings.IDsToPrint.Add(new IDPair()
                {
                    FileID = 2,
                    PathID = 51,
                    Type = "UnityEngine.GameObject"
                });

                foreach (var ids in settings.IDsToPrint)
                {
                    WeaverLog.Log("C");
                    Type type = null;

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(ids.Type);
                        if (type != null)
                        {
                            break;
                        }
                    }
                    WeaverLog.Log("D");

                    if (type == null)
                    {
                        type = typeof(UnityEngine.Object);
                    }

                    WeaverLog.Log("Found Type = " + type.FullName);

                    var holder = typeof(IDHolder<>).MakeGenericType(type).GetMethod("Transform").Invoke(null, new object[] {ids.FileID, ids.PathID });

                    WeaverLog.Log("Holder = " + holder.GetType().FullName);


                    var obj = (UnityEngine.Object)holder.GetType().GetField("OBJ").GetValue(holder);

                    if (obj == null)
                    {
                        WeaverLog.Log($"Object of File ID {ids.FileID} and Path ID {ids.PathID} is null");
                    }
                    else
                    {
                        WeaverLog.Log($"---Stats of Object of File ID {ids.FileID} and Path ID {ids.PathID}:");
                        WeaverLog.Log("Name = " + obj.name);
                        WeaverLog.Log("Type = " + obj.GetType());

                        if (obj is GameObject gm)
                        {
                            ObjectDebugger.DebugObject(gm);
                        }
                    }
                }

                foreach (var name in settings.NamesToPrint)
                {
                    var gm = GameObject.Find(name);
                    WeaverLog.Log($"Found GameObject for name: {name} = {gm}");
                    if (gm != null)
                    {
                        ObjectDebugger.DebugObject(gm);
                    }
                }
            }

        }

        public override string GetVersion()
        {
            return "1.0.0.0";
        }
    }
}
