using System;
using System.Runtime.CompilerServices;
using UnityEngine;

// ReSharper disable file UnusedMember.Global

namespace Modding
{
    public class ModSettings
    {
        /*[SerializeField] public SerializableBoolDictionary BoolValues;

        [SerializeField] public SerializableFloatDictionary FloatValues;

        [SerializeField] public SerializableIntDictionary IntValues;

        [SerializeField] public SerializableStringDictionary StringValues;

        protected ModSettings()
        {
            StringValues = new SerializableStringDictionary();
            IntValues = new SerializableIntDictionary();
            BoolValues = new SerializableBoolDictionary();
            FloatValues = new SerializableFloatDictionary();
        }

        public void SetSettings(ModSettings incomingSettings)
        {
            StringValues = incomingSettings.StringValues;
            IntValues = incomingSettings.IntValues;
            BoolValues = incomingSettings.BoolValues;
            FloatValues = incomingSettings.FloatValues;
        }

        public string GetString(string defaultValue = null, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return null;
            }

            return StringValues.ContainsKey(name) ? StringValues[name] : defaultValue;
        }

        public void SetString(string value, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return;
            }

            if (StringValues.ContainsKey(name))
            {
                StringValues[name] = value;
            }
            else
            {
                StringValues.Add(name, value);
            }
        }

        public int GetInt(int? defaultValue = null, [CallerMemberName] string name = null)
        {
            if (name == null)
            {
                return 0;
            }

            return IntValues.ContainsKey(name) ? IntValues[name] : defaultValue ?? 0;
        }

        public void SetInt(int value, string name = null)
        {
            if (name == null)
            {
                return;
            }

            if (IntValues.ContainsKey(name))
            {
                IntValues[name] = value;
            }
            else
            {
                IntValues.Add(name, value);
            }
        }


        public bool GetBool(bool? defaultValue = null, string name = null)
        {
            if (name == null)
            {
                return false;
            }

            return BoolValues.ContainsKey(name) ? BoolValues[name] : defaultValue ?? false;
        }

        /// <summary>
        ///     Handles setting of a value in the BoolValues Dictionary
        /// </summary>
        /// <param name="value">Value to Set</param>
        /// <param name="name">Compiler Generated Name of the Property</param>
        public void SetBool(bool value, string name = null)
        {
            if (name == null)
            {
                return;
            }

            if (BoolValues.ContainsKey(name))
            {
                BoolValues[name] = value;
            }
            else
            {
                BoolValues.Add(name, value);
            }
        }

        public float GetFloat(float? defaultValue = null, string name = null)
        {
            if (name == null)
            {
                return 0f;
            }

            return FloatValues.ContainsKey(name) ? FloatValues[name] : defaultValue ?? 0f;
        }

        public void SetFloat(float value, string name = null)
        {
            if (name == null)
            {
                return;
            }

            if (FloatValues.ContainsKey(name))
            {
                FloatValues[name] = value;
            }
            else
            {
                FloatValues.Add(name, value);
            }
        }*/
    }
}