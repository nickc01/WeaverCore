/*using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;
using WeaverCore.Implementations;
using WeaverCore.Interfaces;
using System.Collections.Generic;
using WeaverCore.Internal;
using WeaverCore.Editor.Structures;
using WeaverCore.Attributes;

namespace WeaverCore.Editor
{
    class EditorInitializer
    {
        //private static string InputManagerData

        [OnInit]
        static void Init()
        {
            AddInitializer(() =>
            {
            });
        }

        [ExecuteInEditMode]
        class EditorInitializer_Internal : MonoBehaviour
        {
            void Awake()
            {
                Update();
            }

            void Update()
            {
                var next = InitializeEvents[0];
                try
                {
                    next();
                }
                finally
                {
                    InitializeEvents.Remove(next);
                    if (InitializeEvents.Count == 0)
                    {
                        EditorApplication.update -= OnUpdate;
                        DestroyImmediate(gameObject);
                        initializerObject = null;
                    }
                }
            }
        }

        static List<Action> InitializeEvents = new List<Action>();
        static GameObject initializerObject;

        static void OnUpdate()
        {
            if (initializerObject == null)
            {
                initializerObject = new GameObject("__INITIALIZER__", typeof(EditorInitializer_Internal));
            }
        }

        public static void AddInitializer(Action func)
        {
            if (InitializeEvents.Count == 0)
            {
                EditorApplication.update += OnUpdate;
            }
            InitializeEvents.Add(func);
        }
    }
}*/

