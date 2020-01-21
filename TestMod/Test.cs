using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ViridianCore;
using ViridianCore.Hooks;
using UnityEngine;
using System.Diagnostics;
using Modding;
using Logger = Modding.Logger;
using System.Collections;
using System.Reflection;
using HutongGames.PlayMaker;
using ViridianCore.Helpers;

namespace TestMod
{
    public class TestMod : Mod, ITogglableMod
    {
        public override string GetVersion() => "0.0.1";

        public override void Initialize()
        {
            base.Initialize();

            Settings.DebugMode = true;
            Settings.GMTracking = true;
        }

        public void Unload()
        {
            
        }
    }

    public class PlayerTest : PlayerHook<TestMod>
    {
        void Start()
        {
            Modding.Logger.Log("Gravity = " + Physics2D.gravity);
        }
    }

    /*public struct LayerData
    {
        public string[] NameData;

        public bool[,] CollisionData;

        public LayerData(string[] nameData,bool[,] collisionData)
        {
            NameData = nameData;
            CollisionData = collisionData;
        }
    }*/

    /*public class UI : UIHook<TestMod>
    {
        void Start()
        {
            string[] NameData = new string[32];
            for (int i = 0; i < 32; i++)
            {
                try
                {
                    //ameData.Add(i, LayerMask.LayerToName(i));
                    NameData[i] = LayerMask.LayerToName(i);
                    Modding.Logger.Log("LAYER NAME = " + LayerMask.LayerToName(i));
                }
                catch(Exception)
                {
                    NameData[i] = "";
                    Modding.Logger.Log("Layer Name = NULL");
                }
            }
            bool[,] collisionData = new bool[32, 32];
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    bool isColliding = Physics2D.GetIgnoreLayerCollision(i, j);
                    string layer1 = LayerMask.LayerToName(i);
                    string layer2 = LayerMask.LayerToName(j);
                    collisionData[i, j] = isColliding;
                    if (isColliding)
                    {
                        Modding.Logger.Log($"{layer1} and {layer2} COllide : true");
                    }
                    else
                    {
                        Modding.Logger.Log($"{layer1} and {layer2} NOllide : false");
                    }
                }
            }
            var result = Json.Serialize(new LayerData(NameData, collisionData));
            Modding.Logger.Log("JSON RESULT = " + result);
            Logger.Log("UISTART");
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Modding.Logger.Log("LOADED ASSEMBLY = " + assembly);
                if (assembly.FullName.Contains("ViridianLink"))
                {
                    var implType = assembly.GetType("ViridianLink.Implementation");
                    Logger.Log("ImplType = " + implType);
                    var method = implType.GetMethod("GetState", BindingFlags.NonPublic | BindingFlags.Static);
                    Logger.Log("Method = " + method);
                    var enumVal = method.Invoke(null, null);
                    Logger.Log("Enum Val = " + enumVal);
                    Modding.Logger.Log("Current State = " + Enum.GetName(enumVal.GetType(), enumVal));
                }
            }
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
    }*/


    /*public class EnemyTester : EnemyHook<ViridianCore.ViridianCore>
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
            
        }
    }*/



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
