using Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{

    [ShowFeature]
    [CreateAssetMenu(fileName = "Custom Menu Style", menuName = "WeaverCore/Custom Menu Style")]
    public class CustomMenuStyle : ScriptableObject
    {
        delegate (string languageString, GameObject styleGo, int titleIndex, string unlockKey, string[] achievementKeys, MenuStyles.MenuStyle.CameraCurves cameraCurves, AudioMixerSnapshot musicSnapshot) Hook(MenuStyles self);

        delegate void AddMenuStyleDelegate(Func<MenuStyles, (string, GameObject, int, string, string[], MenuStyles.MenuStyle.CameraCurves, AudioMixerSnapshot)> callback);

        static AddMenuStyleDelegate addMenuStyle = null;
        static Type menuStylesHelperType;
        static Assembly sfCoreAssembly;

        static List<CustomMenuStyle> loadedMenus = new List<CustomMenuStyle>();

        [Tooltip("If displayNameLangKey is not set, this will be used instead")]
        public string displayName;

        public string displayNameLangKey;

        public string ID = Guid.NewGuid().ToString();

        public GameObject stylePrefab;

        public bool useCustomCameraCurves = false;

        [Range(0f, 5f)]
        public float cameraSaturation = 1f;

        public AnimationCurve redChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        public AnimationCurve greenChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        public AnimationCurve blueChannel = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(1f, 1f));

        public bool useCustomMusicSnapshot = false;

        public Music.SnapshotType snapshot = Music.SnapshotType.Normal;

        [Tooltip("A custom title to use when the style is activated. If this is not set, then it will use the \"Custom Title ID\" instead")]
        public CustomMenuTitle CustomTitle;

        [SerializeField]
        [Tooltip("The id of the title to use when the style is activated. If \"CustomTitle\" is set, then it will use that instead. Leave as -1 for the default")]
        int customTitleID = -1;

        public int TitleID => CustomTitle != null ? CustomTitle.LogoID : customTitleID;

        [OnRuntimeInit]
        static void OnRuntimeInit()
        {
#if UNITY_EDITOR
            return;
#endif
            if (sfCoreAssembly != null)
            {
                return;
            }

            sfCoreAssembly = ReflectionUtilities.FindLoadedAssembly("SFCore");
            if (sfCoreAssembly != null)
            {
                ModHooks.LanguageGetHook += ModHooks_LanguageGetHook;

                menuStylesHelperType = sfCoreAssembly.GetType("SFCore.MenuStyleHelper");

                if (menuStylesHelperType == null)
                {
                    throw new Exception("Couldn't find SFCore.MenuStyleHelper Type");
                }

                addMenuStyle = ReflectionUtilities.MethodToDelegate<AddMenuStyleDelegate>(menuStylesHelperType.GetMethods(BindingFlags.Public | BindingFlags.Static).FirstOrDefault(m => m.Name == "AddMenuStyle" && m.GetParameters().Length == 1));

                return;
            }
        }

        private static string ModHooks_LanguageGetHook(string key, string sheetTitle, string orig)
        {
            if (sheetTitle == "CP3")
            {
                foreach (var menu in loadedMenus)
                {
                    if (key == menu.ID)
                    {
                        if (!string.IsNullOrEmpty(menu.displayNameLangKey))
                        {
                            return WeaverLanguage.GetString(key, menu.displayNameLangKey, menu.displayName);
                        }
                        else
                        {
                            return menu.displayName;
                        }
                    }
                }
            }

            return orig;
        }

        //Used to load the menus before the registries are enabled.
        [OnWeaverCoreAssemblyFound]
        static void OnWeaverAssemblyLoad(Assembly asm)
        {
#if UNITY_EDITOR
            return;
#endif
            foreach (var bundle in RegistryLoader.LoadBundlesOnly(asm))
            {
                if (!bundle.isStreamedSceneAssetBundle)
                {
                    var registries = bundle.LoadAllAssets<Registry>();
                    foreach (var reg in registries)
                    {
                        foreach (var style in reg.GetFeatures<CustomMenuStyle>())
                        {
                            OnFeatureLoad(style);
                        }
                    }
                }
            }
        }

        [OnFeatureLoad(int.MaxValue)]
        static void OnFeatureLoad(CustomMenuStyle customMenu)
        {
            OnRuntimeInit();
            if (!loadedMenus.Contains(customMenu))
            {
                loadedMenus.Add(customMenu);
                if (addMenuStyle != null)
                {
                    addMenuStyle(styles =>
                    {
                        var instance = GameObject.Instantiate(customMenu.stylePrefab, styles.transform);

                        MenuStyles.MenuStyle.CameraCurves curves = !customMenu.useCustomCameraCurves ? null : new MenuStyles.MenuStyle.CameraCurves
                        {
                            saturation = customMenu.cameraSaturation,
                            redChannel = customMenu.redChannel,
                            greenChannel = customMenu.greenChannel,
                            blueChannel = customMenu.blueChannel
                        };
                        AudioMixerSnapshot snapshot = customMenu.useCustomMusicSnapshot ? Music.GetSnapshot(customMenu.snapshot) : null;

                        return (customMenu.ID, instance, customMenu.TitleID, "", null, curves, snapshot);
                    });
                }
            }
        }
    }
}
