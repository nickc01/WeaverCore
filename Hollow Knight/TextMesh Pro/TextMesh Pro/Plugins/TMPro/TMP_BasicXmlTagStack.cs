namespace TMPro
{
	public struct TMP_BasicXmlTagStack
	{
		public byte bold;

		public byte italic;

		public byte underline;

		public byte strikethrough;

		public byte highlight;

		public byte superscript;

		public byte subscript;

		public byte uppercase;

		public byte lowercase;

		public byte smallcaps;

		public void Clear()
		{
			bold = 0;
			italic = 0;
			underline = 0;
			strikethrough = 0;
			highlight = 0;
			superscript = 0;
			subscript = 0;
			uppercase = 0;
			lowercase = 0;
			smallcaps = 0;
		}

		public byte Add(FontStyles style)
		{
			switch (style)
			{
			case FontStyles.Bold:
				bold++;
				return bold;
			case FontStyles.Italic:
				italic++;
				return italic;
			case FontStyles.Underline:
				underline++;
				return underline;
			case FontStyles.Strikethrough:
				strikethrough++;
				return strikethrough;
			case FontStyles.Superscript:
				superscript++;
				return superscript;
			case FontStyles.Subscript:
				subscript++;
				return subscript;
			case FontStyles.Highlight:
				highlight++;
				return highlight;
			default:
				return 0;
			}
		}

		public byte Remove(FontStyles style)
		{
			switch (style)
			{
			case FontStyles.Bold:
				if (bold > 1)
				{
					bold--;
				}
				else
				{
					bold = 0;
				}
				return bold;
			case FontStyles.Italic:
				if (italic > 1)
				{
					italic--;
				}
				else
				{
					italic = 0;
				}
				return italic;
			case FontStyles.Underline:
				if (underline > 1)
				{
					underline--;
				}
				else
				{
					underline = 0;
				}
				return underline;
			case FontStyles.Strikethrough:
				if (strikethrough > 1)
				{
					strikethrough--;
				}
				else
				{
					strikethrough = 0;
				}
				return strikethrough;
			case FontStyles.Highlight:
				if (highlight > 1)
				{
					highlight--;
				}
				else
				{
					highlight = 0;
				}
				return highlight;
			case FontStyles.Superscript:
				if (superscript > 1)
				{
					superscript--;
				}
				else
				{
					superscript = 0;
				}
				return superscript;
			case FontStyles.Subscript:
				if (subscript > 1)
				{
					subscript--;
				}
				else
				{
					subscript = 0;
				}
				return subscript;
			default:
				return 0;
			}
		}
	}
}
