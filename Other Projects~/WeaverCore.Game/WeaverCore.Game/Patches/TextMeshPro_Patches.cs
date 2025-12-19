using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMProOld;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Patches
{
	static class TextMeshPro_Patch
	{
		[OnInit]
		static void Init()
		{
			On.TMProOld.TextContainer.Awake += TextContainer_Awake;
		}

		private static void TextContainer_Awake(On.TMProOld.TextContainer.orig_Awake orig, TMProOld.TextContainer self)
		{
			var tmp = self.GetComponent<TextMeshPro>();
			var rTransform = self.GetComponent<RectTransform>();
			if (rTransform != null && tmp is WeaverCore.Assets.TMProOld.TextMeshPro wtmp)
			{
				var oldPivot = self.pivot;
				var oldRect = self.rect;
				var oldSizeDelta = self.rectTransform.sizeDelta;
				var oldMargins = self.margins;
				var oldAnchorPosition = self.anchorPosition;

				orig(self);

				if (self.isDefaultWidth || self.isDefaultHeight)
				{
					self.pivot = oldPivot;
					self.rect = oldRect;
					rTransform.sizeDelta = oldSizeDelta;
					self.margins = oldMargins;
					self.anchorPosition = oldAnchorPosition;
					ReflectionUtilities.GetMethod<TextContainer>("UpdateCorners").Invoke(self, null);
					typeof(TextContainer).GetField("m_isDefaultWidth", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(self, false);
					typeof(TextContainer).GetField("m_isDefaultHeight", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(self, false);
				}
			}
			else
			{
				orig(self);
			}
		}
	}
}
