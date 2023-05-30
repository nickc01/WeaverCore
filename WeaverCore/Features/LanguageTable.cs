using GlobalEnums;
using Language;
using System;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{


    /// <summary>
    /// A table thats used for storing translations. 
    /// </summary>
    [ShowFeature]
    [CreateAssetMenu(fileName = "LanguageTable", menuName = "WeaverCore/Language Table")]
    public class LanguageTable : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public class Entry
        {
            public string key;
            public string value;
        }

        [SerializeField]
        [Tooltip("The sheet title of this language table. This determines the \"Group\" that the language entries fall under")]
        string sheetTitle = "General";

        [SerializeField]
        [Tooltip("The language this table will be loaded under")]
        SupportedLanguages language = SupportedLanguages.EN;

        public string SheetTitle => sheetTitle;
        public SupportedLanguages Language => language;

        [SerializeField]
        List<Entry> entries = new List<Entry>();

        [SerializeField]
        [HideInInspector]
        List<string> _entry_Keys = new List<string>();

        [SerializeField]
        [HideInInspector]
        List<string> _entry_Values = new List<string>();

        /// <summary>
        /// Gets a language string based on a key
        /// </summary>
        public string GetString(string key, string fallback = null)
        {
            var keyIndex = _entry_Keys.IndexOf(key);
            if (keyIndex >= 0)
            {
                return _entry_Values[keyIndex];
            }
            else
            {
                return fallback;
            }
        }

        /// <summary>
        /// Does this language table have the specified key
        /// </summary>
        /// <param name="key">The key to check for</param>
        public bool HasString(string key)
        {
            return _entry_Keys.IndexOf(key) >= 0;
        }


        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
#if UNITY_EDITOR
            _entry_Keys.Clear();
            _entry_Values.Clear();

            _entry_Keys.Capacity = entries.Count;
            _entry_Values.Capacity = entries.Count;

            for (int i = 0; i < entries.Count; i++)
            {
                _entry_Keys.Add(entries[i].key);
                _entry_Values.Add(entries[i].value);
            }
#endif
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            
        }

    }
}
