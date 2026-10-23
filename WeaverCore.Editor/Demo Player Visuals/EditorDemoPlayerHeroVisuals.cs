using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WeaverCore;

[InitializeOnLoad]
public sealed class EditorDemoPlayerHeroVisuals : MonoBehaviour
{
    const string HeroLightPrefabPath =
        "Assets/WeaverCore/WeaverCore.Editor/Assets/Prefabs/Demo Player Visuals/Editor Demo HeroLight.prefab";

    const string VignettePrefabPath =
        "Assets/WeaverCore/WeaverCore.Editor/Assets/Prefabs/Demo Player Visuals/Editor Demo Vignette.prefab";

    const string HeroLightChildName = "HeroLight";
    const string VignetteChildName = "Vignette";

    EditorDemoHeroLight heroLight;
    EditorDemoVignette vignette;

    static EditorDemoPlayerHeroVisuals()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredPlayMode)
        {
            InstallIntoOpenScenes();
        }
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (Application.isPlaying)
        {
            InstallIntoOpenScenes();
        }
    }

    static void InstallIntoOpenScenes()
    {
        foreach (var controller in Object.FindObjectsOfType<HeroController>())
        {
            if (controller == null || controller.gameObject.name != "Demo Player")
            {
                continue;
            }

            if (controller.GetComponent<EditorDemoPlayerHeroVisuals>() == null)
            {
                controller.gameObject.AddComponent<EditorDemoPlayerHeroVisuals>();
            }
        }
    }

    void Awake()
    {
        CreateVisuals();
    }

    void OnEnable()
    {
        EventManager.OnEventTriggered += OnEventTriggered;
    }

    void OnDisable()
    {
        EventManager.OnEventTriggered -= OnEventTriggered;
    }

    public void Damage()
    {
        CreateVisuals();
        heroLight.Damage();
        vignette.Damage();
    }

    public void Death()
    {
        CreateVisuals();
        heroLight.Death();
        vignette.Death();
    }

    public void SendEvent(string eventName)
    {
        CreateVisuals();

        if (eventName == "DAMAGE")
        {
            Damage();
            return;
        }

        if (eventName == "DEATH")
        {
            Death();
            return;
        }

        vignette.SendEvent(eventName);
    }

    void OnEventTriggered(string eventName, GameObject source, GameObject destination,
        EventManager.EventType eventType)
    {
        if (!IsVisualEvent(eventName))
        {
            return;
        }

        if (eventType == EventManager.EventType.Message &&
            !BelongsToThisPlayer(source) && !BelongsToThisPlayer(destination))
        {
            return;
        }

        if (eventType == EventManager.EventType.Broadcast &&
            (eventName == "DAMAGE" || eventName == "DEATH") &&
            !BelongsToThisPlayer(source))
        {
            return;
        }

        SendEvent(eventName);
    }

    static bool IsVisualEvent(string eventName)
    {
        switch (eventName)
        {
            case "DAMAGE":
            case "DEATH":
            case "NORMAL":
            case "DARK 1":
            case "DARK 2":
            case "DARK -1":
            case "LANTERN":
            case "RESET":
            case "SCENE RESET":
            case "SCENE RESET NO LANTERN":
                return true;
            default:
                return false;
        }
    }

    bool BelongsToThisPlayer(GameObject value)
    {
        return value != null &&
               (value == gameObject || value.transform.IsChildOf(transform));
    }

    void CreateVisuals()
    {
        if (heroLight == null)
        {
            heroLight = GetOrCreate<EditorDemoHeroLight>(HeroLightPrefabPath, HeroLightChildName);
        }

        if (vignette == null)
        {
            vignette = GetOrCreate<EditorDemoVignette>(VignettePrefabPath, VignetteChildName);
        }
    }

    T GetOrCreate<T>(string prefabPath, string childName) where T : Component
    {
        var child = transform.Find(childName);
        if (child != null)
        {
            return child.GetComponent<T>();
        }

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            Debug.LogError("Editor Demo Player visual prefab is missing: " + prefabPath, this);
            return null;
        }

        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, gameObject.scene);
        instance.name = childName;
        instance.transform.SetParent(transform, false);
        return instance.GetComponent<T>();
    }
}
