using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using VoidCore;
using VoidCore.Hooks;

namespace VoidCore
{
    internal static class BuiltInLoader
    {


        static Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();


        static internal Assembly AssemblyLoader(object sender, ResolveEventArgs args)
        {
            Modding.Logger.Log("IN ASSEMBLY RESOLVER");
            try
            {
                if (LoadedAssemblies.ContainsKey(args.Name))
                {
                    Modding.Logger.Log("YES1");
                    return LoadedAssemblies[args.Name];
                }
                var assemblyName = new AssemblyName(args.Name);

                //var sourceAssembly = typeof(VoidCore).Assembly;
                foreach (var sourceAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (var resource in sourceAssembly.GetManifestResourceNames())
                        {
                            if (resource.Contains(assemblyName.Name))
                            {
                                using (var stream = sourceAssembly.GetManifestResourceStream(resource))
                                {
                                    byte[] assemblyRaw = new byte[stream.Length];
                                    stream.Read(assemblyRaw, 0, assemblyRaw.Length);
                                    LoadedAssemblies.Add(args.Name, Assembly.Load(assemblyRaw));
                                    Modding.Logger.Log("YES2");
                                    return LoadedAssemblies[args.Name];
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {

                    }
                }
            }
            catch (Exception e)
            {
                Modding.Logger.LogError(e);
            }
            Modding.Logger.Log("Fail");
            return null;
        }
    }
}

/*internal class UITesting : UIHook
{
    LoadedAudio music;
    AudioSource audioPlayer;

    void Start()
    {
        var audioObject = new GameObject();
        audioObject.transform.position = FindObjectOfType<AudioListener>().transform.position;
        audioPlayer = audioObject.AddComponent<AudioSource>();
        music = ResourceLoader.LoadResourceAudio("Resources.Audio.kevintest.mp3");
        audioPlayer.clip = music.clip;
        audioPlayer.Play();
    }

    void OnDestroy()
    {
        audioPlayer.Stop();
        audioPlayer.clip = null;
        music.Dispose();
    }
}*/
