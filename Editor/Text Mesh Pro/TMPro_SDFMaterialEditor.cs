using System;
using System.Linq;
using System.Runtime.InteropServices;
using TMPro;
using TMPro.EditorUtilities;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class TMPro_SDFMaterialEditor : MaterialEditor
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	private struct m_foldout
	{
		public static bool editorPanel = true;

		public static bool face = true;

		public static bool outline = true;

		public static bool underlay = false;

		public static bool bevel = false;

		public static bool light = false;

		public static bool bump = false;

		public static bool env = false;

		public static bool glow = false;

		public static bool debug = false;
	}

	private enum FoldoutType
	{
		face,
		outline,
		underlay,
		bevel,
		light,
		bump,
		env,
		glow,
		debug
	}

	private enum WarningTypes
	{
		None,
		ShaderMismatch,
		FontAtlasMismatch
	}

	private enum ShaderTypes
	{
		None,
		Bitmap,
		SDF
	}

	private enum Bevel_Types
	{
		OuterBevel,
		InnerBevel
	}

	private enum Underlay_Types
	{
		Normal,
		Inner
	}

	private string m_warningMsg;

	private WarningTypes m_warning = WarningTypes.None;

	private static int m_eventID;

	private TMP_Text m_textComponent;

	private MaterialProperty m_faceColor;

	private MaterialProperty m_faceTex;

	private MaterialProperty m_faceUVSpeedX;

	private MaterialProperty m_faceUVSpeedY;

	private MaterialProperty m_faceDilate;

	private MaterialProperty m_faceShininess;

	private MaterialProperty m_outlineColor;

	private MaterialProperty m_outlineTex;

	private MaterialProperty m_outlineUVSpeedX;

	private MaterialProperty m_outlineUVSpeedY;

	private MaterialProperty m_outlineThickness;

	private MaterialProperty m_outlineSoftness;

	private MaterialProperty m_outlineShininess;

	private MaterialProperty m_bevel;

	private MaterialProperty m_bevelOffset;

	private MaterialProperty m_bevelWidth;

	private MaterialProperty m_bevelClamp;

	private MaterialProperty m_bevelRoundness;

	private MaterialProperty m_underlayColor;

	private MaterialProperty m_underlayOffsetX;

	private MaterialProperty m_underlayOffsetY;

	private MaterialProperty m_underlayDilate;

	private MaterialProperty m_underlaySoftness;

	private MaterialProperty m_lightAngle;

	private MaterialProperty m_specularColor;

	private MaterialProperty m_specularPower;

	private MaterialProperty m_reflectivity;

	private MaterialProperty m_diffuse;

	private MaterialProperty m_ambientLight;

	private MaterialProperty m_bumpMap;

	private MaterialProperty m_bumpFace;

	private MaterialProperty m_bumpOutline;

	private MaterialProperty m_reflectFaceColor;

	private MaterialProperty m_reflectOutlineColor;

	private MaterialProperty m_reflectTex;

	private MaterialProperty m_reflectRotation;

	private MaterialProperty m_specColor;

	private MaterialProperty m_glowColor;

	private MaterialProperty m_glowInner;

	private MaterialProperty m_glowOffset;

	private MaterialProperty m_glowPower;

	private MaterialProperty m_glowOuter;

	private MaterialProperty m_mainTex;

	private MaterialProperty m_texSampleWidth;

	private MaterialProperty m_texSampleHeight;

	private MaterialProperty m_gradientScale;

	private MaterialProperty m_scaleX;

	private MaterialProperty m_scaleY;

	private MaterialProperty m_PerspectiveFilter;

	private MaterialProperty m_vertexOffsetX;

	private MaterialProperty m_vertexOffsetY;

	private MaterialProperty m_maskCoord;

	private MaterialProperty m_clipRect;

	private MaterialProperty m_maskSoftnessX;

	private MaterialProperty m_maskSoftnessY;

	private MaterialProperty m_maskTex;

	private MaterialProperty m_maskTexInverse;

	private MaterialProperty m_maskTexEdgeColor;

	private MaterialProperty m_maskTexEdgeSoftness;

	private MaterialProperty m_maskTexWipeControl;

	private MaterialProperty m_stencilID;

	private MaterialProperty m_stencilOp;

	private MaterialProperty m_stencilComp;

	private MaterialProperty m_stencilReadMask;

	private MaterialProperty m_stencilWriteMask;

	private MaterialProperty m_shaderFlags;

	private MaterialProperty m_scaleRatio_A;

	private MaterialProperty m_scaleRatio_B;

	private MaterialProperty m_scaleRatio_C;

	private string[] m_bevelOptions = new string[3]
	{
		"Outer Bevel",
		"Inner Bevel",
		"--"
	};

	private int m_bevelSelection;

	private MaskingTypes m_mask;

	private Underlay_Types m_underlaySelection = Underlay_Types.Normal;

	private string[] m_Keywords;

	private bool isOutlineEnabled;

	private bool isRatiosEnabled;

	private bool isBevelEnabled;

	private bool isGlowEnabled;

	private bool isUnderlayEnabled;

	private bool havePropertiesChanged = false;

	private Rect m_inspectorStartRegion;

	private Rect m_inspectorEndRegion;

	public override void OnEnable()
	{
		base.OnEnable();
		TMP_UIStyleManager.GetUIStyles();
		ShaderUtilities.GetShaderPropertyIDs();
		Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(OnUndoRedo));
		if (Selection.activeGameObject != null)
		{
			m_textComponent = Selection.activeGameObject.GetComponent<TMP_Text>();
		}
	}

	public override void OnDisable()
	{
		base.OnDisable();
		Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(OnUndoRedo));
	}

	protected override void OnHeaderGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnHeaderGUI();
		if (EditorGUI.EndChangeCheck())
		{
			m_foldout.editorPanel = InternalEditorUtility.GetIsInspectorExpanded(base.target);
		}
		GUI.skin.GetStyle("HelpBox").richText = true;
		WarningTypes warning = m_warning;
		if (warning == WarningTypes.FontAtlasMismatch)
		{
			EditorGUILayout.HelpBox(m_warningMsg, MessageType.Warning);
		}
	}

	public override void OnInspectorGUI()
	{
		if (!m_foldout.editorPanel)
		{
			return;
		}
		Material material = base.target as Material;
		if (base.targets.Length > 1)
		{
			for (int i = 0; i < base.targets.Length; i++)
			{
				Material material2 = base.targets[i] as Material;
				if (material.shader != material2.shader)
				{
					return;
				}
			}
		}
		ReadMaterialProperties();
		if (!material.HasProperty(ShaderUtilities.ID_GradientScale))
		{
			m_warning = WarningTypes.ShaderMismatch;
			m_warningMsg = "The selected Shader is not compatible with the currently selected Font Asset type.";
			EditorGUILayout.HelpBox(m_warningMsg, MessageType.Warning);
			return;
		}
		m_Keywords = material.shaderKeywords;
		isOutlineEnabled = m_Keywords.Contains("OUTLINE_ON");
		isBevelEnabled = m_Keywords.Contains("BEVEL_ON");
		isGlowEnabled = m_Keywords.Contains("GLOW_ON");
		isRatiosEnabled = !m_Keywords.Contains("RATIOS_OFF");
		if (m_Keywords.Contains("UNDERLAY_ON"))
		{
			isUnderlayEnabled = true;
			m_underlaySelection = Underlay_Types.Normal;
		}
		else if (m_Keywords.Contains("UNDERLAY_INNER"))
		{
			isUnderlayEnabled = true;
			m_underlaySelection = Underlay_Types.Inner;
		}
		else
		{
			isUnderlayEnabled = false;
		}
		if (m_Keywords.Contains("MASK_HARD"))
		{
			m_mask = MaskingTypes.MaskHard;
		}
		else if (m_Keywords.Contains("MASK_SOFT"))
		{
			m_mask = MaskingTypes.MaskSoft;
		}
		else
		{
			m_mask = MaskingTypes.MaskOff;
		}
		if (m_shaderFlags.hasMixedValue)
		{
			m_bevelSelection = 2;
		}
		else
		{
			m_bevelSelection = ((int)m_shaderFlags.floatValue & 1);
		}
		m_inspectorStartRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
		EditorGUIUtility.labelWidth = 130f;
		EditorGUIUtility.fieldWidth = 50f;
		EditorGUI.indentLevel = 0;
		if (GUILayout.Button("<b>Face</b> - <i>Settings</i> -", TMP_UIStyleManager.Group_Label))
		{
			m_foldout.face = !m_foldout.face;
		}
		if (m_foldout.face)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.indentLevel = 1;
			ColorProperty(m_faceColor, "Color");
			if (material.HasProperty("_FaceTex"))
			{
				DrawTextureProperty(m_faceTex, "Texture");
				DrawUVProperty(new MaterialProperty[2]
				{
					m_faceUVSpeedX,
					m_faceUVSpeedY
				}, "UV Speed");
			}
			DrawRangeProperty(m_outlineSoftness, "Softness");
			DrawRangeProperty(m_faceDilate, "Dilate");
			if (material.HasProperty("_FaceShininess"))
			{
				DrawRangeProperty(m_faceShininess, "Gloss");
			}
			if (EditorGUI.EndChangeCheck())
			{
				havePropertiesChanged = true;
			}
		}
		if (material.HasProperty("_OutlineColor"))
		{
			if (material.HasProperty("_Bevel"))
			{
				if (GUILayout.Button("<b>Outline</b> - <i>Settings</i> -", TMP_UIStyleManager.Group_Label))
				{
					m_foldout.outline = !m_foldout.outline;
				}
			}
			else
			{
				isOutlineEnabled = DrawTogglePanel(FoldoutType.outline, "<b>Outline</b> - <i>Settings</i> -", isOutlineEnabled, "OUTLINE_ON");
			}
			EditorGUI.indentLevel = 0;
			if (m_foldout.outline)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.indentLevel = 1;
				ColorProperty(m_outlineColor, "Color");
				if (material.HasProperty("_OutlineTex"))
				{
					DrawTextureProperty(m_outlineTex, "Texture");
					DrawUVProperty(new MaterialProperty[2]
					{
						m_outlineUVSpeedX,
						m_outlineUVSpeedY
					}, "UV Speed");
				}
				DrawRangeProperty(m_outlineThickness, "Thickness");
				if (material.HasProperty("_OutlineShininess"))
				{
					DrawRangeProperty(m_outlineShininess, "Gloss");
				}
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
			}
		}
		if (material.HasProperty("_UnderlayColor"))
		{
			string keyword = (m_underlaySelection == Underlay_Types.Normal) ? "UNDERLAY_ON" : "UNDERLAY_INNER";
			isUnderlayEnabled = DrawTogglePanel(FoldoutType.underlay, "<b>Underlay</b> - <i>Settings</i> -", isUnderlayEnabled, keyword);
			if (m_foldout.underlay)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.indentLevel = 1;
				m_underlaySelection = (Underlay_Types)(object)EditorGUILayout.EnumPopup("Underlay Type", m_underlaySelection);
				if (GUI.changed)
				{
					SetUnderlayKeywords();
				}
				ColorProperty(m_underlayColor, "Color");
				DrawRangeProperty(m_underlayOffsetX, "OffsetX");
				DrawRangeProperty(m_underlayOffsetY, "OffsetY");
				DrawRangeProperty(m_underlayDilate, "Dilate");
				DrawRangeProperty(m_underlaySoftness, "Softness");
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
			}
		}
		if (material.HasProperty("_Bevel"))
		{
			isBevelEnabled = DrawTogglePanel(FoldoutType.bevel, "<b>Bevel</b> - <i>Settings</i> -", isBevelEnabled, "BEVEL_ON");
			if (m_foldout.bevel)
			{
				EditorGUI.indentLevel = 1;
				GUI.changed = false;
				m_bevelSelection = (EditorGUILayout.Popup("Type", m_bevelSelection, m_bevelOptions) & 1);
				if (GUI.changed)
				{
					havePropertiesChanged = true;
					m_shaderFlags.floatValue = m_bevelSelection;
				}
				EditorGUI.BeginChangeCheck();
				DrawRangeProperty(m_bevel, "Amount");
				DrawRangeProperty(m_bevelOffset, "Offset");
				DrawRangeProperty(m_bevelWidth, "Width");
				DrawRangeProperty(m_bevelRoundness, "Roundness");
				DrawRangeProperty(m_bevelClamp, "Clamp");
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
			}
		}
		if (material.HasProperty("_SpecularColor") || material.HasProperty("_SpecColor"))
		{
			isBevelEnabled = DrawTogglePanel(FoldoutType.light, "<b>Lighting</b> - <i>Settings</i> -", isBevelEnabled, "BEVEL_ON");
			if (m_foldout.light)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.indentLevel = 1;
				if (material.HasProperty("_LightAngle"))
				{
					DrawRangeProperty(m_lightAngle, "Light Angle");
					ColorProperty(m_specularColor, "Specular Color");
					DrawRangeProperty(m_specularPower, "Specular Power");
					DrawRangeProperty(m_reflectivity, "Reflectivity Power");
					DrawRangeProperty(m_diffuse, "Diffuse Shadow");
					DrawRangeProperty(m_ambientLight, "Ambient Shadow");
				}
				else
				{
					ColorProperty(m_specColor, "Specular Color");
				}
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
			}
		}
		if (material.HasProperty("_BumpMap"))
		{
			isBevelEnabled = DrawTogglePanel(FoldoutType.bump, "<b>BumpMap</b> - <i>Settings</i> -", isBevelEnabled, "BEVEL_ON");
			if (m_foldout.bump)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.indentLevel = 1;
				DrawTextureProperty(m_bumpMap, "Texture");
				DrawRangeProperty(m_bumpFace, "Face");
				DrawRangeProperty(m_bumpOutline, "Outline");
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
			}
		}
		if (material.HasProperty("_Cube"))
		{
			isBevelEnabled = DrawTogglePanel(FoldoutType.env, "<b>EnvMap</b> - <i>Settings</i> -", isBevelEnabled, "BEVEL_ON");
			if (m_foldout.env)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.indentLevel = 1;
				ColorProperty(m_reflectFaceColor, "Face Color");
				ColorProperty(m_reflectOutlineColor, "Outline Color");
				DrawTextureProperty(m_reflectTex, "Texture");
				if (material.HasProperty("_Cube"))
				{
					DrawVectorProperty(m_reflectRotation, "EnvMap Rotation");
				}
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
			}
		}
		if (material.HasProperty("_GlowColor"))
		{
			isGlowEnabled = DrawTogglePanel(FoldoutType.glow, "<b>Glow</b> - <i>Settings</i> -", isGlowEnabled, "GLOW_ON");
			if (m_foldout.glow)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.indentLevel = 1;
				ColorProperty(m_glowColor, "Color");
				DrawRangeProperty(m_glowOffset, "Offset");
				DrawRangeProperty(m_glowInner, "Inner");
				DrawRangeProperty(m_glowOuter, "Outer");
				DrawRangeProperty(m_glowPower, "Power");
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
			}
		}
		if (material.HasProperty("_GradientScale"))
		{
			EditorGUI.indentLevel = 0;
			if (GUILayout.Button("<b>Debug</b> - <i>Settings</i> -", TMP_UIStyleManager.Group_Label))
			{
				m_foldout.debug = !m_foldout.debug;
			}
			if (m_foldout.debug)
			{
				EditorGUI.indentLevel = 1;
				EditorGUI.BeginChangeCheck();
				DrawTextureProperty(m_mainTex, "Font Atlas");
				DrawFloatProperty(m_gradientScale, "Gradient Scale");
				DrawFloatProperty(m_texSampleWidth, "Texture Width");
				DrawFloatProperty(m_texSampleHeight, "Texture Height");
				GUILayout.Space(20f);
				DrawFloatProperty(m_scaleX, "Scale X");
				DrawFloatProperty(m_scaleY, "Scale Y");
				DrawRangeProperty(m_PerspectiveFilter, "Perspective Filter");
				GUILayout.Space(20f);
				DrawFloatProperty(m_vertexOffsetX, "Offset X");
				DrawFloatProperty(m_vertexOffsetY, "Offset Y");
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
				if (material.HasProperty("_ClipRect"))
				{
					GUILayout.Space(15f);
					m_mask = (MaskingTypes)(object)EditorGUILayout.EnumPopup("Mask", m_mask);
					if (GUI.changed)
					{
						havePropertiesChanged = true;
						SetMaskKeywords(m_mask);
					}
					if (m_mask != 0)
					{
						EditorGUI.BeginChangeCheck();
						Draw2DBoundsProperty(m_maskCoord, "Mask Bounds");
						DrawFloatProperty(m_maskSoftnessX, "Softness X");
						DrawFloatProperty(m_maskSoftnessY, "Softness Y");
						if (material.HasProperty("_MaskEdgeColor"))
						{
							DrawTextureProperty(m_maskTex, "Mask Texture");
							bool value = (m_maskTexInverse.floatValue != 0f) ? true : false;
							value = EditorGUILayout.Toggle("Inverse Mask", value);
							ColorProperty(m_maskTexEdgeColor, "Edge Color");
							RangeProperty(m_maskTexEdgeSoftness, "Edge Softness");
							RangeProperty(m_maskTexWipeControl, "Wipe Position");
							if (EditorGUI.EndChangeCheck())
							{
								m_maskTexInverse.floatValue = (value ? 1 : 0);
								havePropertiesChanged = true;
							}
						}
					}
					GUILayout.Space(15f);
				}
				EditorGUI.BeginChangeCheck();
				Draw2DRectBoundsProperty(m_clipRect, "_ClipRect");
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
				GUILayout.Space(20f);
				if (material.HasProperty("_Stencil"))
				{
					FloatProperty(m_stencilID, "Stencil ID");
					FloatProperty(m_stencilComp, "Stencil Comp");
				}
				GUILayout.Space(20f);
				GUI.changed = false;
				isRatiosEnabled = EditorGUILayout.Toggle("Enable Ratios?", isRatiosEnabled);
				if (GUI.changed)
				{
					SetKeyword(!isRatiosEnabled, "RATIOS_OFF");
				}
				EditorGUI.BeginChangeCheck();
				DrawFloatProperty(m_scaleRatio_A, "Scale Ratio A");
				DrawFloatProperty(m_scaleRatio_B, "Scale Ratio B");
				DrawFloatProperty(m_scaleRatio_C, "Scale Ratio C");
				if (EditorGUI.EndChangeCheck())
				{
					havePropertiesChanged = true;
				}
			}
		}
		m_inspectorEndRegion = GUILayoutUtility.GetRect(0f, 0f, GUILayout.ExpandWidth(true));
		DragAndDropGUI();
		if (havePropertiesChanged)
		{
			havePropertiesChanged = false;
			PropertiesChanged();
			EditorUtility.SetDirty(base.target);
			TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, base.target as Material);
		}
	}

	private void DragAndDropGUI()
	{
		Event current = Event.current;
		Rect rect = new Rect(m_inspectorStartRegion.x, m_inspectorStartRegion.y, m_inspectorEndRegion.width, m_inspectorEndRegion.y - m_inspectorStartRegion.y);
		EventType type = current.type;
		if ((uint)(type - 9) > 1u || !rect.Contains(current.mousePosition))
		{
			return;
		}
		DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
		if (current.type == EventType.DragPerform)
		{
			DragAndDrop.AcceptDrag();
			Material material = base.target as Material;
			Texture texture = material.GetTexture(ShaderUtilities.ID_MainTex);
			Material material2 = DragAndDrop.objectReferences[0] as Material;
			Texture texture2 = material2.GetTexture(ShaderUtilities.ID_MainTex);
			TMP_FontAsset tMP_FontAsset = null;
			if (material2 == null || material2 == material || material2 == null || texture2 == null)
			{
				return;
			}
			if (texture2.GetInstanceID() != texture.GetInstanceID())
			{
				tMP_FontAsset = TMP_EditorUtility.FindMatchingFontAsset(material2);
				if (tMP_FontAsset == null)
				{
					return;
				}
			}
			GameObject[] gameObjects = Selection.gameObjects;
			for (int i = 0; i < gameObjects.Length; i++)
			{
				if (tMP_FontAsset != null)
				{
					TMP_Text component = gameObjects[i].GetComponent<TMP_Text>();
					if ((UnityEngine.Object)(object)component != null)
					{
						Undo.RecordObject((UnityEngine.Object)(object)component, "Font Asset Change");
						component.font = tMP_FontAsset;
					}
				}
				TMPro_EventManager.ON_DRAG_AND_DROP_MATERIAL_CHANGED(gameObjects[i], material, material2);
				EditorUtility.SetDirty(gameObjects[i]);
			}
		}
		current.Use();
	}

	private void OnUndoRedo()
	{
		int currentGroup = Undo.GetCurrentGroup();
		int eventID = m_eventID;
		if (currentGroup != eventID)
		{
			TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, base.target as Material);
			m_eventID = currentGroup;
		}
	}

	private UndoPropertyModification[] OnUndoRedoEvent(UndoPropertyModification[] modifications)
	{
		return modifications;
	}

	private bool DrawTogglePanel(FoldoutType type, string label, bool toggle, string keyword)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = EditorGUIUtility.fieldWidth;
		EditorGUI.indentLevel = 0;
		Rect controlRect = EditorGUILayout.GetControlRect(false, 22f);
		GUI.Label(controlRect, GUIContent.none, TMP_UIStyleManager.Group_Label);
		if (GUI.Button(new Rect(controlRect.x, controlRect.y, 250f, controlRect.height), label, TMP_UIStyleManager.Group_Label_Left))
		{
			switch (type)
			{
			case FoldoutType.outline:
				m_foldout.outline = !m_foldout.outline;
				break;
			case FoldoutType.underlay:
				m_foldout.underlay = !m_foldout.underlay;
				break;
			case FoldoutType.bevel:
				m_foldout.bevel = !m_foldout.bevel;
				break;
			case FoldoutType.light:
				m_foldout.light = !m_foldout.light;
				break;
			case FoldoutType.bump:
				m_foldout.bump = !m_foldout.bump;
				break;
			case FoldoutType.env:
				m_foldout.env = !m_foldout.env;
				break;
			case FoldoutType.glow:
				m_foldout.glow = !m_foldout.glow;
				break;
			}
		}
		EditorGUIUtility.labelWidth = 70f;
		EditorGUI.BeginChangeCheck();
		Material material = base.target as Material;
		if (!material.HasProperty("_FaceShininess") || keyword != "BEVEL_ON")
		{
			toggle = EditorGUI.Toggle(new Rect(controlRect.width - 90f, controlRect.y + 3f, 90f, 22f), new GUIContent("Enable ->"), toggle);
			if (EditorGUI.EndChangeCheck())
			{
				SetKeyword(toggle, keyword);
				havePropertiesChanged = true;
			}
		}
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
		return toggle;
	}

	private void DrawUVProperty(MaterialProperty[] properties, string label)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = EditorGUIUtility.fieldWidth;
		Rect controlRect = EditorGUILayout.GetControlRect(false, 20f);
		Rect position = new Rect(controlRect.x + 15f, controlRect.y, controlRect.width - 55f, 20f);
		Rect position2 = new Rect(130f, controlRect.y, 80f, 18f);
		GUI.Label(position, label);
		EditorGUIUtility.labelWidth = 35f;
		FloatProperty(position2, properties[0], "X");
		EditorGUIUtility.labelWidth = 35f;
		FloatProperty(new Rect(position2.x + 70f, position2.y, position2.width, position2.height), properties[1], "Y");
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
	}

	private void DrawSliderProperty(MaterialProperty property, string label)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = EditorGUIUtility.fieldWidth;
		Rect controlRect = EditorGUILayout.GetControlRect(false, 20f);
		Rect position = new Rect(controlRect.x, controlRect.y, controlRect.width - 55f, 20f);
		Rect source = new Rect(controlRect.width - 46f, controlRect.y, 60f, 18f);
		RangeProperty(position, property, label);
		EditorGUIUtility.labelWidth = 10f;
		FloatProperty(new Rect(source), property, string.Empty);
		if (!property.hasMixedValue)
		{
			property.floatValue = Mathf.Round(property.floatValue * 1000f) / 1000f;
		}
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
	}

	private void DrawTextureProperty(MaterialProperty property, string label)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = EditorGUIUtility.fieldWidth;
		EditorGUIUtility.fieldWidth = 70f;
		Rect controlRect = EditorGUILayout.GetControlRect(false, 75f);
		GUI.Label(new Rect(controlRect.x + 15f, controlRect.y + 5f, 100f, controlRect.height), label);
		TextureProperty(new Rect(controlRect.x, controlRect.y + 5f, 200f, controlRect.height), property, string.Empty, false);
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
	}

	private void DrawFloatProperty(MaterialProperty property, string label)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = EditorGUIUtility.fieldWidth;
		Rect controlRect = EditorGUILayout.GetControlRect(false, 20f);
		Rect position = new Rect(controlRect.x, controlRect.y, 225f, 18f);
		FloatProperty(position, property, label);
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
	}

	private void DrawRangeProperty(MaterialProperty property, string label)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = EditorGUIUtility.fieldWidth;
		Rect controlRect = EditorGUILayout.GetControlRect(false, 16f);
		Rect position = new Rect(controlRect.x + 15f, controlRect.y, controlRect.width, controlRect.height);
		GUI.Label(position, label);
		position.x += 100f;
		position.width -= 115f;
		RangeProperty(position, property, string.Empty);
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
	}

	private void DrawVectorProperty(MaterialProperty property, string label)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = EditorGUIUtility.fieldWidth;
		EditorGUIUtility.labelWidth = 160f;
		Rect controlRect = EditorGUILayout.GetControlRect(false, 20f);
		Rect position = new Rect(controlRect.x + 15f, controlRect.y + 2f, controlRect.width - 120f, 18f);
		Rect position2 = new Rect(175f, controlRect.y - 14f, controlRect.width - 160f, 18f);
		GUI.Label(position, label);
		VectorProperty(position2, property, "");
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
	}

	private void DrawVectorProperty(MaterialProperty property, string label, int floatCount)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = EditorGUIUtility.fieldWidth;
		EditorGUIUtility.labelWidth = 160f;
		Rect controlRect = EditorGUILayout.GetControlRect(false, 20f);
		Rect position = new Rect(controlRect.x + 15f, controlRect.y + 2f, controlRect.width - 120f, 18f);
		Rect position2 = new Rect(175f, controlRect.y - 14f, controlRect.width - 160f, 18f);
		GUI.Label(position, label);
		VectorProperty(position2, property, "");
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
	}

	private void Draw2DBoundsProperty(MaterialProperty property, string label)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = EditorGUIUtility.fieldWidth;
		Rect controlRect = EditorGUILayout.GetControlRect(false, 22f);
		Rect position = new Rect(controlRect.x + 15f, controlRect.y + 2f, controlRect.width - 15f, 18f);
		GUI.Label(position, label);
		EditorGUIUtility.labelWidth = 30f;
		float num = (position.width - 15f) / 5f;
		position.x += labelWidth - 30f;
		Vector4 vectorValue = property.vectorValue;
		position.width = num;
		vectorValue.x = EditorGUI.FloatField(position, "X", vectorValue.x);
		position.x += num - 14f;
		vectorValue.y = EditorGUI.FloatField(position, "Y", vectorValue.y);
		position.x += num - 14f;
		vectorValue.z = EditorGUI.FloatField(position, "W", vectorValue.z);
		position.x += num - 14f;
		vectorValue.w = EditorGUI.FloatField(position, "H", vectorValue.w);
		position.x = controlRect.width - 11f;
		position.width = 25f;
		property.vectorValue = vectorValue;
		if (GUI.Button(position, "X"))
		{
			Renderer component = Selection.activeGameObject.GetComponent<Renderer>();
			if (component != null)
			{
				property.vectorValue = new Vector4(0f, 0f, Mathf.Round(component.bounds.extents.x * 1000f) / 1000f, Mathf.Round(component.bounds.extents.y * 1000f) / 1000f);
			}
		}
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
	}

	private void Draw2DRectBoundsProperty(MaterialProperty property, string label)
	{
		float labelWidth = EditorGUIUtility.labelWidth;
		float fieldWidth = EditorGUIUtility.fieldWidth;
		Rect controlRect = EditorGUILayout.GetControlRect(false, 22f);
		Rect position = new Rect(controlRect.x + 15f, controlRect.y + 2f, controlRect.width - 15f, 18f);
		GUI.Label(position, label);
		float num = (position.width - position.x - 30f) / 4f;
		position.x += labelWidth - 30f;
		Vector4 vectorValue = property.vectorValue;
		EditorGUIUtility.labelWidth = 40f;
		position.width = num + 8f;
		vectorValue.x = EditorGUI.FloatField(position, "BL", vectorValue.x);
		position.x += num;
		position.width = num - 18f;
		vectorValue.y = EditorGUI.FloatField(position, "", vectorValue.y);
		position.x += num - 24f;
		position.width = num + 8f;
		vectorValue.z = EditorGUI.FloatField(position, "TR", vectorValue.z);
		position.x += num;
		position.width = num - 18f;
		vectorValue.w = EditorGUI.FloatField(position, "", vectorValue.w);
		property.vectorValue = vectorValue;
		EditorGUIUtility.labelWidth = labelWidth;
		EditorGUIUtility.fieldWidth = fieldWidth;
	}

	private void SetKeyword(bool state, string keyword)
	{
		Undo.RecordObjects(base.targets, "Keyword State Change");
		for (int i = 0; i < base.targets.Length; i++)
		{
			Material material = base.targets[i] as Material;
			if (state)
			{
				if (!(keyword == "UNDERLAY_ON"))
				{
					if (keyword == "UNDERLAY_INNER")
					{
						material.EnableKeyword("UNDERLAY_INNER");
						material.DisableKeyword("UNDERLAY_ON");
					}
					else
					{
						material.EnableKeyword(keyword);
					}
				}
				else
				{
					material.EnableKeyword("UNDERLAY_ON");
					material.DisableKeyword("UNDERLAY_INNER");
				}
			}
			else if (!(keyword == "UNDERLAY_ON"))
			{
				if (keyword == "UNDERLAY_INNER")
				{
					material.DisableKeyword("UNDERLAY_INNER");
					material.DisableKeyword("UNDERLAY_ON");
				}
				else
				{
					material.DisableKeyword(keyword);
				}
			}
			else
			{
				material.DisableKeyword("UNDERLAY_ON");
				material.DisableKeyword("UNDERLAY_INNER");
			}
		}
	}

	private void SetUnderlayKeywords()
	{
		for (int i = 0; i < base.targets.Length; i++)
		{
			Material material = base.targets[i] as Material;
			if (m_underlaySelection == Underlay_Types.Normal)
			{
				material.EnableKeyword("UNDERLAY_ON");
				material.DisableKeyword("UNDERLAY_INNER");
			}
			else if (m_underlaySelection == Underlay_Types.Inner)
			{
				material.EnableKeyword("UNDERLAY_INNER");
				material.DisableKeyword("UNDERLAY_ON");
			}
		}
	}

	private void SetMaskID(MaskingTypes id)
	{
		for (int i = 0; i < base.targets.Length; i++)
		{
			Material material = base.targets[i] as Material;
			switch (id)
			{
			case MaskingTypes.MaskHard:
				material.SetFloat("_MaskID", (float)id);
				break;
			case MaskingTypes.MaskSoft:
				material.SetFloat("_MaskID", (float)id);
				break;
			case MaskingTypes.MaskOff:
				material.SetFloat("_MaskID", (float)id);
				break;
			}
		}
	}

	private void SetMaskKeywords(MaskingTypes mask)
	{
		for (int i = 0; i < base.targets.Length; i++)
		{
			Material material = base.targets[i] as Material;
			switch (mask)
			{
			case MaskingTypes.MaskHard:
				material.EnableKeyword("MASK_HARD");
				material.DisableKeyword("MASK_SOFT");
				break;
			case MaskingTypes.MaskSoft:
				material.EnableKeyword("MASK_SOFT");
				material.DisableKeyword("MASK_HARD");
				break;
			case MaskingTypes.MaskOff:
				material.DisableKeyword("MASK_HARD");
				material.DisableKeyword("MASK_SOFT");
				break;
			}
		}
	}

	private void ReadMaterialProperties()
	{
		UnityEngine.Object[] targets = base.targets;
		m_faceColor = MaterialEditor.GetMaterialProperty(targets, "_FaceColor");
		m_faceTex = MaterialEditor.GetMaterialProperty(targets, "_FaceTex");
		m_faceUVSpeedX = MaterialEditor.GetMaterialProperty(targets, "_FaceUVSpeedX");
		m_faceUVSpeedY = MaterialEditor.GetMaterialProperty(targets, "_FaceUVSpeedY");
		m_faceDilate = MaterialEditor.GetMaterialProperty(targets, "_FaceDilate");
		m_faceShininess = MaterialEditor.GetMaterialProperty(targets, "_FaceShininess");
		m_outlineColor = MaterialEditor.GetMaterialProperty(targets, "_OutlineColor");
		m_outlineThickness = MaterialEditor.GetMaterialProperty(targets, "_OutlineWidth");
		m_outlineSoftness = MaterialEditor.GetMaterialProperty(targets, "_OutlineSoftness");
		m_outlineTex = MaterialEditor.GetMaterialProperty(targets, "_OutlineTex");
		m_outlineUVSpeedX = MaterialEditor.GetMaterialProperty(targets, "_OutlineUVSpeedX");
		m_outlineUVSpeedY = MaterialEditor.GetMaterialProperty(targets, "_OutlineUVSpeedY");
		m_outlineShininess = MaterialEditor.GetMaterialProperty(targets, "_OutlineShininess");
		m_underlayColor = MaterialEditor.GetMaterialProperty(targets, "_UnderlayColor");
		m_underlayOffsetX = MaterialEditor.GetMaterialProperty(targets, "_UnderlayOffsetX");
		m_underlayOffsetY = MaterialEditor.GetMaterialProperty(targets, "_UnderlayOffsetY");
		m_underlayDilate = MaterialEditor.GetMaterialProperty(targets, "_UnderlayDilate");
		m_underlaySoftness = MaterialEditor.GetMaterialProperty(targets, "_UnderlaySoftness");
		m_bumpMap = MaterialEditor.GetMaterialProperty(targets, "_BumpMap");
		m_bumpFace = MaterialEditor.GetMaterialProperty(targets, "_BumpFace");
		m_bumpOutline = MaterialEditor.GetMaterialProperty(targets, "_BumpOutline");
		m_bevel = MaterialEditor.GetMaterialProperty(targets, "_Bevel");
		m_bevelOffset = MaterialEditor.GetMaterialProperty(targets, "_BevelOffset");
		m_bevelWidth = MaterialEditor.GetMaterialProperty(targets, "_BevelWidth");
		m_bevelClamp = MaterialEditor.GetMaterialProperty(targets, "_BevelClamp");
		m_bevelRoundness = MaterialEditor.GetMaterialProperty(targets, "_BevelRoundness");
		m_specColor = MaterialEditor.GetMaterialProperty(targets, "_SpecColor");
		m_lightAngle = MaterialEditor.GetMaterialProperty(targets, "_LightAngle");
		m_specularColor = MaterialEditor.GetMaterialProperty(targets, "_SpecularColor");
		m_specularPower = MaterialEditor.GetMaterialProperty(targets, "_SpecularPower");
		m_reflectivity = MaterialEditor.GetMaterialProperty(targets, "_Reflectivity");
		m_diffuse = MaterialEditor.GetMaterialProperty(targets, "_Diffuse");
		m_ambientLight = MaterialEditor.GetMaterialProperty(targets, "_Ambient");
		m_glowColor = MaterialEditor.GetMaterialProperty(targets, "_GlowColor");
		m_glowOffset = MaterialEditor.GetMaterialProperty(targets, "_GlowOffset");
		m_glowInner = MaterialEditor.GetMaterialProperty(targets, "_GlowInner");
		m_glowOuter = MaterialEditor.GetMaterialProperty(targets, "_GlowOuter");
		m_glowPower = MaterialEditor.GetMaterialProperty(targets, "_GlowPower");
		m_reflectFaceColor = MaterialEditor.GetMaterialProperty(targets, "_ReflectFaceColor");
		m_reflectOutlineColor = MaterialEditor.GetMaterialProperty(targets, "_ReflectOutlineColor");
		m_reflectTex = MaterialEditor.GetMaterialProperty(targets, "_Cube");
		m_reflectRotation = MaterialEditor.GetMaterialProperty(targets, "_EnvMatrixRotation");
		m_mainTex = MaterialEditor.GetMaterialProperty(targets, "_MainTex");
		m_texSampleWidth = MaterialEditor.GetMaterialProperty(targets, "_TextureWidth");
		m_texSampleHeight = MaterialEditor.GetMaterialProperty(targets, "_TextureHeight");
		m_gradientScale = MaterialEditor.GetMaterialProperty(targets, "_GradientScale");
		m_PerspectiveFilter = MaterialEditor.GetMaterialProperty(targets, "_PerspectiveFilter");
		m_scaleX = MaterialEditor.GetMaterialProperty(targets, "_ScaleX");
		m_scaleY = MaterialEditor.GetMaterialProperty(targets, "_ScaleY");
		m_vertexOffsetX = MaterialEditor.GetMaterialProperty(targets, "_VertexOffsetX");
		m_vertexOffsetY = MaterialEditor.GetMaterialProperty(targets, "_VertexOffsetY");
		m_maskTex = MaterialEditor.GetMaterialProperty(targets, "_MaskTex");
		m_maskCoord = MaterialEditor.GetMaterialProperty(targets, "_MaskCoord");
		m_clipRect = MaterialEditor.GetMaterialProperty(targets, "_ClipRect");
		m_maskSoftnessX = MaterialEditor.GetMaterialProperty(targets, "_MaskSoftnessX");
		m_maskSoftnessY = MaterialEditor.GetMaterialProperty(targets, "_MaskSoftnessY");
		m_maskTexInverse = MaterialEditor.GetMaterialProperty(targets, "_MaskInverse");
		m_maskTexEdgeColor = MaterialEditor.GetMaterialProperty(targets, "_MaskEdgeColor");
		m_maskTexEdgeSoftness = MaterialEditor.GetMaterialProperty(targets, "_MaskEdgeSoftness");
		m_maskTexWipeControl = MaterialEditor.GetMaterialProperty(targets, "_MaskWipeControl");
		m_stencilID = MaterialEditor.GetMaterialProperty(targets, "_Stencil");
		m_stencilComp = MaterialEditor.GetMaterialProperty(targets, "_StencilComp");
		m_stencilOp = MaterialEditor.GetMaterialProperty(targets, "_StencilOp");
		m_stencilReadMask = MaterialEditor.GetMaterialProperty(targets, "_StencilReadMask");
		m_stencilWriteMask = MaterialEditor.GetMaterialProperty(targets, "_StencilWriteMask");
		m_shaderFlags = MaterialEditor.GetMaterialProperty(targets, "_ShaderFlags");
		m_scaleRatio_A = MaterialEditor.GetMaterialProperty(targets, "_ScaleRatioA");
		m_scaleRatio_B = MaterialEditor.GetMaterialProperty(targets, "_ScaleRatioB");
		m_scaleRatio_C = MaterialEditor.GetMaterialProperty(targets, "_ScaleRatioC");

	}
}
