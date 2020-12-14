using System;
#if UNITY_EDITOR
//using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TMPro
{
	[ExecuteInEditMode]
	public class TMP_SubMeshUI : MaskableGraphic, IClippable, IMaskable, IMaterialModifier
	{
		[SerializeField]
		private TMP_FontAsset m_fontAsset;

		[SerializeField]
		private TMP_SpriteAsset m_spriteAsset;

		[SerializeField]
		private Material m_material;

		[SerializeField]
		private Material m_sharedMaterial;

		private Material m_fallbackMaterial;

		private Material m_fallbackSourceMaterial;

		[SerializeField]
		private bool m_isDefaultMaterial;

		[SerializeField]
		private float m_padding;

		[SerializeField]
		private CanvasRenderer m_canvasRenderer;

		private Mesh m_mesh;

		[SerializeField]
		private TextMeshProUGUI m_TextComponent;

		[NonSerialized]
		private bool m_isRegisteredForEvents;

		private bool m_materialDirty;

		[SerializeField]
		private int m_materialReferenceIndex;

		public TMP_FontAsset fontAsset
		{
			get
			{
				return m_fontAsset;
			}
			set
			{
				m_fontAsset = value;
			}
		}

		public TMP_SpriteAsset spriteAsset
		{
			get
			{
				return m_spriteAsset;
			}
			set
			{
				m_spriteAsset = value;
			}
		}

		public override Texture mainTexture
		{
			get
			{
				if (sharedMaterial != null)
				{
					return sharedMaterial.GetTexture(ShaderUtilities.ID_MainTex);
				}
				return null;
			}
		}

		public override Material material
		{
			get
			{
				return GetMaterial(m_sharedMaterial);
			}
			set
			{
				if (!(m_sharedMaterial != null) || m_sharedMaterial.GetInstanceID() != value.GetInstanceID())
				{
					m_sharedMaterial = (m_material = value);
					m_padding = GetPaddingForMaterial();
					SetVerticesDirty();
					SetMaterialDirty();
				}
			}
		}

		public Material sharedMaterial
		{
			get
			{
				return m_sharedMaterial;
			}
			set
			{
				SetSharedMaterial(value);
			}
		}

		public Material fallbackMaterial
		{
			get
			{
				return m_fallbackMaterial;
			}
			set
			{
				if (!(m_fallbackMaterial == value))
				{
					if (m_fallbackMaterial != null && m_fallbackMaterial != value)
					{
						TMP_MaterialManager.ReleaseFallbackMaterial(m_fallbackMaterial);
					}
					m_fallbackMaterial = value;
					TMP_MaterialManager.AddFallbackMaterialReference(m_fallbackMaterial);
					SetSharedMaterial(m_fallbackMaterial);
				}
			}
		}

		public Material fallbackSourceMaterial
		{
			get
			{
				return m_fallbackSourceMaterial;
			}
			set
			{
				m_fallbackSourceMaterial = value;
			}
		}

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

		public bool isDefaultMaterial
		{
			get
			{
				return m_isDefaultMaterial;
			}
			set
			{
				m_isDefaultMaterial = value;
			}
		}

		public float padding
		{
			get
			{
				return m_padding;
			}
			set
			{
				m_padding = value;
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

		public Mesh mesh
		{
			get
			{
				if (m_mesh == null)
				{
					m_mesh = new Mesh();
					m_mesh.hideFlags = HideFlags.HideAndDontSave;
				}
				return m_mesh;
			}
			set
			{
				m_mesh = value;
			}
		}

		public static TMP_SubMeshUI AddSubTextObject(TextMeshProUGUI textComponent, MaterialReference materialReference)
		{
			GameObject gameObject = new GameObject("TMP UI SubObject [" + materialReference.material.name + "]", typeof(RectTransform));
			gameObject.transform.SetParent(textComponent.transform, false);
			gameObject.layer = ((Component)(object)textComponent).gameObject.layer;
			RectTransform component = gameObject.GetComponent<RectTransform>();
			component.anchorMin = Vector2.zero;
			component.anchorMax = Vector2.one;
			component.sizeDelta = Vector2.zero;
			component.pivot = textComponent.rectTransform.pivot;
			TMP_SubMeshUI tMP_SubMeshUI = gameObject.AddComponent<TMP_SubMeshUI>();
			tMP_SubMeshUI.m_canvasRenderer = tMP_SubMeshUI.canvasRenderer;
			tMP_SubMeshUI.m_TextComponent = textComponent;
			tMP_SubMeshUI.m_materialReferenceIndex = materialReference.index;
			tMP_SubMeshUI.m_fontAsset = materialReference.fontAsset;
			tMP_SubMeshUI.m_spriteAsset = materialReference.spriteAsset;
			tMP_SubMeshUI.m_isDefaultMaterial = materialReference.isDefaultMaterial;
			tMP_SubMeshUI.SetSharedMaterial(materialReference.material);
			return tMP_SubMeshUI;
		}

		protected override void OnEnable()
		{
			if (!m_isRegisteredForEvents)
			{
				TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Add(ON_MATERIAL_PROPERTY_CHANGED);
				TMPro_EventManager.FONT_PROPERTY_EVENT.Add(ON_FONT_PROPERTY_CHANGED);
				TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Add(ON_DRAG_AND_DROP_MATERIAL);
				TMPro_EventManager.SPRITE_ASSET_PROPERTY_EVENT.Add(ON_SPRITE_ASSET_PROPERTY_CHANGED);
				m_isRegisteredForEvents = true;
			}
			base.m_ShouldRecalculateStencil = true;
			RecalculateClipping();
			RecalculateMasking();
		}

		protected override void OnDisable()
		{
			TMP_UpdateRegistry.UnRegisterCanvasElementForRebuild((ICanvasElement)(object)this);
			if (base.m_MaskMaterial != null)
			{
				TMP_MaterialManager.ReleaseStencilMaterial(base.m_MaskMaterial);
				base.m_MaskMaterial = null;
			}
			if (m_fallbackMaterial != null)
			{
				TMP_MaterialManager.ReleaseFallbackMaterial(m_fallbackMaterial);
				m_fallbackMaterial = null;
			}
			//((MaskableGraphic)this).OnDisable();
			base.OnDisable();
		}

		protected override void OnDestroy()
		{
			if (m_mesh != null)
			{
				UnityEngine.Object.DestroyImmediate(m_mesh);
			}
			if (base.m_MaskMaterial != null)
			{
				TMP_MaterialManager.ReleaseStencilMaterial(base.m_MaskMaterial);
			}
			if (m_fallbackMaterial != null)
			{
				TMP_MaterialManager.ReleaseFallbackMaterial(m_fallbackMaterial);
				m_fallbackMaterial = null;
			}
			TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Remove(ON_MATERIAL_PROPERTY_CHANGED);
			TMPro_EventManager.FONT_PROPERTY_EVENT.Remove(ON_FONT_PROPERTY_CHANGED);
			TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Remove(ON_DRAG_AND_DROP_MATERIAL);
			TMPro_EventManager.SPRITE_ASSET_PROPERTY_EVENT.Remove(ON_SPRITE_ASSET_PROPERTY_CHANGED);
			m_isRegisteredForEvents = false;
			//((MaskableGraphic)this).RecalculateClipping();
			RecalculateClipping();
		}

		private void ON_MATERIAL_PROPERTY_CHANGED(bool isChanged, Material mat)
		{
			int instanceID = mat.GetInstanceID();
			int instanceID2 = m_sharedMaterial.GetInstanceID();
			int num = (!(base.m_MaskMaterial == null)) ? base.m_MaskMaterial.GetInstanceID() : 0;
			int num2 = (!(m_fallbackSourceMaterial == null)) ? m_fallbackSourceMaterial.GetInstanceID() : 0;
			if (m_fallbackMaterial != null && num2 == instanceID)
			{
				TMP_MaterialManager.CopyMaterialPresetProperties(mat, m_fallbackMaterial);
			}
			if ((UnityEngine.Object)(object)m_TextComponent == null)
			{
				m_TextComponent = base.GetComponentInParent<TextMeshProUGUI>();
			}
			if (base.m_MaskMaterial != null)
			{
#if UNITY_EDITOR
				/*Undo.RecordObject(base.m_MaskMaterial, "Material Property Changes");
				Undo.RecordObject(m_sharedMaterial, "Material Property Changes");*/
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
				else if (num2 == instanceID)
				{
					float float3 = base.m_MaskMaterial.GetFloat(ShaderUtilities.ID_StencilID);
					float float4 = base.m_MaskMaterial.GetFloat(ShaderUtilities.ID_StencilComp);
					base.m_MaskMaterial.CopyPropertiesFromMaterial(m_fallbackMaterial);
					base.m_MaskMaterial.shaderKeywords = m_fallbackMaterial.shaderKeywords;
					base.m_MaskMaterial.SetFloat(ShaderUtilities.ID_StencilID, float3);
					base.m_MaskMaterial.SetFloat(ShaderUtilities.ID_StencilComp, float4);
				}
			}
			m_padding = GetPaddingForMaterial();
			SetVerticesDirty();
			base.m_ShouldRecalculateStencil = true;
			//((MaskableGraphic)this).RecalculateClipping();
			RecalculateClipping();
			//((MaskableGraphic)this).RecalculateMasking();
			RecalculateMasking();
		}

		private void ON_DRAG_AND_DROP_MATERIAL(GameObject obj, Material currentMaterial, Material newMaterial)
		{
#if UNITY_EDITOR
			if ((obj == base.gameObject)/* || PrefabUtility.GetPrefabParent(base.gameObject) == obj) && m_isDefaultMaterial*/)
			{
				if (m_canvasRenderer == null)
				{
					m_canvasRenderer = base.GetComponent<CanvasRenderer>();
				}
				//Undo.RecordObject((UnityEngine.Object)(object)this, "Material Assignment");
				//Undo.RecordObject(m_canvasRenderer, "Material Assignment");
				SetSharedMaterial(newMaterial);
				m_TextComponent.havePropertiesChanged = true;
			}
#endif
		}

		private void ON_SPRITE_ASSET_PROPERTY_CHANGED(bool isChanged, UnityEngine.Object obj)
		{
			if ((UnityEngine.Object)(object)m_TextComponent != null)
			{
				m_TextComponent.havePropertiesChanged = true;
			}
		}

		private void ON_FONT_PROPERTY_CHANGED(bool isChanged, TMP_FontAsset font)
		{
			if (font.GetInstanceID() == m_fontAsset.GetInstanceID() && m_fallbackMaterial != null)
			{
				m_fallbackMaterial.SetFloat(ShaderUtilities.ID_WeightNormal, m_fontAsset.normalStyle);
				m_fallbackMaterial.SetFloat(ShaderUtilities.ID_WeightBold, m_fontAsset.boldStyle);
			}
		}

		private void ON_TMP_SETTINGS_CHANGED()
		{
		}

		protected override void OnTransformParentChanged()
		{
			if (((UIBehaviour)this).IsActive())
			{
				base.m_ShouldRecalculateStencil = true;
				//((MaskableGraphic)this).RecalculateClipping();
				RecalculateClipping();
				//((MaskableGraphic)this).RecalculateMasking();
				RecalculateMasking();
			}
		}

		public override Material GetModifiedMaterial(Material baseMaterial)
		{
			Material material = baseMaterial;
			if (base.m_ShouldRecalculateStencil)
			{
				base.m_StencilValue = TMP_MaterialManager.GetStencilID(base.gameObject);
				base.m_ShouldRecalculateStencil = false;
			}
			if (base.m_StencilValue > 0)
			{
				material = TMP_MaterialManager.GetStencilMaterial(baseMaterial, base.m_StencilValue);
				if (base.m_MaskMaterial != null)
				{
					TMP_MaterialManager.ReleaseStencilMaterial(base.m_MaskMaterial);
				}
				base.m_MaskMaterial = material;
			}
			return material;
		}

		public float GetPaddingForMaterial()
		{
			return ShaderUtilities.GetPadding(m_sharedMaterial, m_TextComponent.extraPadding, m_TextComponent.isUsingBold);
		}

		public float GetPaddingForMaterial(Material mat)
		{
			return ShaderUtilities.GetPadding(mat, m_TextComponent.extraPadding, m_TextComponent.isUsingBold);
		}

		public void UpdateMeshPadding(bool isExtraPadding, bool isUsingBold)
		{
			m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, isExtraPadding, isUsingBold);
		}

		public override void SetAllDirty()
		{
		}

		public override void SetVerticesDirty()
		{
			if (((UIBehaviour)this).IsActive() && (UnityEngine.Object)(object)m_TextComponent != null)
			{
				m_TextComponent.havePropertiesChanged = true;
				((Graphic)m_TextComponent).SetVerticesDirty();
			}
		}

		public override void SetLayoutDirty()
		{
		}

		public override void SetMaterialDirty()
		{
			m_materialDirty = true;
			//((Graphic)this).UpdateMaterial();
			UpdateMaterial();
		}

		public void SetPivotDirty()
		{
			if (((UIBehaviour)this).IsActive())
			{
				((Graphic)this).rectTransform.pivot = m_TextComponent.rectTransform.pivot;
			}
		}

		public override void Cull(Rect clipRect, bool validRect)
		{
			if (!m_TextComponent.ignoreRectMaskCulling)
			{
				//((MaskableGraphic)this).Cull(clipRect, validRect);
				base.Cull(clipRect, validRect);
			}
		}

		protected override void UpdateGeometry()
		{
			Debug.Log("UpdateGeometry()");
		}

		public override void Rebuild(CanvasUpdate update)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Invalid comparison between Unknown and I4
			if ((int)update == 3 && m_materialDirty)
			{
				//((Graphic)this).UpdateMaterial();
				UpdateMaterial();
				m_materialDirty = false;
			}
		}

		public void RefreshMaterial()
		{
			UpdateMaterial();
			//((Graphic)this).UpdateMaterial();
		}

		protected override void UpdateMaterial()
		{
			if (m_canvasRenderer == null)
			{
				m_canvasRenderer = canvasRenderer;
			}
			m_canvasRenderer.materialCount = 1;
			m_canvasRenderer.SetMaterial(((Graphic)this).materialForRendering, 0);
			m_canvasRenderer.SetTexture(((Graphic)this).mainTexture);
			if (m_sharedMaterial != null && base.gameObject.name != "TMP SubMeshUI [" + m_sharedMaterial.name + "]")
			{
				base.gameObject.name = "TMP SubMeshUI [" + m_sharedMaterial.name + "]";
			}
		}

		public override void RecalculateClipping()
		{
			//((MaskableGraphic)this).RecalculateClipping();
			RecalculateClipping();
		}

		public override void RecalculateMasking()
		{
			base.m_ShouldRecalculateStencil = true;
			SetMaterialDirty();
		}

		private Material GetMaterial()
		{
			return m_sharedMaterial;
		}

		private Material GetMaterial(Material mat)
		{
			if (m_material == null || m_material.GetInstanceID() != mat.GetInstanceID())
			{
				m_material = CreateMaterialInstance(mat);
			}
			m_sharedMaterial = m_material;
			m_padding = GetPaddingForMaterial();
			SetVerticesDirty();
			SetMaterialDirty();
			return m_sharedMaterial;
		}

		private Material CreateMaterialInstance(Material source)
		{
			Material material = new Material(source);
			material.shaderKeywords = source.shaderKeywords;
			material.name += " (Instance)";
			return material;
		}

		private Material GetSharedMaterial()
		{
			if (m_canvasRenderer == null)
			{
				m_canvasRenderer = base.GetComponent<CanvasRenderer>();
			}
			return m_canvasRenderer.GetMaterial();
		}

		private void SetSharedMaterial(Material mat)
		{
			m_sharedMaterial = mat;
			base.m_Material = m_sharedMaterial;
			m_padding = GetPaddingForMaterial();
			SetMaterialDirty();
		}

		public TMP_SubMeshUI()
			
		{
		}
	}
}
