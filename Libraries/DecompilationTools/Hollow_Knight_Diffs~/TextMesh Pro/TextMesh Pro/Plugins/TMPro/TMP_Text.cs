using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TMPro
{
	public class TMP_Text : MaskableGraphic
	{
		protected enum TextInputSources
		{
			Text,
			SetText,
			SetCharArray,
			String
		}

		[SerializeField]
		protected string m_text;

		[SerializeField]
		protected bool m_isRightToLeft = false;

		[SerializeField]
		protected TMP_FontAsset m_fontAsset;

		protected TMP_FontAsset m_currentFontAsset;

		protected bool m_isSDFShader;

		[SerializeField]
		protected Material m_sharedMaterial;

		protected Material m_currentMaterial;

		protected MaterialReference[] m_materialReferences = new MaterialReference[32];

		protected Dictionary<int, int> m_materialReferenceIndexLookup = new Dictionary<int, int>();

		protected TMP_XmlTagStack<MaterialReference> m_materialReferenceStack = new TMP_XmlTagStack<MaterialReference>(new MaterialReference[16]);

		protected int m_currentMaterialIndex;

		[SerializeField]
		protected Material[] m_fontSharedMaterials;

		[SerializeField]
		protected Material m_fontMaterial;

		[SerializeField]
		protected Material[] m_fontMaterials;

		protected bool m_isMaterialDirty;

		[SerializeField]
		protected Color32 m_fontColor32 = Color.white;

		[SerializeField]
		protected Color m_fontColor = Color.white;

		protected static Color32 s_colorWhite = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);

		protected Color32 m_underlineColor = s_colorWhite;

		protected Color32 m_strikethroughColor = s_colorWhite;

		protected Color32 m_highlightColor = s_colorWhite;

		[SerializeField]
		protected bool m_enableVertexGradient;

		[SerializeField]
		protected VertexGradient m_fontColorGradient = new VertexGradient(Color.white);

		[SerializeField]
		protected TMP_ColorGradient m_fontColorGradientPreset;

		[SerializeField]
		protected TMP_SpriteAsset m_spriteAsset;

		[SerializeField]
		protected bool m_tintAllSprites;

		protected bool m_tintSprite;

		protected Color32 m_spriteColor;

		[SerializeField]
		protected bool m_overrideHtmlColors = false;

		[SerializeField]
		protected Color32 m_faceColor = Color.white;

		[SerializeField]
		protected Color32 m_outlineColor = Color.black;

		protected float m_outlineWidth = 0f;

		[SerializeField]
		protected float m_fontSize = 36f;

		protected float m_currentFontSize;

		[SerializeField]
		protected float m_fontSizeBase = 36f;

		protected TMP_XmlTagStack<float> m_sizeStack = new TMP_XmlTagStack<float>(new float[16]);

		[SerializeField]
		protected int m_fontWeight = 400;

		protected int m_fontWeightInternal;

		protected TMP_XmlTagStack<int> m_fontWeightStack = new TMP_XmlTagStack<int>(new int[16]);

		[SerializeField]
		protected bool m_enableAutoSizing;

		protected float m_maxFontSize;

		protected float m_minFontSize;

		[SerializeField]
		protected float m_fontSizeMin = 0f;

		[SerializeField]
		protected float m_fontSizeMax = 0f;

		[SerializeField]
		protected FontStyles m_fontStyle = FontStyles.Normal;

		protected FontStyles m_style = FontStyles.Normal;

		protected TMP_BasicXmlTagStack m_fontStyleStack;

		protected bool m_isUsingBold = false;

		[SerializeField]
		[FormerlySerializedAs("m_lineJustification")]
		protected TextAlignmentOptions m_textAlignment = TextAlignmentOptions.TopLeft;

		protected TextAlignmentOptions m_lineJustification;

		protected TMP_XmlTagStack<TextAlignmentOptions> m_lineJustificationStack = new TMP_XmlTagStack<TextAlignmentOptions>(new TextAlignmentOptions[16]);

		protected Vector3[] m_textContainerLocalCorners = new Vector3[4];

		[SerializeField]
		protected bool m_isAlignmentEnumConverted;

		[SerializeField]
		protected float m_characterSpacing = 0f;

		protected float m_cSpacing = 0f;

		protected float m_monoSpacing = 0f;

		[SerializeField]
		protected float m_wordSpacing = 0f;

		[SerializeField]
		protected float m_lineSpacing = 0f;

		protected float m_lineSpacingDelta = 0f;

		protected float m_lineHeight = -32767f;

		[SerializeField]
		protected float m_lineSpacingMax = 0f;

		[SerializeField]
		protected float m_paragraphSpacing = 0f;

		[SerializeField]
		protected float m_charWidthMaxAdj = 0f;

		protected float m_charWidthAdjDelta = 0f;

		[SerializeField]
		protected bool m_enableWordWrapping = false;

		protected bool m_isCharacterWrappingEnabled = false;

		protected bool m_isNonBreakingSpace = false;

		protected bool m_isIgnoringAlignment;

		[SerializeField]
		protected float m_wordWrappingRatios = 0.4f;

		[SerializeField]
		protected TextOverflowModes m_overflowMode = TextOverflowModes.Overflow;

		[SerializeField]
		protected int m_firstOverflowCharacterIndex = -1;

		[SerializeField]
		protected TMP_Text m_linkedTextComponent;

		[SerializeField]
		protected bool m_isLinkedTextComponent;

		protected bool m_isTextTruncated;

		[SerializeField]
		protected bool m_enableKerning;

		[SerializeField]
		protected bool m_enableExtraPadding = false;

		[SerializeField]
		protected bool checkPaddingRequired;

		[SerializeField]
		protected bool m_isRichText = true;

		[SerializeField]
		protected bool m_parseCtrlCharacters = true;

		protected bool m_isOverlay = false;

		[SerializeField]
		protected bool m_isOrthographic = false;

		[SerializeField]
		protected bool m_isCullingEnabled = false;

		[SerializeField]
		protected bool m_ignoreRectMaskCulling;

		[SerializeField]
		protected bool m_ignoreCulling = true;

		[SerializeField]
		protected TextureMappingOptions m_horizontalMapping = TextureMappingOptions.Character;

		[SerializeField]
		protected TextureMappingOptions m_verticalMapping = TextureMappingOptions.Character;

		[SerializeField]
		protected float m_uvLineOffset = 0f;

		protected TextRenderFlags m_renderMode = TextRenderFlags.Render;

		[SerializeField]
		protected VertexSortingOrder m_geometrySortingOrder;

		[SerializeField]
		protected int m_firstVisibleCharacter;

		protected int m_maxVisibleCharacters = 99999;

		protected int m_maxVisibleWords = 99999;

		protected int m_maxVisibleLines = 99999;

		[SerializeField]
		protected bool m_useMaxVisibleDescender = true;

		[SerializeField]
		protected int m_pageToDisplay = 1;

		protected bool m_isNewPage = false;

		[SerializeField]
		protected Vector4 m_margin = new Vector4(0f, 0f, 0f, 0f);

		protected float m_marginLeft;

		protected float m_marginRight;

		protected float m_marginWidth;

		protected float m_marginHeight;

		protected float m_width = -1f;

		//FAULTY
		[SerializeField]
		protected TMP_TextInfo m_textInfo;

		[SerializeField]
		protected bool m_havePropertiesChanged;

		[SerializeField]
		protected bool m_isUsingLegacyAnimationComponent;

		protected Transform m_transform;

		protected RectTransform m_rectTransform;

		protected bool m_autoSizeTextContainer;

		protected Mesh m_mesh;

		[SerializeField]
		protected bool m_isVolumetricText;

		[SerializeField]
		protected TMP_SpriteAnimator m_spriteAnimator;

		protected float m_flexibleHeight = -1f;

		protected float m_flexibleWidth = -1f;

		protected float m_minWidth;

		protected float m_minHeight;

		protected float m_maxWidth;

		protected float m_maxHeight;

		protected LayoutElement m_LayoutElement;

		protected float m_preferredWidth;

		protected float m_renderedWidth;

		protected bool m_isPreferredWidthDirty;

		protected float m_preferredHeight;

		protected float m_renderedHeight;

		protected bool m_isPreferredHeightDirty;

		protected bool m_isCalculatingPreferredValues;

		private int m_recursiveCount;

		protected int m_layoutPriority = 0;

		protected bool m_isCalculateSizeRequired = false;

		protected bool m_isLayoutDirty;

		protected bool m_verticesAlreadyDirty;

		protected bool m_layoutAlreadyDirty;

		protected bool m_isAwake;

		[SerializeField]
		protected bool m_isInputParsingRequired = false;

		[SerializeField]
		protected TextInputSources m_inputSource;

		protected string old_text;

		protected float old_arg0;

		protected float old_arg1;

		protected float old_arg2;

		protected float m_fontScale;

		protected float m_fontScaleMultiplier;

		protected char[] m_htmlTag = new char[128];

		protected XML_TagAttribute[] m_xmlAttribute = new XML_TagAttribute[8];

		protected float[] m_attributeParameterValues = new float[16];

		protected float tag_LineIndent = 0f;

		protected float tag_Indent = 0f;

		protected TMP_XmlTagStack<float> m_indentStack = new TMP_XmlTagStack<float>(new float[16]);

		protected bool tag_NoParsing;

		protected bool m_isParsingText;

		protected Matrix4x4 m_FXMatrix;

		protected bool m_isFXMatrixSet;

		protected int[] m_char_buffer;

		private TMP_CharacterInfo[] m_internalCharacterInfo;

		protected char[] m_input_CharArray = new char[256];

		private int m_charArray_Length = 0;

		protected int m_totalCharacterCount;

		protected WordWrapState m_SavedWordWrapState = default(WordWrapState);

		protected WordWrapState m_SavedLineState = default(WordWrapState);

		protected int m_characterCount;

		protected int m_firstCharacterOfLine;

		protected int m_firstVisibleCharacterOfLine;

		protected int m_lastCharacterOfLine;

		protected int m_lastVisibleCharacterOfLine;

		protected int m_lineNumber;

		protected int m_lineVisibleCharacterCount;

		protected int m_pageNumber;

		protected float m_maxAscender;

		protected float m_maxCapHeight;

		protected float m_maxDescender;

		protected float m_maxLineAscender;

		protected float m_maxLineDescender;

		protected float m_startOfLineAscender;

		protected float m_lineOffset;

		protected Extents m_meshExtents;

		protected Color32 m_htmlColor = new Color(255f, 255f, 255f, 128f);

		protected TMP_XmlTagStack<Color32> m_colorStack = new TMP_XmlTagStack<Color32>(new Color32[16]);

		protected TMP_XmlTagStack<Color32> m_underlineColorStack = new TMP_XmlTagStack<Color32>(new Color32[16]);

		protected TMP_XmlTagStack<Color32> m_strikethroughColorStack = new TMP_XmlTagStack<Color32>(new Color32[16]);

		protected TMP_XmlTagStack<Color32> m_highlightColorStack = new TMP_XmlTagStack<Color32>(new Color32[16]);

		protected float m_tabSpacing = 0f;

		protected float m_spacing = 0f;

		protected TMP_XmlTagStack<int> m_styleStack = new TMP_XmlTagStack<int>(new int[16]);

		protected TMP_XmlTagStack<int> m_actionStack = new TMP_XmlTagStack<int>(new int[16]);

		protected float m_padding = 0f;

		protected float m_baselineOffset;

		protected float m_xAdvance;

		protected TMP_TextElementType m_textElementType;

		protected TMP_TextElement m_cached_TextElement;

		protected TMP_Glyph m_cached_Underline_GlyphInfo;

		protected TMP_Glyph m_cached_Ellipsis_GlyphInfo;

		protected TMP_SpriteAsset m_defaultSpriteAsset;

		protected TMP_SpriteAsset m_currentSpriteAsset;

		protected int m_spriteCount = 0;

		protected int m_spriteIndex;

		protected InlineGraphicManager m_inlineGraphics;

		protected int m_spriteAnimationID;

		protected bool m_ignoreActiveState;

		private readonly float[] k_Power = new float[10]
		{
			0.5f,
			0.05f,
			0.005f,
			0.0005f,
			5E-05f,
			5E-06f,
			5E-07f,
			5E-08f,
			5E-09f,
			5E-10f
		};

		protected static Vector2 k_LargePositiveVector2 = new Vector2(2.14748365E+09f, 2.14748365E+09f);

		protected static Vector2 k_LargeNegativeVector2 = new Vector2(-2.14748365E+09f, -2.14748365E+09f);

		protected static float k_LargePositiveFloat = 32767f;

		protected static float k_LargeNegativeFloat = -32767f;

		protected static int k_LargePositiveInt = int.MaxValue;

		protected static int k_LargeNegativeInt = -2147483647;

		public string text
		{
			get
			{
				return m_text;
			}
			set
			{
				if (!(m_text == value))
				{
					m_text = (old_text = value);
					m_inputSource = TextInputSources.String;
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					m_isInputParsingRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public bool isRightToLeftText
		{
			get
			{
				return m_isRightToLeft;
			}
			set
			{
				if (m_isRightToLeft != value)
				{
					m_isRightToLeft = value;
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					m_isInputParsingRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public TMP_FontAsset font
		{
			get
			{
				return m_fontAsset;
			}
			set
			{
				if (!(m_fontAsset == value))
				{
					m_fontAsset = value;
					LoadFontAsset();
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					m_isInputParsingRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public virtual Material fontSharedMaterial
		{
			get
			{
				return m_sharedMaterial;
			}
			set
			{
				if (!(m_sharedMaterial == value))
				{
					SetSharedMaterial(value);
					m_havePropertiesChanged = true;
					m_isInputParsingRequired = true;
					SetVerticesDirty();
					SetMaterialDirty();
				}
			}
		}

		public virtual Material[] fontSharedMaterials
		{
			get
			{
				return GetSharedMaterials();
			}
			set
			{
				SetSharedMaterials(value);
				m_havePropertiesChanged = true;
				m_isInputParsingRequired = true;
				SetVerticesDirty();
				SetMaterialDirty();
			}
		}

		public Material fontMaterial
		{
			get
			{
				return GetMaterial(m_sharedMaterial);
			}
			set
			{
				if (!(m_sharedMaterial != null) || m_sharedMaterial.GetInstanceID() != value.GetInstanceID())
				{
					m_sharedMaterial = value;
					m_padding = GetPaddingForMaterial();
					m_havePropertiesChanged = true;
					m_isInputParsingRequired = true;
					SetVerticesDirty();
					SetMaterialDirty();
				}
			}
		}

		public virtual Material[] fontMaterials
		{
			get
			{
				return GetMaterials(m_fontSharedMaterials);
			}
			set
			{
				SetSharedMaterials(value);
				m_havePropertiesChanged = true;
				m_isInputParsingRequired = true;
				SetVerticesDirty();
				SetMaterialDirty();
			}
		}

		public override Color color
		{
			get
			{
				return m_fontColor;
			}
			set
			{
				if (!(m_fontColor == value))
				{
					m_havePropertiesChanged = true;
					m_fontColor = value;
					SetVerticesDirty();
				}
			}
		}

		public float alpha
		{
			get
			{
				return m_fontColor.a;
			}
			set
			{
				if (m_fontColor.a != value)
				{
					m_fontColor.a = value;
					m_havePropertiesChanged = true;
					SetVerticesDirty();
				}
			}
		}

		public bool enableVertexGradient
		{
			get
			{
				return m_enableVertexGradient;
			}
			set
			{
				if (m_enableVertexGradient != value)
				{
					m_havePropertiesChanged = true;
					m_enableVertexGradient = value;
					SetVerticesDirty();
				}
			}
		}

		public VertexGradient colorGradient
		{
			get
			{
				return m_fontColorGradient;
			}
			set
			{
				m_havePropertiesChanged = true;
				m_fontColorGradient = value;
				SetVerticesDirty();
			}
		}

		public TMP_ColorGradient colorGradientPreset
		{
			get
			{
				return m_fontColorGradientPreset;
			}
			set
			{
				m_havePropertiesChanged = true;
				m_fontColorGradientPreset = value;
				SetVerticesDirty();
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
				m_havePropertiesChanged = true;
				m_isInputParsingRequired = true;
				m_isCalculateSizeRequired = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}

		public bool tintAllSprites
		{
			get
			{
				return m_tintAllSprites;
			}
			set
			{
				if (m_tintAllSprites != value)
				{
					m_tintAllSprites = value;
					m_havePropertiesChanged = true;
					SetVerticesDirty();
				}
			}
		}

		public bool overrideColorTags
		{
			get
			{
				return m_overrideHtmlColors;
			}
			set
			{
				if (m_overrideHtmlColors != value)
				{
					m_havePropertiesChanged = true;
					m_overrideHtmlColors = value;
					SetVerticesDirty();
				}
			}
		}

		public Color32 faceColor
		{
			get
			{
				if (m_sharedMaterial == null)
				{
					return m_faceColor;
				}
				m_faceColor = m_sharedMaterial.GetColor(ShaderUtilities.ID_FaceColor);
				return m_faceColor;
			}
			set
			{
				if (!m_faceColor.Compare(value))
				{
					SetFaceColor(value);
					m_havePropertiesChanged = true;
					m_faceColor = value;
					SetVerticesDirty();
					SetMaterialDirty();
				}
			}
		}

		public Color32 outlineColor
		{
			get
			{
				if (m_sharedMaterial == null)
				{
					return m_outlineColor;
				}
				m_outlineColor = m_sharedMaterial.GetColor(ShaderUtilities.ID_OutlineColor);
				return m_outlineColor;
			}
			set
			{
				if (!m_outlineColor.Compare(value))
				{
					SetOutlineColor(value);
					m_havePropertiesChanged = true;
					m_outlineColor = value;
					SetVerticesDirty();
				}
			}
		}

		public float outlineWidth
		{
			get
			{
				if (m_sharedMaterial == null)
				{
					return m_outlineWidth;
				}
				m_outlineWidth = m_sharedMaterial.GetFloat(ShaderUtilities.ID_OutlineWidth);
				return m_outlineWidth;
			}
			set
			{
				if (m_outlineWidth != value)
				{
					SetOutlineThickness(value);
					m_havePropertiesChanged = true;
					m_outlineWidth = value;
					SetVerticesDirty();
				}
			}
		}

		public float fontSize
		{
			get
			{
				return m_fontSize;
			}
			set
			{
				if (m_fontSize != value)
				{
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
					m_fontSize = value;
					if (!m_enableAutoSizing)
					{
						m_fontSizeBase = m_fontSize;
					}
				}
			}
		}

		public float fontScale
		{
			get
			{
				return m_fontScale;
			}
		}

		public int fontWeight
		{
			get
			{
				return m_fontWeight;
			}
			set
			{
				if (m_fontWeight != value)
				{
					m_fontWeight = value;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public float pixelsPerUnit
		{
			get
			{
				Canvas canvas = ((Graphic)this).canvas;
				if (!canvas)
				{
					return 1f;
				}
				if (!font)
				{
					return canvas.scaleFactor;
				}
				if (m_currentFontAsset == null || m_currentFontAsset.fontInfo.PointSize <= 0f || m_fontSize <= 0f)
				{
					return 1f;
				}
				return m_fontSize / m_currentFontAsset.fontInfo.PointSize;
			}
		}

		public bool enableAutoSizing
		{
			get
			{
				return m_enableAutoSizing;
			}
			set
			{
				if (m_enableAutoSizing != value)
				{
					m_enableAutoSizing = value;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public float fontSizeMin
		{
			get
			{
				return m_fontSizeMin;
			}
			set
			{
				if (m_fontSizeMin != value)
				{
					m_fontSizeMin = value;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public float fontSizeMax
		{
			get
			{
				return m_fontSizeMax;
			}
			set
			{
				if (m_fontSizeMax != value)
				{
					m_fontSizeMax = value;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public FontStyles fontStyle
		{
			get
			{
				return m_fontStyle;
			}
			set
			{
				if (m_fontStyle != value)
				{
					m_fontStyle = value;
					m_havePropertiesChanged = true;
					checkPaddingRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public bool isUsingBold
		{
			get
			{
				return m_isUsingBold;
			}
		}

		public TextAlignmentOptions alignment
		{
			get
			{
				return m_textAlignment;
			}
			set
			{
				if (m_textAlignment != value)
				{
					m_havePropertiesChanged = true;
					m_textAlignment = value;
					SetVerticesDirty();
				}
			}
		}

		public float characterSpacing
		{
			get
			{
				return m_characterSpacing;
			}
			set
			{
				if (m_characterSpacing != value)
				{
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
					m_characterSpacing = value;
				}
			}
		}

		public float wordSpacing
		{
			get
			{
				return m_wordSpacing;
			}
			set
			{
				if (m_wordSpacing != value)
				{
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
					m_wordSpacing = value;
				}
			}
		}

		public float lineSpacing
		{
			get
			{
				return m_lineSpacing;
			}
			set
			{
				if (m_lineSpacing != value)
				{
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
					m_lineSpacing = value;
				}
			}
		}

		public float lineSpacingAdjustment
		{
			get
			{
				return m_lineSpacingMax;
			}
			set
			{
				if (m_lineSpacingMax != value)
				{
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
					m_lineSpacingMax = value;
				}
			}
		}

		public float paragraphSpacing
		{
			get
			{
				return m_paragraphSpacing;
			}
			set
			{
				if (m_paragraphSpacing != value)
				{
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
					m_paragraphSpacing = value;
				}
			}
		}

		public float characterWidthAdjustment
		{
			get
			{
				return m_charWidthMaxAdj;
			}
			set
			{
				if (m_charWidthMaxAdj != value)
				{
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
					m_charWidthMaxAdj = value;
				}
			}
		}

		public bool enableWordWrapping
		{
			get
			{
				return m_enableWordWrapping;
			}
			set
			{
				if (m_enableWordWrapping != value)
				{
					m_havePropertiesChanged = true;
					m_isInputParsingRequired = true;
					m_isCalculateSizeRequired = true;
					m_enableWordWrapping = value;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public float wordWrappingRatios
		{
			get
			{
				return m_wordWrappingRatios;
			}
			set
			{
				if (m_wordWrappingRatios != value)
				{
					m_wordWrappingRatios = value;
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public TextOverflowModes overflowMode
		{
			get
			{
				return m_overflowMode;
			}
			set
			{
				if (m_overflowMode != value)
				{
					m_overflowMode = value;
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public bool isTextOverflowing
		{
			get
			{
				if (m_firstOverflowCharacterIndex != -1)
				{
					return true;
				}
				return false;
			}
		}

		public int firstOverflowCharacterIndex
		{
			get
			{
				return m_firstOverflowCharacterIndex;
			}
		}

		public TMP_Text linkedTextComponent
		{
			get
			{
				return m_linkedTextComponent;
			}
			set
			{
				if ((UnityEngine.Object)(object)m_linkedTextComponent != (UnityEngine.Object)(object)value)
				{
					if ((UnityEngine.Object)(object)m_linkedTextComponent != null)
					{
						m_linkedTextComponent.overflowMode = TextOverflowModes.Overflow;
						m_linkedTextComponent.linkedTextComponent = null;
						m_linkedTextComponent.isLinkedTextComponent = false;
					}
					m_linkedTextComponent = value;
					if ((UnityEngine.Object)(object)m_linkedTextComponent != null)
					{
						m_linkedTextComponent.isLinkedTextComponent = true;
					}
				}
				m_havePropertiesChanged = true;
				m_isCalculateSizeRequired = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}

		public bool isLinkedTextComponent
		{
			get
			{
				return m_isLinkedTextComponent;
			}
			set
			{
				m_isLinkedTextComponent = value;
				if (!m_isLinkedTextComponent)
				{
					m_firstVisibleCharacter = 0;
				}
				m_havePropertiesChanged = true;
				m_isCalculateSizeRequired = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}

		public bool isTextTruncated
		{
			get
			{
				return m_isTextTruncated;
			}
		}

		public bool enableKerning
		{
			get
			{
				return m_enableKerning;
			}
			set
			{
				if (m_enableKerning != value)
				{
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
					m_enableKerning = value;
				}
			}
		}

		public bool extraPadding
		{
			get
			{
				return m_enableExtraPadding;
			}
			set
			{
				if (m_enableExtraPadding != value)
				{
					m_havePropertiesChanged = true;
					m_enableExtraPadding = value;
					UpdateMeshPadding();
					SetVerticesDirty();
				}
			}
		}

		public bool richText
		{
			get
			{
				return m_isRichText;
			}
			set
			{
				if (m_isRichText != value)
				{
					m_isRichText = value;
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
					m_isInputParsingRequired = true;
				}
			}
		}

		public bool parseCtrlCharacters
		{
			get
			{
				return m_parseCtrlCharacters;
			}
			set
			{
				if (m_parseCtrlCharacters != value)
				{
					m_parseCtrlCharacters = value;
					m_havePropertiesChanged = true;
					m_isCalculateSizeRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
					m_isInputParsingRequired = true;
				}
			}
		}

		public bool isOverlay
		{
			get
			{
				return m_isOverlay;
			}
			set
			{
				if (m_isOverlay != value)
				{
					m_isOverlay = value;
					SetShaderDepth();
					m_havePropertiesChanged = true;
					SetVerticesDirty();
				}
			}
		}

		public bool isOrthographic
		{
			get
			{
				return m_isOrthographic;
			}
			set
			{
				if (m_isOrthographic != value)
				{
					m_havePropertiesChanged = true;
					m_isOrthographic = value;
					SetVerticesDirty();
				}
			}
		}

		public bool enableCulling
		{
			get
			{
				return m_isCullingEnabled;
			}
			set
			{
				if (m_isCullingEnabled != value)
				{
					m_isCullingEnabled = value;
					SetCulling();
					m_havePropertiesChanged = true;
				}
			}
		}

		public bool ignoreRectMaskCulling
		{
			get
			{
				return m_ignoreRectMaskCulling;
			}
			set
			{
				if (m_ignoreRectMaskCulling != value)
				{
					m_ignoreRectMaskCulling = value;
					m_havePropertiesChanged = true;
				}
			}
		}

		public bool ignoreVisibility
		{
			get
			{
				return m_ignoreCulling;
			}
			set
			{
				if (m_ignoreCulling != value)
				{
					m_havePropertiesChanged = true;
					m_ignoreCulling = value;
				}
			}
		}

		public TextureMappingOptions horizontalMapping
		{
			get
			{
				return m_horizontalMapping;
			}
			set
			{
				if (m_horizontalMapping != value)
				{
					m_havePropertiesChanged = true;
					m_horizontalMapping = value;
					SetVerticesDirty();
				}
			}
		}

		public TextureMappingOptions verticalMapping
		{
			get
			{
				return m_verticalMapping;
			}
			set
			{
				if (m_verticalMapping != value)
				{
					m_havePropertiesChanged = true;
					m_verticalMapping = value;
					SetVerticesDirty();
				}
			}
		}

		public float mappingUvLineOffset
		{
			get
			{
				return m_uvLineOffset;
			}
			set
			{
				if (m_uvLineOffset != value)
				{
					m_havePropertiesChanged = true;
					m_uvLineOffset = value;
					SetVerticesDirty();
				}
			}
		}

		public TextRenderFlags renderMode
		{
			get
			{
				return m_renderMode;
			}
			set
			{
				if (m_renderMode != value)
				{
					m_renderMode = value;
					m_havePropertiesChanged = true;
				}
			}
		}

		public VertexSortingOrder geometrySortingOrder
		{
			get
			{
				return m_geometrySortingOrder;
			}
			set
			{
				m_geometrySortingOrder = value;
				m_havePropertiesChanged = true;
				SetVerticesDirty();
			}
		}

		public int firstVisibleCharacter
		{
			get
			{
				return m_firstVisibleCharacter;
			}
			set
			{
				if (m_firstVisibleCharacter != value)
				{
					m_havePropertiesChanged = true;
					m_firstVisibleCharacter = value;
					SetVerticesDirty();
				}
			}
		}

		public int maxVisibleCharacters
		{
			get
			{
				return m_maxVisibleCharacters;
			}
			set
			{
				if (m_maxVisibleCharacters != value)
				{
					m_havePropertiesChanged = true;
					m_maxVisibleCharacters = value;
					SetVerticesDirty();
				}
			}
		}

		public int maxVisibleWords
		{
			get
			{
				return m_maxVisibleWords;
			}
			set
			{
				if (m_maxVisibleWords != value)
				{
					m_havePropertiesChanged = true;
					m_maxVisibleWords = value;
					SetVerticesDirty();
				}
			}
		}

		public int maxVisibleLines
		{
			get
			{
				return m_maxVisibleLines;
			}
			set
			{
				if (m_maxVisibleLines != value)
				{
					m_havePropertiesChanged = true;
					m_isInputParsingRequired = true;
					m_maxVisibleLines = value;
					SetVerticesDirty();
				}
			}
		}

		public bool useMaxVisibleDescender
		{
			get
			{
				return m_useMaxVisibleDescender;
			}
			set
			{
				if (m_useMaxVisibleDescender != value)
				{
					m_havePropertiesChanged = true;
					m_isInputParsingRequired = true;
					SetVerticesDirty();
				}
			}
		}

		public int pageToDisplay
		{
			get
			{
				return m_pageToDisplay;
			}
			set
			{
				if (m_pageToDisplay != value)
				{
					m_havePropertiesChanged = true;
					m_pageToDisplay = value;
					SetVerticesDirty();
				}
			}
		}

		public virtual Vector4 margin
		{
			get
			{
				return m_margin;
			}
			set
			{
				if (!(m_margin == value))
				{
					m_margin = value;
					ComputeMarginSize();
					m_havePropertiesChanged = true;
					SetVerticesDirty();
				}
			}
		}

		public TMP_TextInfo textInfo
		{
			get
			{
				return m_textInfo;
			}
		}

		public bool havePropertiesChanged
		{
			get
			{
				return m_havePropertiesChanged;
			}
			set
			{
				if (m_havePropertiesChanged != value)
				{
					m_havePropertiesChanged = value;
					m_isInputParsingRequired = true;
					SetAllDirty();
				}
			}
		}

		public bool isUsingLegacyAnimationComponent
		{
			get
			{
				return m_isUsingLegacyAnimationComponent;
			}
			set
			{
				m_isUsingLegacyAnimationComponent = value;
			}
		}

		public Transform transform
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

		public RectTransform rectTransform
		{
			get
			{
				if (m_rectTransform == null)
				{
					m_rectTransform = base.GetComponent<RectTransform>();
				}
				return m_rectTransform;
			}
		}

		public virtual bool autoSizeTextContainer
		{
			get;
			set;
		}

		public virtual Mesh mesh
		{
			get
			{
				return m_mesh;
			}
		}

		public bool isVolumetricText
		{
			get
			{
				return m_isVolumetricText;
			}
			set
			{
				if (m_isVolumetricText != value)
				{
					m_havePropertiesChanged = value;
					m_textInfo.ResetVertexLayout(value);
					m_isInputParsingRequired = true;
					SetVerticesDirty();
					SetLayoutDirty();
				}
			}
		}

		public Bounds bounds
		{
			get
			{
				if (m_mesh == null)
				{
					return default(Bounds);
				}
				return GetCompoundBounds();
			}
		}

		public Bounds textBounds
		{
			get
			{
				if (m_textInfo == null)
				{
					return default(Bounds);
				}
				return GetTextBounds();
			}
		}

		protected TMP_SpriteAnimator spriteAnimator
		{
			get
			{
				if (m_spriteAnimator == null)
				{
					m_spriteAnimator = base.GetComponent<TMP_SpriteAnimator>();
					if (m_spriteAnimator == null)
					{
						m_spriteAnimator = base.gameObject.AddComponent<TMP_SpriteAnimator>();
					}
				}
				return m_spriteAnimator;
			}
		}

		public float flexibleHeight
		{
			get
			{
				return m_flexibleHeight;
			}
		}

		public float flexibleWidth
		{
			get
			{
				return m_flexibleWidth;
			}
		}

		public float minWidth
		{
			get
			{
				return m_minWidth;
			}
		}

		public float minHeight
		{
			get
			{
				return m_minHeight;
			}
		}

		public float maxWidth
		{
			get
			{
				return m_maxWidth;
			}
		}

		public float maxHeight
		{
			get
			{
				return m_maxHeight;
			}
		}

		protected LayoutElement layoutElement
		{
			get
			{
				if ((UnityEngine.Object)(object)m_LayoutElement == null)
				{
					m_LayoutElement = base.GetComponent<LayoutElement>();
				}
				return m_LayoutElement;
			}
		}

		public virtual float preferredWidth
		{
			get
			{
				if (!m_isPreferredWidthDirty)
				{
					return m_preferredWidth;
				}
				m_preferredWidth = GetPreferredWidth();
				return m_preferredWidth;
			}
		}

		public virtual float preferredHeight
		{
			get
			{
				if (!m_isPreferredHeightDirty)
				{
					return m_preferredHeight;
				}
				m_preferredHeight = GetPreferredHeight();
				return m_preferredHeight;
			}
		}

		public virtual float renderedWidth
		{
			get
			{
				return GetRenderedWidth();
			}
		}

		public virtual float renderedHeight
		{
			get
			{
				return GetRenderedHeight();
			}
		}

		public int layoutPriority
		{
			get
			{
				return m_layoutPriority;
			}
		}

		protected virtual void LoadFontAsset()
		{
		}

		protected virtual void SetSharedMaterial(Material mat)
		{
		}

		protected virtual Material GetMaterial(Material mat)
		{
			return null;
		}

		protected virtual void SetFontBaseMaterial(Material mat)
		{
		}

		protected virtual Material[] GetSharedMaterials()
		{
			return null;
		}

		protected virtual void SetSharedMaterials(Material[] materials)
		{
		}

		protected virtual Material[] GetMaterials(Material[] mats)
		{
			return null;
		}

		protected virtual Material CreateMaterialInstance(Material source)
		{
			Material material = new Material(source);
			material.shaderKeywords = source.shaderKeywords;
			material.name += " (Instance)";
			return material;
		}

		protected void SetVertexColorGradient(TMP_ColorGradient gradient)
		{
			if (!(gradient == null))
			{
				m_fontColorGradient.bottomLeft = gradient.bottomLeft;
				m_fontColorGradient.bottomRight = gradient.bottomRight;
				m_fontColorGradient.topLeft = gradient.topLeft;
				m_fontColorGradient.topRight = gradient.topRight;
				SetVerticesDirty();
			}
		}

		protected void SetTextSortingOrder(VertexSortingOrder order)
		{
		}

		protected void SetTextSortingOrder(int[] order)
		{
		}

		protected virtual void SetFaceColor(Color32 color)
		{
		}

		protected virtual void SetOutlineColor(Color32 color)
		{
		}

		protected virtual void SetOutlineThickness(float thickness)
		{
		}

		protected virtual void SetShaderDepth()
		{
		}

		protected virtual void SetCulling()
		{
		}

		protected virtual float GetPaddingForMaterial()
		{
			return 0f;
		}

		protected virtual float GetPaddingForMaterial(Material mat)
		{
			return 0f;
		}

		protected virtual Vector3[] GetTextContainerLocalCorners()
		{
			return null;
		}

		public virtual void ForceMeshUpdate()
		{
		}

		public virtual void ForceMeshUpdate(bool ignoreActiveState)
		{
		}

		internal void SetTextInternal(string text)
		{
			m_text = text;
			m_renderMode = TextRenderFlags.DontRender;
			m_isInputParsingRequired = true;
			ForceMeshUpdate();
			m_renderMode = TextRenderFlags.Render;
		}

		public virtual void UpdateGeometry(Mesh mesh, int index)
		{
		}

		public virtual void UpdateVertexData(TMP_VertexDataUpdateFlags flags)
		{
		}

		public virtual void UpdateVertexData()
		{
		}

		public virtual void SetVertices(Vector3[] vertices)
		{
		}

		public virtual void UpdateMeshPadding()
		{
		}

		public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
		{
			//((Graphic)this).CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
			base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
			InternalCrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha);
		}

		public override void CrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
		{
			//((Graphic)this).CrossFadeAlpha(alpha, duration, ignoreTimeScale);
			base.CrossFadeAlpha(alpha, duration, ignoreTimeScale);
			InternalCrossFadeAlpha(alpha, duration, ignoreTimeScale);
		}

		protected virtual void InternalCrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha)
		{
		}

		protected virtual void InternalCrossFadeAlpha(float alpha, float duration, bool ignoreTimeScale)
		{
		}

		protected void ParseInputText()
		{
			m_isInputParsingRequired = false;
			switch (m_inputSource)
			{
			case TextInputSources.Text:
			case TextInputSources.String:
				StringToCharArray(m_text, ref m_char_buffer);
				break;
			case TextInputSources.SetText:
				SetTextArrayToCharArray(m_input_CharArray, ref m_char_buffer);
				break;
			}
			SetArraySizes(m_char_buffer);
		}

		public void SetText(string text)
		{
			SetText(text, true);
		}

		public void SetText(string text, bool syncTextInputBox)
		{
			if (!(text == old_text))
			{
				old_text = text;
				m_inputSource = TextInputSources.SetCharArray;
				StringToCharArray(text, ref m_char_buffer);
				if (syncTextInputBox)
				{
					m_text = text;
				}
				m_isInputParsingRequired = true;
				m_havePropertiesChanged = true;
				m_isCalculateSizeRequired = true;
				SetVerticesDirty();
				SetLayoutDirty();
			}
		}

		public void SetText(string text, float arg0)
		{
			SetText(text, arg0, 255f, 255f);
		}

		public void SetText(string text, float arg0, float arg1)
		{
			SetText(text, arg0, arg1, 255f);
		}

		public void SetText(string text, float arg0, float arg1, float arg2)
		{
			if (text == old_text && arg0 == old_arg0 && arg1 == old_arg1 && arg2 == old_arg2)
			{
				return;
			}
			old_text = text;
			old_arg1 = 255f;
			old_arg2 = 255f;
			int precision = 0;
			int index = 0;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (c == '{')
				{
					if (text[i + 2] == ':')
					{
						precision = text[i + 3] - 48;
					}
					switch (text[i + 1])
					{
					case '0':
						old_arg0 = arg0;
						AddFloatToCharArray(arg0, ref index, precision);
						break;
					case '1':
						old_arg1 = arg1;
						AddFloatToCharArray(arg1, ref index, precision);
						break;
					case '2':
						old_arg2 = arg2;
						AddFloatToCharArray(arg2, ref index, precision);
						break;
					}
					i = ((text[i + 2] != ':') ? (i + 2) : (i + 4));
				}
				else
				{
					m_input_CharArray[index] = c;
					index++;
				}
			}
			m_input_CharArray[index] = '\0';
			m_charArray_Length = index;
			m_text = new string(m_input_CharArray, 0, index);
			m_inputSource = TextInputSources.SetText;
			m_isInputParsingRequired = true;
			m_havePropertiesChanged = true;
			m_isCalculateSizeRequired = true;
			SetVerticesDirty();
			SetLayoutDirty();
		}

		public void SetText(StringBuilder text)
		{
			m_inputSource = TextInputSources.SetCharArray;
			m_text = text.ToString();
			StringBuilderToIntArray(text, ref m_char_buffer);
			m_isInputParsingRequired = true;
			m_havePropertiesChanged = true;
			m_isCalculateSizeRequired = true;
			SetVerticesDirty();
			SetLayoutDirty();
		}

		public void SetCharArray(char[] sourceText)
		{
			if (sourceText == null || sourceText.Length == 0)
			{
				return;
			}
			if (m_char_buffer == null)
			{
				m_char_buffer = new int[8];
			}
			m_styleStack.Clear();
			int writeIndex = 0;
			for (int i = 0; i < sourceText.Length; i++)
			{
				if (sourceText[i] == '\\' && i < sourceText.Length - 1)
				{
					switch (sourceText[i + 1])
					{
					case 'n':
						if (writeIndex == m_char_buffer.Length)
						{
							ResizeInternalArray(ref m_char_buffer);
						}
						m_char_buffer[writeIndex] = 10;
						i++;
						writeIndex++;
						continue;
					case 'r':
						if (writeIndex == m_char_buffer.Length)
						{
							ResizeInternalArray(ref m_char_buffer);
						}
						m_char_buffer[writeIndex] = 13;
						i++;
						writeIndex++;
						continue;
					case 't':
						if (writeIndex == m_char_buffer.Length)
						{
							ResizeInternalArray(ref m_char_buffer);
						}
						m_char_buffer[writeIndex] = 9;
						i++;
						writeIndex++;
						continue;
					}
				}
				if (sourceText[i] == '<')
				{
					if (IsTagName(ref sourceText, "<BR>", i))
					{
						if (writeIndex == m_char_buffer.Length)
						{
							ResizeInternalArray(ref m_char_buffer);
						}
						m_char_buffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref sourceText, "<STYLE=", i))
					{
						int srcOffset = 0;
						if (ReplaceOpeningStyleTag(ref sourceText, i, out srcOffset, ref m_char_buffer, ref writeIndex))
						{
							i = srcOffset;
							continue;
						}
					}
					else if (IsTagName(ref sourceText, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref sourceText, i, ref m_char_buffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == m_char_buffer.Length)
				{
					ResizeInternalArray(ref m_char_buffer);
				}
				m_char_buffer[writeIndex] = sourceText[i];
				writeIndex++;
			}
			if (writeIndex == m_char_buffer.Length)
			{
				ResizeInternalArray(ref m_char_buffer);
			}
			m_char_buffer[writeIndex] = 0;
			m_inputSource = TextInputSources.SetCharArray;
			m_havePropertiesChanged = true;
			m_isInputParsingRequired = true;
		}

		public void SetCharArray(int[] sourceText, int start, int length)
		{
			if (sourceText == null || sourceText.Length == 0 || length == 0)
			{
				return;
			}
			if (m_char_buffer == null)
			{
				m_char_buffer = new int[8];
			}
			m_styleStack.Clear();
			int writeIndex = 0;
			for (int i = 0; i < length; i++)
			{
				if (sourceText[start + i] == 92 && i < length - 1)
				{
					switch (sourceText[start + i + 1])
					{
					case 110:
						if (writeIndex == m_char_buffer.Length)
						{
							ResizeInternalArray(ref m_char_buffer);
						}
						m_char_buffer[writeIndex] = 10;
						i++;
						writeIndex++;
						continue;
					case 114:
						if (writeIndex == m_char_buffer.Length)
						{
							ResizeInternalArray(ref m_char_buffer);
						}
						m_char_buffer[writeIndex] = 13;
						i++;
						writeIndex++;
						continue;
					case 116:
						if (writeIndex == m_char_buffer.Length)
						{
							ResizeInternalArray(ref m_char_buffer);
						}
						m_char_buffer[writeIndex] = 9;
						i++;
						writeIndex++;
						continue;
					}
				}
				if (sourceText[i] == 60)
				{
					if (IsTagName(ref sourceText, "<BR>", i))
					{
						if (writeIndex == m_char_buffer.Length)
						{
							ResizeInternalArray(ref m_char_buffer);
						}
						m_char_buffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref sourceText, "<STYLE=", i))
					{
						int srcOffset = 0;
						if (ReplaceOpeningStyleTag(ref sourceText, i, out srcOffset, ref m_char_buffer, ref writeIndex))
						{
							i = srcOffset;
							continue;
						}
					}
					else if (IsTagName(ref sourceText, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref sourceText, i, ref m_char_buffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == m_char_buffer.Length)
				{
					ResizeInternalArray(ref m_char_buffer);
				}
				m_char_buffer[writeIndex] = sourceText[start + i];
				writeIndex++;
			}
			if (writeIndex == m_char_buffer.Length)
			{
				ResizeInternalArray(ref m_char_buffer);
			}
			m_char_buffer[writeIndex] = 0;
			m_inputSource = TextInputSources.SetCharArray;
			m_havePropertiesChanged = true;
			m_isInputParsingRequired = true;
			m_isCalculateSizeRequired = true;
			SetVerticesDirty();
			SetLayoutDirty();
		}

		protected void SetTextArrayToCharArray(char[] sourceText, ref int[] charBuffer)
		{
			if (sourceText == null || m_charArray_Length == 0)
			{
				return;
			}
			if (charBuffer == null)
			{
				charBuffer = new int[8];
			}
			m_styleStack.Clear();
			int writeIndex = 0;
			for (int i = 0; i < m_charArray_Length; i++)
			{
				if (char.IsHighSurrogate(sourceText[i]) && char.IsLowSurrogate(sourceText[i + 1]))
				{
					if (writeIndex == charBuffer.Length)
					{
						ResizeInternalArray(ref charBuffer);
					}
					charBuffer[writeIndex] = char.ConvertToUtf32(sourceText[i], sourceText[i + 1]);
					i++;
					writeIndex++;
					continue;
				}
				if (sourceText[i] == '<')
				{
					if (IsTagName(ref sourceText, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref sourceText, "<STYLE=", i))
					{
						int srcOffset = 0;
						if (ReplaceOpeningStyleTag(ref sourceText, i, out srcOffset, ref charBuffer, ref writeIndex))
						{
							i = srcOffset;
							continue;
						}
					}
					else if (IsTagName(ref sourceText, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref sourceText, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = sourceText[i];
				writeIndex++;
			}
			if (writeIndex == charBuffer.Length)
			{
				ResizeInternalArray(ref charBuffer);
			}
			charBuffer[writeIndex] = 0;
		}

		protected void StringToCharArray(string sourceText, ref int[] charBuffer)
		{
			if (sourceText == null)
			{
				charBuffer[0] = 0;
				return;
			}
			if (charBuffer == null)
			{
				charBuffer = new int[8];
			}
			m_styleStack.SetDefault(0);
			int writeIndex = 0;
			for (int i = 0; i < sourceText.Length; i++)
			{
				if (m_inputSource == TextInputSources.Text && sourceText[i] == '\\' && sourceText.Length > i + 1)
				{
					switch (sourceText[i + 1])
					{
					case 'U':
						if (sourceText.Length > i + 9)
						{
							if (writeIndex == charBuffer.Length)
							{
								ResizeInternalArray(ref charBuffer);
							}
							charBuffer[writeIndex] = GetUTF32(i + 2);
							i += 9;
							writeIndex++;
							continue;
						}
						break;
					case '\\':
						if (!m_parseCtrlCharacters || sourceText.Length <= i + 2)
						{
							break;
						}
						if (writeIndex + 2 > charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = sourceText[i + 1];
						charBuffer[writeIndex + 1] = sourceText[i + 2];
						i += 2;
						writeIndex += 2;
						continue;
					case 'n':
						if (!m_parseCtrlCharacters)
						{
							break;
						}
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						i++;
						writeIndex++;
						continue;
					case 'r':
						if (!m_parseCtrlCharacters)
						{
							break;
						}
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 13;
						i++;
						writeIndex++;
						continue;
					case 't':
						if (!m_parseCtrlCharacters)
						{
							break;
						}
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 9;
						i++;
						writeIndex++;
						continue;
					case 'u':
						if (sourceText.Length > i + 5)
						{
							if (writeIndex == charBuffer.Length)
							{
								ResizeInternalArray(ref charBuffer);
							}
							charBuffer[writeIndex] = (ushort)GetUTF16(i + 2);
							i += 5;
							writeIndex++;
							continue;
						}
						break;
					}
				}
				if (char.IsHighSurrogate(sourceText[i]) && char.IsLowSurrogate(sourceText[i + 1]))
				{
					if (writeIndex == charBuffer.Length)
					{
						ResizeInternalArray(ref charBuffer);
					}
					charBuffer[writeIndex] = char.ConvertToUtf32(sourceText[i], sourceText[i + 1]);
					i++;
					writeIndex++;
					continue;
				}
				if (sourceText[i] == '<' && m_isRichText)
				{
					if (IsTagName(ref sourceText, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref sourceText, "<STYLE=", i))
					{
						int srcOffset = 0;
						if (ReplaceOpeningStyleTag(ref sourceText, i, out srcOffset, ref charBuffer, ref writeIndex))
						{
							i = srcOffset;
							continue;
						}
					}
					else if (IsTagName(ref sourceText, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref sourceText, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = sourceText[i];
				writeIndex++;
			}
			if (writeIndex == charBuffer.Length)
			{
				ResizeInternalArray(ref charBuffer);
			}
			charBuffer[writeIndex] = 0;
		}

		protected void StringBuilderToIntArray(StringBuilder sourceText, ref int[] charBuffer)
		{
			if (sourceText == null)
			{
				charBuffer[0] = 0;
				return;
			}
			if (charBuffer == null)
			{
				charBuffer = new int[8];
			}
			m_styleStack.Clear();
			m_text = sourceText.ToString();
			int writeIndex = 0;
			for (int i = 0; i < sourceText.Length; i++)
			{
				if (m_parseCtrlCharacters && sourceText[i] == '\\' && sourceText.Length > i + 1)
				{
					switch (sourceText[i + 1])
					{
					case 'U':
						if (sourceText.Length > i + 9)
						{
							if (writeIndex == charBuffer.Length)
							{
								ResizeInternalArray(ref charBuffer);
							}
							charBuffer[writeIndex] = GetUTF32(i + 2);
							i += 9;
							writeIndex++;
							continue;
						}
						break;
					case '\\':
						if (sourceText.Length <= i + 2)
						{
							break;
						}
						if (writeIndex + 2 > charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = sourceText[i + 1];
						charBuffer[writeIndex + 1] = sourceText[i + 2];
						i += 2;
						writeIndex += 2;
						continue;
					case 'n':
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						i++;
						writeIndex++;
						continue;
					case 'r':
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 13;
						i++;
						writeIndex++;
						continue;
					case 't':
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 9;
						i++;
						writeIndex++;
						continue;
					case 'u':
						if (sourceText.Length > i + 5)
						{
							if (writeIndex == charBuffer.Length)
							{
								ResizeInternalArray(ref charBuffer);
							}
							charBuffer[writeIndex] = (ushort)GetUTF16(i + 2);
							i += 5;
							writeIndex++;
							continue;
						}
						break;
					}
				}
				if (char.IsHighSurrogate(sourceText[i]) && char.IsLowSurrogate(sourceText[i + 1]))
				{
					if (writeIndex == charBuffer.Length)
					{
						ResizeInternalArray(ref charBuffer);
					}
					charBuffer[writeIndex] = char.ConvertToUtf32(sourceText[i], sourceText[i + 1]);
					i++;
					writeIndex++;
					continue;
				}
				if (sourceText[i] == '<')
				{
					if (IsTagName(ref sourceText, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref sourceText, "<STYLE=", i))
					{
						int srcOffset = 0;
						if (ReplaceOpeningStyleTag(ref sourceText, i, out srcOffset, ref charBuffer, ref writeIndex))
						{
							i = srcOffset;
							continue;
						}
					}
					else if (IsTagName(ref sourceText, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref sourceText, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = sourceText[i];
				writeIndex++;
			}
			if (writeIndex == charBuffer.Length)
			{
				ResizeInternalArray(ref charBuffer);
			}
			charBuffer[writeIndex] = 0;
		}

		private bool ReplaceOpeningStyleTag(ref string sourceText, int srcIndex, out int srcOffset, ref int[] charBuffer, ref int writeIndex)
		{
			int tagHashCode = GetTagHashCode(ref sourceText, srcIndex + 7, out srcOffset);
			TMP_Style style = TMP_StyleSheet.GetStyle(tagHashCode);
			if (style == null || srcOffset == 0)
			{
				return false;
			}
			m_styleStack.Add(style.hashCode);
			int num = style.styleOpeningTagArray.Length;
			int[] text = style.styleOpeningTagArray;
			for (int i = 0; i < num; i++)
			{
				int num2 = text[i];
				if (num2 == 60)
				{
					if (IsTagName(ref text, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref text, "<STYLE=", i))
					{
						int srcOffset2 = 0;
						if (ReplaceOpeningStyleTag(ref text, i, out srcOffset2, ref charBuffer, ref writeIndex))
						{
							i = srcOffset2;
							continue;
						}
					}
					else if (IsTagName(ref text, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref text, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = num2;
				writeIndex++;
			}
			return true;
		}

		private bool ReplaceOpeningStyleTag(ref int[] sourceText, int srcIndex, out int srcOffset, ref int[] charBuffer, ref int writeIndex)
		{
			int tagHashCode = GetTagHashCode(ref sourceText, srcIndex + 7, out srcOffset);
			TMP_Style style = TMP_StyleSheet.GetStyle(tagHashCode);
			if (style == null || srcOffset == 0)
			{
				return false;
			}
			m_styleStack.Add(style.hashCode);
			int num = style.styleOpeningTagArray.Length;
			int[] text = style.styleOpeningTagArray;
			for (int i = 0; i < num; i++)
			{
				int num2 = text[i];
				if (num2 == 60)
				{
					if (IsTagName(ref text, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref text, "<STYLE=", i))
					{
						int srcOffset2 = 0;
						if (ReplaceOpeningStyleTag(ref text, i, out srcOffset2, ref charBuffer, ref writeIndex))
						{
							i = srcOffset2;
							continue;
						}
					}
					else if (IsTagName(ref text, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref text, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = num2;
				writeIndex++;
			}
			return true;
		}

		private bool ReplaceOpeningStyleTag(ref char[] sourceText, int srcIndex, out int srcOffset, ref int[] charBuffer, ref int writeIndex)
		{
			int tagHashCode = GetTagHashCode(ref sourceText, srcIndex + 7, out srcOffset);
			TMP_Style style = TMP_StyleSheet.GetStyle(tagHashCode);
			if (style == null || srcOffset == 0)
			{
				return false;
			}
			m_styleStack.Add(style.hashCode);
			int num = style.styleOpeningTagArray.Length;
			int[] text = style.styleOpeningTagArray;
			for (int i = 0; i < num; i++)
			{
				int num2 = text[i];
				if (num2 == 60)
				{
					if (IsTagName(ref text, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref text, "<STYLE=", i))
					{
						int srcOffset2 = 0;
						if (ReplaceOpeningStyleTag(ref text, i, out srcOffset2, ref charBuffer, ref writeIndex))
						{
							i = srcOffset2;
							continue;
						}
					}
					else if (IsTagName(ref text, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref text, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = num2;
				writeIndex++;
			}
			return true;
		}

		private bool ReplaceOpeningStyleTag(ref StringBuilder sourceText, int srcIndex, out int srcOffset, ref int[] charBuffer, ref int writeIndex)
		{
			int tagHashCode = GetTagHashCode(ref sourceText, srcIndex + 7, out srcOffset);
			TMP_Style style = TMP_StyleSheet.GetStyle(tagHashCode);
			if (style == null || srcOffset == 0)
			{
				return false;
			}
			m_styleStack.Add(style.hashCode);
			int num = style.styleOpeningTagArray.Length;
			int[] text = style.styleOpeningTagArray;
			for (int i = 0; i < num; i++)
			{
				int num2 = text[i];
				if (num2 == 60)
				{
					if (IsTagName(ref text, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref text, "<STYLE=", i))
					{
						int srcOffset2 = 0;
						if (ReplaceOpeningStyleTag(ref text, i, out srcOffset2, ref charBuffer, ref writeIndex))
						{
							i = srcOffset2;
							continue;
						}
					}
					else if (IsTagName(ref text, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref text, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = num2;
				writeIndex++;
			}
			return true;
		}

		private bool ReplaceClosingStyleTag(ref string sourceText, int srcIndex, ref int[] charBuffer, ref int writeIndex)
		{
			int hashCode = m_styleStack.CurrentItem();
			TMP_Style style = TMP_StyleSheet.GetStyle(hashCode);
			m_styleStack.Remove();
			if (style == null)
			{
				return false;
			}
			int num = style.styleClosingTagArray.Length;
			int[] text = style.styleClosingTagArray;
			for (int i = 0; i < num; i++)
			{
				int num2 = text[i];
				if (num2 == 60)
				{
					if (IsTagName(ref text, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref text, "<STYLE=", i))
					{
						int srcOffset = 0;
						if (ReplaceOpeningStyleTag(ref text, i, out srcOffset, ref charBuffer, ref writeIndex))
						{
							i = srcOffset;
							continue;
						}
					}
					else if (IsTagName(ref text, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref text, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = num2;
				writeIndex++;
			}
			return true;
		}

		private bool ReplaceClosingStyleTag(ref int[] sourceText, int srcIndex, ref int[] charBuffer, ref int writeIndex)
		{
			int hashCode = m_styleStack.CurrentItem();
			TMP_Style style = TMP_StyleSheet.GetStyle(hashCode);
			m_styleStack.Remove();
			if (style == null)
			{
				return false;
			}
			int num = style.styleClosingTagArray.Length;
			int[] text = style.styleClosingTagArray;
			for (int i = 0; i < num; i++)
			{
				int num2 = text[i];
				if (num2 == 60)
				{
					if (IsTagName(ref text, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref text, "<STYLE=", i))
					{
						int srcOffset = 0;
						if (ReplaceOpeningStyleTag(ref text, i, out srcOffset, ref charBuffer, ref writeIndex))
						{
							i = srcOffset;
							continue;
						}
					}
					else if (IsTagName(ref text, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref text, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = num2;
				writeIndex++;
			}
			return true;
		}

		private bool ReplaceClosingStyleTag(ref char[] sourceText, int srcIndex, ref int[] charBuffer, ref int writeIndex)
		{
			int hashCode = m_styleStack.CurrentItem();
			TMP_Style style = TMP_StyleSheet.GetStyle(hashCode);
			m_styleStack.Remove();
			if (style == null)
			{
				return false;
			}
			int num = style.styleClosingTagArray.Length;
			int[] text = style.styleClosingTagArray;
			for (int i = 0; i < num; i++)
			{
				int num2 = text[i];
				if (num2 == 60)
				{
					if (IsTagName(ref text, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref text, "<STYLE=", i))
					{
						int srcOffset = 0;
						if (ReplaceOpeningStyleTag(ref text, i, out srcOffset, ref charBuffer, ref writeIndex))
						{
							i = srcOffset;
							continue;
						}
					}
					else if (IsTagName(ref text, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref text, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = num2;
				writeIndex++;
			}
			return true;
		}

		private bool ReplaceClosingStyleTag(ref StringBuilder sourceText, int srcIndex, ref int[] charBuffer, ref int writeIndex)
		{
			int hashCode = m_styleStack.CurrentItem();
			TMP_Style style = TMP_StyleSheet.GetStyle(hashCode);
			m_styleStack.Remove();
			if (style == null)
			{
				return false;
			}
			int num = style.styleClosingTagArray.Length;
			int[] text = style.styleClosingTagArray;
			for (int i = 0; i < num; i++)
			{
				int num2 = text[i];
				if (num2 == 60)
				{
					if (IsTagName(ref text, "<BR>", i))
					{
						if (writeIndex == charBuffer.Length)
						{
							ResizeInternalArray(ref charBuffer);
						}
						charBuffer[writeIndex] = 10;
						writeIndex++;
						i += 3;
						continue;
					}
					if (IsTagName(ref text, "<STYLE=", i))
					{
						int srcOffset = 0;
						if (ReplaceOpeningStyleTag(ref text, i, out srcOffset, ref charBuffer, ref writeIndex))
						{
							i = srcOffset;
							continue;
						}
					}
					else if (IsTagName(ref text, "</STYLE>", i))
					{
						ReplaceClosingStyleTag(ref text, i, ref charBuffer, ref writeIndex);
						i += 7;
						continue;
					}
				}
				if (writeIndex == charBuffer.Length)
				{
					ResizeInternalArray(ref charBuffer);
				}
				charBuffer[writeIndex] = num2;
				writeIndex++;
			}
			return true;
		}

		private bool IsTagName(ref string text, string tag, int index)
		{
			if (text.Length < index + tag.Length)
			{
				return false;
			}
			for (int i = 0; i < tag.Length; i++)
			{
				if (TMP_TextUtilities.ToUpperFast(text[index + i]) != tag[i])
				{
					return false;
				}
			}
			return true;
		}

		private bool IsTagName(ref char[] text, string tag, int index)
		{
			if (text.Length < index + tag.Length)
			{
				return false;
			}
			for (int i = 0; i < tag.Length; i++)
			{
				if (TMP_TextUtilities.ToUpperFast(text[index + i]) != tag[i])
				{
					return false;
				}
			}
			return true;
		}

		private bool IsTagName(ref int[] text, string tag, int index)
		{
			if (text.Length < index + tag.Length)
			{
				return false;
			}
			for (int i = 0; i < tag.Length; i++)
			{
				if (TMP_TextUtilities.ToUpperFast((char)text[index + i]) != tag[i])
				{
					return false;
				}
			}
			return true;
		}

		private bool IsTagName(ref StringBuilder text, string tag, int index)
		{
			if (text.Length < index + tag.Length)
			{
				return false;
			}
			for (int i = 0; i < tag.Length; i++)
			{
				if (TMP_TextUtilities.ToUpperFast(text[index + i]) != tag[i])
				{
					return false;
				}
			}
			return true;
		}

		private int GetTagHashCode(ref string text, int index, out int closeIndex)
		{
			int num = 0;
			closeIndex = 0;
			for (int i = index; i < text.Length; i++)
			{
				if (text[i] != '"')
				{
					if (text[i] == '>')
					{
						closeIndex = i;
						break;
					}
					num = (((num << 5) + num) ^ text[i]);
				}
			}
			return num;
		}

		private int GetTagHashCode(ref char[] text, int index, out int closeIndex)
		{
			int num = 0;
			closeIndex = 0;
			for (int i = index; i < text.Length; i++)
			{
				if (text[i] != '"')
				{
					if (text[i] == '>')
					{
						closeIndex = i;
						break;
					}
					num = (((num << 5) + num) ^ text[i]);
				}
			}
			return num;
		}

		private int GetTagHashCode(ref int[] text, int index, out int closeIndex)
		{
			int num = 0;
			closeIndex = 0;
			for (int i = index; i < text.Length; i++)
			{
				if (text[i] != 34)
				{
					if (text[i] == 62)
					{
						closeIndex = i;
						break;
					}
					num = (((num << 5) + num) ^ text[i]);
				}
			}
			return num;
		}

		private int GetTagHashCode(ref StringBuilder text, int index, out int closeIndex)
		{
			int num = 0;
			closeIndex = 0;
			for (int i = index; i < text.Length; i++)
			{
				if (text[i] != '"')
				{
					if (text[i] == '>')
					{
						closeIndex = i;
						break;
					}
					num = (((num << 5) + num) ^ text[i]);
				}
			}
			return num;
		}

		private void ResizeInternalArray<T>(ref T[] array)
		{
			int newSize = Mathf.NextPowerOfTwo(array.Length + 1);
			Array.Resize(ref array, newSize);
		}

		protected void AddFloatToCharArray(float number, ref int index, int precision)
		{
			if (number < 0f)
			{
				m_input_CharArray[index++] = '-';
				number = 0f - number;
			}
			number += k_Power[Mathf.Min(9, precision)];
			int num = (int)number;
			AddIntToCharArray(num, ref index, precision);
			if (precision > 0)
			{
				m_input_CharArray[index++] = '.';
				number -= (float)num;
				for (int i = 0; i < precision; i++)
				{
					number *= 10f;
					int num2 = (int)number;
					m_input_CharArray[index++] = (char)(num2 + 48);
					number -= (float)num2;
				}
			}
		}

		protected void AddIntToCharArray(int number, ref int index, int precision)
		{
			if (number < 0)
			{
				m_input_CharArray[index++] = '-';
				number = -number;
			}
			int num = index;
			do
			{
				m_input_CharArray[num++] = (char)(number % 10 + 48);
				number /= 10;
			}
			while (number > 0);
			int num2 = num;
			while (index + 1 < num)
			{
				num--;
				char c = m_input_CharArray[index];
				m_input_CharArray[index] = m_input_CharArray[num];
				m_input_CharArray[num] = c;
				index++;
			}
			index = num2;
		}

		protected virtual int SetArraySizes(int[] chars)
		{
			return 0;
		}

		protected virtual void GenerateTextMesh()
		{
		}

		public Vector2 GetPreferredValues()
		{
			if (m_isInputParsingRequired || m_isTextTruncated)
			{
				m_isCalculatingPreferredValues = true;
				ParseInputText();
			}
			float preferredWidth = GetPreferredWidth();
			float preferredHeight = GetPreferredHeight();
			return new Vector2(preferredWidth, preferredHeight);
		}

		public Vector2 GetPreferredValues(float width, float height)
		{
			if (m_isInputParsingRequired || m_isTextTruncated)
			{
				m_isCalculatingPreferredValues = true;
				ParseInputText();
			}
			Vector2 margin = new Vector2(width, height);
			float preferredWidth = GetPreferredWidth(margin);
			float preferredHeight = GetPreferredHeight(margin);
			return new Vector2(preferredWidth, preferredHeight);
		}

		public Vector2 GetPreferredValues(string text)
		{
			m_isCalculatingPreferredValues = true;
			StringToCharArray(text, ref m_char_buffer);
			SetArraySizes(m_char_buffer);
			Vector2 margin = k_LargePositiveVector2;
			float preferredWidth = GetPreferredWidth(margin);
			float preferredHeight = GetPreferredHeight(margin);
			return new Vector2(preferredWidth, preferredHeight);
		}

		public Vector2 GetPreferredValues(string text, float width, float height)
		{
			m_isCalculatingPreferredValues = true;
			StringToCharArray(text, ref m_char_buffer);
			SetArraySizes(m_char_buffer);
			Vector2 margin = new Vector2(width, height);
			float preferredWidth = GetPreferredWidth(margin);
			float preferredHeight = GetPreferredHeight(margin);
			return new Vector2(preferredWidth, preferredHeight);
		}

		protected float GetPreferredWidth()
		{
			float defaultFontSize = m_enableAutoSizing ? m_fontSizeMax : m_fontSize;
			m_minFontSize = m_fontSizeMin;
			m_maxFontSize = m_fontSizeMax;
			Vector2 marginSize = k_LargePositiveVector2;
			if (m_isInputParsingRequired || m_isTextTruncated)
			{
				m_isCalculatingPreferredValues = true;
				ParseInputText();
			}
			m_recursiveCount = 0;
			float x = CalculatePreferredValues(defaultFontSize, marginSize, true).x;
			m_isPreferredWidthDirty = false;
			return x;
		}

		protected float GetPreferredWidth(Vector2 margin)
		{
			float defaultFontSize = m_enableAutoSizing ? m_fontSizeMax : m_fontSize;
			m_minFontSize = m_fontSizeMin;
			m_maxFontSize = m_fontSizeMax;
			m_recursiveCount = 0;
			return CalculatePreferredValues(defaultFontSize, margin, true).x;
		}

		protected float GetPreferredHeight()
		{
			float defaultFontSize = m_enableAutoSizing ? m_fontSizeMax : m_fontSize;
			m_minFontSize = m_fontSizeMin;
			m_maxFontSize = m_fontSizeMax;
			Vector2 marginSize = new Vector2((m_marginWidth != 0f) ? m_marginWidth : k_LargePositiveFloat, k_LargePositiveFloat);
			if (m_isInputParsingRequired || m_isTextTruncated)
			{
				m_isCalculatingPreferredValues = true;
				ParseInputText();
			}
			m_recursiveCount = 0;
			float y = CalculatePreferredValues(defaultFontSize, marginSize, !m_enableAutoSizing).y;
			m_isPreferredHeightDirty = false;
			return y;
		}

		protected float GetPreferredHeight(Vector2 margin)
		{
			float defaultFontSize = m_enableAutoSizing ? m_fontSizeMax : m_fontSize;
			m_minFontSize = m_fontSizeMin;
			m_maxFontSize = m_fontSizeMax;
			m_recursiveCount = 0;
			return CalculatePreferredValues(defaultFontSize, margin, true).y;
		}

		public Vector2 GetRenderedValues()
		{
			return GetTextBounds().size;
		}

		public Vector2 GetRenderedValues(bool onlyVisibleCharacters)
		{
			return GetTextBounds(onlyVisibleCharacters).size;
		}

		protected float GetRenderedWidth()
		{
			return GetRenderedValues().x;
		}

		protected float GetRenderedWidth(bool onlyVisibleCharacters)
		{
			return GetRenderedValues(onlyVisibleCharacters).x;
		}

		protected float GetRenderedHeight()
		{
			return GetRenderedValues().y;
		}

		protected float GetRenderedHeight(bool onlyVisibleCharacters)
		{
			return GetRenderedValues(onlyVisibleCharacters).y;
		}

		protected virtual Vector2 CalculatePreferredValues(float defaultFontSize, Vector2 marginSize, bool ignoreTextAutoSizing)
		{
			if (m_fontAsset == null || m_fontAsset.characterDictionary == null)
			{
				Debug.LogWarning("Can't Generate Mesh! No Font Asset has been assigned to Object ID: " + base.GetInstanceID());
				return Vector2.zero;
			}
			if (m_char_buffer == null || m_char_buffer.Length == 0 || m_char_buffer[0] == 0)
			{
				return Vector2.zero;
			}
			m_currentFontAsset = m_fontAsset;
			m_currentMaterial = m_sharedMaterial;
			m_currentMaterialIndex = 0;
			m_materialReferenceStack.SetDefault(new MaterialReference(0, m_currentFontAsset, null, m_currentMaterial, m_padding));
			int totalCharacterCount = m_totalCharacterCount;
			if (m_internalCharacterInfo == null || totalCharacterCount > m_internalCharacterInfo.Length)
			{
				m_internalCharacterInfo = new TMP_CharacterInfo[(totalCharacterCount > 1024) ? (totalCharacterCount + 256) : Mathf.NextPowerOfTwo(totalCharacterCount)];
			}
			m_fontScale = defaultFontSize / m_currentFontAsset.fontInfo.PointSize * (m_isOrthographic ? 1f : 0.1f);
			m_fontScaleMultiplier = 1f;
			float num = defaultFontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
			float num2 = m_fontScale;
			m_currentFontSize = defaultFontSize;
			m_sizeStack.SetDefault(m_currentFontSize);
			float num3 = 0f;
			int num4 = 0;
			m_style = m_fontStyle;
			m_lineJustification = m_textAlignment;
			m_lineJustificationStack.SetDefault(m_lineJustification);
			float num5 = 1f;
			m_baselineOffset = 0f;
			m_lineOffset = 0f;
			m_lineHeight = -32767f;
			float num6 = m_currentFontAsset.fontInfo.LineHeight - (m_currentFontAsset.fontInfo.Ascender - m_currentFontAsset.fontInfo.Descender);
			m_cSpacing = 0f;
			m_monoSpacing = 0f;
			float num7 = 0f;
			m_xAdvance = 0f;
			float a = 0f;
			tag_LineIndent = 0f;
			tag_Indent = 0f;
			m_indentStack.SetDefault(0f);
			tag_NoParsing = false;
			m_characterCount = 0;
			m_firstCharacterOfLine = 0;
			m_maxLineAscender = k_LargeNegativeFloat;
			m_maxLineDescender = k_LargePositiveFloat;
			m_lineNumber = 0;
			float x = marginSize.x;
			m_marginLeft = 0f;
			m_marginRight = 0f;
			m_width = -1f;
			float num8 = 0f;
			float num9 = 0f;
			float num10 = 0f;
			m_isCalculatingPreferredValues = true;
			m_maxAscender = 0f;
			m_maxDescender = 0f;
			bool flag = true;
			bool flag2 = false;
			WordWrapState state = default(WordWrapState);
			SaveWordWrappingState(ref state, 0, 0);
			WordWrapState state2 = default(WordWrapState);
			int num11 = 0;
			m_recursiveCount++;
			int endIndex = 0;
			for (int i = 0; m_char_buffer[i] != 0; i++)
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
				float num12 = 1f;
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
						num12 = 0.8f;
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
					m_currentFontAsset = m_fontAsset;
					float num13 = m_currentFontSize / m_fontAsset.fontInfo.PointSize * m_fontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
					num2 = m_fontAsset.fontInfo.Ascender / tMP_Sprite.height * tMP_Sprite.scale * num13;
					m_cached_TextElement = tMP_Sprite;
					m_internalCharacterInfo[m_characterCount].elementType = TMP_TextElementType.Sprite;
					m_internalCharacterInfo[m_characterCount].scale = num13;
					m_currentMaterialIndex = currentMaterialIndex;
				}
				else if (m_textElementType == TMP_TextElementType.Character)
				{
					m_cached_TextElement = m_textInfo.characterInfo[m_characterCount].textElement;
					if (m_cached_TextElement == null)
					{
						continue;
					}
					m_currentMaterialIndex = m_textInfo.characterInfo[m_characterCount].materialReferenceIndex;
					m_fontScale = m_currentFontSize * num12 / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
					num2 = m_fontScale * m_fontScaleMultiplier * m_cached_TextElement.scale;
					m_internalCharacterInfo[m_characterCount].elementType = TMP_TextElementType.Character;
				}
				float num14 = num2;
				if (num4 == 173)
				{
					num2 = 0f;
				}
				m_internalCharacterInfo[m_characterCount].character = (char)num4;
				if (m_enableKerning && m_characterCount >= 1)
				{
					int character = m_internalCharacterInfo[m_characterCount - 1].character;
					KerningPairKey kerningPairKey = new KerningPairKey(character, num4);
					KerningPair value;
					m_currentFontAsset.kerningDictionary.TryGetValue(kerningPairKey.key, out value);
					if (value != null)
					{
						m_xAdvance += value.XadvanceOffset * num2;
					}
				}
				float num15 = 0f;
				if (m_monoSpacing != 0f)
				{
					num15 = m_monoSpacing / 2f - (m_cached_TextElement.width / 2f + m_cached_TextElement.xOffset) * num2;
					m_xAdvance += num15;
				}
				num5 = ((m_textElementType != 0 || isUsingAlternateTypeface || ((m_style & FontStyles.Bold) != FontStyles.Bold && (m_fontStyle & FontStyles.Bold) != FontStyles.Bold)) ? 1f : (1f + m_currentFontAsset.boldSpacing * 0.01f));
				m_internalCharacterInfo[m_characterCount].baseLine = 0f - m_lineOffset + m_baselineOffset;
				float num16 = m_currentFontAsset.fontInfo.Ascender * ((m_textElementType == TMP_TextElementType.Character) ? num2 : m_internalCharacterInfo[m_characterCount].scale) + m_baselineOffset;
				m_internalCharacterInfo[m_characterCount].ascender = num16 - m_lineOffset;
				m_maxLineAscender = ((num16 > m_maxLineAscender) ? num16 : m_maxLineAscender);
				float num17 = m_currentFontAsset.fontInfo.Descender * ((m_textElementType == TMP_TextElementType.Character) ? num2 : m_internalCharacterInfo[m_characterCount].scale) + m_baselineOffset;
				float num18 = m_internalCharacterInfo[m_characterCount].descender = num17 - m_lineOffset;
				m_maxLineDescender = ((num17 < m_maxLineDescender) ? num17 : m_maxLineDescender);
				if ((m_style & FontStyles.Subscript) == FontStyles.Subscript || (m_style & FontStyles.Superscript) == FontStyles.Superscript)
				{
					float num19 = (num16 - m_baselineOffset) / m_currentFontAsset.fontInfo.SubSize;
					num16 = m_maxLineAscender;
					m_maxLineAscender = ((num19 > m_maxLineAscender) ? num19 : m_maxLineAscender);
					float num20 = (num17 - m_baselineOffset) / m_currentFontAsset.fontInfo.SubSize;
					num17 = m_maxLineDescender;
					m_maxLineDescender = ((num20 < m_maxLineDescender) ? num20 : m_maxLineDescender);
				}
				if (m_lineNumber == 0)
				{
					m_maxAscender = ((m_maxAscender > num16) ? m_maxAscender : num16);
				}
				if (num4 == 9 || (!char.IsWhiteSpace((char)num4) && num4 != 8203) || m_textElementType == TMP_TextElementType.Sprite)
				{
					float num21 = (m_width != -1f) ? Mathf.Min(x + 0.0001f - m_marginLeft - m_marginRight, m_width) : (x + 0.0001f - m_marginLeft - m_marginRight);
					bool flag3 = (m_lineJustification & (TextAlignmentOptions)16) == (TextAlignmentOptions)16 || (m_lineJustification & (TextAlignmentOptions)8) == (TextAlignmentOptions)8;
					num10 = m_xAdvance + m_cached_TextElement.xAdvance * ((num4 != 173) ? num2 : num14);
					if (num10 > num21 * (flag3 ? 1.05f : 1f))
					{
						if (enableWordWrapping && m_characterCount != m_firstCharacterOfLine)
						{
							if (num11 == state2.previous_WordBreak || flag)
							{
								if (!m_isCharacterWrappingEnabled)
								{
									m_isCharacterWrappingEnabled = true;
								}
								else
								{
									flag2 = true;
								}
							}
							i = RestoreWordWrappingState(ref state2);
							num11 = i;
							if (m_char_buffer[i] == 173)
							{
								m_isTextTruncated = true;
								m_char_buffer[i] = 45;
								CalculatePreferredValues(defaultFontSize, marginSize, true);
								return Vector2.zero;
							}
							if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == -32767f)
							{
								float num22 = m_maxLineAscender - m_startOfLineAscender;
								m_lineOffset += num22;
								state2.lineOffset = m_lineOffset;
								state2.previousLineAscender = m_maxLineAscender;
							}
							float num23 = m_maxLineAscender - m_lineOffset;
							float num24 = m_maxLineDescender - m_lineOffset;
							m_maxDescender = ((m_maxDescender < num24) ? m_maxDescender : num24);
							m_firstCharacterOfLine = m_characterCount;
							num8 += m_xAdvance;
							num9 = ((!m_enableWordWrapping) ? Mathf.Max(num9, num23 - num24) : (m_maxAscender - m_maxDescender));
							SaveWordWrappingState(ref state, i, m_characterCount - 1);
							m_lineNumber++;
							if (m_lineHeight == -32767f)
							{
								float num25 = m_internalCharacterInfo[m_characterCount].ascender - m_internalCharacterInfo[m_characterCount].baseLine;
								num7 = 0f - m_maxLineDescender + num25 + (num6 + m_lineSpacing + m_lineSpacingDelta) * num;
								m_lineOffset += num7;
								m_startOfLineAscender = num25;
							}
							else
							{
								m_lineOffset += m_lineHeight + m_lineSpacing * num;
							}
							m_maxLineAscender = k_LargeNegativeFloat;
							m_maxLineDescender = k_LargePositiveFloat;
							m_xAdvance = 0f + tag_Indent;
							continue;
						}
						if (!ignoreTextAutoSizing && m_currentFontSize > m_fontSizeMin)
						{
							if (m_charWidthAdjDelta < m_charWidthMaxAdj / 100f)
							{
							}
							m_maxFontSize = m_currentFontSize;
							m_currentFontSize -= Mathf.Max((m_currentFontSize - m_minFontSize) / 2f, 0.05f);
							m_currentFontSize = (float)(int)(Mathf.Max(m_currentFontSize, m_fontSizeMin) * 20f + 0.5f) / 20f;
							if (m_recursiveCount > 20)
							{
								return new Vector2(num8, num9);
							}
							return CalculatePreferredValues(m_currentFontSize, marginSize, false);
						}
					}
				}
				if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == -32767f && !m_isNewPage)
				{
					float num26 = m_maxLineAscender - m_startOfLineAscender;
					num18 -= num26;
					m_lineOffset += num26;
					m_startOfLineAscender += num26;
					state2.lineOffset = m_lineOffset;
					state2.previousLineAscender = m_startOfLineAscender;
				}
				if (num4 == 9)
				{
					float num27 = m_currentFontAsset.fontInfo.TabWidth * num2;
					float num28 = Mathf.Ceil(m_xAdvance / num27) * num27;
					m_xAdvance = ((num28 > m_xAdvance) ? num28 : (m_xAdvance + num27));
				}
				else if (m_monoSpacing != 0f)
				{
					m_xAdvance += m_monoSpacing - num15 + (m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * num2 + m_cSpacing;
					if (char.IsWhiteSpace((char)num4) || num4 == 8203)
					{
						m_xAdvance += m_wordSpacing * num2;
					}
				}
				else
				{
					m_xAdvance += (m_cached_TextElement.xAdvance * num5 + m_characterSpacing + m_currentFontAsset.normalSpacingOffset) * num2 + m_cSpacing;
					if (char.IsWhiteSpace((char)num4) || num4 == 8203)
					{
						m_xAdvance += m_wordSpacing * num2;
					}
				}
				if (num4 == 13)
				{
					a = Mathf.Max(a, num8 + m_xAdvance);
					num8 = 0f;
					m_xAdvance = 0f + tag_Indent;
				}
				if (num4 == 10 || m_characterCount == totalCharacterCount - 1)
				{
					if (m_lineNumber > 0 && !TMP_Math.Approximately(m_maxLineAscender, m_startOfLineAscender) && m_lineHeight == -32767f)
					{
						float num29 = m_maxLineAscender - m_startOfLineAscender;
						num18 -= num29;
						m_lineOffset += num29;
					}
					float num30 = m_maxLineDescender - m_lineOffset;
					m_maxDescender = ((m_maxDescender < num30) ? m_maxDescender : num30);
					m_firstCharacterOfLine = m_characterCount + 1;
					if (num4 == 10 && m_characterCount != totalCharacterCount - 1)
					{
						a = Mathf.Max(a, num8 + num10);
						num8 = 0f;
					}
					else
					{
						num8 = Mathf.Max(a, num8 + num10);
					}
					num9 = m_maxAscender - m_maxDescender;
					if (num4 == 10)
					{
						SaveWordWrappingState(ref state, i, m_characterCount);
						SaveWordWrappingState(ref state2, i, m_characterCount);
						m_lineNumber++;
						if (m_lineHeight == -32767f)
						{
							num7 = 0f - m_maxLineDescender + num16 + (num6 + m_lineSpacing + m_paragraphSpacing + m_lineSpacingDelta) * num;
							m_lineOffset += num7;
						}
						else
						{
							m_lineOffset += m_lineHeight + (m_lineSpacing + m_paragraphSpacing) * num;
						}
						m_maxLineAscender = k_LargeNegativeFloat;
						m_maxLineDescender = k_LargePositiveFloat;
						m_startOfLineAscender = num16;
						m_xAdvance = 0f + tag_LineIndent + tag_Indent;
					}
				}
				if (m_enableWordWrapping || m_overflowMode == TextOverflowModes.Truncate || m_overflowMode == TextOverflowModes.Ellipsis)
				{
					if ((char.IsWhiteSpace((char)num4) || num4 == 8203 || num4 == 45 || num4 == 173) && !m_isNonBreakingSpace && num4 != 160 && num4 != 8209 && num4 != 8239 && num4 != 8288)
					{
						SaveWordWrappingState(ref state2, i, m_characterCount);
						m_isCharacterWrappingEnabled = false;
						flag = false;
					}
					else if (((num4 > 4352 && num4 < 4607) || (num4 > 11904 && num4 < 40959) || (num4 > 43360 && num4 < 43391) || (num4 > 44032 && num4 < 55295) || (num4 > 63744 && num4 < 64255) || (num4 > 65072 && num4 < 65103) || (num4 > 65280 && num4 < 65519)) && !m_isNonBreakingSpace)
					{
						if (flag || flag2 || (!TMP_Settings.linebreakingRules.leadingCharacters.ContainsKey(num4) && m_characterCount < totalCharacterCount - 1 && !TMP_Settings.linebreakingRules.followingCharacters.ContainsKey(m_internalCharacterInfo[m_characterCount + 1].character)))
						{
							SaveWordWrappingState(ref state2, i, m_characterCount);
							m_isCharacterWrappingEnabled = false;
							flag = false;
						}
					}
					else if (flag || m_isCharacterWrappingEnabled || flag2)
					{
						SaveWordWrappingState(ref state2, i, m_characterCount);
					}
				}
				m_characterCount++;
			}
			num3 = m_maxFontSize - m_minFontSize;
			if (!m_isCharacterWrappingEnabled && !ignoreTextAutoSizing && num3 > 0.051f && m_currentFontSize < m_fontSizeMax)
			{
				m_minFontSize = m_currentFontSize;
				m_currentFontSize += Mathf.Max((m_maxFontSize - m_currentFontSize) / 2f, 0.05f);
				m_currentFontSize = (float)(int)(Mathf.Min(m_currentFontSize, m_fontSizeMax) * 20f + 0.5f) / 20f;
				if (m_recursiveCount > 20)
				{
					return new Vector2(num8, num9);
				}
				return CalculatePreferredValues(m_currentFontSize, marginSize, false);
			}
			m_isCharacterWrappingEnabled = false;
			m_isCalculatingPreferredValues = false;
			num8 += ((m_margin.x > 0f) ? m_margin.x : 0f);
			num8 += ((m_margin.z > 0f) ? m_margin.z : 0f);
			num9 += ((m_margin.y > 0f) ? m_margin.y : 0f);
			num9 += ((m_margin.w > 0f) ? m_margin.w : 0f);
			num8 = (float)(int)(num8 * 100f + 1f) / 100f;
			num9 = (float)(int)(num9 * 100f + 1f) / 100f;
			return new Vector2(num8, num9);
		}

		protected virtual Bounds GetCompoundBounds()
		{
			return default(Bounds);
		}

		protected Bounds GetTextBounds()
		{
			if (m_textInfo == null || m_textInfo.characterCount > m_textInfo.characterInfo.Length)
			{
				return default(Bounds);
			}
			Extents extents = new Extents(k_LargePositiveVector2, k_LargeNegativeVector2);
			for (int i = 0; i < m_textInfo.characterCount && i < m_textInfo.characterInfo.Length; i++)
			{
				if (m_textInfo.characterInfo[i].isVisible)
				{
					extents.min.x = Mathf.Min(extents.min.x, m_textInfo.characterInfo[i].bottomLeft.x);
					extents.min.y = Mathf.Min(extents.min.y, m_textInfo.characterInfo[i].descender);
					extents.max.x = Mathf.Max(extents.max.x, m_textInfo.characterInfo[i].xAdvance);
					extents.max.y = Mathf.Max(extents.max.y, m_textInfo.characterInfo[i].ascender);
				}
			}
			Vector2 v = default(Vector2);
			v.x = extents.max.x - extents.min.x;
			v.y = extents.max.y - extents.min.y;
			Vector3 center = (extents.min + extents.max) / 2f;
			return new Bounds(center, v);
		}

		protected Bounds GetTextBounds(bool onlyVisibleCharacters)
		{
			if (m_textInfo == null)
			{
				return default(Bounds);
			}
			Extents extents = new Extents(k_LargePositiveVector2, k_LargeNegativeVector2);
			for (int i = 0; i < m_textInfo.characterCount && i < m_maxVisibleCharacters; i++)
			{
				if (m_textInfo.characterInfo[i].isVisible)
				{
					if (m_textInfo.characterInfo[i].lineNumber > m_maxVisibleLines)
					{
						break;
					}
					extents.min.x = Mathf.Min(extents.min.x, m_textInfo.characterInfo[i].origin);
					extents.min.y = Mathf.Min(extents.min.y, m_textInfo.characterInfo[i].descender);
					extents.max.x = Mathf.Max(extents.max.x, m_textInfo.characterInfo[i].xAdvance);
					extents.max.y = Mathf.Max(extents.max.y, m_textInfo.characterInfo[i].ascender);
				}
			}
			Vector2 v = default(Vector2);
			v.x = extents.max.x - extents.min.x;
			v.y = extents.max.y - extents.min.y;
			Vector2 v2 = (extents.min + extents.max) / 2f;
			return new Bounds(v2, v);
		}

		protected virtual void AdjustLineOffset(int startIndex, int endIndex, float offset)
		{
		}

		protected void ResizeLineExtents(int size)
		{
			size = ((size > 1024) ? (size + 256) : Mathf.NextPowerOfTwo(size + 1));
			TMP_LineInfo[] array = new TMP_LineInfo[size];
			for (int i = 0; i < size; i++)
			{
				if (i < m_textInfo.lineInfo.Length)
				{
					array[i] = m_textInfo.lineInfo[i];
					continue;
				}
				array[i].lineExtents.min = k_LargePositiveVector2;
				array[i].lineExtents.max = k_LargeNegativeVector2;
				array[i].ascender = k_LargeNegativeFloat;
				array[i].descender = k_LargePositiveFloat;
			}
			m_textInfo.lineInfo = array;
		}

		public virtual TMP_TextInfo GetTextInfo(string text)
		{
			return null;
		}

		protected virtual void ComputeMarginSize()
		{
		}

		protected void SaveWordWrappingState(ref WordWrapState state, int index, int count)
		{
			state.currentFontAsset = m_currentFontAsset;
			state.currentSpriteAsset = m_currentSpriteAsset;
			state.currentMaterial = m_currentMaterial;
			state.currentMaterialIndex = m_currentMaterialIndex;
			state.previous_WordBreak = index;
			state.total_CharacterCount = count;
			state.visible_CharacterCount = m_lineVisibleCharacterCount;
			state.visible_LinkCount = m_textInfo.linkCount;
			state.firstCharacterIndex = m_firstCharacterOfLine;
			state.firstVisibleCharacterIndex = m_firstVisibleCharacterOfLine;
			state.lastVisibleCharIndex = m_lastVisibleCharacterOfLine;
			state.fontStyle = m_style;
			state.fontScale = m_fontScale;
			state.fontScaleMultiplier = m_fontScaleMultiplier;
			state.currentFontSize = m_currentFontSize;
			state.xAdvance = m_xAdvance;
			state.maxCapHeight = m_maxCapHeight;
			state.maxAscender = m_maxAscender;
			state.maxDescender = m_maxDescender;
			state.maxLineAscender = m_maxLineAscender;
			state.maxLineDescender = m_maxLineDescender;
			state.previousLineAscender = m_startOfLineAscender;
			state.preferredWidth = m_preferredWidth;
			state.preferredHeight = m_preferredHeight;
			state.meshExtents = m_meshExtents;
			state.lineNumber = m_lineNumber;
			state.lineOffset = m_lineOffset;
			state.baselineOffset = m_baselineOffset;
			state.vertexColor = m_htmlColor;
			state.underlineColor = m_underlineColor;
			state.strikethroughColor = m_strikethroughColor;
			state.highlightColor = m_highlightColor;
			state.tagNoParsing = tag_NoParsing;
			state.basicStyleStack = m_fontStyleStack;
			state.colorStack = m_colorStack;
			state.underlineColorStack = m_underlineColorStack;
			state.strikethroughColorStack = m_strikethroughColorStack;
			state.highlightColorStack = m_highlightColorStack;
			state.sizeStack = m_sizeStack;
			state.indentStack = m_indentStack;
			state.fontWeightStack = m_fontWeightStack;
			state.styleStack = m_styleStack;
			state.actionStack = m_actionStack;
			state.materialReferenceStack = m_materialReferenceStack;
			state.lineJustificationStack = m_lineJustificationStack;
			state.spriteAnimationID = m_spriteAnimationID;
			if (m_lineNumber < m_textInfo.lineInfo.Length)
			{
				state.lineInfo = m_textInfo.lineInfo[m_lineNumber];
			}
		}

		protected int RestoreWordWrappingState(ref WordWrapState state)
		{
			int previous_WordBreak = state.previous_WordBreak;
			m_currentFontAsset = state.currentFontAsset;
			m_currentSpriteAsset = state.currentSpriteAsset;
			m_currentMaterial = state.currentMaterial;
			m_currentMaterialIndex = state.currentMaterialIndex;
			m_characterCount = state.total_CharacterCount + 1;
			m_lineVisibleCharacterCount = state.visible_CharacterCount;
			m_textInfo.linkCount = state.visible_LinkCount;
			m_firstCharacterOfLine = state.firstCharacterIndex;
			m_firstVisibleCharacterOfLine = state.firstVisibleCharacterIndex;
			m_lastVisibleCharacterOfLine = state.lastVisibleCharIndex;
			m_style = state.fontStyle;
			m_fontScale = state.fontScale;
			m_fontScaleMultiplier = state.fontScaleMultiplier;
			m_currentFontSize = state.currentFontSize;
			m_xAdvance = state.xAdvance;
			m_maxCapHeight = state.maxCapHeight;
			m_maxAscender = state.maxAscender;
			m_maxDescender = state.maxDescender;
			m_maxLineAscender = state.maxLineAscender;
			m_maxLineDescender = state.maxLineDescender;
			m_startOfLineAscender = state.previousLineAscender;
			m_preferredWidth = state.preferredWidth;
			m_preferredHeight = state.preferredHeight;
			m_meshExtents = state.meshExtents;
			m_lineNumber = state.lineNumber;
			m_lineOffset = state.lineOffset;
			m_baselineOffset = state.baselineOffset;
			m_htmlColor = state.vertexColor;
			m_underlineColor = state.underlineColor;
			m_strikethroughColor = state.strikethroughColor;
			m_highlightColor = state.highlightColor;
			tag_NoParsing = state.tagNoParsing;
			m_fontStyleStack = state.basicStyleStack;
			m_colorStack = state.colorStack;
			m_underlineColorStack = state.underlineColorStack;
			m_strikethroughColorStack = state.strikethroughColorStack;
			m_highlightColorStack = state.highlightColorStack;
			m_sizeStack = state.sizeStack;
			m_indentStack = state.indentStack;
			m_fontWeightStack = state.fontWeightStack;
			m_styleStack = state.styleStack;
			m_actionStack = state.actionStack;
			m_materialReferenceStack = state.materialReferenceStack;
			m_lineJustificationStack = state.lineJustificationStack;
			m_spriteAnimationID = state.spriteAnimationID;
			if (m_lineNumber < m_textInfo.lineInfo.Length)
			{
				m_textInfo.lineInfo[m_lineNumber] = state.lineInfo;
			}
			return previous_WordBreak;
		}

		protected virtual void SaveGlyphVertexInfo(float padding, float style_padding, Color32 vertexColor)
		{
			m_textInfo.characterInfo[m_characterCount].vertex_BL.position = m_textInfo.characterInfo[m_characterCount].bottomLeft;
			m_textInfo.characterInfo[m_characterCount].vertex_TL.position = m_textInfo.characterInfo[m_characterCount].topLeft;
			m_textInfo.characterInfo[m_characterCount].vertex_TR.position = m_textInfo.characterInfo[m_characterCount].topRight;
			m_textInfo.characterInfo[m_characterCount].vertex_BR.position = m_textInfo.characterInfo[m_characterCount].bottomRight;
			vertexColor.a = ((m_fontColor32.a < vertexColor.a) ? m_fontColor32.a : vertexColor.a);
			if (!m_enableVertexGradient)
			{
				m_textInfo.characterInfo[m_characterCount].vertex_BL.color = vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_TL.color = vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_TR.color = vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_BR.color = vertexColor;
			}
			else if (!m_overrideHtmlColors && m_colorStack.index > 1)
			{
				m_textInfo.characterInfo[m_characterCount].vertex_BL.color = vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_TL.color = vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_TR.color = vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_BR.color = vertexColor;
			}
			else if (m_fontColorGradientPreset != null)
			{
				m_textInfo.characterInfo[m_characterCount].vertex_BL.color = m_fontColorGradientPreset.bottomLeft * vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_TL.color = m_fontColorGradientPreset.topLeft * vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_TR.color = m_fontColorGradientPreset.topRight * vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_BR.color = m_fontColorGradientPreset.bottomRight * vertexColor;
			}
			else
			{
				m_textInfo.characterInfo[m_characterCount].vertex_BL.color = m_fontColorGradient.bottomLeft * vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_TL.color = m_fontColorGradient.topLeft * vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_TR.color = m_fontColorGradient.topRight * vertexColor;
				m_textInfo.characterInfo[m_characterCount].vertex_BR.color = m_fontColorGradient.bottomRight * vertexColor;
			}
			if (!m_isSDFShader)
			{
				style_padding = 0f;
			}
			FaceInfo fontInfo = m_currentFontAsset.fontInfo;
			Vector2 uv = default(Vector2);
			uv.x = (m_cached_TextElement.x - padding - style_padding) / fontInfo.AtlasWidth;
			uv.y = 1f - (m_cached_TextElement.y + padding + style_padding + m_cached_TextElement.height) / fontInfo.AtlasHeight;
			Vector2 uv2 = default(Vector2);
			uv2.x = uv.x;
			uv2.y = 1f - (m_cached_TextElement.y - padding - style_padding) / fontInfo.AtlasHeight;
			Vector2 uv3 = default(Vector2);
			uv3.x = (m_cached_TextElement.x + padding + style_padding + m_cached_TextElement.width) / fontInfo.AtlasWidth;
			uv3.y = uv2.y;
			Vector2 uv4 = default(Vector2);
			uv4.x = uv3.x;
			uv4.y = uv.y;
			m_textInfo.characterInfo[m_characterCount].vertex_BL.uv = uv;
			m_textInfo.characterInfo[m_characterCount].vertex_TL.uv = uv2;
			m_textInfo.characterInfo[m_characterCount].vertex_TR.uv = uv3;
			m_textInfo.characterInfo[m_characterCount].vertex_BR.uv = uv4;
		}

		protected virtual void SaveSpriteVertexInfo(Color32 vertexColor)
		{
			m_textInfo.characterInfo[m_characterCount].vertex_BL.position = m_textInfo.characterInfo[m_characterCount].bottomLeft;
			m_textInfo.characterInfo[m_characterCount].vertex_TL.position = m_textInfo.characterInfo[m_characterCount].topLeft;
			m_textInfo.characterInfo[m_characterCount].vertex_TR.position = m_textInfo.characterInfo[m_characterCount].topRight;
			m_textInfo.characterInfo[m_characterCount].vertex_BR.position = m_textInfo.characterInfo[m_characterCount].bottomRight;
			if (m_tintAllSprites)
			{
				m_tintSprite = true;
			}
			Color32 color = m_tintSprite ? m_spriteColor.Multiply(vertexColor) : m_spriteColor;
			color.a = ((color.a < m_fontColor32.a) ? (color.a = ((color.a < vertexColor.a) ? color.a : vertexColor.a)) : m_fontColor32.a);
			if (!m_enableVertexGradient)
			{
				m_textInfo.characterInfo[m_characterCount].vertex_BL.color = color;
				m_textInfo.characterInfo[m_characterCount].vertex_TL.color = color;
				m_textInfo.characterInfo[m_characterCount].vertex_TR.color = color;
				m_textInfo.characterInfo[m_characterCount].vertex_BR.color = color;
			}
			else if (!m_overrideHtmlColors && m_colorStack.index > 1)
			{
				m_textInfo.characterInfo[m_characterCount].vertex_BL.color = color;
				m_textInfo.characterInfo[m_characterCount].vertex_TL.color = color;
				m_textInfo.characterInfo[m_characterCount].vertex_TR.color = color;
				m_textInfo.characterInfo[m_characterCount].vertex_BR.color = color;
			}
			else if (m_fontColorGradientPreset != null)
			{
				m_textInfo.characterInfo[m_characterCount].vertex_BL.color = (m_tintSprite ? color.Multiply(m_fontColorGradientPreset.bottomLeft) : color);
				m_textInfo.characterInfo[m_characterCount].vertex_TL.color = (m_tintSprite ? color.Multiply(m_fontColorGradientPreset.topLeft) : color);
				m_textInfo.characterInfo[m_characterCount].vertex_TR.color = (m_tintSprite ? color.Multiply(m_fontColorGradientPreset.topRight) : color);
				m_textInfo.characterInfo[m_characterCount].vertex_BR.color = (m_tintSprite ? color.Multiply(m_fontColorGradientPreset.bottomRight) : color);
			}
			else
			{
				m_textInfo.characterInfo[m_characterCount].vertex_BL.color = (m_tintSprite ? color.Multiply(m_fontColorGradient.bottomLeft) : color);
				m_textInfo.characterInfo[m_characterCount].vertex_TL.color = (m_tintSprite ? color.Multiply(m_fontColorGradient.topLeft) : color);
				m_textInfo.characterInfo[m_characterCount].vertex_TR.color = (m_tintSprite ? color.Multiply(m_fontColorGradient.topRight) : color);
				m_textInfo.characterInfo[m_characterCount].vertex_BR.color = (m_tintSprite ? color.Multiply(m_fontColorGradient.bottomRight) : color);
			}
			Vector2 uv = new Vector2(m_cached_TextElement.x / (float)m_currentSpriteAsset.spriteSheet.width, m_cached_TextElement.y / (float)m_currentSpriteAsset.spriteSheet.height);
			Vector2 uv2 = new Vector2(uv.x, (m_cached_TextElement.y + m_cached_TextElement.height) / (float)m_currentSpriteAsset.spriteSheet.height);
			Vector2 uv3 = new Vector2((m_cached_TextElement.x + m_cached_TextElement.width) / (float)m_currentSpriteAsset.spriteSheet.width, uv2.y);
			Vector2 uv4 = new Vector2(uv3.x, uv.y);
			m_textInfo.characterInfo[m_characterCount].vertex_BL.uv = uv;
			m_textInfo.characterInfo[m_characterCount].vertex_TL.uv = uv2;
			m_textInfo.characterInfo[m_characterCount].vertex_TR.uv = uv3;
			m_textInfo.characterInfo[m_characterCount].vertex_BR.uv = uv4;
		}

		protected virtual void FillCharacterVertexBuffers(int i, int index_X4)
		{
			int materialReferenceIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
			index_X4 = m_textInfo.meshInfo[materialReferenceIndex].vertexCount;
			TMP_CharacterInfo[] characterInfo = m_textInfo.characterInfo;
			m_textInfo.characterInfo[i].vertexIndex = index_X4;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[index_X4] = characterInfo[i].vertex_BL.position;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[1 + index_X4] = characterInfo[i].vertex_TL.position;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[2 + index_X4] = characterInfo[i].vertex_TR.position;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[3 + index_X4] = characterInfo[i].vertex_BR.position;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[index_X4] = characterInfo[i].vertex_BL.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[1 + index_X4] = characterInfo[i].vertex_TL.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[2 + index_X4] = characterInfo[i].vertex_TR.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[3 + index_X4] = characterInfo[i].vertex_BR.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[index_X4] = characterInfo[i].vertex_BL.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[1 + index_X4] = characterInfo[i].vertex_TL.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[2 + index_X4] = characterInfo[i].vertex_TR.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[3 + index_X4] = characterInfo[i].vertex_BR.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[index_X4] = characterInfo[i].vertex_BL.color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[1 + index_X4] = characterInfo[i].vertex_TL.color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[2 + index_X4] = characterInfo[i].vertex_TR.color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[3 + index_X4] = characterInfo[i].vertex_BR.color;
			m_textInfo.meshInfo[materialReferenceIndex].vertexCount = index_X4 + 4;
		}

		protected virtual void FillCharacterVertexBuffers(int i, int index_X4, bool isVolumetric)
		{
			int materialReferenceIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
			index_X4 = m_textInfo.meshInfo[materialReferenceIndex].vertexCount;
			TMP_CharacterInfo[] characterInfo = m_textInfo.characterInfo;
			m_textInfo.characterInfo[i].vertexIndex = index_X4;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[index_X4] = characterInfo[i].vertex_BL.position;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[1 + index_X4] = characterInfo[i].vertex_TL.position;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[2 + index_X4] = characterInfo[i].vertex_TR.position;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[3 + index_X4] = characterInfo[i].vertex_BR.position;
			if (isVolumetric)
			{
				Vector3 b = new Vector3(0f, 0f, m_fontSize * m_fontScale);
				m_textInfo.meshInfo[materialReferenceIndex].vertices[4 + index_X4] = characterInfo[i].vertex_BL.position + b;
				m_textInfo.meshInfo[materialReferenceIndex].vertices[5 + index_X4] = characterInfo[i].vertex_TL.position + b;
				m_textInfo.meshInfo[materialReferenceIndex].vertices[6 + index_X4] = characterInfo[i].vertex_TR.position + b;
				m_textInfo.meshInfo[materialReferenceIndex].vertices[7 + index_X4] = characterInfo[i].vertex_BR.position + b;
			}
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[index_X4] = characterInfo[i].vertex_BL.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[1 + index_X4] = characterInfo[i].vertex_TL.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[2 + index_X4] = characterInfo[i].vertex_TR.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[3 + index_X4] = characterInfo[i].vertex_BR.uv;
			if (isVolumetric)
			{
				m_textInfo.meshInfo[materialReferenceIndex].uvs0[4 + index_X4] = characterInfo[i].vertex_BL.uv;
				m_textInfo.meshInfo[materialReferenceIndex].uvs0[5 + index_X4] = characterInfo[i].vertex_TL.uv;
				m_textInfo.meshInfo[materialReferenceIndex].uvs0[6 + index_X4] = characterInfo[i].vertex_TR.uv;
				m_textInfo.meshInfo[materialReferenceIndex].uvs0[7 + index_X4] = characterInfo[i].vertex_BR.uv;
			}
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[index_X4] = characterInfo[i].vertex_BL.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[1 + index_X4] = characterInfo[i].vertex_TL.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[2 + index_X4] = characterInfo[i].vertex_TR.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[3 + index_X4] = characterInfo[i].vertex_BR.uv2;
			if (isVolumetric)
			{
				m_textInfo.meshInfo[materialReferenceIndex].uvs2[4 + index_X4] = characterInfo[i].vertex_BL.uv2;
				m_textInfo.meshInfo[materialReferenceIndex].uvs2[5 + index_X4] = characterInfo[i].vertex_TL.uv2;
				m_textInfo.meshInfo[materialReferenceIndex].uvs2[6 + index_X4] = characterInfo[i].vertex_TR.uv2;
				m_textInfo.meshInfo[materialReferenceIndex].uvs2[7 + index_X4] = characterInfo[i].vertex_BR.uv2;
			}
			m_textInfo.meshInfo[materialReferenceIndex].colors32[index_X4] = characterInfo[i].vertex_BL.color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[1 + index_X4] = characterInfo[i].vertex_TL.color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[2 + index_X4] = characterInfo[i].vertex_TR.color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[3 + index_X4] = characterInfo[i].vertex_BR.color;
			if (isVolumetric)
			{
				Color32 color = new Color32(byte.MaxValue, byte.MaxValue, 128, byte.MaxValue);
				m_textInfo.meshInfo[materialReferenceIndex].colors32[4 + index_X4] = color;
				m_textInfo.meshInfo[materialReferenceIndex].colors32[5 + index_X4] = color;
				m_textInfo.meshInfo[materialReferenceIndex].colors32[6 + index_X4] = color;
				m_textInfo.meshInfo[materialReferenceIndex].colors32[7 + index_X4] = color;
			}
			m_textInfo.meshInfo[materialReferenceIndex].vertexCount = index_X4 + ((!isVolumetric) ? 4 : 8);
		}

		protected virtual void FillSpriteVertexBuffers(int i, int index_X4)
		{
			int materialReferenceIndex = m_textInfo.characterInfo[i].materialReferenceIndex;
			index_X4 = m_textInfo.meshInfo[materialReferenceIndex].vertexCount;
			TMP_CharacterInfo[] characterInfo = m_textInfo.characterInfo;
			m_textInfo.characterInfo[i].vertexIndex = index_X4;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[index_X4] = characterInfo[i].vertex_BL.position;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[1 + index_X4] = characterInfo[i].vertex_TL.position;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[2 + index_X4] = characterInfo[i].vertex_TR.position;
			m_textInfo.meshInfo[materialReferenceIndex].vertices[3 + index_X4] = characterInfo[i].vertex_BR.position;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[index_X4] = characterInfo[i].vertex_BL.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[1 + index_X4] = characterInfo[i].vertex_TL.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[2 + index_X4] = characterInfo[i].vertex_TR.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs0[3 + index_X4] = characterInfo[i].vertex_BR.uv;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[index_X4] = characterInfo[i].vertex_BL.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[1 + index_X4] = characterInfo[i].vertex_TL.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[2 + index_X4] = characterInfo[i].vertex_TR.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].uvs2[3 + index_X4] = characterInfo[i].vertex_BR.uv2;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[index_X4] = characterInfo[i].vertex_BL.color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[1 + index_X4] = characterInfo[i].vertex_TL.color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[2 + index_X4] = characterInfo[i].vertex_TR.color;
			m_textInfo.meshInfo[materialReferenceIndex].colors32[3 + index_X4] = characterInfo[i].vertex_BR.color;
			m_textInfo.meshInfo[materialReferenceIndex].vertexCount = index_X4 + 4;
		}

		protected virtual void DrawUnderlineMesh(Vector3 start, Vector3 end, ref int index, float startScale, float endScale, float maxScale, float sdfScale, Color32 underlineColor)
		{
			if (m_cached_Underline_GlyphInfo == null)
			{
				if (!TMP_Settings.warningsDisabled)
				{
					Debug.LogWarning("Unable to add underline since the Font Asset doesn't contain the underline character.", (UnityEngine.Object)(object)this);
				}
				return;
			}
			int num = index + 12;
			if (num > m_textInfo.meshInfo[0].vertices.Length)
			{
				m_textInfo.meshInfo[0].ResizeMeshInfo(num / 4);
			}
			start.y = Mathf.Min(start.y, end.y);
			end.y = Mathf.Min(start.y, end.y);
			float num2 = m_cached_Underline_GlyphInfo.width / 2f * maxScale;
			if (end.x - start.x < m_cached_Underline_GlyphInfo.width * maxScale)
			{
				num2 = (end.x - start.x) / 2f;
			}
			float num3 = m_padding * startScale / maxScale;
			float num4 = m_padding * endScale / maxScale;
			float height = m_cached_Underline_GlyphInfo.height;
			Vector3[] vertices = m_textInfo.meshInfo[0].vertices;
			vertices[index] = start + new Vector3(0f, 0f - (height + m_padding) * maxScale, 0f);
			vertices[index + 1] = start + new Vector3(0f, m_padding * maxScale, 0f);
			vertices[index + 2] = vertices[index + 1] + new Vector3(num2, 0f, 0f);
			vertices[index + 3] = vertices[index] + new Vector3(num2, 0f, 0f);
			vertices[index + 4] = vertices[index + 3];
			vertices[index + 5] = vertices[index + 2];
			vertices[index + 6] = end + new Vector3(0f - num2, m_padding * maxScale, 0f);
			vertices[index + 7] = end + new Vector3(0f - num2, (0f - (height + m_padding)) * maxScale, 0f);
			vertices[index + 8] = vertices[index + 7];
			vertices[index + 9] = vertices[index + 6];
			vertices[index + 10] = end + new Vector3(0f, m_padding * maxScale, 0f);
			vertices[index + 11] = end + new Vector3(0f, (0f - (height + m_padding)) * maxScale, 0f);
			Vector2[] uvs = m_textInfo.meshInfo[0].uvs0;
			Vector2 vector = new Vector2((m_cached_Underline_GlyphInfo.x - num3) / m_fontAsset.fontInfo.AtlasWidth, 1f - (m_cached_Underline_GlyphInfo.y + m_padding + m_cached_Underline_GlyphInfo.height) / m_fontAsset.fontInfo.AtlasHeight);
			Vector2 vector2 = new Vector2(vector.x, 1f - (m_cached_Underline_GlyphInfo.y - m_padding) / m_fontAsset.fontInfo.AtlasHeight);
			Vector2 vector3 = new Vector2((m_cached_Underline_GlyphInfo.x - num3 + m_cached_Underline_GlyphInfo.width / 2f) / m_fontAsset.fontInfo.AtlasWidth, vector2.y);
			Vector2 vector4 = new Vector2(vector3.x, vector.y);
			Vector2 vector5 = new Vector2((m_cached_Underline_GlyphInfo.x + num4 + m_cached_Underline_GlyphInfo.width / 2f) / m_fontAsset.fontInfo.AtlasWidth, vector2.y);
			Vector2 vector6 = new Vector2(vector5.x, vector.y);
			Vector2 vector7 = new Vector2((m_cached_Underline_GlyphInfo.x + num4 + m_cached_Underline_GlyphInfo.width) / m_fontAsset.fontInfo.AtlasWidth, vector2.y);
			Vector2 vector8 = new Vector2(vector7.x, vector.y);
			uvs[index] = vector;
			uvs[1 + index] = vector2;
			uvs[2 + index] = vector3;
			uvs[3 + index] = vector4;
			uvs[4 + index] = new Vector2(vector3.x - vector3.x * 0.001f, vector.y);
			uvs[5 + index] = new Vector2(vector3.x - vector3.x * 0.001f, vector2.y);
			uvs[6 + index] = new Vector2(vector3.x + vector3.x * 0.001f, vector2.y);
			uvs[7 + index] = new Vector2(vector3.x + vector3.x * 0.001f, vector.y);
			uvs[8 + index] = vector6;
			uvs[9 + index] = vector5;
			uvs[10 + index] = vector7;
			uvs[11 + index] = vector8;
			float num5 = 0f;
			float x = (vertices[index + 2].x - start.x) / (end.x - start.x);
			float scale = Mathf.Abs(sdfScale);
			Vector2[] uvs2 = m_textInfo.meshInfo[0].uvs2;
			uvs2[index] = PackUV(0f, 0f, scale);
			uvs2[1 + index] = PackUV(0f, 1f, scale);
			uvs2[2 + index] = PackUV(x, 1f, scale);
			uvs2[3 + index] = PackUV(x, 0f, scale);
			num5 = (vertices[index + 4].x - start.x) / (end.x - start.x);
			x = (vertices[index + 6].x - start.x) / (end.x - start.x);
			uvs2[4 + index] = PackUV(num5, 0f, scale);
			uvs2[5 + index] = PackUV(num5, 1f, scale);
			uvs2[6 + index] = PackUV(x, 1f, scale);
			uvs2[7 + index] = PackUV(x, 0f, scale);
			num5 = (vertices[index + 8].x - start.x) / (end.x - start.x);
			x = (vertices[index + 6].x - start.x) / (end.x - start.x);
			uvs2[8 + index] = PackUV(num5, 0f, scale);
			uvs2[9 + index] = PackUV(num5, 1f, scale);
			uvs2[10 + index] = PackUV(1f, 1f, scale);
			uvs2[11 + index] = PackUV(1f, 0f, scale);
			Color32[] colors = m_textInfo.meshInfo[0].colors32;
			colors[index] = underlineColor;
			colors[1 + index] = underlineColor;
			colors[2 + index] = underlineColor;
			colors[3 + index] = underlineColor;
			colors[4 + index] = underlineColor;
			colors[5 + index] = underlineColor;
			colors[6 + index] = underlineColor;
			colors[7 + index] = underlineColor;
			colors[8 + index] = underlineColor;
			colors[9 + index] = underlineColor;
			colors[10 + index] = underlineColor;
			colors[11 + index] = underlineColor;
			index += 12;
		}

		protected virtual void DrawTextHighlight(Vector3 start, Vector3 end, ref int index, Color32 highlightColor)
		{
			if (m_cached_Underline_GlyphInfo == null)
			{
				if (!TMP_Settings.warningsDisabled)
				{
					Debug.LogWarning("Unable to add underline since the Font Asset doesn't contain the underline character.", (UnityEngine.Object)(object)this);
				}
				return;
			}
			int num = index + 4;
			if (num > m_textInfo.meshInfo[0].vertices.Length)
			{
				m_textInfo.meshInfo[0].ResizeMeshInfo(num / 4);
			}
			Vector3[] vertices = m_textInfo.meshInfo[0].vertices;
			vertices[index] = start;
			vertices[index + 1] = new Vector3(start.x, end.y, 0f);
			vertices[index + 2] = end;
			vertices[index + 3] = new Vector3(end.x, start.y, 0f);
			Vector2[] uvs = m_textInfo.meshInfo[0].uvs0;
			Vector2 vector = new Vector2((m_cached_Underline_GlyphInfo.x + m_cached_Underline_GlyphInfo.width / 2f) / m_fontAsset.fontInfo.AtlasWidth, 1f - (m_cached_Underline_GlyphInfo.y + m_cached_Underline_GlyphInfo.height / 2f) / m_fontAsset.fontInfo.AtlasHeight);
			uvs[index] = vector;
			uvs[1 + index] = vector;
			uvs[2 + index] = vector;
			uvs[3 + index] = vector;
			Vector2[] uvs2 = m_textInfo.meshInfo[0].uvs2;
			Vector2 vector2 = new Vector2(0f, 1f);
			uvs2[index] = vector2;
			uvs2[1 + index] = vector2;
			uvs2[2 + index] = vector2;
			uvs2[3 + index] = vector2;
			Color32[] colors = m_textInfo.meshInfo[0].colors32;
			highlightColor.a = ((m_htmlColor.a < highlightColor.a) ? m_htmlColor.a : highlightColor.a);
			colors[index] = highlightColor;
			colors[1 + index] = highlightColor;
			colors[2 + index] = highlightColor;
			colors[3 + index] = highlightColor;
			index += 4;
		}

		protected void LoadDefaultSettings()
		{
			if (m_text == null)
			{
				if (TMP_Settings.autoSizeTextContainer)
				{
					autoSizeTextContainer = true;
				}
				else
				{
					m_rectTransform = rectTransform;
					if (base.GetType() == typeof(TextMeshPro))
					{
						m_rectTransform.sizeDelta = TMP_Settings.defaultTextMeshProTextContainerSize;
					}
					else
					{
						m_rectTransform.sizeDelta = TMP_Settings.defaultTextMeshProUITextContainerSize;
					}
				}
				m_enableWordWrapping = TMP_Settings.enableWordWrapping;
				m_enableKerning = TMP_Settings.enableKerning;
				m_enableExtraPadding = TMP_Settings.enableExtraPadding;
				m_tintAllSprites = TMP_Settings.enableTintAllSprites;
				m_parseCtrlCharacters = TMP_Settings.enableParseEscapeCharacters;
				m_fontSize = (m_fontSizeBase = TMP_Settings.defaultFontSize);
				m_fontSizeMin = m_fontSize * TMP_Settings.defaultTextAutoSizingMinRatio;
				m_fontSizeMax = m_fontSize * TMP_Settings.defaultTextAutoSizingMaxRatio;
				m_isAlignmentEnumConverted = true;
			}
			else if (!m_isAlignmentEnumConverted)
			{
				m_isAlignmentEnumConverted = true;
				m_textAlignment = TMP_Compatibility.ConvertTextAlignmentEnumValues(m_textAlignment);
			}
		}

		protected void GetSpecialCharacters(TMP_FontAsset fontAsset)
		{
			if (!fontAsset.characterDictionary.TryGetValue(95, out m_cached_Underline_GlyphInfo))
			{
			}
			if (fontAsset.characterDictionary.TryGetValue(8230, out m_cached_Ellipsis_GlyphInfo))
			{
			}
		}

		protected void ReplaceTagWithCharacter(int[] chars, int insertionIndex, int tagLength, char c)
		{
			chars[insertionIndex] = c;
			for (int i = insertionIndex + tagLength; i < chars.Length; i++)
			{
				chars[i - 3] = chars[i];
			}
		}

		protected TMP_FontAsset GetFontAssetForWeight(int fontWeight)
		{
			bool flag = (m_style & FontStyles.Italic) == FontStyles.Italic || (m_fontStyle & FontStyles.Italic) == FontStyles.Italic;
			TMP_FontAsset tMP_FontAsset = null;
			int num = fontWeight / 100;
			if (flag)
			{
				return m_currentFontAsset.fontWeights[num].italicTypeface;
			}
			return m_currentFontAsset.fontWeights[num].regularTypeface;
		}

		protected virtual void SetActiveSubMeshes(bool state)
		{
		}

		protected virtual void ClearSubMeshObjects()
		{
		}

		public virtual void ClearMesh()
		{
		}

		public virtual void ClearMesh(bool uploadGeometry)
		{
		}

		public virtual string GetParsedText()
		{
			if (m_textInfo == null)
			{
				return string.Empty;
			}
			int characterCount = m_textInfo.characterCount;
			char[] array = new char[characterCount];
			for (int i = 0; i < characterCount && i < m_textInfo.characterInfo.Length; i++)
			{
				array[i] = m_textInfo.characterInfo[i].character;
			}
			return new string(array);
		}

		protected Vector2 PackUV(float x, float y, float scale)
		{
			Vector2 result = default(Vector2);
			result.x = (int)(x * 511f);
			result.y = (int)(y * 511f);
			result.x = result.x * 4096f + result.y;
			result.y = scale;
			return result;
		}

		protected float PackUV(float x, float y)
		{
			double num = (int)(x * 511f);
			double num2 = (int)(y * 511f);
			return (float)(num * 4096.0 + num2);
		}

		protected int HexToInt(char hex)
		{
			switch (hex)
			{
			case '0':
				return 0;
			case '1':
				return 1;
			case '2':
				return 2;
			case '3':
				return 3;
			case '4':
				return 4;
			case '5':
				return 5;
			case '6':
				return 6;
			case '7':
				return 7;
			case '8':
				return 8;
			case '9':
				return 9;
			case 'A':
				return 10;
			case 'B':
				return 11;
			case 'C':
				return 12;
			case 'D':
				return 13;
			case 'E':
				return 14;
			case 'F':
				return 15;
			case 'a':
				return 10;
			case 'b':
				return 11;
			case 'c':
				return 12;
			case 'd':
				return 13;
			case 'e':
				return 14;
			case 'f':
				return 15;
			default:
				return 15;
			}
		}

		protected int GetUTF16(int i)
		{
			int num = HexToInt(m_text[i]) << 12;
			num += HexToInt(m_text[i + 1]) << 8;
			num += HexToInt(m_text[i + 2]) << 4;
			return num + HexToInt(m_text[i + 3]);
		}

		protected int GetUTF32(int i)
		{
			int num = 0;
			num += HexToInt(m_text[i]) << 30;
			num += HexToInt(m_text[i + 1]) << 24;
			num += HexToInt(m_text[i + 2]) << 20;
			num += HexToInt(m_text[i + 3]) << 16;
			num += HexToInt(m_text[i + 4]) << 12;
			num += HexToInt(m_text[i + 5]) << 8;
			num += HexToInt(m_text[i + 6]) << 4;
			return num + HexToInt(m_text[i + 7]);
		}

		protected Color32 HexCharsToColor(char[] hexChars, int tagCount)
		{
			switch (tagCount)
			{
			case 4:
			{
				byte r8 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[1]));
				byte g8 = (byte)(HexToInt(hexChars[2]) * 16 + HexToInt(hexChars[2]));
				byte b8 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[3]));
				return new Color32(r8, g8, b8, byte.MaxValue);
			}
			case 5:
			{
				byte r7 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[1]));
				byte g7 = (byte)(HexToInt(hexChars[2]) * 16 + HexToInt(hexChars[2]));
				byte b7 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[3]));
				byte a4 = (byte)(HexToInt(hexChars[4]) * 16 + HexToInt(hexChars[4]));
				return new Color32(r7, g7, b7, a4);
			}
			case 7:
			{
				byte r6 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[2]));
				byte g6 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[4]));
				byte b6 = (byte)(HexToInt(hexChars[5]) * 16 + HexToInt(hexChars[6]));
				return new Color32(r6, g6, b6, byte.MaxValue);
			}
			case 9:
			{
				byte r5 = (byte)(HexToInt(hexChars[1]) * 16 + HexToInt(hexChars[2]));
				byte g5 = (byte)(HexToInt(hexChars[3]) * 16 + HexToInt(hexChars[4]));
				byte b5 = (byte)(HexToInt(hexChars[5]) * 16 + HexToInt(hexChars[6]));
				byte a3 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
				return new Color32(r5, g5, b5, a3);
			}
			case 10:
			{
				byte r4 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[7]));
				byte g4 = (byte)(HexToInt(hexChars[8]) * 16 + HexToInt(hexChars[8]));
				byte b4 = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[9]));
				return new Color32(r4, g4, b4, byte.MaxValue);
			}
			case 11:
			{
				byte r3 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[7]));
				byte g3 = (byte)(HexToInt(hexChars[8]) * 16 + HexToInt(hexChars[8]));
				byte b3 = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[9]));
				byte a2 = (byte)(HexToInt(hexChars[10]) * 16 + HexToInt(hexChars[10]));
				return new Color32(r3, g3, b3, a2);
			}
			case 13:
			{
				byte r2 = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
				byte g2 = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[10]));
				byte b2 = (byte)(HexToInt(hexChars[11]) * 16 + HexToInt(hexChars[12]));
				return new Color32(r2, g2, b2, byte.MaxValue);
			}
			case 15:
			{
				byte r = (byte)(HexToInt(hexChars[7]) * 16 + HexToInt(hexChars[8]));
				byte g = (byte)(HexToInt(hexChars[9]) * 16 + HexToInt(hexChars[10]));
				byte b = (byte)(HexToInt(hexChars[11]) * 16 + HexToInt(hexChars[12]));
				byte a = (byte)(HexToInt(hexChars[13]) * 16 + HexToInt(hexChars[14]));
				return new Color32(r, g, b, a);
			}
			default:
				return new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			}
		}

		protected Color32 HexCharsToColor(char[] hexChars, int startIndex, int length)
		{
			switch (length)
			{
			case 7:
			{
				byte r2 = (byte)(HexToInt(hexChars[startIndex + 1]) * 16 + HexToInt(hexChars[startIndex + 2]));
				byte g2 = (byte)(HexToInt(hexChars[startIndex + 3]) * 16 + HexToInt(hexChars[startIndex + 4]));
				byte b2 = (byte)(HexToInt(hexChars[startIndex + 5]) * 16 + HexToInt(hexChars[startIndex + 6]));
				return new Color32(r2, g2, b2, byte.MaxValue);
			}
			case 9:
			{
				byte r = (byte)(HexToInt(hexChars[startIndex + 1]) * 16 + HexToInt(hexChars[startIndex + 2]));
				byte g = (byte)(HexToInt(hexChars[startIndex + 3]) * 16 + HexToInt(hexChars[startIndex + 4]));
				byte b = (byte)(HexToInt(hexChars[startIndex + 5]) * 16 + HexToInt(hexChars[startIndex + 6]));
				byte a = (byte)(HexToInt(hexChars[startIndex + 7]) * 16 + HexToInt(hexChars[startIndex + 8]));
				return new Color32(r, g, b, a);
			}
			default:
				return s_colorWhite;
			}
		}

		private int GetAttributeParameters(char[] chars, int startIndex, int length, ref float[] parameters)
		{
			int lastIndex = startIndex;
			int num = 0;
			while (lastIndex < startIndex + length)
			{
				parameters[num] = ConvertToFloat(chars, startIndex, length, out lastIndex);
				length -= lastIndex - startIndex + 1;
				startIndex = lastIndex + 1;
				num++;
			}
			return num;
		}

		protected float ConvertToFloat(char[] chars, int startIndex, int length)
		{
			int lastIndex = 0;
			return ConvertToFloat(chars, startIndex, length, out lastIndex);
		}

		protected float ConvertToFloat(char[] chars, int startIndex, int length, out int lastIndex)
		{
			if (startIndex == 0)
			{
				lastIndex = 0;
				return -9999f;
			}
			int num = startIndex + length;
			float num2 = 0f;
			int num3 = 0;
			int num4 = 0;
			int num5 = 1;
			for (int i = startIndex; i < num; i++)
			{
				char c = chars[i];
				switch (c)
				{
				case '.':
					num4 = i;
					num3 = -1;
					continue;
				case '-':
					num5 = -1;
					continue;
				case '+':
					num5 = 1;
					continue;
				case ',':
					lastIndex = i;
					return num2 * (float)num5;
				case ' ':
					continue;
				}
				if (!char.IsDigit(c))
				{
					lastIndex = i;
					return -9999f;
				}
				switch (num3)
				{
				case 0:
					num2 = chars[i] - 48;
					break;
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
				case 6:
					num2 = num2 * 10f + (float)(int)chars[i] - 48f;
					break;
				case -1:
					num2 += (float)(chars[i] - 48) * 0.1f;
					break;
				case -2:
					num2 += (float)(chars[i] - 48) * 0.01f;
					break;
				case -3:
					num2 += (float)(chars[i] - 48) * 0.001f;
					break;
				case -4:
					num2 += (float)(chars[i] - 48) * 0.0001f;
					break;
				case -5:
					num2 += (float)(chars[i] - 48) * 1E-05f;
					break;
				}
				num3 = ((num4 != 0) ? (num3 - 1) : (num3 + 1));
			}
			lastIndex = num;
			return num2 *= (float)num5;
		}

		protected bool ValidateHtmlTag(int[] chars, int startIndex, out int endIndex)
		{
			int num = 0;
			byte b = 0;
			TagUnits tagUnits = TagUnits.Pixels;
			TagType tagType = TagType.None;
			int num2 = 0;
			m_xmlAttribute[num2].nameHashCode = 0;
			m_xmlAttribute[num2].valueType = TagType.None;
			m_xmlAttribute[num2].valueHashCode = 0;
			m_xmlAttribute[num2].valueStartIndex = 0;
			m_xmlAttribute[num2].valueLength = 0;
			m_xmlAttribute[1].nameHashCode = 0;
			m_xmlAttribute[2].nameHashCode = 0;
			m_xmlAttribute[3].nameHashCode = 0;
			m_xmlAttribute[4].nameHashCode = 0;
			endIndex = startIndex;
			bool flag = false;
			bool flag2 = false;
			for (int i = startIndex; i < chars.Length && chars[i] != 0; i++)
			{
				if (num >= m_htmlTag.Length)
				{
					break;
				}
				if (chars[i] == 60)
				{
					break;
				}
				if (chars[i] == 62)
				{
					flag2 = true;
					endIndex = i;
					m_htmlTag[num] = '\0';
					break;
				}
				m_htmlTag[num] = (char)chars[i];
				num++;
				if (b == 1)
				{
					switch (tagType)
					{
					case TagType.None:
						if (chars[i] == 43 || chars[i] == 45 || char.IsDigit((char)chars[i]))
						{
							tagType = TagType.NumericalValue;
							m_xmlAttribute[num2].valueType = TagType.NumericalValue;
							m_xmlAttribute[num2].valueStartIndex = num - 1;
							m_xmlAttribute[num2].valueLength++;
						}
						else if (chars[i] == 35)
						{
							tagType = TagType.ColorValue;
							m_xmlAttribute[num2].valueType = TagType.ColorValue;
							m_xmlAttribute[num2].valueStartIndex = num - 1;
							m_xmlAttribute[num2].valueLength++;
						}
						else if (chars[i] == 34)
						{
							tagType = TagType.StringValue;
							m_xmlAttribute[num2].valueType = TagType.StringValue;
							m_xmlAttribute[num2].valueStartIndex = num;
						}
						else
						{
							tagType = TagType.StringValue;
							m_xmlAttribute[num2].valueType = TagType.StringValue;
							m_xmlAttribute[num2].valueStartIndex = num - 1;
							m_xmlAttribute[num2].valueHashCode = (((m_xmlAttribute[num2].valueHashCode << 5) + m_xmlAttribute[num2].valueHashCode) ^ chars[i]);
							m_xmlAttribute[num2].valueLength++;
						}
						break;
					case TagType.NumericalValue:
						if (chars[i] == 112 || chars[i] == 101 || chars[i] == 37 || chars[i] == 32)
						{
							b = 2;
							tagType = TagType.None;
							num2++;
							m_xmlAttribute[num2].nameHashCode = 0;
							m_xmlAttribute[num2].valueType = TagType.None;
							m_xmlAttribute[num2].valueHashCode = 0;
							m_xmlAttribute[num2].valueStartIndex = 0;
							m_xmlAttribute[num2].valueLength = 0;
							if (chars[i] == 101)
							{
								tagUnits = TagUnits.FontUnits;
							}
							else if (chars[i] == 37)
							{
								tagUnits = TagUnits.Percentage;
							}
						}
						else if (b != 2)
						{
							m_xmlAttribute[num2].valueLength++;
						}
						break;
					case TagType.ColorValue:
						if (chars[i] != 32)
						{
							m_xmlAttribute[num2].valueLength++;
							break;
						}
						b = 2;
						tagType = TagType.None;
						num2++;
						m_xmlAttribute[num2].nameHashCode = 0;
						m_xmlAttribute[num2].valueType = TagType.None;
						m_xmlAttribute[num2].valueHashCode = 0;
						m_xmlAttribute[num2].valueStartIndex = 0;
						m_xmlAttribute[num2].valueLength = 0;
						break;
					case TagType.StringValue:
						if (chars[i] != 34)
						{
							m_xmlAttribute[num2].valueHashCode = (((m_xmlAttribute[num2].valueHashCode << 5) + m_xmlAttribute[num2].valueHashCode) ^ chars[i]);
							m_xmlAttribute[num2].valueLength++;
							break;
						}
						b = 2;
						tagType = TagType.None;
						num2++;
						m_xmlAttribute[num2].nameHashCode = 0;
						m_xmlAttribute[num2].valueType = TagType.None;
						m_xmlAttribute[num2].valueHashCode = 0;
						m_xmlAttribute[num2].valueStartIndex = 0;
						m_xmlAttribute[num2].valueLength = 0;
						break;
					}
				}
				if (chars[i] == 61)
				{
					b = 1;
				}
				if (b == 0 && chars[i] == 32)
				{
					if (flag)
					{
						return false;
					}
					flag = true;
					b = 2;
					tagType = TagType.None;
					num2++;
					m_xmlAttribute[num2].nameHashCode = 0;
					m_xmlAttribute[num2].valueType = TagType.None;
					m_xmlAttribute[num2].valueHashCode = 0;
					m_xmlAttribute[num2].valueStartIndex = 0;
					m_xmlAttribute[num2].valueLength = 0;
				}
				if (b == 0)
				{
					m_xmlAttribute[num2].nameHashCode = (m_xmlAttribute[num2].nameHashCode << 3) - m_xmlAttribute[num2].nameHashCode + chars[i];
				}
				if (b == 2 && chars[i] == 32)
				{
					b = 0;
				}
			}
			if (!flag2)
			{
				return false;
			}
			if (tag_NoParsing && m_xmlAttribute[0].nameHashCode != 53822163 && m_xmlAttribute[0].nameHashCode != 49429939)
			{
				return false;
			}
			if (m_xmlAttribute[0].nameHashCode == 53822163 || m_xmlAttribute[0].nameHashCode == 49429939)
			{
				tag_NoParsing = false;
				return true;
			}
			if (m_htmlTag[0] == '#' && num == 4)
			{
				m_htmlColor = HexCharsToColor(m_htmlTag, num);
				m_colorStack.Add(m_htmlColor);
				return true;
			}
			if (m_htmlTag[0] == '#' && num == 5)
			{
				m_htmlColor = HexCharsToColor(m_htmlTag, num);
				m_colorStack.Add(m_htmlColor);
				return true;
			}
			if (m_htmlTag[0] == '#' && num == 7)
			{
				m_htmlColor = HexCharsToColor(m_htmlTag, num);
				m_colorStack.Add(m_htmlColor);
				return true;
			}
			if (m_htmlTag[0] == '#' && num == 9)
			{
				m_htmlColor = HexCharsToColor(m_htmlTag, num);
				m_colorStack.Add(m_htmlColor);
				return true;
			}
			float num3 = 0f;
			Material material;
			switch (m_xmlAttribute[0].nameHashCode)
			{
			case 66:
			case 98:
				m_style |= FontStyles.Bold;
				m_fontStyleStack.Add(FontStyles.Bold);
				m_fontWeightInternal = 700;
				m_fontWeightStack.Add(700);
				return true;
			case 395:
			case 427:
				if ((m_fontStyle & FontStyles.Bold) != FontStyles.Bold)
				{
					m_fontWeightInternal = m_fontWeightStack.Remove();
					if (m_fontStyleStack.Remove(FontStyles.Bold) == 0)
					{
						m_style &= (FontStyles)(-2);
					}
				}
				return true;
			case 73:
			case 105:
				m_style |= FontStyles.Italic;
				m_fontStyleStack.Add(FontStyles.Italic);
				return true;
			case 402:
			case 434:
				if (m_fontStyleStack.Remove(FontStyles.Italic) == 0)
				{
					m_style &= (FontStyles)(-3);
				}
				return true;
			case 83:
			case 115:
				m_style |= FontStyles.Strikethrough;
				m_fontStyleStack.Add(FontStyles.Strikethrough);
				if (m_xmlAttribute[1].nameHashCode == 281955 || m_xmlAttribute[1].nameHashCode == 192323)
				{
					m_strikethroughColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength);
				}
				else
				{
					m_strikethroughColor = m_htmlColor;
				}
				m_strikethroughColorStack.Add(m_strikethroughColor);
				return true;
			case 412:
			case 444:
				if ((m_fontStyle & FontStyles.Strikethrough) != FontStyles.Strikethrough && m_fontStyleStack.Remove(FontStyles.Strikethrough) == 0)
				{
					m_style &= (FontStyles)(-65);
				}
				return true;
			case 85:
			case 117:
				m_style |= FontStyles.Underline;
				m_fontStyleStack.Add(FontStyles.Underline);
				if (m_xmlAttribute[1].nameHashCode == 281955 || m_xmlAttribute[1].nameHashCode == 192323)
				{
					m_underlineColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength);
				}
				else
				{
					m_underlineColor = m_htmlColor;
				}
				m_underlineColorStack.Add(m_underlineColor);
				return true;
			case 414:
			case 446:
				if ((m_fontStyle & FontStyles.Underline) != FontStyles.Underline)
				{
					m_underlineColor = m_underlineColorStack.Remove();
					if (m_fontStyleStack.Remove(FontStyles.Underline) == 0)
					{
						m_style &= (FontStyles)(-5);
					}
				}
				return true;
			case 30245:
			case 43045:
				m_style |= FontStyles.Highlight;
				m_fontStyleStack.Add(FontStyles.Highlight);
				m_highlightColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				m_highlightColorStack.Add(m_highlightColor);
				return true;
			case 143092:
			case 155892:
				if ((m_fontStyle & FontStyles.Highlight) != FontStyles.Highlight)
				{
					m_highlightColor = m_highlightColorStack.Remove();
					if (m_fontStyleStack.Remove(FontStyles.Highlight) == 0)
					{
						m_style &= (FontStyles)(-513);
					}
				}
				return true;
			case 4728:
			case 6552:
				m_fontScaleMultiplier *= ((m_currentFontAsset.fontInfo.SubSize > 0f) ? m_currentFontAsset.fontInfo.SubSize : 1f);
				m_baselineOffset += m_currentFontAsset.fontInfo.SubscriptOffset * m_fontScale * m_fontScaleMultiplier;
				m_fontStyleStack.Add(FontStyles.Subscript);
				m_style |= FontStyles.Subscript;
				return true;
			case 20849:
			case 22673:
				if ((m_style & FontStyles.Subscript) == FontStyles.Subscript)
				{
					if (m_fontScaleMultiplier < 1f)
					{
						m_baselineOffset -= m_currentFontAsset.fontInfo.SubscriptOffset * m_fontScale * m_fontScaleMultiplier;
						m_fontScaleMultiplier /= ((m_currentFontAsset.fontInfo.SubSize > 0f) ? m_currentFontAsset.fontInfo.SubSize : 1f);
					}
					if (m_fontStyleStack.Remove(FontStyles.Subscript) == 0)
					{
						m_style &= (FontStyles)(-257);
					}
				}
				return true;
			case 4742:
			case 6566:
				m_fontScaleMultiplier *= ((m_currentFontAsset.fontInfo.SubSize > 0f) ? m_currentFontAsset.fontInfo.SubSize : 1f);
				m_baselineOffset += m_currentFontAsset.fontInfo.SuperscriptOffset * m_fontScale * m_fontScaleMultiplier;
				m_fontStyleStack.Add(FontStyles.Superscript);
				m_style |= FontStyles.Superscript;
				return true;
			case 20863:
			case 22687:
				if ((m_style & FontStyles.Superscript) == FontStyles.Superscript)
				{
					if (m_fontScaleMultiplier < 1f)
					{
						m_baselineOffset -= m_currentFontAsset.fontInfo.SuperscriptOffset * m_fontScale * m_fontScaleMultiplier;
						m_fontScaleMultiplier /= ((m_currentFontAsset.fontInfo.SubSize > 0f) ? m_currentFontAsset.fontInfo.SubSize : 1f);
					}
					if (m_fontStyleStack.Remove(FontStyles.Superscript) == 0)
					{
						m_style &= (FontStyles)(-129);
					}
				}
				return true;
			case -330774850:
			case 2012149182:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				if ((m_fontStyle & FontStyles.Bold) == FontStyles.Bold)
				{
					return true;
				}
				m_style &= (FontStyles)(-2);
				switch ((int)num3)
				{
				case 100:
					m_fontWeightInternal = 100;
					break;
				case 200:
					m_fontWeightInternal = 200;
					break;
				case 300:
					m_fontWeightInternal = 300;
					break;
				case 400:
					m_fontWeightInternal = 400;
					break;
				case 500:
					m_fontWeightInternal = 500;
					break;
				case 600:
					m_fontWeightInternal = 600;
					break;
				case 700:
					m_fontWeightInternal = 700;
					m_style |= FontStyles.Bold;
					break;
				case 800:
					m_fontWeightInternal = 800;
					break;
				case 900:
					m_fontWeightInternal = 900;
					break;
				}
				m_fontWeightStack.Add(m_fontWeightInternal);
				return true;
			case -1885698441:
			case 457225591:
				m_fontWeightInternal = m_fontWeightStack.Remove();
				if (m_fontWeightInternal == 400)
				{
					m_style &= (FontStyles)(-2);
				}
				return true;
			case 4556:
			case 6380:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f)
				{
					return false;
				}
				switch (tagUnits)
				{
				case TagUnits.Pixels:
					m_xAdvance = num3;
					return true;
				case TagUnits.FontUnits:
					m_xAdvance = num3 * m_fontScale * m_fontAsset.fontInfo.TabWidth / (float)(int)m_fontAsset.tabSize;
					return true;
				case TagUnits.Percentage:
					m_xAdvance = m_marginWidth * num3 / 100f;
					return true;
				default:
					return false;
				}
			case 20677:
			case 22501:
				m_isIgnoringAlignment = false;
				return true;
			case 11642281:
			case 16034505:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				switch (tagUnits)
				{
				case TagUnits.Pixels:
					m_baselineOffset = num3;
					return true;
				case TagUnits.FontUnits:
					m_baselineOffset = num3 * m_fontScale * m_fontAsset.fontInfo.Ascender;
					return true;
				case TagUnits.Percentage:
					return false;
				default:
					return false;
				}
			case 50348802:
			case 54741026:
				m_baselineOffset = 0f;
				return true;
			case 31191:
			case 43991:
				if (m_overflowMode == TextOverflowModes.Page)
				{
					m_xAdvance = 0f + tag_LineIndent + tag_Indent;
					m_lineOffset = 0f;
					m_pageNumber++;
					m_isNewPage = true;
				}
				return true;
			case 31169:
			case 43969:
				m_isNonBreakingSpace = true;
				return true;
			case 144016:
			case 156816:
				m_isNonBreakingSpace = false;
				return true;
			case 32745:
			case 45545:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f)
				{
					return false;
				}
				switch (tagUnits)
				{
				case TagUnits.Pixels:
					if (m_htmlTag[5] == '+')
					{
						m_currentFontSize = m_fontSize + num3;
						m_sizeStack.Add(m_currentFontSize);
						m_fontScale = m_currentFontSize / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
						return true;
					}
					if (m_htmlTag[5] == '-')
					{
						m_currentFontSize = m_fontSize + num3;
						m_sizeStack.Add(m_currentFontSize);
						m_fontScale = m_currentFontSize / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
						return true;
					}
					m_currentFontSize = num3;
					m_sizeStack.Add(m_currentFontSize);
					m_fontScale = m_currentFontSize / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
					return true;
				case TagUnits.FontUnits:
					m_currentFontSize = m_fontSize * num3;
					m_sizeStack.Add(m_currentFontSize);
					m_fontScale = m_currentFontSize / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
					return true;
				case TagUnits.Percentage:
					m_currentFontSize = m_fontSize * num3 / 100f;
					m_sizeStack.Add(m_currentFontSize);
					m_fontScale = m_currentFontSize / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
					return true;
				default:
					return false;
				}
			case 145592:
			case 158392:
				m_currentFontSize = m_sizeStack.Remove();
				m_fontScale = m_currentFontSize / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
				return true;
			case 28511:
			case 41311:
			{
				int valueHashCode2 = m_xmlAttribute[0].valueHashCode;
				int nameHashCode2 = m_xmlAttribute[1].nameHashCode;
				int valueHashCode3 = m_xmlAttribute[1].valueHashCode;
				if (valueHashCode2 == 764638571 || valueHashCode2 == 523367755)
				{
					m_currentFontAsset = m_materialReferences[0].fontAsset;
					m_currentMaterial = m_materialReferences[0].material;
					m_currentMaterialIndex = 0;
					m_fontScale = m_currentFontSize / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
					m_materialReferenceStack.Add(m_materialReferences[0]);
					return true;
				}
				TMP_FontAsset fontAsset;
				if (!MaterialReferenceManager.TryGetFontAsset(valueHashCode2, out fontAsset))
				{
					fontAsset = Resources.Load<TMP_FontAsset>(TMP_Settings.defaultFontAssetPath + new string(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));
					if (fontAsset == null)
					{
						return false;
					}
					MaterialReferenceManager.AddFontAsset(fontAsset);
				}
				if (nameHashCode2 == 0 && valueHashCode3 == 0)
				{
					m_currentMaterial = fontAsset.material;
					m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, fontAsset, m_materialReferences, m_materialReferenceIndexLookup);
					m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
				}
				else
				{
					if (nameHashCode2 != 103415287 && nameHashCode2 != 72669687)
					{
						return false;
					}
					if (MaterialReferenceManager.TryGetMaterial(valueHashCode3, out material))
					{
						m_currentMaterial = material;
						m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, fontAsset, m_materialReferences, m_materialReferenceIndexLookup);
						m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
					}
					else
					{
						material = Resources.Load<Material>(TMP_Settings.defaultFontAssetPath + new string(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength));
						if (material == null)
						{
							return false;
						}
						MaterialReferenceManager.AddFontMaterial(valueHashCode3, material);
						m_currentMaterial = material;
						m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, fontAsset, m_materialReferences, m_materialReferenceIndexLookup);
						m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
					}
				}
				m_currentFontAsset = fontAsset;
				m_fontScale = m_currentFontSize / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
				return true;
			}
			case 141358:
			case 154158:
			{
				MaterialReference materialReference = m_materialReferenceStack.Remove();
				m_currentFontAsset = materialReference.fontAsset;
				m_currentMaterial = materialReference.material;
				m_currentMaterialIndex = materialReference.index;
				m_fontScale = m_currentFontSize / m_currentFontAsset.fontInfo.PointSize * m_currentFontAsset.fontInfo.Scale * (m_isOrthographic ? 1f : 0.1f);
				return true;
			}
			case 72669687:
			case 103415287:
			{
				int valueHashCode3 = m_xmlAttribute[0].valueHashCode;
				if (valueHashCode3 == 764638571 || valueHashCode3 == 523367755)
				{
					if (m_currentFontAsset.atlas.GetInstanceID() != m_currentMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
					{
						return false;
					}
					m_currentMaterial = m_materialReferences[0].material;
					m_currentMaterialIndex = 0;
					m_materialReferenceStack.Add(m_materialReferences[0]);
					return true;
				}
				if (MaterialReferenceManager.TryGetMaterial(valueHashCode3, out material))
				{
					if (m_currentFontAsset.atlas.GetInstanceID() != material.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
					{
						return false;
					}
					m_currentMaterial = material;
					m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
					m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
				}
				else
				{
					material = Resources.Load<Material>(TMP_Settings.defaultFontAssetPath + new string(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));
					if (material == null)
					{
						return false;
					}
					if (m_currentFontAsset.atlas.GetInstanceID() != material.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
					{
						return false;
					}
					MaterialReferenceManager.AddFontMaterial(valueHashCode3, material);
					m_currentMaterial = material;
					m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentMaterial, m_currentFontAsset, m_materialReferences, m_materialReferenceIndexLookup);
					m_materialReferenceStack.Add(m_materialReferences[m_currentMaterialIndex]);
				}
				return true;
			}
			case 343615334:
			case 374360934:
			{
				if (m_currentMaterial.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID() != m_materialReferenceStack.PreviousItem().material.GetTexture(ShaderUtilities.ID_MainTex).GetInstanceID())
				{
					return false;
				}
				MaterialReference materialReference2 = m_materialReferenceStack.Remove();
				m_currentMaterial = materialReference2.material;
				m_currentMaterialIndex = materialReference2.index;
				return true;
			}
			case 230446:
			case 320078:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				switch (tagUnits)
				{
				case TagUnits.Pixels:
					m_xAdvance += num3;
					return true;
				case TagUnits.FontUnits:
					m_xAdvance += num3 * m_fontScale * m_fontAsset.fontInfo.TabWidth / (float)(int)m_fontAsset.tabSize;
					return true;
				case TagUnits.Percentage:
					return false;
				default:
					return false;
				}
			case 186622:
			case 276254:
				if (m_xmlAttribute[0].valueLength != 3)
				{
					return false;
				}
				m_htmlColor.a = (byte)(HexToInt(m_htmlTag[7]) * 16 + HexToInt(m_htmlTag[8]));
				return true;
			case 1750458:
				return false;
			case 426:
				return true;
			case 30266:
			case 43066:
				if (m_isParsingText && !m_isCalculatingPreferredValues)
				{
					int linkCount = m_textInfo.linkCount;
					if (linkCount + 1 > m_textInfo.linkInfo.Length)
					{
						TMP_TextInfo.Resize(ref m_textInfo.linkInfo, linkCount + 1);
					}
					m_textInfo.linkInfo[linkCount].textComponent = this;
					m_textInfo.linkInfo[linkCount].hashCode = m_xmlAttribute[0].valueHashCode;
					m_textInfo.linkInfo[linkCount].linkTextfirstCharacterIndex = m_characterCount;
					m_textInfo.linkInfo[linkCount].linkIdFirstCharacterIndex = startIndex + m_xmlAttribute[0].valueStartIndex;
					m_textInfo.linkInfo[linkCount].linkIdLength = m_xmlAttribute[0].valueLength;
					m_textInfo.linkInfo[linkCount].SetLinkID(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				}
				return true;
			case 143113:
			case 155913:
				if (m_isParsingText && !m_isCalculatingPreferredValues)
				{
					m_textInfo.linkInfo[m_textInfo.linkCount].linkTextLength = m_characterCount - m_textInfo.linkInfo[m_textInfo.linkCount].linkTextfirstCharacterIndex;
					m_textInfo.linkCount++;
				}
				return true;
			case 186285:
			case 275917:
				switch (m_xmlAttribute[0].valueHashCode)
				{
				case 3774683:
					m_lineJustification = TextAlignmentOptions.Left;
					m_lineJustificationStack.Add(m_lineJustification);
					return true;
				case 136703040:
					m_lineJustification = TextAlignmentOptions.Right;
					m_lineJustificationStack.Add(m_lineJustification);
					return true;
				case -458210101:
					m_lineJustification = TextAlignmentOptions.Center;
					m_lineJustificationStack.Add(m_lineJustification);
					return true;
				case -523808257:
					m_lineJustification = TextAlignmentOptions.Justified;
					m_lineJustificationStack.Add(m_lineJustification);
					return true;
				case 122383428:
					m_lineJustification = TextAlignmentOptions.Justified;
					m_lineJustificationStack.Add(m_lineJustification);
					return true;
				default:
					return false;
				}
			case 976214:
			case 1065846:
				m_lineJustification = m_lineJustificationStack.Remove();
				return true;
			case 237918:
			case 327550:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				switch (tagUnits)
				{
				case TagUnits.Pixels:
					m_width = num3;
					break;
				case TagUnits.FontUnits:
					return false;
				case TagUnits.Percentage:
					m_width = m_marginWidth * num3 / 100f;
					break;
				}
				return true;
			case 1027847:
			case 1117479:
				m_width = -1f;
				return true;
			case 192323:
			case 281955:
				if (m_htmlTag[6] == '#' && num == 10)
				{
					m_htmlColor = HexCharsToColor(m_htmlTag, num);
					m_colorStack.Add(m_htmlColor);
					return true;
				}
				if (m_htmlTag[6] == '#' && num == 11)
				{
					m_htmlColor = HexCharsToColor(m_htmlTag, num);
					m_colorStack.Add(m_htmlColor);
					return true;
				}
				if (m_htmlTag[6] == '#' && num == 13)
				{
					m_htmlColor = HexCharsToColor(m_htmlTag, num);
					m_colorStack.Add(m_htmlColor);
					return true;
				}
				if (m_htmlTag[6] == '#' && num == 15)
				{
					m_htmlColor = HexCharsToColor(m_htmlTag, num);
					m_colorStack.Add(m_htmlColor);
					return true;
				}
				switch (m_xmlAttribute[0].valueHashCode)
				{
				case 125395:
					m_htmlColor = Color.red;
					m_colorStack.Add(m_htmlColor);
					return true;
				case 3573310:
					m_htmlColor = Color.blue;
					m_colorStack.Add(m_htmlColor);
					return true;
				case 117905991:
					m_htmlColor = Color.black;
					m_colorStack.Add(m_htmlColor);
					return true;
				case 121463835:
					m_htmlColor = Color.green;
					m_colorStack.Add(m_htmlColor);
					return true;
				case 140357351:
					m_htmlColor = Color.white;
					m_colorStack.Add(m_htmlColor);
					return true;
				case 26556144:
					m_htmlColor = new Color32(byte.MaxValue, 128, 0, byte.MaxValue);
					m_colorStack.Add(m_htmlColor);
					return true;
				case -36881330:
					m_htmlColor = new Color32(160, 32, 240, byte.MaxValue);
					m_colorStack.Add(m_htmlColor);
					return true;
				case 554054276:
					m_htmlColor = Color.yellow;
					m_colorStack.Add(m_htmlColor);
					return true;
				default:
					return false;
				}
			case 1356515:
			case 1983971:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				switch (tagUnits)
				{
				case TagUnits.Pixels:
					m_cSpacing = num3;
					break;
				case TagUnits.FontUnits:
					m_cSpacing = num3;
					m_cSpacing *= m_fontScale * m_fontAsset.fontInfo.TabWidth / (float)(int)m_fontAsset.tabSize;
					break;
				case TagUnits.Percentage:
					return false;
				}
				return true;
			case 6886018:
			case 7513474:
				if (!m_isParsingText)
				{
					return true;
				}
				if (m_characterCount > 0)
				{
					m_xAdvance -= m_cSpacing;
					m_textInfo.characterInfo[m_characterCount - 1].xAdvance = m_xAdvance;
				}
				m_cSpacing = 0f;
				return true;
			case 1524585:
			case 2152041:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				switch (tagUnits)
				{
				case TagUnits.Pixels:
					m_monoSpacing = num3;
					break;
				case TagUnits.FontUnits:
					m_monoSpacing = num3;
					m_monoSpacing *= m_fontScale * m_fontAsset.fontInfo.TabWidth / (float)(int)m_fontAsset.tabSize;
					break;
				case TagUnits.Percentage:
					return false;
				}
				return true;
			case 7054088:
			case 7681544:
				m_monoSpacing = 0f;
				return true;
			case 280416:
				return false;
			case 982252:
			case 1071884:
				m_htmlColor = m_colorStack.Remove();
				return true;
			case 1441524:
			case 2068980:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				switch (tagUnits)
				{
				case TagUnits.Pixels:
					tag_Indent = num3;
					break;
				case TagUnits.FontUnits:
					tag_Indent = num3;
					tag_Indent *= m_fontScale * m_fontAsset.fontInfo.TabWidth / (float)(int)m_fontAsset.tabSize;
					break;
				case TagUnits.Percentage:
					tag_Indent = m_marginWidth * num3 / 100f;
					break;
				}
				m_indentStack.Add(tag_Indent);
				m_xAdvance = tag_Indent;
				return true;
			case 6971027:
			case 7598483:
				tag_Indent = m_indentStack.Remove();
				return true;
			case -842656867:
			case 1109386397:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				switch (tagUnits)
				{
				case TagUnits.Pixels:
					tag_LineIndent = num3;
					break;
				case TagUnits.FontUnits:
					tag_LineIndent = num3;
					tag_LineIndent *= m_fontScale * m_fontAsset.fontInfo.TabWidth / (float)(int)m_fontAsset.tabSize;
					break;
				case TagUnits.Percentage:
					tag_LineIndent = m_marginWidth * num3 / 100f;
					break;
				}
				m_xAdvance += tag_LineIndent;
				return true;
			case -445537194:
			case 1897386838:
				tag_LineIndent = 0f;
				return true;
			case 1619421:
			case 2246877:
			{
				int valueHashCode4 = m_xmlAttribute[0].valueHashCode;
				m_spriteIndex = -1;
				TMP_SpriteAsset spriteAsset;
				if (m_xmlAttribute[0].valueType == TagType.None || m_xmlAttribute[0].valueType == TagType.NumericalValue)
				{
					if (m_spriteAsset != null)
					{
						m_currentSpriteAsset = m_spriteAsset;
					}
					else if (m_defaultSpriteAsset != null)
					{
						m_currentSpriteAsset = m_defaultSpriteAsset;
					}
					else if (m_defaultSpriteAsset == null)
					{
						if (TMP_Settings.defaultSpriteAsset != null)
						{
							m_defaultSpriteAsset = TMP_Settings.defaultSpriteAsset;
						}
						else
						{
							m_defaultSpriteAsset = Resources.Load<TMP_SpriteAsset>("Sprite Assets/Default Sprite Asset");
						}
						m_currentSpriteAsset = m_defaultSpriteAsset;
					}
					if (m_currentSpriteAsset == null)
					{
						return false;
					}
				}
				else if (MaterialReferenceManager.TryGetSpriteAsset(valueHashCode4, out spriteAsset))
				{
					m_currentSpriteAsset = spriteAsset;
				}
				else
				{
					if (spriteAsset == null)
					{
						spriteAsset = Resources.Load<TMP_SpriteAsset>(TMP_Settings.defaultSpriteAssetPath + new string(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength));
					}
					if (spriteAsset == null)
					{
						return false;
					}
					MaterialReferenceManager.AddSpriteAsset(valueHashCode4, spriteAsset);
					m_currentSpriteAsset = spriteAsset;
				}
				if (m_xmlAttribute[0].valueType == TagType.NumericalValue)
				{
					int num6 = (int)ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
					if (num6 == -9999)
					{
						return false;
					}
					if (num6 > m_currentSpriteAsset.spriteInfoList.Count - 1)
					{
						return false;
					}
					m_spriteIndex = num6;
				}
				m_spriteColor = s_colorWhite;
				m_tintSprite = false;
				for (int k = 0; k < m_xmlAttribute.Length && m_xmlAttribute[k].nameHashCode != 0; k++)
				{
					int nameHashCode3 = m_xmlAttribute[k].nameHashCode;
					int num7 = 0;
					switch (nameHashCode3)
					{
					case 30547:
					case 43347:
						num7 = m_currentSpriteAsset.GetSpriteIndexFromHashcode(m_xmlAttribute[k].valueHashCode);
						if (num7 == -1)
						{
							return false;
						}
						m_spriteIndex = num7;
						break;
					case 205930:
					case 295562:
						num7 = (int)ConvertToFloat(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength);
						if (num7 == -9999)
						{
							return false;
						}
						if (num7 > m_currentSpriteAsset.spriteInfoList.Count - 1)
						{
							return false;
						}
						m_spriteIndex = num7;
						break;
					case 33019:
					case 45819:
						m_tintSprite = (ConvertToFloat(m_htmlTag, m_xmlAttribute[k].valueStartIndex, m_xmlAttribute[k].valueLength) != 0f);
						break;
					case 192323:
					case 281955:
						m_spriteColor = HexCharsToColor(m_htmlTag, m_xmlAttribute[k].valueStartIndex, m_xmlAttribute[k].valueLength);
						break;
					case 26705:
					case 39505:
					{
						int attributeParameters = GetAttributeParameters(m_htmlTag, m_xmlAttribute[k].valueStartIndex, m_xmlAttribute[k].valueLength, ref m_attributeParameterValues);
						if (attributeParameters != 3)
						{
							return false;
						}
						m_spriteIndex = (int)m_attributeParameterValues[0];
						if (m_isParsingText)
						{
							spriteAnimator.DoSpriteAnimation(m_characterCount, m_currentSpriteAsset, m_spriteIndex, (int)m_attributeParameterValues[1], (int)m_attributeParameterValues[2]);
						}
						break;
					}
					default:
						if (nameHashCode3 != 2246877 && nameHashCode3 != 1619421)
						{
							return false;
						}
						break;
					}
				}
				if (m_spriteIndex == -1)
				{
					return false;
				}
				m_currentMaterialIndex = MaterialReference.AddMaterialReference(m_currentSpriteAsset.material, m_currentSpriteAsset, m_materialReferences, m_materialReferenceIndexLookup);
				m_textElementType = TMP_TextElementType.Sprite;
				return true;
			}
			case 514803617:
			case 730022849:
				m_style |= FontStyles.LowerCase;
				m_fontStyleStack.Add(FontStyles.LowerCase);
				return true;
			case -1883544150:
			case -1668324918:
				if (m_fontStyleStack.Remove(FontStyles.LowerCase) == 0)
				{
					m_style &= (FontStyles)(-9);
				}
				return true;
			case 9133802:
			case 13526026:
			case 566686826:
			case 781906058:
				m_style |= FontStyles.UpperCase;
				m_fontStyleStack.Add(FontStyles.UpperCase);
				return true;
			case -1831660941:
			case -1616441709:
			case 47840323:
			case 52232547:
				if (m_fontStyleStack.Remove(FontStyles.UpperCase) == 0)
				{
					m_style &= (FontStyles)(-17);
				}
				return true;
			case 551025096:
			case 766244328:
				m_style |= FontStyles.SmallCaps;
				m_fontStyleStack.Add(FontStyles.SmallCaps);
				return true;
			case -1847322671:
			case -1632103439:
				if (m_fontStyleStack.Remove(FontStyles.SmallCaps) == 0)
				{
					m_style &= (FontStyles)(-33);
				}
				return true;
			case 1482398:
			case 2109854:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				m_marginLeft = num3;
				switch (tagUnits)
				{
				case TagUnits.FontUnits:
					m_marginLeft *= m_fontScale * m_fontAsset.fontInfo.TabWidth / (float)(int)m_fontAsset.tabSize;
					break;
				case TagUnits.Percentage:
					m_marginLeft = (m_marginWidth - ((m_width != -1f) ? m_width : 0f)) * m_marginLeft / 100f;
					break;
				}
				m_marginLeft = ((m_marginLeft >= 0f) ? m_marginLeft : 0f);
				m_marginRight = m_marginLeft;
				return true;
			case 7011901:
			case 7639357:
				m_marginLeft = 0f;
				m_marginRight = 0f;
				return true;
			case -855002522:
			case 1100728678:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				m_marginLeft = num3;
				switch (tagUnits)
				{
				case TagUnits.FontUnits:
					m_marginLeft *= m_fontScale * m_fontAsset.fontInfo.TabWidth / (float)(int)m_fontAsset.tabSize;
					break;
				case TagUnits.Percentage:
					m_marginLeft = (m_marginWidth - ((m_width != -1f) ? m_width : 0f)) * m_marginLeft / 100f;
					break;
				}
				m_marginLeft = ((m_marginLeft >= 0f) ? m_marginLeft : 0f);
				return true;
			case -1690034531:
			case -884817987:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f || num3 == 0f)
				{
					return false;
				}
				m_marginRight = num3;
				switch (tagUnits)
				{
				case TagUnits.FontUnits:
					m_marginRight *= m_fontScale * m_fontAsset.fontInfo.TabWidth / (float)(int)m_fontAsset.tabSize;
					break;
				case TagUnits.Percentage:
					m_marginRight = (m_marginWidth - ((m_width != -1f) ? m_width : 0f)) * m_marginRight / 100f;
					break;
				}
				m_marginRight = ((m_marginRight >= 0f) ? m_marginRight : 0f);
				return true;
			case -842693512:
			case 1109349752:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f)
				{
					return false;
				}
				m_lineHeight = num3;
				switch (tagUnits)
				{
				case TagUnits.FontUnits:
					m_lineHeight *= m_fontAsset.fontInfo.LineHeight * m_fontScale;
					break;
				case TagUnits.Percentage:
					m_lineHeight = m_fontAsset.fontInfo.LineHeight * m_lineHeight / 100f * m_fontScale;
					break;
				}
				return true;
			case -445573839:
			case 1897350193:
				m_lineHeight = -32767f;
				return true;
			case 10723418:
			case 15115642:
				tag_NoParsing = true;
				return true;
			case 1286342:
			case 1913798:
			{
				int valueHashCode = m_xmlAttribute[0].valueHashCode;
				if (m_isParsingText)
				{
					m_actionStack.Add(valueHashCode);
					Debug.Log("Action ID: [" + valueHashCode + "] First character index: " + m_characterCount);
				}
				return true;
			}
			case 6815845:
			case 7443301:
				if (m_isParsingText)
				{
					Debug.Log("Action ID: [" + m_actionStack.CurrentItem() + "] Last character index: " + (m_characterCount - 1));
				}
				m_actionStack.Remove();
				return true;
			case 1600507:
			case 2227963:
				num3 = ConvertToFloat(m_htmlTag, m_xmlAttribute[0].valueStartIndex, m_xmlAttribute[0].valueLength);
				if (num3 == -9999f)
				{
					return false;
				}
				m_FXMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, num3), Vector3.one);
				m_isFXMatrixSet = true;
				return true;
			case 7130010:
			case 7757466:
				m_isFXMatrixSet = false;
				return true;
			case 227814:
			case 317446:
			{
				int nameHashCode = m_xmlAttribute[1].nameHashCode;
				if (nameHashCode == 327550)
				{
					float num5 = ConvertToFloat(m_htmlTag, m_xmlAttribute[1].valueStartIndex, m_xmlAttribute[1].valueLength);
					switch (tagUnits)
					{
					case TagUnits.Pixels:
						Debug.Log("Table width = " + num5 + "px.");
						break;
					case TagUnits.FontUnits:
						Debug.Log("Table width = " + num5 + "em.");
						break;
					case TagUnits.Percentage:
						Debug.Log("Table width = " + num5 + "%.");
						break;
					}
				}
				return true;
			}
			case 1017743:
			case 1107375:
				return true;
			case 670:
			case 926:
				return true;
			case 2973:
			case 3229:
				return true;
			case 660:
			case 916:
				return true;
			case 2963:
			case 3219:
				return true;
			case 656:
			case 912:
			{
				for (int j = 1; j < m_xmlAttribute.Length && m_xmlAttribute[j].nameHashCode != 0; j++)
				{
					switch (m_xmlAttribute[j].nameHashCode)
					{
					case 327550:
					{
						float num4 = ConvertToFloat(m_htmlTag, m_xmlAttribute[j].valueStartIndex, m_xmlAttribute[j].valueLength);
						switch (tagUnits)
						{
						case TagUnits.Pixels:
							Debug.Log("Table width = " + num4 + "px.");
							break;
						case TagUnits.FontUnits:
							Debug.Log("Table width = " + num4 + "em.");
							break;
						case TagUnits.Percentage:
							Debug.Log("Table width = " + num4 + "%.");
							break;
						}
						break;
					}
					case 275917:
						switch (m_xmlAttribute[j].valueHashCode)
						{
						case 3774683:
							Debug.Log("TD align=\"left\".");
							break;
						case 136703040:
							Debug.Log("TD align=\"right\".");
							break;
						case -458210101:
							Debug.Log("TD align=\"center\".");
							break;
						case -523808257:
							Debug.Log("TD align=\"justified\".");
							break;
						}
						break;
					}
				}
				return true;
			}
			case 2959:
			case 3215:
				return true;
			default:
				return false;
			}
		}

		public TMP_Text()
			
		{
		}
	}
}
