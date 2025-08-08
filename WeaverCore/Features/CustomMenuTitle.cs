using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
    [ShowFeature]
    [CreateAssetMenu(fileName = "Custom Menu Title", menuName = "WeaverCore/Custom Menu Title")]
    public class CustomMenuTitle : ScriptableObject
    {
        private delegate int AddLogoDelegate(Sprite logo);

        private static AddLogoDelegate _addLogo;
        private static Type           _titleLogoHelperType;
        private static Assembly       _sfCoreAssembly;

        private static readonly List<CustomMenuTitle> _loadedTitles = new List<CustomMenuTitle>();

        [Tooltip("Sprite that will appear as an extra title-screen logo")]
        public Sprite titleSprite;

        public int LogoID { get; private set; } = -1;

        [OnRuntimeInit]
        private static void OnRuntimeInit()
        {
#if UNITY_EDITOR
            //  Avoid running inside the Unity editor play-mode
            return;
#endif
            if (_sfCoreAssembly != null)
                return;

            _sfCoreAssembly = ReflectionUtilities.FindLoadedAssembly("SFCore");
            if (_sfCoreAssembly == null)
                return;

            _titleLogoHelperType = _sfCoreAssembly.GetType("SFCore.TitleLogoHelper");
            if (_titleLogoHelperType == null)
                throw new Exception("Couldn't locate SFCore.TitleLogoHelper");

            _addLogo = ReflectionUtilities.MethodToDelegate<AddLogoDelegate>(
                           _titleLogoHelperType.GetMethod("AddLogo",
                               BindingFlags.Public | BindingFlags.Static));

            _titleLogoHelperType.GetMethod("unusedInit",
                    BindingFlags.Public | BindingFlags.Static)
                ?.Invoke(null, null);
        }

        [OnWeaverCoreAssemblyFound]
        private static void OnWeaverAssemblyLoad(Assembly asm)
        {
#if UNITY_EDITOR
            return;
#endif
            foreach (var bundle in RegistryLoader.LoadBundlesOnly(asm))
            {
                if (bundle.isStreamedSceneAssetBundle) continue;

                foreach (var reg in bundle.LoadAllAssets<Registry>())
                {
                    foreach (var feature in reg.GetFeatures<CustomMenuTitle>())
                        OnFeatureLoad(feature);
                }
            }
        }
        [OnFeatureLoad]
        private static void OnFeatureLoad(CustomMenuTitle customTitle)
        {
            OnRuntimeInit();

            if (_loadedTitles.Contains(customTitle) || _addLogo == null)
                return;

            _loadedTitles.Add(customTitle);

            if (customTitle.titleSprite != null)
            {
                customTitle.LogoID = _addLogo(customTitle.titleSprite);
            }
            else
            {
                WeaverLog.LogWarning($"{nameof(CustomMenuTitle)} “{customTitle.name}” has no sprite assigned");
            }
        }
    }
}
