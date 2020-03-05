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
using WeaverCore.Editor.Helpers;
using System;

namespace WeaverCore.Editor.Implementations
{
    public class Initializer : InitializerImplementation
    {

        public override void Initialize()
        {
            LoadVisualAssembly();
            EditorInitializer.AddInitializer(() =>
            {
                var data = LayerData.GetData();
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
                var visualAssembly = System.Reflection.Assembly.LoadFile($"Assets/{nameof(WeaverCore)}/Editor/{nameof(WeaverCore)}.Editor.Visual.dll");
                var initializerType = visualAssembly.GetType("WeaverCore.Editor.Visual.Internal.Initializer");
                initializerType.GetMethod("Initialize",BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            });
        }

        static void LoadVisualAssembly()
        {
            Stream resourceStream = ResourceLoader.Retrieve($"{nameof(WeaverCore)}.Editor.Visual"); //Gets disposed in the Initializer below
            var resourceHash = Hash.GetHash(resourceStream);
            var directory = Directory.CreateDirectory($"Assets/{nameof(WeaverCore)}/Editor");
            string filePath = directory.FullName + $"/{nameof(WeaverCore)}.Editor.Visual.dll";

            if (!File.Exists(filePath) || resourceHash != Hash.GetHash(filePath))
            {
                EditorInitializer.AddInitializer(() =>
                {
                    AssetDatabase.StartAssetEditing();
                    try
                    {
                        WriteAssembly(new DirectoryInfo("Assets").Parent.FullName, $"Assets/{nameof(WeaverCore)}/Editor/{nameof(WeaverCore)}.Editor.Visual.dll", $"{nameof(WeaverCore)}.Editor.Visual.dll", resourceStream);
                        resourceStream.Dispose();
                        using (var internalStream = ResourceLoader.Retrieve($"{nameof(WeaverCore)}.Resources.InternalClasses.txt"))
                        {
                            using (var output = File.Create($"Assets/{nameof(WeaverCore)}/Internal.cs"))
                            {
                                using (var writer = new StreamWriter(output))
                                {
                                    using (var reader = new StreamReader(internalStream))
                                    {
                                        writer.Write(reader.ReadToEnd());
                                    }
                                }
                            }
                        }
                        AssetDatabase.ImportAsset($"Assets/{nameof(WeaverCore)}/Internal.cs");
                    }
                    finally
                    {
                        AssetDatabase.StopAssetEditing();
                    }
                });
            }
        }

        static void WriteAssembly(string directory, string filePath, string fileName, Stream data)
        {
            string fullPath = directory + "/" + filePath;
            if (File.Exists(fullPath))
            {
                AssetDatabase.DeleteAsset(filePath);
            }

            var tempPath = Path.GetTempPath();
            if (File.Exists(tempPath + fileName))
            {
                File.Delete(tempPath + fileName);
            }
            using (var file = File.Create(tempPath + fileName))
            {
                using (var reader = new BinaryReader(data))
                {
                    using (var writer = new BinaryWriter(file))
                    {
                        byte[] buffer = new byte[1024];
                        int amount = 0;
                        do
                        {
                            amount = reader.Read(buffer, 0, buffer.Length);
                            if (amount > 0)
                            {
                                writer.Write(buffer, 0, amount);
                            }

                        } while (amount != 0);
                    }
                }
            }
            File.Move(tempPath + fileName, fullPath);
            AssetDatabase.ImportAsset(filePath);
        }
    }
}

