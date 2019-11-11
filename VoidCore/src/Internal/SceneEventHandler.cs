using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoidCore.Hooks;

namespace VoidCore
{
    internal class SceneEventHandler : SceneHook<VoidCore>
    {
        public override void OnActiveSceneChange(Scene prev, Scene now)
        {

        }

        public override void OnSceneAddition(Scene scene, LoadSceneMode loadMode)
        {
            Modding.Logger.Log("NEW SCENE ADDED = " + scene.name);
            new Thread(() =>
            {
                Modding.Logger.Log("BEGINNING OF THREAD");
                Thread.Sleep(10000);
                Modding.Logger.Log("Loaded GameObjects = " + GameObjectTracker.AllGameObjects.Count);
            }).Start();
        }

        public override void OnSceneRemoval(Scene scene)
        {

        }
    }
}
