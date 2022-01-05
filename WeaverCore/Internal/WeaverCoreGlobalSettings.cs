using WeaverCore.Assets;
using WeaverCore.Settings;

namespace WeaverCore.Internal
{
    public sealed class WeaverCoreGlobalSettings : GlobalSettings
    {
        public override string TabName => "WeaverCore";


        [SettingField(EnabledType.Both, "Open Debug Tools")]
        public void OpenDebugTools()
        {
            SettingsScreen.Instance.Hide();
            if (!WeaverCoreDebugTools.IsOpen)
            {
                WeaverCoreDebugTools.Open();
            }
        }
    }
}
