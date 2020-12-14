using System;
using System.Collections.Generic;
#if UNITY_EDITOR
//using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TMPro
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(RectTransform))]
	[RequireComponent(typeof(CanvasRenderer))]
	//[AddComponentMenu("UI/TextMeshPro - Text (UI)", 11)]
	[SelectionBase]
	public class TextMeshProUGUI : TMP_Text, ILayoutElement
	{


		[SerializeField]
		private bool m_hasFontAssetChanged = false;

		[SerializeField]
		protected TMP_SubMeshUI[] m_subTextObjects = new TMP_SubMeshUI[8];

		private float m_previousLossyScaleY = -1f;

		private Vector3[] m_RectTransformCorners = new Vector3[4];

		private CanvasRenderer m_canvasRenderer;

		private Canvas m_canvas;

		private bool m_isFirstAllocation;

		private int m_max_characters = 8;

		private bool m_isMaskingEnabled;

		[SerializeField]
		private Material m_baseMaterial;

		private bool m_isScrollRegionSet;

		private int m_stencilID = 0;

		[SerializeField]
		private Vector4 m_maskOffset;

		private Matrix4x4 m_EnvMapMatrix = default(Matrix4x4);

		[NonSerialized]
		private bool m_isRegisteredForEvents;

		private int m_recursiveCountA = 0;

		private int loopCountA = 0;

		private bool m_isRebuildingLayout = false;

		public override Material materialForRendering
		{
			get
			{
				if (m_sharedMaterial == null)
				{
					return null;
				}
				//return ((MaskableGraphic)this).GetModifiedMaterial(m_sharedMaterial);
				return GetModifiedMaterial(m_sharedMaterial);
			}
		}

		public override bool autoSizeTextContainer
		{
			get
			{
				return m_autoSizeTextContainer;
			}
			set
			{
				if (m_autoSizeTextContainer != value)
				{
					m_autoSizeTextContainer = value;
					if (m_autoSizeTextContainer)
					{
						CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild((ICanvasElement)(object)this);
						//SetLayoutDirty();
						SetLayoutDirty();
					}
				}
			}
		}

		public override Mesh mesh
		{
			get
			{
				return m_mesh;
			}
		}

		public CanvasRenderer canvasRenderer
		{
			get
			{
				if (m_canvasRenderer == null)
				{
					m_canvasRenderer = base.GetComponent<CanvasRenderer>();
				}
				return m_canvasRenderer;
			}
		}

		public InlineGraphicManager inlineGraphicManager
		{
			get
			{
				return m_inlineGraphics;
			}
		}

		public Vector4 maskOffset
		{
			get
			{
				return m_maskOffset;
			}
			set
			{
				m_maskOffset = value;
				UpdateMask();
				m_havePropertiesChanged = true;
			}
		}

		protected override void Awake()
		{
			m_canvas = ((Graphic)this).canvas;
			m_isOrthographic = true;
			m_rectTransform = base.gameObject.GetComponent<RectTransform>();
			if (m_rectTransform == null)
			{
				m_rectTransform = base.gameObject.AddComponent<RectTransform>();
			}
			m_canvasRenderer = base.GetComponent<CanvasRenderer>();
			if (m_canvasRenderer == null)
			{
				m_canvasRenderer = base.gameObject.AddComponent<CanvasRenderer>();
			}
			if (m_mesh == null)
			{
				m_mesh = new Mesh();
				m_mesh.hideFlags = HideFlags.HideAndDontSave;
			}
			LoadDefaultSettings();
			LoadFontAsset();
			TMP_StyleSheet.LoadDefaultStyleSheet();
			if (m_char_buffer == null)
			{
				m_char_buffer = new int[m_max_characters];
			}
			m_cached_TextElement = new TMP_Glyph();
			m_isFirstAllocation = true;
			if (m_textInfo == null)
			{
				m_textInfo = new TMP_TextInfo(this);
			}
			if (m_fontAsset == null)
			{
				Debug.LogWarning("Please assign a Font Asset to this " + base.transform.name + " gameobject.", (UnityEngine.Object)(object)this);
				return;
			}
			TMP_SubMeshUI[] componentsInChildren = base.GetComponentsInChildren<TMP_SubMeshUI>();
			if (componentsInChildren.Length != 0)
			{
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					m_subTextObjects[i + 1] = componentsInChildren[i];
				}
			}
			m_isInputParsingRequired = true;
			m_havePropertiesChanged = true;
			m_isCalculateSizeRequired = true;
			m_isAwake = true;
		}

		protected override void OnEnable()
		{
			if (!m_isRegisteredForEvents)
			{
				TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Add(ON_MATERIAL_PROPERTY_CHANGED);
				TMPro_EventManager.FONT_PROPERTY_EVENT.Add(ON_FONT_PROPERTY_CHANGED);
				TMPro_EventManager.TEXTMESHPRO_UGUI_PROPERTY_EVENT.Add(ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED);
				TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Add(ON_DRAG_AND_DROP_MATERIAL);
				TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Add(ON_TEXT_STYLE_CHANGED);
				TMPro_EventManager.COLOR_GRADIENT_PROPERTY_EVENT.Add(ON_COLOR_GRADIENT_CHANGED);
				TMPro_EventManager.TMP_SETTINGS_PROPERTY_EVENT.Add(ON_TMP_SETTINGS_CHANGED);
				m_isRegisteredForEvents = true;
			}
			m_canvas = GetCanvas();
			SetActiveSubMeshes(true);
			GraphicRegistry.RegisterGraphicForCanvas(m_canvas, (Graphic)(object)this);
			ComputeMarginSize();
			m_verticesAlreadyDirty = false;
			m_layoutAlreadyDirty = false;
			base.m_ShouldRecalculateStencil = true;
			m_isInputParsingRequired = true;
			SetAllDirty();
			//((MaskableGraphic)this).RecalculateClipping();
			RecalculateClipping();
		}

		protected override void OnDisable()
		{
			if (base.m_MaskMaterial != null)
			{
				TMP_MaterialManager.ReleaseStencilMaterial(base.m_MaskMaterial);
				base.m_MaskMaterial = null;
			}
			GraphicRegistry.UnregisterGraphicForCanvas(m_canvas, (Graphic)(object)this);
			CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild((ICanvasElement)(object)this);
			if (m_canvasRenderer != null)
			{
				m_canvasRenderer.Clear();
			}
			SetActiveSubMeshes(false);
			LayoutRebuilder.MarkLayoutForRebuild(m_rectTransform);
			//((MaskableGraphic)this).RecalculateClipping();
			RecalculateClipping();
		}

		protected override void OnDestroy()
		{
			GraphicRegistry.UnregisterGraphicForCanvas(m_canvas, (Graphic)(object)this);
			if (m_mesh != null)
			{
				UnityEngine.Object.DestroyImmediate(m_mesh);
			}
			if (base.m_MaskMaterial != null)
			{
				TMP_MaterialManager.ReleaseStencilMaterial(base.m_MaskMaterial);
				base.m_MaskMaterial = null;
			}
			TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Remove(ON_MATERIAL_PROPERTY_CHANGED);
			TMPro_EventManager.FONT_PROPERTY_EVENT.Remove(ON_FONT_PROPERTY_CHANGED);
			TMPro_EventManager.TEXTMESHPRO_UGUI_PROPERTY_EVENT.Remove(ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED);
			TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Remove(ON_DRAG_AND_DROP_MATERIAL);
			TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Remove(ON_TEXT_STYLE_CHANGED);
			TMPro_EventManager.COLOR_GRADIENT_PROPERTY_EVENT.Remove(ON_COLOR_GRADIENT_CHANGED);
			TMPro_EventManager.TMP_SETTINGS_PROPERTY_EVENT.Remove(ON_TMP_SETTINGS_CHANGED);
			m_isRegisteredForEvents = false;
		}

#if UNITY_EDITOR
		protected override void Reset()
		{
			if (GetType() == typeof(TextMeshProUGUI))
			{
				//DestroyImmediate(this);
				Debug.LogError("Error. Wrong TextMeshProUGUI type added");
				Destroy(this);
			}
			else
			{
				LoadDefaultSettings();
				LoadFontAsset();
				m_isInputParsingRequired = true;
				m_havePropertiesChanged = true;
			}
		}
#endif
#if UNITY_EDITOR
		protected override void OnValidate()
		{
			if (m_fontAsset == null || m_hasFontAssetChanged)
			{
				LoadFontAsset();
				m_isCalculateSizeRequired = true;
				m_hasFontAssetChanged = false;
			}
			if (m_canvasRenderer == null || m_canvasRenderer.GetMaterial() == null || m_canvasRenderer.GetMaterial().GetTexture(ShaderUtilities.ID_MainTex) == null || m_fontAsset == null || m_fontAsset.atlas.GetInstanceID() != m_canvasRenderer.GetMaterial().GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
			{
				LoadFontAsset();
				m_isCalculateSizeRequired = true;
				m_hasFontAssetChanged = false;
			}
			m_padding = GetPaddingForMaterial();
			m_isInputParsingRequired = true;
			m_inputSource = TextInputSources.Text;
			m_havePropertiesChanged = true;
			m_isCalculateSizeRequired = true;
			m_isPreferredWidthDirty = true;
			m_isPreferredHeightDirty = true;
			SetAllDirty();
		}
#endif

		private void ON_MATERIAL_PROPERTY_CHANGED(bool isChanged, Material mat)
		{
			ShaderUtilities.GetShaderPropertyIDs();
			int instanceID = mat.GetInstanceID();
			int instanceID2 = m_sharedMaterial.GetInstanceID();
			int num = (!(base.m_MaskMaterial == null)) ? base.m_MaskMaterial.GetInstanceID() : 0;
			if (m_canvasRenderer == null || m_canvasRenderer.GetMaterial() == null)
			{
				if (m_canvasRenderer == null)
				{
					return;
				}
				if (m_fontAsset != null)
				{
					m_canvasRenderer.SetMaterial(m_fontAsset.material, m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex));
				}
				else
				{
					Debug.LogWarning("No Font Asset assigned to " + base.name + ". Please assign a Font Asset.", (UnityEngine.Object)(object)this);
				}
			}
			if (m_canvasRenderer.GetMaterial() != m_sharedMaterial && m_fontAsset == null)
			{
				m_sharedMaterial = m_canvasRenderer.GetMaterial();
			}
			if (base.m_MaskMaterial != null)
			{
#if UNITY_EDITOR
				//Undo.RecordObject(base.m_MaskMaterial, "Material Property Changes");
				//Undo.RecordObject(m_sharedMaterial, "Material Property Changes");
#endif
				if (instanceID == instanceID2)
				{
					float @float = base.m_MaskMaterial.GetFloat(ShaderUtilities.ID_StencilID);
					float float2 = base.m_MaskMaterial.GetFloat(ShaderUtilities.ID_StencilComp);
					base.m_MaskMaterial.CopyPropertiesFromMaterial(mat);
					base.m_MaskMaterial.shaderKeywords = mat.shaderKeywords;
					base.m_MaskMaterial.SetFloat(ShaderUtilities.ID_StencilID, @float);
					base.m_MaskMaterial.SetFloat(ShaderUtilities.ID_StencilComp, float2);
				}
				else if (instanceID == num)
				{
					GetPaddingForMaterial(mat);
					m_sharedMaterial.CopyPropertiesFromMaterial(mat);
					m_sharedMaterial.shaderKeywords = mat.shaderKeywords;
					m_sharedMaterial.SetFloat(ShaderUtilities.ID_StencilID, 0f);
					m_sharedMaterial.SetFloat(ShaderUtilities.ID_StencilComp, 8f);
				}
			}
			m_padding = GetPaddingForMaterial();
			m_havePropertiesChanged = true;
			SetVerticesDirty();
		}

		private void ON_FONT_PROPERTY_CHANGED(bool isChanged, TMP_FontAsset font)
		{
			if (MaterialReference.Contains(m_materialReferences, font))
			{
				m_isInputParsingRequired = true;
				m_havePropertiesChanged = true;
				SetLayoutDirty();
				SetVerticesDirty();
			}
		}

		private void ON_TEXTMESHPRO_UGUI_PROPERTY_CHANGED(bool isChanged, TextMeshProUGUI obj)
		{
			Debug.Log("Event Received by " + obj);
			if ((UnityEngine.Object)(object)obj == (UnityEngine.Object)(object)this)
			{
				m_havePropertiesChanged = true;
				m_isInputParsingRequired = true;
				SetVerticesDirty();
			}
		}

		private void ON_DRAG_AND_DROP_MATERIAL(GameObject obj, Material currentMaterial, Material newMaterial)
		{
#if UNITY_EDITOR
			if (obj == base.gameObject/* || PrefabUtility.GetPrefabParent(base.gameObject) == obj*/)
			{
				//Undo.RecordObject((UnityEngine.Object)(object)this, "Material Assignment");
				//Undo.RecordObject(m_canvasRenderer, "Material Assignment");
				m_sharedMaterial = newMaterial;
				m_padding = GetPaddingForMaterial();
				m_havePropertiesChanged = true;
				SetVerticesDirty();
				SetMaterialDirty();
			}
#endif
		}

		private void ON_TEXT_STYLE_CHANGED(bool isChanged)
		{
			m_havePropertiesChanged = true;
			m_isInputParsingRequired = true;
			SetVerticesDirty();
		}

		private void ON_COLOR_GRADIENT_CHANGED(TMP_ColorGradient gradient)
		{
			if (m_fontColorGradientPreset != null && gradient.GetInstanceID() == m_fontColorGradientPreset.GetInstanceID())
			{
				m_havePropertiesChanged = true;
				SetVerticesDirty();
			}
		}

		private void ON_TMP_SETTINGS_CHANGED()
		{
			m_defaultSpriteAsset = null;
			m_havePropertiesChanged = true;
			m_isInputParsingRequired = true;
			SetAllDirty();
		}

		protected override void LoadFontAsset()
		{
			ShaderUtilities.GetShaderPropertyIDs();
			if (m_fontAsset == null)
			{
				if (TMP_Settings.defaultFontAsset != null)
				{
					m_fontAsset = TMP_Settings.defaultFontAsset;
				}
				else
				{
					m_fontAsset = (Resources.Load("Fonts & Materials/LiberationSans SDF", typeof(TMP_FontAsset)) as TMP_FontAsset);
				}
				if (m_fontAsset == null)
				{
					Debug.LogWarning("The LiberationSans SDF Font Asset was not found. There is no Font Asset assigned to " + base.gameObject.name + ".", (UnityEngine.Object)(object)this);
					return;
				}
				if (m_fontAsset.characterDictionary == null)
				{
					Debug.Log("Dictionary is Null!");
				}
				m_sharedMaterial = m_fontAsset.material;
			}
			else
			{
				if (m_fontAsset.characterDictionary == null)
				{
					m_fontAsset.ReadFontDefinition();
				}
				if (m_sharedMaterial == null && m_baseMaterial != null)
				{
					m_sharedMaterial = m_baseMaterial;
					m_baseMaterial = null;
				}
				if (m_sharedMaterial == null || m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex) == null || m_fontAsset.atlas.GetInstanceID() != m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
				{
					if (m_fontAsset.material == null)
					{
						Debug.LogWarning("The Font Atlas Texture of the Font Asset " + m_fontAsset.name + " assigned to " + base.gameObject.name + " is missing.", (UnityEngine.Object)(object)this);
					}
					else
					{
						m_sharedMaterial = m_fontAsset.material;
					}
				}
			}
			GetSpecialCharacters(m_fontAsset);
			m_padding = GetPaddingForMaterial();
			SetMaterialDirty();
		}

		private Canvas GetCanvas()
		{
			Canvas result = null;
			List<Canvas> list = TMP_ListPool<Canvas>.Get();
			base.gameObject.GetComponentsInParent(false, list);
			if (list.Count > 0)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].isActiveAndEnabled)
					{
						result = list[i];
						break;
					}
				}
			}
			TMP_ListPool<Canvas>.Release(list);
			return result;
		}

		private void UpdateEnvMapMatrix()
		{
			if (m_sharedMaterial.HasProperty(ShaderUtilities.ID_EnvMap) && !(m_sharedMaterial.GetTexture(ShaderUtilities.ID_EnvMap) == null))
			{
				Vector3 euler = m_sharedMaterial.GetVector(ShaderUtilities.ID_EnvMatrixRotation);
				m_EnvMapMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(euler), Vector3.one);
				m_sharedMaterial.SetMatrix(ShaderUtilities.ID_EnvMatrix, m_EnvMapMatrix);
			}
		}

		private void EnableMasking()
		{
			if (m_fontMaterial == null)
			{
				m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
				m_canvasRenderer.SetMaterial(m_fontMaterial, m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex));
			}
			m_sharedMaterial = m_fontMaterial;
			if (m_sharedMaterial.HasProperty(ShaderUtilities.ID_ClipRect))
			{
				m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);
				UpdateMask();
			}
			m_isMaskingEnabled = true;
		}

		private void DisableMasking()
		{
			if (m_fontMaterial != null)
			{
				if (m_stencilID > 0)
				{
					m_sharedMaterial = base.m_MaskMaterial;
				}
				m_canvasRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex));
				UnityEngine.Object.DestroyImmediate(m_fontMaterial);
			}
			m_isMaskingEnabled = false;
		}

		private void UpdateMask()
		{
			if (m_rectTransform != null)
			{
				if (!ShaderUtilities.isInitialized)
				{
					ShaderUtilities.GetShaderPropertyIDs();
				}
				m_isScrollRegionSet = true;
				float num = Mathf.Min(Mathf.Min(m_margin.x, m_margin.z), m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessX));
				float num2 = Mathf.Min(Mathf.Min(m_margin.y, m_margin.w), m_sharedMaterial.GetFloat(ShaderUtilities.ID_MaskSoftnessY));
				num = ((num > 0f) ? num : 0f);
				num2 = ((num2 > 0f) ? num2 : 0f);
				float z = (m_rectTransform.rect.width - Mathf.Max(m_margin.x, 0f) - Mathf.Max(m_margin.z, 0f)) / 2f + num;
				float w = (m_rectTransform.rect.height - Mathf.Max(m_margin.y, 0f) - Mathf.Max(m_margin.w, 0f)) / 2f + num2;
				Vector2 vector = m_rectTransform.localPosition + new Vector3((0.5f - m_rectTransform.pivot.x) * m_rectTransform.rect.width + (Mathf.Max(m_margin.x, 0f) - Mathf.Max(m_margin.z, 0f)) / 2f, (0.5f - m_rectTransform.pivot.y) * m_rectTransform.rect.height + (0f - Mathf.Max(m_margin.y, 0f) + Mathf.Max(m_margin.w, 0f)) / 2f);
				Vector4 value = new Vector4(vector.x, vector.y, z, w);
				m_sharedMaterial.SetVector(ShaderUtilities.ID_ClipRect, value);
			}
		}

		protected override Material GetMaterial(Material mat)
		{
			ShaderUtilities.GetShaderPropertyIDs();
			if (m_fontMaterial == null || m_fontMaterial.GetInstanceID() != mat.GetInstanceID())
			{
				m_fontMaterial = CreateMaterialInstance(mat);
			}
			m_sharedMaterial = m_fontMaterial;
			m_padding = GetPaddingForMaterial();
			base.m_ShouldRecalculateStencil = true;
			SetVerticesDirty();
			SetMaterialDirty();
			return m_sharedMaterial;
		}

		protected override Material[] GetMaterials(Material[] mats)
		{
			int materialCount = m_textInfo.materialCount;
			if (m_fontMaterials == null)
			{
				m_fontMaterials = new Material[materialCount];
			}
			else if (m_fontMaterials.Length != materialCount)
			{
				TMP_TextInfo.Resize(ref m_fontMaterials, materialCount, false);
			}
			for (int i = 0; i < materialCount; i++)
			{
				if (i == 0)
				{
					m_fontMaterials[i] = base.fontMaterial;
				}
				else
				{
					m_fontMaterials[i] = ((Graphic)m_subTextObjects[i]).material;
				}
			}
			m_fontSharedMaterials = m_fontMaterials;
			return m_fontMaterials;
		}

		protected override void SetSharedMaterial(Material mat)
		{
			m_sharedMaterial = mat;
			m_padding = GetPaddingForMaterial();
			SetMaterialDirty();
		}

		protected override Material[] GetSharedMaterials()
		{
			int materialCount = m_textInfo.materialCount;
			if (m_fontSharedMaterials == null)
			{
				m_fontSharedMaterials = new Material[materialCount];
			}
			else if (m_fontSharedMaterials.Length != materialCount)
			{
				TMP_TextInfo.Resize(ref m_fontSharedMaterials, materialCount, false);
			}
			for (int i = 0; i < materialCount; i++)
			{
				if (i == 0)
				{
					m_fontSharedMaterials[i] = m_sharedMaterial;
				}
				else
				{
					m_fontSharedMaterials[i] = m_subTextObjects[i].sharedMaterial;
				}
			}
			return m_fontSharedMaterials;
		}

		protected override void SetSharedMaterials(Material[] materials)
		{
			int materialCount = m_textInfo.materialCount;
			if (m_fontSharedMaterials == null)
			{
				m_fontSharedMaterials = new Material[materialCount];
			}
			else if (m_fontSharedMaterials.Length != materialCount)
			{
				TMP_TextInfo.Resize(ref m_fontSharedMaterials, materialCount, false);
			}
			for (int i = 0; i < materialCount; i++)
			{
				if (i == 0)
				{
					if (!(materials[i].GetTexture(ShaderUtilities.ID_MainTex) == null) && materials[i].GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() == m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
					{
						m_sharedMaterial = (m_fontSharedMaterials[i] = materials[i]);
						m_padding = GetPaddingForMaterial(m_sharedMaterial);
					}
				}
				else if (!(materials[i].GetTexture(ShaderUtilities.ID_MainTex) == null) && materials[i].GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() == m_subTextObjects[i].sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() && m_subTextObjects[i].isDefaultMaterial)
				{
					m_subTextObjects[i].sharedMaterial = (m_fontSharedMaterials[i] = materials[i]);
				}
			}
		}

		protected override void SetOutlineThickness(float thickness)
		{
			if (m_fontMaterial != null && m_sharedMaterial.GetInstanceID() != m_fontMaterial.GetInstanceID())
			{
				m_sharedMaterial = m_fontMaterial;
				m_canvasRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex));
			}
			else if (m_fontMaterial == null)
			{
				m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
				m_sharedMaterial = m_fontMaterial;
				m_canvasRenderer.SetMaterial(m_sharedMaterial, m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex));
			}
			thickness = Mathf.Clamp01(thickness);
			m_sharedMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, thickness);
			m_padding = GetPaddingForMaterial();
		}

		protected override void SetFaceColor(Color32 color)
		{
			if (m_fontMaterial == null)
			{
				m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
			}
			m_sharedMaterial = m_fontMaterial;
			m_padding = GetPaddingForMaterial();
			m_sharedMaterial.SetColor(ShaderUtilities.ID_FaceColor, color);
		}

		protected override void SetOutlineColor(Color32 color)
		{
			if (m_fontMaterial == null)
			{
				m_fontMaterial = CreateMaterialInstance(m_sharedMaterial);
			}
			m_sharedMaterial = m_fontMaterial;
			m_padding = GetPaddingForMaterial();
			m_sharedMaterial.SetColor(ShaderUtilities.ID_OutlineColor, color);
		}

		protected override void SetShaderDepth()
		{
			if (!(m_canvas == null) && !(m_sharedMaterial == null))
			{
				if (m_canvas.renderMode == RenderMode.ScreenSpaceOverlay || m_isOverlay)
				{
					m_sharedMaterial.SetFloat(ShaderUtilities.ShaderTag_ZTestMode, 0f);
				}
				else
				{
					m_sharedMaterial.SetFloat(ShaderUtilities.ShaderTag_ZTestMode, 4f);
				}
			}
		}

		protected override void SetCulling()
		{
			if (m_isCullingEnabled)
			{
				m_canvasRenderer.GetMaterial().SetFloat("_CullMode", 2f);
			}
			else
			{
				m_canvasRenderer.GetMaterial().SetFloat("_CullMode", 0f);
			}
		}

		private void SetPerspectiveCorrection()
		{
			if (m_isOrthographic)
			{
				m_sharedMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0f);
			}
			else
			{
				m_sharedMaterial.SetFloat(ShaderUtilities.ID_PerspectiveFilter, 0.875f);
			}
		}

		protected override float GetPaddingForMaterial(Material mat)
		{
			m_padding = ShaderUtilities.GetPadding(mat, m_enableExtraPadding, m_isUsingBold);
			m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
			m_isSDFShader = mat.HasProperty(ShaderUtilities.ID_WeightNormal);
			return m_padding;
		}

		protected override float GetPaddingForMaterial()
		{
			ShaderUtilities.GetShaderPropertyIDs();
			m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
			m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
			m_isSDFShader = m_sharedMaterial.HasProperty(ShaderUtilities.ID_WeightNormal);
			return m_padding;
		}

		private void SetMeshArrays(int size)
		{
			m_textInfo.meshInfo[0].ResizeMeshInfo(size);
			m_canvasRenderer.SetMesh(m_textInfo.meshInfo[0].mesh);
		}

		protected override int SetArraySizes(int[] chars)
		{
			int endIndex = 0;
			int num = 0;
			m_totalCharacterCount = 0;
			m_isUsingBold = false;
			m_isParsingText = false;
			tag_NoParsing = false;
			m_style = m_fontStyle;
			m_fontWeightInternal = (((m_style & FontStyles.Bold) == FontStyles.Bold) ? 700 : m_fontWeight);
			m_fontWeightStack.SetDefault(m_fontWeightInternal);
			m_currentFontAsset = m_fontAsset;
			m_currentMaterial = m_sharedMaterial;
			m_currentMaterialIndex = 0;
			m_materialReferenceStack.SetDefault(new MaterialReference(m_currentMaterialIndex, m_currentFontAsset, null, m_currentMaterial, m_padding));
			m_materialReferenceIndexLookup.Clear();
			MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
			if (m_textInfo == null)
			{
				m_textInfo = new TMP_TextInfo();
			}
			m_textElementType = TMP_TextElementType.Character;
			if ((UnityEngine.Object)(object)m_linkedTextComponent != null)
			{
				m_linkedTextComponent.text = string.Empty;
				m_linkedTextComponent.ForceMeshUpdate();
			}
			for (int i = 0; i < chars.Length && chars[i] != 0; i++)
			{
				if (m_textInfo.characterInfo == null || m_totalCharacterCount >= m_textInfo.characterInfo.Length)
				{
					TMP_TextInfo.Resize(ref m_textInfo.characterInfo, m_totalCharacterCount + 1, true);
				}
				int num2 = chars[i];
				if (m_isRichText && num2 == 60)
				{
					int currentMaterialIndex = m_currentMaterialIndex;
					if (ValidateHtmlTag(chars, i + 1, out endIndex))
					{
						i = endIndex;
						if ((m_style & FontStyles.Bold) == FontStyles.Bold)
						{
							m_isUsingBold = true;
						}
						if (m_textElementType == TMP_TextElementType.Sprite)
						{
							m_materialReferences[m_currentMaterialIndex].referenceCount++;
							m_textInfo.characterInfo[m_totalCharacterCount].character = (char)(57344 + m_spriteIndex);
							m_textInfo.characterInfo[m_totalCharacterCount].spriteIndex = m_spriteIndex;
							m_textInfo.characterInfo[m_totalCharacterCount].fontAsset = m_currentFontAsset;
							m_textInfo.characterInfo[m_totalCharacterCount].spriteAsset = m_currentSpriteAsset;
							m_textInfo.characterInfo[m_totalCharacterCount].materialReferenceIndex = m_currentMaterialIndex;
							m_textInfo.characterInfo[m_totalCharacterCount].elementType = m_textElementType;
							m_textElementType = TMP_TextElementType.Character;
							m_currentMaterialIndex = currentMaterialIndex;
							num++;
							m_totalCharacterCount++;
						}
						continue;
					}
				}
				bool flag = false;
				bool isUsingAlternateTypeface = false;
				TMP_FontAsset currentFontAsset = m_currentFontAsset;
				Material currentMaterial = m_currentMaterial;
				int currentMaterialIndex2 = m_currentMaterialIndex;
				if (m_textElementType == TMP_TextElementType.Character)
				{
					if ((m_style & FontStyles.UpperCase) == FontStyles.UpperCase)
					{
						if (char.IsLower((char)num2))
						{
							num2 = char.ToUpper((char)num2);
						}
					}
					else if ((m_style & FontStyles.LowerCase) == FontStyles.LowerCase)
					{
						if (char.IsUpper((char)num2))
						{
							num2 = char.ToLower((char)num2);
						}
					}
					else if (((m_fontStyle & FontStyles.SmallCaps) == FontStyles.SmallCaps || (m_style & FontStyles.SmallCaps) == FontStyles.SmallCaps) && char.IsLower((char)num2))
					{
						num2 = char.ToUpper((char)num2);
					}
				}
				TMP_FontAsset fontAssetForWeight = GetFontAssetForWeight(m_fontWeightInternal);
				if (fontAssetForWeight != null)
				{
					flag = true;
					isUsingAlternateTypeface = true;
					m_currentFontAsset = fontAssetForWeight;
				}
				TMP_Glyph glyph;
				fontAssetForWeight = TMP_FontUtilities.SearchForGlyph(m_currentFontAsset, num2, out glyph);
				if (glyph == null)
				{
					TMP_SpriteAsset spriteAsset = base.spriteAsset;
					if (spriteAsset != null)
					{
						int spriteIndex = -1;
						spriteAsset = TMP_SpriteAsset.SearchFallbackForSprite(spriteAsset, num2, out spriteIndex);
						if (spriteIndex != -1)
						{
							m_textElementType = TMP_TextElementType.Sprite;
							m_textInfo.characterInfo[m_totalCharacterCount].elementType = m_textElementType;
							m_currentMaterialIndex = MaterialReference.AddMaterialReference(spriteAsset.material, spriteAsset, m_materialReferences, m_materialReferenceIndexLookup);
							m_materialReferences[m_currentMaterialIndex].referenceCount++;
							m_textInfo.characterInfo[m_totalCharacterCount].character = (char)num2;
							m_textInfo.characterInfo[m_totalCharacterCount].spriteIndex = spriteIndex;
							m_textInfo.characterInfo[m_totalCharacterCount].fontAsset = m_currentFontAsset;
							m_textInfo.characterInfo[m_totalCharacterCount].spriteAsset = spriteAsset;
							m_textInfo.characterInfo[m_totalCharacterCount].materialReferenceIndex = m_currentMaterialIndex;
							m_textElementType = TMP_TextElementType.Character;
							m_currentMaterialIndex = currentMaterialIndex2;
							num++;
							m_totalCharacterCount++;
							continue;
						}
					}
				}
				if (glyph == null && TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
				{
					fontAssetForWeight = TMP_FontUtilities.SearchForGlyph(TMP_Settings.fallbackFontAssets, num2, out glyph);
				}
				if (glyph == null && TMP_Settings.defaultFontAsset != null)
				{
					fontAssetForWeight = TMP_FontUtilities.SearchForGlyph(TMP_Settings.defaultFontAsset, num2, out glyph);
				}
				if (glyph == null)
				{
					TMP_SpriteAsset defaultSpriteAsset = TMP_Settings.defaultSpriteAsset;
					if (defaultSpriteAsset != null)
					{
						int spriteIndex2 = -1;
						defaultSpriteAsset = TMP_SpriteAsset.SearchFallbackForSprite(defaultSpriteAsset, num2, out spriteIndex2);
						if (spriteIndex2 != -1)
						{
							m_textElementType = TMP_TextElementType.Sprite;
							m_textInfo.characterInfo[m_totalCharacterCount].elementType = m_textElementType;
							m_currentMaterialIndex = MaterialReference.AddMaterialReference(defaultSpriteAsset.material, defaultSpriteAsset, m_materialReferences, m_materialReferenceIndexLookup);
							m_materialReferences[m_currentMaterialIndex].referenceCount++;
							m_textInfo.characterInfo[m_totalCharacterCount].character = (char)num2;
							m_textInfo.characterInfo[m_totalCharacterCount].spriteIndex = spriteIndex2;
							m_textInfo.characterInfo[m_totalCharacterCount].fontAsset = m_currentFontAsset;
							m_textInfo.characterInfo[m_totalCharacterCount].spriteAsset = defaultSpriteAsset;
							m_textInfo.characterInfo[m_totalCharacterCount].materialReferenceIndex = m_currentMaterialIndex;
							m_textElementType = TMP_TextElementType.Character;
							m_currentMaterialIndex = currentMaterialIndex2;
							num++;
							m_totalCharacterCount++;
							continue;
						}
					}
				}
				if (glyph == null)
				{
					num2 = (chars[i] = ((TMP_Settings.missingGlyphCharacter == 0) ? 9633 : TMP_Settings.missingGlyphCharacter));
					fontAssetForWeight = TMP_FontUtilities.SearchForGlyph(m_currentFontAsset, num2, out glyph);
					if (glyph == null && TMP_Settings.fallbackFontAssets != null && TMP_Settings.fallbackFontAssets.Count > 0)
					{
						fontAssetForWeight = TMP_FontUtilities.SearchForGlyph(TMP_Settings.fallbackFontAssets, num2, out glyph);
					}
					if (glyph == null && TMP_Settings.defaultFontAsset != null)
					{
						fontAssetForWeight = TMP_FontUtilities.SearchForGlyph(TMP_Settings.defaultFontAsset, num2, out glyph);
					}
					if (glyph == null)
					{
						num2 = (chars[i] = 32);
						fontAssetForWeight = TMP_FontUtilities.SearchForGlyph(m_currentFontAsset, num2, out glyph);
						if (!TMP_Settings.warningsDisabled)
						{
							Debug.LogWarning("Character with ASCII value of " + num2 + " was not found in the Font Asset Glyph Table. It was replaced by a space.", (UnityEngine.Object)(object)this);
						}
					}
				}
				if (fontAssetForWeight != null && fontAssetForWeight.GetInstanceID() != m_currentFontAsset.GetInstanceID())
				{
					flag = true;
					m_currentFontAsset = fontAssetForWeight;
				}
				m_textInfo.characterInfo[m_totalCharacterCount].elementType = TMP_TextElementType.Character;
				m_textInfo.characterInfo[m_totalCharacterCount].textElement = glyph;
				m_textInfo.characterInfo[m_totalCharacterCount].isUsingAlternateTypeface = isUsingAlternateTypeface;
				m_textInfo.characterInfo[m_totalCharacterCount].character = (char)num2;
				m_textInfo.characterInfo[m_totalCharacterCount].fontAsset = m_currentFontAsset;
				if (flag)
				{
					if (TMP_Settings.matchMaterialPreset)
					{
						m_currentMaterial = TMP_MaterialManager.GetFallbackMaterial(m_currentMaterial, m_currentFontAsset.material);
					}
					else
					{
						m_currentMaterial = m_currentFontAsset.material;
					}
					m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
				}
				if (!char.IsWhiteSpace((char)num2) && num2 != 8203)
				{
					if (m_materialReferences[m_currentMaterialIndex].referenceCount < 16383)
					{
						m_materialReferences[m_currentMaterialIndex].referenceCount++;
					}
					else
					{
						m_currentMaterialIndex = MaterialReference.AddMaterialReference(new Material(m_currentMaterial), m_currentFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
						m_materialReferences[m_currentMaterialIndex].referenceCount++;
					}
				}
				m_textInfo.characterInfo[m_totalCharacterCount].material = m_currentMaterial;
				m_textInfo.characterInfo[m_totalCharacterCount].materialReferenceIndex = m_currentMaterialIndex;
				m_materialReferences[m_currentMaterialIndex].isFallbackMaterial = flag;
				if (flag)
				{
					m_materialReferences[m_currentMaterialIndex].fallbackMaterial = currentMaterial;
					m_currentFontAsset = currentFontAsset;
					m_currentMaterial = currentMaterial;
					m_currentMaterialIndex = currentMaterialIndex2;
				}
				m_totalCharacterCount++;
			}
			if (m_isCalculatingPreferredValues)
			{
				m_isCalculatingPreferredValues = false;
				m_isInputParsingRequired = true;
				return m_totalCharacterCount;
			}
			m_textInfo.spriteCount = num;
			int num3 = m_textInfo.materialCount = m_materialReferenceIndexLookup.Count;
			if (num3 > m_textInfo.meshInfo.Length)
			{
				TMP_TextInfo.Resize(ref m_textInfo.meshInfo, num3, false);
			}
			if (num3 > m_subTextObjects.Length)
			{
				TMP_TextInfo.Resize(ref m_subTextObjects, Mathf.NextPowerOfTwo(num3 + 1));
			}
			if (m_textInfo.characterInfo.Length - m_totalCharacterCount > 256)
			{
				TMP_TextInfo.Resize(ref m_textInfo.characterInfo, Mathf.Max(m_totalCharacterCount + 1, 256), true);
			}
			for (int j = 0; j < num3; j++)
			{
				if (j > 0)
				{
					if ((UnityEngine.Object)(object)m_subTextObjects[j] == null)
					{
						m_subTextObjects[j] = TMP_SubMeshUI.AddSubTextObject(this, m_materialReferences[j]);
						m_textInfo.meshInfo[j].vertices = null;
					}
					if (m_rectTransform.pivot != ((Graphic)m_subTextObjects[j]).rectTransform.pivot)
					{
						((Graphic)m_subTextObjects[j]).rectTransform.pivot = m_rectTransform.pivot;
					}
					if (m_subTextObjects[j].sharedMaterial == null || m_subTextObjects[j].sharedMaterial.GetInstanceID() != m_materialReferences[j].material.GetInstanceID())
					{
						bool isDefaultMaterial = m_materialReferences[j].isDefaultMaterial;
						m_subTextObjects[j].isDefaultMaterial = isDefaultMaterial;
						if (!isDefaultMaterial || m_subTextObjects[j].sharedMaterial == null || m_subTextObjects[j].sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() != m_materialReferences[j].material.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
						{
							m_subTextObjects[j].sharedMaterial = m_materialReferences[j].material;
							m_subTextObjects[j].fontAsset = m_materialReferences[j].fontAsset;
							m_subTextObjects[j].spriteAsset = m_materialReferences[j].spriteAsset;
						}
					}
					if (m_materialReferences[j].isFallbackMaterial)
					{
						m_subTextObjects[j].fallbackMaterial = m_materialReferences[j].material;
						m_subTextObjects[j].fallbackSourceMaterial = m_materialReferences[j].fallbackMaterial;
					}
				}
				int referenceCount = m_materialReferences[j].referenceCount;
				if (m_textInfo.meshInfo[j].vertices == null || m_textInfo.meshInfo[j].vertices.Length < referenceCount * 4)
				{
					if (m_textInfo.meshInfo[j].vertices == null)
					{
						if (j == 0)
						{
							m_textInfo.meshInfo[j] = new TMP_MeshInfo(m_mesh, referenceCount + 1);
						}
						else
						{
							m_textInfo.meshInfo[j] = new TMP_MeshInfo(m_subTextObjects[j].mesh, referenceCount + 1);
						}
					}
					else
					{
						m_textInfo.meshInfo[j].ResizeMeshInfo((referenceCount > 1024) ? (referenceCount + 256) : Mathf.NextPowerOfTwo(referenceCount));
					}
				}
				else if (m_textInfo.meshInfo[j].vertices.Length - referenceCount * 4 > 1024)
				{
					m_textInfo.meshInfo[j].ResizeMeshInfo((referenceCount > 1024) ? (referenceCount + 256) : Mathf.Max(Mathf.NextPowerOfTwo(referenceCount), 256));
				}
			}
			for (int k = num3; k < m_subTextObjects.Length && (UnityEngine.Object)(object)m_subTextObjects[k] != null; k++)
			{
				if (k < m_textInfo.meshInfo.Length)
				{
					m_subTextObjects[k].canvasRenderer.SetMesh(null);
				}
			}
			return m_totalCharacterCount;
		}

		protected override void ComputeMarginSize()
		{
			if (base.rectTransform != null)
			{
				m_marginWidth = m_rectTransform.rect.width - m_margin.x - m_margin.z;
				m_marginHeight = m_rectTransform.rect.height - m_margin.y - m_margin.w;
				m_RectTransformCorners = GetTextContainerLocalCorners();
			}
		}

		protected override void OnDidApplyAnimationProperties()
		{
			m_havePropertiesChanged = true;
			SetVerticesDirty();
			SetLayoutDirty();
		}

		protected override void OnCanvasHierarchyChanged()
		{
			//((MaskableGraphic)this).OnCanvasHierarchyChanged();
			base.OnCanvasHierarchyChanged();
			m_canvas = ((Graphic)this).canvas;
		}

		protected override void OnTransformParentChanged()
		{
			//((MaskableGraphic)this).OnTransformParentChanged();
			base.OnTransformParentChanged();
			m_canvas = ((Graphic)this).canvas;
			ComputeMarginSize();
			m_havePropertiesChanged = true;
		}

		protected override void OnRectTransformDimensionsChange()
		{
			if (base.gameObject.activeInHierarchy)
			{
				ComputeMarginSize();
				UpdateSubObjectPivot();
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}

		private void LateUpdate()
		{
			if (m_rectTransform.hasChanged)
			{
				float y = m_rectTransform.lossyScale.y;
				if (!m_havePropertiesChanged && y != m_previousLossyScaleY && m_text != string.Empty && m_text != null)
				{
					UpdateSDFScale(y);
					m_previousLossyScaleY = y;
				}
				m_rectTransform.hasChanged = false;
			}
			if (m_isUsingLegacyAnimationComponent)
			{
				m_havePropertiesChanged = true;
				OnPreRenderCanvas();
			}
		}

		private void OnPreRenderCanvas()
		{
			if (!m_isAwake || (!m_ignoreActiveState && !((UIBehaviour)this).IsActive()))
			{
				return;
			}
			if (m_canvas == null)
			{
				m_canvas = ((Graphic)this).canvas;
				if (m_canvas == null)
				{
					return;
				}
			}
			loopCountA = 0;
			if (m_havePropertiesChanged || m_isLayoutDirty)
			{
				if (checkPaddingRequired)
				{
					UpdateMeshPadding();
				}
				if (m_isInputParsingRequired || m_isTextTruncated)
				{
					ParseInputText();
				}
				if (m_enableAutoSizing)
				{
					m_fontSize = Mathf.Clamp(m_fontSize, m_fontSizeMin, m_fontSizeMax);
				}
				m_maxFontSize = m_fontSizeMax;
				m_minFontSize = m_fontSizeMin;
				m_lineSpacingDelta = 0f;
				m_charWidthAdjDelta = 0f;
				m_isCharacterWrappingEnabled = false;
				m_isTextTruncated = false;
				m_havePropertiesChanged = false;
				m_isLayoutDirty = false;
				m_ignoreActiveState = false;
				GenerateTextMesh();
			}
		}

		protected override void GenerateTextMesh()
		{
			if (m_fontAsset == null || m_fontAsset.characterDictionary == null)
			{
				Debug.LogWarning("Can't Generate Mesh! No Font Asset has been assigned to Object ID: " + base.GetInstanceID());
				return;
			}
			if (m_textInfo != null)
			{
				m_textInfo.Clear();
			}
			if (m_char_buffer == null || m_char_buffer.Length == 0 || m_char_buffer[0] == 0)
			{
				ClearMesh();
				m_preferredWidth = 0f;
				m_preferredHeight = 0f;
				TMPro_EventManager.ON_TEXT_CHANGED((UnityEngine.Object)(object)this);
				return;
			}
			m_currentFontAsset = m_fontAsset;
			m_currentMaterial = m_sharedMaterial;
			m_currentMaterialIndex = 0;
			m_materialReferenceStack.SetDefault(new MaterialReference(m_currentMaterialIndex, m_currentFontAsset, null, m_currentMaterial, m_padding));
			m_currentSpriteAsset = m_spriteAsset;
			if (m_spriteAnimator != null)
			{
				m_spriteAnimator.StopAllAnimations();
			}
			int totalCharacterCount = m_totalCharacterCount;
			m_fontScale = m_fontSize / m_currentFontAsset.fontInfo.PointSize;
			float num = m_fontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale;
			float num2 = m_fontScale;
			m_fontScaleMultiplier = 1f;
			m_currentFontSize = m_fontSize;
			m_sizeStack.SetDefault(m_currentFontSize);
			float num3 = 0f;
			int num4 = 0;
			m_style = m_fontStyle;
			m_fontWeightInternal = (((m_style & FontStyles.Bold) == FontStyles.Bold) ? 700 : m_fontWeight);
			m_fontWeightStack.SetDefault(m_fontWeightInternal);
			m_fontStyleStack.Clear();
			m_lineJustification = m_textAlignment;
			m_lineJustificationStack.SetDefault(m_lineJustification);
			float num5 = 0f;
			float num6 = 0f;
			float num7 = 1f;
			m_baselineOffset = 0f;
			bool flag = false;
			Vector3 start = Vector3.zero;
			Vector3 zero = Vector3.zero;
			bool flag2 = false;
			Vector3 start2 = Vector3.zero;
			Vector3 zero2 = Vector3.zero;
			bool flag3 = false;
			Vector3 start3 = Vector3.zero;
			Vector3 vector = Vector3.zero;
			m_fontColor32 = m_fontColor;
			m_htmlColor = m_fontColor32;
			m_underlineColor = m_htmlColor;
			m_strikethroughColor = m_htmlColor;
			m_colorStack.SetDefault(m_htmlColor);
			m_underlineColorStack.SetDefault(m_htmlColor);
			m_strikethroughColorStack.SetDefault(m_htmlColor);
			m_highlightColorStack.SetDefault(m_htmlColor);
			m_actionStack.Clear();
			m_isFXMatrixSet = false;
			m_lineOffset = 0f;
			m_lineHeight = -32767f;
			float num8 = m_currentFontAsset.fontInfo.LineHeight - (m_currentFontAsset.fontInfo.Ascender - m_currentFontAsset.fontInfo.Descender);
			m_cSpacing = 0f;
			m_monoSpacing = 0f;
			float num9 = 0f;
			m_xAdvance = 0f;
			tag_LineIndent = 0f;
			tag_Indent = 0f;
			m_indentStack.SetDefault(0f);
			tag_NoParsing = false;
			m_characterCount = 0;
			m_firstCharacterOfLine = 0;
			m_lastCharacterOfLine = 0;
			m_firstVisibleCharacterOfLine = 0;
			m_lastVisibleCharacterOfLine = 0;
			m_maxLineAscender = TMP_Text.k_LargeNegativeFloat;
			m_maxLineDescender = TMP_Text.k_LargePositiveFloat;
			m_lineNumber = 0;
			m_lineVisibleCharacterCount = 0;
			bool flag4 = true;
			m_firstOverflowCharacterIndex = -1;
			m_pageNumber = 0;
			int num10 = Mathf.Clamp(m_pageToDisplay - 1, 0, m_textInfo.pageInfo.Length - 1);
			int num11 = 0;
			int num12 = 0;
			Vector4 margin = m_margin;
			float marginWidth = m_marginWidth;
			float marginHeight = m_marginHeight;
			m_marginLeft = 0f;
			m_marginRight = 0f;
			m_width = -1f;
			float num13 = marginWidth + 0.0001f - m_marginLeft - m_marginRight;
			m_meshExtents.min = TMP_Text.k_LargePositiveVector2;
			m_meshExtents.max = TMP_Text.k_LargeNegativeVector2;
			m_textInfo.ClearLineInfo();
			m_maxCapHeight = 0f;
			m_maxAscender = 0f;
			m_maxDescender = 0f;
			float num14 = 0f;
			float num15 = 0f;
			bool flag5 = false;
			m_isNewPage = false;
			bool flag6 = true;
			m_isNonBreakingSpace = false;
			bool flag7 = false;
			bool flag8 = false;
			int num16 = 0;
			SaveWordWrappingState(ref m_SavedWordWrapState, -1, -1);
			SaveWordWrappingState(ref m_SavedLineState, -1, -1);
			loopCountA++;
			int endIndex = 0;
			Vector3 vector2 = default(Vector3);
			Vector3 vector3 = default(Vector3);
			Vector3 vector4 = default(Vector3);
			Vector3 vector5 = default(Vector3);
			for (int i = 0; i < m_char_buffer.Length && m_char_buffer[i] != 0; i++)
			{
				num4 = m_char_buffer[i];
				m_textElementType = m_textInfo.characterInfo[m_characterCount].elementType;
				m_currentMaterialIndex = m_textInfo.characterInfo[m_characterCount].materialReferenceIndex;
				m_currentFontAsset = m_materialReferences[m_currentMaterialIndex].fontAsset;
				int currentMaterialIndex = m_currentMaterialIndex;
				if (m_isRichText && num4 == 60)
				{
					m_isParsingText = true;
					m_textElementType = TMP_TextElementType.Character;
					if (ValidateHtmlTag(m_char_buffer, i + 1, out endIndex))
					{
						i = endIndex;
						if (m_textElementType == TMP_TextElementType.Character)
						{
							continue;
						}
					}
				}
				m_isParsingText = false;
				bool isUsingAlternateTypeface = m_textInfo.characterInfo[m_characterCount].isUsingAlternateTypeface;
				if (m_characterCount < m_firstVisibleCharacter)
				{
					m_textInfo.characterInfo[m_characterCount].isVisible = false;
					m_textInfo.characterInfo[m_characterCount].character = '\u200b';
					m_characterCount++;
					continue;
				}
				float num17 = 1f;
				if (m_textElementType == TMP_TextElementType.Character)
				{
					if ((m_style & FontStyles.UpperCase) == FontStyles.UpperCase)
					{
						if (char.IsLower((char)num4))
						{
							num4 = char.ToUpper((char)num4);
						}
					}
					else if ((m_style & FontStyles.LowerCase) == FontStyles.LowerCase)
					{
						if (char.IsUpper((char)num4))
						{
							num4 = char.ToLower((char)num4);
						}
					}
					else if (((m_fontStyle & FontStyles.SmallCaps) == FontStyles.SmallCaps || (m_style & FontStyles.SmallCaps) == FontStyles.SmallCaps) && char.IsLower((char)num4))
					{
						num17 = 0.8f;
						num4 = char.ToUpper((char)num4);
					}
				}
				if (m_textElementType == TMP_TextElementType.Sprite)
				{
					m_currentSpriteAsset = m_textInfo.characterInfo[m_characterCount].spriteAsset;
					m_spriteIndex = m_textInfo.characterInfo[m_characterCount].spriteIndex;
					TMP_Sprite tMP_Sprite = m_currentSpriteAsset.spriteInfoList[m_spriteIndex];
					if (tMP_Sprite == null)
					{
						continue;
					}
					if (num4 == 60)
					{
						num4 = 57344 + m_spriteIndex;
					}
					else
					{
						m_spriteColor = TMP_Text.s_colorWhite;
					}
					m_currentFontAsset = m_fontAsset;
					float num18 = m_currentFontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale;
					num2 = m_fontAsset.fontInfo.Ascender / tMP_Sprite.height * tMP_Sprite.scale * num18;
					m_cached_TextElement = tMP_Sprite;
					m_textInfo.characterInfo[m_characterCount].elementType = TMP_TextElementType.Sprite;
					m_textInfo.characterInfo[m_characterCount].scale = num18;
					m_textInfo.characterInfo[m_characterCount].spriteAsset = m_currentSpriteAsset;
					m_textInfo.characterInfo[m_characterCount].fontAsset = m_currentFontAsset;
					m_textInfo.characterInfo[m_characterCount].materialReferenceIndex = m_currentMaterialIndex;
					m_currentMaterialIndex = currentMaterialIndex;
					num5 = 0f;
				}
				else if (m_textElementType == TMP_TextElementType.Character)
				{
					m_cached_TextElement = m_textInfo.characterInfo[m_characterCount].textElement;
					if (m_cached_TextElement == null)
					{
						continue;
					}
					m_currentFontAsset = m_textInfo.characterInfo[m_characterCount].fontAsset;
					m_currentMaterial = m_textInfo.characterInfo[m_characterCount].material;
					m_currentMaterialIndex = m_textInfo.characterInfo[m_characterCount].materialReferenceIndex;
					m_fontScale = m_currentFontSize * num17 / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale;
					num2 = m_fontScale * m_fontScaleMultiplier * m_cached_TextElement.scale;
					m_textInfo.characterInfo[m_characterCount].elementType = TMP_TextElementType.Character;
					m_textInfo.characterInfo[m_characterCount].scale = num2;
					num5 = ((m_currentMaterialIndex == 0) ? m_padding : m_subTextObjects[m_currentMaterialIndex].padding);
				}
				float num19 = num2;
				if (num4 == 173)
				{
					num2 = 0f;
				}
				if (m_isRightToLeft)
				{
					m_xAdvance -= ((m_cached_TextElement.xAdvance * num7 + m_characterSpacing + m_wordSpacing + m_currentFontAsset.normalSpacingOffset) * num2 + m_cSpacing) * (1f - m_charWidthAdjDelta);
					if (char.IsWhiteSpace((char)num4) || num4 == 8203)
					{
						m_xAdvance -= m_wordSpacing * num2;
					}
				}
				m_textInfo.characterInfo[m_characterCount].character = (char)num4;
				m_textInfo.characterInfo[m_characterCount].pointSize = m_currentFontSize;
				m_textInfo.characterInfo[m_characterCount].color = m_htmlColor;
				m_textInfo.characterInfo[m_characterCount].underlineColor = m_underlineColor;
				m_textInfo.characterInfo[m_characterCount].strikethroughColor = m_strikethroughColor;
				m_textInfo.characterInfo[m_characterCount].highlightColor = m_highlightColor;
				m_textInfo.characterInfo[m_characterCount].style = m_style;
				m_textInfo.characterInfo[m_characterCount].index = (short)i;
				if (m_enableKerning && m_characterCount >= 1)
				{
					int character = m_textInfo.characterInfo[m_characterCount - 1].character;
					KerningPairKey kerningPairKey = new KerningPairKey(character, num4);
					KerningPair value;
					m_currentFontAsset.kerningDictionary.TryGetValue(kerningPairKey.key, out value);
					if (value != null)
					{
						m_xAdvance += value.XadvanceOffset * num2;
					}
				}
				float num20 = 0f;
				if (m_monoSpacing != 0f)
				{
					num20 = (m_monoSpacing / 2f - (m_cached_TextElement.width / 2f + m_cached_TextElement.xOffset) * num2) * (1f - m_charWidthAdjDelta);
					m_xAdvance += num20;
				}
				if (m_textElementType == TMP_TextElementType.Character && !isUsingAlternateTypeface && ((m_style & FontStyles.Bold) == FontStyles.Bold || (m_fontStyle & FontStyles.Bold) == FontStyles.Bold))
				{
					if (m_currentMaterial.HasProperty(ShaderUtilities.ID_GradientScale))
					{
						float @float = m_currentMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
						num6 = m_currentFontAsset.boldStyle / 4f * @float * m_currentMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);
						if (num6 + num5 > @float)
						{
							num5 = @float - num6;
						}
					}
					else
					{
						num6 = 0f;
					}
					num7 = 1f + m_currentFontAsset.boldSpacing * 0.01f;
				}
				else
				{
					if (m_currentMaterial.HasProperty(ShaderUtilities.ID_GradientScale))
					{
						float float2 = m_currentMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
						num6 = m_currentFontAsset.normalStyle / 4f * float2 * m_currentMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);
						if (num6 + num5 > float2)
						{
							num5 = float2 - num6;
						}
					}
					else
					{
						num6 = 0f;
					}
					num7 = 1f;
				}
				float baseline = m_currentFontAsset.fontInfo.Baseline;
				vector2.x = m_xAdvance + (m_cached_TextElement.xOffset - num5 - num6) * num2 * (1f - m_charWidthAdjDelta);
				vector2.y = (baseline + m_cached_TextElement.yOffset + num5) * num2 - m_lineOffset + m_baselineOffset;
				vector2.z = 0f;
				vector3.x = vector2.x;
				vector3.y = vector2.y - (m_cached_TextElement.height + num5 * 2f) * num2;
				vector3.z = 0f;
				vector4.x = vector3.x + (m_cached_TextElement.width + num5 * 2f + num6 * 2f) * num2 * (1f - m_charWidthAdjDelta);
				vector4.y = vector2.y;
				vector4.z = 0f;
				vector5.x = vector4.x;
				vector5.y = vector3.y;
				vector5.z = 0f;
				if (m_textElementType == TMP_TextElementType.Character && !isUsingAlternateTypeface && ((m_style & FontStyles.Italic) == FontStyles.Italic || (m_fontStyle & FontStyles.Italic) == FontStyles.Italic))
				{
					float num21 = (float)(int)m_currentFontAsset.italicStyle * 0.01f;
					Vector3 vector6 = new Vector3(num21 * ((m_cached_TextElement.yOffset + num5 + num6) * num2), 0f, 0f);
					Vector3 vector7 = new Vector3(num21 * ((m_cached_TextElement.yOffset - m_cached_TextElement.height - num5 - num6) * num2), 0f, 0f);
					vector2 += vector6;
					vector3 += vector7;
					vector4 += vector6;
					vector5 += vector7;
				}
				if (m_isFXMatrixSet)
				{
					Vector3 b = (vector4 + vector3) / 2f;
					vector2 = m_FXMatrix.MultiplyPoint3x4(vector2 - b) + b;
					vector3 = m_FXMatrix.MultiplyPoint3x4(vector3 - b) + b;
					vector4 = m_FXMatrix.MultiplyPoint3x4(vector4 - b) + b;
					vector5 = m_FXMatrix.MultiplyPoint3x4(vector5 - b) + b;
				}
				m_textInfo.characterInfo[m_characterCount].bottomLeft = vector3;
				m_textInfo.characterInfo[m_characterCount].topLeft = vector2;
				m_textInfo.characterInfo[m_characterCount].topRight = vector4;
				m_textInfo.characterInfo[m_characterCount].bottomRight = vector5;
				m_textInfo.characterInfo[m_characterCount].origin = m_xAdvance;
				m_textInfo.characterInfo[m_characterCount].baseLine = 0f - m_lineOffset + m_baselineOffset;
				m_textInfo.characterInfo[m_characterCount].aspectRatio = (vector4.x - vector3.x) / (vector2.y - vector3.y);
				float num22 = m_currentFontAsset.fontInfo.Ascender * ((m_textElementType == TMP_TextElementType.Character) ? num2 : m_textInfo.characterInfo[m_characterCount].scale) + m_baselineOffset;
				m_textInfo.characterInfo[m_characterCount].ascender = num22 - m_lineOffset;
				m_maxLineAscender = ((num22 > m_maxLineAscender) ? num22 : m_maxLineAscender);
				float num23 = m_currentFontAsset.fontInfo.Descender * ((m_textElementType == TMP_TextElementType.Character) ? num2 : m_textInfo.characterInfo[m_characterCount].scale) + m_baselineOffset;
				float num24 = m_textInfo.characterInfo[m_characterCount].descender = num23 - m_lineOffset;
				m_maxLineDescender = ((num23 < m_maxLineDescender) ? num23 : m_maxLineDescender);
				if ((m_style & FontStyles.Subscript) == FontStyles.Subscript || (m_style & FontStyles.Superscript) == FontStyles.Superscript)
				{
					float num25 = (num22 - m_baselineOffset) / m_currentFontAsset.fontInfo.SubSize;
					num22 = m_maxLineAscender;
					m_maxLineAscender = ((num25 > m_maxLineAscender) ? num25 : m_maxLineAscender);
					float num26 = (num23 - m_baselineOffset) / m_currentFontAsset.fontInfo.SubSize;
					num23 = m_maxLineDescender;
					m_maxLineDescender = ((num26 < m_maxLineDescender) ? num26 : m_maxLineDescender);
				}
				if (m_lineNumber == 0 || m_isNewPage)
				{
					m_maxAscender = ((m_maxAscender > num22) ? m_maxAscender : num22);
					m_maxCapHeight = Mathf.Max(m_maxCapHeight, m_currentFontAsset.fontInfo.CapHeight * num2);
				}
				if (m_lineOffset == 0f)
				{
					num14 = ((num14 > num22) ? num14 : num22);
				}
				m_textInfo.characterInfo[m_characterCount].isVisible = false;
				if (num4 == 9 || (!char.IsWhiteSpace((char)num4) && num4 != 8203) || m_textElementType == TMP_TextElementType.Sprite)
				{
					m_textInfo.characterInfo[m_characterCount].isVisible = true;
					num13 = ((m_width != -1f) ? Mathf.Min(marginWidth + 0.0001f - m_marginLeft - m_marginRight, m_width) : (marginWidth + 0.0001f - m_marginLeft - m_marginRight));
					m_textInfo.lineInfo[m_lineNumber].marginLeft = m_marginLeft;
					bool flag9 = (m_lineJustification & (TextAlignmentOptions)16) == (TextAlignmentOptions)16 || (m_lineJustification & (TextAlignmentOptions)8) == (TextAlignmentOptions)8;
					if (Mathf.Abs(m_xAdvance) + ((!m_isRightToLeft) ? m_cached_TextElement.xAdvance : 0f) * (1f - m_charWidthAdjDelta) * ((num4 != 173) ? num2 : num19) > num13 * (flag9 ? 1.05f : 1f))
					{
						num12 = m_characterCount - 1;
						if (base.enableWordWrapping && m_characterCount != m_firstCharacterOfLine)
						{
							if (num16 == m_SavedWordWrapState.previous_WordBreak || flag6)
							{
								if (m_enableAutoSizing && m_fontSize > m_fontSizeMin)
								{
									if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100f)
									{
										loopCountA = 0;
										m_charWidthAdjDelta += 0.01f;
										GenerateTextMesh();
										return;
									}
									m_maxFontSize = m_fontSize;
									m_fontSize -= Mathf.Max((m_fontSize - m_minFontSize) / 2f, 0.05f);
									m_fontSize = (float)(int)(Mathf.Max(m_fontSize, m_fontSizeMin) * 20f + 0.5f) / 20f;
									if (loopCountA <= 20)
									{
										GenerateTextMesh();
									}
									return;
								}
								if (!m_isCharacterWrappingEnabled)
								{
									if (!flag7)
									{
										flag7 = true;
									}
									else
									{
										m_isCharacterWrappingEnabled = true;
									}
								}
								else
								{
									flag8 = true;
								}
							}
							i = RestoreWordWrappingState(ref m_SavedWordWrapState);
							num16 = i;
							if (m_char_buffer[i] == 173)
							{
								m_isTextTruncated = true;
								m_char_buffer[i] = 45;
								GenerateTextMesh();
								return;
							}
							if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == -32767f && !m_isNewPage)
							{
								float num27 = m_maxLineAscender - m_startOfLineAscender;
								AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, num27);
								m_lineOffset += num27;
								m_SavedWordWrapState.lineOffset = m_lineOffset;
								m_SavedWordWrapState.previousLineAscender = m_maxLineAscender;
							}
							m_isNewPage = false;
							float num28 = m_maxLineAscender - m_lineOffset;
							float num29 = m_maxLineDescender - m_lineOffset;
							m_maxDescender = ((m_maxDescender < num29) ? m_maxDescender : num29);
							if (!flag5)
							{
								num15 = m_maxDescender;
							}
							if (m_useMaxVisibleDescender && (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines))
							{
								flag5 = true;
							}
							m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex = m_firstCharacterOfLine;
							m_textInfo.lineInfo[m_lineNumber].firstVisibleCharacterIndex = (m_firstVisibleCharacterOfLine = ((m_firstCharacterOfLine > m_firstVisibleCharacterOfLine) ? m_firstCharacterOfLine : m_firstVisibleCharacterOfLine));
							m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex = (m_lastCharacterOfLine = ((m_characterCount - 1 > 0) ? (m_characterCount - 1) : 0));
							m_textInfo.lineInfo[m_lineNumber].lastVisibleCharacterIndex = (m_lastVisibleCharacterOfLine = ((m_lastVisibleCharacterOfLine < m_firstVisibleCharacterOfLine) ? m_firstVisibleCharacterOfLine : m_lastVisibleCharacterOfLine));
							m_textInfo.lineInfo[m_lineNumber].characterCount = m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex - m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex + 1;
							m_textInfo.lineInfo[m_lineNumber].visibleCharacterCount = m_lineVisibleCharacterCount;
							m_textInfo.lineInfo[m_lineNumber].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_firstVisibleCharacterOfLine].bottomLeft.x, num29);
							m_textInfo.lineInfo[m_lineNumber].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].topRight.x, num28);
							m_textInfo.lineInfo[m_lineNumber].length = m_textInfo.lineInfo[m_lineNumber].lineExtents.max.x;
							m_textInfo.lineInfo[m_lineNumber].width = num13;
							m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance - (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * num2 - m_cSpacing;
							m_textInfo.lineInfo[m_lineNumber].baseline = 0f - m_lineOffset;
							m_textInfo.lineInfo[m_lineNumber].ascender = num28;
							m_textInfo.lineInfo[m_lineNumber].descender = num29;
							m_textInfo.lineInfo[m_lineNumber].lineHeight = num28 - num29 + num8 * num;
							m_firstCharacterOfLine = m_characterCount;
							m_lineVisibleCharacterCount = 0;
							SaveWordWrappingState(ref m_SavedLineState, i, m_characterCount - 1);
							m_lineNumber++;
							flag4 = true;
							if (m_lineNumber >= m_textInfo.lineInfo.Length)
							{
								ResizeLineExtents(m_lineNumber);
							}
							if (m_lineHeight == -32767f)
							{
								float num30 = m_textInfo.characterInfo[m_characterCount].ascender - m_textInfo.characterInfo[m_characterCount].baseLine;
								num9 = 0f - m_maxLineDescender + num30 + (num8 + m_lineSpacing + m_lineSpacingDelta) * num;
								m_lineOffset += num9;
								m_startOfLineAscender = num30;
							}
							else
							{
								m_lineOffset += m_lineHeight + m_lineSpacing * num;
							}
							m_maxLineAscender = TMP_Text.k_LargeNegativeFloat;
							m_maxLineDescender = TMP_Text.k_LargePositiveFloat;
							m_xAdvance = 0f + tag_Indent;
							continue;
						}
						if (m_enableAutoSizing && m_fontSize > m_fontSizeMin)
						{
							if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100f)
							{
								loopCountA = 0;
								m_charWidthAdjDelta += 0.01f;
								GenerateTextMesh();
								return;
							}
							m_maxFontSize = m_fontSize;
							m_fontSize -= Mathf.Max((m_fontSize - m_minFontSize) / 2f, 0.05f);
							m_fontSize = (float)(int)(Mathf.Max(m_fontSize, m_fontSizeMin) * 20f + 0.5f) / 20f;
							if (loopCountA <= 20)
							{
								GenerateTextMesh();
							}
							return;
						}
						switch (m_overflowMode)
						{
						case TextOverflowModes.Overflow:
							if (m_isMaskingEnabled)
							{
								DisableMasking();
							}
							break;
						case TextOverflowModes.Ellipsis:
							if (m_isMaskingEnabled)
							{
								DisableMasking();
							}
							m_isTextTruncated = true;
							if (m_characterCount < 1)
							{
								m_textInfo.characterInfo[m_characterCount].isVisible = false;
								break;
							}
							m_char_buffer[i - 1] = 8230;
							m_char_buffer[i] = 0;
							if (m_cached_Ellipsis_GlyphInfo != null)
							{
								m_textInfo.characterInfo[num12].character = '…';
								m_textInfo.characterInfo[num12].textElement = m_cached_Ellipsis_GlyphInfo;
								m_textInfo.characterInfo[num12].fontAsset = m_materialReferences[0].fontAsset;
								m_textInfo.characterInfo[num12].material = m_materialReferences[0].material;
								m_textInfo.characterInfo[num12].materialReferenceIndex = 0;
							}
							else
							{
								Debug.LogWarning("Unable to use Ellipsis character since it wasn't found in the current Font Asset [" + m_fontAsset.name + "]. Consider regenerating this font asset to include the Ellipsis character (u+2026).\nNote: Warnings can be disabled in the TMP Settings file.", (UnityEngine.Object)(object)this);
							}
							m_totalCharacterCount = num12 + 1;
							GenerateTextMesh();
							return;
						case TextOverflowModes.Masking:
							if (!m_isMaskingEnabled)
							{
								EnableMasking();
							}
							break;
						case TextOverflowModes.ScrollRect:
							if (!m_isMaskingEnabled)
							{
								EnableMasking();
							}
							break;
						case TextOverflowModes.Truncate:
							if (m_isMaskingEnabled)
							{
								DisableMasking();
							}
							m_textInfo.characterInfo[m_characterCount].isVisible = false;
							break;
						}
					}
					if (num4 != 9)
					{
						Color32 vertexColor = (!m_overrideHtmlColors) ? m_htmlColor : m_fontColor32;
						if (m_textElementType == TMP_TextElementType.Character)
						{
							SaveGlyphVertexInfo(num5, num6, vertexColor);
						}
						else if (m_textElementType == TMP_TextElementType.Sprite)
						{
							SaveSpriteVertexInfo(vertexColor);
						}
					}
					else
					{
						m_textInfo.characterInfo[m_characterCount].isVisible = false;
						m_lastVisibleCharacterOfLine = m_characterCount;
						m_textInfo.lineInfo[m_lineNumber].spaceCount++;
						m_textInfo.spaceCount++;
					}
					if (m_textInfo.characterInfo[m_characterCount].isVisible && num4 != 173)
					{
						if (flag4)
						{
							flag4 = false;
							m_firstVisibleCharacterOfLine = m_characterCount;
						}
						m_lineVisibleCharacterCount++;
						m_lastVisibleCharacterOfLine = m_characterCount;
					}
				}
				else if ((num4 == 10 || char.IsSeparator((char)num4)) && num4 != 173 && num4 != 8203 && num4 != 8288)
				{
					m_textInfo.lineInfo[m_lineNumber].spaceCount++;
					m_textInfo.spaceCount++;
				}
				if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == -32767f && !m_isNewPage)
				{
					float num31 = m_maxLineAscender - m_startOfLineAscender;
					AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, num31);
					num24 -= num31;
					m_lineOffset += num31;
					m_startOfLineAscender += num31;
					m_SavedWordWrapState.lineOffset = m_lineOffset;
					m_SavedWordWrapState.previousLineAscender = m_startOfLineAscender;
				}
				m_textInfo.characterInfo[m_characterCount].lineNumber = (short)m_lineNumber;
				m_textInfo.characterInfo[m_characterCount].pageNumber = (short)m_pageNumber;
				if ((num4 != 10 && num4 != 13 && num4 != 8230) || m_textInfo.lineInfo[m_lineNumber].characterCount == 1)
				{
					m_textInfo.lineInfo[m_lineNumber].alignment = m_lineJustification;
				}
				if (m_maxAscender - num24 > marginHeight + 0.0001f)
				{
					if (m_enableAutoSizing && m_lineSpacingDelta > m_lineSpacingMax && m_lineNumber > 0)
					{
						loopCountA = 0;
						m_lineSpacingDelta -= 1f;
						GenerateTextMesh();
						return;
					}
					if (m_enableAutoSizing && m_fontSize > m_fontSizeMin)
					{
						m_maxFontSize = m_fontSize;
						m_fontSize -= Mathf.Max((m_fontSize - m_minFontSize) / 2f, 0.05f);
						m_fontSize = (float)(int)(Mathf.Max(m_fontSize, m_fontSizeMin) * 20f + 0.5f) / 20f;
						if (loopCountA <= 20)
						{
							GenerateTextMesh();
						}
						return;
					}
					if (m_firstOverflowCharacterIndex == -1)
					{
						m_firstOverflowCharacterIndex = m_characterCount;
					}
					switch (m_overflowMode)
					{
					case TextOverflowModes.Overflow:
						if (m_isMaskingEnabled)
						{
							DisableMasking();
						}
						break;
					case TextOverflowModes.Ellipsis:
						if (m_isMaskingEnabled)
						{
							DisableMasking();
						}
						if (m_lineNumber > 0)
						{
							m_char_buffer[m_textInfo.characterInfo[num12].index] = 8230;
							m_char_buffer[m_textInfo.characterInfo[num12].index + 1] = 0;
							if (m_cached_Ellipsis_GlyphInfo != null)
							{
								m_textInfo.characterInfo[num12].character = '…';
								m_textInfo.characterInfo[num12].textElement = m_cached_Ellipsis_GlyphInfo;
								m_textInfo.characterInfo[num12].fontAsset = m_materialReferences[0].fontAsset;
								m_textInfo.characterInfo[num12].material = m_materialReferences[0].material;
								m_textInfo.characterInfo[num12].materialReferenceIndex = 0;
							}
							else
							{
								Debug.LogWarning("Unable to use Ellipsis character since it wasn't found in the current Font Asset [" + m_fontAsset.name + "]. Consider regenerating this font asset to include the Ellipsis character (u+2026).\nNote: Warnings can be disabled in the TMP Settings file.", (UnityEngine.Object)(object)this);
							}
							m_totalCharacterCount = num12 + 1;
							GenerateTextMesh();
							m_isTextTruncated = true;
						}
						else
						{
							ClearMesh();
						}
						return;
					case TextOverflowModes.Masking:
						if (!m_isMaskingEnabled)
						{
							EnableMasking();
						}
						break;
					case TextOverflowModes.ScrollRect:
						if (!m_isMaskingEnabled)
						{
							EnableMasking();
						}
						break;
					case TextOverflowModes.Truncate:
						if (m_isMaskingEnabled)
						{
							DisableMasking();
						}
						if (m_lineNumber > 0)
						{
							m_char_buffer[m_textInfo.characterInfo[num12].index + 1] = 0;
							m_totalCharacterCount = num12 + 1;
							GenerateTextMesh();
							m_isTextTruncated = true;
						}
						else
						{
							ClearMesh();
						}
						return;
					case TextOverflowModes.Page:
						if (m_isMaskingEnabled)
						{
							DisableMasking();
						}
						if (num4 == 13 || num4 == 10)
						{
							break;
						}
						if (i == 0)
						{
							ClearMesh();
							return;
						}
						if (num11 == i)
						{
							m_char_buffer[i] = 0;
							m_isTextTruncated = true;
						}
						num11 = i;
						i = RestoreWordWrappingState(ref m_SavedLineState);
						m_isNewPage = true;
						m_xAdvance = 0f + tag_Indent;
						m_lineOffset = 0f;
						m_maxAscender = 0f;
						num14 = 0f;
						m_lineNumber++;
						m_pageNumber++;
						continue;
					case TextOverflowModes.Linked:
						if ((UnityEngine.Object)(object)m_linkedTextComponent != null)
						{
							m_linkedTextComponent.text = base.text;
							m_linkedTextComponent.firstVisibleCharacter = m_characterCount;
							m_linkedTextComponent.ForceMeshUpdate();
						}
						if (m_lineNumber > 0)
						{
							m_char_buffer[i] = 0;
							m_totalCharacterCount = m_characterCount;
							GenerateTextMesh();
							m_isTextTruncated = true;
						}
						else
						{
							ClearMesh();
						}
						return;
					}
				}
				if (num4 == 9)
				{
					float num32 = m_currentFontAsset.fontInfo.TabWidth * num2;
					float num33 = Mathf.Ceil(m_xAdvance / num32) * num32;
					m_xAdvance = ((num33 > m_xAdvance) ? num33 : (m_xAdvance + num32));
				}
				else if (m_monoSpacing != 0f)
				{
					m_xAdvance += (m_monoSpacing - num20 + (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * num2 + m_cSpacing) * (1f - m_charWidthAdjDelta);
					if (char.IsWhiteSpace((char)num4) || num4 == 8203)
					{
						m_xAdvance += m_wordSpacing * num2;
					}
				}
				else if (!m_isRightToLeft)
				{
					m_xAdvance += ((m_cached_TextElement.xAdvance * num7 + m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * num2 + m_cSpacing) * (1f - m_charWidthAdjDelta);
					if (char.IsWhiteSpace((char)num4) || num4 == 8203)
					{
						m_xAdvance += m_wordSpacing * num2;
					}
				}
				m_textInfo.characterInfo[m_characterCount].xAdvance = m_xAdvance;
				if (num4 == 13)
				{
					m_xAdvance = 0f + tag_Indent;
				}
				if (num4 == 10 || m_characterCount == totalCharacterCount - 1)
				{
					if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == -32767f && !m_isNewPage)
					{
						float num34 = m_maxLineAscender - m_startOfLineAscender;
						AdjustLineOffset(m_firstCharacterOfLine, m_characterCount, num34);
						num24 -= num34;
						m_lineOffset += num34;
					}
					m_isNewPage = false;
					float num35 = m_maxLineAscender - m_lineOffset;
					float num36 = m_maxLineDescender - m_lineOffset;
					m_maxDescender = ((m_maxDescender < num36) ? m_maxDescender : num36);
					if (!flag5)
					{
						num15 = m_maxDescender;
					}
					if (m_useMaxVisibleDescender && (m_characterCount >= m_maxVisibleCharacters || m_lineNumber >= m_maxVisibleLines))
					{
						flag5 = true;
					}
					m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex = m_firstCharacterOfLine;
					m_textInfo.lineInfo[m_lineNumber].firstVisibleCharacterIndex = (m_firstVisibleCharacterOfLine = ((m_firstCharacterOfLine > m_firstVisibleCharacterOfLine) ? m_firstCharacterOfLine : m_firstVisibleCharacterOfLine));
					m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex = (m_lastCharacterOfLine = m_characterCount);
					m_textInfo.lineInfo[m_lineNumber].lastVisibleCharacterIndex = (m_lastVisibleCharacterOfLine = ((m_lastVisibleCharacterOfLine < m_firstVisibleCharacterOfLine) ? m_firstVisibleCharacterOfLine : m_lastVisibleCharacterOfLine));
					m_textInfo.lineInfo[m_lineNumber].characterCount = m_textInfo.lineInfo[m_lineNumber].lastCharacterIndex - m_textInfo.lineInfo[m_lineNumber].firstCharacterIndex + 1;
					m_textInfo.lineInfo[m_lineNumber].visibleCharacterCount = m_lineVisibleCharacterCount;
					m_textInfo.lineInfo[m_lineNumber].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_firstVisibleCharacterOfLine].bottomLeft.x, num36);
					m_textInfo.lineInfo[m_lineNumber].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].topRight.x, num35);
					m_textInfo.lineInfo[m_lineNumber].length = m_textInfo.lineInfo[m_lineNumber].lineExtents.max.x - num5 * num2;
					m_textInfo.lineInfo[m_lineNumber].width = num13;
					if (m_textInfo.lineInfo[m_lineNumber].characterCount == 1)
					{
						m_textInfo.lineInfo[m_lineNumber].alignment = m_lineJustification;
					}
					if (m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].isVisible)
					{
						m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastVisibleCharacterOfLine].xAdvance - (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * num2 - m_cSpacing;
					}
					else
					{
						m_textInfo.lineInfo[m_lineNumber].maxAdvance = m_textInfo.characterInfo[m_lastCharacterOfLine].xAdvance - (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * num2 - m_cSpacing;
					}
					m_textInfo.lineInfo[m_lineNumber].baseline = 0f - m_lineOffset;
					m_textInfo.lineInfo[m_lineNumber].ascender = num35;
					m_textInfo.lineInfo[m_lineNumber].descender = num36;
					m_textInfo.lineInfo[m_lineNumber].lineHeight = num35 - num36 + num8 * num;
					m_firstCharacterOfLine = m_characterCount + 1;
					m_lineVisibleCharacterCount = 0;
					if (num4 == 10)
					{
						SaveWordWrappingState(ref m_SavedLineState, i, m_characterCount);
						SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
						m_lineNumber++;
						flag4 = true;
						if (m_lineNumber >= m_textInfo.lineInfo.Length)
						{
							ResizeLineExtents(m_lineNumber);
						}
						if (m_lineHeight == -32767f)
						{
							num9 = 0f - m_maxLineDescender + num22 + (num8 + m_lineSpacing + m_paragraphSpacing + m_lineSpacingDelta) * num;
							m_lineOffset += num9;
						}
						else
						{
							m_lineOffset += m_lineHeight + (m_lineSpacing + m_paragraphSpacing) * num;
						}
						m_maxLineAscender = TMP_Text.k_LargeNegativeFloat;
						m_maxLineDescender = TMP_Text.k_LargePositiveFloat;
						m_startOfLineAscender = num22;
						m_xAdvance = 0f + tag_LineIndent + tag_Indent;
						num12 = m_characterCount - 1;
						m_characterCount++;
						continue;
					}
				}
				if (m_textInfo.characterInfo[m_characterCount].isVisible)
				{
					m_meshExtents.min.x = Mathf.Min(m_meshExtents.min.x, m_textInfo.characterInfo[m_characterCount].bottomLeft.x);
					m_meshExtents.min.y = Mathf.Min(m_meshExtents.min.y, m_textInfo.characterInfo[m_characterCount].bottomLeft.y);
					m_meshExtents.max.x = Mathf.Max(m_meshExtents.max.x, m_textInfo.characterInfo[m_characterCount].topRight.x);
					m_meshExtents.max.y = Mathf.Max(m_meshExtents.max.y, m_textInfo.characterInfo[m_characterCount].topRight.y);
				}
				if (m_overflowMode == TextOverflowModes.Page && num4 != 13 && num4 != 10)
				{
					if (m_pageNumber + 1 > m_textInfo.pageInfo.Length)
					{
						TMP_TextInfo.Resize(ref m_textInfo.pageInfo, m_pageNumber + 1, true);
					}
					m_textInfo.pageInfo[m_pageNumber].ascender = num14;
					m_textInfo.pageInfo[m_pageNumber].descender = ((num23 < m_textInfo.pageInfo[m_pageNumber].descender) ? num23 : m_textInfo.pageInfo[m_pageNumber].descender);
					if (m_pageNumber == 0 && m_characterCount == 0)
					{
						m_textInfo.pageInfo[m_pageNumber].firstCharacterIndex = m_characterCount;
					}
					else if (m_characterCount > 0 && m_pageNumber != m_textInfo.characterInfo[m_characterCount - 1].pageNumber)
					{
						m_textInfo.pageInfo[m_pageNumber - 1].lastCharacterIndex = m_characterCount - 1;
						m_textInfo.pageInfo[m_pageNumber].firstCharacterIndex = m_characterCount;
					}
					else if (m_characterCount == totalCharacterCount - 1)
					{
						m_textInfo.pageInfo[m_pageNumber].lastCharacterIndex = m_characterCount;
					}
				}
				if (m_enableWordWrapping || m_overflowMode == TextOverflowModes.Truncate || m_overflowMode == TextOverflowModes.Ellipsis)
				{
					if ((char.IsWhiteSpace((char)num4) || num4 == 8203 || num4 == 45 || num4 == 173) && (!m_isNonBreakingSpace || flag7) && num4 != 160 && num4 != 8209 && num4 != 8239 && num4 != 8288)
					{
						SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
						m_isCharacterWrappingEnabled = false;
						flag6 = false;
					}
					else if (((num4 > 4352 && num4 < 4607) || (num4 > 11904 && num4 < 40959) || (num4 > 43360 && num4 < 43391) || (num4 > 44032 && num4 < 55295) || (num4 > 63744 && num4 < 64255) || (num4 > 65072 && num4 < 65103) || (num4 > 65280 && num4 < 65519)) && !m_isNonBreakingSpace)
					{
						if (flag6 || flag8 || (!TMP_Settings.linebreakingRules.leadingCharacters.ContainsKey(num4) && m_characterCount < totalCharacterCount - 1 && !TMP_Settings.linebreakingRules.followingCharacters.ContainsKey(m_textInfo.characterInfo[m_characterCount + 1].character)))
						{
							SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
							m_isCharacterWrappingEnabled = false;
							flag6 = false;
						}
					}
					else if (flag6 || m_isCharacterWrappingEnabled || flag8)
					{
						SaveWordWrappingState(ref m_SavedWordWrapState, i, m_characterCount);
					}
				}
				m_characterCount++;
			}
			num3 = m_maxFontSize - m_minFontSize;
			if (!m_isCharacterWrappingEnabled && m_enableAutoSizing && num3 > 0.051f && m_fontSize < m_fontSizeMax)
			{
				m_minFontSize = m_fontSize;
				m_fontSize += Mathf.Max((m_maxFontSize - m_fontSize) / 2f, 0.05f);
				m_fontSize = (float)(int)(Mathf.Min(m_fontSize, m_fontSizeMax) * 20f + 0.5f) / 20f;
				if (loopCountA <= 20)
				{
					GenerateTextMesh();
				}
				return;
			}
			m_isCharacterWrappingEnabled = false;
			if (m_characterCount == 0)
			{
				ClearMesh();
				TMPro_EventManager.ON_TEXT_CHANGED((UnityEngine.Object)(object)this);
				return;
			}
			int index = m_materialReferences[0].referenceCount * 4;
			m_textInfo.meshInfo[0].Clear(false);
			Vector3 a = Vector3.zero;
			Vector3[] rectTransformCorners = m_RectTransformCorners;
			switch (m_textAlignment)
			{
			case TextAlignmentOptions.TopLeft:
			case TextAlignmentOptions.Top:
			case TextAlignmentOptions.TopRight:
			case TextAlignmentOptions.TopJustified:
			/*case TextAlignmentOptions.TopFlush:
			case TextAlignmentOptions.TopGeoAligned:*/
				a = ((m_overflowMode == TextOverflowModes.Page) ? (rectTransformCorners[1] + new Vector3(0f + margin.x, 0f - m_textInfo.pageInfo[num10].ascender - margin.y, 0f)) : (rectTransformCorners[1] + new Vector3(0f + margin.x, 0f - m_maxAscender - margin.y, 0f)));
				break;
			case TextAlignmentOptions.Left:
			case TextAlignmentOptions.Center:
			case TextAlignmentOptions.Right:
			case TextAlignmentOptions.Justified:
			/*case TextAlignmentOptions.Flush:
			case TextAlignmentOptions.CenterGeoAligned:*/
				a = ((m_overflowMode == TextOverflowModes.Page) ? ((rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + margin.x, 0f - (m_textInfo.pageInfo[num10].ascender + margin.y + m_textInfo.pageInfo[num10].descender - margin.w) / 2f, 0f)) : ((rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + margin.x, 0f - (m_maxAscender + margin.y + num15 - margin.w) / 2f, 0f)));
				break;
			case TextAlignmentOptions.BottomLeft:
			case TextAlignmentOptions.Bottom:
			case TextAlignmentOptions.BottomRight:
			case TextAlignmentOptions.BottomJustified:
			/*case TextAlignmentOptions.BottomFlush:
			case TextAlignmentOptions.BottomGeoAligned:*/
				a = ((m_overflowMode == TextOverflowModes.Page) ? (rectTransformCorners[0] + new Vector3(0f + margin.x, 0f - m_textInfo.pageInfo[num10].descender + margin.w, 0f)) : (rectTransformCorners[0] + new Vector3(0f + margin.x, 0f - num15 + margin.w, 0f)));
				break;
			case TextAlignmentOptions.BaselineLeft:
			case TextAlignmentOptions.Baseline:
			case TextAlignmentOptions.BaselineRight:
			case TextAlignmentOptions.BaselineJustified:
			/*case TextAlignmentOptions.BaselineFlush:
			case TextAlignmentOptions.BaselineGeoAligned:*/
				a = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + margin.x, 0f, 0f);
				break;
			case TextAlignmentOptions.MidlineLeft:
			case TextAlignmentOptions.Midline:
			case TextAlignmentOptions.MidlineRight:
			case TextAlignmentOptions.MidlineJustified:
			/*case TextAlignmentOptions.MidlineFlush:
			case TextAlignmentOptions.MidlineGeoAligned:*/
				a = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + margin.x, 0f - (m_meshExtents.max.y + margin.y + m_meshExtents.min.y - margin.w) / 2f, 0f);
				break;
			case TextAlignmentOptions.CaplineLeft:
			case TextAlignmentOptions.Capline:
			case TextAlignmentOptions.CaplineRight:
			case TextAlignmentOptions.CaplineJustified:
			/*case TextAlignmentOptions.CaplineFlush:
			case TextAlignmentOptions.CaplineGeoAligned:*/
				a = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + margin.x, 0f - (m_maxCapHeight - margin.y - margin.w) / 2f, 0f);
				break;
			}
			Vector3 b2 = Vector3.zero;
			Vector3 zero3 = Vector3.zero;
			int index_X = 0;
			int index_X2 = 0;
			int num37 = 0;
			int num38 = 0;
			int num39 = 0;
			bool flag10 = false;
			bool flag11 = false;
			int num40 = 0;
			int num41 = 0;
			bool flag12 = (!(m_canvas.worldCamera == null)) ? true : false;
			float num42 = m_previousLossyScaleY = base.transform.lossyScale.y;
			RenderMode renderMode = m_canvas.renderMode;
			float scaleFactor = m_canvas.scaleFactor;
			Color32 color = Color.white;
			Color32 underlineColor = Color.white;
			Color32 color2 = new Color32(byte.MaxValue, byte.MaxValue, 0, 64);
			float num43 = 0f;
			float num44 = 0f;
			float num45 = 0f;
			float num46 = 0f;
			float num47 = TMP_Text.k_LargePositiveFloat;
			int num48 = 0;
			float num49 = 0f;
			float num50 = 0f;
			float b3 = 0f;
			TMP_CharacterInfo[] characterInfo = m_textInfo.characterInfo;
			for (int j = 0; j < m_characterCount; j++)
			{
				TMP_FontAsset fontAsset = characterInfo[j].fontAsset;
				char character2 = characterInfo[j].character;
				int lineNumber = characterInfo[j].lineNumber;
				TMP_LineInfo tMP_LineInfo = m_textInfo.lineInfo[lineNumber];
				num38 = lineNumber + 1;
				TextAlignmentOptions alignment = tMP_LineInfo.alignment;
				switch (alignment)
				{
				case TextAlignmentOptions.TopLeft:
				case TextAlignmentOptions.Left:
				case TextAlignmentOptions.BottomLeft:
				case TextAlignmentOptions.BaselineLeft:
				case TextAlignmentOptions.MidlineLeft:
				case TextAlignmentOptions.CaplineLeft:
					b2 = (m_isRightToLeft ? new Vector3(0f - tMP_LineInfo.maxAdvance, 0f, 0f) : new Vector3(0f + tMP_LineInfo.marginLeft, 0f, 0f));
					break;
				case TextAlignmentOptions.Top:
				case TextAlignmentOptions.Center:
				case TextAlignmentOptions.Bottom:
				case TextAlignmentOptions.Baseline:
				case TextAlignmentOptions.Midline:
				case TextAlignmentOptions.Capline:
					b2 = new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width / 2f - tMP_LineInfo.maxAdvance / 2f, 0f, 0f);
					break;
				/*case TextAlignmentOptions.TopGeoAligned:
				case TextAlignmentOptions.CenterGeoAligned:
				case TextAlignmentOptions.BottomGeoAligned:
				case TextAlignmentOptions.BaselineGeoAligned:
				case TextAlignmentOptions.MidlineGeoAligned:
				case TextAlignmentOptions.CaplineGeoAligned:
					b2 = new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width / 2f - (tMP_LineInfo.lineExtents.min.x + tMP_LineInfo.lineExtents.max.x) / 2f, 0f, 0f);
					break;*/
				case TextAlignmentOptions.TopRight:
				case TextAlignmentOptions.Right:
				case TextAlignmentOptions.BottomRight:
				case TextAlignmentOptions.BaselineRight:
				case TextAlignmentOptions.MidlineRight:
				case TextAlignmentOptions.CaplineRight:
					b2 = (m_isRightToLeft ? new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width, 0f, 0f) : new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width - tMP_LineInfo.maxAdvance, 0f, 0f));
					break;
				case TextAlignmentOptions.TopJustified:
				//case TextAlignmentOptions.TopFlush:
				case TextAlignmentOptions.Justified:
				//case TextAlignmentOptions.Flush:
				case TextAlignmentOptions.BottomJustified:
				//case TextAlignmentOptions.BottomFlush:
				case TextAlignmentOptions.BaselineJustified:
				//case TextAlignmentOptions.BaselineFlush:
				case TextAlignmentOptions.MidlineJustified:
				//case TextAlignmentOptions.MidlineFlush:
				case TextAlignmentOptions.CaplineJustified:
				//case TextAlignmentOptions.CaplineFlush:
				{
					if (character2 == '­' || character2 == '\u200b' || character2 == '\u2060')
					{
						break;
					}
					char character3 = characterInfo[tMP_LineInfo.lastCharacterIndex].character;
					bool flag13 = (alignment & (TextAlignmentOptions)16) == (TextAlignmentOptions)16;
					if ((!char.IsControl(character3) && lineNumber < m_lineNumber) || flag13 || tMP_LineInfo.maxAdvance > tMP_LineInfo.width)
					{
						if (lineNumber != num39 || j == 0 || j == m_firstVisibleCharacter)
						{
							b2 = ((!m_isRightToLeft) ? new Vector3(tMP_LineInfo.marginLeft, 0f, 0f) : new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width, 0f, 0f));
							flag10 = (char.IsSeparator(character2) ? true : false);
							break;
						}
						float num51 = (!m_isRightToLeft) ? (tMP_LineInfo.width - tMP_LineInfo.maxAdvance) : (tMP_LineInfo.width + tMP_LineInfo.maxAdvance);
						int num52 = tMP_LineInfo.visibleCharacterCount - 1;
						int num53 = characterInfo[tMP_LineInfo.lastCharacterIndex].isVisible ? tMP_LineInfo.spaceCount : (tMP_LineInfo.spaceCount - 1);
						if (flag10)
						{
							num53--;
							num52++;
						}
						float num54 = (num53 > 0) ? m_wordWrappingRatios : 1f;
						if (num53 < 1)
						{
							num53 = 1;
						}
						if (character2 == '\t' || char.IsSeparator(character2))
						{
							if (!m_isRightToLeft)
							{
								b2 += new Vector3(num51 * (1f - num54) / (float)num53, 0f, 0f);
							}
							else
							{
								b2 -= new Vector3(num51 * (1f - num54) / (float)num53, 0f, 0f);
							}
						}
						else if (!m_isRightToLeft)
						{
							b2 += new Vector3(num51 * num54 / (float)num52, 0f, 0f);
						}
						else
						{
							b2 -= new Vector3(num51 * num54 / (float)num52, 0f, 0f);
						}
					}
					else
					{
						b2 = (m_isRightToLeft ? new Vector3(tMP_LineInfo.marginLeft + tMP_LineInfo.width, 0f, 0f) : new Vector3(tMP_LineInfo.marginLeft, 0f, 0f));
					}
					break;
				}
				}
				zero3 = a + b2;
				bool isVisible = characterInfo[j].isVisible;
				if (isVisible)
				{
					TMP_TextElementType elementType = characterInfo[j].elementType;
					switch (elementType)
					{
					case TMP_TextElementType.Character:
					{
						Extents lineExtents = tMP_LineInfo.lineExtents;
						float num55 = m_uvLineOffset * (float)lineNumber % 1f;
						switch (m_horizontalMapping)
						{
						case TextureMappingOptions.Character:
							characterInfo[j].vertex_BL.uv2.x = 0f;
							characterInfo[j].vertex_TL.uv2.x = 0f;
							characterInfo[j].vertex_TR.uv2.x = 1f;
							characterInfo[j].vertex_BR.uv2.x = 1f;
							break;
						case TextureMappingOptions.Line:
							if (m_textAlignment != TextAlignmentOptions.Justified)
							{
								characterInfo[j].vertex_BL.uv2.x = (characterInfo[j].vertex_BL.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num55;
								characterInfo[j].vertex_TL.uv2.x = (characterInfo[j].vertex_TL.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num55;
								characterInfo[j].vertex_TR.uv2.x = (characterInfo[j].vertex_TR.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num55;
								characterInfo[j].vertex_BR.uv2.x = (characterInfo[j].vertex_BR.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num55;
							}
							else
							{
								characterInfo[j].vertex_BL.uv2.x = (characterInfo[j].vertex_BL.position.x + b2.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num55;
								characterInfo[j].vertex_TL.uv2.x = (characterInfo[j].vertex_TL.position.x + b2.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num55;
								characterInfo[j].vertex_TR.uv2.x = (characterInfo[j].vertex_TR.position.x + b2.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num55;
								characterInfo[j].vertex_BR.uv2.x = (characterInfo[j].vertex_BR.position.x + b2.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num55;
							}
							break;
						case TextureMappingOptions.Paragraph:
							characterInfo[j].vertex_BL.uv2.x = (characterInfo[j].vertex_BL.position.x + b2.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num55;
							characterInfo[j].vertex_TL.uv2.x = (characterInfo[j].vertex_TL.position.x + b2.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num55;
							characterInfo[j].vertex_TR.uv2.x = (characterInfo[j].vertex_TR.position.x + b2.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num55;
							characterInfo[j].vertex_BR.uv2.x = (characterInfo[j].vertex_BR.position.x + b2.x - m_meshExtents.min.x) / (m_meshExtents.max.x - m_meshExtents.min.x) + num55;
							break;
						case TextureMappingOptions.MatchAspect:
						{
							switch (m_verticalMapping)
							{
							case TextureMappingOptions.Character:
								characterInfo[j].vertex_BL.uv2.y = 0f;
								characterInfo[j].vertex_TL.uv2.y = 1f;
								characterInfo[j].vertex_TR.uv2.y = 0f;
								characterInfo[j].vertex_BR.uv2.y = 1f;
								break;
							case TextureMappingOptions.Line:
								characterInfo[j].vertex_BL.uv2.y = (characterInfo[j].vertex_BL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + num55;
								characterInfo[j].vertex_TL.uv2.y = (characterInfo[j].vertex_TL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + num55;
								characterInfo[j].vertex_TR.uv2.y = characterInfo[j].vertex_BL.uv2.y;
								characterInfo[j].vertex_BR.uv2.y = characterInfo[j].vertex_TL.uv2.y;
								break;
							case TextureMappingOptions.Paragraph:
								characterInfo[j].vertex_BL.uv2.y = (characterInfo[j].vertex_BL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + num55;
								characterInfo[j].vertex_TL.uv2.y = (characterInfo[j].vertex_TL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y) + num55;
								characterInfo[j].vertex_TR.uv2.y = characterInfo[j].vertex_BL.uv2.y;
								characterInfo[j].vertex_BR.uv2.y = characterInfo[j].vertex_TL.uv2.y;
								break;
							case TextureMappingOptions.MatchAspect:
								Debug.Log("ERROR: Cannot Match both Vertical & Horizontal.");
								break;
							}
							float num56 = (1f - (characterInfo[j].vertex_BL.uv2.y + characterInfo[j].vertex_TL.uv2.y) * characterInfo[j].aspectRatio) / 2f;
							characterInfo[j].vertex_BL.uv2.x = characterInfo[j].vertex_BL.uv2.y * characterInfo[j].aspectRatio + num56 + num55;
							characterInfo[j].vertex_TL.uv2.x = characterInfo[j].vertex_BL.uv2.x;
							characterInfo[j].vertex_TR.uv2.x = characterInfo[j].vertex_TL.uv2.y * characterInfo[j].aspectRatio + num56 + num55;
							characterInfo[j].vertex_BR.uv2.x = characterInfo[j].vertex_TR.uv2.x;
							break;
						}
						}
						switch (m_verticalMapping)
						{
						case TextureMappingOptions.Character:
							characterInfo[j].vertex_BL.uv2.y = 0f;
							characterInfo[j].vertex_TL.uv2.y = 1f;
							characterInfo[j].vertex_TR.uv2.y = 1f;
							characterInfo[j].vertex_BR.uv2.y = 0f;
							break;
						case TextureMappingOptions.Line:
							characterInfo[j].vertex_BL.uv2.y = (characterInfo[j].vertex_BL.position.y - tMP_LineInfo.descender) / (tMP_LineInfo.ascender - tMP_LineInfo.descender);
							characterInfo[j].vertex_TL.uv2.y = (characterInfo[j].vertex_TL.position.y - tMP_LineInfo.descender) / (tMP_LineInfo.ascender - tMP_LineInfo.descender);
							characterInfo[j].vertex_TR.uv2.y = characterInfo[j].vertex_TL.uv2.y;
							characterInfo[j].vertex_BR.uv2.y = characterInfo[j].vertex_BL.uv2.y;
							break;
						case TextureMappingOptions.Paragraph:
							characterInfo[j].vertex_BL.uv2.y = (characterInfo[j].vertex_BL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y);
							characterInfo[j].vertex_TL.uv2.y = (characterInfo[j].vertex_TL.position.y - m_meshExtents.min.y) / (m_meshExtents.max.y - m_meshExtents.min.y);
							characterInfo[j].vertex_TR.uv2.y = characterInfo[j].vertex_TL.uv2.y;
							characterInfo[j].vertex_BR.uv2.y = characterInfo[j].vertex_BL.uv2.y;
							break;
						case TextureMappingOptions.MatchAspect:
						{
							float num57 = (1f - (characterInfo[j].vertex_BL.uv2.x + characterInfo[j].vertex_TR.uv2.x) / characterInfo[j].aspectRatio) / 2f;
							characterInfo[j].vertex_BL.uv2.y = num57 + characterInfo[j].vertex_BL.uv2.x / characterInfo[j].aspectRatio;
							characterInfo[j].vertex_TL.uv2.y = num57 + characterInfo[j].vertex_TR.uv2.x / characterInfo[j].aspectRatio;
							characterInfo[j].vertex_BR.uv2.y = characterInfo[j].vertex_BL.uv2.y;
							characterInfo[j].vertex_TR.uv2.y = characterInfo[j].vertex_TL.uv2.y;
							break;
						}
						}
						num43 = characterInfo[j].scale * (1f - m_charWidthAdjDelta);
						if (!characterInfo[j].isUsingAlternateTypeface && (characterInfo[j].style & FontStyles.Bold) == FontStyles.Bold)
						{
							num43 *= -1f;
						}
						switch (renderMode)
						{
						case RenderMode.ScreenSpaceOverlay:
							num43 *= num42 / scaleFactor;
							break;
						case RenderMode.ScreenSpaceCamera:
							num43 *= (flag12 ? num42 : 1f);
							break;
						case RenderMode.WorldSpace:
							num43 *= num42;
							break;
						}
						float x = characterInfo[j].vertex_BL.uv2.x;
						float y = characterInfo[j].vertex_BL.uv2.y;
						float x2 = characterInfo[j].vertex_TR.uv2.x;
						float y2 = characterInfo[j].vertex_TR.uv2.y;
						float num58 = (int)x;
						float num59 = (int)y;
						x -= num58;
						x2 -= num58;
						y -= num59;
						y2 -= num59;
						characterInfo[j].vertex_BL.uv2.x = PackUV(x, y);
						characterInfo[j].vertex_BL.uv2.y = num43;
						characterInfo[j].vertex_TL.uv2.x = PackUV(x, y2);
						characterInfo[j].vertex_TL.uv2.y = num43;
						characterInfo[j].vertex_TR.uv2.x = PackUV(x2, y2);
						characterInfo[j].vertex_TR.uv2.y = num43;
						characterInfo[j].vertex_BR.uv2.x = PackUV(x2, y);
						characterInfo[j].vertex_BR.uv2.y = num43;
						break;
					}
					}
					if (j < m_maxVisibleCharacters && num37 < m_maxVisibleWords && lineNumber < m_maxVisibleLines && m_overflowMode != TextOverflowModes.Page)
					{
						characterInfo[j].vertex_BL.position += zero3;
						characterInfo[j].vertex_TL.position += zero3;
						characterInfo[j].vertex_TR.position += zero3;
						characterInfo[j].vertex_BR.position += zero3;
					}
					else if (j < m_maxVisibleCharacters && num37 < m_maxVisibleWords && lineNumber < m_maxVisibleLines && m_overflowMode == TextOverflowModes.Page && characterInfo[j].pageNumber == num10)
					{
						characterInfo[j].vertex_BL.position += zero3;
						characterInfo[j].vertex_TL.position += zero3;
						characterInfo[j].vertex_TR.position += zero3;
						characterInfo[j].vertex_BR.position += zero3;
					}
					else
					{
						characterInfo[j].vertex_BL.position = Vector3.zero;
						characterInfo[j].vertex_TL.position = Vector3.zero;
						characterInfo[j].vertex_TR.position = Vector3.zero;
						characterInfo[j].vertex_BR.position = Vector3.zero;
						characterInfo[j].isVisible = false;
					}
					switch (elementType)
					{
					case TMP_TextElementType.Character:
						FillCharacterVertexBuffers(j, index_X);
						break;
					case TMP_TextElementType.Sprite:
						FillSpriteVertexBuffers(j, index_X2);
						break;
					}
				}
				m_textInfo.characterInfo[j].bottomLeft += zero3;
				m_textInfo.characterInfo[j].topLeft += zero3;
				m_textInfo.characterInfo[j].topRight += zero3;
				m_textInfo.characterInfo[j].bottomRight += zero3;
				m_textInfo.characterInfo[j].origin += zero3.x;
				m_textInfo.characterInfo[j].xAdvance += zero3.x;
				m_textInfo.characterInfo[j].ascender += zero3.y;
				m_textInfo.characterInfo[j].descender += zero3.y;
				m_textInfo.characterInfo[j].baseLine += zero3.y;
				if (isVisible)
				{
				}
				if (lineNumber != num39 || j == m_characterCount - 1)
				{
					if (lineNumber != num39)
					{
						m_textInfo.lineInfo[num39].baseline += zero3.y;
						m_textInfo.lineInfo[num39].ascender += zero3.y;
						m_textInfo.lineInfo[num39].descender += zero3.y;
						m_textInfo.lineInfo[num39].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[num39].firstCharacterIndex].bottomLeft.x, m_textInfo.lineInfo[num39].descender);
						m_textInfo.lineInfo[num39].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[num39].lastVisibleCharacterIndex].topRight.x, m_textInfo.lineInfo[num39].ascender);
					}
					if (j == m_characterCount - 1)
					{
						m_textInfo.lineInfo[lineNumber].baseline += zero3.y;
						m_textInfo.lineInfo[lineNumber].ascender += zero3.y;
						m_textInfo.lineInfo[lineNumber].descender += zero3.y;
						m_textInfo.lineInfo[lineNumber].lineExtents.min = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[lineNumber].firstCharacterIndex].bottomLeft.x, m_textInfo.lineInfo[lineNumber].descender);
						m_textInfo.lineInfo[lineNumber].lineExtents.max = new Vector2(m_textInfo.characterInfo[m_textInfo.lineInfo[lineNumber].lastVisibleCharacterIndex].topRight.x, m_textInfo.lineInfo[lineNumber].ascender);
					}
				}
				if (char.IsLetterOrDigit(character2) || character2 == '-' || character2 == '­' || character2 == '‐' || character2 == '‑')
				{
					if (!flag11)
					{
						flag11 = true;
						num40 = j;
					}
					if (flag11 && j == m_characterCount - 1)
					{
						int num60 = m_textInfo.wordInfo.Length;
						int wordCount = m_textInfo.wordCount;
						if (m_textInfo.wordCount + 1 > num60)
						{
							TMP_TextInfo.Resize(ref m_textInfo.wordInfo, num60 + 1);
						}
						num41 = j;
						m_textInfo.wordInfo[wordCount].firstCharacterIndex = num40;
						m_textInfo.wordInfo[wordCount].lastCharacterIndex = num41;
						m_textInfo.wordInfo[wordCount].characterCount = num41 - num40 + 1;
						m_textInfo.wordInfo[wordCount].textComponent = this;
						num37++;
						m_textInfo.wordCount++;
						m_textInfo.lineInfo[lineNumber].wordCount++;
					}
				}
				else if ((flag11 || (j == 0 && (!char.IsPunctuation(character2) || char.IsWhiteSpace(character2) || character2 == '\u200b' || j == m_characterCount - 1))) && (j <= 0 || j >= characterInfo.Length - 1 || j >= m_characterCount || (character2 != '\'' && character2 != '’') || !char.IsLetterOrDigit(characterInfo[j - 1].character) || !char.IsLetterOrDigit(characterInfo[j + 1].character)))
				{
					num41 = ((j == m_characterCount - 1 && char.IsLetterOrDigit(character2)) ? j : (j - 1));
					flag11 = false;
					int num61 = m_textInfo.wordInfo.Length;
					int wordCount2 = m_textInfo.wordCount;
					if (m_textInfo.wordCount + 1 > num61)
					{
						TMP_TextInfo.Resize(ref m_textInfo.wordInfo, num61 + 1);
					}
					m_textInfo.wordInfo[wordCount2].firstCharacterIndex = num40;
					m_textInfo.wordInfo[wordCount2].lastCharacterIndex = num41;
					m_textInfo.wordInfo[wordCount2].characterCount = num41 - num40 + 1;
					m_textInfo.wordInfo[wordCount2].textComponent = this;
					num37++;
					m_textInfo.wordCount++;
					m_textInfo.lineInfo[lineNumber].wordCount++;
				}
				if ((m_textInfo.characterInfo[j].style & FontStyles.Underline) == FontStyles.Underline)
				{
					bool flag14 = true;
					int pageNumber = m_textInfo.characterInfo[j].pageNumber;
					if (j > m_maxVisibleCharacters || lineNumber > m_maxVisibleLines || (m_overflowMode == TextOverflowModes.Page && pageNumber + 1 != m_pageToDisplay))
					{
						flag14 = false;
					}
					if (!char.IsWhiteSpace(character2) && character2 != '\u200b')
					{
						num46 = Mathf.Max(num46, m_textInfo.characterInfo[j].scale);
						num47 = Mathf.Min((pageNumber == num48) ? num47 : TMP_Text.k_LargePositiveFloat, m_textInfo.characterInfo[j].baseLine + base.font.fontInfo.Underline * num46);
						num48 = pageNumber;
					}
					if (!flag && flag14 && j <= tMP_LineInfo.lastVisibleCharacterIndex && character2 != '\n' && character2 != '\r' && (j != tMP_LineInfo.lastVisibleCharacterIndex || !char.IsSeparator(character2)))
					{
						flag = true;
						num44 = m_textInfo.characterInfo[j].scale;
						if (num46 == 0f)
						{
							num46 = num44;
						}
						start = new Vector3(m_textInfo.characterInfo[j].bottomLeft.x, num47, 0f);
						color = m_textInfo.characterInfo[j].underlineColor;
					}
					if (flag && m_characterCount == 1)
					{
						flag = false;
						zero = new Vector3(m_textInfo.characterInfo[j].topRight.x, num47, 0f);
						num45 = m_textInfo.characterInfo[j].scale;
						DrawUnderlineMesh(start, zero, ref index, num44, num45, num46, num43, color);
						num46 = 0f;
						num47 = TMP_Text.k_LargePositiveFloat;
					}
					else if (flag && (j == tMP_LineInfo.lastCharacterIndex || j >= tMP_LineInfo.lastVisibleCharacterIndex))
					{
						if (char.IsWhiteSpace(character2) || character2 == '\u200b')
						{
							int lastVisibleCharacterIndex = tMP_LineInfo.lastVisibleCharacterIndex;
							zero = new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x, num47, 0f);
							num45 = m_textInfo.characterInfo[lastVisibleCharacterIndex].scale;
						}
						else
						{
							zero = new Vector3(m_textInfo.characterInfo[j].topRight.x, num47, 0f);
							num45 = m_textInfo.characterInfo[j].scale;
						}
						flag = false;
						DrawUnderlineMesh(start, zero, ref index, num44, num45, num46, num43, color);
						num46 = 0f;
						num47 = TMP_Text.k_LargePositiveFloat;
					}
					else if (flag && !flag14)
					{
						flag = false;
						zero = new Vector3(m_textInfo.characterInfo[j - 1].topRight.x, num47, 0f);
						num45 = m_textInfo.characterInfo[j - 1].scale;
						DrawUnderlineMesh(start, zero, ref index, num44, num45, num46, num43, color);
						num46 = 0f;
						num47 = TMP_Text.k_LargePositiveFloat;
					}
					else if (flag && j < m_characterCount - 1 && !color.Compare(m_textInfo.characterInfo[j + 1].underlineColor))
					{
						flag = false;
						zero = new Vector3(m_textInfo.characterInfo[j].topRight.x, num47, 0f);
						num45 = m_textInfo.characterInfo[j].scale;
						DrawUnderlineMesh(start, zero, ref index, num44, num45, num46, num43, color);
						num46 = 0f;
						num47 = TMP_Text.k_LargePositiveFloat;
					}
				}
				else if (flag)
				{
					flag = false;
					zero = new Vector3(m_textInfo.characterInfo[j - 1].topRight.x, num47, 0f);
					num45 = m_textInfo.characterInfo[j - 1].scale;
					DrawUnderlineMesh(start, zero, ref index, num44, num45, num46, num43, color);
					num46 = 0f;
					num47 = TMP_Text.k_LargePositiveFloat;
				}
				bool flag15 = (m_textInfo.characterInfo[j].style & FontStyles.Strikethrough) == FontStyles.Strikethrough;
				float strikethrough = fontAsset.fontInfo.strikethrough;
				if (flag15)
				{
					bool flag16 = true;
					if (j > m_maxVisibleCharacters || lineNumber > m_maxVisibleLines || (m_overflowMode == TextOverflowModes.Page && m_textInfo.characterInfo[j].pageNumber + 1 != m_pageToDisplay))
					{
						flag16 = false;
					}
					if (!flag2 && flag16 && j <= tMP_LineInfo.lastVisibleCharacterIndex && character2 != '\n' && character2 != '\r' && (j != tMP_LineInfo.lastVisibleCharacterIndex || !char.IsSeparator(character2)))
					{
						flag2 = true;
						num49 = m_textInfo.characterInfo[j].pointSize;
						num50 = m_textInfo.characterInfo[j].scale;
						start2 = new Vector3(m_textInfo.characterInfo[j].bottomLeft.x, m_textInfo.characterInfo[j].baseLine + strikethrough * num50, 0f);
						underlineColor = m_textInfo.characterInfo[j].strikethroughColor;
						b3 = m_textInfo.characterInfo[j].baseLine;
					}
					if (flag2 && m_characterCount == 1)
					{
						flag2 = false;
						zero2 = new Vector3(m_textInfo.characterInfo[j].topRight.x, m_textInfo.characterInfo[j].baseLine + strikethrough * num50, 0f);
						DrawUnderlineMesh(start2, zero2, ref index, num50, num50, num50, num43, underlineColor);
					}
					else if (flag2 && j == tMP_LineInfo.lastCharacterIndex)
					{
						if (!char.IsWhiteSpace(character2) && character2 != '\u200b')
						{
							zero2 = new Vector3(m_textInfo.characterInfo[j].topRight.x, m_textInfo.characterInfo[j].baseLine + strikethrough * num50, 0f);
						}
						else
						{
							int lastVisibleCharacterIndex2 = tMP_LineInfo.lastVisibleCharacterIndex;
							zero2 = new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex2].topRight.x, m_textInfo.characterInfo[lastVisibleCharacterIndex2].baseLine + strikethrough * num50, 0f);
						}
						flag2 = false;
						DrawUnderlineMesh(start2, zero2, ref index, num50, num50, num50, num43, underlineColor);
					}
					else if (flag2 && j < m_characterCount && (m_textInfo.characterInfo[j + 1].pointSize != num49 || !TMP_Math.Approximately(m_textInfo.characterInfo[j + 1].baseLine + zero3.y, b3)))
					{
						flag2 = false;
						int lastVisibleCharacterIndex3 = tMP_LineInfo.lastVisibleCharacterIndex;
						zero2 = ((j > lastVisibleCharacterIndex3) ? new Vector3(m_textInfo.characterInfo[lastVisibleCharacterIndex3].topRight.x, m_textInfo.characterInfo[lastVisibleCharacterIndex3].baseLine + strikethrough * num50, 0f) : new Vector3(m_textInfo.characterInfo[j].topRight.x, m_textInfo.characterInfo[j].baseLine + strikethrough * num50, 0f));
						DrawUnderlineMesh(start2, zero2, ref index, num50, num50, num50, num43, underlineColor);
					}
					else if (flag2 && j < m_characterCount && fontAsset.GetInstanceID() != characterInfo[j + 1].fontAsset.GetInstanceID())
					{
						flag2 = false;
						zero2 = new Vector3(m_textInfo.characterInfo[j].topRight.x, m_textInfo.characterInfo[j].baseLine + strikethrough * num50, 0f);
						DrawUnderlineMesh(start2, zero2, ref index, num50, num50, num50, num43, underlineColor);
					}
					else if (flag2 && !flag16)
					{
						flag2 = false;
						zero2 = new Vector3(m_textInfo.characterInfo[j - 1].topRight.x, m_textInfo.characterInfo[j - 1].baseLine + strikethrough * num50, 0f);
						DrawUnderlineMesh(start2, zero2, ref index, num50, num50, num50, num43, underlineColor);
					}
				}
				else if (flag2)
				{
					flag2 = false;
					zero2 = new Vector3(m_textInfo.characterInfo[j - 1].topRight.x, m_textInfo.characterInfo[j - 1].baseLine + strikethrough * num50, 0f);
					DrawUnderlineMesh(start2, zero2, ref index, num50, num50, num50, num43, underlineColor);
				}
				if ((m_textInfo.characterInfo[j].style & FontStyles.Highlight) == FontStyles.Highlight)
				{
					bool flag17 = true;
					int pageNumber2 = m_textInfo.characterInfo[j].pageNumber;
					if (j > m_maxVisibleCharacters || lineNumber > m_maxVisibleLines || (m_overflowMode == TextOverflowModes.Page && pageNumber2 + 1 != m_pageToDisplay))
					{
						flag17 = false;
					}
					if (!flag3 && flag17 && j <= tMP_LineInfo.lastVisibleCharacterIndex && character2 != '\n' && character2 != '\r' && (j != tMP_LineInfo.lastVisibleCharacterIndex || !char.IsSeparator(character2)))
					{
						flag3 = true;
						start3 = TMP_Text.k_LargePositiveVector2;
						vector = TMP_Text.k_LargeNegativeVector2;
						color2 = m_textInfo.characterInfo[j].highlightColor;
					}
					if (flag3)
					{
						Color32 highlightColor = m_textInfo.characterInfo[j].highlightColor;
						bool flag18 = false;
						if (!color2.Compare(highlightColor))
						{
							vector.x = (vector.x + m_textInfo.characterInfo[j].bottomLeft.x) / 2f;
							start3.y = Mathf.Min(start3.y, m_textInfo.characterInfo[j].descender);
							vector.y = Mathf.Max(vector.y, m_textInfo.characterInfo[j].ascender);
							DrawTextHighlight(start3, vector, ref index, color2);
							flag3 = true;
							start3 = vector;
							vector = new Vector3(m_textInfo.characterInfo[j].topRight.x, m_textInfo.characterInfo[j].descender, 0f);
							color2 = m_textInfo.characterInfo[j].highlightColor;
							flag18 = true;
						}
						if (!flag18)
						{
							start3.x = Mathf.Min(start3.x, m_textInfo.characterInfo[j].bottomLeft.x);
							start3.y = Mathf.Min(start3.y, m_textInfo.characterInfo[j].descender);
							vector.x = Mathf.Max(vector.x, m_textInfo.characterInfo[j].topRight.x);
							vector.y = Mathf.Max(vector.y, m_textInfo.characterInfo[j].ascender);
						}
					}
					if (flag3 && m_characterCount == 1)
					{
						flag3 = false;
						DrawTextHighlight(start3, vector, ref index, color2);
					}
					else if (flag3 && (j == tMP_LineInfo.lastCharacterIndex || j >= tMP_LineInfo.lastVisibleCharacterIndex))
					{
						flag3 = false;
						DrawTextHighlight(start3, vector, ref index, color2);
					}
					else if (flag3 && !flag17)
					{
						flag3 = false;
						DrawTextHighlight(start3, vector, ref index, color2);
					}
				}
				else if (flag3)
				{
					flag3 = false;
					DrawTextHighlight(start3, vector, ref index, color2);
				}
				num39 = lineNumber;
			}
			m_textInfo.characterCount = (short)m_characterCount;
			m_textInfo.spriteCount = m_spriteCount;
			m_textInfo.lineCount = (short)num38;
			m_textInfo.wordCount = ((num37 == 0 || m_characterCount <= 0) ? 1 : ((short)num37));
			m_textInfo.pageCount = m_pageNumber + 1;
			if (m_renderMode == TextRenderFlags.Render)
			{
				if (m_canvas.additionalShaderChannels != (AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent))
				{
					m_canvas.additionalShaderChannels = (AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.Normal | AdditionalCanvasShaderChannels.Tangent);
				}
				if (m_geometrySortingOrder != 0)
				{
					m_textInfo.meshInfo[0].SortGeometry(VertexSortingOrder.Reverse);
				}
				m_mesh.MarkDynamic();
				m_mesh.vertices = m_textInfo.meshInfo[0].vertices;
				m_mesh.uv = m_textInfo.meshInfo[0].uvs0;
				m_mesh.uv2 = m_textInfo.meshInfo[0].uvs2;
				m_mesh.colors32 = m_textInfo.meshInfo[0].colors32;
				m_mesh.RecalculateBounds();
				m_canvasRenderer.SetMesh(m_mesh);
				Color color3 = m_canvasRenderer.GetColor();
				for (int k = 1; k < m_textInfo.materialCount; k++)
				{
					m_textInfo.meshInfo[k].ClearUnusedVertices();
					if (!((UnityEngine.Object)(object)m_subTextObjects[k] == null))
					{
						if (m_geometrySortingOrder != 0)
						{
							m_textInfo.meshInfo[k].SortGeometry(VertexSortingOrder.Reverse);
						}
						m_subTextObjects[k].mesh.vertices = m_textInfo.meshInfo[k].vertices;
						m_subTextObjects[k].mesh.uv = m_textInfo.meshInfo[k].uvs0;
						m_subTextObjects[k].mesh.uv2 = m_textInfo.meshInfo[k].uvs2;
						m_subTextObjects[k].mesh.colors32 = m_textInfo.meshInfo[k].colors32;
						m_subTextObjects[k].mesh.RecalculateBounds();
						m_subTextObjects[k].canvasRenderer.SetMesh(m_subTextObjects[k].mesh);
						m_subTextObjects[k].canvasRenderer.SetColor(color3);
					}
				}
			}
			TMPro_EventManager.ON_TEXT_CHANGED((UnityEngine.Object)(object)this);
		}

		protected override Vector3[] GetTextContainerLocalCorners()
		{
			if (m_rectTransform == null)
			{
				m_rectTransform = base.rectTransform;
			}
			m_rectTransform.GetLocalCorners(m_RectTransformCorners);
			return m_RectTransformCorners;
		}

		protected override void SetActiveSubMeshes(bool state)
		{
			for (int i = 1; i < m_subTextObjects.Length && (UnityEngine.Object)(object)m_subTextObjects[i] != null; i++)
			{
				if (((Behaviour)(object)m_subTextObjects[i]).enabled != state)
				{
					((Behaviour)(object)m_subTextObjects[i]).enabled = state;
				}
			}
		}

		protected override Bounds GetCompoundBounds()
		{
			Bounds bounds = m_mesh.bounds;
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			for (int i = 1; i < m_subTextObjects.Length && (UnityEngine.Object)(object)m_subTextObjects[i] != null; i++)
			{
				Bounds bounds2 = m_subTextObjects[i].mesh.bounds;
				min.x = ((min.x < bounds2.min.x) ? min.x : bounds2.min.x);
				min.y = ((min.y < bounds2.min.y) ? min.y : bounds2.min.y);
				max.x = ((max.x > bounds2.max.x) ? max.x : bounds2.max.x);
				max.y = ((max.y > bounds2.max.y) ? max.y : bounds2.max.y);
			}
			Vector3 center = (min + max) / 2f;
			Vector2 v = max - min;
			return new Bounds(center, v);
		}

		private void UpdateSDFScale(float lossyScale)
		{
			if (m_canvas == null)
			{
				m_canvas = GetCanvas();
				if (m_canvas == null)
				{
					return;
				}
			}
			lossyScale = ((lossyScale == 0f) ? 1f : lossyScale);
			float num = 0f;
			float scaleFactor = m_canvas.scaleFactor;
			num = ((m_canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? (lossyScale / scaleFactor) : ((m_canvas.renderMode != RenderMode.ScreenSpaceCamera) ? lossyScale : ((m_canvas.worldCamera != null) ? lossyScale : 1f)));
			for (int i = 0; i < m_textInfo.characterCount; i++)
			{
				if (m_textInfo.characterInfo[i].isVisible && m_textInfo.characterInfo[i].elementType == TMP_TextElementType.Character)
				{
					float num2 = num * m_textInfo.characterInfo[i].scale * (1f - m_charWidthAdjDelta);
					if (!m_textInfo.characterInfo[i].isUsingAlternateTypeface && (m_textInfo.characterInfo[i].style & FontStyles.Bold) == FontStyles.Bold)
					{
						num2 *= -1f;
					}
					int materialReferenceIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
					int vertexIndex = m_textInfo.characterInfo[i].vertexIndex;
					m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexIndex].y = num2;
					m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexIndex + 1].y = num2;
					m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexIndex + 2].y = num2;
					m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexIndex + 3].y = num2;
				}
			}
			for (int j = 0; j < m_textInfo.materialCount; j++)
			{
				if (j == 0)
				{
					m_mesh.uv2 = m_textInfo.meshInfo[0].uvs2;
					m_canvasRenderer.SetMesh(m_mesh);
				}
				else
				{
					m_subTextObjects[j].mesh.uv2 = m_textInfo.meshInfo[j].uvs2;
					m_subTextObjects[j].canvasRenderer.SetMesh(m_subTextObjects[j].mesh);
				}
			}
		}

		protected override void AdjustLineOffset(int startIndex, int endIndex, float offset)
		{
			Vector3 vector = new Vector3(0f, offset, 0f);
			for (int i = startIndex; i <= endIndex; i++)
			{
				m_textInfo.characterInfo[i].bottomLeft -= vector;
				m_textInfo.characterInfo[i].topLeft -= vector;
				m_textInfo.characterInfo[i].topRight -= vector;
				m_textInfo.characterInfo[i].bottomRight -= vector;
				m_textInfo.characterInfo[i].ascender -= vector.y;
				m_textInfo.characterInfo[i].baseLine -= vector.y;
				m_textInfo.characterInfo[i].descender -= vector.y;
				if (m_textInfo.characterInfo[i].isVisible)
				{
					m_textInfo.characterInfo[i].vertex_BL.position -= vector;
					m_textInfo.characterInfo[i].vertex_TL.position -= vector;
					m_textInfo.characterInfo[i].vertex_TR.position -= vector;
					m_textInfo.characterInfo[i].vertex_BR.position -= vector;
				}
			}
		}

		public void CalculateLayoutInputHorizontal()
		{
			if (base.gameObject.activeInHierarchy && (m_isCalculateSizeRequired || m_rectTransform.hasChanged))
			{
				m_preferredWidth = GetPreferredWidth();
				ComputeMarginSize();
				m_isLayoutDirty = true;
			}
		}

		public void CalculateLayoutInputVertical()
		{
			if (base.gameObject.activeInHierarchy)
			{
				if (m_isCalculateSizeRequired || m_rectTransform.hasChanged)
				{
					m_preferredHeight = GetPreferredHeight();
					ComputeMarginSize();
					m_isLayoutDirty = true;
				}
				m_isCalculateSizeRequired = false;
			}
		}

		public override void SetVerticesDirty()
		{
			if (!m_verticesAlreadyDirty && !((UnityEngine.Object)(object)this == null) && ((UIBehaviour)this).IsActive() && !CanvasUpdateRegistry.IsRebuildingGraphics())
			{
				m_verticesAlreadyDirty = true;
				CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild((ICanvasElement)(object)this);
			}
		}

		public override void SetLayoutDirty()
		{
			m_isPreferredWidthDirty = true;
			m_isPreferredHeightDirty = true;
			if (!m_layoutAlreadyDirty && !((UnityEngine.Object)(object)this == null) && ((UIBehaviour)this).IsActive())
			{
				m_layoutAlreadyDirty = true;
				LayoutRebuilder.MarkLayoutForRebuild(base.rectTransform);
				m_isLayoutDirty = true;
			}
		}

		public override void SetMaterialDirty()
		{
			if (!((UnityEngine.Object)(object)this == null) && ((UIBehaviour)this).IsActive() && !CanvasUpdateRegistry.IsRebuildingGraphics())
			{
				m_isMaterialDirty = true;
				CanvasUpdateRegistry.RegisterCanvasElementForGraphicRebuild((ICanvasElement)(object)this);
			}
		}

		public override void SetAllDirty()
		{
			SetLayoutDirty();
			SetVerticesDirty();
			SetMaterialDirty();
		}

		public override void Rebuild(CanvasUpdate update)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Invalid comparison between Unknown and I4
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Invalid comparison between Unknown and I4
			if ((UnityEngine.Object)(object)this == null)
			{
				return;
			}
			if ((int)update == 0)
			{
				if (m_autoSizeTextContainer)
				{
					m_rectTransform.sizeDelta = GetPreferredValues(float.PositiveInfinity, float.PositiveInfinity);
				}
			}
			else if ((int)update == 3)
			{
				OnPreRenderCanvas();
				m_verticesAlreadyDirty = false;
				m_layoutAlreadyDirty = false;
				if (m_isMaterialDirty)
				{
					//((Graphic)this).UpdateMaterial();
					UpdateMaterial();
					m_isMaterialDirty = false;
				}
			}
		}

		private void UpdateSubObjectPivot()
		{
			if (m_textInfo != null)
			{
				for (int i = 1; i < m_subTextObjects.Length && (UnityEngine.Object)(object)m_subTextObjects[i] != null; i++)
				{
					m_subTextObjects[i].SetPivotDirty();
				}
			}
		}

		public override Material GetModifiedMaterial(Material baseMaterial)
		{
			Material material = baseMaterial;
			if (base.m_ShouldRecalculateStencil)
			{
				m_stencilID = TMP_MaterialManager.GetStencilID(base.gameObject);
				base.m_ShouldRecalculateStencil = false;
			}
			if (m_stencilID > 0)
			{
				material = TMP_MaterialManager.GetStencilMaterial(baseMaterial, m_stencilID);
				if (base.m_MaskMaterial != null)
				{
					TMP_MaterialManager.ReleaseStencilMaterial(base.m_MaskMaterial);
				}
				base.m_MaskMaterial = material;
			}
			return material;
		}

		protected override void UpdateMaterial()
		{
			if (!(m_sharedMaterial == null))
			{
				if (m_canvasRenderer == null)
				{
					m_canvasRenderer = canvasRenderer;
				}
				m_canvasRenderer.materialCount = 1;
				m_canvasRenderer.SetMaterial(((Graphic)this).materialForRendering, m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex));
			}
		}

		public override void RecalculateClipping()
		{
			//((MaskableGraphic)this).RecalculateClipping();
			base.RecalculateClipping();
		}

		public override void RecalculateMasking()
		{
			base.m_ShouldRecalculateStencil = true;
			SetMaterialDirty();
		}

		public override void Cull(Rect clipRect, bool validRect)
		{
			if (!m_ignoreRectMaskCulling)
			{
				//((MaskableGraphic)this).Cull(clipRect, validRect);
				base.Cull(clipRect, validRect);
			}
		}

		public override void UpdateMeshPadding()
		{
			m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
			m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
			m_havePropertiesChanged = true;
			checkPaddingRequired = false;
			if (m_textInfo != null)
			{
				for (int i = 1; i < m_textInfo.materialCount; i++)
				{
					m_subTextObjects[i].UpdateMeshPadding(m_enableExtraPadding, m_isUsingBold);
				}
			}
		}

		protected override void InternalCrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
		{
			int materialCount = m_textInfo.materialCount;
			for (int i = 1; i < materialCount; i++)
			{
				((Graphic)m_subTextObjects[i]).CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
			}
		}

		protected override void InternalCrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
		{
			int materialCount = m_textInfo.materialCount;
			for (int i = 1; i < materialCount; i++)
			{
				((Graphic)m_subTextObjects[i]).CrossFadeAlpha(alpha, duration, ignoreTimeScale);
			}
		}

		public override void ForceMeshUpdate()
		{
			m_havePropertiesChanged = true;
			OnPreRenderCanvas();
		}

		public override void ForceMeshUpdate(bool ignoreInactive)
		{
			m_havePropertiesChanged = true;
			m_ignoreActiveState = true;
			OnPreRenderCanvas();
		}

		public override TMP_TextInfo GetTextInfo(string text)
		{
			StringToCharArray(text, ref m_char_buffer);
			SetArraySizes(m_char_buffer);
			m_renderMode = TextRenderFlags.DontRender;
			ComputeMarginSize();
			if (m_canvas == null)
			{
				m_canvas = ((Graphic)this).canvas;
			}
			GenerateTextMesh();
			m_renderMode = TextRenderFlags.Render;
			return base.textInfo;
		}

		public override void ClearMesh()
		{
			m_canvasRenderer.SetMesh(null);
			for (int i = 1; i < m_subTextObjects.Length && (UnityEngine.Object)(object)m_subTextObjects[i] != null; i++)
			{
				m_subTextObjects[i].canvasRenderer.SetMesh(null);
			}
		}

		public override void UpdateGeometry(Mesh mesh, int index)
		{
			mesh.RecalculateBounds();
			if (index == 0)
			{
				m_canvasRenderer.SetMesh(mesh);
			}
			else
			{
				m_subTextObjects[index].canvasRenderer.SetMesh(mesh);
			}
		}

		public override void UpdateVertexData(TMP_VertexDataUpdateFlags flags)
		{
			int materialCount = m_textInfo.materialCount;
			for (int i = 0; i < materialCount; i++)
			{
				Mesh mesh = (i != 0) ? m_subTextObjects[i].mesh : m_mesh;
				if ((flags & TMP_VertexDataUpdateFlags.Vertices) == TMP_VertexDataUpdateFlags.Vertices)
				{
					mesh.vertices = m_textInfo.meshInfo[i].vertices;
				}
				if ((flags & TMP_VertexDataUpdateFlags.Uv0) == TMP_VertexDataUpdateFlags.Uv0)
				{
					mesh.uv = m_textInfo.meshInfo[i].uvs0;
				}
				if ((flags & TMP_VertexDataUpdateFlags.Uv2) == TMP_VertexDataUpdateFlags.Uv2)
				{
					mesh.uv2 = m_textInfo.meshInfo[i].uvs2;
				}
				if ((flags & TMP_VertexDataUpdateFlags.Colors32) == TMP_VertexDataUpdateFlags.Colors32)
				{
					mesh.colors32 = m_textInfo.meshInfo[i].colors32;
				}
				mesh.RecalculateBounds();
				if (i == 0)
				{
					m_canvasRenderer.SetMesh(mesh);
				}
				else
				{
					m_subTextObjects[i].canvasRenderer.SetMesh(mesh);
				}
			}
		}

		public override void UpdateVertexData()
		{
			int materialCount = m_textInfo.materialCount;
			for (int i = 0; i < materialCount; i++)
			{
				Mesh mesh;
				if (i == 0)
				{
					mesh = m_mesh;
				}
				else
				{
					m_textInfo.meshInfo[i].ClearUnusedVertices();
					mesh = m_subTextObjects[i].mesh;
				}
				mesh.vertices = m_textInfo.meshInfo[i].vertices;
				mesh.uv = m_textInfo.meshInfo[i].uvs0;
				mesh.uv2 = m_textInfo.meshInfo[i].uvs2;
				mesh.colors32 = m_textInfo.meshInfo[i].colors32;
				mesh.RecalculateBounds();
				if (i == 0)
				{
					m_canvasRenderer.SetMesh(mesh);
				}
				else
				{
					m_subTextObjects[i].canvasRenderer.SetMesh(mesh);
				}
			}
		}

		public void UpdateFontAsset()
		{
			LoadFontAsset();
		}
	}
}
