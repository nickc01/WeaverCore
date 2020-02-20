using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.IO;
using WeaverCore.Implementations;
using UnityEditor.Compilation;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using WeaverCore.Internal;
using WeaverCore.Helpers;

namespace WeaverCore.Editor.Implementations
{
	public class Initializer : InitializerImplementation
    {
        [Serializable]
        struct LayerData
        {
            public string[] NameData;

            public bool[] CollisionData;

            public LayerData(string[] nameData, bool[] collisionData)
            {
                NameData = nameData;
                CollisionData = collisionData;
            }
        }

        public override void Initialize()
        {
            EditorInitializer.AddInitializer(() =>
            {
                var data = GetData();
                for (int i = 8; i < 32; i++)
                {
                    LayerChanger.SetLayerName(i, data.NameData[i]);
                }
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        int index = i + (j * 32);
                        Physics2D.IgnoreLayerCollision(i, j, data.CollisionData[index]);
                    }
                }
                Physics2D.gravity = new Vector2(0f,-60f);
                var visualAssembly = System.Reflection.Assembly.Load("WeaverCore.Editor.Visual");
                //Debugger.Log("Visual Assembly = " + visualAssembly);
                var initializerType = visualAssembly.GetType("WeaverCore.Editor.Visual.Internal.Initializer");
                //Debugger.Log("InitializerType = " + initializerType);
                initializerType.GetMethod("Initialize",BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            });

        }

        LayerData GetData()
        {
            using (var stream = typeof(Initializer).Assembly.GetManifestResourceStream($"{nameof(WeaverCore)}.Editor.Resources.layerData.json"))
            {
                using (var reader = new StreamReader(stream))
                {
                    return JsonUtility.FromJson<LayerData>(reader.ReadToEnd());
                }
            }
        }
    }
}
