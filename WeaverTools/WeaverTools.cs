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

using Debugger = WeaverCore.Debugger;

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
                Debugger.Log("Json = " + json);
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
            HutongGames.PlayMaker.FsmLog.LoggingEnabled = true;
            HutongGames.PlayMaker.FsmLog.EnableDebugFlow = true;
           // HutongGames.PlayMaker.FsmLog.
            On.HealthManager.Start += ObjectPrinterPatch.HealthManager_Start;
            On.HeroController.Start += ObjectPrinterPatch.HeroController_Start;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += ObjectPrinterPatch.SceneManager_sceneLoaded;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoad;

            var harmonyInstance = Harmony.Create($"com.{nameof(WeaverTools).ToLower()}.nickc01");


            DebugPatches.ApplyPatches(harmonyInstance);
            //On.HealthManager.Awake += ObjectPrinterPatch.HealthManager_Awake;

            //ToolSettings.SetSettings(new ToolSettings());
            //var GameObjectDir = WeaverCore.WeaverDirectory.GetWeaverDirectory().CreateSubdirectory("Tools").CreateSubdirectory("Debug").CreateSubdirectory("GameObjects");
            //GameObjectDir.Create();

            //URoutine.Start(PrintObjectIDs());


        }

        private void OnSceneLoad(Scene arg0, LoadSceneMode arg1)
        {
            URoutine.Start(PrintObjectIDs());
        }

        static IEnumerator<IUAwaiter> PrintObjectIDs()
        {
            yield return null;
            ToolSettings settings = ToolSettings.GetSettings();
            if (settings == null)
            {
                yield break;
            }

            if (settings.PrintIDs)
            {
                foreach (var ids in settings.IDsToPrint)
                {
                    Type type = null;

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        type = assembly.GetType(ids.Type);
                        if (type != null)
                        {
                            break;
                        }
                    }

                    if (type == null)
                    {
                        type = typeof(UnityEngine.Object);
                    }

                    Debugger.Log("Found Type = " + type.FullName);

                    var holder = typeof(IDHolder<>).MakeGenericType(type).GetMethod("Transform").Invoke(null, new object[] {ids.FileID, ids.PathID });

                    Debugger.Log("Holder = " + holder.GetType().FullName);


                    var obj = (UnityEngine.Object)holder.GetType().GetField("OBJ").GetValue(holder);

                    if (obj == null)
                    {
                        Debugger.Log($"Object of File ID {ids.FileID} and Path ID {ids.PathID} is null");
                    }
                    else
                    {
                        Debugger.Log($"---Stats of Object of File ID {ids.FileID} and Path ID {ids.PathID}:");
                        Debugger.Log("Name = " + obj.name);
                        Debugger.Log("Type = " + obj.GetType());

                        if (obj is GameObject gm)
                        {
                            ObjectDebugger.DebugObject(gm);
                        }
                    }
                }

                foreach (var name in settings.NamesToPrint)
                {
                    var gm = GameObject.Find(name);
                    Debugger.Log($"Found GameObject for name: {name} = {gm}");
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
