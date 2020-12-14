using System;
using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	public abstract class TMP_BaseShaderGUI : ShaderGUI
	{
		protected class ShaderFeature
		{
			public string undoLabel;

			public GUIContent label;

			public GUIContent[] keywordLabels;

			public string[] keywords;

			private int state;

			public bool Active
			{
				get
				{
					return state >= 0;
				}
			}

			public int State
			{
				get
				{
					return state;
				}
			}

			public void ReadState(Material material)
			{
				for (int i = 0; i < keywords.Length; i++)
				{
					if (material.IsKeywordEnabled(keywords[i]))
					{
						state = i;
						return;
					}
				}
				state = -1;
			}

			public void SetActive(bool active, Material material)
			{
				state = ((!active) ? (-1) : 0);
				SetStateKeywords(material);
			}

			public void DoPopup(MaterialEditor editor, Material material)
			{
				EditorGUI.BeginChangeCheck();
				int num = EditorGUILayout.Popup(label, state + 1, keywordLabels);
				if (EditorGUI.EndChangeCheck())
				{
					state = num - 1;
					editor.RegisterPropertyChangeUndo(undoLabel);
					SetStateKeywords(material);
				}
			}

			private void SetStateKeywords(Material material)
			{
				for (int i = 0; i < keywords.Length; i++)
				{
					if (i == state)
					{
						material.EnableKeyword(keywords[i]);
					}
					else
					{
						material.DisableKeyword(keywords[i]);
					}
				}
			}
		}

		protected class MaterialPanel
		{
			private string key;

			private string label;

			private bool expanded;

			public bool Expanded
			{
				get
				{
					return expanded;
				}
			}

			public string Label
			{
				get
				{
					return label;
				}
			}

			public MaterialPanel(string name, bool expandedByDefault)
			{
				label = "<b>" + name + "</b> - <i>Settings</i> -";
				key = "TexMeshPro.material." + name + ".expanded";
				expanded = EditorPrefs.GetBool(key, expandedByDefault);
			}

			public void ToggleExpanded()
			{
				expanded = !expanded;
				EditorPrefs.SetBool(key, expanded);
			}
		}

		private static GUIContent tempLabel;

		private static int undoRedoCount;

		private static int lastSeenUndoRedoCount;

		private static float[][] tempFloats;

		protected static GUIContent[] xywhVectorLabels;

		protected static GUIContent[] lbrtVectorLabels;

		private bool isNewGUI = true;

		private float dragAndDropMinY;

		protected MaterialEditor editor;

		protected Material material;

		protected MaterialProperty[] properties;

		static TMP_BaseShaderGUI()
		{
			tempLabel = new GUIContent();
			tempFloats = new float[5][]
			{
				null,
				new float[1],
				new float[2],
				new float[3],
				new float[4]
			};
			xywhVectorLabels = new GUIContent[4]
			{
				new GUIContent("X"),
				new GUIContent("Y"),
				new GUIContent("W", "Width"),
				new GUIContent("H", "Height")
			};
			lbrtVectorLabels = new GUIContent[4]
			{
				new GUIContent("L", "Left"),
				new GUIContent("B", "Bottom"),
				new GUIContent("R", "Right"),
				new GUIContent("T", "Top")
			};
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, (Undo.UndoRedoCallback)delegate
			{
				undoRedoCount++;
			});
		}

		private void PrepareGUI()
		{
			isNewGUI = false;
			TMP_UIStyleManager.GetUIStyles();
			ShaderUtilities.GetShaderPropertyIDs();
			if (lastSeenUndoRedoCount != undoRedoCount)
			{
				TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, material);
			}
			lastSeenUndoRedoCount = undoRedoCount;
		}

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			editor = materialEditor;
			material = (materialEditor.target as Material);
			this.properties = properties;
			if (isNewGUI)
			{
				PrepareGUI();
			}
			EditorGUIUtility.labelWidth = 130f;
			EditorGUIUtility.fieldWidth = 50f;
			DoDragAndDropBegin();
			EditorGUI.BeginChangeCheck();
			DoGUI();
			if (EditorGUI.EndChangeCheck())
			{
				TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, material);
			}
			DoDragAndDropEnd();
		}

		protected abstract void DoGUI();

		protected bool DoPanelHeader(MaterialPanel panel)
		{
			if (GUILayout.Button(panel.Label, TMP_UIStyleManager.Group_Label))
			{
				panel.ToggleExpanded();
			}
			return panel.Expanded;
		}

		protected bool DoPanelHeader(MaterialPanel panel, ShaderFeature feature, bool readState = true)
		{
			Rect controlRect = EditorGUILayout.GetControlRect(false, 22f);
			GUI.Label(controlRect, GUIContent.none, TMP_UIStyleManager.Group_Label);
			if (GUI.Button(new Rect(controlRect.x, controlRect.y, 250f, controlRect.height), panel.Label, TMP_UIStyleManager.Group_Label_Left))
			{
				panel.ToggleExpanded();
			}
			if (readState)
			{
				feature.ReadState(material);
			}
			EditorGUI.BeginChangeCheck();
			float labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 70f;
			bool active = EditorGUI.Toggle(new Rect(controlRect.width - 90f, controlRect.y + 3f, 90f, 22f), new GUIContent("Enable ->"), feature.Active);
			EditorGUIUtility.labelWidth = labelWidth;
			if (EditorGUI.EndChangeCheck())
			{
				editor.RegisterPropertyChangeUndo(feature.undoLabel);
				feature.SetActive(active, material);
			}
			return panel.Expanded;
		}

		private MaterialProperty BeginProperty(string name)
		{
			MaterialProperty materialProperty = ShaderGUI.FindProperty(name, properties);
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = materialProperty.hasMixedValue;
			editor.BeginAnimatedCheck(materialProperty);
			return materialProperty;
		}

		private bool EndProperty()
		{
			editor.EndAnimatedCheck();
			EditorGUI.showMixedValue = false;
			return EditorGUI.EndChangeCheck();
		}

		protected void DoPopup(string name, string label, GUIContent[] options)
		{
			MaterialProperty materialProperty = BeginProperty(name);
			tempLabel.text = label;
			int num = EditorGUILayout.Popup(tempLabel, (int)materialProperty.floatValue, options);
			if (EndProperty())
			{
				materialProperty.floatValue = num;
			}
		}

		protected void DoCubeMap(string name, string label)
		{
			DoTexture(name, label, typeof(Cubemap));
		}

		protected void DoTexture2D(string name, string label, bool withTilingOffset = false, string[] speedNames = null)
		{
			DoTexture(name, label, typeof(Texture2D), withTilingOffset, speedNames);
		}

		private void DoTexture(string name, string label, Type type, bool withTilingOffset = false, string[] speedNames = null)
		{
			MaterialProperty materialProperty = BeginProperty(name);
			Rect controlRect = EditorGUILayout.GetControlRect(true, 60f);
			float width = controlRect.width;
			controlRect.width = EditorGUIUtility.labelWidth + 60f;
			tempLabel.text = label;
			UnityEngine.Object @object = EditorGUI.ObjectField(controlRect, tempLabel, materialProperty.textureValue, type, false);
			if (EndProperty())
			{
				materialProperty.textureValue = (@object as Texture);
			}
			controlRect.x += controlRect.width + 4f;
			controlRect.width = width - controlRect.width - 4f;
			controlRect.height = EditorGUIUtility.singleLineHeight;
			if (withTilingOffset)
			{
				DoTilingOffset(controlRect, materialProperty);
				controlRect.y += (controlRect.height + 2f) * 2f;
			}
			if (speedNames != null)
			{
				DoUVSpeed(controlRect, speedNames);
			}
		}

		private void DoTilingOffset(Rect rect, MaterialProperty property)
		{
			float labelWidth = EditorGUIUtility.labelWidth;
			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUIUtility.labelWidth = 40f;
			Vector4 textureScaleAndOffset = property.textureScaleAndOffset;
			bool flag = false;
			float[] array = tempFloats[2];
			tempLabel.text = "Tiling";
			Rect position = EditorGUI.PrefixLabel(rect, tempLabel);
			array[0] = textureScaleAndOffset.x;
			array[1] = textureScaleAndOffset.y;
			EditorGUI.BeginChangeCheck();
			EditorGUI.MultiFloatField(position, xywhVectorLabels, array);
			if (EndProperty())
			{
				textureScaleAndOffset.x = array[0];
				textureScaleAndOffset.y = array[1];
				flag = true;
			}
			rect.y += rect.height + 2f;
			tempLabel.text = "Offset";
			position = EditorGUI.PrefixLabel(rect, tempLabel);
			array[0] = textureScaleAndOffset.z;
			array[1] = textureScaleAndOffset.w;
			EditorGUI.BeginChangeCheck();
			EditorGUI.MultiFloatField(position, xywhVectorLabels, array);
			if (EndProperty())
			{
				textureScaleAndOffset.z = array[0];
				textureScaleAndOffset.w = array[1];
				flag = true;
			}
			if (flag)
			{
				property.textureScaleAndOffset = textureScaleAndOffset;
			}
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUI.indentLevel = indentLevel;
		}

		protected void DoUVSpeed(Rect rect, string[] names)
		{
			float labelWidth = EditorGUIUtility.labelWidth;
			int indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			EditorGUIUtility.labelWidth = 40f;
			tempLabel.text = "Speed";
			rect = EditorGUI.PrefixLabel(rect, tempLabel);
			EditorGUIUtility.labelWidth = 13f;
			rect.width = rect.width * 0.5f - 1f;
			DoFloat(rect, names[0], "X");
			rect.x += rect.width + 2f;
			DoFloat(rect, names[1], "Y");
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUI.indentLevel = indentLevel;
		}

		protected void DoToggle(string name, string label)
		{
			MaterialProperty materialProperty = BeginProperty(name);
			tempLabel.text = label;
			bool flag = EditorGUILayout.Toggle(tempLabel, materialProperty.floatValue == 1f);
			if (EndProperty())
			{
				materialProperty.floatValue = (flag ? 1f : 0f);
			}
		}

		protected void DoFloat(string name, string label)
		{
			MaterialProperty materialProperty = BeginProperty(name);
			Rect controlRect = EditorGUILayout.GetControlRect();
			controlRect.width = 225f;
			tempLabel.text = label;
			float floatValue = EditorGUI.FloatField(controlRect, tempLabel, materialProperty.floatValue);
			if (EndProperty())
			{
				materialProperty.floatValue = floatValue;
			}
		}

		protected void DoColor(string name, string label)
		{
			MaterialProperty materialProperty = BeginProperty(name);
			tempLabel.text = label;
			Color colorValue = EditorGUI.ColorField(EditorGUILayout.GetControlRect(), tempLabel, materialProperty.colorValue);
			if (EndProperty())
			{
				materialProperty.colorValue = colorValue;
			}
		}

		private void DoFloat(Rect rect, string name, string label)
		{
			MaterialProperty materialProperty = BeginProperty(name);
			tempLabel.text = label;
			float floatValue = EditorGUI.FloatField(rect, tempLabel, materialProperty.floatValue);
			if (EndProperty())
			{
				materialProperty.floatValue = floatValue;
			}
		}

		protected void DoSlider(string name, string label)
		{
			MaterialProperty materialProperty = BeginProperty(name);
			Vector2 rangeLimits = materialProperty.rangeLimits;
			tempLabel.text = label;
			float floatValue = EditorGUI.Slider(EditorGUILayout.GetControlRect(), tempLabel, materialProperty.floatValue, rangeLimits.x, rangeLimits.y);
			if (EndProperty())
			{
				materialProperty.floatValue = floatValue;
			}
		}

		protected void DoVector3(string name, string label)
		{
			MaterialProperty materialProperty = BeginProperty(name);
			tempLabel.text = label;
			Vector4 vectorValue = EditorGUILayout.Vector3Field(tempLabel, materialProperty.vectorValue);
			if (EndProperty())
			{
				materialProperty.vectorValue = vectorValue;
			}
		}

		protected void DoVector(string name, string label, GUIContent[] subLabels)
		{
			MaterialProperty materialProperty = BeginProperty(name);
			Rect controlRect = EditorGUILayout.GetControlRect();
			tempLabel.text = label;
			controlRect = EditorGUI.PrefixLabel(controlRect, tempLabel);
			Vector4 vectorValue = materialProperty.vectorValue;
			float[] array = tempFloats[subLabels.Length];
			for (int i = 0; i < subLabels.Length; i++)
			{
				array[i] = vectorValue[i];
			}
			EditorGUI.MultiFloatField(controlRect, subLabels, array);
			if (EndProperty())
			{
				for (int j = 0; j < subLabels.Length; j++)
				{
					vectorValue[j] = array[j];
				}
				materialProperty.vectorValue = vectorValue;
			}
		}

		protected void DoEmptyLine()
		{
			GUILayout.Space(EditorGUIUtility.singleLineHeight);
		}

		private void DoDragAndDropBegin()
		{
			dragAndDropMinY = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true)).y;
		}

		private void DoDragAndDropEnd()
		{
			Rect rect = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
			Event current = Event.current;
			if (current.type == EventType.DragUpdated)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				current.Use();
			}
			else if (current.type == EventType.DragPerform && Rect.MinMaxRect(rect.xMin, dragAndDropMinY, rect.xMax, rect.yMax).Contains(current.mousePosition))
			{
				DragAndDrop.AcceptDrag();
				current.Use();
				Material material = DragAndDrop.objectReferences[0] as Material;
				if ((bool)material && material != this.material)
				{
					PerformDrop(material);
				}
			}
		}

		private void PerformDrop(Material droppedMaterial)
		{
			Texture texture = droppedMaterial.GetTexture(ShaderUtilities.ID_MainTex);
			if (!texture)
			{
				return;
			}
			Texture texture2 = material.GetTexture(ShaderUtilities.ID_MainTex);
			TMP_FontAsset tMP_FontAsset = null;
			if (texture != texture2)
			{
				tMP_FontAsset = TMP_EditorUtility.FindMatchingFontAsset(droppedMaterial);
				if (!tMP_FontAsset)
				{
					return;
				}
			}
			GameObject[] gameObjects = Selection.gameObjects;
			foreach (GameObject gameObject in gameObjects)
			{
				if ((bool)tMP_FontAsset)
				{
					TMP_Text component = gameObject.GetComponent<TMP_Text>();
					if ((bool)(UnityEngine.Object)(object)component)
					{
						Undo.RecordObject((UnityEngine.Object)(object)component, "Font Asset Change");
						component.font = tMP_FontAsset;
					}
				}
				TMPro_EventManager.ON_DRAG_AND_DROP_MATERIAL_CHANGED(gameObject, material, droppedMaterial);
				EditorUtility.SetDirty(gameObject);
			}
		}
	}
}
