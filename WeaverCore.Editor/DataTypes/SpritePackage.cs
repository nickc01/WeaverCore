using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore.Editor.DataTypes
{
	public class SpritePackage
	{
		public readonly Texture2D Texture;
		readonly Dictionary<int, Sprite> SpriteIDs = new Dictionary<int, Sprite>();

		public SpritePackage(Texture2D texture)
		{
			Texture = texture;
		}


		public bool AddSprite(int id, Sprite sprite)
		{
			if (!SpriteIDs.ContainsKey(id))
			{
				SpriteIDs.Add(id, sprite);
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool RemoveSprite(int id)
		{
			if (SpriteIDs.ContainsKey(id))
			{
				SpriteIDs.Remove(id);
				return true;
			}
			else
			{
				return false;
			}
		}

		public Sprite GetSprite(int id)
		{
			if (SpriteIDs.ContainsKey(id))
			{
				return SpriteIDs[id];
			}
			else
			{
				return null;
			}
		}

		public IEnumerable<Sprite> GetAllSprites()
		{
			foreach (var spritePair in SpriteIDs)
			{
				yield return spritePair.Value;
			}
		}

		public IEnumerable<KeyValuePair<int,Sprite>> GetSpritesWithIDs()
		{
			foreach (var spritePair in SpriteIDs)
			{
				yield return spritePair;
			}
		}

		public IEnumerable<int> GetAllIDs()
		{
			foreach (var spritePair in SpriteIDs)
			{
				yield return spritePair.Key;
			}
		}
	}
}
