using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	public static class EditorShaderUtilities
	{
		public static void CopyMaterialProperties(Material source, Material destination)
		{
			MaterialProperty[] materialProperties = MaterialEditor.GetMaterialProperties(new Material[1]
			{
				source
			});
			for (int i = 0; i < materialProperties.Length; i++)
			{
				int nameID = Shader.PropertyToID(materialProperties[i].name);
				if (destination.HasProperty(nameID))
				{
					switch (ShaderUtil.GetPropertyType(source.shader, i))
					{
					case ShaderUtil.ShaderPropertyType.Color:
						destination.SetColor(nameID, source.GetColor(nameID));
						break;
					case ShaderUtil.ShaderPropertyType.Float:
						destination.SetFloat(nameID, source.GetFloat(nameID));
						break;
					case ShaderUtil.ShaderPropertyType.Range:
						destination.SetFloat(nameID, source.GetFloat(nameID));
						break;
					case ShaderUtil.ShaderPropertyType.TexEnv:
						destination.SetTexture(nameID, source.GetTexture(nameID));
						break;
					case ShaderUtil.ShaderPropertyType.Vector:
						destination.SetVector(nameID, source.GetVector(nameID));
						break;
					}
				}
			}
		}
	}
}
