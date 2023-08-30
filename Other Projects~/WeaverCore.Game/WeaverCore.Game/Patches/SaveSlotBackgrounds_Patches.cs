using GlobalEnums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
    public static class SaveSlotBackgrounds_Patches
    {
        [OnInit]
        static void OnInit()
        {
            On.SaveSlotBackgrounds.GetBackground_MapZone += SaveSlotBackgrounds_GetBackground_MapZone;
            On.SaveSlotBackgrounds.GetBackground_string += SaveSlotBackgrounds_GetBackground_string;
        }

        private static AreaBackground SaveSlotBackgrounds_GetBackground_string(On.SaveSlotBackgrounds.orig_GetBackground_string orig, SaveSlotBackgrounds self, string areaName)
        {
            foreach (var zone in MapZoneUtilities.GetCustomMapZones())
            {
                if (zone.GetInternalName() == areaName)
                {
                    return new AreaBackground
                    {
                        areaName = (MapZone)zone.MapZoneID,
                        backgroundImage = zone.MapZoneBackgroundImage
                    };
                }
            }

            return orig(self, areaName);
        }

        private static AreaBackground SaveSlotBackgrounds_GetBackground_MapZone(On.SaveSlotBackgrounds.orig_GetBackground_MapZone orig, SaveSlotBackgrounds self, GlobalEnums.MapZone mapZone)
        {
            foreach (var zone in MapZoneUtilities.GetCustomMapZones())
            {
                if (zone.MapZoneID == (int)mapZone)
                {
                    return new AreaBackground
                    {
                        areaName = mapZone,
                        backgroundImage = zone.MapZoneBackgroundImage
                    };
                }
            }

            return orig(self, mapZone);
        }
    }
}
