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

        /*static MethodInfo addMenuStyleMethod;
        static Type cameraCurvesType;*/

        /*static IList _queue;

        static FieldInfo cameraCurves_saturation;
        static FieldInfo cameraCurves_redChannel;
        static FieldInfo cameraCurves_greenChannel;
        static FieldInfo cameraCurves_blueChannel;

        static Type menuStylesType;
        static FieldInfo menuStylesInstanceField;

        static FieldInfo stylesField;
        static Type styleType;
        static FieldInfo style_styleObject;*/

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

                //menuStylesType = typeof(CameraController).Assembly.GetType("MenuStyles");

                return;

                /*menuStylesInstanceField = menuStylesType.GetField("Instance");

                _queue = menuStylesHelperType.GetField("_queue", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as IList;

                var addCustomMenuStyles = menuStylesHelperType.GetMethod("AddCustomStyles", BindingFlags.NonPublic | BindingFlags.Static);

                var prefix = typeof(CustomMenuStyle).GetMethod(nameof(AddCustomStylesPrefix), BindingFlags.NonPublic | BindingFlags.Static);

                var postfix = typeof(CustomMenuStyle).GetMethod(nameof(AddCustomStylesPostfix), BindingFlags.NonPublic | BindingFlags.Static);

                patcher.Patch(addCustomMenuStyles, prefix, postfix);*/
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

        /*static int addedElementCount = 0;

        static bool AddCustomStylesPrefix(MonoBehaviour self)
        {
            WeaverLog.Log("ADD CUSTOM STYLES PREFIX");
            //var defaultStyle = GetStyleAtIndex(self, 0);

            //originalQueueLength = _queue.Count;
            addedElementCount = 0;
            foreach (var m in loadedMenus)
            {
                var instance = GameObject.Instantiate(m.stylePrefab, self.transform);

                object cameraCurves = m.useCustomCameraCurves ? CreateCameraCurve(m.cameraSaturation, m.redChannel, m.greenChannel, m.blueChannel) : null;
                AudioMixerSnapshot snapshot = m.useCustomMusicSnapshot ? Music.GetSnapshot(m.snapshot) : null;

                _queue.Add(CreateMenuStyleTuple(m.ID, instance, -1, "", null, cameraCurves, snapshot));

                addedElementCount++;
            }

            return true;
        }

        static void AddCustomStylesPostfix(MonoBehaviour self)
        {
            WeaverLog.Log("FINDING FEATURES");

            WeaverLog.Log("ADD CUSTOM STYLES POSTFIX");
            for (int i = 0; i < addedElementCount; i++)
            {
                _queue.RemoveAt(_queue.Count - 1);
            }
        }*/

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

        [OnFeatureLoad]
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

                        //object cameraCurves = m.useCustomCameraCurves ? CreateCameraCurve(customMenu.cameraSaturation, customMenu.redChannel, customMenu.greenChannel, customMenu.blueChannel) : null;
                        MenuStyles.MenuStyle.CameraCurves curves = !customMenu.useCustomCameraCurves ? null : new MenuStyles.MenuStyle.CameraCurves
                        {
                            saturation = customMenu.cameraSaturation,
                            redChannel = customMenu.redChannel,
                            greenChannel = customMenu.greenChannel,
                            blueChannel = customMenu.blueChannel
                        };
                        AudioMixerSnapshot snapshot = customMenu.useCustomMusicSnapshot ? Music.GetSnapshot(customMenu.snapshot) : null;

                        return (customMenu.ID, instance, -1, "", null, curves, snapshot);
                    });
                }
            }
        }

        /*static void AddMenuStyle(CustomMenuStyle customMenu)
        {
            if (Initialization.Environment == Enums.RunningState.Editor)
            {
                return;
            }

            if (sfCoreAssembly == null)
            {
                sfCoreAssembly = ReflectionUtilities.FindLoadedAssembly("SFCore");
                if (sfCoreAssembly == null)
                {
                    throw new System.Exception("Attempting to add a custom menu style without SFCore installed. Install SFCore to fix this issue");
                }

                menuStylesHelperType = sfCoreAssembly.GetType("SFCore.MenuStyleHelper");

                if (menuStylesHelperType == null)
                {
                    throw new System.Exception("Unable to find the MenuStyleHelper class");
                }

                cameraCurvesType = menuStylesHelperType.GetNestedType("CameraCurves");

                if (cameraCurvesType == null)
                {
                    throw new System.Exception("Unable to find the CameraCurves class");
                }

                _queue = menuStylesHelperType.GetField("_queue", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null) as IList;

                cameraCurves_saturation = cameraCurvesType.GetField("saturation");
                cameraCurves_redChannel = cameraCurvesType.GetField("redChannel");
                cameraCurves_greenChannel = cameraCurvesType.GetField("greenChannel");
                cameraCurves_blueChannel = cameraCurvesType.GetField("blueChannel");

                menuStylesType = typeof(CameraController).Assembly.GetType("MenuStyles");

                addMenuStyleMethod = menuStylesHelperType.GetMethods().Where(m => m.Name == "AddMenuStyle" && m.GetParameters().Length > 2).FirstOrDefault();

                if (addMenuStyleMethod == null)
                {
                    throw new System.Exception("Unable to find the AddMenuStyle method on the MenuStyleHelper class");
                }
            }

        }*/

        /*static object CreateCameraCurve(float saturation, AnimationCurve redChannel, AnimationCurve greenChannel, AnimationCurve blueChannel)
        {
            if (cameraCurvesType == null)
            {
                cameraCurvesType = menuStylesHelperType.GetNestedType("CameraCurves");
                cameraCurves_saturation = cameraCurvesType.GetField("saturation");
                cameraCurves_redChannel = cameraCurvesType.GetField("redChannel");
                cameraCurves_greenChannel = cameraCurvesType.GetField("greenChannel");
                cameraCurves_blueChannel = cameraCurvesType.GetField("blueChannel");
            }

            var instance = Activator.CreateInstance(cameraCurvesType);

            cameraCurves_saturation.SetValue(instance, saturation);
            cameraCurves_redChannel.SetValue(instance, redChannel);
            cameraCurves_greenChannel.SetValue(instance, greenChannel);
            cameraCurves_blueChannel.SetValue(instance, blueChannel);

            return instance;
        }*/

        /*static FieldInfo style_enabled;
        static FieldInfo style_displayName;

        static FieldInfo style_cameraColorCorrection;
        static FieldInfo style_musicSnapshot;
        static FieldInfo style_titleIndex;
        static FieldInfo style_unlockKey;
        static FieldInfo style_achievementKeys;
        static FieldInfo style_initialAudioVolumes;

        static object CreateNewMenuStyle(bool enabled, string displayName, GameObject styleObject, object colorCorrection, AudioMixerSnapshot musicSnapshot, int titleIndex, string unlockKey, string[] achievementKeys, float[] initialAudioVolumes)
        {
            InitStyleGetters();

            var instance = Activator.CreateInstance(styleType);

            style_enabled.SetValue(instance, enabled);
            style_displayName.SetValue(instance, displayName);
            style_styleObject.SetValue(instance, styleObject);
            style_cameraColorCorrection.SetValue(instance, colorCorrection);
            style_musicSnapshot.SetValue(instance, musicSnapshot);
            style_titleIndex.SetValue(instance, titleIndex);
            style_unlockKey.SetValue(instance, unlockKey);
            style_achievementKeys.SetValue(instance, achievementKeys);
            style_initialAudioVolumes.SetValue(instance, initialAudioVolumes);

            return instance;
        }

        internal static object GetMenuStylesInstance()
        {
            return menuStylesInstanceField.GetValue(null);
        }

        static Type tupleType;

        static FieldInfo[] tupleFields;

        static object CreateMenuStyleTuple(string languageString, GameObject styleGo, int titleIndex, string unlockKey, string[] achivementKeys, object cameraCurves, AudioMixerSnapshot musicSnapshot)
        {
            InitTupleFields();

            var instance = Activator.CreateInstance(tupleType);
            tupleFields[0].SetValue(instance, languageString);
            tupleFields[1].SetValue(instance, styleGo);
            tupleFields[2].SetValue(instance, titleIndex);
            tupleFields[3].SetValue(instance, unlockKey);
            tupleFields[4].SetValue(instance, achivementKeys);
            tupleFields[5].SetValue(instance, cameraCurves);
            tupleFields[6].SetValue(instance, musicSnapshot);

            return instance;
        }

        static object GetStyleAtIndex(object stylesObject, int index)
        {
            if (stylesField == null)
            {
                stylesField = menuStylesType.GetField("styles");
            }

            var array = stylesField.GetValue(stylesObject) as Array;

            return array.GetValue(index);
        }

        static void InitTupleFields()
        {
            if (tupleType == null)
            {
                tupleType = _queue.GetType().GenericTypeArguments[0];

                tupleFields = tupleType.GetFields();
            }
        }

        static void InitStyleGetters()
        {
            if (styleType == null)
            {
                styleType = menuStylesType.GetNestedType("MenuStyle");
            }

            if (style_enabled == null)
            {
                style_enabled = styleType.GetField("enabled");
                style_displayName = styleType.GetField("displayName");
                style_styleObject = styleType.GetField("styleObject");
                style_musicSnapshot = styleType.GetField("musicSnapshot");
                style_cameraColorCorrection = styleType.GetField("cameraColorCorrection");
                style_titleIndex = styleType.GetField("titleIndex");
                style_unlockKey = styleType.GetField("unlockKey");
                style_achievementKeys = styleType.GetField("achivementKeys");
            }
        }*/
    }
}
