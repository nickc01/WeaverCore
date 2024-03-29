﻿using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Makes an entire scene dark on Start
    /// </summary>
    public class TotalSceneDarkness : MonoBehaviour 
    {
        private void Start()
        {
            var vignette = GameObject.Find("Vignette");

            if (vignette != null)
            {
                EventManager.SendEventToGameObject("SCENE RESET NO LANTERN", vignette, gameObject);
            }
        }
    }
}
