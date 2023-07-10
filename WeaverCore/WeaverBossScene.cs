using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore
{
    [CreateAssetMenu(fileName = "New Boss Scene", menuName = "WeaverCore/Boss Scene")]
    public class WeaverBossScene : BossScene, ISerializationCallbackReceiver
	{
        [OnHarmonyPatch]
        static void Patch(HarmonyPatcher patcher)
        {
            var orig = typeof(BossScene).GetMethod(nameof(IsUnlockedSelf));

            var post = typeof(WeaverBossScene).GetMethod(nameof(IsUnlockedSelf_Postfix), BindingFlags.Static | BindingFlags.NonPublic);

            patcher.Patch(orig, null, post);
        }

        static bool CompareInt(int a, WeaverBossTest.IntTest.Comparison comparison, int b)
        {
            switch (comparison)
            {
                case WeaverBossTest.IntTest.Comparison.Equal:
                    return b == a;
                case WeaverBossTest.IntTest.Comparison.NotEqual:
                    return b != a;
                case WeaverBossTest.IntTest.Comparison.MoreThan:
                    return b > a;
                case WeaverBossTest.IntTest.Comparison.LessThan:
                    return b < a;
                default:
                    return false;
            }
        }

        static void IsUnlockedSelf_Postfix(BossScene __instance, BossSceneCheckSource source, ref bool __result)
        {
            //WeaverLog.Log("RESULT = " + __result);
            if (__instance is WeaverBossScene wbs)
            {
                bool weaverUnlocked = true;

                foreach (var test in wbs.WeaverBossTests.boolTests)
                {
                    if (!test.settings.TryGetFieldValue<bool>(test.settingsBoolField,out var result) || result != test.value)
                    {
                        weaverUnlocked = false;
                        break;
                    }
                }

                if (weaverUnlocked)
                {
                    foreach (var test in wbs.WeaverBossTests.intTests)
                    {
                        if (!test.settings.TryGetFieldValue<int>(test.settingsIntField, out var result) || !CompareInt(test.value, test.comparison, result))
                        {
                            weaverUnlocked = false;
                            break;
                        }
                    }
                }

                __result = __result && weaverUnlocked;
            }
        }

        [Serializable]
        class DataContainer
        {
            public BossTest[] tests;
        }

        [Serializable]
        enum TestType
        {
            Int,
            Bool
        }

        [Serializable]
        public class WeaverBossTest
        {
            [Serializable]
            public struct BoolTest
            {
                public SaveSpecificSettings settings;

                public string settingsBoolField;

                public bool value;
            }

            [Serializable]
            public struct IntTest
            {
                public SaveSpecificSettings settings;

                public enum Comparison
                {
                    Equal,
                    NotEqual,
                    MoreThan,
                    LessThan
                }

                public string settingsIntField;

                public int value;

                public Comparison comparison;
            }

            public BoolTest[] boolTests;

            public IntTest[] intTests;
        }

        [SerializeField]
        WeaverBossTest weaverBossTests;

        [SerializeField]
        [HideInInspector]
        int settingsCount = 0;

        [SerializeField]
        [HideInInspector]
        string[] settingsArray;

        [SerializeField]
        [HideInInspector]
        string[] fieldArray;

        [SerializeField]
        [HideInInspector]
        TestType[] testTypeArray;

        [SerializeField]
        [HideInInspector]
        bool[] boolValueArray;

        [SerializeField]
        [HideInInspector]
        int[] intValueArray;

        [SerializeField]
        [HideInInspector]
        WeaverBossTest.IntTest.Comparison[] intComparisonArray;




        [SerializeField]
        [HideInInspector]
        string bossTestData;

        [NonSerialized]
        bool initialized = false;

        public WeaverBossTest WeaverBossTests
        {
            get
            {
                if (initialized)
                {
                    return weaverBossTests;
                }
                initialized = true;

                if (Initialization.Environment == Enums.RunningState.Editor)
                {
                    return weaverBossTests;
                }

                var boolSize = testTypeArray.Count(t => t == TestType.Bool);

                var intSize = settingsCount - boolSize;

                weaverBossTests = new WeaverBossTest
                {
                    intTests = new WeaverBossTest.IntTest[intSize],
                    boolTests = new WeaverBossTest.BoolTest[boolSize]
                };

                for (int i = 0; i < boolSize; i++)
                {
                    weaverBossTests.boolTests[i] = new WeaverBossTest.BoolTest
                    {
                        settings = Registry.GetAllFeatures<SaveSpecificSettings>(s => s.GetType().FullName == settingsArray[i]).FirstOrDefault(),//settingsArray[i],
                        settingsBoolField = fieldArray[i],
                        value = boolValueArray[i]
                    };
                }

                for (int i = 0; i < intSize; i++)
                {
                    weaverBossTests.intTests[i] = new WeaverBossTest.IntTest
                    {
                        settings = Registry.GetAllFeatures<SaveSpecificSettings>(s => s.GetType().FullName == settingsArray[boolSize + i]).FirstOrDefault(),
                        settingsIntField = fieldArray[boolSize + i],
                        comparison = intComparisonArray[i],
                        value = intValueArray[i]
                    };
                }

                return weaverBossTests;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            bossTestData = JsonUtility.ToJson(new DataContainer() { tests = bossTests});

            settingsCount = weaverBossTests.intTests.Length + weaverBossTests.boolTests.Length;

            settingsArray = new string[settingsCount];
            fieldArray = new string[settingsCount];
            testTypeArray = new TestType[settingsCount];

            boolValueArray = new bool[weaverBossTests.boolTests.Length];
            intValueArray = new int[weaverBossTests.intTests.Length];

            intComparisonArray = new WeaverBossTest.IntTest.Comparison[weaverBossTests.intTests.Length];

            for (int i = 0; i < weaverBossTests.boolTests.Length; i++)
            {
                //settingsArray[i] = weaverBossTests.boolTests[i].settings;

                if (weaverBossTests.boolTests[i].settings == null)
                {
                    settingsArray[i] = null;
                }
                else
                {
                    settingsArray[i] = weaverBossTests.boolTests[i].settings.GetType().FullName;
                }
                fieldArray[i] = weaverBossTests.boolTests[i].settingsBoolField;
                testTypeArray[i] = TestType.Bool;

                boolValueArray[i] = weaverBossTests.boolTests[i].value;
            }

            int startIndex = weaverBossTests.boolTests.Length;

            for (int i = 0; i < weaverBossTests.intTests.Length; i++)
            {
                settingsArray[startIndex + i] = weaverBossTests.boolTests[i].settings.GetType().FullName;
                fieldArray[startIndex + i] = weaverBossTests.intTests[i].settingsIntField;
                testTypeArray[startIndex + i] = TestType.Int;
                intComparisonArray[i] = weaverBossTests.intTests[i].comparison;

                intValueArray[i] = weaverBossTests.intTests[i].value;
            }


#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
#if !UNITY_EDITOR
            var tests = JsonUtility.FromJson<DataContainer>(bossTestData);
            bossTests = tests.tests;
            WeaverLog.Log("Tests = " + JsonUtility.ToJson(tests));
            
#endif
            //JsonUtility.FromJsonOverwrite(bossTestData,)
        }
    }
}
