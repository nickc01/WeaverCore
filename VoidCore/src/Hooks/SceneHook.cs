using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.SceneManagement;
using VoidCore.Hooks.Internal;
using Modding;
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
    public abstract class SceneHook<Mod> : IHook<Mod> where Mod : IMod
    {


        void IHookBase.LoadHook(IMod mod)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneAddition;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneRemoval;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += OnActiveSceneChange;
        }

        void IHookBase.UnloadHook(IMod mod)
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneAddition;
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneRemoval;
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged -= OnActiveSceneChange;
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
