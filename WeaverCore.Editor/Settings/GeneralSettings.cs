using GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverCore.Editor.Compilation;

namespace WeaverCore.Editor
{
    /// <summary>
    /// Contains general editor settings
    /// </summary>
    [Serializable]
    public class GeneralSettings
    {
        static GeneralSettings _instance;
        public static GeneralSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    Load();
                }
                return _instance;
            }
        }

        /// <summary>
        /// The current language of the unity editor
        /// </summary>
        public SupportedLanguages CurrentGameLanguage = SupportedLanguages.EN;

        /// <summary>
        /// Disables the Freezing effect that occurs when taking damage or parrying an attack in the editor
        /// </summary>
        public bool DisableGameFreezing = false;

        public static void Load()
        {
            if (PersistentData.TryGetData(out GeneralSettings settings))
            {
                _instance = settings;
            }
            else
            {
                _instance = new GeneralSettings();
            }
        }

        public static void Save()
        {
            if (Instance != null)
            {
                PersistentData.StoreData(Instance);
                PersistentData.SaveData();
            }
        }
    }
}
