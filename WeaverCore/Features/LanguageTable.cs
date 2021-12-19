using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{
    [ShowFeature]
	public class LanguageTable : ScriptableObject
    {
        [Serializable]
        public class Sheet
        {
            public string sheetName = "General";
            public List<Key> Keys;
        }

        [Serializable]
        public class Key
        {
            public string key;
            public string value;
        }


        public List<Sheet> sheets;

        [SerializeField]
        List<string> sheetTitleList;

        [SerializeField]
        List<string> keys;

        [SerializeField]
        List<string> values;

        public string GetString(string sheet, string key, string fallback = null)
        {
            var keyIndex = keys.IndexOf(key);
            if (keyIndex >= 0)
            {
                if (sheetTitleList[keyIndex] == sheet)
                {
                    return values[keyIndex];
                }
                else
                {
                    return fallback;
                }
            }
            else
            {
                return fallback;
            }
        }

        public string GetString(string key, string fallback = null)
        {
            return GetString("General", fallback);
        }

        public bool HasString(string sheet, string key)
        {
            var keyIndex = keys.IndexOf(key);
            if (keyIndex >= 0)
            {
                if (sheetTitleList[keyIndex] == sheet)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public bool HasString(string key)
        {
            return HasString("General", key);
        }
    }
}
