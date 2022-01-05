using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Compilation;
using WeaverCore.Editor.Utilities;
using WeaverCore.Utilities;

namespace WeaverCore.Editor
{
    public class CreateModMenu : EditorWindow
    {
        string modName;

        [MenuItem("WeaverCore/Create/New Mod")]
        static void CreateWeaverMod()
        {
            var window = GetWindow<CreateModMenu>();
            window.titleContent = new GUIContent("Create Mod");
            window.Show();
        }


        private void Awake()
        {
            modName = "Awesome New Mod";
        }

        private void OnGUI()
        {
            modName = EditorGUILayout.TextField(new GUIContent("Mod Name"), modName);

            if (GUILayout.Button("Create Mod"))
            {
                Close();
                var template = EditorAssets.LoadEditorAsset<TextAsset>("Mod Template");

                var contents = template.text;
                var className = modName.Replace(" ", "");
                contents = contents.Replace("MODNAME", className);
                contents = contents.Replace("MODDISPLAYNAME", modName);

                var path = PathUtilities.AssetsFolder.AddSlash() + $"{className}.cs";
                File.WriteAllText(path, contents);

                AssetDatabase.ImportAsset($"Assets/{className}.cs",ImportAssetOptions.ForceSynchronousImport);

                Selection.activeObject = AssetDatabase.LoadAssetAtPath($"Assets/{className}.cs", typeof(TextAsset));
            }
        }
    }
}
