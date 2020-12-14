using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	public static class TMP_SpriteAssetMenu
	{
		[MenuItem("CONTEXT/TMP_SpriteAsset/Add Default Material", false, 2000)]
		private static void CopyTexture(MenuCommand command)
		{
			TMP_SpriteAsset tMP_SpriteAsset = (TMP_SpriteAsset)command.context;
			if (tMP_SpriteAsset != null && tMP_SpriteAsset.material == null)
			{
				AddDefaultMaterial(tMP_SpriteAsset);
			}
		}

		[MenuItem("Assets/Create/TextMeshPro/Sprite Asset", false, 100)]
		public static void CreateTextMeshProObjectPerform()
		{
			Object activeObject = Selection.activeObject;
			if (activeObject == null || activeObject.GetType() != typeof(Texture2D))
			{
				Debug.LogWarning("A texture which contains sprites must first be selected in order to create a TextMesh Pro Sprite Asset.");
				return;
			}
			Texture2D texture2D = activeObject as Texture2D;
			string assetPath = AssetDatabase.GetAssetPath(texture2D);
			string fileName = Path.GetFileName(assetPath);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(assetPath);
			string str = assetPath.Replace(fileName, "");
			TMP_SpriteAsset tMP_SpriteAsset = AssetDatabase.LoadAssetAtPath(str + fileNameWithoutExtension + ".asset", typeof(TMP_SpriteAsset)) as TMP_SpriteAsset;
			if ((tMP_SpriteAsset == null) ? true : false)
			{
				tMP_SpriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
				AssetDatabase.CreateAsset(tMP_SpriteAsset, str + fileNameWithoutExtension + ".asset");
				tMP_SpriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(tMP_SpriteAsset.name);
				tMP_SpriteAsset.spriteSheet = texture2D;
				tMP_SpriteAsset.spriteInfoList = GetSpriteInfo(texture2D);
				AddDefaultMaterial(tMP_SpriteAsset);
			}
			else
			{
				tMP_SpriteAsset.spriteInfoList = UpdateSpriteInfo(tMP_SpriteAsset);
				if (tMP_SpriteAsset.material == null)
				{
					AddDefaultMaterial(tMP_SpriteAsset);
				}
			}
			EditorUtility.SetDirty(tMP_SpriteAsset);
			AssetDatabase.SaveAssets();
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(tMP_SpriteAsset));
		}

		private static List<TMP_Sprite> GetSpriteInfo(Texture source)
		{
			string assetPath = AssetDatabase.GetAssetPath(source);
			Sprite[] array = (from x in AssetDatabase.LoadAllAssetsAtPath(assetPath)
				select x as Sprite into x
				where x != null
				orderby x.rect.y descending, x.rect.x
				select x).ToArray();
			List<TMP_Sprite> list = new List<TMP_Sprite>();
			for (int i = 0; i < array.Length; i++)
			{
				TMP_Sprite tMP_Sprite = new TMP_Sprite();
				Sprite sprite = array[i];
				tMP_Sprite.id = i;
				tMP_Sprite.name = sprite.name;
				tMP_Sprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(tMP_Sprite.name);
				Rect rect = sprite.rect;
				tMP_Sprite.x = rect.x;
				tMP_Sprite.y = rect.y;
				tMP_Sprite.width = rect.width;
				tMP_Sprite.height = rect.height;
				Vector2 vector = new Vector2(0f - sprite.bounds.min.x / (sprite.bounds.extents.x * 2f), 0f - sprite.bounds.min.y / (sprite.bounds.extents.y * 2f));
				tMP_Sprite.pivot = new Vector2(0f - vector.x * rect.width, rect.height - vector.y * rect.height);
				tMP_Sprite.sprite = sprite;
				tMP_Sprite.xAdvance = rect.width;
				tMP_Sprite.scale = 1f;
				tMP_Sprite.xOffset = tMP_Sprite.pivot.x;
				tMP_Sprite.yOffset = tMP_Sprite.pivot.y;
				list.Add(tMP_Sprite);
			}
			return list;
		}

		private static void AddDefaultMaterial(TMP_SpriteAsset spriteAsset)
		{
			Shader shader = Shader.Find("TextMeshPro/Sprite");
			Material material = new Material(shader);
			material.SetTexture(ShaderUtilities.ID_MainTex, spriteAsset.spriteSheet);
			spriteAsset.material = material;
			material.hideFlags = HideFlags.HideInHierarchy;
			AssetDatabase.AddObjectToAsset(material, spriteAsset);
		}

		private static List<TMP_Sprite> UpdateSpriteInfo(TMP_SpriteAsset spriteAsset)
		{
			string assetPath = AssetDatabase.GetAssetPath(spriteAsset.spriteSheet);
			Sprite[] array = (from x in AssetDatabase.LoadAllAssetsAtPath(assetPath)
				select x as Sprite into x
				where x != null
				orderby x.rect.y descending, x.rect.x
				select x).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				Sprite sprite = array[i];
				int num = -1;
				if (spriteAsset.spriteInfoList.Count > i && spriteAsset.spriteInfoList[i].sprite != null)
				{
					num = spriteAsset.spriteInfoList.FindIndex((TMP_Sprite item) => item.sprite.GetInstanceID() == sprite.GetInstanceID());
				}
				TMP_Sprite tMP_Sprite = (num == -1) ? new TMP_Sprite() : spriteAsset.spriteInfoList[num];
				Rect rect = sprite.rect;
				tMP_Sprite.x = rect.x;
				tMP_Sprite.y = rect.y;
				tMP_Sprite.width = rect.width;
				tMP_Sprite.height = rect.height;
				Vector2 vector = new Vector2(0f - sprite.bounds.min.x / (sprite.bounds.extents.x * 2f), 0f - sprite.bounds.min.y / (sprite.bounds.extents.y * 2f));
				tMP_Sprite.pivot = new Vector2(0f - vector.x * rect.width, rect.height - vector.y * rect.height);
				if (num == -1)
				{
					int[] array2 = spriteAsset.spriteInfoList.Select((TMP_Sprite item) => item.id).ToArray();
					int id = 0;
					for (int j = 0; j < array2.Length; j++)
					{
						if (array2[0] != 0)
						{
							break;
						}
						if (j > 0 && array2[j] - array2[j - 1] > 1)
						{
							id = array2[j - 1] + 1;
							break;
						}
						id = j + 1;
					}
					tMP_Sprite.sprite = sprite;
					tMP_Sprite.name = sprite.name;
					tMP_Sprite.hashCode = TMP_TextUtilities.GetSimpleHashCode(tMP_Sprite.name);
					tMP_Sprite.id = id;
					tMP_Sprite.xAdvance = rect.width;
					tMP_Sprite.scale = 1f;
					tMP_Sprite.xOffset = tMP_Sprite.pivot.x;
					tMP_Sprite.yOffset = tMP_Sprite.pivot.y;
					spriteAsset.spriteInfoList.Add(tMP_Sprite);
					spriteAsset.spriteInfoList = spriteAsset.spriteInfoList.OrderBy((TMP_Sprite s) => s.id).ToList();
				}
				else
				{
					spriteAsset.spriteInfoList[num] = tMP_Sprite;
				}
			}
			return spriteAsset.spriteInfoList;
		}
	}
}
