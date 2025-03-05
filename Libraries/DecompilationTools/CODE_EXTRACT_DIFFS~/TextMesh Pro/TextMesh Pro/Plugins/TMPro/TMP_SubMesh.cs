using System;
#if UNITY_EDITOR
//using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace TMPro
{
	[ExecuteInEditMode]
	[RequireComponent(typeof(MeshRenderer))]
	[RequireComponent(typeof(MeshFilter))]
	public class TMP_SubMesh : MonoBehaviour
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
		private Renderer m_renderer;

		[SerializeField]
		private MeshFilter m_meshFilter;

		private Mesh m_mesh;

		[SerializeField]
		private BoxCollider m_boxCollider;

		[SerializeField]
		private TextMeshPro m_TextComponent;

		[NonSerialized]
		private bool m_isRegisteredForEvents;

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

		public Material material
		{
			get
			{
				return GetMaterial(m_sharedMaterial);
			}
			set
			{
				if (m_sharedMaterial.GetInstanceID() != value.GetInstanceID())
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

		public new Renderer renderer
		{
			get
			{
				if (m_renderer == null)
				{
					m_renderer = GetComponent<Renderer>();
				}
				return m_renderer;
			}
		}

		public MeshFilter meshFilter
		{
			get
			{
				if (m_meshFilter == null)
				{
					m_meshFilter = GetComponent<MeshFilter>();
				}
				return m_meshFilter;
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
					meshFilter.mesh = m_mesh;
				}
				return m_mesh;
			}
			set
			{
				m_mesh = value;
			}
		}

		public BoxCollider boxCollider
		{
			get
			{
				if (m_boxCollider == null)
				{
					m_boxCollider = GetComponent<BoxCollider>();
					if (m_boxCollider == null)
					{
						m_boxCollider = base.gameObject.AddComponent<BoxCollider>();
						base.gameObject.AddComponent<Rigidbody>();
					}
				}
				return m_boxCollider;
			}
		}

		private void OnEnable()
		{
			if (!m_isRegisteredForEvents)
			{
				TMPro_EventManager.MATERIAL_PROPERTY_EVENT.Add(ON_MATERIAL_PROPERTY_CHANGED);
				TMPro_EventManager.FONT_PROPERTY_EVENT.Add(ON_FONT_PROPERTY_CHANGED);
				TMPro_EventManager.DRAG_AND_DROP_MATERIAL_EVENT.Add(ON_DRAG_AND_DROP_MATERIAL);
				TMPro_EventManager.SPRITE_ASSET_PROPERTY_EVENT.Add(ON_SPRITE_ASSET_PROPERTY_CHANGED);
				m_isRegisteredForEvents = true;
			}
			meshFilter.sharedMesh = mesh;
			if (m_sharedMaterial != null)
			{
				m_sharedMaterial.SetVector(ShaderUtilities.ID_ClipRect, new Vector4(-32767f, -32767f, 32767f, 32767f));
			}
		}

		private void OnDisable()
		{
			m_meshFilter.sharedMesh = null;
			if (m_fallbackMaterial != null)
			{
				TMP_MaterialManager.ReleaseFallbackMaterial(m_fallbackMaterial);
				m_fallbackMaterial = null;
			}
		}

		private void OnDestroy()
		{
			if (m_mesh != null)
			{
				UnityEngine.Object.DestroyImmediate(m_mesh);
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
		}

		private void ON_MATERIAL_PROPERTY_CHANGED(bool isChanged, Material mat)
		{
			int instanceID = mat.GetInstanceID();
			int instanceID2 = m_sharedMaterial.GetInstanceID();
			int num = (!(m_fallbackSourceMaterial == null)) ? m_fallbackSourceMaterial.GetInstanceID() : 0;
			if (instanceID != instanceID2)
			{
				if (!(m_fallbackMaterial != null) || num != instanceID)
				{
					return;
				}
				TMP_MaterialManager.CopyMaterialPresetProperties(mat, m_fallbackMaterial);
			}
			if ((UnityEngine.Object)(object)m_TextComponent == null)
			{
				m_TextComponent = GetComponentInParent<TextMeshPro>();
			}
			m_padding = GetPaddingForMaterial();
			m_TextComponent.havePropertiesChanged = true;
			((Graphic)m_TextComponent).SetVerticesDirty();
		}

		private void ON_DRAG_AND_DROP_MATERIAL(GameObject obj, Material currentMaterial, Material newMaterial)
		{
#if UNITY_EDITOR
			if ((obj == base.gameObject)/* || PrefabUtility.GetPrefabParent(base.gameObject) == obj)*/ && m_isDefaultMaterial)
			{
				if (m_renderer == null)
				{
					m_renderer = GetComponent<Renderer>();
				}
				//Undo.RecordObject(this, "Material Assignment");
				//Undo.RecordObject(m_renderer, "Material Assignment");
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

		public static TMP_SubMesh AddSubTextObject(TextMeshPro textComponent, MaterialReference materialReference)
		{
			GameObject gameObject = new GameObject("TMP SubMesh [" + materialReference.material.name + "]", typeof(TMP_SubMesh));
			TMP_SubMesh component = gameObject.GetComponent<TMP_SubMesh>();
			gameObject.transform.SetParent(textComponent.transform, false);
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.transform.localScale = Vector3.one;
			gameObject.layer = ((Component)(object)textComponent).gameObject.layer;
			component.m_meshFilter = gameObject.GetComponent<MeshFilter>();
			component.m_TextComponent = textComponent;
			component.m_fontAsset = materialReference.fontAsset;
			component.m_spriteAsset = materialReference.spriteAsset;
			component.m_isDefaultMaterial = materialReference.isDefaultMaterial;
			component.SetSharedMaterial(materialReference.material);
			component.renderer.sortingLayerID = textComponent.renderer.sortingLayerID;
			component.renderer.sortingOrder = textComponent.renderer.sortingOrder;
			return component;
		}

		public void DestroySelf()
		{
			UnityEngine.Object.Destroy(base.gameObject, 1f);
		}

		private Material GetMaterial(Material mat)
		{
			if (m_renderer == null)
			{
				m_renderer = GetComponent<Renderer>();
			}
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
			if (m_renderer == null)
			{
				m_renderer = GetComponent<Renderer>();
			}
			return m_renderer.sharedMaterial;
		}

		private void SetSharedMaterial(Material mat)
		{
			m_sharedMaterial = mat;
			m_padding = GetPaddingForMaterial();
			SetMaterialDirty();
			if (m_sharedMaterial != null)
			{
				base.gameObject.name = "TMP SubMesh [" + m_sharedMaterial.name + "]";
			}
		}

		public float GetPaddingForMaterial()
		{
			return ShaderUtilities.GetPadding(m_sharedMaterial, m_TextComponent.extraPadding, m_TextComponent.isUsingBold);
		}

		public void UpdateMeshPadding(bool isExtraPadding, bool isUsingBold)
		{
			m_padding = ShaderUtilities.GetPadding(m_sharedMaterial, isExtraPadding, isUsingBold);
		}

		public void SetVerticesDirty()
		{
			if (base.enabled && (UnityEngine.Object)(object)m_TextComponent != null)
			{
				m_TextComponent.havePropertiesChanged = true;
				((Graphic)m_TextComponent).SetVerticesDirty();
			}
		}

		public void SetMaterialDirty()
		{
			UpdateMaterial();
		}

		protected void UpdateMaterial()
		{
			if (m_renderer == null)
			{
				m_renderer = renderer;
			}
			m_renderer.sharedMaterial = m_sharedMaterial;
			if (m_sharedMaterial != null && base.gameObject.name != "TMP SubMesh [" + m_sharedMaterial.name + "]")
			{
				base.gameObject.name = "TMP SubMesh [" + m_sharedMaterial.name + "]";
			}
		}

		public void UpdateColliders(int vertexCount)
		{
			if (!(boxCollider == null))
			{
				Vector2 mAX_16BIT = TMP_Math.MAX_16BIT;
				Vector2 mIN_16BIT = TMP_Math.MIN_16BIT;
				for (int i = 0; i < vertexCount; i++)
				{
					mAX_16BIT.x = Mathf.Min(mAX_16BIT.x, m_mesh.vertices[i].x);
					mAX_16BIT.y = Mathf.Min(mAX_16BIT.y, m_mesh.vertices[i].y);
					mIN_16BIT.x = Mathf.Max(mIN_16BIT.x, m_mesh.vertices[i].x);
					mIN_16BIT.y = Mathf.Max(mIN_16BIT.y, m_mesh.vertices[i].y);
				}
				Vector3 center = (mAX_16BIT + mIN_16BIT) / 2f;
				Vector3 size = mIN_16BIT - mAX_16BIT;
				size.z = 0.1f;
				boxCollider.center = center;
				boxCollider.size = size;
			}
		}
	}
}
