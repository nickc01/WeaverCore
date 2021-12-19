using System;
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

namespace WeaverCore.Editor.Implementations
{
    class EditorInitializer
    {
        //private static string InputManagerData

        [OnInit]
        static void Init()
        {
            AddInitializer(() =>
            {
                

                //Physics2D.gravity = new Vector2(0f, -60f);

                /*SerializedObject graphicsSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);

                var AIS = graphicsSettings.FindProperty("m_AlwaysIncludedShaders");
                for (int i = 0; i < AIS.arraySize; i++)
                {
                    var element = AIS.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (element != null && element.name == "Sprites/Default")
                    {
                        AIS.DeleteArrayElementAtIndex(i);
                        graphicsSettings.ApplyModifiedProperties();
                        break;
                    }
                }*/

                /*var inputManObject = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];

                //Debug.Log("Input Obj Type = " + inputManObject.GetType());
                //Debug.Log("Input Man = " + EditorJsonUtility.ToJson(inputManObject,true));

                SerializedObject inputManager = new SerializedObject(inputManObject);
                var axes = inputManager.FindProperty("m_Axes");

				if (axes.arraySize <= 20)
				{
                    var inputManData = JsonUtility.FromJson<InputList>(EditorAssets.LoadEditorAsset<TextAsset>("Input Manager Data").text);

                    axes.arraySize = inputManData.Inputs.Count;

					for (int i = 0; i < inputManData.Inputs.Count; i++)
					{
                        var inputField = inputManData.Inputs[i];
                        var parent = axes.GetArrayElementAtIndex(i);

                        parent.FindPropertyRelative("m_Name").stringValue = inputField.m_Name;
                        parent.FindPropertyRelative("descriptiveName").stringValue = inputField.descriptiveName;
                        parent.FindPropertyRelative("descriptiveNegativeName").stringValue = inputField.descriptiveNegativeName;
                        parent.FindPropertyRelative("negativeButton").stringValue = inputField.negativeButton;
                        parent.FindPropertyRelative("positiveButton").stringValue = inputField.positiveButton;
                        parent.FindPropertyRelative("altNegativeButton").stringValue = inputField.altNegativeButton;
                        parent.FindPropertyRelative("altPositiveButton").stringValue = inputField.altPositiveButton;
                        parent.FindPropertyRelative("gravity").floatValue = inputField.gravity;
                        parent.FindPropertyRelative("gravity").doubleValue = inputField.dead;
                        parent.FindPropertyRelative("sensitivity").floatValue = inputField.sensitivity;
                        parent.FindPropertyRelative("snap").boolValue = inputField.snap;
                        parent.FindPropertyRelative("invert").boolValue = inputField.invert;
                        parent.FindPropertyRelative("type").intValue = inputField.type;
                        parent.FindPropertyRelative("axis").intValue = inputField.axis;
                        parent.FindPropertyRelative("joyNum").intValue = inputField.joyNum;
                    }

                    inputManager.ApplyModifiedProperties();
                }*/

                /*InputList inputs = new InputList();

                var axes = inputManager.FindProperty("m_Axes");

				for (int i = 0; i < axes.arraySize; i++)
				{
                    var parent = axes.GetArrayElementAtIndex(i);

                    var inputField = new InputField();
                    inputField.m_Name = parent.FindPropertyRelative("m_Name").stringValue;
                    inputField.descriptiveName = parent.FindPropertyRelative("descriptiveName").stringValue;
                    inputField.descriptiveNegativeName = parent.FindPropertyRelative("descriptiveNegativeName").stringValue;
                    inputField.negativeButton = parent.FindPropertyRelative("negativeButton").stringValue;
                    inputField.positiveButton = parent.FindPropertyRelative("positiveButton").stringValue;
                    inputField.altNegativeButton = parent.FindPropertyRelative("altNegativeButton").stringValue;
                    inputField.altPositiveButton = parent.FindPropertyRelative("altPositiveButton").stringValue;
                    inputField.gravity = parent.FindPropertyRelative("gravity").floatValue;
                    inputField.dead = parent.FindPropertyRelative("gravity").doubleValue;
                    inputField.sensitivity = parent.FindPropertyRelative("sensitivity").floatValue;
                    inputField.snap = parent.FindPropertyRelative("snap").boolValue;
                    inputField.invert = parent.FindPropertyRelative("invert").boolValue;
                    inputField.type = parent.FindPropertyRelative("type").intValue;
                    inputField.axis = parent.FindPropertyRelative("axis").intValue;
                    inputField.joyNum = parent.FindPropertyRelative("joyNum").intValue;

                    inputs.Inputs.Add(inputField);
                }*/

                //Debug.Log(EditorJsonUtility.ToJson(inputs, true));
                //File.WriteAllText("TestFile.txt", EditorJsonUtility.ToJson(inputs, true));
                //SerializedObject inputManager = new SerializedObject();
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
}

