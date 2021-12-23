using System;
using System.IO;
using UnityEngine;
using WeaverCore.Editor.Compilation;
using WeaverCore.Editor.Implementations;
using WeaverCore.Utilities;

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
            var assetDir = new DirectoryInfo(BuildTools.WeaverCoreFolder.AddSlash() + $"WeaverCore.Editor{Path.DirectorySeparatorChar}Data");
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
