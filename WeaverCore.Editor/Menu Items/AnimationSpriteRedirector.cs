using System.Linq;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Menu_Items
{
    public class AnimationSpriteRedirector : EditorWindow
    {
        WeaverAnimationData animData;
        Texture2D textureToRedirectTo;

        string status;

        [MenuItem("WeaverCore/Tools/Redirect Animation Sprites")]
        static void OpenMenu()
        {
            var window = EditorWindow.CreateWindow<AnimationSpriteRedirector>();
            window.titleContent = new GUIContent("Redirect Animation Sprites");
            window.Show();
        }

        private void OnGUI()
        {
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.wordWrap = true;

            EditorGUILayout.LabelField("This window is useful if you have multiple identical sprite sheets in your project, and you want an Animation Data object to use only one of them",labelStyle);

            EditorGUILayout.Space();

            animData = (WeaverAnimationData)EditorGUILayout.ObjectField(new GUIContent("Animation Data to redirect"), animData, typeof(WeaverAnimationData),false);

            textureToRedirectTo = (Texture2D)EditorGUILayout.ObjectField(new GUIContent("Texture to Redirect To"), textureToRedirectTo, typeof(Texture2D), false);

            EditorGUI.BeginDisabledGroup(animData == null || textureToRedirectTo == null);
            if (GUILayout.Button("Redirect Sprites"))
            {
                status = "";

                var redirectionSprites = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(textureToRedirectTo)).OfType<Sprite>().ToList();

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
        }
    }
}
