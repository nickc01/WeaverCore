using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

namespace WeaverCore.Helpers
{
	public class TextureSheet
	{
		public int Width;
		public int Height;
		public string TextureName;

		public List<SpriteSheet> Sprites;
	}

	public class SpriteSheet
	{
		public string SpriteName;

		public bool Flipped;
		//public float PixelsPerUnit;
		public Vector2 Pivot;

		public List<Vector2> UVs;
		public Vector2 WorldSize;

		//public float BottomLeftX;
		//public float BottomLeftY;

		//public float TopRightX;
		//public float TopRightY;
	}


}
