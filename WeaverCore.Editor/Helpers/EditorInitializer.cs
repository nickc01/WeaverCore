using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Editor.Helpers
{
    public static class EditorInitializer
    {
        [ExecuteInEditMode]
        class EditorInitializer_Internal : MonoBehaviour
        {
            void Awake()
            {
                Update();
                /*initializeEvent?.Invoke();
                if (initializeEvent == null)
                {
                    DestroyImmediate(gameObject);
                }*/
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
                /*initializeEvent?.Invoke();
                if (initializeEvent == null)
                {
                    DestroyImmediate(gameObject);
                }*/
            }
        }

        //static event Action initializeEvent;
        static List<Action> InitializeEvents = new List<Action>();
        static GameObject initializerObject;

        static void OnUpdate()
        {
            if (initializerObject == null)
            {
                initializerObject = new GameObject("__INITIALIZER__", typeof(EditorInitializer_Internal));
            }
            //EditorApplication.update -= OnUpdate;
        }

        public static void AddInitializer(Action func)
        {
            /*Action additionFunc = null;
            additionFunc = () =>
            {
                try
                {
                    func();
                }
                finally
                {
                    InitializeEvents.Remove(additionFunc);
                    //initializeEvent -= additionFunc;
                }
            };*/
            if (InitializeEvents.Count == 0)
            {
                EditorApplication.update += OnUpdate;
            }
            InitializeEvents.Add(func);
        }
    }
}
