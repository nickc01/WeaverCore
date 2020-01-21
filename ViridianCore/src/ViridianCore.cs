using System;
using System.Reflection;
using GlobalEnums;
using Modding;
using UnityEngine;
using ViridianCore;
using ViridianCore.Hooks;
using System.Collections;
using System.IO;
using Logger = Modding.Logger;



namespace ViridianCore
{

    public class ViridianCore : Mod
    {
        static bool Patched = false;
        public override int LoadPriority()
        {
            return int.MinValue;
        }


        static internal ViridianCore Instance;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;

            if (!Patched)
            {
                //var harmony = ModuleInitializer.GetViridianCoreHarmony() as HarmonyInstance;
                //harmony.PatchAll();
                Helpers.Harmony.ViridianHarmonyInstance.PatchAll();
                //Helpers.Harmony.PatchAll();
                Patched = true;
            }

            base.Initialize();
        }
    }
}

/*internal class PlayerTest : PlayerHook<ViridianCore.ViridianCore>
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