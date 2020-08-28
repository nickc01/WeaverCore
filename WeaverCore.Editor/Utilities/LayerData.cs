using System;
using System.IO;
using UnityEngine;
using WeaverCore.Editor.Implementations;

namespace WeaverCore.Editor.Utilities
{
    [Serializable]
    public class LayerData
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
            var assetDir = new DirectoryInfo("Assets\\WeaverCore\\WeaverCore.Editor\\Data");
            if (assetDir.Exists)
            {
                var dataFile = new FileInfo(assetDir.FullName + "\\layerData.json");
                if (dataFile.Exists)
                {
                    return JsonUtility.FromJson<LayerData>(File.ReadAllText(dataFile.FullName));
                }
            }
            return default(LayerData);
        }
    }
}
