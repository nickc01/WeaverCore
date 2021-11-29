using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Editor.Settings;
using WeaverCore.Implementations;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Implementations
{
    class GlobalSettingsPanel : Panel
    {
        public GlobalSettings ModSettings { get; private set; }

        public static GlobalSettingsPanel Create(GlobalSettings settings)
        {
            var instance = ScriptableObject.CreateInstance<GlobalSettingsPanel>();
            instance.ModSettings = settings;
            return instance;
        }

        protected internal override void OnPanelOpen()
        {
            foreach (var (member, settings) in GetSerializableMembers(ModSettings))
            {
                var name = settings != null && settings.DisplayName != null ? settings.DisplayName : StringUtilities.Prettify(member.Name);
                var description = SettingsScreen.GetDescriptionOfMember(member);
                AddMemberElement(member, ModSettings, name, description);
            }
            ModSettings.OnOpen();
        }

        protected internal override void OnPanelClose()
        {
            ModSettings.OnClose();
        }

        public override string TabName => ModSettings.Title;

        public override void LoadSettings()
        {
            GlobalSettings_I.Impl.LoadSettings(ModSettings);
            ModSettings.OnLoad();
        }

        public override void SaveSettings()
        {
            ModSettings.OnSave();
            GlobalSettings_I.Impl.SaveSettings(ModSettings);
        }
    }



    public class E_GlobalSettings_I : GlobalSettings_I
    {
        static HashSet<GlobalSettings> loadedSettings = new HashSet<GlobalSettings>();

        [OnFeatureLoad]
        static void SettingsLoaded(GlobalSettings settings)
        {
            if (loadedSettings.Add(settings))
            {
                SettingsScreen.RegisterPanel(GlobalSettingsPanel.Create(settings));
            }
        }


        /*public override void Destroy(GlobalSettings settings)
        {
            
        }

        public override void Init(GlobalSettings settings)
        {
            
        }*/

        public override void LoadSettings(GlobalSettings settings)
        {
            //No Loading needs to be done in the editor
        }

        public override void SaveSettings(GlobalSettings settings)
        {
            //No saving needs to be done in the editor
        }
    }
}
