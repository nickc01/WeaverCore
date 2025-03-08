using System;
#if UNITY_EDITOR
//using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace TMPro
{
	[ExecuteInEditMode]
	[DisallowMultipleComponent]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	[AddComponentMenu("Mesh/TextMeshPro - Text")]
	[SelectionBase]
	public class TextMeshPro : TMP_Text, ILayoutElement
	{
		[SerializeField]
		private bool m_hasFontAssetChanged = false;

		private float m_previousLossyScaleY = -1f;

		[SerializeField]
		private Renderer m_renderer;

		private MeshFilter m_meshFilter;

		private bool m_isFirstAllocation;

		private int m_max_characters = 8;

		private int m_max_numberOfLines = 4;

		private Bounds m_default_bounds = new Bounds(Vector3.zero, new Vector3(1000f, 1000f, 0f));

		[SerializeField]
		protected TMP_SubMesh[] m_subTextObjects = new TMP_SubMesh[8];

		private bool m_isMaskingEnabled;

		private bool isMaskUpdateRequired;

		[SerializeField]
		private MaskingTypes m_maskType;

		private Matrix4x4 m_EnvMapMatrix = default(Matrix4x4);

		private Vector3[] m_RectTransformCorners = new Vector3[4];

		[NonSerialized]
		private bool m_isRegisteredForEvents;

		private int loopCountA;

		private bool m_currentAutoSizeMode;

		public int sortingLayerID
		{
			get
			{
				return m_renderer.sortingLayerID;
			}
			set
			{
				m_renderer.sortingLayerID = value;
			}
		}

		public int sortingOrder
		{
			get
			{
				return m_renderer.sortingOrder;
			}
			set
			{
				m_renderer.sortingOrder = value;
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
						TMP_UpdateManager.RegisterTextElementForLayoutRebuild(this);
						//SetLayoutDirty();
						//base.SetLayoutDirty();
						SetLayoutDirty();
					}
				}
			}
		}

		[Obsolete("The TextContainer is now obsolete. Use the RectTransform instead.")]
		public TextContainer textContainer
		{
			get
			{
				return null;
			}
		}

		public new Transform transform
		{
			get
			{
				if (m_transform == null)
				{
					m_transform = base.GetComponent<Transform>();
				}
				return m_transform;
			}
		}

		public Renderer renderer
		{
			get
			{
				if (m_renderer == null)
				{
					m_renderer = base.GetComponent<Renderer>();
				}
				return m_renderer;
			}
		}

		public override Mesh mesh
		{
			get
			{
				if (m_mesh == null)
				{
					m_mesh = new Mesh();
					m_mesh.hideFlags = HideFlags.HideAndDontSave;
					meshFilter.mesh = m_mesh;
				}
				return m_mesh;
			}
		}

		public MeshFilter meshFilter
		{
			get
			{
				if (m_meshFilter == null)
				{
					m_meshFilter = base.GetComponent<MeshFilter>();
				}
				return m_meshFilter;
			}
		}

		public MaskingTypes maskType
		{
			get
			{
				return m_maskType;
			}
			set
			{
				m_maskType = value;
				SetMask(m_maskType);
			}
		}

		protected override void Awake()
		{
			m_renderer = base.GetComponent<Renderer>();
			if (m_renderer == null)
			{
				m_renderer = base.gameObject.AddComponent<Renderer>();
			}
			if (((Graphic)this).canvasRenderer != null)
			{
				((Graphic)this).canvasRenderer.hideFlags = HideFlags.HideInInspector;
			}
			else
			{

				CanvasRenderer canvasRenderer = GetComponent<CanvasRenderer>();
				if (canvasRenderer == null)
				{
					canvasRenderer = gameObject.AddComponent<CanvasRenderer>();
				}

				canvasRenderer.hideFlags = HideFlags.HideInInspector;
			}
			m_rectTransform = base.rectTransform;
			m_transform = transform;
			m_meshFilter = base.GetComponent<MeshFilter>();
			if (m_meshFilter == null)
			{
				m_meshFilter = base.gameObject.AddComponent<MeshFilter>();
			}
			if (m_mesh == null)
			{
				m_mesh = new Mesh();
				m_mesh.hideFlags = HideFlags.HideAndDontSave;
				m_meshFilter.mesh = m_mesh;
			}
			m_meshFilter.hideFlags = HideFlags.HideInInspector;
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
				Debug.LogWarning("Please assign a Font Asset to this " + transform.name + " gameobject.", (UnityEngine.Object)(object)this);
				return;
			}
			TMP_SubMesh[] componentsInChildren = base.GetComponentsInChildren<TMP_SubMesh>();
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
				TMPro_EventManager.TEXTMESHPRO_PROPERTY_EVENT.Add(ON_TEXTMESHPRO_PROPERTY_CHANGED);
				TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Add(ON_DRAG_AND_DROP_MATERIAL);
				TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Add(ON_TEXT_STYLE_CHANGED);
				TMPro_EventManager.COLOR_GRADIENT_PROPERTY_EVENT.Add(ON_COLOR_GRADIENT_CHANGED);
				TMPro_EventManager.TMP_SETTINGS_PROPERTY_EVENT.Add(ON_TMP_SETTINGS_CHANGED);
				m_isRegisteredForEvents = true;
			}
			meshFilter.sharedMesh = mesh;
			SetActiveSubMeshes(true);
			ComputeMarginSize();
			m_isInputParsingRequired = true;
			m_havePropertiesChanged = true;
			m_verticesAlreadyDirty = false;
			//SetVerticesDirty();
			//base.SetVerticesDirty();
			SetVerticesDirty();
		}

		protected override void OnDisable()
		{
			TMP_UpdateManager.UnRegisterTextElementForRebuild(this);
			m_meshFilter.sharedMesh = null;
			SetActiveSubMeshes(false);
		}

		protected override void OnDestroy()
		{
			if (m_mesh != null)
			{
				UnityEngine.Object.DestroyImmediate(m_mesh);
			}
			TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Remove(ON_MATERIAL_PROPERTY_CHANGED);
			TMPro_EventManager.FONT_PROPERTY_EVENT.Remove(ON_FONT_PROPERTY_CHANGED);
			TMPro_EventManager.TEXTMESHPRO_PROPERTY_EVENT.Remove(ON_TEXTMESHPRO_PROPERTY_CHANGED);
			TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Remove(ON_DRAG_AND_DROP_MATERIAL);
			TMPro_EventManager.TEXT_STYLE_PROPERTY_EVENT.Remove(ON_TEXT_STYLE_CHANGED);
			TMPro_EventManager.COLOR_GRADIENT_PROPERTY_EVENT.Remove(ON_COLOR_GRADIENT_CHANGED);
			TMPro_EventManager.TMP_SETTINGS_PROPERTY_EVENT.Remove(ON_TMP_SETTINGS_CHANGED);
			m_isRegisteredForEvents = false;
			TMP_UpdateManager.UnRegisterTextElementForRebuild(this);
		}
#if UNITY_EDITOR
		protected override void Reset()
		{
			if (m_mesh != null)
			{
				UnityEngine.Object.DestroyImmediate(m_mesh);
			}
			//((UIBehaviour)this).Awake();
			//base.Awake();
			Awake();
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
			m_padding = GetPaddingForMaterial();
			m_isInputParsingRequired = true;
			m_inputSource = TextInputSources.Text;
			m_havePropertiesChanged = true;
			m_isCalculateSizeRequired = true;
			m_isPreferredWidthDirty = true;
			m_isPreferredHeightDirty = true;
			//SetAllDirty();
			SetAllDirty();
		}
#endif

		private void ON_MATERIAL_PROPERTY_CHANGED(bool isChanged, Material mat)
		{
			if (m_renderer.sharedMaterial == null)
			{
				if (m_fontAsset != null)
				{
					m_renderer.sharedMaterial = m_fontAsset.material;
					Debug.LogWarning("No Material was assigned to " + base.name + ". " + m_fontAsset.material.name + " was assigned.", (UnityEngine.Object)(object)this);
				}
				else
				{
					Debug.LogWarning("No Font Asset assigned to " + base.name + ". Please assign a Font Asset.", (UnityEngine.Object)(object)this);
				}
			}
			if (m_fontAsset.atlas.GetInstanceID() != m_renderer.sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
			{
				m_renderer.sharedMaterial = m_sharedMaterial;
				Debug.LogWarning("Font Asset Atlas doesn't match the Atlas in the newly assigned material. Select a matching material or a different font asset.", (UnityEngine.Object)(object)this);
			}
			if (m_renderer.sharedMaterial != m_sharedMaterial)
			{
				m_sharedMaterial = m_renderer.sharedMaterial;
			}
			m_padding = GetPaddingForMaterial();
			UpdateMask();
			UpdateEnvMapMatrix();
			m_havePropertiesChanged = true;
			//SetVerticesDirty();
			SetVerticesDirty();
		}

		private void ON_FONT_PROPERTY_CHANGED(bool isChanged, TMP_FontAsset font)
		{
			if (MaterialReference.Contains(m_materialReferences, font))
			{
				m_isInputParsingRequired = true;
				m_havePropertiesChanged = true;
				//SetMaterialDirty();
				//SetVerticesDirty();
				SetMaterialDirty();
				SetVerticesDirty();
			}
		}

		private void ON_TEXTMESHPRO_PROPERTY_CHANGED(bool isChanged, TextMeshPro obj)
		{
			if ((UnityEngine.Object)(object)obj == (UnityEngine.Object)(object)this)
			{
				m_havePropertiesChanged = true;
				m_isInputParsingRequired = true;
				m_padding = GetPaddingForMaterial();
				//SetVerticesDirty();
				SetVerticesDirty();
			}
		}

		private void ON_DRAG_AND_DROP_MATERIAL(GameObject obj, Material currentMaterial, Material newMaterial)
		{
#if UNITY_EDITOR
			if (obj == base.gameObject/* || PrefabUtility.GetPrefabParent(base.gameObject) == obj*/)
			{
				//Undo.RecordObject((UnityEngine.Object)(object)this, "Material Assignment");
				//Undo.RecordObject(m_renderer, "Material Assignment");
				m_sharedMaterial = newMaterial;
				m_padding = GetPaddingForMaterial();
				m_havePropertiesChanged = true;
				//SetVerticesDirty();
				//SetMaterialDirty();
				SetVerticesDirty();
				SetMaterialDirty();
			}
#endif
		}

		private void ON_TEXT_STYLE_CHANGED(bool isChanged)
		{
			m_havePropertiesChanged = true;
			m_isInputParsingRequired = true;
			//SetVerticesDirty();
			SetVerticesDirty();
		}

		private void ON_COLOR_GRADIENT_CHANGED(TMP_ColorGradient gradient)
		{
			if (m_fontColorGradientPreset != null && gradient.GetInstanceID() == m_fontColorGradientPreset.GetInstanceID())
			{
				m_havePropertiesChanged = true;
				//SetVerticesDirty();
				SetVerticesDirty();
			}
		}

		private void ON_TMP_SETTINGS_CHANGED()
		{
			m_defaultSpriteAsset = null;
			m_havePropertiesChanged = true;
			m_isInputParsingRequired = true;
			//SetAllDirty();
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
				m_renderer.sharedMaterial = m_fontAsset.material;
				m_sharedMaterial = m_fontAsset.material;
				m_sharedMaterial.SetFloat("_CullMode", 0f);
				m_sharedMaterial.SetFloat(ShaderUtilities.ShaderTag_ZTestMode, 4f);
				m_renderer.receiveShadows = false;
				m_renderer.shadowCastingMode = ShadowCastingMode.Off;
			}
			else
			{
				if (m_fontAsset.characterDictionary == null)
				{
					m_fontAsset.ReadFontDefinition();
				}
				if (m_renderer.sharedMaterial == null || m_renderer.sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex) == null || m_fontAsset.atlas.GetInstanceID() != m_renderer.sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
				{
					m_renderer.sharedMaterial = m_fontAsset.material;
					m_sharedMaterial = m_fontAsset.material;
				}
				else
				{
					m_sharedMaterial = m_renderer.sharedMaterial;
				}
				m_sharedMaterial.SetFloat(ShaderUtilities.ShaderTag_ZTestMode, 4f);
				if (m_sharedMaterial.passCount == 1)
				{
					m_renderer.receiveShadows = false;
					m_renderer.shadowCastingMode = ShadowCastingMode.Off;
				}
			}
			m_padding = GetPaddingForMaterial();
			m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
			GetSpecialCharacters(m_fontAsset);
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

		private void SetMask(MaskingTypes maskType)
		{
			switch (maskType)
			{
			case MaskingTypes.MaskOff:
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);
				break;
			case MaskingTypes.MaskSoft:
				m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);
				break;
			case MaskingTypes.MaskHard:
				m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_HARD);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);
				break;
			}
		}

		private void SetMaskCoordinates(Vector4 coords)
		{
			m_sharedMaterial.SetVector(ShaderUtilities.ID_ClipRect, coords);
		}

		private void SetMaskCoordinates(Vector4 coords, float softX, float softY)
		{
			m_sharedMaterial.SetVector(ShaderUtilities.ID_ClipRect, coords);
			m_sharedMaterial.SetFloat(ShaderUtilities.ID_MaskSoftnessX, softX);
			m_sharedMaterial.SetFloat(ShaderUtilities.ID_MaskSoftnessY, softY);
		}

		private void EnableMasking()
		{
			if (m_sharedMaterial.HasProperty(ShaderUtilities.ID_ClipRect))
			{
				m_sharedMaterial.EnableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);
				m_isMaskingEnabled = true;
				UpdateMask();
			}
		}

		private void DisableMasking()
		{
			if (m_sharedMaterial.HasProperty(ShaderUtilities.ID_ClipRect))
			{
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_SOFT);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_HARD);
				m_sharedMaterial.DisableKeyword(ShaderUtilities.Keyword_MASK_TEX);
				m_isMaskingEnabled = false;
				UpdateMask();
			}
		}

		private void UpdateMask()
		{
			if (m_isMaskingEnabled && m_isMaskingEnabled && m_fontMaterial == null)
			{
				CreateMaterialInstance();
			}
		}

		protected override Material GetMaterial(Material mat)
		{
			if (m_fontMaterial == null || m_fontMaterial.GetInstanceID() != mat.GetInstanceID())
			{
				m_fontMaterial = CreateMaterialInstance(mat);
			}
			m_sharedMaterial = m_fontMaterial;
			m_padding = GetPaddingForMaterial();
			//SetVerticesDirty();
			//SetMaterialDirty();
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
					m_fontMaterials[i] = m_subTextObjects[i].material;
				}
			}
			m_fontSharedMaterials = m_fontMaterials;
			return m_fontMaterials;
		}

		protected override void SetSharedMaterial(Material mat)
		{
			m_sharedMaterial = mat;
			m_padding = GetPaddingForMaterial();
			//SetMaterialDirty();
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
				Texture texture = materials[i].GetTexture(ShaderUtilities.ID_MainTex);
				if (i == 0)
				{
					if (!(texture == null) && texture.GetInstanceID() == m_sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
					{
						m_sharedMaterial = (m_fontSharedMaterials[i] = materials[i]);
						m_padding = GetPaddingForMaterial(m_sharedMaterial);
					}
				}
				else if (!(texture == null) && texture.GetInstanceID() == m_subTextObjects[i].sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() && m_subTextObjects[i].isDefaultMaterial)
				{
					m_subTextObjects[i].sharedMaterial = (m_fontSharedMaterials[i] = materials[i]);
				}
			}
		}

		protected override void SetOutlineThickness(float thickness)
		{
			thickness = Mathf.Clamp01(thickness);
			m_renderer.material.SetFloat(ShaderUtilities.ID_OutlineWidth, thickness);
			if (m_fontMaterial == null)
			{
				m_fontMaterial = m_renderer.material;
			}
			m_fontMaterial = m_renderer.material;
			m_sharedMaterial = m_fontMaterial;
			m_padding = GetPaddingForMaterial();
		}

		protected override void SetFaceColor(Color32 color)
		{
			m_renderer.material.SetColor(ShaderUtilities.ID_FaceColor, color);
			if (m_fontMaterial == null)
			{
				m_fontMaterial = m_renderer.material;
			}
			m_sharedMaterial = m_fontMaterial;
		}

		protected override void SetOutlineColor(Color32 color)
		{
			m_renderer.material.SetColor(ShaderUtilities.ID_OutlineColor, color);
			if (m_fontMaterial == null)
			{
				m_fontMaterial = m_renderer.material;
			}
			m_sharedMaterial = m_fontMaterial;
		}

		private void CreateMaterialInstance()
		{
			Material material = new Material(m_sharedMaterial);
			material.shaderKeywords = m_sharedMaterial.shaderKeywords;
			material.name += " Instance";
			m_fontMaterial = material;
		}

		protected override void SetShaderDepth()
		{
			if (m_isOverlay)
			{
				m_sharedMaterial.SetFloat(ShaderUtilities.ShaderTag_ZTestMode, 0f);
				m_renderer.material.renderQueue = 4000;
				m_sharedMaterial = m_renderer.material;
			}
			else
			{
				m_sharedMaterial.SetFloat(ShaderUtilities.ShaderTag_ZTestMode, 4f);
				m_renderer.material.renderQueue = -1;
				m_sharedMaterial = m_renderer.material;
			}
		}

		protected override void SetCulling()
		{
			if (m_isCullingEnabled)
			{
				m_renderer.material.SetFloat("_CullMode", 2f);
			}
			else
			{
				m_renderer.material.SetFloat("_CullMode", 0f);
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
			if (m_sharedMaterial == null)
			{
				return 0f;
			}
			m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, m_enableExtraPadding, m_isUsingBold);
			m_isMaskingEnabled = ShaderUtilities.IsMaskingEnabled(m_sharedMaterial);
			m_isSDFShader = m_sharedMaterial.HasProperty(ShaderUtilities.ID_WeightNormal);
			return m_padding;
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
					if (m_subTextObjects[j] == null)
					{
						m_subTextObjects[j] = TMP_SubMesh.AddSubTextObject(this, m_materialReferences[j]);
						m_textInfo.meshInfo[j].vertices = null;
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
				if (m_textInfo.meshInfo[j].vertices == null || m_textInfo.meshInfo[j].vertices.Length < referenceCount * ((!m_isVolumetricText) ? 4 : 8))
				{
					if (m_textInfo.meshInfo[j].vertices == null)
					{
						if (j == 0)
						{
							m_textInfo.meshInfo[j] = new TMP_MeshInfo(m_mesh, referenceCount + 1, m_isVolumetricText);
						}
						else
						{
							m_textInfo.meshInfo[j] = new TMP_MeshInfo(m_subTextObjects[j].mesh, referenceCount + 1, m_isVolumetricText);
						}
					}
					else
					{
						m_textInfo.meshInfo[j].ResizeMeshInfo((referenceCount > 1024) ? (referenceCount + 256) : Mathf.NextPowerOfTwo(referenceCount), m_isVolumetricText);
					}
				}
				else if (m_textInfo.meshInfo[j].vertices.Length - referenceCount * ((!m_isVolumetricText) ? 4 : 8) > 1024)
				{
					m_textInfo.meshInfo[j].ResizeMeshInfo((referenceCount > 1024) ? (referenceCount + 256) : Mathf.Max(Mathf.NextPowerOfTwo(referenceCount), 256), m_isVolumetricText);
				}
			}
			for (int k = num3; k < m_subTextObjects.Length && m_subTextObjects[k] != null; k++)
			{
				if (k < m_textInfo.meshInfo.Length)
				{
					m_textInfo.meshInfo[k].ClearUnusedVertices(0, true);
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
			isMaskUpdateRequired = true;
			//SetVerticesDirty();
			SetMaterialDirty();
		}

		protected override void OnTransformParentChanged()
		{
			//SetVerticesDirty();
			//SetLayoutDirty();
			SetVerticesDirty();
			SetLayoutDirty();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			ComputeMarginSize();
			//SetVerticesDirty();
			//SetLayoutDirty();
			SetVerticesDirty();
			SetLayoutDirty();
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
			}
			if (m_isUsingLegacyAnimationComponent)
			{
				m_havePropertiesChanged = true;
				OnPreRenderObject();
			}
		}

		private void OnPreRenderObject()
		{
			if (!m_isAwake || (!m_ignoreActiveState && !((UIBehaviour)this).IsActive()))
			{
				return;
			}
			loopCountA = 0;
			if (m_havePropertiesChanged || m_isLayoutDirty)
			{
				if (isMaskUpdateRequired)
				{
					UpdateMask();
					isMaskUpdateRequired = false;
				}
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

		// TMPro.TextMeshPro
		// Token: 0x060003F5 RID: 1013 RVA: 0x0002278C File Offset: 0x0002098C
		protected override void GenerateTextMesh()
		{
			bool flag = this.m_fontAsset == null || this.m_fontAsset.characterDictionary == null;
			if (flag)
			{
				Debug.LogWarning("Can't Generate Mesh! No Font Asset has been assigned to Object ID: " + base.GetInstanceID());
			}
			else
			{
				bool flag2 = this.m_textInfo != null;
				if (flag2)
				{
					this.m_textInfo.Clear();
				}
				bool flag3 = this.m_char_buffer == null || this.m_char_buffer.Length == 0 || this.m_char_buffer[0] == 0;
				if (flag3)
				{
					this.ClearMesh(true);
					this.m_preferredWidth = 0f;
					this.m_preferredHeight = 0f;
					TMPro_EventManager.ON_TEXT_CHANGED(this);
				}
				else
				{
					this.m_currentFontAsset = this.m_fontAsset;
					this.m_currentMaterial = this.m_sharedMaterial;
					this.m_currentMaterialIndex = 0;
					this.m_materialReferenceStack.SetDefault(new MaterialReference(this.m_currentMaterialIndex, this.m_currentFontAsset, null, this.m_currentMaterial, this.m_padding));
					this.m_currentSpriteAsset = this.m_spriteAsset;
					bool flag4 = this.m_spriteAnimator != null;
					if (flag4)
					{
						this.m_spriteAnimator.StopAllAnimations();
					}
					int totalCharacterCount = this.m_totalCharacterCount;
					this.m_fontScale = this.m_fontSize / this.m_currentFontAsset.fontInfo.PointSize * (this.m_isOrthographic ? 1f : 0.1f);
					float num = this.m_fontSize / this.m_fontAsset.fontInfo.PointSize * this.m_fontAsset.fontInfo.Scale * (this.m_isOrthographic ? 1f : 0.1f);
					float num2 = this.m_fontScale;
					this.m_fontScaleMultiplier = 1f;
					this.m_currentFontSize = this.m_fontSize;
					this.m_sizeStack.SetDefault(this.m_currentFontSize);
					this.m_style = this.m_fontStyle;
					this.m_fontWeightInternal = (((this.m_style & FontStyles.Bold) == FontStyles.Bold) ? 700 : this.m_fontWeight);
					this.m_fontWeightStack.SetDefault(this.m_fontWeightInternal);
					this.m_fontStyleStack.Clear();
					this.m_lineJustification = this.m_textAlignment;
					this.m_lineJustificationStack.SetDefault(this.m_lineJustification);
					float num3 = 0f;
					float num4 = 1f;
					this.m_baselineOffset = 0f;
					bool flag5 = false;
					Vector3 zero = Vector3.zero;
					Vector3 zero2 = Vector3.zero;
					bool flag6 = false;
					Vector3 zero3 = Vector3.zero;
					Vector3 zero4 = Vector3.zero;
					bool flag7 = false;
					Vector3 vector = Vector3.zero;
					Vector3 vector2 = Vector3.zero;
					this.m_fontColor32 = this.m_fontColor;
					this.m_htmlColor = this.m_fontColor32;
					this.m_underlineColor = this.m_htmlColor;
					this.m_strikethroughColor = this.m_htmlColor;
					this.m_colorStack.SetDefault(this.m_htmlColor);
					this.m_underlineColorStack.SetDefault(this.m_htmlColor);
					this.m_strikethroughColorStack.SetDefault(this.m_htmlColor);
					this.m_highlightColorStack.SetDefault(this.m_htmlColor);
					this.m_actionStack.Clear();
					this.m_isFXMatrixSet = false;
					this.m_lineOffset = 0f;
					this.m_lineHeight = -32767f;
					float num5 = this.m_currentFontAsset.fontInfo.LineHeight - (this.m_currentFontAsset.fontInfo.Ascender - this.m_currentFontAsset.fontInfo.Descender);
					this.m_cSpacing = 0f;
					this.m_monoSpacing = 0f;
					this.m_xAdvance = 0f;
					this.tag_LineIndent = 0f;
					this.tag_Indent = 0f;
					this.m_indentStack.SetDefault(0f);
					this.tag_NoParsing = false;
					this.m_characterCount = 0;
					this.m_firstCharacterOfLine = 0;
					this.m_lastCharacterOfLine = 0;
					this.m_firstVisibleCharacterOfLine = 0;
					this.m_lastVisibleCharacterOfLine = 0;
					this.m_maxLineAscender = TMP_Text.k_LargeNegativeFloat;
					this.m_maxLineDescender = TMP_Text.k_LargePositiveFloat;
					this.m_lineNumber = 0;
					this.m_lineVisibleCharacterCount = 0;
					bool flag8 = true;
					this.m_firstOverflowCharacterIndex = -1;
					this.m_pageNumber = 0;
					int num6 = Mathf.Clamp(this.m_pageToDisplay - 1, 0, this.m_textInfo.pageInfo.Length - 1);
					int num7 = 0;
					int num8 = 0;
					Vector4 margin = this.m_margin;
					float marginWidth = this.m_marginWidth;
					float marginHeight = this.m_marginHeight;
					this.m_marginLeft = 0f;
					this.m_marginRight = 0f;
					this.m_width = -1f;
					float num9 = marginWidth + 0.0001f - this.m_marginLeft - this.m_marginRight;
					this.m_meshExtents.min = TMP_Text.k_LargePositiveVector2;
					this.m_meshExtents.max = TMP_Text.k_LargeNegativeVector2;
					this.m_textInfo.ClearLineInfo();
					this.m_maxCapHeight = 0f;
					this.m_maxAscender = 0f;
					this.m_maxDescender = 0f;
					float num10 = 0f;
					float num11 = 0f;
					bool flag9 = false;
					this.m_isNewPage = false;
					bool flag10 = true;
					this.m_isNonBreakingSpace = false;
					bool flag11 = false;
					bool flag12 = false;
					int num12 = 0;
					base.SaveWordWrappingState(ref this.m_SavedWordWrapState, -1, -1);
					base.SaveWordWrappingState(ref this.m_SavedLineState, -1, -1);
					this.loopCountA++;
					int num13 = 0;
					int num14 = 0;
					while (num14 < this.m_char_buffer.Length && this.m_char_buffer[num14] != 0)
					{
						int num15 = this.m_char_buffer[num14];
						this.m_textElementType = this.m_textInfo.characterInfo[this.m_characterCount].elementType;
						this.m_currentMaterialIndex = this.m_textInfo.characterInfo[this.m_characterCount].materialReferenceIndex;
						this.m_currentFontAsset = this.m_materialReferences[this.m_currentMaterialIndex].fontAsset;
						int currentMaterialIndex = this.m_currentMaterialIndex;
						bool flag13 = this.m_isRichText && num15 == 60;
						if (flag13)
						{
							this.m_isParsingText = true;
							this.m_textElementType = TMP_TextElementType.Character;
							bool flag14 = base.ValidateHtmlTag(this.m_char_buffer, num14 + 1, out num13);
							if (flag14)
							{
								num14 = num13;
								bool flag15 = this.m_textElementType == TMP_TextElementType.Character;
								if (flag15)
								{
									goto IL_3400;
								}
							}
							goto IL_606;
						}
						goto IL_606;
					IL_3400:
						num14++;
						continue;
					IL_606:
						this.m_isParsingText = false;
						bool isUsingAlternateTypeface = this.m_textInfo.characterInfo[this.m_characterCount].isUsingAlternateTypeface;
						bool flag16 = this.m_characterCount < this.m_firstVisibleCharacter;
						if (flag16)
						{
							this.m_textInfo.characterInfo[this.m_characterCount].isVisible = false;
							this.m_textInfo.characterInfo[this.m_characterCount].character = '​';
							this.m_characterCount++;
							goto IL_3400;
						}
						float num16 = 1f;
						bool flag17 = this.m_textElementType == TMP_TextElementType.Character;
						if (flag17)
						{
							bool flag18 = (this.m_style & FontStyles.UpperCase) == FontStyles.UpperCase;
							if (flag18)
							{
								bool flag19 = char.IsLower((char)num15);
								if (flag19)
								{
									num15 = (int)char.ToUpper((char)num15);
								}
							}
							else
							{
								bool flag20 = (this.m_style & FontStyles.LowerCase) == FontStyles.LowerCase;
								if (flag20)
								{
									bool flag21 = char.IsUpper((char)num15);
									if (flag21)
									{
										num15 = (int)char.ToLower((char)num15);
									}
								}
								else
								{
									bool flag22 = (this.m_fontStyle & FontStyles.SmallCaps) == FontStyles.SmallCaps || (this.m_style & FontStyles.SmallCaps) == FontStyles.SmallCaps;
									if (flag22)
									{
										bool flag23 = char.IsLower((char)num15);
										if (flag23)
										{
											num16 = 0.8f;
											num15 = (int)char.ToUpper((char)num15);
										}
									}
								}
							}
						}
						bool flag24 = this.m_textElementType == TMP_TextElementType.Sprite;
						if (flag24)
						{
							this.m_currentSpriteAsset = this.m_textInfo.characterInfo[this.m_characterCount].spriteAsset;
							this.m_spriteIndex = this.m_textInfo.characterInfo[this.m_characterCount].spriteIndex;
							TMP_Sprite tmp_Sprite = this.m_currentSpriteAsset.spriteInfoList[this.m_spriteIndex];
							bool flag25 = tmp_Sprite == null;
							if (flag25)
							{
								goto IL_3400;
							}
							bool flag26 = num15 == 60;
							if (flag26)
							{
								num15 = 57344 + this.m_spriteIndex;
							}
							else
							{
								this.m_spriteColor = TMP_Text.s_colorWhite;
							}
							this.m_currentFontAsset = this.m_fontAsset;
							float num17 = this.m_currentFontSize / this.m_fontAsset.fontInfo.PointSize * this.m_fontAsset.fontInfo.Scale * (this.m_isOrthographic ? 1f : 0.1f);
							num2 = this.m_fontAsset.fontInfo.Ascender / tmp_Sprite.height * tmp_Sprite.scale * num17;
							this.m_cached_TextElement = tmp_Sprite;
							this.m_textInfo.characterInfo[this.m_characterCount].elementType = TMP_TextElementType.Sprite;
							this.m_textInfo.characterInfo[this.m_characterCount].scale = num17;
							this.m_textInfo.characterInfo[this.m_characterCount].spriteAsset = this.m_currentSpriteAsset;
							this.m_textInfo.characterInfo[this.m_characterCount].fontAsset = this.m_currentFontAsset;
							this.m_textInfo.characterInfo[this.m_characterCount].materialReferenceIndex = this.m_currentMaterialIndex;
							this.m_currentMaterialIndex = currentMaterialIndex;
							num3 = 0f;
						}
						else
						{
							bool flag27 = this.m_textElementType == TMP_TextElementType.Character;
							if (flag27)
							{
								this.m_cached_TextElement = this.m_textInfo.characterInfo[this.m_characterCount].textElement;
								bool flag28 = this.m_cached_TextElement == null;
								if (flag28)
								{
									goto IL_3400;
								}
								this.m_currentFontAsset = this.m_textInfo.characterInfo[this.m_characterCount].fontAsset;
								this.m_currentMaterial = this.m_textInfo.characterInfo[this.m_characterCount].material;
								this.m_currentMaterialIndex = this.m_textInfo.characterInfo[this.m_characterCount].materialReferenceIndex;
								this.m_fontScale = this.m_currentFontSize * num16 / this.m_currentFontAsset.fontInfo.PointSize * this.m_currentFontAsset.fontInfo.Scale * (this.m_isOrthographic ? 1f : 0.1f);
								num2 = this.m_fontScale * this.m_fontScaleMultiplier * this.m_cached_TextElement.scale;
								this.m_textInfo.characterInfo[this.m_characterCount].elementType = TMP_TextElementType.Character;
								this.m_textInfo.characterInfo[this.m_characterCount].scale = num2;
								num3 = ((this.m_currentMaterialIndex == 0) ? this.m_padding : this.m_subTextObjects[this.m_currentMaterialIndex].padding);
							}
						}
						float num18 = num2;
						bool flag29 = num15 == 173;
						if (flag29)
						{
							num2 = 0f;
						}
						bool isRightToLeft = this.m_isRightToLeft;
						if (isRightToLeft)
						{
							this.m_xAdvance -= ((this.m_cached_TextElement.xAdvance * num4 + this.m_characterSpacing + this.m_wordSpacing + this.m_currentFontAsset.normalSpacingOffset) * num2 + this.m_cSpacing) * (1f - this.m_charWidthAdjDelta);
							bool flag30 = char.IsWhiteSpace((char)num15) || num15 == 8203;
							if (flag30)
							{
								this.m_xAdvance -= this.m_wordSpacing * num2;
							}
						}
						this.m_textInfo.characterInfo[this.m_characterCount].character = (char)num15;
						this.m_textInfo.characterInfo[this.m_characterCount].pointSize = this.m_currentFontSize;
						this.m_textInfo.characterInfo[this.m_characterCount].color = this.m_htmlColor;
						this.m_textInfo.characterInfo[this.m_characterCount].underlineColor = this.m_underlineColor;
						this.m_textInfo.characterInfo[this.m_characterCount].strikethroughColor = this.m_strikethroughColor;
						this.m_textInfo.characterInfo[this.m_characterCount].highlightColor = this.m_highlightColor;
						this.m_textInfo.characterInfo[this.m_characterCount].style = this.m_style;
						this.m_textInfo.characterInfo[this.m_characterCount].index = (short)num14;
						bool flag31 = this.m_enableKerning && this.m_characterCount >= 1;
						if (flag31)
						{
							int character = (int)this.m_textInfo.characterInfo[this.m_characterCount - 1].character;
							KerningPairKey kerningPairKey = new KerningPairKey(character, num15);
							KerningPair kerningPair;
							this.m_currentFontAsset.kerningDictionary.TryGetValue(kerningPairKey.key, out kerningPair);
							bool flag32 = kerningPair != null;
							if (flag32)
							{
								this.m_xAdvance += kerningPair.XadvanceOffset * num2;
							}
						}
						float num19 = 0f;
						bool flag33 = this.m_monoSpacing != 0f;
						if (flag33)
						{
							num19 = (this.m_monoSpacing / 2f - (this.m_cached_TextElement.width / 2f + this.m_cached_TextElement.xOffset) * num2) * (1f - this.m_charWidthAdjDelta);
							this.m_xAdvance += num19;
						}
						bool flag34 = this.m_textElementType == TMP_TextElementType.Character && !isUsingAlternateTypeface && ((this.m_style & FontStyles.Bold) == FontStyles.Bold || (this.m_fontStyle & FontStyles.Bold) == FontStyles.Bold);
						float num20;
						if (flag34)
						{
							bool flag35 = this.m_currentMaterial.HasProperty(ShaderUtilities.ID_GradientScale);
							if (flag35)
							{
								float @float = this.m_currentMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
								num20 = this.m_currentFontAsset.boldStyle / 4f * @float * this.m_currentMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);
								bool flag36 = num20 + num3 > @float;
								if (flag36)
								{
									num3 = @float - num20;
								}
							}
							else
							{
								num20 = 0f;
							}
							num4 = 1f + this.m_currentFontAsset.boldSpacing * 0.01f;
						}
						else
						{
							bool flag37 = this.m_currentMaterial.HasProperty(ShaderUtilities.ID_GradientScale);
							if (flag37)
							{
								float float2 = this.m_currentMaterial.GetFloat(ShaderUtilities.ID_GradientScale);
								num20 = this.m_currentFontAsset.normalStyle / 4f * float2 * this.m_currentMaterial.GetFloat(ShaderUtilities.ID_ScaleRatio_A);
								bool flag38 = num20 + num3 > float2;
								if (flag38)
								{
									num3 = float2 - num20;
								}
							}
							else
							{
								num20 = 0f;
							}
							num4 = 1f;
						}
						float baseline = this.m_currentFontAsset.fontInfo.Baseline;
						Vector3 vector3;
						vector3.x = this.m_xAdvance + (this.m_cached_TextElement.xOffset - num3 - num20) * num2 * (1f - this.m_charWidthAdjDelta);
						vector3.y = (baseline + this.m_cached_TextElement.yOffset + num3) * num2 - this.m_lineOffset + this.m_baselineOffset;
						vector3.z = 0f;
						Vector3 vector4;
						vector4.x = vector3.x;
						vector4.y = vector3.y - (this.m_cached_TextElement.height + num3 * 2f) * num2;
						vector4.z = 0f;
						Vector3 vector5;
						vector5.x = vector4.x + (this.m_cached_TextElement.width + num3 * 2f + num20 * 2f) * num2 * (1f - this.m_charWidthAdjDelta);
						vector5.y = vector3.y;
						vector5.z = 0f;
						Vector3 vector6;
						vector6.x = vector5.x;
						vector6.y = vector4.y;
						vector6.z = 0f;
						bool flag39 = this.m_textElementType == TMP_TextElementType.Character && !isUsingAlternateTypeface && ((this.m_style & FontStyles.Italic) == FontStyles.Italic || (this.m_fontStyle & FontStyles.Italic) == FontStyles.Italic);
						if (flag39)
						{
							float num21 = (float)this.m_currentFontAsset.italicStyle * 0.01f;
							Vector3 b = new Vector3(num21 * ((this.m_cached_TextElement.yOffset + num3 + num20) * num2), 0f, 0f);
							Vector3 b2 = new Vector3(num21 * ((this.m_cached_TextElement.yOffset - this.m_cached_TextElement.height - num3 - num20) * num2), 0f, 0f);
							vector3 += b;
							vector4 += b2;
							vector5 += b;
							vector6 += b2;
						}
						bool isFXMatrixSet = this.m_isFXMatrixSet;
						if (isFXMatrixSet)
						{
							Vector3 b3 = (vector5 + vector4) / 2f;
							vector3 = this.m_FXMatrix.MultiplyPoint3x4(vector3 - b3) + b3;
							vector4 = this.m_FXMatrix.MultiplyPoint3x4(vector4 - b3) + b3;
							vector5 = this.m_FXMatrix.MultiplyPoint3x4(vector5 - b3) + b3;
							vector6 = this.m_FXMatrix.MultiplyPoint3x4(vector6 - b3) + b3;
						}
						this.m_textInfo.characterInfo[this.m_characterCount].bottomLeft = vector4;
						this.m_textInfo.characterInfo[this.m_characterCount].topLeft = vector3;
						this.m_textInfo.characterInfo[this.m_characterCount].topRight = vector5;
						this.m_textInfo.characterInfo[this.m_characterCount].bottomRight = vector6;
						this.m_textInfo.characterInfo[this.m_characterCount].origin = this.m_xAdvance;
						this.m_textInfo.characterInfo[this.m_characterCount].baseLine = 0f - this.m_lineOffset + this.m_baselineOffset;
						this.m_textInfo.characterInfo[this.m_characterCount].aspectRatio = (vector5.x - vector4.x) / (vector3.y - vector4.y);
						float num22 = this.m_currentFontAsset.fontInfo.Ascender * ((this.m_textElementType == TMP_TextElementType.Character) ? num2 : this.m_textInfo.characterInfo[this.m_characterCount].scale) + this.m_baselineOffset;
						this.m_textInfo.characterInfo[this.m_characterCount].ascender = num22 - this.m_lineOffset;
						this.m_maxLineAscender = ((num22 > this.m_maxLineAscender) ? num22 : this.m_maxLineAscender);
						float num23 = this.m_currentFontAsset.fontInfo.Descender * ((this.m_textElementType == TMP_TextElementType.Character) ? num2 : this.m_textInfo.characterInfo[this.m_characterCount].scale) + this.m_baselineOffset;
						float num24 = this.m_textInfo.characterInfo[this.m_characterCount].descender = num23 - this.m_lineOffset;
						this.m_maxLineDescender = ((num23 < this.m_maxLineDescender) ? num23 : this.m_maxLineDescender);
						bool flag40 = (this.m_style & FontStyles.Subscript) == FontStyles.Subscript || (this.m_style & FontStyles.Superscript) == FontStyles.Superscript;
						if (flag40)
						{
							float num25 = (num22 - this.m_baselineOffset) / this.m_currentFontAsset.fontInfo.SubSize;
							num22 = this.m_maxLineAscender;
							this.m_maxLineAscender = ((num25 > this.m_maxLineAscender) ? num25 : this.m_maxLineAscender);
							float num26 = (num23 - this.m_baselineOffset) / this.m_currentFontAsset.fontInfo.SubSize;
							num23 = this.m_maxLineDescender;
							this.m_maxLineDescender = ((num26 < this.m_maxLineDescender) ? num26 : this.m_maxLineDescender);
						}
						bool flag41 = this.m_lineNumber == 0 || this.m_isNewPage;
						if (flag41)
						{
							this.m_maxAscender = ((this.m_maxAscender > num22) ? this.m_maxAscender : num22);
							this.m_maxCapHeight = Mathf.Max(this.m_maxCapHeight, this.m_currentFontAsset.fontInfo.CapHeight * num2);
						}
						bool flag42 = this.m_lineOffset == 0f;
						if (flag42)
						{
							num10 = ((num10 > num22) ? num10 : num22);
						}
						this.m_textInfo.characterInfo[this.m_characterCount].isVisible = false;
						bool flag43 = num15 == 9 || (!char.IsWhiteSpace((char)num15) && num15 != 8203) || this.m_textElementType == TMP_TextElementType.Sprite;
						if (flag43)
						{
							this.m_textInfo.characterInfo[this.m_characterCount].isVisible = true;
							num9 = ((this.m_width != -1f) ? Mathf.Min(marginWidth + 0.0001f - this.m_marginLeft - this.m_marginRight, this.m_width) : (marginWidth + 0.0001f - this.m_marginLeft - this.m_marginRight));
							this.m_textInfo.lineInfo[this.m_lineNumber].marginLeft = this.m_marginLeft;
							bool flag44 = (this.m_lineJustification & (TextAlignmentOptions)16) == (TextAlignmentOptions)16 || (this.m_lineJustification & (TextAlignmentOptions)8) == (TextAlignmentOptions)8;
							bool flag45 = Mathf.Abs(this.m_xAdvance) + ((!this.m_isRightToLeft) ? this.m_cached_TextElement.xAdvance : 0f) * (1f - this.m_charWidthAdjDelta) * ((num15 != 173) ? num2 : num18) > num9 * (flag44 ? 1.05f : 1f);
							if (flag45)
							{
								num8 = this.m_characterCount - 1;
								bool flag46 = base.enableWordWrapping && this.m_characterCount != this.m_firstCharacterOfLine;
								if (flag46)
								{
									bool flag47 = num12 == this.m_SavedWordWrapState.previous_WordBreak || flag10;
									if (flag47)
									{
										bool flag48 = this.m_enableAutoSizing && this.m_fontSize > this.m_fontSizeMin;
										if (flag48)
										{
											bool flag49 = this.m_charWidthAdjDelta < this.m_charWidthMaxAdj / 100f;
											if (flag49)
											{
												this.loopCountA = 0;
												this.m_charWidthAdjDelta += 0.01f;
												this.GenerateTextMesh();
												return;
											}
											this.m_maxFontSize = this.m_fontSize;
											this.m_fontSize -= Mathf.Max((this.m_fontSize - this.m_minFontSize) / 2f, 0.05f);
											this.m_fontSize = (float)((int)(Mathf.Max(this.m_fontSize, this.m_fontSizeMin) * 20f + 0.5f)) / 20f;
											bool flag50 = this.loopCountA > 20;
											if (flag50)
											{
												return;
											}
											this.GenerateTextMesh();
											return;
										}
										else
										{
											bool flag51 = !this.m_isCharacterWrappingEnabled;
											if (flag51)
											{
												bool flag52 = !flag11;
												if (flag52)
												{
													flag11 = true;
												}
												else
												{
													this.m_isCharacterWrappingEnabled = true;
												}
											}
											else
											{
												flag12 = true;
											}
										}
									}
									num14 = base.RestoreWordWrappingState(ref this.m_SavedWordWrapState);
									num12 = num14;
									bool flag53 = this.m_char_buffer[num14] == 173;
									if (flag53)
									{
										this.m_isTextTruncated = true;
										this.m_char_buffer[num14] = 45;
										this.GenerateTextMesh();
										return;
									}
									bool flag54 = this.m_lineNumber > 0 && !TMP_Math.Approximately(this.m_maxLineAscender, this.m_startOfLineAscender) && this.m_lineHeight == -32767f && !this.m_isNewPage;
									if (flag54)
									{
										float num27 = this.m_maxLineAscender - this.m_startOfLineAscender;
										this.AdjustLineOffset(this.m_firstCharacterOfLine, this.m_characterCount, num27);
										this.m_lineOffset += num27;
										this.m_SavedWordWrapState.lineOffset = this.m_lineOffset;
										this.m_SavedWordWrapState.previousLineAscender = this.m_maxLineAscender;
									}
									this.m_isNewPage = false;
									float num28 = this.m_maxLineAscender - this.m_lineOffset;
									float num29 = this.m_maxLineDescender - this.m_lineOffset;
									this.m_maxDescender = ((this.m_maxDescender < num29) ? this.m_maxDescender : num29);
									bool flag55 = !flag9;
									if (flag55)
									{
										num11 = this.m_maxDescender;
									}
									bool flag56 = this.m_useMaxVisibleDescender && (this.m_characterCount >= this.m_maxVisibleCharacters || this.m_lineNumber >= this.m_maxVisibleLines);
									if (flag56)
									{
										flag9 = true;
									}
									this.m_textInfo.lineInfo[this.m_lineNumber].firstCharacterIndex = this.m_firstCharacterOfLine;
									this.m_textInfo.lineInfo[this.m_lineNumber].firstVisibleCharacterIndex = (this.m_firstVisibleCharacterOfLine = ((this.m_firstCharacterOfLine > this.m_firstVisibleCharacterOfLine) ? this.m_firstCharacterOfLine : this.m_firstVisibleCharacterOfLine));
									this.m_textInfo.lineInfo[this.m_lineNumber].lastCharacterIndex = (this.m_lastCharacterOfLine = ((this.m_characterCount - 1 > 0) ? (this.m_characterCount - 1) : 0));
									this.m_textInfo.lineInfo[this.m_lineNumber].lastVisibleCharacterIndex = (this.m_lastVisibleCharacterOfLine = ((this.m_lastVisibleCharacterOfLine < this.m_firstVisibleCharacterOfLine) ? this.m_firstVisibleCharacterOfLine : this.m_lastVisibleCharacterOfLine));
									this.m_textInfo.lineInfo[this.m_lineNumber].characterCount = this.m_textInfo.lineInfo[this.m_lineNumber].lastCharacterIndex - this.m_textInfo.lineInfo[this.m_lineNumber].firstCharacterIndex + 1;
									this.m_textInfo.lineInfo[this.m_lineNumber].visibleCharacterCount = this.m_lineVisibleCharacterCount;
									this.m_textInfo.lineInfo[this.m_lineNumber].lineExtents.min = new Vector2(this.m_textInfo.characterInfo[this.m_firstVisibleCharacterOfLine].bottomLeft.x, num29);
									this.m_textInfo.lineInfo[this.m_lineNumber].lineExtents.max = new Vector2(this.m_textInfo.characterInfo[this.m_lastVisibleCharacterOfLine].topRight.x, num28);
									this.m_textInfo.lineInfo[this.m_lineNumber].length = this.m_textInfo.lineInfo[this.m_lineNumber].lineExtents.max.x;
									this.m_textInfo.lineInfo[this.m_lineNumber].width = num9;
									this.m_textInfo.lineInfo[this.m_lineNumber].maxAdvance = this.m_textInfo.characterInfo[this.m_lastVisibleCharacterOfLine].xAdvance - (this.m_characterSpacing + this.m_currentFontAsset.normalSpacingOffset) * num2 - this.m_cSpacing;
									this.m_textInfo.lineInfo[this.m_lineNumber].baseline = 0f - this.m_lineOffset;
									this.m_textInfo.lineInfo[this.m_lineNumber].ascender = num28;
									this.m_textInfo.lineInfo[this.m_lineNumber].descender = num29;
									this.m_textInfo.lineInfo[this.m_lineNumber].lineHeight = num28 - num29 + num5 * num;
									this.m_firstCharacterOfLine = this.m_characterCount;
									this.m_lineVisibleCharacterCount = 0;
									base.SaveWordWrappingState(ref this.m_SavedLineState, num14, this.m_characterCount - 1);
									this.m_lineNumber++;
									flag8 = true;
									bool flag57 = this.m_lineNumber >= this.m_textInfo.lineInfo.Length;
									if (flag57)
									{
										base.ResizeLineExtents(this.m_lineNumber);
									}
									bool flag58 = this.m_lineHeight == -32767f;
									if (flag58)
									{
										float num30 = this.m_textInfo.characterInfo[this.m_characterCount].ascender - this.m_textInfo.characterInfo[this.m_characterCount].baseLine;
										float num31 = 0f - this.m_maxLineDescender + num30 + (num5 + this.m_lineSpacing + this.m_lineSpacingDelta) * num;
										this.m_lineOffset += num31;
										this.m_startOfLineAscender = num30;
									}
									else
									{
										this.m_lineOffset += this.m_lineHeight + this.m_lineSpacing * num;
									}
									this.m_maxLineAscender = TMP_Text.k_LargeNegativeFloat;
									this.m_maxLineDescender = TMP_Text.k_LargePositiveFloat;
									this.m_xAdvance = 0f + this.tag_Indent;
									goto IL_3400;
								}
								else
								{
									bool flag59 = this.m_enableAutoSizing && this.m_fontSize > this.m_fontSizeMin;
									if (flag59)
									{
										bool flag60 = this.m_charWidthAdjDelta < this.m_charWidthMaxAdj / 100f;
										if (flag60)
										{
											this.loopCountA = 0;
											this.m_charWidthAdjDelta += 0.01f;
											this.GenerateTextMesh();
											return;
										}
										this.m_maxFontSize = this.m_fontSize;
										this.m_fontSize -= Mathf.Max((this.m_fontSize - this.m_minFontSize) / 2f, 0.05f);
										this.m_fontSize = (float)((int)(Mathf.Max(this.m_fontSize, this.m_fontSizeMin) * 20f + 0.5f)) / 20f;
										bool flag61 = this.loopCountA > 20;
										if (flag61)
										{
											return;
										}
										this.GenerateTextMesh();
										return;
									}
									else
									{
										switch (this.m_overflowMode)
										{
											case TextOverflowModes.Overflow:
												{
													bool isMaskingEnabled = this.m_isMaskingEnabled;
													if (isMaskingEnabled)
													{
														this.DisableMasking();
													}
													break;
												}
											case TextOverflowModes.Ellipsis:
												{
													bool isMaskingEnabled2 = this.m_isMaskingEnabled;
													if (isMaskingEnabled2)
													{
														this.DisableMasking();
													}
													this.m_isTextTruncated = true;
													bool flag62 = this.m_characterCount < 1;
													if (!flag62)
													{
														this.m_char_buffer[num14 - 1] = 8230;
														this.m_char_buffer[num14] = 0;
														bool flag63 = this.m_cached_Ellipsis_GlyphInfo != null;
														if (flag63)
														{
															this.m_textInfo.characterInfo[num8].character = '…';
															this.m_textInfo.characterInfo[num8].textElement = this.m_cached_Ellipsis_GlyphInfo;
															this.m_textInfo.characterInfo[num8].fontAsset = this.m_materialReferences[0].fontAsset;
															this.m_textInfo.characterInfo[num8].material = this.m_materialReferences[0].material;
															this.m_textInfo.characterInfo[num8].materialReferenceIndex = 0;
														}
														else
														{
															Debug.LogWarning("Unable to use Ellipsis character since it wasn't found in the current Font Asset [" + this.m_fontAsset.name + "]. Consider regenerating this font asset to include the Ellipsis character (u+2026).\nNote: Warnings can be disabled in the TMP Settings file.", this);
														}
														this.m_totalCharacterCount = num8 + 1;
														this.GenerateTextMesh();
														return;
													}
													this.m_textInfo.characterInfo[this.m_characterCount].isVisible = false;
													break;
												}
											case TextOverflowModes.Masking:
												{
													bool flag64 = !this.m_isMaskingEnabled;
													if (flag64)
													{
														this.EnableMasking();
													}
													break;
												}
											case TextOverflowModes.Truncate:
												{
													bool isMaskingEnabled3 = this.m_isMaskingEnabled;
													if (isMaskingEnabled3)
													{
														this.DisableMasking();
													}
													this.m_textInfo.characterInfo[this.m_characterCount].isVisible = false;
													break;
												}
											case TextOverflowModes.ScrollRect:
												{
													bool flag65 = !this.m_isMaskingEnabled;
													if (flag65)
													{
														this.EnableMasking();
													}
													break;
												}
										}
									}
								}
							}
							bool flag66 = num15 != 9;
							if (flag66)
							{
								bool overrideHtmlColors = this.m_overrideHtmlColors;
								Color32 vertexColor;
								if (overrideHtmlColors)
								{
									vertexColor = this.m_fontColor32;
								}
								else
								{
									vertexColor = this.m_htmlColor;
								}
								bool flag67 = this.m_textElementType == TMP_TextElementType.Character;
								if (flag67)
								{
									this.SaveGlyphVertexInfo(num3, num20, vertexColor);
								}
								else
								{
									bool flag68 = this.m_textElementType == TMP_TextElementType.Sprite;
									if (flag68)
									{
										this.SaveSpriteVertexInfo(vertexColor);
									}
								}
							}
							else
							{
								this.m_textInfo.characterInfo[this.m_characterCount].isVisible = false;
								this.m_lastVisibleCharacterOfLine = this.m_characterCount;
								TMP_LineInfo[] lineInfo = this.m_textInfo.lineInfo;
								int lineNumber = this.m_lineNumber;
								lineInfo[lineNumber].spaceCount = lineInfo[lineNumber].spaceCount + 1;
								this.m_textInfo.spaceCount++;
							}
							bool flag69 = this.m_textInfo.characterInfo[this.m_characterCount].isVisible && num15 != 173;
							if (flag69)
							{
								bool flag70 = flag8;
								if (flag70)
								{
									flag8 = false;
									this.m_firstVisibleCharacterOfLine = this.m_characterCount;
								}
								this.m_lineVisibleCharacterCount++;
								this.m_lastVisibleCharacterOfLine = this.m_characterCount;
							}
						}
						else
						{
							bool flag71 = (num15 == 10 || char.IsSeparator((char)num15)) && num15 != 173 && num15 != 8203 && num15 != 8288;
							if (flag71)
							{
								TMP_LineInfo[] lineInfo2 = this.m_textInfo.lineInfo;
								int lineNumber2 = this.m_lineNumber;
								lineInfo2[lineNumber2].spaceCount = lineInfo2[lineNumber2].spaceCount + 1;
								this.m_textInfo.spaceCount++;
							}
						}
						bool flag72 = this.m_lineNumber > 0 && !TMP_Math.Approximately(this.m_maxLineAscender, this.m_startOfLineAscender) && this.m_lineHeight == -32767f && !this.m_isNewPage;
						if (flag72)
						{
							float num32 = this.m_maxLineAscender - this.m_startOfLineAscender;
							this.AdjustLineOffset(this.m_firstCharacterOfLine, this.m_characterCount, num32);
							num24 -= num32;
							this.m_lineOffset += num32;
							this.m_startOfLineAscender += num32;
							this.m_SavedWordWrapState.lineOffset = this.m_lineOffset;
							this.m_SavedWordWrapState.previousLineAscender = this.m_startOfLineAscender;
						}
						this.m_textInfo.characterInfo[this.m_characterCount].lineNumber = (short)this.m_lineNumber;
						this.m_textInfo.characterInfo[this.m_characterCount].pageNumber = (short)this.m_pageNumber;
						bool flag73 = (num15 != 10 && num15 != 13 && num15 != 8230) || this.m_textInfo.lineInfo[this.m_lineNumber].characterCount == 1;
						if (flag73)
						{
							this.m_textInfo.lineInfo[this.m_lineNumber].alignment = this.m_lineJustification;
						}
						bool flag74 = this.m_maxAscender - num24 > marginHeight + 0.0001f;
						if (flag74)
						{
							bool flag75 = this.m_enableAutoSizing && this.m_lineSpacingDelta > this.m_lineSpacingMax && this.m_lineNumber > 0;
							if (flag75)
							{
								this.loopCountA = 0;
								this.m_lineSpacingDelta -= 1f;
								this.GenerateTextMesh();
								return;
							}
							bool flag76 = this.m_enableAutoSizing && this.m_fontSize > this.m_fontSizeMin;
							if (flag76)
							{
								this.m_maxFontSize = this.m_fontSize;
								this.m_fontSize -= Mathf.Max((this.m_fontSize - this.m_minFontSize) / 2f, 0.05f);
								this.m_fontSize = (float)((int)(Mathf.Max(this.m_fontSize, this.m_fontSizeMin) * 20f + 0.5f)) / 20f;
								bool flag77 = this.loopCountA > 20;
								if (flag77)
								{
									return;
								}
								this.GenerateTextMesh();
								return;
							}
							else
							{
								bool flag78 = this.m_firstOverflowCharacterIndex == -1;
								if (flag78)
								{
									this.m_firstOverflowCharacterIndex = this.m_characterCount;
								}
								switch (this.m_overflowMode)
								{
									case TextOverflowModes.Overflow:
										{
											bool isMaskingEnabled4 = this.m_isMaskingEnabled;
											if (isMaskingEnabled4)
											{
												this.DisableMasking();
											}
											break;
										}
									case TextOverflowModes.Ellipsis:
										{
											bool isMaskingEnabled5 = this.m_isMaskingEnabled;
											if (isMaskingEnabled5)
											{
												this.DisableMasking();
											}
											bool flag79 = this.m_lineNumber > 0;
											if (flag79)
											{
												this.m_char_buffer[(int)this.m_textInfo.characterInfo[num8].index] = 8230;
												this.m_char_buffer[(int)(this.m_textInfo.characterInfo[num8].index + 1)] = 0;
												bool flag80 = this.m_cached_Ellipsis_GlyphInfo != null;
												if (flag80)
												{
													this.m_textInfo.characterInfo[num8].character = '…';
													this.m_textInfo.characterInfo[num8].textElement = this.m_cached_Ellipsis_GlyphInfo;
													this.m_textInfo.characterInfo[num8].fontAsset = this.m_materialReferences[0].fontAsset;
													this.m_textInfo.characterInfo[num8].material = this.m_materialReferences[0].material;
													this.m_textInfo.characterInfo[num8].materialReferenceIndex = 0;
												}
												else
												{
													Debug.LogWarning("Unable to use Ellipsis character since it wasn't found in the current Font Asset [" + this.m_fontAsset.name + "]. Consider regenerating this font asset to include the Ellipsis character (u+2026).\nNote: Warnings can be disabled in the TMP Settings file.", this);
												}
												this.m_totalCharacterCount = num8 + 1;
												this.GenerateTextMesh();
												this.m_isTextTruncated = true;
												return;
											}
											this.ClearMesh(false);
											return;
										}
									case TextOverflowModes.Masking:
										{
											bool flag81 = !this.m_isMaskingEnabled;
											if (flag81)
											{
												this.EnableMasking();
											}
											break;
										}
									case TextOverflowModes.Truncate:
										{
											bool isMaskingEnabled6 = this.m_isMaskingEnabled;
											if (isMaskingEnabled6)
											{
												this.DisableMasking();
											}
											bool flag82 = this.m_lineNumber > 0;
											if (flag82)
											{
												this.m_char_buffer[(int)(this.m_textInfo.characterInfo[num8].index + 1)] = 0;
												this.m_totalCharacterCount = num8 + 1;
												this.GenerateTextMesh();
												this.m_isTextTruncated = true;
												return;
											}
											this.ClearMesh(false);
											return;
										}
									case TextOverflowModes.ScrollRect:
										{
											bool flag83 = !this.m_isMaskingEnabled;
											if (flag83)
											{
												this.EnableMasking();
											}
											break;
										}
									case TextOverflowModes.Page:
										{
											bool isMaskingEnabled7 = this.m_isMaskingEnabled;
											if (isMaskingEnabled7)
											{
												this.DisableMasking();
											}
											bool flag84 = num15 == 13 || num15 == 10;
											if (!flag84)
											{
												bool flag85 = num14 == 0;
												if (flag85)
												{
													this.ClearMesh();
													return;
												}
												bool flag86 = num7 == num14;
												if (flag86)
												{
													this.m_char_buffer[num14] = 0;
													this.m_isTextTruncated = true;
												}
												num7 = num14;
												num14 = base.RestoreWordWrappingState(ref this.m_SavedLineState);
												this.m_isNewPage = true;
												this.m_xAdvance = 0f + this.tag_Indent;
												this.m_lineOffset = 0f;
												this.m_maxAscender = 0f;
												num10 = 0f;
												this.m_lineNumber++;
												this.m_pageNumber++;
												goto IL_3400;
											}
											break;
										}
									case TextOverflowModes.Linked:
										{
											bool flag87 = this.m_linkedTextComponent != null;
											if (flag87)
											{
												this.m_linkedTextComponent.text = base.text;
												this.m_linkedTextComponent.firstVisibleCharacter = this.m_characterCount;
												this.m_linkedTextComponent.ForceMeshUpdate();
											}
											bool flag88 = this.m_lineNumber > 0;
											if (flag88)
											{
												this.m_char_buffer[num14] = 0;
												this.m_totalCharacterCount = this.m_characterCount;
												this.GenerateTextMesh();
												this.m_isTextTruncated = true;
												return;
											}
											this.ClearMesh(true);
											return;
										}
								}
							}
						}
						bool flag89 = num15 == 9;
						if (flag89)
						{
							float num33 = this.m_currentFontAsset.fontInfo.TabWidth * num2;
							float num34 = Mathf.Ceil(this.m_xAdvance / num33) * num33;
							this.m_xAdvance = ((num34 > this.m_xAdvance) ? num34 : (this.m_xAdvance + num33));
						}
						else
						{
							bool flag90 = this.m_monoSpacing != 0f;
							if (flag90)
							{
								this.m_xAdvance += (this.m_monoSpacing - num19 + (this.m_characterSpacing + this.m_currentFontAsset.normalSpacingOffset) * num2 + this.m_cSpacing) * (1f - this.m_charWidthAdjDelta);
								bool flag91 = char.IsWhiteSpace((char)num15) || num15 == 8203;
								if (flag91)
								{
									this.m_xAdvance += this.m_wordSpacing * num2;
								}
							}
							else
							{
								bool flag92 = !this.m_isRightToLeft;
								if (flag92)
								{
									this.m_xAdvance += ((this.m_cached_TextElement.xAdvance * num4 + this.m_characterSpacing + this.m_currentFontAsset.normalSpacingOffset) * num2 + this.m_cSpacing) * (1f - this.m_charWidthAdjDelta);
									bool flag93 = char.IsWhiteSpace((char)num15) || num15 == 8203;
									if (flag93)
									{
										this.m_xAdvance += this.m_wordSpacing * num2;
									}
								}
							}
						}
						this.m_textInfo.characterInfo[this.m_characterCount].xAdvance = this.m_xAdvance;
						bool flag94 = num15 == 13;
						if (flag94)
						{
							this.m_xAdvance = 0f + this.tag_Indent;
						}
						bool flag95 = num15 == 10 || this.m_characterCount == totalCharacterCount - 1;
						if (flag95)
						{
							bool flag96 = this.m_lineNumber > 0 && !TMP_Math.Approximately(this.m_maxLineAscender, this.m_startOfLineAscender) && this.m_lineHeight == -32767f && !this.m_isNewPage;
							if (flag96)
							{
								float num35 = this.m_maxLineAscender - this.m_startOfLineAscender;
								this.AdjustLineOffset(this.m_firstCharacterOfLine, this.m_characterCount, num35);
								num24 -= num35;
								this.m_lineOffset += num35;
							}
							this.m_isNewPage = false;
							float num36 = this.m_maxLineAscender - this.m_lineOffset;
							float num37 = this.m_maxLineDescender - this.m_lineOffset;
							this.m_maxDescender = ((this.m_maxDescender < num37) ? this.m_maxDescender : num37);
							bool flag97 = !flag9;
							if (flag97)
							{
								num11 = this.m_maxDescender;
							}
							bool flag98 = this.m_useMaxVisibleDescender && (this.m_characterCount >= this.m_maxVisibleCharacters || this.m_lineNumber >= this.m_maxVisibleLines);
							if (flag98)
							{
								flag9 = true;
							}
							this.m_textInfo.lineInfo[this.m_lineNumber].firstCharacterIndex = this.m_firstCharacterOfLine;
							this.m_textInfo.lineInfo[this.m_lineNumber].firstVisibleCharacterIndex = (this.m_firstVisibleCharacterOfLine = ((this.m_firstCharacterOfLine > this.m_firstVisibleCharacterOfLine) ? this.m_firstCharacterOfLine : this.m_firstVisibleCharacterOfLine));
							this.m_textInfo.lineInfo[this.m_lineNumber].lastCharacterIndex = (this.m_lastCharacterOfLine = this.m_characterCount);
							this.m_textInfo.lineInfo[this.m_lineNumber].lastVisibleCharacterIndex = (this.m_lastVisibleCharacterOfLine = ((this.m_lastVisibleCharacterOfLine < this.m_firstVisibleCharacterOfLine) ? this.m_firstVisibleCharacterOfLine : this.m_lastVisibleCharacterOfLine));
							this.m_textInfo.lineInfo[this.m_lineNumber].characterCount = this.m_textInfo.lineInfo[this.m_lineNumber].lastCharacterIndex - this.m_textInfo.lineInfo[this.m_lineNumber].firstCharacterIndex + 1;
							this.m_textInfo.lineInfo[this.m_lineNumber].visibleCharacterCount = this.m_lineVisibleCharacterCount;
							this.m_textInfo.lineInfo[this.m_lineNumber].lineExtents.min = new Vector2(this.m_textInfo.characterInfo[this.m_firstVisibleCharacterOfLine].bottomLeft.x, num37);
							this.m_textInfo.lineInfo[this.m_lineNumber].lineExtents.max = new Vector2(this.m_textInfo.characterInfo[this.m_lastVisibleCharacterOfLine].topRight.x, num36);
							this.m_textInfo.lineInfo[this.m_lineNumber].length = this.m_textInfo.lineInfo[this.m_lineNumber].lineExtents.max.x - num3 * num2;
							this.m_textInfo.lineInfo[this.m_lineNumber].width = num9;
							bool flag99 = this.m_textInfo.lineInfo[this.m_lineNumber].characterCount == 1;
							if (flag99)
							{
								this.m_textInfo.lineInfo[this.m_lineNumber].alignment = this.m_lineJustification;
							}
							bool isVisible = this.m_textInfo.characterInfo[this.m_lastVisibleCharacterOfLine].isVisible;
							if (isVisible)
							{
								this.m_textInfo.lineInfo[this.m_lineNumber].maxAdvance = this.m_textInfo.characterInfo[this.m_lastVisibleCharacterOfLine].xAdvance - (this.m_characterSpacing + this.m_currentFontAsset.normalSpacingOffset) * num2 - this.m_cSpacing;
							}
							else
							{
								this.m_textInfo.lineInfo[this.m_lineNumber].maxAdvance = this.m_textInfo.characterInfo[this.m_lastCharacterOfLine].xAdvance - (this.m_characterSpacing + this.m_currentFontAsset.normalSpacingOffset) * num2 - this.m_cSpacing;
							}
							this.m_textInfo.lineInfo[this.m_lineNumber].baseline = 0f - this.m_lineOffset;
							this.m_textInfo.lineInfo[this.m_lineNumber].ascender = num36;
							this.m_textInfo.lineInfo[this.m_lineNumber].descender = num37;
							this.m_textInfo.lineInfo[this.m_lineNumber].lineHeight = num36 - num37 + num5 * num;
							this.m_firstCharacterOfLine = this.m_characterCount + 1;
							this.m_lineVisibleCharacterCount = 0;
							bool flag100 = num15 == 10;
							if (flag100)
							{
								base.SaveWordWrappingState(ref this.m_SavedLineState, num14, this.m_characterCount);
								base.SaveWordWrappingState(ref this.m_SavedWordWrapState, num14, this.m_characterCount);
								this.m_lineNumber++;
								flag8 = true;
								bool flag101 = this.m_lineNumber >= this.m_textInfo.lineInfo.Length;
								if (flag101)
								{
									base.ResizeLineExtents(this.m_lineNumber);
								}
								bool flag102 = this.m_lineHeight == -32767f;
								if (flag102)
								{
									float num31 = 0f - this.m_maxLineDescender + num22 + (num5 + this.m_lineSpacing + this.m_paragraphSpacing + this.m_lineSpacingDelta) * num;
									this.m_lineOffset += num31;
								}
								else
								{
									this.m_lineOffset += this.m_lineHeight + (this.m_lineSpacing + this.m_paragraphSpacing) * num;
								}
								this.m_maxLineAscender = TMP_Text.k_LargeNegativeFloat;
								this.m_maxLineDescender = TMP_Text.k_LargePositiveFloat;
								this.m_startOfLineAscender = num22;
								this.m_xAdvance = 0f + this.tag_LineIndent + this.tag_Indent;
								num8 = this.m_characterCount - 1;
								this.m_characterCount++;
								goto IL_3400;
							}
						}
						bool isVisible2 = this.m_textInfo.characterInfo[this.m_characterCount].isVisible;
						if (isVisible2)
						{
							this.m_meshExtents.min.x = Mathf.Min(this.m_meshExtents.min.x, this.m_textInfo.characterInfo[this.m_characterCount].bottomLeft.x);
							this.m_meshExtents.min.y = Mathf.Min(this.m_meshExtents.min.y, this.m_textInfo.characterInfo[this.m_characterCount].bottomLeft.y);
							this.m_meshExtents.max.x = Mathf.Max(this.m_meshExtents.max.x, this.m_textInfo.characterInfo[this.m_characterCount].topRight.x);
							this.m_meshExtents.max.y = Mathf.Max(this.m_meshExtents.max.y, this.m_textInfo.characterInfo[this.m_characterCount].topRight.y);
						}
						bool flag103 = this.m_overflowMode == TextOverflowModes.Page && num15 != 13 && num15 != 10;
						if (flag103)
						{
							bool flag104 = this.m_pageNumber + 1 > this.m_textInfo.pageInfo.Length;
							if (flag104)
							{
								TMP_TextInfo.Resize<TMP_PageInfo>(ref this.m_textInfo.pageInfo, this.m_pageNumber + 1, true);
							}
							this.m_textInfo.pageInfo[this.m_pageNumber].ascender = num10;
							this.m_textInfo.pageInfo[this.m_pageNumber].descender = ((num23 < this.m_textInfo.pageInfo[this.m_pageNumber].descender) ? num23 : this.m_textInfo.pageInfo[this.m_pageNumber].descender);
							bool flag105 = this.m_pageNumber == 0 && this.m_characterCount == 0;
							if (flag105)
							{
								this.m_textInfo.pageInfo[this.m_pageNumber].firstCharacterIndex = this.m_characterCount;
							}
							else
							{
								bool flag106 = this.m_characterCount > 0 && this.m_pageNumber != (int)this.m_textInfo.characterInfo[this.m_characterCount - 1].pageNumber;
								if (flag106)
								{
									this.m_textInfo.pageInfo[this.m_pageNumber - 1].lastCharacterIndex = this.m_characterCount - 1;
									this.m_textInfo.pageInfo[this.m_pageNumber].firstCharacterIndex = this.m_characterCount;
								}
								else
								{
									bool flag107 = this.m_characterCount == totalCharacterCount - 1;
									if (flag107)
									{
										this.m_textInfo.pageInfo[this.m_pageNumber].lastCharacterIndex = this.m_characterCount;
									}
								}
							}
						}
						bool flag108 = this.m_enableWordWrapping || this.m_overflowMode == TextOverflowModes.Truncate || this.m_overflowMode == TextOverflowModes.Ellipsis;
						if (flag108)
						{
							bool flag109 = (char.IsWhiteSpace((char)num15) || num15 == 8203 || num15 == 45 || num15 == 173) && (!this.m_isNonBreakingSpace || flag11) && num15 != 160 && num15 != 8209 && num15 != 8239 && num15 != 8288;
							if (flag109)
							{
								base.SaveWordWrappingState(ref this.m_SavedWordWrapState, num14, this.m_characterCount);
								this.m_isCharacterWrappingEnabled = false;
								flag10 = false;
							}
							else
							{
								bool flag110 = ((num15 > 4352 && num15 < 4607) || (num15 > 11904 && num15 < 40959) || (num15 > 43360 && num15 < 43391) || (num15 > 44032 && num15 < 55295) || (num15 > 63744 && num15 < 64255) || (num15 > 65072 && num15 < 65103) || (num15 > 65280 && num15 < 65519)) && !this.m_isNonBreakingSpace;
								if (flag110)
								{
									bool flag111 = flag10 || flag12 || (!TMP_Settings.linebreakingRules.leadingCharacters.ContainsKey(num15) && this.m_characterCount < totalCharacterCount - 1 && !TMP_Settings.linebreakingRules.followingCharacters.ContainsKey((int)this.m_textInfo.characterInfo[this.m_characterCount + 1].character));
									if (flag111)
									{
										base.SaveWordWrappingState(ref this.m_SavedWordWrapState, num14, this.m_characterCount);
										this.m_isCharacterWrappingEnabled = false;
										flag10 = false;
									}
								}
								else
								{
									bool flag112 = flag10 || this.m_isCharacterWrappingEnabled || flag12;
									if (flag112)
									{
										base.SaveWordWrappingState(ref this.m_SavedWordWrapState, num14, this.m_characterCount);
									}
								}
							}
						}
						this.m_characterCount++;
						goto IL_3400;
					}
					float num38 = this.m_maxFontSize - this.m_minFontSize;
					bool flag113 = !this.m_isCharacterWrappingEnabled && this.m_enableAutoSizing && num38 > 0.051f && this.m_fontSize < this.m_fontSizeMax;
					if (flag113)
					{
						this.m_minFontSize = this.m_fontSize;
						this.m_fontSize += Mathf.Max((this.m_maxFontSize - this.m_fontSize) / 2f, 0.05f);
						this.m_fontSize = (float)((int)(Mathf.Min(this.m_fontSize, this.m_fontSizeMax) * 20f + 0.5f)) / 20f;
						bool flag114 = this.loopCountA > 20;
						if (!flag114)
						{
							this.GenerateTextMesh();
						}
					}
					else
					{
						this.m_isCharacterWrappingEnabled = false;
						bool flag115 = this.m_characterCount == 0;
						if (flag115)
						{
							this.ClearMesh(true);
							TMPro_EventManager.ON_TEXT_CHANGED(this);
						}
						else
						{
							int num39 = this.m_materialReferences[0].referenceCount * ((!this.m_isVolumetricText) ? 4 : 8);
							this.m_textInfo.meshInfo[0].Clear(false);
							Vector3 a = Vector3.zero;
							Vector3[] rectTransformCorners = this.m_RectTransformCorners;
							TextAlignmentOptions textAlignment = this.m_textAlignment;
							if (textAlignment <= TextAlignmentOptions.BottomJustified)
							{
								if (textAlignment <= TextAlignmentOptions.Right)
								{
									if (textAlignment <= TextAlignmentOptions.TopJustified)
									{
										if (textAlignment - TextAlignmentOptions.TopLeft > 1 && textAlignment != TextAlignmentOptions.TopRight && textAlignment != TextAlignmentOptions.TopJustified)
										{
											goto IL_3B21;
										}
									}
									else if (textAlignment <= TextAlignmentOptions.Justified)
									{
										//if (textAlignment != TextAlignmentOptions.TopFlush && textAlignment != TextAlignmentOptions.TopGeoAligned)
										//{
											goto IL_3B21;
										//}
									}
									else
									{
										if (textAlignment - TextAlignmentOptions.Left > 1 && textAlignment != TextAlignmentOptions.Right)
										{
											goto IL_3B21;
										}
										goto IL_385D;
									}
									bool flag116 = this.m_overflowMode != TextOverflowModes.Page;
									if (flag116)
									{
										a = rectTransformCorners[1] + new Vector3(0f + margin.x, 0f - this.m_maxAscender - margin.y, 0f);
									}
									else
									{
										a = rectTransformCorners[1] + new Vector3(0f + margin.x, 0f - this.m_textInfo.pageInfo[num6].ascender - margin.y, 0f);
									}
									goto IL_3B21;
								}
								if (textAlignment <= TextAlignmentOptions.Bottom)
								{
									if (textAlignment <= TextAlignmentOptions.Justified)
									{
										if (textAlignment != TextAlignmentOptions.Justified)
										{
											goto IL_3B21;
										}
										goto IL_385D;
									}
									else
									{
										/*if (textAlignment == TextAlignmentOptions.CenterGeoAligned)
										{
											goto IL_385D;
										}*/
										if (textAlignment - TextAlignmentOptions.BottomLeft > 1)
										{
											goto IL_3B21;
										}
									}
								}
								else if (textAlignment <= TextAlignmentOptions.BottomJustified)
								{
									if (textAlignment != TextAlignmentOptions.BottomRight && textAlignment != TextAlignmentOptions.BottomJustified)
									{
										goto IL_3B21;
									}
								}
								else// if (textAlignment != TextAlignmentOptions.BottomFlush && textAlignment != TextAlignmentOptions.BottomGeoAligned)
								{
									goto IL_3B21;
								}
								bool flag117 = this.m_overflowMode != TextOverflowModes.Page;
								if (flag117)
								{
									a = rectTransformCorners[0] + new Vector3(0f + margin.x, 0f - num11 + margin.w, 0f);
								}
								else
								{
									a = rectTransformCorners[0] + new Vector3(0f + margin.x, 0f - this.m_textInfo.pageInfo[num6].descender + margin.w, 0f);
								}
								goto IL_3B21;
							IL_385D:
								bool flag118 = this.m_overflowMode != TextOverflowModes.Page;
								if (flag118)
								{
									a = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + margin.x, 0f - (this.m_maxAscender + margin.y + num11 - margin.w) / 2f, 0f);
								}
								else
								{
									a = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + margin.x, 0f - (this.m_textInfo.pageInfo[num6].ascender + margin.y + this.m_textInfo.pageInfo[num6].descender - margin.w) / 2f, 0f);
								}
							}
							else
							{
								if (textAlignment <= TextAlignmentOptions.MidlineRight)
								{
									if (textAlignment <= TextAlignmentOptions.BaselineJustified)
									{
										if (textAlignment - TextAlignmentOptions.BaselineLeft > 1 && textAlignment != TextAlignmentOptions.BaselineRight && textAlignment != TextAlignmentOptions.BaselineJustified)
										{
											goto IL_3B21;
										}
									}
									else if (textAlignment <= TextAlignmentOptions.BaselineJustified)
									{
										//if (textAlignment != TextAlignmentOptions.BaselineFlush && textAlignment != TextAlignmentOptions.BaselineGeoAligned)
										//{
											goto IL_3B21;
										//}
									}
									else
									{
										if (textAlignment - TextAlignmentOptions.MidlineLeft > 1 && textAlignment != TextAlignmentOptions.MidlineRight)
										{
											goto IL_3B21;
										}
										goto IL_3A44;
									}
									a = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + margin.x, 0f, 0f);
									goto IL_3B21;
								}
								if (textAlignment <= TextAlignmentOptions.Capline)
								{
									if (textAlignment <= TextAlignmentOptions.MidlineJustified)
									{
										if (textAlignment != TextAlignmentOptions.MidlineJustified/* && textAlignment != TextAlignmentOptions.MidlineFlush*/)
										{
											goto IL_3B21;
										}
										goto IL_3A44;
									}
									else
									{
										/*if (textAlignment == TextAlignmentOptions.MidlineGeoAligned)
										{
											goto IL_3A44;
										}*/
										if (textAlignment - TextAlignmentOptions.CaplineLeft > 1)
										{
											goto IL_3B21;
										}
									}
								}
								else if (textAlignment <= TextAlignmentOptions.CaplineJustified)
								{
									if (textAlignment != TextAlignmentOptions.CaplineRight && textAlignment != TextAlignmentOptions.CaplineJustified)
									{
										goto IL_3B21;
									}
								}
								else// if (textAlignment != TextAlignmentOptions.CaplineFlush && textAlignment != TextAlignmentOptions.CaplineGeoAligned)
								{
									goto IL_3B21;
								}
								a = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + margin.x, 0f - (this.m_maxCapHeight - margin.y - margin.w) / 2f, 0f);
								goto IL_3B21;
							IL_3A44:
								a = (rectTransformCorners[0] + rectTransformCorners[1]) / 2f + new Vector3(0f + margin.x, 0f - (this.m_meshExtents.max.y + margin.y + this.m_meshExtents.min.y - margin.w) / 2f, 0f);
							}
						IL_3B21:
							Vector3 vector7 = Vector3.zero;
							Vector3 vector8 = Vector3.zero;
							int index_X = 0;
							int index_X2 = 0;
							int num40 = 0;
							int num41 = 0;
							int num42 = 0;
							bool flag119 = false;
							bool flag120 = false;
							int num43 = 0;
							float num44 = this.m_previousLossyScaleY = this.transform.lossyScale.y;
							Color32 color = Color.white;
							Color32 underlineColor = Color.white;
							Color32 highlightColor = new Color32(byte.MaxValue, byte.MaxValue, 0, 64);
							float num45 = 0f;
							float num46 = 0f;
							float num47 = 0f;
							float num48 = TMP_Text.k_LargePositiveFloat;
							int num49 = 0;
							float num50 = 0f;
							float num51 = 0f;
							float b4 = 0f;
							TMP_CharacterInfo[] characterInfo = this.m_textInfo.characterInfo;
							int i = 0;
							while (i < this.m_characterCount)
							{
								TMP_FontAsset fontAsset = characterInfo[i].fontAsset;
								char character2 = characterInfo[i].character;
								int lineNumber3 = (int)characterInfo[i].lineNumber;
								TMP_LineInfo tmp_LineInfo = this.m_textInfo.lineInfo[lineNumber3];
								num41 = lineNumber3 + 1;
								TextAlignmentOptions alignment = tmp_LineInfo.alignment;
								TextAlignmentOptions textAlignmentOptions = alignment;
								if (textAlignmentOptions <= TextAlignmentOptions.BottomJustified)
								{
									if (textAlignmentOptions <= TextAlignmentOptions.Justified)
									{
										if (textAlignmentOptions <= TextAlignmentOptions.TopJustified)
										{
											switch (textAlignmentOptions)
											{
												case TextAlignmentOptions.TopLeft:
													goto IL_3E51;
												case TextAlignmentOptions.Top:
													goto IL_3EA3;
												case (TextAlignmentOptions)259:
													break;
												case TextAlignmentOptions.TopRight:
													goto IL_3F31;
												default:
													if (textAlignmentOptions == TextAlignmentOptions.TopJustified/* || textAlignmentOptions == TextAlignmentOptions.TopFlush*/)
													{
														goto IL_3F8F;
													}
													break;
											}
										}
										else
										{
											/*if (textAlignmentOptions == TextAlignmentOptions.TopGeoAligned)
											{
												goto IL_3EDC;
											}*/
											switch (textAlignmentOptions)
											{
												case TextAlignmentOptions.Left:
													goto IL_3E51;
												case TextAlignmentOptions.Center:
													goto IL_3EA3;
												case (TextAlignmentOptions)515:
													break;
												case TextAlignmentOptions.Right:
													goto IL_3F31;
												default:
													if (textAlignmentOptions == TextAlignmentOptions.Justified)
													{
														goto IL_3F8F;
													}
													break;
											}
										}
									}
									else if (textAlignmentOptions <= TextAlignmentOptions.BottomRight)
									{
										/*if (textAlignmentOptions == TextAlignmentOptions.Flush)
										{
											goto IL_3F8F;
										}
										if (textAlignmentOptions == TextAlignmentOptions.CenterGeoAligned)
										{
											goto IL_3EDC;
										}*/
										switch (textAlignmentOptions)
										{
											case TextAlignmentOptions.BottomLeft:
												goto IL_3E51;
											case TextAlignmentOptions.Bottom:
												goto IL_3EA3;
											case TextAlignmentOptions.BottomRight:
												goto IL_3F31;
										}
									}
									else
									{
										if (textAlignmentOptions == TextAlignmentOptions.BottomJustified/* || textAlignmentOptions == TextAlignmentOptions.BottomFlush*/)
										{
											goto IL_3F8F;
										}
										/*if (textAlignmentOptions == TextAlignmentOptions.BottomGeoAligned)
										{
											goto IL_3EDC;
										}*/
									}
								}
								else if (textAlignmentOptions <= TextAlignmentOptions.MidlineJustified)
								{
									if (textAlignmentOptions <= TextAlignmentOptions.BaselineJustified)
									{
										switch (textAlignmentOptions)
										{
											case TextAlignmentOptions.BaselineLeft:
												goto IL_3E51;
											case TextAlignmentOptions.Baseline:
												goto IL_3EA3;
											case (TextAlignmentOptions)2051:
												break;
											case TextAlignmentOptions.BaselineRight:
												goto IL_3F31;
											default:
												if (textAlignmentOptions == TextAlignmentOptions.BaselineJustified/* || textAlignmentOptions == TextAlignmentOptions.BaselineFlush*/)
												{
													goto IL_3F8F;
												}
												break;
										}
									}
									else
									{
										/*if (textAlignmentOptions == TextAlignmentOptions.BaselineGeoAligned)
										{
											goto IL_3EDC;
										}*/
										switch (textAlignmentOptions)
										{
											case TextAlignmentOptions.MidlineLeft:
												goto IL_3E51;
											case TextAlignmentOptions.Midline:
												goto IL_3EA3;
											case (TextAlignmentOptions)4099:
												break;
											case TextAlignmentOptions.MidlineRight:
												goto IL_3F31;
											default:
												if (textAlignmentOptions == TextAlignmentOptions.MidlineJustified)
												{
													goto IL_3F8F;
												}
												break;
										}
									}
								}
								else if (textAlignmentOptions <= TextAlignmentOptions.CaplineRight)
								{
									/*if (textAlignmentOptions == TextAlignmentOptions.MidlineFlush)
									{
										goto IL_3F8F;
									}
									if (textAlignmentOptions == TextAlignmentOptions.MidlineGeoAligned)
									{
										goto IL_3EDC;
									}*/
									switch (textAlignmentOptions)
									{
										case TextAlignmentOptions.CaplineLeft:
											goto IL_3E51;
										case TextAlignmentOptions.Capline:
											goto IL_3EA3;
										case TextAlignmentOptions.CaplineRight:
											goto IL_3F31;
									}
								}
								else
								{
									if (textAlignmentOptions == TextAlignmentOptions.CaplineJustified/* || textAlignmentOptions == TextAlignmentOptions.CaplineFlush*/)
									{
										goto IL_3F8F;
									}
									/*if (textAlignmentOptions == TextAlignmentOptions.CaplineGeoAligned)
									{
										goto IL_3EDC;
									}*/
								}
							IL_4270:
								vector8 = a + vector7;
								bool isVisible3 = characterInfo[i].isVisible;
								bool flag121 = isVisible3;
								if (flag121)
								{
									TMP_TextElementType elementType = characterInfo[i].elementType;
									TMP_TextElementType tmp_TextElementType = elementType;
									if (tmp_TextElementType != TMP_TextElementType.Character)
									{
										if (tmp_TextElementType != TMP_TextElementType.Sprite)
										{
										}
									}
									else
									{
										Extents lineExtents = tmp_LineInfo.lineExtents;
										float num52 = this.m_uvLineOffset * (float)lineNumber3 % 1f;
										switch (this.m_horizontalMapping)
										{
											case TextureMappingOptions.Character:
												characterInfo[i].vertex_BL.uv2.x = 0f;
												characterInfo[i].vertex_TL.uv2.x = 0f;
												characterInfo[i].vertex_TR.uv2.x = 1f;
												characterInfo[i].vertex_BR.uv2.x = 1f;
												break;
											case TextureMappingOptions.Line:
												{
													bool flag122 = this.m_textAlignment != TextAlignmentOptions.Justified;
													if (flag122)
													{
														characterInfo[i].vertex_BL.uv2.x = (characterInfo[i].vertex_BL.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num52;
														characterInfo[i].vertex_TL.uv2.x = (characterInfo[i].vertex_TL.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num52;
														characterInfo[i].vertex_TR.uv2.x = (characterInfo[i].vertex_TR.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num52;
														characterInfo[i].vertex_BR.uv2.x = (characterInfo[i].vertex_BR.position.x - lineExtents.min.x) / (lineExtents.max.x - lineExtents.min.x) + num52;
													}
													else
													{
														characterInfo[i].vertex_BL.uv2.x = (characterInfo[i].vertex_BL.position.x + vector7.x - this.m_meshExtents.min.x) / (this.m_meshExtents.max.x - this.m_meshExtents.min.x) + num52;
														characterInfo[i].vertex_TL.uv2.x = (characterInfo[i].vertex_TL.position.x + vector7.x - this.m_meshExtents.min.x) / (this.m_meshExtents.max.x - this.m_meshExtents.min.x) + num52;
														characterInfo[i].vertex_TR.uv2.x = (characterInfo[i].vertex_TR.position.x + vector7.x - this.m_meshExtents.min.x) / (this.m_meshExtents.max.x - this.m_meshExtents.min.x) + num52;
														characterInfo[i].vertex_BR.uv2.x = (characterInfo[i].vertex_BR.position.x + vector7.x - this.m_meshExtents.min.x) / (this.m_meshExtents.max.x - this.m_meshExtents.min.x) + num52;
													}
													break;
												}
											case TextureMappingOptions.Paragraph:
												characterInfo[i].vertex_BL.uv2.x = (characterInfo[i].vertex_BL.position.x + vector7.x - this.m_meshExtents.min.x) / (this.m_meshExtents.max.x - this.m_meshExtents.min.x) + num52;
												characterInfo[i].vertex_TL.uv2.x = (characterInfo[i].vertex_TL.position.x + vector7.x - this.m_meshExtents.min.x) / (this.m_meshExtents.max.x - this.m_meshExtents.min.x) + num52;
												characterInfo[i].vertex_TR.uv2.x = (characterInfo[i].vertex_TR.position.x + vector7.x - this.m_meshExtents.min.x) / (this.m_meshExtents.max.x - this.m_meshExtents.min.x) + num52;
												characterInfo[i].vertex_BR.uv2.x = (characterInfo[i].vertex_BR.position.x + vector7.x - this.m_meshExtents.min.x) / (this.m_meshExtents.max.x - this.m_meshExtents.min.x) + num52;
												break;
											case TextureMappingOptions.MatchAspect:
												{
													switch (this.m_verticalMapping)
													{
														case TextureMappingOptions.Character:
															characterInfo[i].vertex_BL.uv2.y = 0f;
															characterInfo[i].vertex_TL.uv2.y = 1f;
															characterInfo[i].vertex_TR.uv2.y = 0f;
															characterInfo[i].vertex_BR.uv2.y = 1f;
															break;
														case TextureMappingOptions.Line:
															characterInfo[i].vertex_BL.uv2.y = (characterInfo[i].vertex_BL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + num52;
															characterInfo[i].vertex_TL.uv2.y = (characterInfo[i].vertex_TL.position.y - lineExtents.min.y) / (lineExtents.max.y - lineExtents.min.y) + num52;
															characterInfo[i].vertex_TR.uv2.y = characterInfo[i].vertex_BL.uv2.y;
															characterInfo[i].vertex_BR.uv2.y = characterInfo[i].vertex_TL.uv2.y;
															break;
														case TextureMappingOptions.Paragraph:
															characterInfo[i].vertex_BL.uv2.y = (characterInfo[i].vertex_BL.position.y - this.m_meshExtents.min.y) / (this.m_meshExtents.max.y - this.m_meshExtents.min.y) + num52;
															characterInfo[i].vertex_TL.uv2.y = (characterInfo[i].vertex_TL.position.y - this.m_meshExtents.min.y) / (this.m_meshExtents.max.y - this.m_meshExtents.min.y) + num52;
															characterInfo[i].vertex_TR.uv2.y = characterInfo[i].vertex_BL.uv2.y;
															characterInfo[i].vertex_BR.uv2.y = characterInfo[i].vertex_TL.uv2.y;
															break;
														case TextureMappingOptions.MatchAspect:
															Debug.Log("ERROR: Cannot Match both Vertical & Horizontal.");
															break;
													}
													float num53 = (1f - (characterInfo[i].vertex_BL.uv2.y + characterInfo[i].vertex_TL.uv2.y) * characterInfo[i].aspectRatio) / 2f;
													characterInfo[i].vertex_BL.uv2.x = characterInfo[i].vertex_BL.uv2.y * characterInfo[i].aspectRatio + num53 + num52;
													characterInfo[i].vertex_TL.uv2.x = characterInfo[i].vertex_BL.uv2.x;
													characterInfo[i].vertex_TR.uv2.x = characterInfo[i].vertex_TL.uv2.y * characterInfo[i].aspectRatio + num53 + num52;
													characterInfo[i].vertex_BR.uv2.x = characterInfo[i].vertex_TR.uv2.x;
													break;
												}
										}
										switch (this.m_verticalMapping)
										{
											case TextureMappingOptions.Character:
												characterInfo[i].vertex_BL.uv2.y = 0f;
												characterInfo[i].vertex_TL.uv2.y = 1f;
												characterInfo[i].vertex_TR.uv2.y = 1f;
												characterInfo[i].vertex_BR.uv2.y = 0f;
												break;
											case TextureMappingOptions.Line:
												characterInfo[i].vertex_BL.uv2.y = (characterInfo[i].vertex_BL.position.y - tmp_LineInfo.descender) / (tmp_LineInfo.ascender - tmp_LineInfo.descender);
												characterInfo[i].vertex_TL.uv2.y = (characterInfo[i].vertex_TL.position.y - tmp_LineInfo.descender) / (tmp_LineInfo.ascender - tmp_LineInfo.descender);
												characterInfo[i].vertex_TR.uv2.y = characterInfo[i].vertex_TL.uv2.y;
												characterInfo[i].vertex_BR.uv2.y = characterInfo[i].vertex_BL.uv2.y;
												break;
											case TextureMappingOptions.Paragraph:
												characterInfo[i].vertex_BL.uv2.y = (characterInfo[i].vertex_BL.position.y - this.m_meshExtents.min.y) / (this.m_meshExtents.max.y - this.m_meshExtents.min.y);
												characterInfo[i].vertex_TL.uv2.y = (characterInfo[i].vertex_TL.position.y - this.m_meshExtents.min.y) / (this.m_meshExtents.max.y - this.m_meshExtents.min.y);
												characterInfo[i].vertex_TR.uv2.y = characterInfo[i].vertex_TL.uv2.y;
												characterInfo[i].vertex_BR.uv2.y = characterInfo[i].vertex_BL.uv2.y;
												break;
											case TextureMappingOptions.MatchAspect:
												{
													float num54 = (1f - (characterInfo[i].vertex_BL.uv2.x + characterInfo[i].vertex_TR.uv2.x) / characterInfo[i].aspectRatio) / 2f;
													characterInfo[i].vertex_BL.uv2.y = num54 + characterInfo[i].vertex_BL.uv2.x / characterInfo[i].aspectRatio;
													characterInfo[i].vertex_TL.uv2.y = num54 + characterInfo[i].vertex_TR.uv2.x / characterInfo[i].aspectRatio;
													characterInfo[i].vertex_BR.uv2.y = characterInfo[i].vertex_BL.uv2.y;
													characterInfo[i].vertex_TR.uv2.y = characterInfo[i].vertex_TL.uv2.y;
													break;
												}
										}
										num45 = characterInfo[i].scale * num44 * (1f - this.m_charWidthAdjDelta);
										bool flag123 = !characterInfo[i].isUsingAlternateTypeface && (characterInfo[i].style & FontStyles.Bold) == FontStyles.Bold;
										if (flag123)
										{
											num45 *= -1f;
										}
										float num55 = characterInfo[i].vertex_BL.uv2.x;
										float num56 = characterInfo[i].vertex_BL.uv2.y;
										float num57 = characterInfo[i].vertex_TR.uv2.x;
										float num58 = characterInfo[i].vertex_TR.uv2.y;
										float num59 = (float)((int)num55);
										float num60 = (float)((int)num56);
										num55 -= num59;
										num57 -= num59;
										num56 -= num60;
										num58 -= num60;
										characterInfo[i].vertex_BL.uv2.x = base.PackUV(num55, num56);
										characterInfo[i].vertex_BL.uv2.y = num45;
										characterInfo[i].vertex_TL.uv2.x = base.PackUV(num55, num58);
										characterInfo[i].vertex_TL.uv2.y = num45;
										characterInfo[i].vertex_TR.uv2.x = base.PackUV(num57, num58);
										characterInfo[i].vertex_TR.uv2.y = num45;
										characterInfo[i].vertex_BR.uv2.x = base.PackUV(num57, num56);
										characterInfo[i].vertex_BR.uv2.y = num45;
									}
									bool flag124 = i < this.m_maxVisibleCharacters && num40 < this.m_maxVisibleWords && lineNumber3 < this.m_maxVisibleLines && this.m_overflowMode != TextOverflowModes.Page;
									if (flag124)
									{
										TMP_CharacterInfo[] array = characterInfo;
										int num61 = i;
										array[num61].vertex_BL.position = array[num61].vertex_BL.position + vector8;
										TMP_CharacterInfo[] array2 = characterInfo;
										int num62 = i;
										array2[num62].vertex_TL.position = array2[num62].vertex_TL.position + vector8;
										TMP_CharacterInfo[] array3 = characterInfo;
										int num63 = i;
										array3[num63].vertex_TR.position = array3[num63].vertex_TR.position + vector8;
										TMP_CharacterInfo[] array4 = characterInfo;
										int num64 = i;
										array4[num64].vertex_BR.position = array4[num64].vertex_BR.position + vector8;
									}
									else
									{
										bool flag125 = i < this.m_maxVisibleCharacters && num40 < this.m_maxVisibleWords && lineNumber3 < this.m_maxVisibleLines && this.m_overflowMode == TextOverflowModes.Page && (int)characterInfo[i].pageNumber == num6;
										if (flag125)
										{
											TMP_CharacterInfo[] array5 = characterInfo;
											int num65 = i;
											array5[num65].vertex_BL.position = array5[num65].vertex_BL.position + vector8;
											TMP_CharacterInfo[] array6 = characterInfo;
											int num66 = i;
											array6[num66].vertex_TL.position = array6[num66].vertex_TL.position + vector8;
											TMP_CharacterInfo[] array7 = characterInfo;
											int num67 = i;
											array7[num67].vertex_TR.position = array7[num67].vertex_TR.position + vector8;
											TMP_CharacterInfo[] array8 = characterInfo;
											int num68 = i;
											array8[num68].vertex_BR.position = array8[num68].vertex_BR.position + vector8;
										}
										else
										{
											characterInfo[i].vertex_BL.position = Vector3.zero;
											characterInfo[i].vertex_TL.position = Vector3.zero;
											characterInfo[i].vertex_TR.position = Vector3.zero;
											characterInfo[i].vertex_BR.position = Vector3.zero;
											characterInfo[i].isVisible = false;
										}
									}
									bool flag126 = elementType == TMP_TextElementType.Character;
									if (flag126)
									{
										this.FillCharacterVertexBuffers(i, index_X, this.m_isVolumetricText);
									}
									else
									{
										bool flag127 = elementType == TMP_TextElementType.Sprite;
										if (flag127)
										{
											this.FillSpriteVertexBuffers(i, index_X2);
										}
									}
								}
								TMP_CharacterInfo[] characterInfo2 = this.m_textInfo.characterInfo;
								int num69 = i;
								characterInfo2[num69].bottomLeft = characterInfo2[num69].bottomLeft + vector8;
								TMP_CharacterInfo[] characterInfo3 = this.m_textInfo.characterInfo;
								int num70 = i;
								characterInfo3[num70].topLeft = characterInfo3[num70].topLeft + vector8;
								TMP_CharacterInfo[] characterInfo4 = this.m_textInfo.characterInfo;
								int num71 = i;
								characterInfo4[num71].topRight = characterInfo4[num71].topRight + vector8;
								TMP_CharacterInfo[] characterInfo5 = this.m_textInfo.characterInfo;
								int num72 = i;
								characterInfo5[num72].bottomRight = characterInfo5[num72].bottomRight + vector8;
								TMP_CharacterInfo[] characterInfo6 = this.m_textInfo.characterInfo;
								int num73 = i;
								characterInfo6[num73].origin = characterInfo6[num73].origin + vector8.x;
								TMP_CharacterInfo[] characterInfo7 = this.m_textInfo.characterInfo;
								int num74 = i;
								characterInfo7[num74].xAdvance = characterInfo7[num74].xAdvance + vector8.x;
								TMP_CharacterInfo[] characterInfo8 = this.m_textInfo.characterInfo;
								int num75 = i;
								characterInfo8[num75].ascender = characterInfo8[num75].ascender + vector8.y;
								TMP_CharacterInfo[] characterInfo9 = this.m_textInfo.characterInfo;
								int num76 = i;
								characterInfo9[num76].descender = characterInfo9[num76].descender + vector8.y;
								TMP_CharacterInfo[] characterInfo10 = this.m_textInfo.characterInfo;
								int num77 = i;
								characterInfo10[num77].baseLine = characterInfo10[num77].baseLine + vector8.y;
								bool flag128 = isVisible3;
								if (flag128)
								{
								}
								bool flag129 = lineNumber3 != num42 || i == this.m_characterCount - 1;
								if (flag129)
								{
									bool flag130 = lineNumber3 != num42;
									if (flag130)
									{
										TMP_LineInfo[] lineInfo3 = this.m_textInfo.lineInfo;
										int num78 = num42;
										lineInfo3[num78].baseline = lineInfo3[num78].baseline + vector8.y;
										TMP_LineInfo[] lineInfo4 = this.m_textInfo.lineInfo;
										int num79 = num42;
										lineInfo4[num79].ascender = lineInfo4[num79].ascender + vector8.y;
										TMP_LineInfo[] lineInfo5 = this.m_textInfo.lineInfo;
										int num80 = num42;
										lineInfo5[num80].descender = lineInfo5[num80].descender + vector8.y;
										this.m_textInfo.lineInfo[num42].lineExtents.min = new Vector2(this.m_textInfo.characterInfo[this.m_textInfo.lineInfo[num42].firstCharacterIndex].bottomLeft.x, this.m_textInfo.lineInfo[num42].descender);
										this.m_textInfo.lineInfo[num42].lineExtents.max = new Vector2(this.m_textInfo.characterInfo[this.m_textInfo.lineInfo[num42].lastVisibleCharacterIndex].topRight.x, this.m_textInfo.lineInfo[num42].ascender);
									}
									bool flag131 = i == this.m_characterCount - 1;
									if (flag131)
									{
										TMP_LineInfo[] lineInfo6 = this.m_textInfo.lineInfo;
										int num81 = lineNumber3;
										lineInfo6[num81].baseline = lineInfo6[num81].baseline + vector8.y;
										TMP_LineInfo[] lineInfo7 = this.m_textInfo.lineInfo;
										int num82 = lineNumber3;
										lineInfo7[num82].ascender = lineInfo7[num82].ascender + vector8.y;
										TMP_LineInfo[] lineInfo8 = this.m_textInfo.lineInfo;
										int num83 = lineNumber3;
										lineInfo8[num83].descender = lineInfo8[num83].descender + vector8.y;
										this.m_textInfo.lineInfo[lineNumber3].lineExtents.min = new Vector2(this.m_textInfo.characterInfo[this.m_textInfo.lineInfo[lineNumber3].firstCharacterIndex].bottomLeft.x, this.m_textInfo.lineInfo[lineNumber3].descender);
										this.m_textInfo.lineInfo[lineNumber3].lineExtents.max = new Vector2(this.m_textInfo.characterInfo[this.m_textInfo.lineInfo[lineNumber3].lastVisibleCharacterIndex].topRight.x, this.m_textInfo.lineInfo[lineNumber3].ascender);
									}
								}
								bool flag132 = char.IsLetterOrDigit(character2) || character2 == '-' || character2 == '­' || character2 == '‐' || character2 == '‑';
								if (flag132)
								{
									bool flag133 = !flag120;
									if (flag133)
									{
										flag120 = true;
										num43 = i;
									}
									bool flag134 = flag120 && i == this.m_characterCount - 1;
									if (flag134)
									{
										int num84 = this.m_textInfo.wordInfo.Length;
										int wordCount = this.m_textInfo.wordCount;
										bool flag135 = this.m_textInfo.wordCount + 1 > num84;
										if (flag135)
										{
											TMP_TextInfo.Resize<TMP_WordInfo>(ref this.m_textInfo.wordInfo, num84 + 1);
										}
										int num85 = i;
										this.m_textInfo.wordInfo[wordCount].firstCharacterIndex = num43;
										this.m_textInfo.wordInfo[wordCount].lastCharacterIndex = num85;
										this.m_textInfo.wordInfo[wordCount].characterCount = num85 - num43 + 1;
										this.m_textInfo.wordInfo[wordCount].textComponent = this;
										num40++;
										this.m_textInfo.wordCount++;
										TMP_LineInfo[] lineInfo9 = this.m_textInfo.lineInfo;
										int num86 = lineNumber3;
										lineInfo9[num86].wordCount = lineInfo9[num86].wordCount + 1;
									}
								}
								else
								{
									bool flag136 = flag120 || (i == 0 && (!char.IsPunctuation(character2) || char.IsWhiteSpace(character2) || character2 == '​' || i == this.m_characterCount - 1));
									if (flag136)
									{
										bool flag137 = i > 0 && i < characterInfo.Length - 1 && i < this.m_characterCount && (character2 == '\'' || character2 == '’') && char.IsLetterOrDigit(characterInfo[i - 1].character) && char.IsLetterOrDigit(characterInfo[i + 1].character);
										if (!flag137)
										{
											int num85 = (i == this.m_characterCount - 1 && char.IsLetterOrDigit(character2)) ? i : (i - 1);
											flag120 = false;
											int num87 = this.m_textInfo.wordInfo.Length;
											int wordCount2 = this.m_textInfo.wordCount;
											bool flag138 = this.m_textInfo.wordCount + 1 > num87;
											if (flag138)
											{
												TMP_TextInfo.Resize<TMP_WordInfo>(ref this.m_textInfo.wordInfo, num87 + 1);
											}
											this.m_textInfo.wordInfo[wordCount2].firstCharacterIndex = num43;
											this.m_textInfo.wordInfo[wordCount2].lastCharacterIndex = num85;
											this.m_textInfo.wordInfo[wordCount2].characterCount = num85 - num43 + 1;
											this.m_textInfo.wordInfo[wordCount2].textComponent = this;
											num40++;
											this.m_textInfo.wordCount++;
											TMP_LineInfo[] lineInfo10 = this.m_textInfo.lineInfo;
											int num88 = lineNumber3;
											lineInfo10[num88].wordCount = lineInfo10[num88].wordCount + 1;
										}
									}
								}
								bool flag139 = (this.m_textInfo.characterInfo[i].style & FontStyles.Underline) == FontStyles.Underline;
								bool flag140 = flag139;
								if (flag140)
								{
									bool flag141 = true;
									int pageNumber = (int)this.m_textInfo.characterInfo[i].pageNumber;
									bool flag142 = i > this.m_maxVisibleCharacters || lineNumber3 > this.m_maxVisibleLines || (this.m_overflowMode == TextOverflowModes.Page && pageNumber + 1 != this.m_pageToDisplay);
									if (flag142)
									{
										flag141 = false;
									}
									bool flag143 = !char.IsWhiteSpace(character2) && character2 != '​';
									if (flag143)
									{
										num47 = Mathf.Max(num47, this.m_textInfo.characterInfo[i].scale);
										num48 = Mathf.Min((pageNumber == num49) ? num48 : TMP_Text.k_LargePositiveFloat, this.m_textInfo.characterInfo[i].baseLine + base.font.fontInfo.Underline * num47);
										num49 = pageNumber;
									}
									bool flag144 = !flag5 && flag141 && i <= tmp_LineInfo.lastVisibleCharacterIndex && character2 != '\n' && character2 != '\r';
									if (flag144)
									{
										bool flag145 = i == tmp_LineInfo.lastVisibleCharacterIndex && char.IsSeparator(character2);
										if (!flag145)
										{
											flag5 = true;
											num46 = this.m_textInfo.characterInfo[i].scale;
											bool flag146 = num47 == 0f;
											if (flag146)
											{
												num47 = num46;
											}
											zero = new Vector3(this.m_textInfo.characterInfo[i].bottomLeft.x, num48, 0f);
											color = this.m_textInfo.characterInfo[i].underlineColor;
										}
									}
									bool flag147 = flag5 && this.m_characterCount == 1;
									if (flag147)
									{
										flag5 = false;
										zero2 = new Vector3(this.m_textInfo.characterInfo[i].topRight.x, num48, 0f);
										float scale = this.m_textInfo.characterInfo[i].scale;
										this.DrawUnderlineMesh(zero, zero2, ref num39, num46, scale, num47, num45, color);
										num47 = 0f;
										num48 = TMP_Text.k_LargePositiveFloat;
									}
									else
									{
										bool flag148 = flag5 && (i == tmp_LineInfo.lastCharacterIndex || i >= tmp_LineInfo.lastVisibleCharacterIndex);
										if (flag148)
										{
											bool flag149 = char.IsWhiteSpace(character2) || character2 == '​';
											float scale;
											if (flag149)
											{
												int lastVisibleCharacterIndex = tmp_LineInfo.lastVisibleCharacterIndex;
												zero2 = new Vector3(this.m_textInfo.characterInfo[lastVisibleCharacterIndex].topRight.x, num48, 0f);
												scale = this.m_textInfo.characterInfo[lastVisibleCharacterIndex].scale;
											}
											else
											{
												zero2 = new Vector3(this.m_textInfo.characterInfo[i].topRight.x, num48, 0f);
												scale = this.m_textInfo.characterInfo[i].scale;
											}
											flag5 = false;
											this.DrawUnderlineMesh(zero, zero2, ref num39, num46, scale, num47, num45, color);
											num47 = 0f;
											num48 = TMP_Text.k_LargePositiveFloat;
										}
										else
										{
											bool flag150 = flag5 && !flag141;
											if (flag150)
											{
												flag5 = false;
												zero2 = new Vector3(this.m_textInfo.characterInfo[i - 1].topRight.x, num48, 0f);
												float scale = this.m_textInfo.characterInfo[i - 1].scale;
												this.DrawUnderlineMesh(zero, zero2, ref num39, num46, scale, num47, num45, color);
												num47 = 0f;
												num48 = TMP_Text.k_LargePositiveFloat;
											}
											else
											{
												bool flag151 = flag5 && i < this.m_characterCount - 1 && !color.Compare(this.m_textInfo.characterInfo[i + 1].underlineColor);
												if (flag151)
												{
													flag5 = false;
													zero2 = new Vector3(this.m_textInfo.characterInfo[i].topRight.x, num48, 0f);
													float scale = this.m_textInfo.characterInfo[i].scale;
													this.DrawUnderlineMesh(zero, zero2, ref num39, num46, scale, num47, num45, color);
													num47 = 0f;
													num48 = TMP_Text.k_LargePositiveFloat;
												}
											}
										}
									}
								}
								else
								{
									bool flag152 = flag5;
									if (flag152)
									{
										flag5 = false;
										zero2 = new Vector3(this.m_textInfo.characterInfo[i - 1].topRight.x, num48, 0f);
										float scale = this.m_textInfo.characterInfo[i - 1].scale;
										this.DrawUnderlineMesh(zero, zero2, ref num39, num46, scale, num47, num45, color);
										num47 = 0f;
										num48 = TMP_Text.k_LargePositiveFloat;
									}
								}
								bool flag153 = (this.m_textInfo.characterInfo[i].style & FontStyles.Strikethrough) == FontStyles.Strikethrough;
								float strikethrough = fontAsset.fontInfo.strikethrough;
								bool flag154 = flag153;
								if (flag154)
								{
									bool flag155 = true;
									bool flag156 = i > this.m_maxVisibleCharacters || lineNumber3 > this.m_maxVisibleLines || (this.m_overflowMode == TextOverflowModes.Page && (int)(this.m_textInfo.characterInfo[i].pageNumber + 1) != this.m_pageToDisplay);
									if (flag156)
									{
										flag155 = false;
									}
									bool flag157 = !flag6 && flag155 && i <= tmp_LineInfo.lastVisibleCharacterIndex && character2 != '\n' && character2 != '\r';
									if (flag157)
									{
										bool flag158 = i == tmp_LineInfo.lastVisibleCharacterIndex && char.IsSeparator(character2);
										if (!flag158)
										{
											flag6 = true;
											num50 = this.m_textInfo.characterInfo[i].pointSize;
											num51 = this.m_textInfo.characterInfo[i].scale;
											zero3 = new Vector3(this.m_textInfo.characterInfo[i].bottomLeft.x, this.m_textInfo.characterInfo[i].baseLine + strikethrough * num51, 0f);
											underlineColor = this.m_textInfo.characterInfo[i].strikethroughColor;
											b4 = this.m_textInfo.characterInfo[i].baseLine;
										}
									}
									bool flag159 = flag6 && this.m_characterCount == 1;
									if (flag159)
									{
										flag6 = false;
										zero4 = new Vector3(this.m_textInfo.characterInfo[i].topRight.x, this.m_textInfo.characterInfo[i].baseLine + strikethrough * num51, 0f);
										this.DrawUnderlineMesh(zero3, zero4, ref num39, num51, num51, num51, num45, underlineColor);
									}
									else
									{
										bool flag160 = flag6 && i == tmp_LineInfo.lastCharacterIndex;
										if (flag160)
										{
											bool flag161 = char.IsWhiteSpace(character2) || character2 == '​';
											if (flag161)
											{
												int lastVisibleCharacterIndex2 = tmp_LineInfo.lastVisibleCharacterIndex;
												zero4 = new Vector3(this.m_textInfo.characterInfo[lastVisibleCharacterIndex2].topRight.x, this.m_textInfo.characterInfo[lastVisibleCharacterIndex2].baseLine + strikethrough * num51, 0f);
											}
											else
											{
												zero4 = new Vector3(this.m_textInfo.characterInfo[i].topRight.x, this.m_textInfo.characterInfo[i].baseLine + strikethrough * num51, 0f);
											}
											flag6 = false;
											this.DrawUnderlineMesh(zero3, zero4, ref num39, num51, num51, num51, num45, underlineColor);
										}
										else
										{
											bool flag162 = flag6 && i < this.m_characterCount && (this.m_textInfo.characterInfo[i + 1].pointSize != num50 || !TMP_Math.Approximately(this.m_textInfo.characterInfo[i + 1].baseLine + vector8.y, b4));
											if (flag162)
											{
												flag6 = false;
												int lastVisibleCharacterIndex3 = tmp_LineInfo.lastVisibleCharacterIndex;
												bool flag163 = i > lastVisibleCharacterIndex3;
												if (flag163)
												{
													zero4 = new Vector3(this.m_textInfo.characterInfo[lastVisibleCharacterIndex3].topRight.x, this.m_textInfo.characterInfo[lastVisibleCharacterIndex3].baseLine + strikethrough * num51, 0f);
												}
												else
												{
													zero4 = new Vector3(this.m_textInfo.characterInfo[i].topRight.x, this.m_textInfo.characterInfo[i].baseLine + strikethrough * num51, 0f);
												}
												this.DrawUnderlineMesh(zero3, zero4, ref num39, num51, num51, num51, num45, underlineColor);
											}
											else
											{
												bool flag164 = flag6 && i < this.m_characterCount && fontAsset.GetInstanceID() != characterInfo[i + 1].fontAsset.GetInstanceID();
												if (flag164)
												{
													flag6 = false;
													zero4 = new Vector3(this.m_textInfo.characterInfo[i].topRight.x, this.m_textInfo.characterInfo[i].baseLine + strikethrough * num51, 0f);
													this.DrawUnderlineMesh(zero3, zero4, ref num39, num51, num51, num51, num45, underlineColor);
												}
												else
												{
													bool flag165 = flag6 && !flag155;
													if (flag165)
													{
														flag6 = false;
														zero4 = new Vector3(this.m_textInfo.characterInfo[i - 1].topRight.x, this.m_textInfo.characterInfo[i - 1].baseLine + strikethrough * num51, 0f);
														this.DrawUnderlineMesh(zero3, zero4, ref num39, num51, num51, num51, num45, underlineColor);
													}
												}
											}
										}
									}
								}
								else
								{
									bool flag166 = flag6;
									if (flag166)
									{
										flag6 = false;
										zero4 = new Vector3(this.m_textInfo.characterInfo[i - 1].topRight.x, this.m_textInfo.characterInfo[i - 1].baseLine + strikethrough * num51, 0f);
										this.DrawUnderlineMesh(zero3, zero4, ref num39, num51, num51, num51, num45, underlineColor);
									}
								}
								bool flag167 = (this.m_textInfo.characterInfo[i].style & FontStyles.Highlight) == FontStyles.Highlight;
								bool flag168 = flag167;
								if (flag168)
								{
									bool flag169 = true;
									int pageNumber2 = (int)this.m_textInfo.characterInfo[i].pageNumber;
									bool flag170 = i > this.m_maxVisibleCharacters || lineNumber3 > this.m_maxVisibleLines || (this.m_overflowMode == TextOverflowModes.Page && pageNumber2 + 1 != this.m_pageToDisplay);
									if (flag170)
									{
										flag169 = false;
									}
									bool flag171 = !flag7 && flag169 && i <= tmp_LineInfo.lastVisibleCharacterIndex && character2 != '\n' && character2 != '\r';
									if (flag171)
									{
										bool flag172 = i == tmp_LineInfo.lastVisibleCharacterIndex && char.IsSeparator(character2);
										if (!flag172)
										{
											flag7 = true;
											vector = TMP_Text.k_LargePositiveVector2;
											vector2 = TMP_Text.k_LargeNegativeVector2;
											highlightColor = this.m_textInfo.characterInfo[i].highlightColor;
										}
									}
									bool flag173 = flag7;
									if (flag173)
									{
										Color32 highlightColor2 = this.m_textInfo.characterInfo[i].highlightColor;
										bool flag174 = false;
										bool flag175 = !highlightColor.Compare(highlightColor2);
										if (flag175)
										{
											vector2.x = (vector2.x + this.m_textInfo.characterInfo[i].bottomLeft.x) / 2f;
											vector.y = Mathf.Min(vector.y, this.m_textInfo.characterInfo[i].descender);
											vector2.y = Mathf.Max(vector2.y, this.m_textInfo.characterInfo[i].ascender);
											this.DrawTextHighlight(vector, vector2, ref num39, highlightColor);
											flag7 = true;
											vector = vector2;
											vector2 = new Vector3(this.m_textInfo.characterInfo[i].topRight.x, this.m_textInfo.characterInfo[i].descender, 0f);
											highlightColor = this.m_textInfo.characterInfo[i].highlightColor;
											flag174 = true;
										}
										bool flag176 = !flag174;
										if (flag176)
										{
											vector.x = Mathf.Min(vector.x, this.m_textInfo.characterInfo[i].bottomLeft.x);
											vector.y = Mathf.Min(vector.y, this.m_textInfo.characterInfo[i].descender);
											vector2.x = Mathf.Max(vector2.x, this.m_textInfo.characterInfo[i].topRight.x);
											vector2.y = Mathf.Max(vector2.y, this.m_textInfo.characterInfo[i].ascender);
										}
									}
									bool flag177 = flag7 && this.m_characterCount == 1;
									if (flag177)
									{
										flag7 = false;
										this.DrawTextHighlight(vector, vector2, ref num39, highlightColor);
									}
									else
									{
										bool flag178 = flag7 && (i == tmp_LineInfo.lastCharacterIndex || i >= tmp_LineInfo.lastVisibleCharacterIndex);
										if (flag178)
										{
											flag7 = false;
											this.DrawTextHighlight(vector, vector2, ref num39, highlightColor);
										}
										else
										{
											bool flag179 = flag7 && !flag169;
											if (flag179)
											{
												flag7 = false;
												this.DrawTextHighlight(vector, vector2, ref num39, highlightColor);
											}
										}
									}
								}
								else
								{
									bool flag180 = flag7;
									if (flag180)
									{
										flag7 = false;
										this.DrawTextHighlight(vector, vector2, ref num39, highlightColor);
									}
								}
								num42 = lineNumber3;
								i++;
								continue;
							IL_3E51:
								bool flag181 = !this.m_isRightToLeft;
								if (flag181)
								{
									vector7 = new Vector3(0f + tmp_LineInfo.marginLeft, 0f, 0f);
								}
								else
								{
									vector7 = new Vector3(0f - tmp_LineInfo.maxAdvance, 0f, 0f);
								}
								goto IL_4270;
							IL_3EA3:
								vector7 = new Vector3(tmp_LineInfo.marginLeft + tmp_LineInfo.width / 2f - tmp_LineInfo.maxAdvance / 2f, 0f, 0f);
								goto IL_4270;
							IL_3EDC:
								vector7 = new Vector3(tmp_LineInfo.marginLeft + tmp_LineInfo.width / 2f - (tmp_LineInfo.lineExtents.min.x + tmp_LineInfo.lineExtents.max.x) / 2f, 0f, 0f);
								goto IL_4270;
							IL_3F31:
								bool flag182 = !this.m_isRightToLeft;
								if (flag182)
								{
									vector7 = new Vector3(tmp_LineInfo.marginLeft + tmp_LineInfo.width - tmp_LineInfo.maxAdvance, 0f, 0f);
								}
								else
								{
									vector7 = new Vector3(tmp_LineInfo.marginLeft + tmp_LineInfo.width, 0f, 0f);
								}
								goto IL_4270;
							IL_3F8F:
								bool flag183 = character2 == '­' || character2 == '​' || character2 == '⁠';
								if (flag183)
								{
									goto IL_4270;
								}
								char character3 = characterInfo[tmp_LineInfo.lastCharacterIndex].character;
								bool flag184 = (alignment & (TextAlignmentOptions)16) == (TextAlignmentOptions)16;
								bool flag185 = (!char.IsControl(character3) && lineNumber3 < this.m_lineNumber) || flag184 || tmp_LineInfo.maxAdvance > tmp_LineInfo.width;
								if (flag185)
								{
									bool flag186 = lineNumber3 != num42 || i == 0 || i == this.m_firstVisibleCharacter;
									if (flag186)
									{
										bool flag187 = !this.m_isRightToLeft;
										if (flag187)
										{
											vector7 = new Vector3(tmp_LineInfo.marginLeft, 0f, 0f);
										}
										else
										{
											vector7 = new Vector3(tmp_LineInfo.marginLeft + tmp_LineInfo.width, 0f, 0f);
										}
										bool flag188 = char.IsSeparator(character2);
										flag119 = flag188;
									}
									else
									{
										float num89 = (!this.m_isRightToLeft) ? (tmp_LineInfo.width - tmp_LineInfo.maxAdvance) : (tmp_LineInfo.width + tmp_LineInfo.maxAdvance);
										int num90 = tmp_LineInfo.visibleCharacterCount - 1;
										int num91 = characterInfo[tmp_LineInfo.lastCharacterIndex].isVisible ? tmp_LineInfo.spaceCount : (tmp_LineInfo.spaceCount - 1);
										bool flag189 = flag119;
										if (flag189)
										{
											num91--;
											num90++;
										}
										float num92 = (num91 > 0) ? this.m_wordWrappingRatios : 1f;
										bool flag190 = num91 < 1;
										if (flag190)
										{
											num91 = 1;
										}
										bool flag191 = character2 == '\t' || char.IsSeparator(character2);
										if (flag191)
										{
											bool flag192 = !this.m_isRightToLeft;
											if (flag192)
											{
												vector7 += new Vector3(num89 * (1f - num92) / (float)num91, 0f, 0f);
											}
											else
											{
												vector7 -= new Vector3(num89 * (1f - num92) / (float)num91, 0f, 0f);
											}
										}
										else
										{
											bool flag193 = !this.m_isRightToLeft;
											if (flag193)
											{
												vector7 += new Vector3(num89 * num92 / (float)num90, 0f, 0f);
											}
											else
											{
												vector7 -= new Vector3(num89 * num92 / (float)num90, 0f, 0f);
											}
										}
									}
								}
								else
								{
									bool flag194 = !this.m_isRightToLeft;
									if (flag194)
									{
										vector7 = new Vector3(tmp_LineInfo.marginLeft, 0f, 0f);
									}
									else
									{
										vector7 = new Vector3(tmp_LineInfo.marginLeft + tmp_LineInfo.width, 0f, 0f);
									}
								}
								goto IL_4270;
							}
							this.m_textInfo.characterCount = (int)((short)this.m_characterCount);
							this.m_textInfo.spriteCount = this.m_spriteCount;
							this.m_textInfo.lineCount = (int)((short)num41);
							this.m_textInfo.wordCount = (int)((num40 != 0 && this.m_characterCount > 0) ? ((short)num40) : 1);
							this.m_textInfo.pageCount = this.m_pageNumber + 1;
							bool flag195 = this.m_renderMode == TextRenderFlags.Render;
							if (flag195)
							{
								bool flag196 = this.m_geometrySortingOrder > VertexSortingOrder.Normal;
								if (flag196)
								{
									this.m_textInfo.meshInfo[0].SortGeometry(VertexSortingOrder.Reverse);
								}
								this.m_mesh.MarkDynamic();
								this.m_mesh.vertices = this.m_textInfo.meshInfo[0].vertices;
								this.m_mesh.uv = this.m_textInfo.meshInfo[0].uvs0;
								this.m_mesh.uv2 = this.m_textInfo.meshInfo[0].uvs2;
								this.m_mesh.colors32 = this.m_textInfo.meshInfo[0].colors32;
								this.m_mesh.RecalculateBounds();
								for (int j = 1; j < this.m_textInfo.materialCount; j++)
								{
									this.m_textInfo.meshInfo[j].ClearUnusedVertices();
									bool flag197 = this.m_subTextObjects[j] == null;
									if (!flag197)
									{
										bool flag198 = this.m_geometrySortingOrder > VertexSortingOrder.Normal;
										if (flag198)
										{
											this.m_textInfo.meshInfo[j].SortGeometry(VertexSortingOrder.Reverse);
										}
										this.m_subTextObjects[j].mesh.vertices = this.m_textInfo.meshInfo[j].vertices;
										this.m_subTextObjects[j].mesh.uv = this.m_textInfo.meshInfo[j].uvs0;
										this.m_subTextObjects[j].mesh.uv2 = this.m_textInfo.meshInfo[j].uvs2;
										this.m_subTextObjects[j].mesh.colors32 = this.m_textInfo.meshInfo[j].colors32;
										this.m_subTextObjects[j].mesh.RecalculateBounds();
									}
								}
							}
							TMPro_EventManager.ON_TEXT_CHANGED(this);
						}
					}
				}
			}
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

		private void SetMeshFilters(bool state)
		{
			if (m_meshFilter != null)
			{
				if (state)
				{
					m_meshFilter.sharedMesh = m_mesh;
				}
				else
				{
					m_meshFilter.sharedMesh = null;
				}
			}
			for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
			{
				if (m_subTextObjects[i].meshFilter != null)
				{
					if (state)
					{
						m_subTextObjects[i].meshFilter.sharedMesh = m_subTextObjects[i].mesh;
					}
					else
					{
						m_subTextObjects[i].meshFilter.sharedMesh = null;
					}
				}
			}
		}

		protected override void SetActiveSubMeshes(bool state)
		{
			for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
			{
				if (m_subTextObjects[i].enabled != state)
				{
					m_subTextObjects[i].enabled = state;
				}
			}
		}

		protected override void ClearSubMeshObjects()
		{
			for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
			{
				Debug.Log("Destroying Sub Text object[" + i + "].");
				UnityEngine.Object.DestroyImmediate(m_subTextObjects[i]);
			}
		}

		protected override Bounds GetCompoundBounds()
		{
			Bounds bounds = m_mesh.bounds;
			Vector3 min = bounds.min;
			Vector3 max = bounds.max;
			for (int i = 1; i < m_subTextObjects.Length && m_subTextObjects[i] != null; i++)
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
			for (int i = 0; i < m_textInfo.characterCount; i++)
			{
				if (m_textInfo.characterInfo[i].isVisible && m_textInfo.characterInfo[i].elementType == TMP_TextElementType.Character)
				{
					float num = lossyScale * m_textInfo.characterInfo[i].scale * (1f - m_charWidthAdjDelta);
					if (!m_textInfo.characterInfo[i].isUsingAlternateTypeface && (m_textInfo.characterInfo[i].style & FontStyles.Bold) == FontStyles.Bold)
					{
						num *= -1f;
					}
					int materialReferenceIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
					int vertexIndex = m_textInfo.characterInfo[i].vertexIndex;
					m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexIndex].y = num;
					m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexIndex + 1].y = num;
					m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexIndex + 2].y = num;
					m_textInfo.meshInfo[materialReferenceIndex].uvs2[vertexIndex + 3].y = num;
				}
			}
			for (int j = 0; j < m_textInfo.meshInfo.Length; j++)
			{
				if (j == 0)
				{
					m_mesh.uv2 = m_textInfo.meshInfo[0].uvs2;
				}
				else
				{
					m_subTextObjects[j].mesh.uv2 = m_textInfo.meshInfo[j].uvs2;
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

		public void SetMask(MaskingTypes type, Vector4 maskCoords)
		{
			SetMask(type);
			SetMaskCoordinates(maskCoords);
		}

		public void SetMask(MaskingTypes type, Vector4 maskCoords, float softnessX, float softnessY)
		{
			SetMask(type);
			SetMaskCoordinates(maskCoords, softnessX, softnessY);
		}

		public override void SetVerticesDirty()
		{
			if (!m_verticesAlreadyDirty && !((UnityEngine.Object)(object)this == null) && ((UIBehaviour)this).IsActive())
			{
				TMP_UpdateManager.RegisterTextElementForGraphicRebuild(this);
				m_verticesAlreadyDirty = true;
			}
		}

		public override void SetLayoutDirty()
		{
			m_isPreferredWidthDirty = true;
			m_isPreferredHeightDirty = true;
			if (!m_layoutAlreadyDirty && !((UnityEngine.Object)(object)this == null) && ((UIBehaviour)this).IsActive())
			{
				m_layoutAlreadyDirty = true;
				m_isLayoutDirty = true;
			}
		}

		public override void SetMaterialDirty()
		{
			//((Graphic)this).UpdateMaterial();
			UpdateMaterial();
		}

		public override void SetAllDirty()
		{
			//SetLayoutDirty();
			//SetVerticesDirty();
			//SetMaterialDirty();

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
				OnPreRenderObject();
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

		protected override void UpdateMaterial()
		{
			if (!(m_sharedMaterial == null))
			{
				if (m_renderer == null)
				{
					m_renderer = renderer;
				}
				if (m_renderer.sharedMaterial.GetInstanceID() != m_sharedMaterial.GetInstanceID())
				{
					m_renderer.sharedMaterial = m_sharedMaterial;
				}
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

		public override void ForceMeshUpdate()
		{
			m_havePropertiesChanged = true;
			OnPreRenderObject();
		}

		public override void ForceMeshUpdate(bool ignoreInactive)
		{
			m_havePropertiesChanged = true;
			m_ignoreActiveState = true;
			OnPreRenderObject();
		}

		public override TMP_TextInfo GetTextInfo(string text)
		{
			StringToCharArray(text, ref m_char_buffer);
			SetArraySizes(m_char_buffer);
			m_renderMode = TextRenderFlags.DontRender;
			ComputeMarginSize();
			GenerateTextMesh();
			m_renderMode = TextRenderFlags.Render;
			return base.textInfo;
		}

		public override void ClearMesh(bool updateMesh)
		{
			if (m_textInfo.meshInfo[0].mesh == null)
			{
				m_textInfo.meshInfo[0].mesh = m_mesh;
			}
			m_textInfo.ClearMeshInfo(updateMesh);
		}

		public override void UpdateGeometry(Mesh mesh, int index)
		{
			mesh.RecalculateBounds();
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
			}
		}

		public void UpdateFontAsset()
		{
			LoadFontAsset();
		}

		public void CalculateLayoutInputHorizontal()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			m_currentAutoSizeMode = m_enableAutoSizing;
			if (m_isCalculateSizeRequired || m_rectTransform.hasChanged)
			{
				m_minWidth = 0f;
				m_flexibleWidth = 0f;
				if (m_enableAutoSizing)
				{
					m_fontSize = m_fontSizeMax;
				}
				m_marginWidth = TMP_Text.k_LargePositiveFloat;
				m_marginHeight = TMP_Text.k_LargePositiveFloat;
				if (m_isInputParsingRequired || m_isTextTruncated)
				{
					ParseInputText();
				}
				GenerateTextMesh();
				m_renderMode = TextRenderFlags.Render;
				ComputeMarginSize();
				m_isLayoutDirty = true;
			}
		}

		public void CalculateLayoutInputVertical()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (m_isCalculateSizeRequired || m_rectTransform.hasChanged)
			{
				m_minHeight = 0f;
				m_flexibleHeight = 0f;
				if (m_enableAutoSizing)
				{
					m_currentAutoSizeMode = true;
					m_enableAutoSizing = false;
				}
				m_marginHeight = TMP_Text.k_LargePositiveFloat;
				GenerateTextMesh();
				m_enableAutoSizing = m_currentAutoSizeMode;
				m_renderMode = TextRenderFlags.Render;
				ComputeMarginSize();
				m_isLayoutDirty = true;
			}
			m_isCalculateSizeRequired = false;
		}
	}
}
