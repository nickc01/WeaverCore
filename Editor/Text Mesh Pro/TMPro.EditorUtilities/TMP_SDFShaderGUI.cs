using UnityEditor;
using UnityEngine;

namespace TMPro.EditorUtilities
{
	public class TMP_SDFShaderGUI : TMP_BaseShaderGUI
	{
		private static MaterialPanel facePanel;

		private static MaterialPanel outlinePanel;

		private static MaterialPanel underlayPanel;

		private static MaterialPanel bevelPanel;

		private static MaterialPanel lightingPanel;

		private static MaterialPanel bumpMapPanel;

		private static MaterialPanel envMapPanel;

		private static MaterialPanel glowPanel;

		private static MaterialPanel debugPanel;

		private static ShaderFeature outlineFeature;

		private static ShaderFeature underlayFeature;

		private static ShaderFeature bevelFeature;

		private static ShaderFeature glowFeature;

		private static ShaderFeature maskFeature;

		private static string[] faceUVSpeedNames;

		private static string[] outlineUVSpeedNames;

		private static GUIContent[] bevelTypeLabels;

		static TMP_SDFShaderGUI()
		{
			faceUVSpeedNames = new string[2]
			{
				"_FaceUVSpeedX",
				"_FaceUVSpeedY"
			};
			outlineUVSpeedNames = new string[2]
			{
				"_OutlineUVSpeedX",
				"_OutlineUVSpeedY"
			};
			bevelTypeLabels = new GUIContent[2]
			{
				new GUIContent("Outer Bevel"),
				new GUIContent("Inner Bevel")
			};
			facePanel = new MaterialPanel("Face", true);
			outlinePanel = new MaterialPanel("Outline", true);
			underlayPanel = new MaterialPanel("Underlay", false);
			bevelPanel = new MaterialPanel("Bevel", false);
			lightingPanel = new MaterialPanel("Lighting", false);
			bumpMapPanel = new MaterialPanel("BumpMap", false);
			envMapPanel = new MaterialPanel("EnvMap", false);
			glowPanel = new MaterialPanel("Glow", false);
			debugPanel = new MaterialPanel("Debug", false);
			outlineFeature = new ShaderFeature
			{
				undoLabel = "Outline",
				keywords = new string[1]
				{
					"OUTLINE_ON"
				}
			};
			underlayFeature = new ShaderFeature
			{
				undoLabel = "Underlay",
				keywords = new string[2]
				{
					"UNDERLAY_ON",
					"UNDERLAY_INNER"
				},
				label = new GUIContent("Underlay Type"),
				keywordLabels = new GUIContent[3]
				{
					new GUIContent("None"),
					new GUIContent("Normal"),
					new GUIContent("Inner")
				}
			};
			bevelFeature = new ShaderFeature
			{
				undoLabel = "Bevel",
				keywords = new string[1]
				{
					"BEVEL_ON"
				}
			};
			glowFeature = new ShaderFeature
			{
				undoLabel = "Glow",
				keywords = new string[1]
				{
					"GLOW_ON"
				}
			};
			maskFeature = new ShaderFeature
			{
				undoLabel = "Mask",
				keywords = new string[2]
				{
					"MASK_HARD",
					"MASK_SOFT"
				},
				label = new GUIContent("Mask"),
				keywordLabels = new GUIContent[3]
				{
					new GUIContent("Mask Off"),
					new GUIContent("Mask Hard"),
					new GUIContent("Mask Soft")
				}
			};
		}

		protected override void DoGUI()
		{
			if (DoPanelHeader(facePanel))
			{
				DoFacePanel();
			}
			if (material.HasProperty(ShaderUtilities.ID_OutlineTex) ? DoPanelHeader(outlinePanel) : DoPanelHeader(outlinePanel, outlineFeature))
			{
				DoOutlinePanel();
			}
			if (material.HasProperty(ShaderUtilities.ID_UnderlayColor) && DoPanelHeader(underlayPanel, underlayFeature))
			{
				DoUnderlayPanel();
			}
			if (material.HasProperty("_SpecularColor"))
			{
				if (DoPanelHeader(bevelPanel, bevelFeature))
				{
					DoBevelPanel();
				}
				if (DoPanelHeader(lightingPanel, bevelFeature, false))
				{
					DoLocalLightingPanel();
				}
				if (DoPanelHeader(bumpMapPanel, bevelFeature, false))
				{
					DoBumpMapPanel();
				}
				if (DoPanelHeader(envMapPanel, bevelFeature, false))
				{
					DoEnvMapPanel();
				}
			}
			else if (material.HasProperty("_SpecColor"))
			{
				if (DoPanelHeader(bevelPanel))
				{
					DoBevelPanel();
				}
				if (DoPanelHeader(lightingPanel))
				{
					DoSurfaceLightingPanel();
				}
				if (DoPanelHeader(bumpMapPanel))
				{
					DoBumpMapPanel();
				}
				if (DoPanelHeader(envMapPanel))
				{
					DoEnvMapPanel();
				}
			}
			if (material.HasProperty(ShaderUtilities.ID_GlowColor) && DoPanelHeader(glowPanel, glowFeature))
			{
				DoGlowPanel();
			}
			if (DoPanelHeader(debugPanel))
			{
				DoDebugPanel();
			}
		}

		private void DoFacePanel()
		{
			EditorGUI.indentLevel++;
			DoColor("_FaceColor", "Color");
			if (material.HasProperty(ShaderUtilities.ID_FaceTex))
			{
				if (material.HasProperty("_FaceUVSpeedX"))
				{
					DoTexture2D("_FaceTex", "Texture", true, faceUVSpeedNames);
				}
				else
				{
					DoTexture2D("_FaceTex", "Texture", true);
				}
			}
			DoSlider("_OutlineSoftness", "Softness");
			DoSlider("_FaceDilate", "Dilate");
			if (material.HasProperty(ShaderUtilities.ID_Shininess))
			{
				DoSlider("_FaceShininess", "Gloss");
			}
			EditorGUI.indentLevel--;
		}

		private void DoOutlinePanel()
		{
			EditorGUI.indentLevel++;
			DoColor("_OutlineColor", "Color");
			if (material.HasProperty(ShaderUtilities.ID_OutlineTex))
			{
				if (material.HasProperty("_OutlineUVSpeedX"))
				{
					DoTexture2D("_OutlineTex", "Texture", true, outlineUVSpeedNames);
				}
				else
				{
					DoTexture2D("_OutlineTex", "Texture", true);
				}
			}
			DoSlider("_OutlineWidth", "Thickness");
			if (material.HasProperty("_OutlineShininess"))
			{
				DoSlider("_OutlineShininess", "Gloss");
			}
			EditorGUI.indentLevel--;
		}

		private void DoUnderlayPanel()
		{
			EditorGUI.indentLevel++;
			underlayFeature.DoPopup(editor, material);
			DoColor("_UnderlayColor", "Color");
			DoSlider("_UnderlayOffsetX", "Offset X");
			DoSlider("_UnderlayOffsetY", "Offset Y");
			DoSlider("_UnderlayDilate", "Dilate");
			DoSlider("_UnderlaySoftness", "Softness");
			EditorGUI.indentLevel--;
		}

		private void DoBevelPanel()
		{
			EditorGUI.indentLevel++;
			DoPopup("_ShaderFlags", "Type", bevelTypeLabels);
			DoSlider("_Bevel", "Amount");
			DoSlider("_BevelOffset", "Offset");
			DoSlider("_BevelWidth", "Width");
			DoSlider("_BevelRoundness", "Roundness");
			DoSlider("_BevelClamp", "Clamp");
			EditorGUI.indentLevel--;
		}

		private void DoLocalLightingPanel()
		{
			EditorGUI.indentLevel++;
			DoSlider("_LightAngle", "Light Angle");
			DoColor("_SpecularColor", "Specular Color");
			DoSlider("_SpecularPower", "Specular Power");
			DoSlider("_Reflectivity", "Reflectivity Power");
			DoSlider("_Diffuse", "Diffuse Shadow");
			DoSlider("_Ambient", "Ambient Shadow");
			EditorGUI.indentLevel--;
		}

		private void DoSurfaceLightingPanel()
		{
			EditorGUI.indentLevel++;
			DoColor("_SpecColor", "Specular Color");
			EditorGUI.indentLevel--;
		}

		private void DoBumpMapPanel()
		{
			EditorGUI.indentLevel++;
			DoTexture2D("_BumpMap", "Texture");
			DoSlider("_BumpFace", "Face");
			DoSlider("_BumpOutline", "Outline");
			EditorGUI.indentLevel--;
		}

		private void DoEnvMapPanel()
		{
			EditorGUI.indentLevel++;
			DoColor("_ReflectFaceColor", "Face Color");
			DoColor("_ReflectOutlineColor", "Outline Color");
			DoCubeMap("_Cube", "Texture");
			DoVector3("_EnvMatrixRotation", "EnvMap Rotation");
			EditorGUI.indentLevel--;
		}

		private void DoGlowPanel()
		{
			EditorGUI.indentLevel++;
			DoColor("_GlowColor", "Color");
			DoSlider("_GlowOffset", "Offset");
			DoSlider("_GlowInner", "Inner");
			DoSlider("_GlowOuter", "Outer");
			DoSlider("_GlowPower", "Power");
			EditorGUI.indentLevel--;
		}

		private void DoDebugPanel()
		{
			EditorGUI.indentLevel++;
			DoTexture2D("_MainTex", "Font Atlas");
			DoFloat("_GradientScale", "Gradient Scale");
			DoFloat("_TextureWidth", "Texture Width");
			DoFloat("_TextureHeight", "Texture Height");
			DoEmptyLine();
			DoFloat("_ScaleX", "Scale X");
			DoFloat("_ScaleY", "Scale Y");
			DoSlider("_PerspectiveFilter", "Perspective Filter");
			DoEmptyLine();
			DoFloat("_VertexOffsetX", "Offset X");
			DoFloat("_VertexOffsetY", "Offset Y");
			if (material.HasProperty(ShaderUtilities.ID_MaskCoord))
			{
				DoEmptyLine();
				maskFeature.ReadState(material);
				maskFeature.DoPopup(editor, material);
				if (maskFeature.Active)
				{
					DoMaskSubgroup();
				}
				DoEmptyLine();
				DoVector("_ClipRect", "Clip Rect", TMP_BaseShaderGUI.lbrtVectorLabels);
			}
			else if (material.HasProperty("_MaskTex"))
			{
				DoMaskTexSubgroup();
			}
			else if (material.HasProperty(ShaderUtilities.ID_MaskSoftnessX))
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
			DoEmptyLine();
			EditorGUI.BeginChangeCheck();
			bool flag = EditorGUILayout.Toggle("Use Ratios?", !material.IsKeywordEnabled("RATIOS_OFF"));
			if (EditorGUI.EndChangeCheck())
			{
				editor.RegisterPropertyChangeUndo("Use Ratios");
				if (flag)
				{
					material.DisableKeyword("RATIOS_OFF");
				}
				else
				{
					material.EnableKeyword("RATIOS_OFF");
				}
			}
			EditorGUI.BeginDisabledGroup(true);
			DoFloat("_ScaleRatioA", "Scale Ratio A");
			DoFloat("_ScaleRatioB", "Scale Ratio B");
			DoFloat("_ScaleRatioC", "Scale Ratio C");
			EditorGUI.EndDisabledGroup();
			EditorGUI.indentLevel--;
		}

		private void DoMaskSubgroup()
		{
			DoVector("_MaskCoord", "Mask Bounds", TMP_BaseShaderGUI.xywhVectorLabels);
			if (Selection.activeGameObject != null)
			{
				Renderer component = Selection.activeGameObject.GetComponent<Renderer>();
				if (component != null)
				{
					Rect controlRect = EditorGUILayout.GetControlRect();
					controlRect.x += EditorGUIUtility.labelWidth;
					controlRect.width -= EditorGUIUtility.labelWidth;
					if (GUI.Button(controlRect, "Match Renderer Bounds"))
					{
						ShaderGUI.FindProperty("_MaskCoord", properties).vectorValue = new Vector4(0f, 0f, Mathf.Round(component.bounds.extents.x * 1000f) / 1000f, Mathf.Round(component.bounds.extents.y * 1000f) / 1000f);
					}
				}
			}
			if (maskFeature.State == 1)
			{
				DoFloat("_MaskSoftnessX", "Softness X");
				DoFloat("_MaskSoftnessY", "Softness Y");
			}
		}

		private void DoMaskTexSubgroup()
		{
			DoEmptyLine();
			DoTexture2D("_MaskTex", "Mask Texture");
			DoToggle("_MaskInverse", "Inverse Mask");
			DoColor("_MaskEdgeColor", "Edge Color");
			DoSlider("_MaskEdgeSoftness", "Edge Softness");
			DoSlider("_MaskWipeControl", "Wipe Position");
			DoFloat("_MaskSoftnessX", "Softness X");
			DoFloat("_MaskSoftnessY", "Softness Y");
			DoVector("_ClipRect", "Clip Rect", TMP_BaseShaderGUI.lbrtVectorLabels);
		}
	}
}
