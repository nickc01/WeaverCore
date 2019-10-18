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
            VoidModLog.Log("LOADING TESTMOD");
            base.Initialize();
        }
    }


    public class Scenes : VoidCore.Hooks.SceneHook
    {
        

        public override void ChangedActiveScene(UnityEngine.SceneManagement.Scene prev, UnityEngine.SceneManagement.Scene now)
        {
            VoidModLog.Log("TEST2");
            VoidModLog.Log("ACTIVE CHANGED");
        }

        public override void SceneAdd(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode loadMode)
        {
            if (scene.name == "Town")
            {
                //GameObject Knight = null;
                VoidModLog.Log("-----------------------TOWN LOADED");
                foreach (var obj in UnityEngine.GameObject.FindObjectsOfType<GameObject>())
                {
                    VoidModLog.Log("OBJ = " + obj);
                }
            }
        }

        public override void SceneRemove(UnityEngine.SceneManagement.Scene scene)
        {
            VoidModLog.Log("TEST");
            VoidModLog.Log("TEST2");
        }
    }

    public class UI : UIManagerHook
    {
        void Start()
        {

            VoidModLog.Log("UISTART");
            VoidModLog.Log(Environment.StackTrace);
            var test = new StackTrace(true);
            foreach (var r in test.GetFrames())
            {
                VoidModLog.Log($"Filename: {r.GetFileName()} Method: {r.GetMethod()} Line: {r.GetFileLineNumber()} Column: {r.GetFileColumnNumber()}  ");
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
                VoidModLog.Log("PLAYER START");
                foreach (var item in gameObject.GetComponents<Component>())
                {
                    VoidModLog.Log("PLAYER COMPONENT = " + item);
                    VoidModLog.Log("COMPONENT TYPE = " + item.GetType().FullName);
                    VoidModLog.Log("FSM = " + (item is PlayMakerFSM));
                    if (item is PlayMakerFSM fsm)
                    {
                        foreach (var state in fsm.FsmEvents)
                        {
                            VoidModLog.Log("State = " + state.Name);
                        }
                        
                        VoidModLog.Log("IS FSM");
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
                VoidModLog.Log("Exception = " + e);
            }
        }
    }

}
