using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;
using System.IO;

public class DuplicateTextures : EditorWindow
{
	[MenuItem("WeaverCore/Tools/Duplicate Texture")]
	public static void Convert()
	{
		Display();
	}

	Texture2D texture;

	public static DuplicateTextures Display()
	{
		var window = GetWindow<DuplicateTextures>();
		window.titleContent = new GUIContent("Duplicate Texture");
		window.Show();

		return window;
	}

	private void OnGUI()
	{
		texture = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Texture", "The texture to duplicate"), texture, typeof(Texture2D), true);

		if (GUILayout.Button("Duplicate"))
		{
			Close();
			DuplicateTexture();
			//UnboundCoroutine.Start(Convert(spriteList, destroyOriginalTextures, outputAtlasName, cropTextures));
		}
	}

	void DuplicateTexture()
    {
		var clone = texture.Clone();
		//var texturePath = AssetDatabase.GetAssetPath(texture);
		var texturePath = AssetDatabase.GenerateUniqueAssetPath("Assets/CLONED_TEXTURE.PNG");
		Debug.Log("Path = " + texturePath);

		var png = clone.EncodeToPNG();

		File.WriteAllBytes(texturePath, png);

		AssetDatabase.ImportAsset(texturePath);
	}
}
