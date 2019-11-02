using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoidCore.Hooks;

namespace VoidCore
{
    internal class SceneEventHandler : SceneHook
    {
        public override void OnActiveSceneChange(Scene prev, Scene now)
        {

        }

        public override void OnSceneAddition(Scene scene, LoadSceneMode loadMode)
        {
            /*foreach (var root in scene.GetRootGameObjects())
            {
                Events.InternalGameObjectCreated(root);
            }*/
        }

        public override void OnSceneRemoval(Scene scene)
        {

        }
    }
}
