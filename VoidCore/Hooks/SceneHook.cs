using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoidCore.Hooks.Utility;

namespace VoidCore.Hooks
{
    /// <summary>
    /// Hooks into the Scene Loading Process.
    /// </summary>
    /// <example>
    /// <code>
    ///public class ExampleHook : SceneHook
    ///{
    ///    public override void OnActiveSceneChange(Scene prev, Scene now)
    ///    {
    ///        //Called when the active scene changes
    ///    }
    ///
    ///    public override void OnSceneAddition(Scene scene, LoadSceneMode loadMode)
    ///    {
    ///        //Called when a new scene is loaded into the game
    ///    }
    ///
    ///    public override void OnSceneRemoval(Scene scene)
    ///    {
    ///        //Called when a scene is unloaded from the game
    ///    }
    ///}
    ///</code>
    ///</example>
    public abstract class SceneHook : IHook
    {
        internal static List<Scene> Scenes = new List<Scene>();
        private static Scene activeScene;

        /// <summary>
        /// All currently loaded scenes in the game
        /// </summary>
        public static IEnumerable<Scene> LoadedScenes => Scenes;
        /// <summary>
        /// The currently active scene. Any new GameObjects that are created are put into this scene
        /// </summary>
        public static Scene ActiveScene => activeScene;

        void IHook.LoadHook()
        {

            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneAddition;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneRemoval;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChange;
        }

        //Called when the game initially loads up and is called only once
        internal static void OnGameLoad()
        {
            //Add current scenes
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                Scenes.Add(scene);
            }
            activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += AddScene;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += RemoveScene;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += ChangeActiveScene;
        }

        internal static void AddScene(Scene scene,LoadSceneMode mode)
        {
            ModLog.Log("Added Scene = " + scene.name);
            Scenes.Add(scene);
        }

        internal static void RemoveScene(Scene scene)
        {
            ModLog.Log("Removed Scene = " + scene.name);
            Scenes.Remove(scene);
        }

        internal static void ChangeActiveScene(Scene prev, Scene now)
        {
            ModLog.Log("Active Scene = " + now.name);
            activeScene = now;
        }


        /// <summary>
        /// Called when a new scene is loaded into the game
        /// </summary>
        /// <param name="scene">The newly loaded scene</param>
        /// <param name="loadMode">How the scene was loaded into the game <seealso cref="LoadSceneMode"/></param>
        public abstract void OnSceneAddition(Scene scene, LoadSceneMode loadMode);

        /// <summary>
        /// Called when a scene is unloaded from the game
        /// </summary>
        /// <param name="scene">The scene to be unloaded</param>
        public abstract void OnSceneRemoval(Scene scene);

        /// <summary>
        /// Called when the active scene of the game changes
        /// </summary>
        /// <param name="prev">The scene that was previously the active one</param>
        /// <param name="now">The scene that is now the active one</param>
        public abstract void OnActiveSceneChange(Scene prev, Scene now);
    }
}
