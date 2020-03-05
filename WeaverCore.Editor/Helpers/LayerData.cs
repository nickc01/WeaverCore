using System;
using System.IO;
using UnityEngine;
using WeaverCore.Editor.Implementations;

namespace WeaverCore.Editor.Helpers
{
    [Serializable]
    public struct LayerData
    {
        public string[] NameData;

        public bool[] CollisionData;

        public LayerData(string[] nameData, bool[] collisionData)
        {
            NameData = nameData;
            CollisionData = collisionData;
        }

        public static LayerData GetData()
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
