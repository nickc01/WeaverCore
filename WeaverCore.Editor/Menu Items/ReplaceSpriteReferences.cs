using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using WeaverCore.Utilities;

public class ReplaceSpriteReferences : EditorWindow
{
	[MenuItem("WeaverCore/Tools/Replace Sprite References")]
	public static void Convert()
	{
		Display();
	}

	UnityEngine.Object obj;
	public List<Sprite> spriteList;
	ReorderableList sprites = null;
	bool removeExcessSprites = false;
	bool removeBlankAnimations = false;

	SerializedObject serializedObject;
	bool closed = false;

	Vector2 scrollPosition;


	public static ReplaceSpriteReferences Display()
	{
		var window = GetWindow<ReplaceSpriteReferences>();
		window.titleContent = new GUIContent("Replace Sprite References");
		window.Show();

		return window;
	}

	private void OnEnable()
    {
		serializedObject = new SerializedObject(this);

		sprites = new ReorderableList(serializedObject, serializedObject.FindProperty("spriteList"), false, true, true, true);

		sprites.drawHeaderCallback = (rect) => EditorGUI.LabelField(rect, "Sprites To Change");
		sprites.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
		{
			rect.y += 2f;
			rect.height = EditorGUIUtility.singleLineHeight;

			GUIContent objectLabel = new GUIContent($"Sprite {index}");
			EditorGUI.PropertyField(rect, serializedObject.FindProperty("spriteList").GetArrayElementAtIndex(index), objectLabel);
		};
	}

	private void OnGUI()
	{
		if (serializedObject == null || closed)
		{
			return;
		}
		scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
		serializedObject.Update();

		EditorGUILayout.LabelField("Add the textures you want to be replaced in the animation data below");
		EditorGUILayout.Space();
		sprites.DoLayoutList();

		EditorGUILayout.Space();
		if (GUILayout.Button(new GUIContent("Add Selected Sprites", "Adds all the textures that are highlighted in the \"Project\" Window")))
		{
			var loadedSprites = Selection.objects.OfType<Sprite>().ToList();

            foreach (var loadedSprite in Selection.objects.OfType<Texture2D>().SelectMany(t => AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(t)).OfType<Sprite>()))
            {
                if (!loadedSprites.Contains(loadedSprite))
                {
					loadedSprites.Add(loadedSprite);
                }
            }
			//var sprites = ;
			foreach (var sprite in loadedSprites)
			{
				var list = serializedObject.FindProperty("spriteList");
				list.arraySize++;
				list.GetArrayElementAtIndex(list.arraySize - 1).objectReferenceValue = sprite;
			}
		}

		obj = EditorGUILayout.ObjectField(new GUIContent("Object to Check", "The object to look over. Any textures on this object will be replaced with the sprites you selected in the list above"),obj,typeof(UnityEngine.Object),true);

		removeExcessSprites = EditorGUILayout.Toggle(new GUIContent("Remove Excess Sprites", "Any sprites that don't share any name in the list above will be removed from the animation data"), removeExcessSprites);
		removeBlankAnimations = EditorGUILayout.Toggle(new GUIContent("Remove Blank Animations", "If you are checking over a WeaverAnimationData object, any blank animations will be removed"), removeBlankAnimations);

		if (GUILayout.Button("Replace Sprites"))
		{
			closed = true;
			Close();
			ChangeOutSprites();
			//UnboundCoroutine.Start(Convert(spriteList, destroyOriginalTextures, outputAtlasName, cropTextures));
		}

		EditorGUILayout.EndScrollView();
		if (!closed)
		{
			serializedObject.ApplyModifiedProperties();
		}
	}

	void ChangeOutSprites()
    {
		var objectToCheck = new SerializedObject(obj);

		SerializedProperty prop = objectToCheck.GetIterator();
		if (prop.NextVisible(true))
		{
			do
			{
                if (prop.propertyType == SerializedPropertyType.ObjectReference)
                {
					var objectReference = prop.objectReferenceValue;
                    if (objectReference is Sprite oldSprite)
                    {
						ChangeOutReference(prop,oldSprite);
					}
				}
                else if (prop.isArray)
                {
                    for (int i = 0; i < prop.arraySize; i++)
                    {
						var element = prop.GetArrayElementAtIndex(i);
                        if (element.propertyType != SerializedPropertyType.ObjectReference)
                        {
							break;
                        }
                        if (element.objectReferenceValue is Sprite oldSprite)
                        {
							ChangeOutReference(element, oldSprite);
                        }
                    }
                }
			}
			while (prop.NextVisible(false));
		}
		objectToCheck.ApplyModifiedProperties();

        if (obj is WeaverAnimationData data)
        {
			var clipNames = data.AllClips.Select(c => c.Name).ToList();

            foreach (var clipName in clipNames)
            {
				var clip = WeaverAnimationDataEditor.GetClip(objectToCheck, clipName);
                if (clip.Frames.All(f => f == null))
                {
					WeaverAnimationDataEditor.RemoveClip(objectToCheck, clipName);
                }

			}

        }
	}

	void ChangeOutReference(SerializedProperty textureProperty, Sprite oldSprite)
    {
        if (oldSprite != null)
        {
			Sprite bestSprite = null;
			float bestDimensions = float.PositiveInfinity;

			var oldRect = oldSprite.rect;

            if (spriteList == null)
            {
				spriteList = new List<Sprite>();
            }

            foreach (var sprite in spriteList.Where(s => s.name == oldSprite.name))
            {
				var rect = sprite.rect;
				var dimensionDelta = Mathf.Abs(rect.width - oldRect.width) + Mathf.Abs(rect.height - oldRect.height);
                if (dimensionDelta < bestDimensions)
                {
					bestDimensions = dimensionDelta;
					bestSprite = sprite;
                }
            }

            if (bestSprite != null)
            {
				textureProperty.objectReferenceValue = bestSprite;
				return;
            }
        }

        if (removeExcessSprites)
        {
			textureProperty.objectReferenceValue = null;
        }
    }
}

