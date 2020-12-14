using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	public class TMPro_TexturePostProcessor : AssetPostprocessor
	{
		private void OnPostprocessTexture(Texture2D texture)
		{
			Texture2D texture2D = AssetDatabase.LoadAssetAtPath(base.assetPath, typeof(Texture2D)) as Texture2D;
			if (texture2D != null)
			{
				TMPro_EventManager.ON_SPRITE_ASSET_PROPERTY_CHANGED(true, texture2D);
			}
		}
	}
}
