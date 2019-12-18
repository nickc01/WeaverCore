using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Harmony;
using HutongGames.PlayMaker;
using UnityEngine;
using VoidCore.Helpers;
using VoidCore.Hooks;
using VoidCore.Machine;

class EnemyTest : EnemyHook<VoidCore.VoidCore>
{
    StreamWriter OpenOrCreate(string filePath,bool ClearContents = true)
    {
        if (File.Exists(filePath))
        {
            if (ClearContents)
            {
                File.WriteAllText(filePath, string.Empty);
            }
            return new StreamWriter(filePath);
        }
        else
        {
            return File.CreateText(filePath);
        }
    }

    string PrintEnum(IEnumerable e)
    {
        string final = "{ ";
        foreach (var item in e)
        {
            if (item is IEnumerable otherE)
            {
                final += PrintEnum(otherE);
            }
            else
            {
                final += item.ToString();
            }
            final += ", ";
        }
        final.Remove(final.Length - 3, 2);
        final += " }";
        return final;
    }

    class testing
    {
        public int test1 = 123;
        public string test2 = "thisTestString";
        public float testFloat = 1.23459f;
    }

    void DebugFSM(PlayMakerFSM fsm, string dirPath)
    {
        using (StreamWriter writer = OpenOrCreate(dirPath + "\\" + fsm.name + " - " + fsm.FsmName + ".fsm"))
        {
            /*var machine = new Machine("testMachine");
            State idle = new State("Idle");
            State finish = new State("Finish");
            idle.AddEvent(new VoidCore.Machine.Event("toFinish",finish));
            machine.InitialState = idle;
            machine.AddState(idle);
            machine.AddState(finish);*/

            /*var machine2 = new XMachine("test",new XState("Idle"))
            {
                new XState("Idle",Actions: new List<string>(){ "idleAction" })
                {
                    new XEvent("toFinish",new XState("Finish"))
                },
                new XState("Finish")
            };*/

            //fsm.Fsm.state
            var machine = new XMachine(fsm.Fsm.Name);

            machine.InitialState = new XState(fsm.Fsm.StartState);
            foreach (var state in fsm.Fsm.States)
            {
                XState currentState = new XState(state.Name);
                foreach (var transition in state.Transitions)
                {
                    currentState.AddEvent(new XEvent(transition.FsmEvent.Name,new XState(transition.ToState)));
                }
                foreach (var action in state.Actions)
                {
                    currentState.AddAction(action.Name + " : " + action.GetType().Name);
                }
                machine.AddState(currentState);
            }

            writer.WriteLine(Json.Serialize(machine));
            //writer.WriteLine(Json.Serialize(machine2));
            //writer.Write(Json.Serialize(new testing()));
        }
    }

    void DebugObject(GameObject g, string dirPath)
    {
        //File.Create(dirPath + "\\" + g.name + ".dat");
        var components = g.GetComponents<Component>();
        for (int i = 0; i < components.GetLength(0); i++)
        {
            var component = components[i];
            if (component is PlayMakerFSM fsm)
            {
                DebugFSM(fsm, dirPath);
            }
            using (StreamWriter stream = OpenOrCreate(dirPath + "\\" + component.name + " - " + component.GetType() + " - " + i + ".dat"))
            {
                var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    var attributes = field.GetCustomAttributes(false);
                    if ((field.IsPublic && !attributes.Any(a => a.GetType() == typeof(HideInInspector))) || (field.IsPrivate && attributes.Any(a => a.GetType() == typeof(SerializeField))))
                    {
                        var preModifier = "public ";
                        if (field.IsPrivate)
                        {
                            preModifier = "private ";
                        }
                        var val = field.GetValue(component);
                        if (val == null)
                        {
                            val = "NULL";
                        }
                        else
                        {
                            try
                            {
                                var output = JsonUtility.ToJson(val, true);
                                if (output.Length <= 120 && output != "{}")
                                {
                                    val = output;
                                }
                            }
                            catch (Exception)
                            {

                            }
                        }
                        if (val.ToString() == field.FieldType.ToString())
                        {
                            try
                            {
                                if (val is IEnumerable collection)
                                {
                                    val = PrintEnum(collection);
                                }
                                else
                                {
                                    XmlSerializer serializer = new XmlSerializer(val.GetType());
                                    using (var memStream = new MemoryStream())
                                    {
                                        serializer.Serialize(memStream, val);
                                        using (var streamReader = new StreamReader(memStream))
                                        {
                                            val = streamReader.ReadToEnd();
                                        }
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                //Modding.Logger.Log($"Failed to Serialize {val.GetType()}, => " + e);
                            }
                        }
                        stream.WriteLine(preModifier + field.FieldType + " " + field.Name + " = " + val + ";");
                    }
                }
            }
        }
        if (g.transform.childCount > 0)
        {
            for (int i = 0; i < g.transform.childCount; i++)
            {
                var child = g.transform.GetChild(i);
                if (!Directory.Exists(dirPath + "\\" + child.gameObject.name))
                {
                    Directory.CreateDirectory(dirPath + "\\" + child.gameObject.name);
                }
                DebugObject(child.gameObject, dirPath + "\\" + child.gameObject.name);
            }
        }
    }

    PlayMakerFSM fsm;

    string activeStateName = "";

    void Start()
    {
        //var dir = new DirectoryInfo(typeof(VoidCore.VoidCore).Assembly.CodeBase);
        var dir = Application.dataPath + "\\Managed\\Mods";

        if (!Directory.Exists(dir + "\\VoidCore"))
        {
            Directory.CreateDirectory(dir + "\\VoidCore");
        }
        if (!Directory.Exists(dir + "\\VoidCore\\Debug"))
        {
            Directory.CreateDirectory(dir + "\\VoidCore\\Debug");
        }
        if (!Directory.Exists(dir + "\\VoidCore\\Debug\\" + gameObject.name))
        {
            Directory.CreateDirectory(dir + "\\VoidCore\\Debug\\" + gameObject.name);
            //Directory.Delete(dir + "\\VoidCore\\Debug\\" + gameObject.name);
        }
        //Directory.CreateDirectory(dir + "\\VoidCore\\Debug\\" + gameObject.name);

        DebugObject(gameObject, dir + "\\VoidCore\\Debug\\" + gameObject.name);

        /*var dir = Directory.CreateDirectory(typeof(VoidCore.VoidCore).Assembly.CodeBase);
        dir.*/
        fsm = GetComponent<PlayMakerFSM>();
        if (fsm != null)
        {
            Modding.Logger.Log("FSM = " + fsm);
            foreach (var e in fsm.FsmEvents)
            {
                Modding.Logger.Log("Event = " + e.Name);
            }
            foreach (var state in fsm.FsmStates)
            {
                Modding.Logger.Log("State = " + state.Name);
                foreach (var trans in state.Transitions)
                {
                    Modding.Logger.Log("Transition From = " + trans.FsmEvent);
                    Modding.Logger.Log("Transition To = " + trans.ToState);
                    Modding.Logger.Log("Transition Event = " + trans.EventName);

                }
                foreach (var action in state.Actions)
                {
                    Modding.Logger.Log("Action = " + action.Name);
                    Modding.Logger.Log("ActionType = " + action.GetType());
                    foreach (var field in action.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        Modding.Logger.Log("Action Field = " + field.Name + " = " + field.GetValue(action));
                    }
                    if (action is HutongGames.PlayMaker.Actions.Wait waiter)
                    {
                        waiter.time.Value = waiter.time.Value * 2f;
                    }
                }
            }
        }
    }

    void Update()
    {
        if (activeStateName != fsm.ActiveStateName)
        {
            activeStateName = fsm.ActiveStateName;
            Modding.Logger.Log("NEW STATE = " + activeStateName);
        }
    }
}

/*namespace VoidCore.Internal
{
    static class ActionLoader
    {

        class EnemyTest : EnemyHook<VoidCore>
        {
            void Start()
            {
                var fsm = GetComponent<PlayMakerFSM>();
                if (fsm != null)
                {
                    Modding.Logger.Log("FSM = " + fsm);
                    foreach (var state in fsm.FsmStates)
                    {
                        Modding.Logger.Log("State = " + state.Name);
                        foreach (var action in state.Actions)
                        {
                            Modding.Logger.Log("Action = " + action.Name);
                            Modding.Logger.Log("ActionType = " + action.GetType());
                            foreach (var field in action.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                            {
                                Modding.Logger.Log("Action Field = " + field.Name + " = " + field.GetValue(action));
                            }
                            if (action is HutongGames.PlayMaker.Actions.Wait waiter)
                            {
                                waiter.time.Value = waiter.time.Value / 2f;
                            }
                        }
                    }
                }
            }
        }



        public static void ModifyActions(List<FsmStateAction> Actions,FsmState state,GameObject obj)
        {
            //Modding.Logger.Log("MODIFYING ACTIONS for state = " + state.Name);
        }

        class Props
        {
            public bool Modified = false;
        }

        static PropertyTable<FsmState, Props> PropertyTable = new PropertyTable<FsmState, Props>();

        public static void ApplyActionMods(FsmState state, ref FsmStateAction[] actions)
        {
            //Modding.Logger.Log("ACTION B");
            var properties = PropertyTable.GetOrCreate(state);
            if (!properties.Modified)
            {
                //Modding.Logger.Log("ACTION E");
                properties.Modified = true;
                List<FsmStateAction> actionList;
                if (actions == null)
                {
                    actionList = new List<FsmStateAction>();
                }
                else
                {
                    actionList = actions.ToList();
                }
                ModifyActions(actionList, state, state.Fsm.GameObject);
                foreach (var action in actionList)
                {
                    action.Init(state);
                }
                actions = actionList.ToArray();
            }
        }

        public static bool IsModified(FsmState state)
        {
            //Modding.Logger.Log("Checking Modification");
            return PropertyTable.GetOrCreate(state).Modified;
        }
    }


    //[HarmonyPatch]
    static class ActionsGetterPatch
    {
        static MethodBase TargetMethod()
        {
            return typeof(FsmState).GetProperty("Actions").GetGetMethod();
        }

        /*static bool Prefix(ref FsmStateAction[] ___actions)
        {
            Modding.Logger.Log("Y");
            //Modding.Logger.Log("ACTION F");
            //Modding.Logger.Log("STATE = " + __state);
            //Modding.Logger.Log("Actionsk = " + (___actions != null));
            __state = (___actions == null);
            //Modding.Logger.Log("PREFIX END 1");
            //Modding.Logger.Log("STACK TRACE = " + new StackTrace(true));
            return true;
            //return ActionsLoaderPatch.Prefix(ref __state, ref ___actions);
        }*/

/*static void Postfix(ref FsmStateAction[] ___actions,FsmState __instance)
        {
            //Modding.Logger.Log("ACTION G");
            //Modding.Logger.Log("STATE = " + __state);
            //Modding.Logger.Log("NOT ON MAIN THREAD = " + PlayMakerFSM.NotMainThread);
            //Modding.Logger.Log("ACTION U");
            //Modding.Logger.Log("Y");
            //Modding.Logger.Log("Modified = " + ActionLoader.IsModified(__instance));
            //Modding.Logger.Log("F");
            if (!ActionLoader.IsModified(__instance) && !PlayMakerFSM.NotMainThread)
            {
                //Modding.Logger.Log("A");
                //Modding.Logger.Log("ACTION Z");
                ActionLoader.ApplyActionMods(__instance, ref ___actions);
            }
            //Modding.Logger.Log("ACTION V");
            //ActionsLoaderPatch.Postfix(ref __state, ref ___actions, __instance);
        }
    }

    //[HarmonyPatch(typeof(FsmState))]
    //[HarmonyPatch("LoadActions")]
    static class ActionsLoaderPatch
    {
        /*public static bool Prefix(ref FsmStateAction[] ___actions)
        {
            Modding.Logger.Log("Z");
            //Modding.Logger.Log("ACTION A");
            __state = ___actions == null;
            return true;
        }*/

 /*       public static void Postfix( ref FsmStateAction[] ___actions, FsmState __instance)
        {
            //Modding.Logger.Log("ACTION C");
            //Modding.Logger.Log("Z");
            if (!ActionLoader.IsModified(__instance) && !PlayMakerFSM.NotMainThread)
            {
                //Modding.Logger.Log("B");
                //Modding.Logger.Log("ACTION D");
                ActionLoader.ApplyActionMods(__instance, ref ___actions);
            }
        }
    }
}*/
