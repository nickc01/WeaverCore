using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using ViridianLink.Core;

namespace ViridianLink.Helpers
{
    public static class ImplInfo
    {
        public static RunningState State { get; private set; }


        static Dictionary<Type, Type> Cache = new Dictionary<Type, Type>();
        static List<Type> FoundImplementations;
        static Assembly ImplAssembly;

        static RunningState GetState()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetName().Name == "UnityEditor")
                {
                    return RunningState.Editor;
                }
            }
            return RunningState.Game;
        }

        static void WriteAssembly(string filePath,Stream data)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            using (var file = File.Create(filePath))
            {
                //data.Copy
                using (var reader = new BinaryReader(data))
                {
                    using (var writer = new BinaryWriter(file))
                    {
                        byte[] buffer = new byte[1024];
                        int amount = 0;
                        do
                        {
                            amount = data.Read(buffer, 0, buffer.Length);
                            if (amount > 0)
                            {
                                writer.Write(buffer, 0, amount);
                            }

                        } while (amount != 0);
                    }
                }
            }
        }

        static string GetHash(Stream stream)
        {
            //nameof(IAsyncResult.)
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(stream));
            }
        }

        static void LoadImplementations()
        {
            Debug.Log("ZZZZ");
            if (FoundImplementations == null)
            {
                FoundImplementations = new List<Type>();
                Debug.Log("Z");
                State = GetState();
                if (State == RunningState.Editor)
                {
                    ImplAssembly = ResourceLoader.LoadAssembly("ViridianLink.Editor");

                    Debug.Log("Y");
                    string filePath = null;
                    //ImplAssembly = ResourceLoader.LoadAssembly("ViridianLink.Editor");
                    using (var resourceStream = ResourceLoader.Retrieve("ViridianLink.Editor.Visual"))
                    {
                        Debug.Log("ResourceStream = " + resourceStream);
                        Debug.Log("X");
                        var info = Directory.CreateDirectory("Assets/Editor/ViridianLink");
                        Debug.Log("W");
                        filePath = info.FullName + "/ViridianLink.Editor.Visual.dll";
                        Debug.Log("V");
                        Debug.Log("File Path = " + filePath);
                        if (File.Exists(filePath))
                        {
                            Debug.Log("AA");
                            string hashA = null;
                            string hashB = null;
                            using (var openStream = File.OpenRead(filePath))
                            {
                                Debug.Log("BB");
                                hashA = GetHash(openStream);
                                openStream.Close();
                            }
                            Debug.Log("CC");
                            hashB = GetHash(resourceStream);
                            if (hashB != hashA)
                            {
                                if (File.Exists(filePath + ".meta"))
                                {
                                    File.Delete(filePath + ".meta");
                                }
                                Debug.Log("SAME HASH!");
                                Debug.Log("Writing");
                                WriteAssembly(filePath, resourceStream);
                            }
                            Debug.Log("DD");
                        }
                        else
                        {
                            if (File.Exists(filePath + ".meta"))
                            {
                                File.Delete(filePath + ".meta");
                            }
                            Debug.Log("Writing2");
                            WriteAssembly(filePath, resourceStream);
                        }
                    }
                    Debug.Log("A");
                    //ImplAssembly = Assembly.LoadFile(filePath);
                    Debug.Log("B");
                    Starter.AddInitializer(() =>
                    {
                        Debug.Log("STARTER");
                        var asm = Assembly.Load("UnityEditor");
                        var adb = asm.GetType("UnityEditor.AssetDatabase");
                        //var method = adb.GetMethod("ImportAsset", new Type[] { typeof(string) });
                        //method.Invoke(null, new object[] { filePath });

                        var refreshMethod = adb.GetMethod("Refresh", new Type[] { });
                        refreshMethod.Invoke(null, null);
                    });
                    Debug.Log("C");

                }
                else
                {
                    try
                    {
                        ImplAssembly = ResourceLoader.LoadAssembly("ViridianLink.Game");
                    }
                    catch (Exception e)
                    {
                        throw new Exception("ERROR : ViridianLink requires the ViridianCore mod to be installed in the hollow knight mod directory to work. Please install it");
                    }
                }
                foreach (var type in ImplAssembly.GetTypes())
                {
                    if (typeof(IImplementation).IsAssignableFrom(type) && !type.IsAbstract && !type.ContainsGenericParameters)
                    {
                        FoundImplementations.Add(type);
                    }
                }
            }
        }

        public static Type GetImplementationType<T>() where T : IImplementation
        {
            var type = typeof(T);
            Type implType = null;
            LoadImplementations();
            if (Cache.ContainsKey(type))
            {
                implType = Cache[type];
            }
            else
            {
                foreach (var foundType in FoundImplementations)
                {
                    if (typeof(T).IsAssignableFrom(foundType))
                    {
                        implType = foundType;
                        Cache.Add(type, implType);
                        break;
                    }
                }
            }
            if (implType == null)
            {
                throw new Exception($"Implementation for {typeof(T).FullName} does not exist in {ImplAssembly.FullName}");
            }
            else
            {
                return implType;
            }
        }

        public static T GetImplementation<T>() where T : IImplementation
        {
            return (T)Activator.CreateInstance(GetImplementationType<T>());
        }
    }
}
