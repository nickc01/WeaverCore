using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Assets
{
	[CreateAssetMenu(fileName = "Font Asset Container", menuName = "Font Asset Container")]
    public class FontAssetContainer : ScriptableObject
	{
		public static List<(string, string)> sourceDestPairs = new List<(string, string)>()
		{
			/*("trajan_bold_tmpro", "Trajan Pro Regular SDF Thin Outline"),
			("trajan_bold_tmpro", "Trajan Pro Regular SDF Thick Outline"),*/
			("TrajanPro-Bold SDF", "TrajanPro-Bold SDF"),
			("russian_body", "Russian SDF"),
			("russian_body_fallback", "Russian Fallback SDF"),
			("chinese_body", "Chinese SDF"),
			("chinese_body_bold", "Chinese Bold SDF"),
			("perpetua_tmpro", "Perpetua Fallback SDF"),
            ("Perpetua-SDF", "Perpetua SDF"),
            ("japanese_body", "Japanese SDF"),
			("korean_body", "Korean SDF")
        };

		public static List<string> RemovedTextures = new List<string>()
		{
			"chinese_body Atlas R Channeled",
            "chinese_body_bold Atlas R Channeled",
            "japanese_body Atlas R Channeled",
            "korean_body Atlas R Channeled",
            "TrajanPro-Bold SDF Atlas - R Channeled",
            "perpetua_tmpro Atlas R Channeled",
            "russian_body Atlas - R Channeled",
            "Perpetua-SDF Atlas - R Channeled",
            "russian_body_fallback Atlas - R Channeled"
        };


		public List<TMP_FontAsset> WeaverCoreFonts;

		public static HashSet<TMP_FontAsset> InGameFonts;

		public static FontAssetContainer Load()
		{
			return WeaverAssets.LoadWeaverAsset<FontAssetContainer>("Font Asset Container");
		}

		public void ReplaceFonts()
		{
			foreach (var pair in sourceDestPairs)
			{
				var source = InGameFonts.FirstOrDefault(f => f.name == pair.Item1);
				var dest = WeaverCoreFonts.FirstOrDefault(f => f.name == pair.Item2);

				if (source != null && dest != null)
				{
					ReplaceFont(source, dest);
				}
			}
		}

		static void ReplaceFont(TMP_FontAsset source, TMP_FontAsset dest)
		{
			var oldMaterial = dest.material;

			ShallowCopy(source, dest);

            oldMaterial.CopyPropertiesFromMaterial(dest.material);

			if (dest is WeaverCore.Assets.TMPro.TMP_FontAsset weaverFont)
			{
				weaverFont.FontReplaced = true;
			}
        }

		static void ShallowCopy<T>(T from, T to, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
		{
			foreach (var field in typeof(T).GetFields(flags))
			{
				field.SetValue(to, field.GetValue(from));
			}
		}
	}
}
