using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tizen;

namespace WeaverCore.Editor
{
	public class TexturePacker : EditorWindow
	{
		bool foldoutTextures = false;
		int padding = 1;
		Vector2Int packedTextureSize = new Vector2Int(1024, 1024);
		string textureName;
		List<Texture2D> TexturesToPack = new List<Texture2D>();

		Vector2 scrollPosition;


		//[MenuItem("WeaverCore/Tools/Pack Textures")]
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
			foldoutTextures = false;
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
			packedTextureSize = EditorGUILayout.Vector2IntField("Packed Texture Size", packedTextureSize);
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Add Texture"))
			{
				TexturesToPack.Add(null);
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
			var texture = new Texture2D(packedTextureSize.x, packedTextureSize.y);
			texture.name = textureName;
			texture.PackTextures(Textures.ToArray(), padding);

			AssetDatabase.CreateAsset(texture, textureName + ".png");
		}
	}
}
