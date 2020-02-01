using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using System.IO;
using ViridianLink.Helpers;
using ViridianLink.Implementations;
using UnityEditor.Compilation;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace ViridianLink.Editor.Implementations
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
            EditorApplication.playModeStateChanged += PlayingCallback;
            Starter.AddInitializer(() =>
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
            });

        }

        private static void PlayingCallback(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.EnteredPlayMode)
            {
                ModLoader.LoadAllMods();
            }
        }

        LayerData GetData()
        {
            using (var stream = typeof(Initializer).Assembly.GetManifestResourceStream("ViridianLink.Editor.Resources.layerData.json"))
            {
                using (var reader = new StreamReader(stream))
                {
                    return JsonUtility.FromJson<LayerData>(reader.ReadToEnd());
                }
            }
        }
    }
}
