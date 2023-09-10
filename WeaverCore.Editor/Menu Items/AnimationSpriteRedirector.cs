using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Menu_Items
{
    public class AnimationSpriteRedirector : EditorWindow
    {
        WeaverAnimationData animData;
        //Texture2D textureToRedirectTo;

        public List<Texture2D> textureList;
        ReorderableList textures = null;
        SerializedObject serializedObject;
        Vector2 scrollPosition;

        string status;

        [MenuItem("WeaverCore/Tools/Redirect Animation Sprites")]
        static void OpenMenu()
        {
            var window = EditorWindow.CreateWindow<AnimationSpriteRedirector>();
            window.titleContent = new GUIContent("Redirect Animation Sprites");
            window.Show();
        }

        private void OnEnable()
        {
            serializedObject = new SerializedObject(this);
            textureList = new List<Texture2D>();
            textures = new ReorderableList(serializedObject, serializedObject.FindProperty(nameof(textureList)), false, true, true, true);

            textures.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Textures");
            textures.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                rect.y += 2f;
                rect.height = EditorGUIUtility.singleLineHeight;

                GUIContent objectLabel = new GUIContent($"Texture {index}");
                EditorGUI.PropertyField(rect, serializedObject.FindProperty(nameof(textureList)).GetArrayElementAtIndex(index), objectLabel);
            };
        }

        private void OnGUI()
        {
            if (serializedObject == null)
            {
                return;
            }
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            serializedObject.Update();
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.wordWrap = true;

            EditorGUILayout.LabelField("This window is useful if you have multiple identical sprite sheets in your project, and you want an Animation Data object to use only one of them",labelStyle);

            EditorGUILayout.Space();

            animData = (WeaverAnimationData)EditorGUILayout.ObjectField(new GUIContent("Animation Data to redirect"), animData, typeof(WeaverAnimationData),false);

            EditorGUILayout.LabelField("Add the textures you want to redirect to");
            EditorGUILayout.Space();
            textures.DoLayoutList();

            EditorGUILayout.Space();
            if (GUILayout.Button(new GUIContent("Add Selected Textures", "Adds all the textures that are highlighted in the \"Project\" Window")))
            {
                foreach (var tex in Selection.objects.OfType<Texture2D>())
                {
                    var list = serializedObject.FindProperty(nameof(textureList));
                    list.arraySize++;
                    list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = tex;
                }
            }

            //textureToRedirectTo = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Texture to Redirect To"), textureToRedirectTo, typeof(Texture2D), false);

            EditorGUI.BeginDisabledGroup(animData == null || textureList == null || textureList.Count == 0);
            if (GUILayout.Button("Redirect Sprites"))
            {
                status = "";

                //var redirectionSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(textureToRedirectTo)).OfType<Sprite>().ToList();
                var redirectionSprites = textureList.SelectMany(t => AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(t)).OfType<Sprite>()).ToList();

                var allClips = animData.AllClips.ToList();

                int frameChangeCounter = 0;

                try
                {
                    animData.Clear();
                    foreach (var clip in allClips)
                    {
                        for (int i = 0; i < clip.Frames.Count; i++)
                        {
                            var frame = clip.Frames[i];
                            if (frame != null)
                            {
                                var frameName = clip.Frames[i].name;

                                var newFrame = redirectionSprites.FirstOrDefault(s => s.name == frameName);

                                if (newFrame != null)
                                {
                                    clip.Frames[i] = newFrame;
                                    frameChangeCounter++;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    foreach (var clip in allClips)
                    {
                        animData.AddClip(clip);
                    }
                    EditorUtility.SetDirty(animData);
                }
                status = $"{frameChangeCounter} frames have been modified";
            }
            EditorGUI.EndDisabledGroup();
            if (!string.IsNullOrEmpty(status))
            {
                EditorGUILayout.LabelField(status, labelStyle);
            }
            EditorGUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();
        }
    }
}
