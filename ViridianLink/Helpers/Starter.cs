using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace ViridianLink.Helpers
{
    public static class Starter
    {
        [ExecuteInEditMode]
        class InternalStarter : MonoBehaviour
        {
            void Awake()
            {
                initializer?.Invoke();
                Debug.Log("INITIALIZER = NULL = " + (initializer == null));
                if (initializer == null)
                {
                    Debug.Log("DESTROY");
                    GameObject.DestroyImmediate(gameObject);
                }
            }

            void Update()
            {
                initializer?.Invoke();
                Debug.Log("INITIALIZER = NULL = " + (initializer == null));
                if (initializer == null)
                {
                    Debug.Log("DESTROY");
                    GameObject.DestroyImmediate(gameObject);
                }
            }
        }

        static event Action initializer;
        static GameObject starterObject;

        static Assembly UnityEditorAsm;
        static FieldInfo updateEvent;
        static Type EditorApplicationType;
        static Type CallbackFunctionDelegate;

        static Delegate UpdateDelegate;

        static void OnUpdate()
        {
            Debug.Log("CREATE");
            starterObject = new GameObject("__INITIALIZER__", typeof(InternalStarter));
            updateEvent.SetValue(null, Delegate.Remove((Delegate)updateEvent.GetValue(null), UpdateDelegate));
        }

        public static void AddInitializer(Action func)
        {
            if (CallbackFunctionDelegate == null)
            {
                UnityEditorAsm = Assembly.Load("UnityEditor");
                EditorApplicationType = UnityEditorAsm.GetType("UnityEditor.EditorApplication");
                updateEvent = EditorApplicationType.GetField("update", BindingFlags.Public | BindingFlags.Static);
                CallbackFunctionDelegate = updateEvent.FieldType;

                UpdateDelegate = Delegate.CreateDelegate(CallbackFunctionDelegate, typeof(Starter).GetMethod("OnUpdate", BindingFlags.NonPublic | BindingFlags.Static));
            }
            Action additionFunc = null;
            additionFunc = () =>
            {
                func();
                initializer -= additionFunc;
            };
            if (initializer == null)
            {
                updateEvent.SetValue(null,Delegate.Combine((Delegate)updateEvent.GetValue(null),UpdateDelegate));
            }
            initializer += additionFunc;
        }
    }
}
