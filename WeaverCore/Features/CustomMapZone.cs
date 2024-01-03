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
		/// <summary>
		/// The name of the map zone
		/// </summary>
		public string MapZoneName;

		/// <summary>
		/// The unique id of the map zone
		/// </summary>
		public int MapZoneID;

		/// <summary>
		/// The background image used in the save file selection menu
		/// </summary>
		public Sprite MapZoneBackgroundImage;

		public MapZone MapZone => (MapZone)MapZoneID;

		public string GetInternalName()
		{
			return MapZoneName.ToUpper().Replace(" ", "_");
		}
	}
}
