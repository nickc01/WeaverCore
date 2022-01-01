using GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverCore.Editor.Compilation;

namespace WeaverCore.Editor
{
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


        public SupportedLanguages CurrentGameLanguage = SupportedLanguages.EN;
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
