using System.IO;
using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	public class TMP_ContextMenus : Editor
	{
		private static Texture m_copiedTexture;

		private static Material m_copiedProperties;

		private static Material m_copiedAtlasProperties;

		[MenuItem("CONTEXT/Texture/Copy", false, 2000)]
		private static void CopyTexture(MenuCommand command)
		{
			m_copiedTexture = (command.context as Texture);
		}

		[MenuItem("CONTEXT/Material/Select Material", false, 500)]
		private static void SelectMaterial(MenuCommand command)
		{
			Material obj = command.context as Material;
			EditorUtility.FocusProjectWindow();
			EditorGUIUtility.PingObject(obj);
		}

		[MenuItem("CONTEXT/Material/Create Material Preset", false)]
		private static void DuplicateMaterial(MenuCommand command)
		{
			Material material = (Material)command.context;
			if (!EditorUtility.IsPersistent(material))
			{
				Debug.LogWarning("Material is an instance and cannot be converted into a permanent asset.");
				return;
			}
			string str = AssetDatabase.GetAssetPath(material).Split('.')[0];
			Material material2 = new Material(material);
			material2.shaderKeywords = material.shaderKeywords;
			AssetDatabase.CreateAsset(material2, AssetDatabase.GenerateUniqueAssetPath(str + ".mat"));
			if (Selection.activeGameObject != null)
			{
				TMP_Text component = Selection.activeGameObject.GetComponent<TMP_Text>();
				if ((Object)(object)component != null)
				{
					component.fontSharedMaterial = material2;
				}
				else
				{
					TMP_SubMesh component2 = Selection.activeGameObject.GetComponent<TMP_SubMesh>();
					if (component2 != null)
					{
						component2.sharedMaterial = material2;
					}
					else
					{
						TMP_SubMeshUI component3 = Selection.activeGameObject.GetComponent<TMP_SubMeshUI>();
						if ((Object)(object)component3 != null)
						{
							component3.sharedMaterial = material2;
						}
					}
				}
			}
			EditorUtility.FocusProjectWindow();
			EditorGUIUtility.PingObject(material2);
		}

		[MenuItem("CONTEXT/Material/Copy Material Properties", false)]
		private static void CopyMaterialProperties(MenuCommand command)
		{
			Material material = null;
			material = ((command.context.GetType() != typeof(Material)) ? Selection.activeGameObject.GetComponent<CanvasRenderer>().GetMaterial() : ((Material)command.context));
			m_copiedProperties = new Material(material);
			m_copiedProperties.shaderKeywords = material.shaderKeywords;
			m_copiedProperties.hideFlags = HideFlags.DontSave;
		}

		[MenuItem("CONTEXT/Material/Paste Material Properties", false)]
		private static void PasteMaterialProperties(MenuCommand command)
		{
			if (m_copiedProperties == null)
			{
				Debug.LogWarning("No Material Properties to Paste. Use Copy Material Properties first.");
				return;
			}
			Material material = null;
			material = ((command.context.GetType() != typeof(Material)) ? Selection.activeGameObject.GetComponent<CanvasRenderer>().GetMaterial() : ((Material)command.context));
			Undo.RecordObject(material, "Paste Material");
			ShaderUtilities.GetShaderPropertyIDs();
			if (material.HasProperty(ShaderUtilities.ID_GradientScale))
			{
				m_copiedProperties.SetTexture(ShaderUtilities.ID_MainTex, material.GetTexture(ShaderUtilities.ID_MainTex));
				m_copiedProperties.SetFloat(ShaderUtilities.ID_GradientScale, material.GetFloat(ShaderUtilities.ID_GradientScale));
				m_copiedProperties.SetFloat(ShaderUtilities.ID_TextureWidth, material.GetFloat(ShaderUtilities.ID_TextureWidth));
				m_copiedProperties.SetFloat(ShaderUtilities.ID_TextureHeight, material.GetFloat(ShaderUtilities.ID_TextureHeight));
			}
			EditorShaderUtilities.CopyMaterialProperties(m_copiedProperties, material);
			material.shaderKeywords = m_copiedProperties.shaderKeywords;
			TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, material);
		}

		[MenuItem("CONTEXT/Material/Reset", false, 2100)]
		private static void ResetSettings(MenuCommand command)
		{
			Material material = null;
			material = ((command.context.GetType() != typeof(Material)) ? Selection.activeGameObject.GetComponent<CanvasRenderer>().GetMaterial() : ((Material)command.context));
			Undo.RecordObject(material, "Reset Material");
			Material material2 = new Material(material.shader);
			ShaderUtilities.GetShaderPropertyIDs();
			if (material.HasProperty(ShaderUtilities.ID_GradientScale))
			{
				material2.SetTexture(ShaderUtilities.ID_MainTex, material.GetTexture(ShaderUtilities.ID_MainTex));
				material2.SetFloat(ShaderUtilities.ID_GradientScale, material.GetFloat(ShaderUtilities.ID_GradientScale));
				material2.SetFloat(ShaderUtilities.ID_TextureWidth, material.GetFloat(ShaderUtilities.ID_TextureWidth));
				material2.SetFloat(ShaderUtilities.ID_TextureHeight, material.GetFloat(ShaderUtilities.ID_TextureHeight));
				material2.SetFloat(ShaderUtilities.ID_StencilID, material.GetFloat(ShaderUtilities.ID_StencilID));
				material2.SetFloat(ShaderUtilities.ID_StencilComp, material.GetFloat(ShaderUtilities.ID_StencilComp));
				material.CopyPropertiesFromMaterial(material2);
				material.shaderKeywords = new string[0];
			}
			else
			{
				material.CopyPropertiesFromMaterial(material2);
			}
			Object.DestroyImmediate(material2);
			TMPro_EventManager.ON_MATERIAL_PROPERTY_CHANGED(true, material);
		}

		[MenuItem("CONTEXT/Material/Copy Atlas", false, 2000)]
		private static void CopyAtlas(MenuCommand command)
		{
			Material source = command.context as Material;
			m_copiedAtlasProperties = new Material(source);
			m_copiedAtlasProperties.hideFlags = HideFlags.DontSave;
		}

		[MenuItem("CONTEXT/Material/Paste Atlas", false, 2001)]
		private static void PasteAtlas(MenuCommand command)
		{
			Material material = command.context as Material;
			if (m_copiedAtlasProperties != null)
			{
				Undo.RecordObject(material, "Paste Texture");
				ShaderUtilities.GetShaderPropertyIDs();
				material.SetTexture(ShaderUtilities.ID_MainTex, m_copiedAtlasProperties.GetTexture(ShaderUtilities.ID_MainTex));
				material.SetFloat(ShaderUtilities.ID_GradientScale, m_copiedAtlasProperties.GetFloat(ShaderUtilities.ID_GradientScale));
				material.SetFloat(ShaderUtilities.ID_TextureWidth, m_copiedAtlasProperties.GetFloat(ShaderUtilities.ID_TextureWidth));
				material.SetFloat(ShaderUtilities.ID_TextureHeight, m_copiedAtlasProperties.GetFloat(ShaderUtilities.ID_TextureHeight));
			}
			else if (m_copiedTexture != null)
			{
				Undo.RecordObject(material, "Paste Texture");
				material.SetTexture(ShaderUtilities.ID_MainTex, m_copiedTexture);
			}
		}

		[MenuItem("CONTEXT/TMP_FontAsset/Extract Atlas", false, 2000)]
		private static void ExtractAtlas(MenuCommand command)
		{
			TMP_FontAsset tMP_FontAsset = command.context as TMP_FontAsset;
			string assetPath = AssetDatabase.GetAssetPath(tMP_FontAsset);
			string text = Path.GetDirectoryName(assetPath) + "/" + Path.GetFileNameWithoutExtension(assetPath) + " Atlas.png";
			SerializedObject serializedObject = new SerializedObject(tMP_FontAsset.material.GetTexture(ShaderUtilities.ID_MainTex));
			serializedObject.FindProperty("m_IsReadable").boolValue = true;
			serializedObject.ApplyModifiedProperties();
			Texture2D texture2D = Object.Instantiate(tMP_FontAsset.material.GetTexture(ShaderUtilities.ID_MainTex)) as Texture2D;
			serializedObject.FindProperty("m_IsReadable").boolValue = false;
			serializedObject.ApplyModifiedProperties();
			Debug.Log(text);
			byte[] bytes = texture2D.EncodeToPNG();
			File.WriteAllBytes(text, bytes);
			AssetDatabase.Refresh();
			Object.DestroyImmediate(texture2D);
		}
	}
}
