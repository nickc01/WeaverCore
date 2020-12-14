using UnityEditor;

namespace TMPro.EditorUtilities
{
	public class TMP_BitmapShaderGUI : TMP_BaseShaderGUI
	{
		private static MaterialPanel facePanel;

		private static MaterialPanel debugPanel;

		static TMP_BitmapShaderGUI()
		{
			facePanel = new MaterialPanel("Face", true);
			debugPanel = new MaterialPanel("Debug", false);
		}

		protected override void DoGUI()
		{
			if (DoPanelHeader(facePanel))
			{
				DoFacePanel();
			}
			if (DoPanelHeader(debugPanel))
			{
				DoDebugPanel();
			}
		}

		private void DoFacePanel()
		{
			EditorGUI.indentLevel++;
			if (material.HasProperty(ShaderUtilities.ID_FaceTex))
			{
				DoColor("_FaceColor", "Color");
				DoTexture2D("_FaceTex", "Texture", true);
			}
			else
			{
				DoColor("_Color", "Color");
				DoSlider("_DiffusePower", "Diffuse Power");
			}
			EditorGUI.indentLevel--;
		}

		private void DoDebugPanel()
		{
			EditorGUI.indentLevel++;
			DoTexture2D("_MainTex", "Font Atlas");
			if (material.HasProperty(ShaderUtilities.ID_VertexOffsetX))
			{
				DoEmptyLine();
				DoFloat("_VertexOffsetX", "Offset X");
				DoFloat("_VertexOffsetY", "Offset Y");
			}
			if (material.HasProperty(ShaderUtilities.ID_MaskSoftnessX))
			{
				DoEmptyLine();
				DoFloat("_MaskSoftnessX", "Softness X");
				DoFloat("_MaskSoftnessY", "Softness Y");
				DoVector("_ClipRect", "Clip Rect", TMP_BaseShaderGUI.lbrtVectorLabels);
			}
			if (material.HasProperty(ShaderUtilities.ID_StencilID))
			{
				DoEmptyLine();
				DoFloat("_Stencil", "Stencil ID");
				DoFloat("_StencilComp", "Stencil Comp");
			}
			EditorGUI.indentLevel--;
		}
	}
}
