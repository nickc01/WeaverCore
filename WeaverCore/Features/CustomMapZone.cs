using GlobalEnums;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Features
{
    /// <summary>
    /// Used to create custom map zones
    /// </summary>
    [ShowFeature]
	public class CustomMapZone : ScriptableObject
	{
		public string MapZoneName;
		public int MapZoneID;
		public Sprite MapZoneBackgroundImage;

		public MapZone MapZone => (MapZone)MapZoneID;

		public string GetInternalName()
		{
			return MapZoneName.ToUpper().Replace(" ", "_");
		}
	}
}
