using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
    [Serializable]
    class InputList
    {
        public List<InputField> Inputs = new List<InputField>();
    }

    [Serializable]
    class InputField
    {
        public string m_Name;
        public string descriptiveName;
        public string descriptiveNegativeName;
        public string negativeButton;
        public string positiveButton;
        public string altNegativeButton;
        public string altPositiveButton;
        public float gravity;
        public double dead;
        public float sensitivity;
        public bool snap;
        public bool invert;
        public int type;
        public int axis;
        public int joyNum;
    }

    /// <summary>
    /// Verifies the Input Manager is properly configured for WeaverCore
    /// </summary>
    class VerifyInputChecker : DependencyCheck
    {

        public override void StartCheck(Action<DependencyCheckResult> finishCheck)
        {
            var inputManObject = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];

            SerializedObject inputManager = new SerializedObject(inputManObject);
            var axes = inputManager.FindProperty("m_Axes");

            var inputData = EditorAssets.LoadEditorAsset<TextAsset>("Input Manager Data");

            if (inputData == null)
            {
                finishCheck(DependencyCheckResult.Complete);
                return;
            }

            var inputManData = JsonUtility.FromJson<InputList>(inputData.text);

            axes.arraySize = inputManData.Inputs.Count;

            var currentFields = GetCurrentInputFields(axes);

            for (int i = 0; i < inputManData.Inputs.Count; i++)
            {
                var inputField = inputManData.Inputs[i];

                var currentFieldIndex = currentFields.FindIndex(i, f => f.m_Name == inputField.m_Name);

                if (currentFieldIndex >= 0)
                {
                    var currentFieldFound = currentFields[currentFieldIndex];
                    if (currentFieldFound.positiveButton != inputField.positiveButton)
                    {
                        var currentFieldParent = axes.GetArrayElementAtIndex(currentFieldIndex);
                        currentFieldParent.FindPropertyRelative("positiveButton").stringValue = inputField.positiveButton;
                    }
                    continue;
                }


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
            //}
            finishCheck(DependencyCheckResult.Complete);
        }

        static List<InputField> GetCurrentInputFields(SerializedProperty axesProperties)
        {
            List<InputField> fields = new List<InputField>();

            for (int i = 0; i < axesProperties.arraySize; i++)
            {
                var parent = axesProperties.GetArrayElementAtIndex(i);
                var name = parent.FindPropertyRelative("m_Name").stringValue;
                if (!string.IsNullOrEmpty(name))
                {
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

                    fields.Add(inputField);
                }
            }
            return fields;
        }
    }
}
