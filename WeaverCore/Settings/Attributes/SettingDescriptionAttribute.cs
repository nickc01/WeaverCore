using System;
using WeaverCore.Utilities;

namespace WeaverCore.Settings
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class SettingDescriptionAttribute : Attribute
    {
        /// <summary>
        /// The description of the settings property
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Applies a description to the settings property
        /// </summary>
        /// <param name="description">The text describing the settings property</param>
        public SettingDescriptionAttribute(string description)
        {
            Description = description;//StringUtilities.AddNewLines(description);
        }
    }
}
