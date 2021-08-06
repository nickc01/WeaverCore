using System.IO;
using System.Text;
using UnityEditor;

namespace WeaverCore.Editor.Utilities
{
	public static class CreationUtilities
	{
		public static void CreateFileFromTemplate(FileInfo template, string name, string extension)
		{
			name = name.Replace(" ", "").Trim();
			MonoScript script = new MonoScript();
			var path = AssetDatabase.GenerateUniqueAssetPath($"Assets/{name}.{extension}");
			try
			{
				AssetDatabase.StartAssetEditing();

				var contents = new StringBuilder(File.ReadAllText(template.FullName));

				contents.Replace("%NAME%", name);

				File.WriteAllText(path, contents.ToString());

				AssetDatabase.ImportAsset(path);
			}
			finally
			{
				AssetDatabase.StopAssetEditing();
			}
		}
	}
}
