using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using ViridianCore.Machine;
using Newtonsoft.Json;

namespace ViridianCore.Helpers
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
            }
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
            }
        }

        static void PrintObjectInfo(GameObject g, string dirPath,bool includeProperties = false,bool everything = false)
        {
            using (StreamWriter stream = OpenOrCreate(dirPath + "\\" + g.name + ".gmdata"))
            {
                stream.WriteLine("Name = " + g.name);
                stream.WriteLine("Active = " + g.activeSelf);
                stream.WriteLine("Layer = " + g.layer);
                stream.WriteLine("Layer Name = " + LayerMask.LayerToName(g.layer));
                stream.WriteLine("Tag = " + g.tag);
                stream.WriteLine("Scene = " + g.scene.name);
                stream.Close();
            }

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
                    try
                    {
                        //Modding.Logger.Log("COMPONENT = " + component.name);
                        var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        foreach (var field in fields)
                        {
                            var attributes = field.GetCustomAttributes(false);
                            string preModifier = "";
                            if (!((field.IsPublic && !attributes.Any(a => a.GetType() == typeof(HideInInspector))) || (field.IsPrivate && attributes.Any(a => a.GetType() == typeof(SerializeField)))))
                            {
                                preModifier += "_internal_";
                                if (!everything)
                                {
                                    break;
                                }
                            }
                            if (field.IsPrivate)
                            {
                                preModifier += " private ";
                            }
                            else
                            {
                                preModifier += " public ";
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
                                catch (Exception)
                                {

                                }
                            }
                            stream.WriteLine(preModifier + field.FieldType + " " + field.Name + " = " + val + ";");
                        }
                        //Modding.Logger.Log("A2");
                        if (includeProperties)
                        {
                            var props = component.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            foreach (var property in props)
                            {
                                //Modding.Logger.Log("A1");
                                var attributes = property.GetCustomAttributes(false);
                                //Modding.Logger.Log("Z1");
                                string preModifier = "prop ";
                                MethodInfo propMethod = property.GetGetMethod();
                                if (propMethod == null)
                                {
                                    propMethod = property.GetSetMethod();
                                }
                                //Modding.Logger.Log("PropMethod = " + propMethod);
                                //Modding.Logger.Log("Attributes = " + attributes);
                                if (!((propMethod != null && propMethod.IsPublic && !attributes.Any(a => a.GetType() == typeof(HideInInspector))) || (propMethod != null && propMethod.IsPrivate && attributes.Any(a => a.GetType() == typeof(SerializeField)))))
                                {
                                    preModifier += "_internal_";
                                    if (!everything)
                                    {
                                        break;
                                    }
                                }
                                //Modding.Logger.Log("Z2");
                                //if ((field.IsPublic && !attributes.Any(a => a.GetType() == typeof(HideInInspector))) || (field.IsPrivate && attributes.Any(a => a.GetType() == typeof(SerializeField))))
                                //{
                                if (propMethod != null && !propMethod.IsPublic)
                                {
                                    preModifier += " private ";
                                }
                                else
                                {
                                    preModifier += " public ";
                                }
                                //Modding.Logger.Log("Z3");
                                //Modding.Logger.Log("A");
                                var val = property.GetValue(component, null);
                                //Modding.Logger.Log("B");
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
                                if (val.ToString() == property.PropertyType.ToString())
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
                                    catch (Exception)
                                    {

                                    }
                                }
                                //Modding.Logger.Log("C");
                                stream.WriteLine(preModifier + property.PropertyType + " " + property.Name + " = " + val + ";");
                            }
                        }
                    }
                    catch (Exception)
                    {

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
