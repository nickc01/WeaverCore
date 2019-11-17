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
using Logger = Modding.Logger;
using System.Collections;
using HutongGames.PlayMaker;

namespace TestMod
{
    public class TestMod : Mod, ITogglableMod
    {
        public override string GetVersion() => "0.0.1";

        public override void Initialize()
        {
            Logger.Log("LOADING TESTMOD");
            base.Initialize();

            //Settings.DebugMode = true;
            //Settings.GMTracking = true;
        }

        public void Unload()
        {
            
        }
    }

    public class UI : UIHook<TestMod>
    {
        void Start()
        {
            Logger.Log("UISTART");
        }

        void OnDestroy()
        {
            Logger.Log("UIEND");
        }

        public override void LoadHook(IMod mod)
        {
            
        }

        public override void UnloadHook(IMod mod)
        {
            
        }
    }


    public class EnemyTester : EnemyHook<VoidCore.VoidCore>
    {
        static void PrintList(string pre, IEnumerable list)
        {
            foreach (var item in list)
            {
                Modding.Logger.Log(pre + item.ToString());
            }
        }
        static void PrintList<T>(string pre, IEnumerable<T> list, Func<T,string> printer)
        {
            foreach (var item in list)
            {
                Modding.Logger.Log(pre + printer(item));
            }
        }

        void Start()
        {
            var fsm = GetComponent<PlayMakerFSM>();
            Modding.Logger.Log("\n\nFSM = " + fsm.FsmName);
            Modding.Logger.Log("TEST OUTPUT = " + JsonUtility.ToJson(fsm, true));
            PrintList("Events = ", fsm.FsmEvents,e => $"Name = {e.Name}, IsApplicationEvent = {e.IsApplicationEvent}, IsGlobal = {e.IsGlobal}, IsMouseEvent {e.IsMouseEvent}, IsSystemEvent = {e.IsSystemEvent}, Path = {e.Path}");
            //Modding.Logger.Log("Events 2 = " + fsm.FsmEvents)
            PrintList("Global Transitions = ", fsm.FsmGlobalTransitions,t => $" EventName = {t.EventName} FSMEvent = {t.FsmEvent} ToState = {t.ToState} LinkStyle = {t.LinkStyle}, LinkConstraint = {t.LinkConstraint}");
            PrintList("Variables = ", fsm.FsmVariables.GetAllNamedVariables(),v => $"IsNone = {v.IsNone}, Name = {v.Name}, ObjectType = {v.ObjectType}, RawValue = {v.RawValue}, TypeConstraint = {v.TypeConstraint}, UsesVariable = {v.UsesVariable}, UseVariable = {v.UseVariable}, VariableType = {v.VariableType}");
            PrintList("States = ", fsm.FsmStates);
            Modding.Logger.Log("Template = " + fsm.FsmTemplate);
            Modding.Logger.Log("Start State = " + fsm.Fsm.StartState);
            /*Modding.Logger.Log("ENEMY = " + gameObject.name);
            foreach (var component in GetComponents<Component>())
            {
                Modding.Logger.Log("Component = " + component);
            }*/
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
