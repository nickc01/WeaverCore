using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;

using UnityEngine;
using UnityEngine.SceneManagement;



namespace VoidCore.Hooks
{
    public abstract class SceneHook : HookBase
    {
        internal static List<Scene> Scenes = new List<Scene>();
        internal static Scene activeScene;

        public static IEnumerable<Scene> LoadedScenes => Scenes;
        public static Scene ActiveScene => activeScene;


        public override void LoadHook()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneAdd;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += SceneRemove;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ChangedActiveScene;
        }

        internal static void OnGameLoad()
        {
            //Add current scenes
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                Scenes.Add(scene);
            }
            activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneAdd;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneRemove;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ActiveSceneChanged;
        }

        internal static void OnSceneAdd(Scene scene,LoadSceneMode mode)
        {
            VoidModLog.Log("Added Scene = " + scene.name);
            Scenes.Add(scene);
        }

        internal static void OnSceneRemove(Scene scene)
        {
            VoidModLog.Log("Removed Scene = " + scene.name);
            Scenes.Remove(scene);
        }

        internal static void ActiveSceneChanged(Scene prev, Scene now)
        {
            VoidModLog.Log("Active Scene = " + now.name);
            activeScene = now;
        }



        public abstract void SceneAdd(Scene scene, LoadSceneMode loadMode);
        public abstract void SceneRemove(Scene scene);
        public abstract void ChangedActiveScene(Scene prev, Scene now);


    }
}
