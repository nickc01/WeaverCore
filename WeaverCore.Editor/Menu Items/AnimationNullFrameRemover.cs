using System.Linq;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Editor.Menu_Items
{
    public class AnimationNullFrameRemover : EditorWindow
    {
        WeaverAnimationData animData;
        string status;
        Vector2 scrollPosition;

        [MenuItem("WeaverCore/Tools/Remove Null Frame Animations")]
        static void OpenMenu()
        {
            var window = EditorWindow.CreateWindow<AnimationNullFrameRemover>();
            window.titleContent = new GUIContent("Remove Null Frame Animations");
            window.Show();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.wordWrap = true;

            EditorGUILayout.LabelField("This tool removes animation clips that contain only null sprites from a WeaverAnimationData object", labelStyle);

            EditorGUILayout.Space();

            animData = (WeaverAnimationData)EditorGUILayout.ObjectField(new GUIContent("Animation Data"), animData, typeof(WeaverAnimationData), false);

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(animData == null);
            if (GUILayout.Button("Remove Null Frame Animations"))
            {
                status = "";
                RemoveNullFrameAnimations();
            }
            EditorGUI.EndDisabledGroup();

            if (!string.IsNullOrEmpty(status))
            {
                EditorGUILayout.LabelField(status, labelStyle);
            }

            EditorGUILayout.EndScrollView();
        }

        private void RemoveNullFrameAnimations()
        {
            if (animData == null)
            {
                status = "No animation data selected";
                return;
            }

            var allClips = animData.AllClips.ToList();
            int removedClipsCount = 0;
            int totalClips = allClips.Count;

            try
            {
                // Clear the animation data to rebuild it without null frame clips
                animData.Clear();

                foreach (var clip in allClips)
                {
                    // Check if all frames in the clip are null
                    bool hasNonNullFrames = clip.Frames.Any(frame => frame != null);

                    if (hasNonNullFrames)
                    {
                        // Keep clips that have at least one non-null frame
                        animData.AddClip(clip);
                    }
                    else
                    {
                        // Skip clips that have only null frames
                        removedClipsCount++;
                        Debug.Log($"Removed animation clip '{clip.Name}' - contained only null frames");
                    }
                }

                EditorUtility.SetDirty(animData);
                status = $"Removed {removedClipsCount} animation clips with only null frames out of {totalClips} total clips";
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error removing null frame animations: {ex.Message}");
                status = $"Error occurred: {ex.Message}";
                
                // Restore original clips in case of error
                foreach (var clip in allClips)
                {
                    animData.AddClip(clip);
                }
            }
        }

        /// <summary>
        /// Utility method that can be called from other scripts to remove null frame animations
        /// </summary>
        /// <param name="animationData">The WeaverAnimationData to process</param>
        /// <returns>Number of clips removed</returns>
        public static int RemoveNullFrameAnimationsFromData(WeaverAnimationData animationData)
        {
            if (animationData == null)
            {
                return 0;
            }

            var allClips = animationData.AllClips.ToList();
            int removedClipsCount = 0;

            animationData.Clear();

            foreach (var clip in allClips)
            {
                bool hasNonNullFrames = clip.Frames.Any(frame => frame != null);

                if (hasNonNullFrames)
                {
                    animationData.AddClip(clip);
                }
                else
                {
                    removedClipsCount++;
                }
            }

            return removedClipsCount;
        }
    }
}