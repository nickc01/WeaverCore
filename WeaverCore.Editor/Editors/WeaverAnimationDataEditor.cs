using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WeaverCore.Editor.DataTypes;
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
		//serializedObject.UpdateIfRequiredOrScript();
		EditorGUI.BeginChangeCheck();
		//showClipsDropdown = EditorGUILayout.DropdownButton(new GUIContent("Clips"),FocusType.Keyboard);
		showClipsDropdown = EditorGUILayout.Foldout(showClipsDropdown, "Clips");
		if (showClipsDropdown)
		{
			clipScrollPosition = EditorGUILayout.BeginScrollView(clipScrollPosition,GUILayout.MaxHeight(maxClipScrollHeight), GUILayout.ExpandHeight(true));
			var clipNames = serializedObject.FindProperty("clipNames");
			for (int i = 0; i < clipNames.arraySize; i++)
			//for (int i = clipNames.arraySize - 1; i >= 0; i--)
			{
				var value = clipNames.GetArrayElementAtIndex(i);

				EditorGUILayout.BeginHorizontal();

				var zeros = 0f;
				if (i > 0)
				{
					zeros = Mathf.Floor(Mathf.Log10(i));
				}
				//var zeros = Mathf.Floor(Mathf.Log10(i));

				EditorGUILayout.LabelField(i.ToString(),GUILayout.MaxWidth(10f * (zeros + 1f)));
				string oldName = value.stringValue;
				value.stringValue = EditorGUILayout.TextField(value.stringValue);
				//EditorGUILayout.SelectableLabel(value.stringValue);
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
					//data.RemoveClip(oldName);
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
			//spriteFieldHeight = EditorGUI.GetPropertyHeight(SerializedPropertyType.B, new GUIContent(""));
			/*var height = (spriteFieldHeight + 4f) * newClipFrames.Count + (EditorGUIUtility.singleLineHeight + 5f);
			if (height > maxFrameScrollHeight)
			{
				height = maxFrameScrollHeight;
			}*/
			framesScrollPosition = EditorGUILayout.BeginScrollView(framesScrollPosition, GUILayout.MaxHeight(maxFrameScrollHeight), GUILayout.ExpandHeight(true));

			//foreach (var clipFrame in newClipFrames)
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

		//bool hasClip = data.HasClip(newClipName);
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
				//data.RemoveClip(newClipName);
				//data.AddClip(clip);
			}
			AddClip(clip);
			
		}


		/*EditorGUILayout.LabelField("Add New Clip 1");
		EditorGUILayout.LabelField("Add New Clip 2");
		EditorGUILayout.LabelField("Add New Clip 3");
		EditorGUILayout.LabelField("Add New Clip 4");*/


		EditorGUILayout.Space();
		EditorGUILayout.EndVertical();

		if (EditorGUI.EndChangeCheck())
		{
			serializedObject.ApplyModifiedProperties();
		}

		//base.OnInspectorGUI();
	}

	public bool HasClip(string clipName)
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
		if (!HasClip(clipName))
		{
			throw new Exception("The clip " + clipName + " does not exist on this animator");
		}

		var clipIndex = GetClipIndex(clipName);

		var clipFPSs = serializedObject.FindProperty("clipFPSs");
		var clipLoopStarts = serializedObject.FindProperty("clipLoopStarts");
		var clipWrapModes = serializedObject.FindProperty("clipWrapModes");
		var clipFrameStartIndexes = serializedObject.FindProperty("clipFrameStartIndexes");
		var clipFrameCounts = serializedObject.FindProperty("clipFrameCounts");
		var frames = serializedObject.FindProperty("frames");

		var clip = new WeaverAnimationData.Clip
		{
			FPS = clipFPSs.GetArrayElementAtIndex(clipIndex).floatValue,//clipFPSs[clipIndex],
			LoopStart = clipLoopStarts.GetArrayElementAtIndex(clipIndex).intValue,//clipLoopStarts[clipIndex],
			Name = clipName,
			WrapMode = (WeaverAnimationData.WrapMode)clipWrapModes.GetArrayElementAtIndex(clipIndex).enumValueIndex//clipWrapModes[clipIndex]
		};

		var frameStart = clipFrameStartIndexes.GetArrayElementAtIndex(clipIndex).intValue;//clipFrameStartIndexes[clipIndex];
		var frameCount = clipFrameCounts.GetArrayElementAtIndex(clipIndex).intValue;//clipFrameCounts[clipIndex];

		//Debug.Log("Frames_A = " + clip.Frames);
		//Debug.Log("Frame Count = " + frameCount);

		for (int i = frameStart; i < frameStart + frameCount; i++)
		{
			//clip.AddFrame(frames[i]);
			clip.AddFrame((Sprite)frames.GetArrayElementAtIndex(i).objectReferenceValue);
		}
		return clip;
	}

	public bool RemoveClip(string clipName)
	{
		if (!HasClip(clipName))
		{
			return false;
		}

		var clipIndex = GetClipIndex(clipName);

		var clipNames = serializedObject.FindProperty("clipNames"); //string
		var clipFPSs = serializedObject.FindProperty("clipFPSs"); //float
		var clipLoopStarts = serializedObject.FindProperty("clipLoopStarts"); //int
		var clipWrapModes = serializedObject.FindProperty("clipWrapModes"); //WrapMode
		var clipFrameStartIndexes = serializedObject.FindProperty("clipFrameStartIndexes"); //int
		var clipFrameCounts = serializedObject.FindProperty("clipFrameCounts"); //int
		var frames = serializedObject.FindProperty("frames"); //Sprite

		//var clipIndex = clipNames.IndexOf(clipName); //45

		//clipNames.RemoveAt(clipIndex);
		clipNames.DeleteArrayElementAtIndex(clipIndex);

		//var frameCount = clipFrameCounts[clipIndex]; //3
		var frameCount = clipFrameCounts.GetArrayElementAtIndex(clipIndex).intValue;

		//var startFrameIndex = clipFrameStartIndexes[clipIndex]; //264
		var startFrameIndex = clipFrameStartIndexes.GetArrayElementAtIndex(clipIndex).intValue;


		for (int i = clipIndex + 1; i < clipFrameStartIndexes.arraySize; i++)
		{
			//clipFrameStartIndexes[i] = clipFrameStartIndexes[i] - frameCount;
			var valueAtIndex = clipFrameStartIndexes.GetArrayElementAtIndex(i);
			valueAtIndex.intValue = valueAtIndex.intValue - frameCount;
		}

		//clipFrameStartIndexes.RemoveAt(clipIndex);
		//clipFrameCounts.RemoveAt(clipIndex);
		clipFrameStartIndexes.DeleteArrayElementAtIndex(clipIndex);
		clipFrameCounts.DeleteArrayElementAtIndex(clipIndex);

		//From 0 to 2
		for (int i = frameCount - 1; i >= 0; i--)
		{
			//frames.RemoveAt(startFrameIndex + i);
			frames.DeleteArrayElementAtIndex(startFrameIndex + i);
		}

		//clipFPSs.RemoveAt(clipIndex);
		//clipWrapModes.RemoveAt(clipIndex);
		//clipLoopStarts.RemoveAt(clipIndex);
		clipFPSs.DeleteArrayElementAtIndex(clipIndex);
		clipWrapModes.DeleteArrayElementAtIndex(clipIndex);
		clipLoopStarts.DeleteArrayElementAtIndex(clipIndex);
		return true;
	}

	public bool AddClip(WeaverAnimationData.Clip clip)
	{
		if (HasClip(clip.Name))
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
		//clipNames.Add(clip.Name);
		//clipFrameStartIndexes.Add(frames.Count);
		//clipFrameCounts.Add(clip.Frames.Count);

		foreach (var frame in clip.Frames)
		{
			//frames.Add(frame);
			frames.InsertArrayElementAtIndex(frames.arraySize);
			frames.GetArrayElementAtIndex(frames.arraySize - 1).objectReferenceValue = frame;
		}

		clipFPSs.InsertArrayElementAtIndex(clipFPSs.arraySize);
		clipFPSs.GetArrayElementAtIndex(clipFPSs.arraySize - 1).floatValue = clip.FPS;

		clipWrapModes.InsertArrayElementAtIndex(clipWrapModes.arraySize);
		clipWrapModes.GetArrayElementAtIndex(clipWrapModes.arraySize - 1).enumValueIndex = (int)clip.WrapMode;

		clipLoopStarts.InsertArrayElementAtIndex(clipLoopStarts.arraySize);
		clipLoopStarts.GetArrayElementAtIndex(clipLoopStarts.arraySize - 1).intValue = clip.LoopStart;


		//clipFPSs.Add(clip.FPS);
		//clipWrapModes.Add(clip.WrapMode);
		//clipLoopStarts.Add(clip.LoopStart);

		return true;
	}
}

