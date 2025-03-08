using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TMPro
{
	public static class TMP_MaterialManager
	{
		private class FallbackMaterial
		{
			public int baseID;

			public Material baseMaterial;

			public long fallbackID;

			public Material fallbackMaterial;

			public int count;
		}

		private class MaskingMaterial
		{
			public Material baseMaterial;

			public Material stencilMaterial;

			public int count;

			public int stencilID;
		}

		private static List<MaskingMaterial> m_materialList;

		private static Dictionary<long, FallbackMaterial> m_fallbackMaterials;

		private static Dictionary<int, long> m_fallbackMaterialLookup;

		private static List<FallbackMaterial> m_fallbackCleanupList;

		private static bool isFallbackListDirty;

		static TMP_MaterialManager()
		{
			m_materialList = new List<MaskingMaterial>();
			m_fallbackMaterials = new Dictionary<long, FallbackMaterial>();
			m_fallbackMaterialLookup = new Dictionary<int, long>();
			m_fallbackCleanupList = new List<FallbackMaterial>();
			Camera.onPreRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPreRender, new Camera.CameraCallback(OnPreRender));
			Canvas.willRenderCanvases += OnPreRenderCanvas;
		}

		private static void OnPreRender(Camera cam)
		{
			if (isFallbackListDirty)
			{
				CleanupFallbackMaterials();
				isFallbackListDirty = false;
			}
		}

		private static void OnPreRenderCanvas()
		{
			if (isFallbackListDirty)
			{
				CleanupFallbackMaterials();
				isFallbackListDirty = false;
			}
		}

		public static Material GetStencilMaterial(Material baseMaterial, int stencilID)
		{
			if (!baseMaterial.HasProperty(ShaderUtilities.ID_StencilID))
			{
				Debug.LogWarning("Selected Shader does not support Stencil Masking. Please select the Distance Field or Mobile Distance Field Shader.");
				return baseMaterial;
			}
			int instanceID = baseMaterial.GetInstanceID();
			for (int i = 0; i < m_materialList.Count; i++)
			{
				if (m_materialList[i].baseMaterial.GetInstanceID() == instanceID && m_materialList[i].stencilID == stencilID)
				{
					m_materialList[i].count++;
					return m_materialList[i].stencilMaterial;
				}
			}
			Material material = new Material(baseMaterial);
			material.hideFlags = HideFlags.HideAndDontSave;
			material.name = material.name + " Masking ID:" + stencilID;
			material.shaderKeywords = baseMaterial.shaderKeywords;
			ShaderUtilities.GetShaderPropertyIDs();
			material.SetFloat(ShaderUtilities.ID_StencilID, stencilID);
			material.SetFloat(ShaderUtilities.ID_StencilComp, 4f);
			MaskingMaterial maskingMaterial = new MaskingMaterial();
			maskingMaterial.baseMaterial = baseMaterial;
			maskingMaterial.stencilMaterial = material;
			maskingMaterial.stencilID = stencilID;
			maskingMaterial.count = 1;
			m_materialList.Add(maskingMaterial);
			return material;
		}

		public static void ReleaseStencilMaterial(Material stencilMaterial)
		{
			int instanceID = stencilMaterial.GetInstanceID();
			int num = 0;
			while (true)
			{
				if (num < m_materialList.Count)
				{
					if (m_materialList[num].stencilMaterial.GetInstanceID() == instanceID)
					{
						break;
					}
					num++;
					continue;
				}
				return;
			}
			if (m_materialList[num].count > 1)
			{
				m_materialList[num].count--;
				return;
			}
			UnityEngine.Object.DestroyImmediate(m_materialList[num].stencilMaterial);
			m_materialList.RemoveAt(num);
			stencilMaterial = null;
		}

		public static Material GetBaseMaterial(Material stencilMaterial)
		{
			int num = m_materialList.FindIndex((MaskingMaterial item) => item.stencilMaterial == stencilMaterial);
			if (num == -1)
			{
				return null;
			}
			return m_materialList[num].baseMaterial;
		}

		public static Material SetStencil(Material material, int stencilID)
		{
			material.SetFloat(ShaderUtilities.ID_StencilID, stencilID);
			if (stencilID == 0)
			{
				material.SetFloat(ShaderUtilities.ID_StencilComp, 8f);
			}
			else
			{
				material.SetFloat(ShaderUtilities.ID_StencilComp, 4f);
			}
			return material;
		}

		public static void AddMaskingMaterial(Material baseMaterial, Material stencilMaterial, int stencilID)
		{
			int num = m_materialList.FindIndex((MaskingMaterial item) => item.stencilMaterial == stencilMaterial);
			if (num == -1)
			{
				MaskingMaterial maskingMaterial = new MaskingMaterial();
				maskingMaterial.baseMaterial = baseMaterial;
				maskingMaterial.stencilMaterial = stencilMaterial;
				maskingMaterial.stencilID = stencilID;
				maskingMaterial.count = 1;
				m_materialList.Add(maskingMaterial);
			}
			else
			{
				stencilMaterial = m_materialList[num].stencilMaterial;
				m_materialList[num].count++;
			}
		}

		public static void RemoveStencilMaterial(Material stencilMaterial)
		{
			int num = m_materialList.FindIndex((MaskingMaterial item) => item.stencilMaterial == stencilMaterial);
			if (num != -1)
			{
				m_materialList.RemoveAt(num);
			}
		}

		public static void ReleaseBaseMaterial(Material baseMaterial)
		{
			int num = m_materialList.FindIndex((MaskingMaterial item) => item.baseMaterial == baseMaterial);
			if (num == -1)
			{
				Debug.Log("No Masking Material exists for " + baseMaterial.name);
			}
			else if (m_materialList[num].count > 1)
			{
				m_materialList[num].count--;
				Debug.Log("Removed (1) reference to " + m_materialList[num].stencilMaterial.name + ". There are " + m_materialList[num].count + " references left.");
			}
			else
			{
				Debug.Log("Removed last reference to " + m_materialList[num].stencilMaterial.name + " with ID " + m_materialList[num].stencilMaterial.GetInstanceID());
				UnityEngine.Object.DestroyImmediate(m_materialList[num].stencilMaterial);
				m_materialList.RemoveAt(num);
			}
		}

		public static void ClearMaterials()
		{
			if (m_materialList.Count == 0)
			{
				Debug.Log("Material List has already been cleared.");
				return;
			}
			for (int i = 0; i < m_materialList.Count; i++)
			{
				Material stencilMaterial = m_materialList[i].stencilMaterial;
				UnityEngine.Object.DestroyImmediate(stencilMaterial);
				m_materialList.RemoveAt(i);
			}
		}

		public static int GetStencilID(GameObject obj)
		{
			int num = 0;
			List<Mask> list = TMP_ListPool<Mask>.Get();
			obj.GetComponentsInParent(false, list);
			for (int i = 0; i < list.Count; i++)
			{
				if (((UIBehaviour)list[i]).IsActive())
				{
					num++;
				}
			}
			TMP_ListPool<Mask>.Release(list);
			return Mathf.Min((1 << num) - 1, 255);
		}

		public static Material GetFallbackMaterial(Material sourceMaterial, Material targetMaterial)
		{
			int instanceID = sourceMaterial.GetInstanceID();
			Texture texture = targetMaterial.GetTexture(ShaderUtilities.ID_MainTex);
			int instanceID2 = texture.GetInstanceID();
			long num = ((long)instanceID << 32) | (uint)instanceID2;
			FallbackMaterial value;
			if (m_fallbackMaterials.TryGetValue(num, out value))
			{
				return value.fallbackMaterial;
			}
			Material material = null;
			if (sourceMaterial.HasProperty(ShaderUtilities.ID_GradientScale) && targetMaterial.HasProperty(ShaderUtilities.ID_GradientScale))
			{
				material = new Material(sourceMaterial);
				material.hideFlags = HideFlags.HideAndDontSave;
				Material material2 = material;
				material2.name = material2.name + " + " + texture.name;
				material.SetTexture(ShaderUtilities.ID_MainTex, texture);
				material.SetFloat(ShaderUtilities.ID_GradientScale, targetMaterial.GetFloat(ShaderUtilities.ID_GradientScale));
				material.SetFloat(ShaderUtilities.ID_TextureWidth, targetMaterial.GetFloat(ShaderUtilities.ID_TextureWidth));
				material.SetFloat(ShaderUtilities.ID_TextureHeight, targetMaterial.GetFloat(ShaderUtilities.ID_TextureHeight));
				material.SetFloat(ShaderUtilities.ID_WeightNormal, targetMaterial.GetFloat(ShaderUtilities.ID_WeightNormal));
				material.SetFloat(ShaderUtilities.ID_WeightBold, targetMaterial.GetFloat(ShaderUtilities.ID_WeightBold));
			}
			else
			{
				material = new Material(targetMaterial);
			}
			value = new FallbackMaterial();
			value.baseID = instanceID;
			value.baseMaterial = sourceMaterial;
			value.fallbackID = num;
			value.fallbackMaterial = material;
			value.count = 0;
			m_fallbackMaterials.Add(num, value);
			m_fallbackMaterialLookup.Add(material.GetInstanceID(), num);
			return material;
		}

		public static void AddFallbackMaterialReference(Material targetMaterial)
		{
			if (!(targetMaterial == null))
			{
				int instanceID = targetMaterial.GetInstanceID();
				long value;
				FallbackMaterial value2;
				if (m_fallbackMaterialLookup.TryGetValue(instanceID, out value) && m_fallbackMaterials.TryGetValue(value, out value2))
				{
					value2.count++;
				}
			}
		}

		public static void RemoveFallbackMaterialReference(Material targetMaterial)
		{
			if (targetMaterial == null)
			{
				return;
			}
			int instanceID = targetMaterial.GetInstanceID();
			long value;
			FallbackMaterial value2;
			if (m_fallbackMaterialLookup.TryGetValue(instanceID, out value) && m_fallbackMaterials.TryGetValue(value, out value2))
			{
				value2.count--;
				if (value2.count < 1)
				{
					m_fallbackCleanupList.Add(value2);
				}
			}
		}

		public static void CleanupFallbackMaterials()
		{
			if (m_fallbackCleanupList.Count == 0)
			{
				return;
			}
			for (int i = 0; i < m_fallbackCleanupList.Count; i++)
			{
				FallbackMaterial fallbackMaterial = m_fallbackCleanupList[i];
				if (fallbackMaterial.count < 1)
				{
					Material fallbackMaterial2 = fallbackMaterial.fallbackMaterial;
					m_fallbackMaterials.Remove(fallbackMaterial.fallbackID);
					m_fallbackMaterialLookup.Remove(fallbackMaterial2.GetInstanceID());
					UnityEngine.Object.DestroyImmediate(fallbackMaterial2);
					fallbackMaterial2 = null;
				}
			}
			m_fallbackCleanupList.Clear();
		}

		public static void ReleaseFallbackMaterial(Material fallackMaterial)
		{
			if (fallackMaterial == null)
			{
				return;
			}
			int instanceID = fallackMaterial.GetInstanceID();
			long value;
			FallbackMaterial value2;
			if (m_fallbackMaterialLookup.TryGetValue(instanceID, out value) && m_fallbackMaterials.TryGetValue(value, out value2))
			{
				value2.count--;
				if (value2.count < 1)
				{
					m_fallbackCleanupList.Add(value2);
				}
			}
			isFallbackListDirty = true;
		}

		public static void CopyMaterialPresetProperties(Material source, Material destination)
		{
			if (source.HasProperty(ShaderUtilities.ID_GradientScale) && destination.HasProperty(ShaderUtilities.ID_GradientScale))
			{
				Texture texture = destination.GetTexture(ShaderUtilities.ID_MainTex);
				float @float = destination.GetFloat(ShaderUtilities.ID_GradientScale);
				float float2 = destination.GetFloat(ShaderUtilities.ID_TextureWidth);
				float float3 = destination.GetFloat(ShaderUtilities.ID_TextureHeight);
				float float4 = destination.GetFloat(ShaderUtilities.ID_WeightNormal);
				float float5 = destination.GetFloat(ShaderUtilities.ID_WeightBold);
				destination.CopyPropertiesFromMaterial(source);
				destination.shaderKeywords = source.shaderKeywords;
				destination.SetTexture(ShaderUtilities.ID_MainTex, texture);
				destination.SetFloat(ShaderUtilities.ID_GradientScale, @float);
				destination.SetFloat(ShaderUtilities.ID_TextureWidth, float2);
				destination.SetFloat(ShaderUtilities.ID_TextureHeight, float3);
				destination.SetFloat(ShaderUtilities.ID_WeightNormal, float4);
				destination.SetFloat(ShaderUtilities.ID_WeightBold, float5);
			}
		}
	}
}
