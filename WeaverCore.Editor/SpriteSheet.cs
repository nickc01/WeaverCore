using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeaverCore.DataTypes
{
	[Serializable]
	public class SpriteSheet
	{
		public string SpriteName;

		public bool Flipped;

		public Vector2 Pivot;

		public List<Vector2> UVs;
		public Vector2 WorldSize;
	}


}
