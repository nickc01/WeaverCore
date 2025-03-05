using System.Collections.Generic;

namespace TMPro
{
	public static class TMP_FontUtilities
	{
		private static List<int> k_searchedFontAssets;

		public static TMP_FontAsset SearchForGlyph(TMP_FontAsset font, int character, out TMP_Glyph glyph)
		{
			if (k_searchedFontAssets == null)
			{
				k_searchedFontAssets = new List<int>();
			}
			k_searchedFontAssets.Clear();
			return SearchForGlyphInternal(font, character, out glyph);
		}

		public static TMP_FontAsset SearchForGlyph(List<TMP_FontAsset> fonts, int character, out TMP_Glyph glyph)
		{
			return SearchForGlyphInternal(fonts, character, out glyph);
		}

		private static TMP_FontAsset SearchForGlyphInternal(TMP_FontAsset font, int character, out TMP_Glyph glyph)
		{
			glyph = null;
			if (font == null)
			{
				return null;
			}
			if (font.characterDictionary.TryGetValue(character, out glyph))
			{
				return font;
			}
			if (font.fallbackFontAssets != null && font.fallbackFontAssets.Count > 0)
			{
				for (int i = 0; i < font.fallbackFontAssets.Count; i++)
				{
					if (glyph != null)
					{
						break;
					}
					TMP_FontAsset tMP_FontAsset = font.fallbackFontAssets[i];
					int instanceID = tMP_FontAsset.GetInstanceID();
					if (!(tMP_FontAsset == null) && !k_searchedFontAssets.Contains(instanceID))
					{
						k_searchedFontAssets.Add(instanceID);
						tMP_FontAsset = SearchForGlyphInternal(tMP_FontAsset, character, out glyph);
						if (tMP_FontAsset != null)
						{
							return tMP_FontAsset;
						}
					}
				}
			}
			return null;
		}

		private static TMP_FontAsset SearchForGlyphInternal(List<TMP_FontAsset> fonts, int character, out TMP_Glyph glyph)
		{
			glyph = null;
			if (fonts != null && fonts.Count > 0)
			{
				for (int i = 0; i < fonts.Count; i++)
				{
					TMP_FontAsset tMP_FontAsset = SearchForGlyphInternal(fonts[i], character, out glyph);
					if (tMP_FontAsset != null)
					{
						return tMP_FontAsset;
					}
				}
			}
			return null;
		}
	}
}
