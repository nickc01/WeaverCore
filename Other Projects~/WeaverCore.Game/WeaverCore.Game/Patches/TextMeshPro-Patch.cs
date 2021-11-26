using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
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
			On.TMPro.TextContainer.Awake += TextContainer_Awake;
		}

		private static void TextContainer_Awake(On.TMPro.TextContainer.orig_Awake orig, TMPro.TextContainer self)
		{
			var tmp = self.GetComponent<TextMeshPro>();
			var rTransform = self.GetComponent<RectTransform>();
			if (rTransform != null && tmp is WeaverCore.Assets.TMPro.TextMeshPro wtmp)
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
