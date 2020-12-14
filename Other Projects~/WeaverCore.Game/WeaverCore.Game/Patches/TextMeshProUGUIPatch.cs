using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using UnityEngine;
using WeaverCore.Attributes;

namespace WeaverCore.Game.Patches
{
	static class TextMeshProUGUIPatch
	{
		static HashSet<TextMeshProUGUI> TMProObjects = new HashSet<TextMeshProUGUI>();

		static MethodInfo PreferredWidthF;
		static MethodInfo PreferredHeightF;

		[OnInit]
		static void Init()
		{
			//WeaverLog.Log("TEST TM INIT");
			//On.TMPro.TextMeshProUGUI.Awake += TextMeshProUGUI_Awake;
			//On.TMPro.TextMeshProUGUI.OnEnable += TextMeshProUGUI_OnEnable;
			//On.TMPro.TextMeshProUGUI.OnDisable += TextMeshProUGUI_OnDisable;
			//On.TMPro.TextMeshProUGUI.OnDestroy += TextMeshProUGUI_OnDestroy;

			//Camera.onPreRender += PreRender;
			//Camera.onPostRender += PostRender;


			//PreferredWidthF = typeof(TextMeshProUGUI).GetMethod("GetRenderedWidth", BindingFlags.Instance | BindingFlags.NonPublic,null, new Type[] { },null);
			//PreferredHeightF = typeof(TextMeshProUGUI).GetMethod("GetRenderedHeight", BindingFlags.Instance | BindingFlags.NonPublic,null, new Type[] { },null);
			//WeaverLog.Log("Preferred Width Function = " + PreferredWidthF);
		}

		/*void Awake()
		{
			WeaverLog.Log("Patch Awake!!!");
		}

		void Start()
		{
			WeaverLog.Log("Patch Started!!!");
		}

		void OnDisable()
		{
			WeaverLog.Log("Patch On Disable!!!");
		}

		void OnEnable()
		{
			WeaverLog.Log("Patch On Enable!!!");
		}

		void OnDestroy()
		{
			WeaverLog.Log("Patch On Destroy!!!");
		}*/

		static bool first = false;

		static void PreRender(Camera cam)
		{
			if (!first)
			{
				first = true;
				WeaverLog.Log("Camera = " + cam);
				foreach (var obj in TMProObjects)
				{
					WeaverLog.Log("Obj = " + obj.name);
					var oldPosition = obj.rectTransform.anchoredPosition;
					WeaverLog.Log("Old Position = " + oldPosition);
					WeaverLog.Log("Preferred Width = " + PreferredWidthF.Invoke(obj,null));
					WeaverLog.Log("Preferred Height = " + PreferredHeightF.Invoke(obj,null));

					var preferredWidth = (float)PreferredWidthF.Invoke(obj, null);
					var preferredHeight = (float)PreferredHeightF.Invoke(obj, null);

					//obj.rectTransform.anchoredPosition = new Vector2(oldPosition.x,oldPosition.y + preferredHeight);
					//obj.rectTransform.anchoredPosition = new Vector3(oldPosition.x - (preferredWidth / 2f), oldPosition.y - (preferredHeight / 2f) + obj.font.fontInfo.Padding);
					WeaverLog.Log("New Position = " + obj.rectTransform.anchoredPosition);

					
				}
			}
		}

		static void PostRender(Camera cam)
		{
			if (first)
			{
				first = false;
				WeaverLog.Log("Post Render");
				foreach (var obj in TMProObjects)
				{
					var oldPosition = obj.rectTransform.anchoredPosition;
					var preferredWidth = (float)PreferredWidthF.Invoke(obj, null);
					var preferredHeight = (float)PreferredHeightF.Invoke(obj, null);
					//obj.rectTransform.anchoredPosition = new Vector2(oldPosition.x + (preferredWidth / 2f), oldPosition.y + (preferredHeight / 2f) - obj.font.fontInfo.Padding);
				}
			}
		}

		private static void TextMeshProUGUI_Awake(On.TMPro.TextMeshProUGUI.orig_Awake orig, TMPro.TextMeshProUGUI self)
		{
			orig(self);
			if (self.gameObject.activeSelf)
			{
				TMProObjects.Add(self);
				self.SetAllDirty();

				var oldPosition = self.rectTransform.anchoredPosition;

				var preferredWidth = (float)PreferredWidthF.Invoke(self, null);
				var renderedHeight = (float)PreferredHeightF.Invoke(self, null);

				self.rectTransform.anchoredPosition = new Vector2(oldPosition.x, oldPosition.y + renderedHeight);

				//self.rectTransform.anchoredPosition = new Vector3(oldPosition.x - (preferredWidth / 2f), oldPosition.y - (preferredHeight / 2f) + self.font.fontInfo.Padding);
			}
		}

		private static void TextMeshProUGUI_OnEnable(On.TMPro.TextMeshProUGUI.orig_OnEnable orig, TextMeshProUGUI self)
		{
			WeaverLog.Log("Adding TMPRO");
			TMProObjects.Add(self);
			self.SetAllDirty();
			orig(self);
		}

		private static void TextMeshProUGUI_OnDisable(On.TMPro.TextMeshProUGUI.orig_OnDisable orig, TextMeshProUGUI self)
		{
			WeaverLog.Log("Removing TMPRO");
			TMProObjects.Remove(self);
			orig(self);
		}

		private static void TextMeshProUGUI_OnDestroy(On.TMPro.TextMeshProUGUI.orig_OnDestroy orig, TextMeshProUGUI self)
		{
			WeaverLog.Log("Removing TMPRO");
			TMProObjects.Remove(self);
			orig(self);
		}

		/*void OnPreRender()
		{
			Debug.Log("Objects = " + TMProObjects.Count);
			foreach (var obj in TMProObjects)
			{
				var oldPosition = obj.transform.localPosition;
				obj.transform.localPosition = new Vector3(oldPosition.x - (obj.preferredWidth / 2f),oldPosition.y - (obj.preferredHeight / 2f) + obj.font.fontInfo.Padding);
			}
		}

		void OnPostRender()
		{
			foreach (var obj in TMProObjects)
			{
				var oldPosition = obj.transform.localPosition;
				obj.transform.localPosition = new Vector3(oldPosition.x + (obj.preferredWidth / 2f), oldPosition.y + (obj.preferredHeight / 2f) - obj.font.fontInfo.Padding);
			}
		}*/

	}



}
