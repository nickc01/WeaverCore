using System;
using System.Reflection;
using GlobalEnums;
using Modding;
using Harmony;
using UnityEngine;
using CrystalCore;
using CrystalCore.Hooks;
using System.Collections;
using System.IO;
using Logger = Modding.Logger;



namespace CrystalCore
{

    public class CrystalCore : Mod
    {
        static bool Patched = false;
        public override int LoadPriority()
        {
            return int.MinValue;
        }


        static internal CrystalCore Instance;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;

            if (!Patched)
            {
                var harmony = ModuleInitializer.GetCrystalCoreHarmony() as HarmonyInstance;
                harmony.PatchAll();
                Patched = true;
            }

            base.Initialize();
        }
    }
}

/*internal class PlayerTest : PlayerHook<CrystalCore.CrystalCore>
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