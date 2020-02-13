using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace WeaverCore.Internal
{
    public static class EditorInitializer
    {
        [ExecuteInEditMode]
        class EditorInitializer_Internal : MonoBehaviour
        {
            void Awake()
            {
                initializeEvent?.Invoke();
                if (initializeEvent == null)
                {
                    DestroyImmediate(gameObject);
                }
            }

            void Update()
            {
                initializeEvent?.Invoke();
                if (initializeEvent == null)
                {
                    DestroyImmediate(gameObject);
                }
            }
        }

        static event Action initializeEvent;
        static GameObject initializerObject;

        static Assembly UnityEditorAsm;
        static FieldInfo updateEventF;
        static Type EditorApplicationT;
        static Type CallbackFunctionDelegateT;

        static Delegate UpdateDelegate;

        static void OnUpdate()
        {
            initializerObject = new GameObject("__INITIALIZER__", typeof(EditorInitializer_Internal));
            updateEventF.SetValue(null, Delegate.Remove((Delegate)updateEventF.GetValue(null), UpdateDelegate));
        }

        public static void AddInitializer(Action func)
        {
            if (CallbackFunctionDelegateT == null)
            {
                UnityEditorAsm = Assembly.Load("UnityEditor");
                EditorApplicationT = UnityEditorAsm.GetType("UnityEditor.EditorApplication");
                updateEventF = EditorApplicationT.GetField("update", BindingFlags.Public | BindingFlags.Static);
                CallbackFunctionDelegateT = updateEventF.FieldType;

                UpdateDelegate = Delegate.CreateDelegate(CallbackFunctionDelegateT, typeof(EditorInitializer).GetMethod("OnUpdate", BindingFlags.NonPublic | BindingFlags.Static));
            }
            Action additionFunc = null;
            additionFunc = () =>
            {
                func();
                initializeEvent -= additionFunc;
            };
            if (initializeEvent == null)
            {
                updateEventF.SetValue(null, Delegate.Combine((Delegate)updateEventF.GetValue(null), UpdateDelegate));
            }
            initializeEvent += additionFunc;
        }
    }
}
