using UnityEngine;
using WeaverCore.DataTypes;


namespace WeaverCore.Editor.Structures
{
	public struct SpriteLocation
	{
		public SpriteSheet Sprite;
		public string FileLocation;
		public int UVWidth;
		public int UVHeight;
		public Vector2Int SpriteDimensions;
	}
}
