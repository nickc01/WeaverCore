using UnityEngine;
using WeaverCore.DataTypes;


namespace WeaverCore.Editor.Structures
{
	public struct SpriteData
	{
		public Texture2D Texture;
		public SpriteSheet Sheet;
		public Vector2Int UVDimensions;
		public float PixelsPerUnit;
		public Rect SpriteCoords;
	}
}
