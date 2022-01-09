//using Language;
using TMPro;
using UnityEngine;

public class ChangeFontByLanguage : MonoBehaviour
{

	public enum FontScaleLangTypes
	{
		None,
		AreaName,
		SubAreaName,
		WideMap,
		CreditsTitle,
		ExcerptAuthor
	}

	private class FontScaleLang
	{
		public float? fontSizeJA;

		public float? fontSizeRU;

		public float? fontSizeZH;

		public float? fontSizeKO;

		public float GetFontScale(string lang, float defaultScale)
		{
			switch (lang)
			{
				case "JA":
					if (!fontSizeJA.HasValue)
					{
						return defaultScale;
					}
					return fontSizeJA.Value;
				case "RU":
					if (!fontSizeRU.HasValue)
					{
						return defaultScale;
					}
					return fontSizeRU.Value;
				case "ZH":
					if (!fontSizeZH.HasValue)
					{
						return defaultScale;
					}
					return fontSizeZH.Value;
				case "KO":
					if (!fontSizeKO.HasValue)
					{
						return defaultScale;
					}
					return fontSizeKO.Value;
				default:
					return defaultScale;
			}
		}
	}

	public TMP_FontAsset defaultFont;

	public TMP_FontAsset fontJA;

	public TMP_FontAsset fontRU;

	public TMP_FontAsset fontZH;

	public TMP_FontAsset fontKO;

	public bool onlyOnStart;

	private TextMeshPro tmpro;

	private float startFontSize;

	public FontScaleLangTypes fontScaleLangType;

	private FontScaleLang fontScaleAreaName = new FontScaleLang
	{
		fontSizeJA = 4.5f,
		fontSizeRU = 2.8f,
		fontSizeZH = 4.5f,
		fontSizeKO = 4.5f
	};

	private FontScaleLang fontScaleSubAreaName = new FontScaleLang
	{
		fontSizeJA = null,
		fontSizeRU = 3.9f,
		fontSizeZH = null,
		fontSizeKO = null
	};

	private FontScaleLang fontScaleWideMap = new FontScaleLang
	{
		fontSizeJA = 2.5f,
		fontSizeRU = null,
		fontSizeZH = 3.14f,
		fontSizeKO = 2.89f
	};

	private FontScaleLang fontScaleCreditsTitle = new FontScaleLang
	{
		fontSizeJA = null,
		fontSizeRU = 5.5f,
		fontSizeZH = null,
		fontSizeKO = null
	};

	private FontScaleLang fontScaleExcerptAuthor = new FontScaleLang
	{
		fontSizeJA = 4.5f,
		fontSizeRU = 4.5f,
		fontSizeZH = 4.5f,
		fontSizeKO = 4.5f
	};

	private void Awake()
	{
		tmpro = GetComponent<TextMeshPro>();
		if ((bool)tmpro)
		{
			if (defaultFont == null)
			{
				defaultFont = tmpro.font;
			}
			startFontSize = tmpro.fontSize;
		}
	}

	private void Start()
	{
		SetFont();
	}

	private void OnEnable()
	{
		if (!onlyOnStart)
		{
			SetFont();
		}
	}

	public void SetFont()
	{
		if (tmpro == null)
		{
			return;
		}
		tmpro.fontSize = startFontSize;
		if (defaultFont != null)
		{
			tmpro.font = defaultFont;
		}
	}

	private float GetFontScale(string lang)
	{
		return fontScaleLangType switch
		{
			FontScaleLangTypes.AreaName => fontScaleAreaName.GetFontScale(lang, startFontSize),
			FontScaleLangTypes.SubAreaName => fontScaleSubAreaName.GetFontScale(lang, startFontSize),
			FontScaleLangTypes.WideMap => fontScaleWideMap.GetFontScale(lang, startFontSize),
			FontScaleLangTypes.CreditsTitle => fontScaleCreditsTitle.GetFontScale(lang, startFontSize),
			FontScaleLangTypes.ExcerptAuthor => fontScaleExcerptAuthor.GetFontScale(lang, startFontSize),
			_ => startFontSize,
		};
	}
}
