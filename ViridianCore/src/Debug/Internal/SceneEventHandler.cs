using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using ViridianCore.Hooks;
using System.Reflection;
using System.Diagnostics;

namespace ViridianCore
{
    internal class SceneEventHandler : SceneHook<ViridianCore>
    {
        public override void OnActiveSceneChange(Scene prev, Scene now)
        {
            /*try
            {
                foreach (var invo in eInstance.GetInvocationList())
                {
                    Modding.Logger.Log("INVO = " + invo.Method.Name);
                    Modding.Logger.Log(invo.Method.Module.Assembly);
                    Modding.Logger.Log(invo.Target);
                    Modding.Logger.Log(invo.Method.DeclaringType);
                    invo.DynamicInvoke(now, LoadSceneMode.Additive);
                    Modding.Logger.Log("INVO END");
                }
            }
            catch (Exception e)
            {
                Modding.Logger.Log("Exception = " + e);
            }*/
            /*foreach (var invo in e.)
            {

            }*/
            
        }

        public override void OnSceneAddition(Scene scene, LoadSceneMode loadMode)
        {
            Modding.Logger.Log("NEW SCENE ADDED = " + scene.name);
            /*new Thread(() =>
            {
                Modding.Logger.Log("BEGINNING OF THREAD");
                Thread.Sleep(20000);
                Modding.Logger.Log("Loaded GameObjects = " + GameObjectTracker.AllGameObjects.Count);
                int falseObjects = 0;
                foreach (var item in GameObjectTracker.AllGameObjects)
                {
                    if (item == null)
                    {
                        falseObjects++;
                    }
                }
                Modding.Logger.Log("False Objects = " + falseObjects);
                Modding.Logger.Log("Loaded GameObjects2 = " + GameObjectTracker.AllGameObjects.Count);
                falseObjects = 0;
                foreach (var item in GameObjectTracker.AllGameObjects)
                {
                    if (item == null)
                    {
                        falseObjects++;
                    }
                }
                Modding.Logger.Log("False Objects2 = " + falseObjects);
            }).Start();*/
        }

        public override void OnSceneRemoval(Scene scene)
        {
            
        }
    }
}
