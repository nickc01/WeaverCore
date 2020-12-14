using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;

namespace TMPro
{
	public class TMP_PostBuildProcessHandler
	{
		[PostProcessBuild(10000)]
		public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
		{
			if (target == BuildTarget.iOS && TMP_Settings.enableEmojiSupport)
			{
				string path = Path.Combine(pathToBuiltProject, "Classes/UI/Keyboard.mm");
				string text = File.ReadAllText(path);
				text = text.Replace("FILTER_EMOJIS_IOS_KEYBOARD 1", "FILTER_EMOJIS_IOS_KEYBOARD 0");
				File.WriteAllText(path, text);
			}
		}
	}
}
