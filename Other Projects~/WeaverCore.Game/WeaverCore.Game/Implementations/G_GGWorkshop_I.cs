using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Game.Implementations
{
	/*class StatueTextChanger : MonoBehaviour
	{
		public BossStatue statue;
		public string Name;
		public string Description;

		void Update()
		{
			statue.na
		}
	}*/

	struct BossInfo
	{
		public string Name;
		public string Description;
	}

	
	public class G_GGWorkshop_I : GGWorkshop_I
	{
		static Dictionary<BossStatue, BossInfo> StatueChanges = new Dictionary<BossStatue, BossInfo>();

		public override void ChangeStatue(string statueGMName, string bossName = "", string bossDescription = "")
		{
			var gm = GameObject.Find(statueGMName);
			//Debugger.Log("Found GM with = " + statueGMName + " = " + gm);

			BossStatue statue;
			if (gm != null && (statue = gm.GetComponentInChildren<BossStatue>()) != null)
			{
				if (StatueChanges.Count == 0)
				{
					On.BossChallengeUI.Setup += BossChallengeUI_Setup;
				}

				StatueChanges.Add(statue, new BossInfo()
				{
					Name = bossName,
					Description = bossDescription
				});
				/*var textChanger = gm.GetComponent<StatueTextChanger>();
				if (textChanger == null)
				{
					textChanger = gm.AddComponent<StatueTextChanger>();
				}
				textChanger.statue = statue;
				textChanger.name = bossName;
				textChanger.Description = bossDescription;*/
			}
		}

		private void BossChallengeUI_Setup(On.BossChallengeUI.orig_Setup orig, BossChallengeUI self, BossStatue bossStatue, string bossNameSheet, string bossNameKey, string descriptionSheet, string descriptionKey)
		{
			orig(self, bossStatue, bossNameSheet, bossNameKey, descriptionSheet, descriptionKey);
			if (StatueChanges.ContainsKey(bossStatue))
			{
				var changes = StatueChanges[bossStatue];
				self.bossNameText.text = changes.Name == "" ? self.bossNameText.text : changes.Name;
				self.descriptionText.text = changes.Description == "" ? self.descriptionText.text : changes.Description;
			}
		}
	}
}
