using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Assets.TMPro
{
	/*
	 * THIS REPLACES THE DEFAULT TMP_FontAsset OBJECT TO MAKE IT WORK WITH ASSET BUNDLES
	 */

#if UNITY_EDITOR
	[System.ComponentModel.Browsable(false)]
	[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
#endif
	public class TMP_FontAsset : global::TMPro.TMP_FontAsset
	{
		[SerializeField]
		
		string FaceInfo_Name;
		[SerializeField]
		
		float FaceInfo_PointSize;
		[SerializeField]
		
		float FaceInfo_Scale;
		[SerializeField]
		
		int FaceInfo_CharacterCount;
		[SerializeField]
		
		float FaceInfo_LineHeight;
		[SerializeField]
		
		float FaceInfo_Baseline;
		[SerializeField]
		
		float FaceInfo_Ascender;
		[SerializeField]
		
		float FaceInfo_CapHeight;
		[SerializeField]
		
		float FaceInfo_Descender;
		[SerializeField]
		
		float FaceInfo_CenterLine;
		[SerializeField]
		
		float FaceInfo_SuperscriptOffset;
		[SerializeField]
		
		float FaceInfo_SubscriptOffset;
		[SerializeField]
		
		float FaceInfo_SubSize;
		[SerializeField]
		
		float FaceInfo_Underline;
		[SerializeField]
		
		float FaceInfo_UnderlineThickness;
		[SerializeField]
		
		float FaceInfo_strikethrough;
		[SerializeField]
		
		float FaceInfo_strikethroughThickness;
		[SerializeField]
		
		float FaceInfo_TabWidth;
		[SerializeField]
		
		float FaceInfo_Padding;
		[SerializeField]
		
		float FaceInfo_AtlasWidth;
		[SerializeField]
		
		float FaceInfo_AtlasHeight;



		[SerializeField]
		
		List<int> TMP_Glyph_id;
		[SerializeField]
		
		List<float> TMP_Glyph_x;
		[SerializeField]
		
		List<float> TMP_Glyph_y;
		[SerializeField]
		
		List<float> TMP_Glyph_width;
		[SerializeField]
		
		List<float> TMP_Glyph_height;
		[SerializeField]
		
		List<float> TMP_Glyph_xOffset;
		[SerializeField]
		
		List<float> TMP_Glyph_yOffset;
		[SerializeField]
		
		List<float> TMP_Glyph_xAdvance;
		[SerializeField]
		
		List<float> TMP_Glyph_scale;

		[SerializeField]
		
		List<int> KerningTable_AscII_Left;
		[SerializeField]
		
		List<int> KerningTable_AscII_Right;
		[SerializeField]
		
		List<float> KerningTable_XadvanceOffset;


		[SerializeField]
		int KerningPair_AscII_Left;
		[SerializeField]
		int KerningPair_AscII_Right;
		[SerializeField]
		float KerningPair_XadvanceOffset;


		[SerializeField]
		string FontCreationSetting_fontSourcePath;
		[SerializeField]
		int FontCreationSetting_fontSizingMode;
		[SerializeField]
		int FontCreationSetting_fontSize;
		[SerializeField]
		int FontCreationSetting_fontPadding;
		[SerializeField]
		int FontCreationSetting_fontPackingMode;
		[SerializeField]
		int FontCreationSetting_fontAtlasWidth;
		[SerializeField]
		int FontCreationSetting_fontAtlasHeight;
		[SerializeField]
		int FontCreationSetting_fontCharacterSet;
		[SerializeField]
		int FontCreationSetting_fontStyle;
		[SerializeField]
		float FontCreationSetting_fontStyleModifier;
		[SerializeField]
		int FontCreationSetting_fontRenderMode;
		[SerializeField]
		bool FontCreationSetting_fontKerning;


		[SerializeField]
		List<global::TMPro.TMP_FontAsset> TMP_FontWeights_regularTypeface;
		[SerializeField]
		List<global::TMPro.TMP_FontAsset> TMP_FontWeights_italicTypeface;

		[SerializeField]
		string editorAtlasTextureName;

        [SerializeField]
        string editorMaterialName;

		[NonSerialized]
		public bool FontReplaced = false;

#if UNITY_EDITOR
		[NonSerialized]
        Texture2D loadedAtlas;

        [NonSerialized]
        bool atlasRefreshed = false;

        [NonSerialized]
        Material loadedMaterial;

        [NonSerialized]
        bool materialRefreshed = false;
#endif

        public Texture2D RefreshAtlas()
		{
#if UNITY_EDITOR
			if (!atlasRefreshed)
			{
				atlasRefreshed = true;
				if (!string.IsNullOrEmpty(editorAtlasTextureName))
				{
					var assetIDs = UnityEditor.AssetDatabase.FindAssets(editorAtlasTextureName);

					foreach (var id in assetIDs)
					{
                        loadedAtlas = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(id));
						if (loadedAtlas != null)
						{
							break;
						}
					}
                }
			}

			if (loadedAtlas != null)
			{
				return loadedAtlas;
			}
#endif

			return atlas;
        }

        public Material RefreshMaterial()
        {
#if UNITY_EDITOR
            if (!materialRefreshed)
            {
                materialRefreshed = true;
                if (!string.IsNullOrEmpty(editorMaterialName))
                {
                    var assetIDs = UnityEditor.AssetDatabase.FindAssets(editorMaterialName);

                    foreach (var id in assetIDs)
                    {
                        loadedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(UnityEditor.AssetDatabase.GUIDToAssetPath(id));
                        if (loadedMaterial != null)
                        {
                            break;
                        }
                    }
                }
            }

            if (loadedMaterial != null)
            {
                return loadedMaterial;
            }
#endif

            return material;
        }

#if UNITY_EDITOR

		[BeforeBuild]
		static void BeforeBuild()
		{
			var weaverFontIDs = UnityEditor.AssetDatabase.FindAssets("t:WeaverCore.Assets.TMPro.TMP_FontAsset");

			var trajanProAtlas = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(UnityEditor.AssetDatabase.GUIDToAssetPath(UnityEditor.AssetDatabase.FindAssets("t:Texture2D \"Trajan Pro Regular SDF Atlas\"")[0]));

			foreach (var id in weaverFontIDs)
			{
				var font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(id));

				if (font != null)
				{
					if (!string.IsNullOrEmpty(font.editorAtlasTextureName) && !string.IsNullOrEmpty(font.editorMaterialName))
					{
                        font.atlas = trajanProAtlas;
						font.material.mainTexture = trajanProAtlas;
                    }
				}
			}


		}

		[AfterBuild]
		static void AfterBuild()
		{
            var weaverFontIDs = UnityEditor.AssetDatabase.FindAssets("t:WeaverCore.Assets.TMPro.TMP_FontAsset");

            foreach (var id in weaverFontIDs)
            {
                var font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(id));

                if (font != null)
                {
                    if (!string.IsNullOrEmpty(font.editorAtlasTextureName) && !string.IsNullOrEmpty(font.editorMaterialName))
                    {
						font.material = LoadEditorAsset<Material>("Material", font.editorMaterialName);
						font.atlas = LoadEditorAsset<Texture2D>("Texture2D", font.editorAtlasTextureName);
						if (font.material != null)
						{
							font.material.mainTexture = font.atlas;
						}
                    }
                }
            }
        }

		static T LoadEditorAsset<T>(string type, string name) where T : UnityEngine.Object
		{
			var guids = UnityEditor.AssetDatabase.FindAssets($"t:{type} \"{name}\"");

			var guid = guids.FirstOrDefault();

			if (string.IsNullOrEmpty(guid))
			{
				return default;
			}

			return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));
		}

        [OnInit]
		static void Init()
		{
			var patcher = HarmonyPatcher.Create("WeaverCore.TMPFONT.com");

			var fontType = typeof(global::TMPro.TMP_FontAsset);

			var method = fontType.GetMethod("OnValidate", BindingFlags.NonPublic | BindingFlags.Instance);

			patcher.Patch(method, typeof(TMP_FontAsset).GetMethod("PatchedOnValidate", BindingFlags.Static | BindingFlags.NonPublic), null);

		}
#endif

        static void ShallowCopy<T>(T from, T to, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            foreach (var field in typeof(T).GetFields(flags))
            {
                field.SetValue(to, field.GetValue(from));
            }
        }

        void Awake()
		{
			if (FontReplaced || !Application.isPlaying || Initialization.Environment == Enums.RunningState.Editor)
			{
				return;
			}
			if (FontAssetContainer.InGameFonts != null)
			{
				var pair = FontAssetContainer.sourceDestPairs.FirstOrDefault(p => p.Item2 == name);

				if (pair != default)
				{
					var source = FontAssetContainer.InGameFonts.FirstOrDefault(f => f.name == pair.Item1);

					if (source != null)
					{
						Debug.Log($"Replacing {name} with {source.name}");

						ShallowCopy<global::TMPro.TMP_FontAsset>(source, this);

						return;
                    }
                }
			}

			var type = typeof(global::TMPro.TMP_FontAsset);
			var fontInfo = new FaceInfo();

			fontInfo.Name = FaceInfo_Name;
			fontInfo.PointSize = FaceInfo_PointSize;
			fontInfo.Scale = FaceInfo_Scale;
			fontInfo.CharacterCount = FaceInfo_CharacterCount;
			fontInfo.LineHeight = FaceInfo_LineHeight;
			fontInfo.Baseline = FaceInfo_Baseline;
			fontInfo.Ascender = FaceInfo_Ascender;
			fontInfo.CapHeight = FaceInfo_CapHeight;
			fontInfo.Descender = FaceInfo_Descender;
			fontInfo.CenterLine = FaceInfo_CenterLine;
			fontInfo.SuperscriptOffset = FaceInfo_SuperscriptOffset;
			fontInfo.SubscriptOffset = FaceInfo_SubscriptOffset;
			fontInfo.SubSize = FaceInfo_SubSize;
			fontInfo.Underline = FaceInfo_Underline;
			fontInfo.UnderlineThickness = FaceInfo_UnderlineThickness;
			fontInfo.TabWidth = FaceInfo_TabWidth;
			fontInfo.Padding = FaceInfo_Padding;
			fontInfo.AtlasWidth = FaceInfo_AtlasWidth;
			fontInfo.AtlasHeight = FaceInfo_AtlasHeight;
#if UNITY_EDITOR
			fontInfo.strikethrough = FaceInfo_strikethrough;
			fontInfo.strikethroughThickness = FaceInfo_strikethroughThickness;
#endif

			type.GetField("m_fontInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this,fontInfo);

			List<TMP_Glyph> glyphs = new List<TMP_Glyph>();

			for (int i = 0; i < TMP_Glyph_height.Count; i++)
			{
				TMP_Glyph glyph = new TMP_Glyph();

				glyph.height = TMP_Glyph_height[i];
				glyph.id = TMP_Glyph_id[i];
				glyph.scale = TMP_Glyph_scale[i];
				glyph.width = TMP_Glyph_width[i];
				glyph.x = TMP_Glyph_x[i];
				glyph.xAdvance = TMP_Glyph_xAdvance[i];
				glyph.xOffset = TMP_Glyph_xOffset[i];
				glyph.y = TMP_Glyph_y[i];
				glyph.yOffset = TMP_Glyph_yOffset[i];
				glyphs.Add(glyph);
			}

			type.GetField("m_glyphInfoList", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this,glyphs);

			KerningTable kTable = new KerningTable();

			for (int i = 0; i < KerningTable_AscII_Left.Count; i++)
			{
				kTable.kerningPairs.Add(new KerningPair(KerningTable_AscII_Left[i], KerningTable_AscII_Right[i], KerningTable_XadvanceOffset[i]));
			}

			type.GetField("m_kerningInfo", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this,kTable);

			var newKerningPair = new KerningPair(KerningPair_AscII_Left,KerningPair_AscII_Right,KerningPair_XadvanceOffset);
			type.GetField("m_kerningPair", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this,newKerningPair);



			FontCreationSetting fontSettings = new FontCreationSetting();

			fontSettings.fontAtlasHeight = FontCreationSetting_fontAtlasHeight;
			fontSettings.fontAtlasWidth = FontCreationSetting_fontAtlasWidth;
			fontSettings.fontCharacterSet = FontCreationSetting_fontCharacterSet;
			fontSettings.fontKerning = FontCreationSetting_fontKerning;
			fontSettings.fontPackingMode = FontCreationSetting_fontPackingMode;
			fontSettings.fontPadding = FontCreationSetting_fontPadding;
			fontSettings.fontRenderMode = FontCreationSetting_fontRenderMode;
			fontSettings.fontSize = FontCreationSetting_fontSize;
			fontSettings.fontSizingMode = FontCreationSetting_fontSizingMode;
			fontSettings.fontSourcePath = FontCreationSetting_fontSourcePath;
			fontSettings.fontStlyeModifier = FontCreationSetting_fontStyleModifier;
			fontSettings.fontStyle = FontCreationSetting_fontStyle;
			type.GetField("fontCreationSettings", BindingFlags.Public | BindingFlags.Instance).SetValue(this,fontSettings);


			TMP_FontWeights[] weights = new TMP_FontWeights[TMP_FontWeights_italicTypeface.Count];

			for (int i = 0; i < TMP_FontWeights_italicTypeface.Count; i++)
			{
				var newWeight = new TMP_FontWeights();
				newWeight.italicTypeface = TMP_FontWeights_italicTypeface[i];
				newWeight.regularTypeface = TMP_FontWeights_regularTypeface[i];

				weights[i] = newWeight;
			}


			type.GetField("fontWeights", BindingFlags.Public | BindingFlags.Instance).SetValue(this,weights);
		}

#if UNITY_EDITOR
        void RefreshJaggedFields()
		{
            var type = typeof(global::TMPro.TMP_FontAsset);


            var fontInfo = (FaceInfo)type.GetField("m_fontInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

            this.FaceInfo_Ascender = fontInfo.Ascender;
            this.FaceInfo_AtlasHeight = fontInfo.AtlasHeight;
            this.FaceInfo_AtlasWidth = fontInfo.AtlasWidth;
            this.FaceInfo_Baseline = fontInfo.Baseline;
            this.FaceInfo_CapHeight = fontInfo.CapHeight;
            this.FaceInfo_CenterLine = fontInfo.CenterLine;
            this.FaceInfo_CharacterCount = fontInfo.CharacterCount;
            this.FaceInfo_Descender = fontInfo.Descender;
            this.FaceInfo_LineHeight = fontInfo.LineHeight;
            this.FaceInfo_Name = fontInfo.Name;
            this.FaceInfo_Padding = fontInfo.Padding;
            this.FaceInfo_PointSize = fontInfo.PointSize;
            this.FaceInfo_Scale = fontInfo.Scale;
            this.FaceInfo_strikethrough = fontInfo.strikethrough;
            this.FaceInfo_strikethroughThickness = fontInfo.strikethroughThickness;
            this.FaceInfo_SubscriptOffset = fontInfo.SubscriptOffset;
            this.FaceInfo_SubSize = fontInfo.SubSize;
            this.FaceInfo_SuperscriptOffset = fontInfo.SuperscriptOffset;
            this.FaceInfo_TabWidth = fontInfo.TabWidth;
            this.FaceInfo_Underline = fontInfo.Underline;
            this.FaceInfo_UnderlineThickness = fontInfo.UnderlineThickness;

            var glyphList = (List<TMP_Glyph>)type.GetField("m_glyphInfoList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

            this.TMP_Glyph_height = new List<float>();
            this.TMP_Glyph_id = new List<int>();
            this.TMP_Glyph_scale = new List<float>();
            this.TMP_Glyph_width = new List<float>();
            this.TMP_Glyph_x = new List<float>();
            this.TMP_Glyph_xAdvance = new List<float>();
            this.TMP_Glyph_xOffset = new List<float>();
            this.TMP_Glyph_y = new List<float>();
            this.TMP_Glyph_yOffset = new List<float>();

            for (int i = 0; i < glyphList.Count; i++)
            {
                this.TMP_Glyph_height.Add(glyphList[i].height);
                this.TMP_Glyph_id.Add(glyphList[i].id);
                this.TMP_Glyph_scale.Add(glyphList[i].scale);
                this.TMP_Glyph_width.Add(glyphList[i].width);
                this.TMP_Glyph_x.Add(glyphList[i].x);
                this.TMP_Glyph_xAdvance.Add(glyphList[i].xAdvance);
                this.TMP_Glyph_xOffset.Add(glyphList[i].xOffset);
                this.TMP_Glyph_y.Add(glyphList[i].y);
                this.TMP_Glyph_yOffset.Add(glyphList[i].yOffset);
            }

            var kerningTable = (KerningTable)type.GetField("m_kerningInfo", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
            //TODO TODO TODO
            this.KerningTable_AscII_Left = new List<int>();
            this.KerningTable_AscII_Right = new List<int>();
            this.KerningTable_XadvanceOffset = new List<float>();


            for (int i = 0; i < kerningTable.kerningPairs.Count; i++)
            {
                this.KerningTable_AscII_Left.Add(kerningTable.kerningPairs[i].AscII_Left);
                this.KerningTable_AscII_Right.Add(kerningTable.kerningPairs[i].AscII_Right);
                this.KerningTable_XadvanceOffset.Add(kerningTable.kerningPairs[i].XadvanceOffset);
            }

            var kerningPair = (KerningPair)type.GetField("m_kerningPair", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

            this.KerningPair_AscII_Left = kerningPair.AscII_Left;
            this.KerningPair_AscII_Right = kerningPair.AscII_Right;
            this.KerningPair_XadvanceOffset = kerningPair.XadvanceOffset;


            var fontSettings = (FontCreationSetting)type.GetField("fontCreationSettings", BindingFlags.Public | BindingFlags.Instance).GetValue(this);


            this.FontCreationSetting_fontAtlasHeight = fontSettings.fontAtlasHeight;
            this.FontCreationSetting_fontAtlasWidth = fontSettings.fontAtlasWidth;
            this.FontCreationSetting_fontCharacterSet = fontSettings.fontCharacterSet;
            this.FontCreationSetting_fontKerning = fontSettings.fontKerning;
            this.FontCreationSetting_fontPackingMode = fontSettings.fontPackingMode;
            this.FontCreationSetting_fontPadding = fontSettings.fontPadding;
            this.FontCreationSetting_fontRenderMode = fontSettings.fontRenderMode;
            this.FontCreationSetting_fontSize = fontSettings.fontSize;
            this.FontCreationSetting_fontSizingMode = fontSettings.fontSizingMode;
            this.FontCreationSetting_fontSourcePath = fontSettings.fontSourcePath;
            this.FontCreationSetting_fontStyleModifier = fontSettings.fontStlyeModifier;
            this.FontCreationSetting_fontStyle = fontSettings.fontStyle;

            var fontWeights = (TMP_FontWeights[])type.GetField("fontWeights", BindingFlags.Public | BindingFlags.Instance).GetValue(this);

            this.TMP_FontWeights_italicTypeface = new List<global::TMPro.TMP_FontAsset>();
            this.TMP_FontWeights_regularTypeface = new List<global::TMPro.TMP_FontAsset>();

            for (int i = 0; i < fontWeights.GetLength(0); i++)
            {
                this.TMP_FontWeights_italicTypeface.Add(fontWeights[i].italicTypeface);
                this.TMP_FontWeights_regularTypeface.Add(fontWeights[i].regularTypeface);
            }
        }

		static bool PatchedOnValidate(object __instance)
		{
			if (Application.isPlaying || !(__instance is TMP_FontAsset))
			{
				return true;
			}
			var obj = __instance as TMP_FontAsset;

			obj.RefreshJaggedFields();

			return true;
		}
#endif
	}
}
