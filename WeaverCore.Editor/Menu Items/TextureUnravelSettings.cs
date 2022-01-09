using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Editor.Enums;

namespace WeaverCore.Editor
{
	/// <summary>
	/// Stores the settings used for unravelling tk2D texture into multiple sprites
	/// </summary>
	public class TextureUnravelSettings
	{
		public Texture2D texture;

		public string SheetPath;

		public UnravelMode UnravelMode;
	}
}
