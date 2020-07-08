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
            //var workingDir = new DirectoryInfo(typeof(LayerData).Assembly.Location);
            var assetDir = new DirectoryInfo("Assets\\WeaverCore\\WeaverCore.Editor\\Data");
            Debug.Log("Assets Dir = " + assetDir.FullName);
            Debug.Log("Exists = " + assetDir.Exists);
            if (assetDir.Exists)
            {
                var dataFile = new FileInfo(assetDir.FullName + "\\layerData.json");
                Debug.Log("Data File = " + dataFile.FullName);
                Debug.Log("Exists = " + dataFile.Exists);
                if (dataFile.Exists)
                {
                    return JsonUtility.FromJson<LayerData>(File.ReadAllText(dataFile.FullName));
                }
            }
            return default(LayerData);
            /*foreach (var file in workingDir.GetFiles("layerData.json",SearchOption.AllDirectories))
            {
                if (file.FullName.Contains("Data"))
                {
                    return JsonUtility.FromJson<LayerData>(File.ReadAllText(file.FullName));
                }
            }
            return default(LayerData);*/
            /*using (var stream = typeof(Implementations.EditorInitializer).Assembly.GetManifestResourceStream("WeaverCore.Editor.Editor.Data.layerData.json"))
            {
                using (var reader = new StreamReader(stream))
                {
                    return JsonUtility.FromJson<LayerData>(reader.ReadToEnd());
                }
            }*/
        }
    }
}
