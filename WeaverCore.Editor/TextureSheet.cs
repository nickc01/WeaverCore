using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeaverCore.DataTypes
{
	[Serializable]
	public class TextureSheet
	{
		public int Width;
		public int Height;
		public string TextureName;

		public List<SpriteSheet> Sprites;
	}


}
