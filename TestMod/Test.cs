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

namespace TestMod
{
    public class TestMod : Mod
    {
        public override string GetVersion() => "0.0.1";

        public override void Initialize()
        {
            VoidCore.ModLog.Log("LOADING TESTMOD");
            base.Initialize();
        }
    }


    public class Scenes : VoidCore.Hooks.SceneHook
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
    }

    public class UI : UIHook
    {
        void Start()
        {

            ModLog.Log("UISTART");
            ModLog.Log(Environment.StackTrace);
            var test = new StackTrace(true);
            foreach (var r in test.GetFrames())
            {
                ModLog.Log($"Filename: {r.GetFileName()} Method: {r.GetMethod()} Line: {r.GetFileLineNumber()} Column: {r.GetFileColumnNumber()}  ");
            }
            /*UIManager Manager = GetComponent<UIManager>();
            foreach (var obj in GameObject.FindObjectsOfType<GameObject>())
            {
                VoidModLog.Log("OBJ = " + obj.name);
                foreach (var com in obj.GetComponents<Component>())
                {
                    VoidModLog.Log("COM = " + com);
                    VoidModLog.Log("COM NAME = " + com.name);
                }
            }*/
        }
    }



    public class Player : PlayerHook
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
                        ModLog.Log("TEST");*/
                    }
                }
            }
            catch (Exception e)
            {
                ModLog.Log("Exception = " + e);
            }
        }
    }

}
