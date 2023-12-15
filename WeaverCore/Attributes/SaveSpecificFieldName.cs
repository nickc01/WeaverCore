using System;
using UnityEngine;
using WeaverCore.Settings;

namespace WeaverCore.Attributes
{
    public sealed class SaveSpecificFieldName : PropertyAttribute
    {
        public readonly Type ExpectedFieldType;
        public readonly string SaveSettingsName;

        public SaveSpecificFieldName(Type expectedFieldType, string saveSettingsName)
        {
            ExpectedFieldType = expectedFieldType;
            SaveSettingsName = saveSettingsName;
        }
    }
}
