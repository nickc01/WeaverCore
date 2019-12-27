using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ViridianCore.Internal
{
    internal class GameExit : MonoBehaviour
    {
        internal static event Action OnGameQuit;


        static GameObject quitTracker = null;


        [ModStart]
        static void OnModStart()
        {
            if (quitTracker == null)
            {
                quitTracker = new GameObject("__QUIT_TRACKER__", typeof(GameExit));
            }
        }

        void OnApplicationQuit()
        {
            OnGameQuit?.Invoke();
        }
    }
}
