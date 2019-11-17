using System;
using System.Reflection;
using GlobalEnums;
using Modding;
using Harmony;
using UnityEngine;
using VoidCore;
using VoidCore.Hooks;
using System.Collections;
using System.IO;
using Logger = Modding.Logger;



namespace VoidCore
{

    public class VoidCore : Mod, ITogglableMod
    {
        static bool Patched = false;
        public override int LoadPriority()
        {
            return int.MinValue;
        }


        static internal VoidCore Instance;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;

            if (!Patched)
            {
                var harmony = ModuleInitializer.GetVoidCoreHarmony() as HarmonyInstance;
                harmony.PatchAll();
                Patched = true;
            }

            base.Initialize();
        }

        public void Unload()
        {
            
        }
    }
}

/*internal class PlayerTest : PlayerHook<VoidCore.VoidCore>
{
    LoadedAudio loadedAudio = null;
    AudioSource source;
    GameObject audioObject;

    void Start()
    {
        try
        {
            loadedAudio = ResourceLoader.LoadResourceAudio("Resources.Audio.kevinMusic.mp3");
            audioObject = new GameObject("TESTAUDIO");
            DontDestroyOnLoad(audioObject);
            audioObject.transform.position = Camera.main.transform.position;
            source = audioObject.AddComponent<AudioSource>();
            source.volume = 1.0f;
            source.clip = loadedAudio.Clip;
            source.Play();
        }
        catch (Exception e)
        {
            Modding.Logger.LogError("PLAYER EXCEPTION = " + e);
        }
    }

    void OnDestroy()
    {
        source.Stop();
        source.clip = null;
        loadedAudio.Dispose();
        Destroy(audioObject);
    }

    void Update()
    {
        audioObject.transform.position = Camera.main.transform.position;
    }

    public override void LoadHook(IMod mod)
    {
        
    }

    public override void UnloadHook(IMod mod)
    {
        
    }
}*/