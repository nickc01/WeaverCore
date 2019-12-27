using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ViridianLink
{
    static class ModuleInitializer
    {
        static bool Initialized = false;

        public static void Initialize()
        {
            if (!Initialized)
            {
                Initialized = true;
                var init = Implementation.GetImplementation<InitializerImplementation>();
                init.Initialize();
            }
        }
    }

    public class TestComponent : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("TEST COMPONENT STARTED");
        }
    }
}
