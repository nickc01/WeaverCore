using System;
using UnityEngine;
using WeaverCore.Settings;

namespace WeaverCore.Attributes
{
    /// <summary>
    /// Can be applied to a string field to allow the user to select a field from a SaveSpecificSettings object via a list
    /// </summary>
    public sealed class SaveSpecificFieldName : PropertyAttribute
    {
        /// <summary>
        /// The expected field type for this save field. Used to filter out fields that don't cast to the ExpectedFieldType
        /// </summary>
        public readonly Type ExpectedFieldType;

        /// <summary>
        /// The name of the SaveSpecificSettings field within the Monobehaviour.
        /// </summary>
        public readonly string SaveSettingsName;

        /// <summary>
        /// Can be applied to a string field to allow the user to select a field from a SaveSpecificSettings object via a list
        /// </summary>
        /// <param name="expectedFieldType">The expected field type for this save field. Used to filter out fields that don't cast to the ExpectedFieldType</param>
        /// <param name="saveSettingsName">The name of the SaveSpecificSettings field within the Monobehaviour.</param>
        public SaveSpecificFieldName(Type expectedFieldType, string saveSettingsName)
        {
            ExpectedFieldType = expectedFieldType;
            SaveSettingsName = saveSettingsName;
        }
    }
}
