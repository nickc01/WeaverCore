using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tizen;
using WeaverCore.DataTypes;
using WeaverCore.Editor.Utilities;

namespace WeaverCore.Editor
{
	public class TexturePacker : EditorWindow
	{
		bool foldoutTextures = true;
		int padding = 1;
		Vector2Int packedTextureSize = new Vector2Int(1024, 1024);
		string textureName;
		List<Texture2D> TexturesToPack = new List<Texture2D>();

		Vector2 scrollPosition;


		[MenuItem("WeaverCore/Tools/Pack Textures")]
		public static void PackTextures()
		{
			var packer = GetWindow<TexturePacker>();
			packer.Init();
			packer.Show();
		}


		void Init()
		{
			scrollPosition = Vector2.zero;
			Vector2Int packedTextureSize = new Vector2Int(1024, 1024);
			textureName = "";
			foldoutTextures = true;
			padding = 1;
			TexturesToPack.Clear();
		}


		void OnGUI()
		{
			foldoutTextures = EditorGUILayout.Foldout(foldoutTextures, "Textures");
			if (foldoutTextures)
			{
				scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
				for (int i = 0; i < TexturesToPack.Count; i++)
				{
					EditorGUILayout.BeginHorizontal();
					TexturesToPack[i] = (Texture2D)EditorGUILayout.ObjectField("Texture", TexturesToPack[i], typeof(Texture2D), false);
					if (GUILayout.Button("X"))
					{
						TexturesToPack.RemoveAt(i);
						i--;
					}
					EditorGUILayout.EndHorizontal();
				}
				EditorGUILayout.EndScrollView();
			}
			EditorGUILayout.Space();
			padding = EditorGUILayout.IntField("Padding", padding);
			textureName = EditorGUILayout.TextField("Final Texture Name", textureName);
			//packedTextureSize = EditorGUILayout.Vector2IntField("Packed Texture Size", packedTextureSize);
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Texture"))
			{
				TexturesToPack.Add(null);
			}
			if (GUILayout.Button("Add Selected Textures"))
			{
				foreach (var assetID in Selection.assetGUIDs)
				{
					WeaverLog.Log("AssetID = " + assetID);
					WeaverLog.Log("AssetPath = " + AssetDatabase.GUIDToAssetPath(assetID));

					var test = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AssetDatabase.GUIDToAssetPath(assetID));
					WeaverLog.Log("Asset Type = " + (test == null ? "null" : test.GetType().FullName));
					var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(assetID));
					WeaverLog.Log("Asset = " + asset);
					if (asset != null)
					{
						WeaverLog.Log("Adding Asset = " + asset);
						TexturesToPack.Add(asset);
					}
				}
			}
			if (GUILayout.Button("Pack"))
			{
				Close();
				Pack(TexturesToPack, padding, textureName, packedTextureSize);
			}
			EditorGUILayout.EndHorizontal();


		}

		public static void Pack(List<Texture2D> Textures, int padding, string textureName, Vector2Int packedTextureSize)
		{
			if (textureName == "")
			{
				textureName = "PackedTexture";
			}
			using (var context = new ReadableTextureContext(Textures))
			{
				var texture = new Texture2D(packedTextureSize.x, packedTextureSize.y);
				texture.name = textureName;
				var uvs = texture.PackTextures(Textures.ToArray(), padding,8192);

				var pngData = texture.EncodeToPNG();

				var assetLocation = new FileInfo("Assets\\" + textureName + ".png");

				File.WriteAllBytes(assetLocation.FullName, pngData);

				AssetDatabase.ImportAsset("Assets/" + textureName + ".png", ImportAssetOptions.ForceUpdate);

				float averagePPU = 0f;
				List<Vector2> texturePivots = new List<Vector2>();

				foreach (var t in Textures)
				{
					var t_importer = (TextureImporter)TextureImporter.GetAtPath(AssetDatabase.GetAssetPath(t));
					texturePivots.Add(t_importer.spritePivot);
					averagePPU += t_importer.spritePixelsPerUnit;
				}
				averagePPU /= Textures.Count;

				var importer = (TextureImporter)TextureImporter.GetAtPath("Assets/" + textureName + ".png");

				importer.maxTextureSize = Mathf.Max(texture.width,texture.height);

				importer.spriteImportMode = SpriteImportMode.Multiple;
				importer.spritePixelsPerUnit = averagePPU;

				//NOT DONE YET. NEED TO ADD SPRITES TO TEXTURE : TODO TODO TODO
				var sheets = new SpriteMetaData[Textures.Count];
				for (int i = 0; i < Textures.Count; i++)
				{
					var uv = uvs[i];
					sheets[i] = new SpriteMetaData()
					{
						name = Textures[i].name,
						border = Vector4.zero,
						pivot = texturePivots[i],
						alignment = (int)SpriteAlignment.Custom,
						rect = new Rect(uv.x * texture.width, uv.y * texture.height, uv.width * texture.width, uv.height * texture.height)
					};
				}
				importer.spritesheet = sheets;

				importer.SaveAndReimport();

				//AssetDatabase.CreateAsset(texture, "Assets/" + textureName + ".png");
			}
		}
	}
}
