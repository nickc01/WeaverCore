using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMProOld;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{

    class TMP_Text_Patches
	{
		static Func<TMP_Text, bool> GetPreferredWidthDirty;
		static Func<TMP_Text, bool> GetPreferredHeightDirty;

		static Action<TMP_Text, bool> SetPreferredWidthDirty;
		static Action<TMP_Text, bool> SetPreferredHeightDirty;


		[OnInit]
		static void Init()
		{
			GetPreferredWidthDirty = ReflectionUtilities.CreateFieldGetter<TMP_Text,bool>("m_isPreferredWidthDirty");
			GetPreferredHeightDirty = ReflectionUtilities.CreateFieldGetter<TMP_Text,bool>("m_isPreferredHeightDirty");

			SetPreferredWidthDirty = ReflectionUtilities.CreateFieldSetter<TMP_Text, bool>("m_isPreferredWidthDirty");
			SetPreferredHeightDirty = ReflectionUtilities.CreateFieldSetter<TMP_Text, bool>("m_isPreferredHeightDirty");


			On.TMProOld.TextMeshProUGUI.SetLayoutDirty += TextMeshProUGUI_SetLayoutDirty;
			On.TMProOld.TextMeshPro.SetLayoutDirty += TextMeshPro_SetLayoutDirty;
		}

		private static void TextMeshPro_SetLayoutDirty(On.TMProOld.TextMeshPro.orig_SetLayoutDirty orig, TMProOld.TextMeshPro self)
		{
			SetPreferredWidthDirty(self, true);
			SetPreferredHeightDirty(self, true);
			orig(self);
		}

		private static void TextMeshProUGUI_SetLayoutDirty(On.TMProOld.TextMeshProUGUI.orig_SetLayoutDirty orig, TMProOld.TextMeshProUGUI self)
		{
			SetPreferredWidthDirty(self, true);
			SetPreferredHeightDirty(self, true);
			orig(self);
		}
	}
}
