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

    internal class VoidCore : Mod, ITogglableMod
    {

        public override int LoadPriority()
        {
            return int.MinValue;
        }


        static internal VoidCore Instance;

        public override string GetVersion() => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public override void Initialize()
        {
            Instance = this;

            var harmony = ModuleInitializer.GetVoidCoreHarmony() as HarmonyInstance;
            harmony.PatchAll();

            base.Initialize();
        }

        public void Unload()
        {
            
        }
    }
}

internal class PlayerTest : PlayerHook<VoidCore.VoidCore>
{
    LoadedAudio loadedAudio = null;
    AudioSource source;
    GameObject audioObject;

    void Start()
    {
        try
        {
            ModLog.Log("T1");
            loadedAudio = ResourceLoader.LoadResourceAudio("Resources.Audio.kevinMusic.mp3");
            audioObject = new GameObject("TESTAUDIO");
            DontDestroyOnLoad(audioObject);
            ModLog.Log("T2");
            audioObject.transform.position = Camera.main.transform.position;
            source = audioObject.AddComponent<AudioSource>();
            ModLog.Log("T3");
            source.volume = 1.0f;
            source.clip = loadedAudio.Clip;
            source.Play();
            /*ModLog.Log("T4");
            ModLog.Log("PLAYER TEST LOADED_____");
            ModLog.Log("THIS SHOULD BE IN THE LOG");
            foreach (var component in GetComponents<Component>())
            {
                ModLog.Log("PLAYER COMPONENT = " + component.name);
                ModLog.Log("COMPONENT TYPE = " + component.GetType());
                ModLog.Log("TESTING");
                try
                {
                    string test = JsonUtility.ToJson(component);
                    ModLog.Log("JSON = " + test);
                }
                catch (Exception e)
                {
                    ModLog.LogError(e);
                }
            }

            ModLog.Log("PLAYER LAYER = " + gameObject.layer);
            ModLog.Log("PLAYER LAYER Name = " + LayerMask.LayerToName(gameObject.layer));*/
        }
        catch (Exception e)
        {
            Modding.Logger.LogError("PLAYER EXCEPTION = " + e);
        }

        // ModLog.Log("TK2D Sprite = " + JsonUtility.ToJson(GetComponent<tk2dSprite>()));




        //VoidCore.VoidCore.testObject.GetComponent<SpriteRenderer>().sortingLayerID = GetComponent<SpriteRenderer>().sortingLayerID;
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
        //VoidCore.VoidCore.testObject.transform.position = transform.position;
        //VoidCore.VoidCore.testObject.layer = gameObject.layer;
        //var setZ = GetComponent<SetZ>();
        //setZ.z -= Time.deltaTime;
        //setZ.enabled = false;
        //setZ.enabled = true;
        //ModLog.Log("Z = " + setZ.z);
        //VoidCore.VoidCore.testObject.transform.position = transform.position - Vector3.one;
    }

    public override void LoadHook(IMod mod)
    {
        
    }

    public override void UnloadHook(IMod mod)
    {
        
    }
}

/*internal class UITESTING : UIHook
{
    static UITESTING instance;

    internal static Coroutine CoroutineStart(IEnumerator routine)
    {
        if (instance == null)
        {
            return null;
        }
        return instance.StartCoroutine(routine);
    }

    void Awake()
    {
        ModLog.Log("UITESTINGSTART");
        if (instance == null)
        {
            instance = this;
        }
    }
}

internal class ComponentCtor1
{

    static IEnumerator Testing(Component instance)
    {
        yield return 0;
        ModLog.Log("Testing");
    }


    static void Post(Component __instance)
    {
        UITESTING.CoroutineStart(Testing(__instance));
        //ModLog.Log("Component Created 1 = " + __instance.name);
    }
}*/
