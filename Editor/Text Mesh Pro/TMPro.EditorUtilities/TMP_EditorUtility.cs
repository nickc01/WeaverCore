using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	public static class TMP_EditorUtility
	{
		private static bool isTMProFolderLocated;

		private static string folderPath = "Not Found";

		private static EditorWindow Gameview;

		private static bool isInitialized = false;

		private static void GetGameview()
		{
			Assembly assembly = typeof(EditorWindow).Assembly;
			Type type = assembly.GetType("UnityEditor.GameView");
			Gameview = EditorWindow.GetWindow(type);
		}

		public static void RepaintAll()
		{
			if (!isInitialized)
			{
				GetGameview();
				isInitialized = true;
			}
			SceneView.RepaintAll();
			Gameview.Repaint();
		}

		public static T CreateAsset<T>(string name) where T : ScriptableObject
		{
			string assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
			assetPath = ((assetPath.Length == 0) ? ("Assets/" + name + ".asset") : ((!Directory.Exists(assetPath)) ? (Path.GetDirectoryName(assetPath) + "/" + name + ".asset") : (assetPath + "/" + name + ".asset")));
			T val = ScriptableObject.CreateInstance<T>();
			AssetDatabase.CreateAsset(val, AssetDatabase.GenerateUniqueAssetPath(assetPath));
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = val;
			return val;
		}

		public static Material[] FindMaterialReferences(TMP_FontAsset fontAsset)
		{
			List<Material> list = new List<Material>();
			Material material = fontAsset.material;
			list.Add(material);
			string filter = "t:Material " + fontAsset.name.Split(' ')[0];
			string[] array = AssetDatabase.FindAssets(filter);
			for (int i = 0; i < array.Length; i++)
			{
				string assetPath = AssetDatabase.GUIDToAssetPath(array[i]);
				Material material2 = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
				if (material2.HasProperty(ShaderUtilities.ID_MainTex) && material2.GetTexture(ShaderUtilities.ID_MainTex) != null && material.GetTexture(ShaderUtilities.ID_MainTex) != null && material2.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() == material.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() && !list.Contains(material2))
				{
					list.Add(material2);
				}
			}
			return list.ToArray();
		}

		public static TMP_FontAsset FindMatchingFontAsset(Material mat)
		{
			if (mat.GetTexture(ShaderUtilities.ID_MainTex) == null)
			{
				return null;
			}
			string[] dependencies = AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(mat), false);
			for (int i = 0; i < dependencies.Length; i++)
			{
				TMP_FontAsset tMP_FontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(dependencies[i]);
				if (tMP_FontAsset != null)
				{
					return tMP_FontAsset;
				}
			}
			return null;
		}

		public static string GetAssetLocation()
		{
			if (!isTMProFolderLocated)
			{
				isTMProFolderLocated = true;
				string currentDirectory = Directory.GetCurrentDirectory();
				string[] directories = Directory.GetDirectories(currentDirectory + "/Assets", "TextMesh Pro", SearchOption.AllDirectories);
				folderPath = ValidateLocation(directories);
				if (folderPath != null)
				{
					return folderPath;
				}
				directories = Directory.GetDirectories(currentDirectory + "/Assets", "TextMeshPro", SearchOption.AllDirectories);
				folderPath = ValidateLocation(directories);
				if (folderPath != null)
				{
					return folderPath;
				}
			}
			if (folderPath != null)
			{
				return folderPath;
			}
			Debug.LogWarning("Could not located the \"TextMesh Pro/GUISkins\" Folder to load the Editor Skins.");
			return null;
		}

		private static string ValidateLocation(string[] paths)
		{
			for (int i = 0; i < paths.Length; i++)
			{
				if (Directory.Exists(paths[i] + "/GUISkins"))
				{
					folderPath = "Assets" + paths[i].Split(new string[1]
					{
						"/Assets"
					}, StringSplitOptions.None)[1];
					return folderPath;
				}
			}
			return null;
		}

		public static string GetDecimalCharacterSequence(int[] characterSet)
		{
			string text = string.Empty;
			int num = characterSet.Length;
			int num2 = characterSet[0];
			int num3 = num2;
			for (int i = 1; i < num; i++)
			{
				if (characterSet[i - 1] + 1 == characterSet[i])
				{
					num3 = characterSet[i];
					continue;
				}
				text = ((num2 != num3) ? (text + num2 + "-" + num3 + ",") : (text + num2 + ","));
				num2 = (num3 = characterSet[i]);
			}
			if (num2 == num3)
			{
				return text + num2;
			}
			return text + num2 + "-" + num3;
		}

		public static string GetUnicodeCharacterSequence(int[] characterSet)
		{
			string text = string.Empty;
			int num = characterSet.Length;
			int num2 = characterSet[0];
			int num3 = num2;
			for (int i = 1; i < num; i++)
			{
				if (characterSet[i - 1] + 1 == characterSet[i])
				{
					num3 = characterSet[i];
					continue;
				}
				text = ((num2 != num3) ? (text + num2.ToString("X2") + "-" + num3.ToString("X2") + ",") : (text + num2.ToString("X2") + ","));
				num2 = (num3 = characterSet[i]);
			}
			if (num2 == num3)
			{
				return text + num2.ToString("X2");
			}
			return text + num2.ToString("X2") + "-" + num3.ToString("X2");
		}

		public static void DrawBox(Rect rect, float thickness, Color color)
		{
			EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.y + thickness, rect.width + thickness * 2f, thickness), color);
			EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.y + thickness, thickness, rect.height - thickness * 2f), color);
			EditorGUI.DrawRect(new Rect(rect.x - thickness, rect.y + rect.height - thickness * 2f, rect.width + thickness * 2f, thickness), color);
			EditorGUI.DrawRect(new Rect(rect.x + rect.width, rect.y + thickness, thickness, rect.height - thickness * 2f), color);
		}

		public static int GetHorizontalAlignmentGridValue(int value)
		{
			//int horizontalValue = value % 4;
			return value % 4;
			/*switch (horizontalValue)
			{
				case 0:
					return 0;
					case 
				default:
					return 0;
			}*/
			/*if ((value & 1) == 1)
			{
				return 0;
			}
			if ((value & 2) == 2)
			{
				return 1;
			}
			if ((value & 4) == 4)
			{
				return 2;
			}
			if ((value & 8) == 8)
			{
				return 3;
			}
			if ((value & 0x10) == 16)
			{
				return 4;
			}
			if ((value & 0x20) == 32)
			{
				return 5;
			}*/
			//return 0;
		}

		public static int GetVerticalAlignmentGridValue(int value)
		{
			return Mathf.FloorToInt(value / 4f);
			/*if ((value & 0x100) == 256)
			{
				return 0;
			}
			if ((value & 0x200) == 512)
			{
				return 1;
			}
			if ((value & 0x400) == 1024)
			{
				return 2;
			}
			if ((value & 0x800) == 2048)
			{
				return 3;
			}
			if ((value & 0x1000) == 4096)
			{
				return 4;
			}
			if ((value & 0x2000) == 8192)
			{
				return 5;
			}
			return 0;*/
		}
	}
}
