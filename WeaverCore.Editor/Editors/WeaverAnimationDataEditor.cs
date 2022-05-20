using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WeaverCore.Utilities;

[CustomEditor(typeof(WeaverAnimationData))]
public class WeaverAnimationDataEditor : Editor
{
	bool showClipsDropdown = true;

	Vector2 clipScrollPosition = default(Vector2);
	float maxClipScrollHeight = 400f;
	WeaverAnimationData data;



	string newClipName;
	List<Sprite> newClipFrames;
	float newClipFPS = 12f;
	WeaverAnimationData.WrapMode newClipWrapMode = WeaverAnimationData.WrapMode.Once;
	int newClipLoopStart;

	bool framesFoldout = true;
	Vector2 framesScrollPosition = default(Vector2);
	float maxFrameScrollHeight = 200f;

	float spriteFieldHeight = 0f;

	void Awake()
	{
		data = target as WeaverAnimationData;
		newClipFrames = new List<Sprite>();
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();

		showClipsDropdown = EditorGUILayout.Foldout(showClipsDropdown, "Clips");
		if (showClipsDropdown)
		{
			clipScrollPosition = EditorGUILayout.BeginScrollView(clipScrollPosition,GUILayout.MaxHeight(maxClipScrollHeight), GUILayout.ExpandHeight(true));
			var clipNames = serializedObject.FindProperty("clipNames");
			for (int i = 0; i < clipNames.arraySize; i++)
			{
				var value = clipNames.GetArrayElementAtIndex(i);

				EditorGUILayout.BeginHorizontal();

				var zeros = 0f;
				if (i > 0)
				{
					zeros = Mathf.Floor(Mathf.Log10(i));
				}

				EditorGUILayout.LabelField(i.ToString(),GUILayout.MaxWidth(10f * (zeros + 1f)));

				value.stringValue = EditorGUILayout.TextField(value.stringValue);
				if (GUILayout.Button("Edit", GUILayout.MaxWidth(50f)))
				{
					var clip = GetClip(value.stringValue);
					newClipName = clip.Name;
					newClipFrames = clip.Frames;
					newClipFPS = clip.FPS;
					newClipLoopStart = clip.LoopStart;
					newClipWrapMode = clip.WrapMode;
				}
				if (GUILayout.Button("X", GUILayout.MaxWidth(35f)))
				{
					RemoveClip(value.stringValue);
					i--;
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndScrollView();
		}

		EditorGUILayout.Space();

		EditorGUILayout.LabelField("Add New Clip", EditorStyles.boldLabel);

		EditorGUILayout.BeginVertical(EditorStyles.helpBox,GUILayout.MinHeight(0f));
		EditorGUILayout.Space();
		newClipName = EditorGUILayout.TextField("Clip Name", newClipName);
		EditorGUI.indentLevel++;
		framesFoldout = EditorGUILayout.Foldout(framesFoldout, "Frames");
		EditorGUI.indentLevel--;
		if (framesFoldout)
		{
			framesScrollPosition = EditorGUILayout.BeginScrollView(framesScrollPosition, GUILayout.MaxHeight(maxFrameScrollHeight), GUILayout.ExpandHeight(true));

			for (int i = 0; i < newClipFrames.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();

				var zeros = 0f;
				if (i > 0)
				{
					zeros = Mathf.Floor(Mathf.Log10(i));
				}

				EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(10f * (zeros + 1f)));

				newClipFrames[i] = (Sprite)EditorGUILayout.ObjectField(newClipFrames[i], typeof(Sprite), false);

				if (GUILayout.Button("X", GUILayout.MaxWidth(35f)))
				{
					newClipFrames.RemoveAt(i);
					i--;
				}

				EditorGUILayout.EndHorizontal();
			}

			if (GUILayout.Button("Add Frame"))
			{
				newClipFrames.Add(null);
			}
			EditorGUILayout.EndScrollView();
		}

		newClipFPS = EditorGUILayout.FloatField("Animation FPS", newClipFPS);
		newClipWrapMode = (WeaverAnimationData.WrapMode)EditorGUILayout.EnumPopup("Clip Wrap Mode", newClipWrapMode);
		if (newClipWrapMode == WeaverAnimationData.WrapMode.LoopSection || newClipWrapMode == WeaverAnimationData.WrapMode.SingleFrame)
		{
			var wrapModeText = "Loop Start";
			var wrapModeTooltip = "This is the frame index that the animation will start looping from";

			if (newClipWrapMode == WeaverAnimationData.WrapMode.SingleFrame)
			{
				wrapModeText = "Frame To Play";
				wrapModeTooltip = "This is the index of the frame that is going to be played";
			}

			newClipLoopStart = EditorGUILayout.IntField(new GUIContent(wrapModeText, wrapModeTooltip), newClipLoopStart);
			if (newClipLoopStart < 0)
			{
				newClipLoopStart = 0;
			}
			else if (newClipLoopStart > newClipFrames.Count)
			{
				newClipLoopStart = newClipFrames.Count - 1;
			}
		}
		else
		{
			newClipLoopStart = 0;
		}

		bool hasClip = HasClip(newClipName);
		string text = "Add Clip";

		if (hasClip)
		{
			text = "Overwrite Clip";
		}

		if (GUILayout.Button(text))
		{
			var clip = new WeaverAnimationData.Clip(newClipName, newClipFPS, newClipWrapMode, newClipFrames, newClipLoopStart);
			if (hasClip)
			{
				RemoveClip(newClipName);
			}
			AddClip(clip);
			
		}

		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();

		if (EditorGUI.EndChangeCheck())
		{
			serializedObject.ApplyModifiedProperties();
		}
	}

	public bool HasClip(string clipName)
    {
		return HasClip(serializedObject, clipName);
    }

	public static bool HasClip(SerializedObject serializedObject, string clipName)
	{
		var clipNames = serializedObject.FindProperty("clipNames");

		for (int i = 0; i < clipNames.arraySize; i++)
		{
			if (clipNames.GetArrayElementAtIndex(i).stringValue == clipName)
			{
				return true;
			}
		}
		return false;
	}

	public int GetClipIndex(string clipName)
    {
		return GetClipIndex(serializedObject, clipName);
    }

	public static int GetClipIndex(SerializedObject serializedObject, string clipName)
	{
		var clipNames = serializedObject.FindProperty("clipNames");

		for (int i = 0; i < clipNames.arraySize; i++)
		{
			if (clipNames.GetArrayElementAtIndex(i).stringValue == clipName)
			{
				return i;
			}
		}
		return -1;
	}

	public WeaverAnimationData.Clip GetClip(string clipName)
    {
		return GetClip(serializedObject, clipName);
    }

	public static WeaverAnimationData.Clip GetClip(SerializedObject serializedObject, string clipName)
	{
		if (!HasClip(serializedObject, clipName))
		{
			throw new Exception("The clip " + clipName + " does not exist on this animator");
		}

		var clipIndex = GetClipIndex(serializedObject, clipName);

		var clipFPSs = serializedObject.FindProperty("clipFPSs");
		var clipLoopStarts = serializedObject.FindProperty("clipLoopStarts");
		var clipWrapModes = serializedObject.FindProperty("clipWrapModes");
		var clipFrameStartIndexes = serializedObject.FindProperty("clipFrameStartIndexes");
		var clipFrameCounts = serializedObject.FindProperty("clipFrameCounts");
		var frames = serializedObject.FindProperty("frames");

		var clip = new WeaverAnimationData.Clip
		{
			FPS = clipFPSs.GetArrayElementAtIndex(clipIndex).floatValue,
			LoopStart = clipLoopStarts.GetArrayElementAtIndex(clipIndex).intValue,
			Name = clipName,
			WrapMode = (WeaverAnimationData.WrapMode)clipWrapModes.GetArrayElementAtIndex(clipIndex).enumValueIndex
		};

		var frameStart = clipFrameStartIndexes.GetArrayElementAtIndex(clipIndex).intValue;
		var frameCount = clipFrameCounts.GetArrayElementAtIndex(clipIndex).intValue;

		for (int i = frameStart; i < frameStart + frameCount; i++)
		{
			clip.AddFrame((Sprite)frames.GetArrayElementAtIndex(i).objectReferenceValue);
		}
		return clip;
	}

	public bool RemoveClip(string clipName)
    {
		return RemoveClip(serializedObject, clipName);
    }

	public static bool RemoveClip(SerializedObject serializedObject, string clipName)
	{
		if (!HasClip(serializedObject, clipName))
		{
			return false;
		}

		var clipIndex = GetClipIndex(serializedObject, clipName);

		var clipNames = serializedObject.FindProperty("clipNames"); //string
		var clipFPSs = serializedObject.FindProperty("clipFPSs"); //float
		var clipLoopStarts = serializedObject.FindProperty("clipLoopStarts"); //int
		var clipWrapModes = serializedObject.FindProperty("clipWrapModes"); //WrapMode
		var clipFrameStartIndexes = serializedObject.FindProperty("clipFrameStartIndexes"); //int
		var clipFrameCounts = serializedObject.FindProperty("clipFrameCounts"); //int
		var frames = serializedObject.FindProperty("frames"); //Sprite

		clipNames.DeleteArrayElementAtIndex(clipIndex);

		var frameCount = clipFrameCounts.GetArrayElementAtIndex(clipIndex).intValue;

		var startFrameIndex = clipFrameStartIndexes.GetArrayElementAtIndex(clipIndex).intValue;


		for (int i = clipIndex + 1; i < clipFrameStartIndexes.arraySize; i++)
		{
			var valueAtIndex = clipFrameStartIndexes.GetArrayElementAtIndex(i);
			valueAtIndex.intValue = valueAtIndex.intValue - frameCount;
		}

		clipFrameStartIndexes.DeleteArrayElementAtIndex(clipIndex);
		clipFrameCounts.DeleteArrayElementAtIndex(clipIndex);

		if (frameCount > 0)
		{
			for (int i = startFrameIndex + frameCount; i < frames.arraySize; i++)
			{
				frames.GetArrayElementAtIndex(i - frameCount).objectReferenceValue = frames.GetArrayElementAtIndex(i).objectReferenceValue;
			}

			frames.arraySize -= frameCount;
		}

		clipFPSs.DeleteArrayElementAtIndex(clipIndex);
		clipWrapModes.DeleteArrayElementAtIndex(clipIndex);
		clipLoopStarts.DeleteArrayElementAtIndex(clipIndex);

		serializedObject.ApplyModifiedProperties();

		return true;
	}

	public bool AddClip(WeaverAnimationData.Clip clip)
	{
		return AddClip(serializedObject, clip);
	}

	public static bool AddClip(SerializedObject serializedObject, WeaverAnimationData.Clip clip)
	{
		if (HasClip(serializedObject, clip.Name))
		{
			return false;
		}

		var clipNames = serializedObject.FindProperty("clipNames"); //string
		var clipFPSs = serializedObject.FindProperty("clipFPSs"); //float
		var clipLoopStarts = serializedObject.FindProperty("clipLoopStarts"); //int
		var clipWrapModes = serializedObject.FindProperty("clipWrapModes"); //WrapMode
		var clipFrameStartIndexes = serializedObject.FindProperty("clipFrameStartIndexes"); //int
		var clipFrameCounts = serializedObject.FindProperty("clipFrameCounts"); //int
		var frames = serializedObject.FindProperty("frames"); //Sprite

		clipNames.InsertArrayElementAtIndex(clipNames.arraySize);
		clipNames.GetArrayElementAtIndex(clipNames.arraySize - 1).stringValue = clip.Name;

		clipFrameStartIndexes.InsertArrayElementAtIndex(clipFrameStartIndexes.arraySize);
		clipFrameStartIndexes.GetArrayElementAtIndex(clipFrameStartIndexes.arraySize - 1).intValue = frames.arraySize;

		clipFrameCounts.InsertArrayElementAtIndex(clipFrameCounts.arraySize);
		clipFrameCounts.GetArrayElementAtIndex(clipFrameCounts.arraySize - 1).intValue = clip.Frames.Count;

		foreach (var frame in clip.Frames)
		{
			frames.InsertArrayElementAtIndex(frames.arraySize);
			frames.GetArrayElementAtIndex(frames.arraySize - 1).objectReferenceValue = frame;
		}

		clipFPSs.InsertArrayElementAtIndex(clipFPSs.arraySize);
		clipFPSs.GetArrayElementAtIndex(clipFPSs.arraySize - 1).floatValue = clip.FPS;

		clipWrapModes.InsertArrayElementAtIndex(clipWrapModes.arraySize);
		clipWrapModes.GetArrayElementAtIndex(clipWrapModes.arraySize - 1).enumValueIndex = (int)clip.WrapMode;

		clipLoopStarts.InsertArrayElementAtIndex(clipLoopStarts.arraySize);
		clipLoopStarts.GetArrayElementAtIndex(clipLoopStarts.arraySize - 1).intValue = clip.LoopStart;

		serializedObject.ApplyModifiedProperties();

		return true;
	}
}

