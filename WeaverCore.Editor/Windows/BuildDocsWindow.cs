using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.Compilation;

namespace WeaverCore.Editor
{
    /// <summary>
    /// The main window used for building the documentation with DocFX
    /// </summary>
    public class BuildDocsWindow : EditorWindow
    {
        static bool processStarted = false;
        static Process buildProcess;
        static string status;

        Vector2 scrollStorage;

        [MenuItem("WeaverCore/Tools/Build Documentation")]
        static void BuildDocs()
        {
            GetWindow<BuildDocsWindow>().Show();
        }



        private void Awake()
        {
            titleContent = new UnityEngine.GUIContent("Build Docs");
            var oldPos = position;
            oldPos.width = 1014;
            oldPos.height = 226;
            position = oldPos;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Status");

            var style = new GUIStyle(EditorStyles.textField);
            style.wordWrap = true;
            style.richText = true;

            scrollStorage = ScrollableSelectableLabel(scrollStorage, status, style);

            EditorGUILayout.BeginHorizontal();

            if (!processStarted)
            {
                if (GUILayout.Button(new GUIContent("Build Documentation")))
                {
                    status = "";
                    buildProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            Arguments = $"\"{BuildTools.WeaverCoreFolder}{Path.DirectorySeparatorChar}docs_src~{Path.DirectorySeparatorChar}docfx.json\"",
                            WorkingDirectory = $"{BuildTools.WeaverCoreFolder}{Path.DirectorySeparatorChar}Libraries{Path.DirectorySeparatorChar}docfx~",
                            CreateNoWindow = true,
                            ErrorDialog = false,
                            FileName = $"{BuildTools.WeaverCoreFolder}{Path.DirectorySeparatorChar}Libraries{Path.DirectorySeparatorChar}docfx~{Path.DirectorySeparatorChar}docfx.exe",
                            RedirectStandardOutput = true,
                            UseShellExecute = false
                        },
                        EnableRaisingEvents = true,
                    };
                    buildProcess.Exited += BuildProcess_Exited;

                    processStarted = true;

                    buildProcess.Start();

                    _ = ReadFromOutputStream(this, buildProcess.StandardOutput);
                }

                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(new GUIContent("Cancel"));
                EditorGUI.EndDisabledGroup();
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(new GUIContent("Building..."));
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button(new GUIContent("Cancel")))
                {
                    if (buildProcess != null)
                    {
                        buildProcess.Kill();
                    }
                }

                if (buildProcess != null && buildProcess.HasExited)
                {
                    processStarted = false;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }

        private static void BuildProcess_Exited(object sender, System.EventArgs e)
        {
            processStarted = false;
        }

        static async Task ReadFromOutputStream(EditorWindow window, StreamReader stdOutput)
        {
            while (processStarted)
            {
                var line = await stdOutput.ReadLineAsync();
                status += $"{FilterLine(line)}\n";
            }
        }

        static string FilterLine(string line)
        {
            if (line.Contains("Warning:"))
            {
                line = $"<color=#ffa500ff>{line}</color>";
            }

            if (line.Contains("Error:") || line.Contains("Exception:"))
            {
                line = $"<color=#ff0000ff>{line}</color>";
            }

            return line;
        }

        private Vector3 ScrollableSelectableLabel(Vector3 position, string text, GUIStyle style)
        {

            Vector2 scrollPos = new Vector2(position.x, position.y);
            float width = position.z;
            scrollPos = GUILayout.BeginScrollView(scrollPos,GUILayout.Height(this.position.height - (EditorGUIUtility.singleLineHeight * 2.5f)));

            float pixelHeight = style.CalcHeight(new GUIContent(text), width);
            EditorGUILayout.LabelField(text, style, GUILayout.ExpandWidth(true), GUILayout.MinHeight(Mathf.Max(pixelHeight, this.position.height - (EditorGUIUtility.singleLineHeight * 2.5f)/*EditorGUIUtility.singleLineHeight * 10f*/)));

            if (Event.current.type == EventType.Repaint)
            {
                width = GUILayoutUtility.GetLastRect().width;
            }
            GUILayout.EndScrollView();

            return new Vector3(scrollPos.x, scrollPos.y, width);
        }
    }
}
