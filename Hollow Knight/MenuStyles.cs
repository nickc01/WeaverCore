using MonoMod.Utils;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

public class MenuStyles : MonoBehaviour
{
    [Serializable]
    public class MenuStyle
    {
        [Serializable]
        public class CameraCurves
        {
            [Range(0f, 5f)]
            public float saturation = 1f;

            public AnimationCurve redChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

            public AnimationCurve greenChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

            public AnimationCurve blueChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));
        }

        public bool enabled = true;

        public string displayName = "Untitled Style";

        public GameObject styleObject;

        public CameraCurves cameraColorCorrection;

        [FormerlySerializedAs("snapshot")]
        public AudioMixerSnapshot musicSnapshot;

        public int titleIndex = -1;

        public string unlockKey = "";

        public string[] achievementKeys;

        [HideInInspector]
        public float[] initialAudioVolumes;

        public bool IsAvailable
        {
            get
            {
                return true;
            }
        }

        public void SetInitialAudioVolumes(AudioSource[] sources)
        {
            if (initialAudioVolumes == null || initialAudioVolumes.Length == 0)
            {
                initialAudioVolumes = new float[sources.Length];
                for (int i = 0; i < initialAudioVolumes.Length; i++)
                {
                    initialAudioVolumes[i] = sources[i].volume;
                }
            }
        }
    }

    [Serializable]
    public struct StyleSettings
    {
        public int styleIndex;

        public string autoChangeVersion;
    }

    [Serializable]
    public struct StyleSettingsPlatform
    {
        public RuntimePlatform platform;

        public StyleSettings settings;
    }

    private enum FadeType
    {
        Up = 0,
        Down = 1
    }

    public static MenuStyles Instance;

    public MenuStyle[] styles;

    [Space]
    public StyleSettings StyleDefault;

    public StyleSettingsPlatform[] StylePlatforms;

    private StyleSettings currentSettings;

    [Space]
    public SpriteRenderer blackSolid;

    public float fadeTime = 0.25f;

    private Coroutine fadeRoutine;

    //public MenuStyleTitle title;

    private bool subscribed;

    public int CurrentStyle => currentSettings.styleIndex;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        currentSettings = StyleDefault;
        StyleSettingsPlatform[] stylePlatforms = StylePlatforms;
        for (int i = 0; i < stylePlatforms.Length; i++)
        {
            StyleSettingsPlatform styleSettingsPlatform = stylePlatforms[i];
            if (Application.platform == styleSettingsPlatform.platform)
            {
                currentSettings = styleSettingsPlatform.settings;
                break;
            }
        }
        LoadStyle(force: true, fade: false);
        UnlockFromAchievements();
    }

    public void LoadStyle(bool force, bool fade)
    {
        bool setAsCurrentStyle = false;
        /*if (Platform.Current.SharedData.HasKey("lastVersion") && Platform.Current.SharedData.GetString("lastVersion", "0.0.0.0") == currentSettings.autoChangeVersion)
        {
            int @int = Platform.Current.EncryptedSharedData.GetInt("menuStyle", currentSettings.styleIndex);
            if (currentSettings.styleIndex != @int)
            {
                currentSettings.styleIndex = @int;
                setAsCurrentStyle = true;
            }
        }
        string key = "unlockedMenuStyle";
        if (Platform.Current.SharedData.HasKey(key))
        {
            string @string = Platform.Current.SharedData.GetString(key, "");
            Platform.Current.SharedData.DeleteKey(key);
            for (int i = 0; i < styles.Length; i++)
            {
                if (styles[i].unlockKey == @string && styles[i].IsAvailable)
                {
                    int num = i;
                    if (currentSettings.styleIndex != num)
                    {
                        currentSettings.styleIndex = num;
                        setAsCurrentStyle = true;
                    }
                    break;
                }
            }
        }*/
        if (setAsCurrentStyle || force)
        {
            SetStyle(currentSettings.styleIndex, fade);
        }
    }

    private void OnDestroy()
    {
        /*if (subscribed)
        {
            Platform.AchievementsFetched -= UnlockFromAchievements;
        }*/
    }

    public void SetStyle(int index, bool fade, bool save = true)
    {
        if (index < 0 || index >= styles.Length)
        {
            Debug.LogError("Menu Style \"" + index + "\" is out of bounds.");
            return;
        }
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(SwitchStyle(index, fade, currentSettings.styleIndex));
        currentSettings.styleIndex = index;
        /*if (save)
        {
            Platform.Current.SharedData.SetString("lastVersion", currentSettings.autoChangeVersion);
            Platform.Current.EncryptedSharedData.SetInt("menuStyle", currentSettings.styleIndex);
            Platform.Current.SharedData.Save();
            Platform.Current.EncryptedSharedData.Save();
        }*/
    }

    private IEnumerator SwitchStyle(int index, bool fade, int oldIndex)
    {
        yield return null;
        if ((bool)styles[index].musicSnapshot)
        {
            //GameManager.instance.AudioManager.ApplyMusicSnapshot(styles[index].musicSnapshot, 0f, fade ? (fadeTime * 2f) : 0f);
        }
        AudioSource[] componentsInChildren = styles[oldIndex].styleObject.GetComponentsInChildren<AudioSource>();
        styles[oldIndex].SetInitialAudioVolumes(componentsInChildren);
        yield return StartCoroutine(Fade(oldIndex, FadeType.Down, fade, componentsInChildren));
        UpdateTitle();
        for (int i = 0; i < styles.Length; i++)
        {
            styles[i].styleObject.SetActive(index == i);
        }
        GameCameras instance = GameCameras.instance;
        if ((bool)instance && (bool)instance.colorCorrectionCurves)
        {
            MenuStyle.CameraCurves cameraColorCorrection = styles[index].cameraColorCorrection;
            instance.colorCorrectionCurves.saturation = cameraColorCorrection.saturation;
            instance.colorCorrectionCurves.redChannel = cameraColorCorrection.redChannel;
            instance.colorCorrectionCurves.greenChannel = cameraColorCorrection.greenChannel;
            instance.colorCorrectionCurves.blueChannel = cameraColorCorrection.blueChannel;
        }
        componentsInChildren = styles[index].styleObject.GetComponentsInChildren<AudioSource>();
        styles[index].SetInitialAudioVolumes(componentsInChildren);
        yield return StartCoroutine(Fade(index, FadeType.Up, fade, componentsInChildren));
        fadeRoutine = null;
    }

    private IEnumerator Fade(int styleIndex, FadeType fadeType, bool fade, AudioSource[] audioSources)
    {
        float toAlpha = ((fadeType == FadeType.Down) ? 1 : 0);
        if (!blackSolid)
        {
            yield break;
        }
        Color color = blackSolid.color;
        float startAlpha = color.a;
        if (fade)
        {
            for (float elapsed = 0f; elapsed < fadeTime; elapsed += Time.deltaTime)
            {
                float t = elapsed / fadeTime;
                color.a = Mathf.Lerp(startAlpha, toAlpha, t);
                blackSolid.color = color;
                for (int i = 0; i < audioSources.Length; i++)
                {
                    float num = styles[styleIndex].initialAudioVolumes[i];
                    if (fadeType == FadeType.Down)
                    {
                        audioSources[i].volume = Mathf.Lerp(num, 0f, t);
                    }
                    else
                    {
                        audioSources[i].volume = Mathf.Lerp(0f, num, t);
                    }
                }
                yield return null;
            }
            for (int j = 0; j < audioSources.Length; j++)
            {
                float volume = styles[styleIndex].initialAudioVolumes[j];
                if (fadeType == FadeType.Down)
                {
                    audioSources[j].volume = 0f;
                }
                else
                {
                    audioSources[j].volume = volume;
                }
            }
        }
        color.a = toAlpha;
        blackSolid.color = color;
    }

    public void StopAudio()
    {
        AudioSource[] componentsInChildren = styles[currentSettings.styleIndex].styleObject.GetComponentsInChildren<AudioSource>();
        StartCoroutine(FadeOutAudio(componentsInChildren));
    }

    private IEnumerator FadeOutAudio(AudioSource[] audioSources)
    {
        AudioSource[] array;
        for (float elapsed = 0f; elapsed <= fadeTime; elapsed += Time.deltaTime)
        {
            array = audioSources;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].volume = Mathf.Lerp(0f, 1f, elapsed / fadeTime);
            }
            yield return null;
        }
        array = audioSources;
        for (int i = 0; i < array.Length; i++)
        {
            array[i].volume = 0f;
        }
    }

    public void UnlockFromAchievements()
    {
        MenuStyle[] array = styles;
        foreach (MenuStyle menuStyle in array)
        {
            if (menuStyle.IsAvailable || menuStyle.achievementKeys.Length == 0)
            {
                continue;
            }
            string[] achievementKeys = menuStyle.achievementKeys;
            foreach (string text in achievementKeys)
            {
                /*if (Platform.Current.IsAchievementUnlocked(text).GetValueOrDefault())
                {
                    menuStyle.enabled = true;
                    Platform.Current.EncryptedSharedData.SetInt(menuStyle.unlockKey, 1);
                    Debug.Log("Unlocked menu style: " + menuStyle.displayName + " with achievement: " + text);
                    break;
                }*/
            }
        }
    }

    public void UpdateTitle()
    {
        /*if ((bool)title)
        {
            title.SetTitle(styles[CurrentStyle].titleIndex);
        }*/
    }
}
