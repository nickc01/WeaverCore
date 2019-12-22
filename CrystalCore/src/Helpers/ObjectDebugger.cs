using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using CrystalCore.Machine;

namespace CrystalCore.Helpers
{
    public static class ObjectDebugger
    {
        public static void DebugObject(GameObject gameObject,string subDirectory)
        {
            var dir = Application.dataPath + "\\Managed\\Mods";

            if (!Directory.Exists(dir + "\\" + subDirectory))
            {
                Directory.CreateDirectory(dir + "\\" + subDirectory);
            }
            if (!Directory.Exists(dir + "\\" + subDirectory + "\\Debug"))
            {
                Directory.CreateDirectory(dir + "\\" + subDirectory + "\\Debug");
            }
            if (!Directory.Exists(dir + "\\" + subDirectory + "\\Debug\\" + gameObject.name))
            {
                Directory.CreateDirectory(dir + "\\" + subDirectory + "\\Debug\\" + gameObject.name);
                //Directory.Delete(dir + "\\CrystalCore\\Debug\\" + gameObject.name);
            }
            //Directory.CreateDirectory(dir + "\\CrystalCore\\Debug\\" + gameObject.name);

            PrintObjectInfo(gameObject, dir + "\\" + subDirectory + "\\Debug\\" + gameObject.name);
        }

        static StreamWriter OpenOrCreate(string filePath, bool ClearContents = true)
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

        static string PrintEnum(IEnumerable e)
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

        static void DebugFSM(PlayMakerFSM fsm, string dirPath)
        {
            using (StreamWriter writer = OpenOrCreate(dirPath + "\\" + fsm.name + " - " + fsm.FsmName + ".fsm"))
            {
                /*var machine = new Machine("testMachine");
                State idle = new State("Idle");
                State finish = new State("Finish");
                idle.AddEvent(new CrystalCore.Machine.Event("toFinish",finish));
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
                        currentState.AddEvent(new XEvent(transition.FsmEvent.Name, new XState(transition.ToState)));
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

        static void PrintObjectInfo(GameObject g, string dirPath)
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
                    PrintObjectInfo(child.gameObject, dirPath + "\\" + child.gameObject.name);
                }
            }
        }
    }
}
