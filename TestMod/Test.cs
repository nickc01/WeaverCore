using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using VoidCore;
using VoidCore.Hooks;
using UnityEngine;
using System.Diagnostics;
using Modding;

namespace TestMod
{
    public class TestMod : Mod, ITogglableMod
    {
        public override string GetVersion() => "0.0.1";

        public override void Initialize()
        {
            VoidCore.ModLog.Log("LOADING TESTMOD");
            base.Initialize();

            VoidCore.Settings.DebugMode = true;
            VoidCore.Settings.GameObjectTracking = true;
        }

        public void Unload()
        {
            
        }
    }


    /*public class Scenes : VoidCore.Hooks.SceneHook<TestMod>
    {
        

        public override void OnActiveSceneChange(UnityEngine.SceneManagement.Scene prev, UnityEngine.SceneManagement.Scene now)
        {
            ModLog.Log("TEST2");
            ModLog.Log("ACTIVE CHANGED");
        }

        public override void OnSceneAddition(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadMode)
        {
            if (scene.name == "Town")
            {
                //GameObject Knight = null;
                ModLog.Log("-----------------------TOWN LOADED");
                foreach (var obj in UnityEngine.GameObject.FindObjectsOfType<GameObject>())
                {
                    ModLog.Log("OBJ = " + obj);
                }
            }
        }

        public override void OnSceneRemoval(UnityEngine.SceneManagement.Scene scene)
        {
            ModLog.Log("TEST");
            ModLog.Log("TEST2");
        }
    }*/

    public class UI : UIHook<TestMod>
    {
        void Start()
        {
            ModLog.Log("UISTART");
        }

        void OnDestroy()
        {
            ModLog.Log("UIEND");
        }

        public override void LoadHook(IMod mod)
        {
            
        }

        public override void UnloadHook(IMod mod)
        {
            
        }
    }



    /*public class Player : PlayerHook<TestMod>
    {


        private void Start()
        {
            try
            {
                
                ModLog.Log("PLAYER START");
                foreach (var item in gameObject.GetComponents<Component>())
                {
                    ModLog.Log("PLAYER COMPONENT = " + item);
                    ModLog.Log("COMPONENT TYPE = " + item.GetType().FullName);
                    ModLog.Log("FSM = " + (item is PlayMakerFSM));
                    if (item is PlayMakerFSM fsm)
                    {
                        foreach (var state in fsm.FsmEvents)
                        {
                            ModLog.Log("State = " + state.Name);
                        }
                        
                        ModLog.Log("IS FSM");
                        //HutongGames.PlayMaker.Actions.string
                        //XmlSerializer xml = new XmlSerializer(typeof(PlayMakerFSM));

                        /*using (var textWriter = new StringWriter())
                        {
                            xml.Serialize(textWriter, item);
                            ModLog.Log("SERIALIZATION = ");
                            ModLog.Log(textWriter.ToString());
                        }
                        ModLog.Log("TEST");
                    }
                }
            }
            catch (Exception e)
            {
                ModLog.Log("Exception = " + e);
            }
        }
    }*/

}
