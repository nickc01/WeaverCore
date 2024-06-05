using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class WeaverCinematicPlayer : CinematicPlayer
	{
		static Func<CinematicPlayer, bool> videoTriggeredGetter;


		[SerializeField]
		[Tooltip("If true, the layer of this gameobject will be auto changed depending on the scene it's being used in")]
		bool autoUpdateLayers = true;

		[SerializeField]
		[Tooltip("If true, then an audio snapshot will be applied when the video starts playing (This is normally not used)")]
		bool applySnapshotOnPlay = false;

		[SerializeField]
		[Tooltip("Applies this snapshot when the cinematic plays. Only does it if \"Apply Snapshot On Play\" is true")]
		Music.SnapshotType snapshotOnPlay = Music.SnapshotType.Silent;

		[SerializeField]
		[Tooltip("If true, then an audio snapshot will be applied when the video is done playing")]
		bool applySnapshotOnDone = false;

		[SerializeField]
		[Tooltip("Applies this snapshot when the cinematic finishes. Only does it if \"Apply Snapshot On Done\" is true")]
		Music.SnapshotType snapshotOnDone = Music.SnapshotType.Silent;

		public bool VideoPlaying => videoTriggeredGetter(this);



		[OnHarmonyPatch]
		static void OnHarmonyPatch(HarmonyPatcher patcher)
		{
			//Awake()
			{
				var orig = typeof(CinematicPlayer).GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
				var prefix = typeof(WeaverCinematicPlayer).GetMethod(nameof(AwakePrefix), BindingFlags.NonPublic | BindingFlags.Static);

				patcher.Patch(orig, prefix, null);
			}

			videoTriggeredGetter = ReflectionUtilities.CreateFieldGetter<CinematicPlayer, bool>("videoTriggered");
		}

		static bool AwakePrefix(CinematicPlayer __instance)
		{
			if (__instance is WeaverCinematicPlayer wcp)
			{
				if (wcp.autoUpdateLayers)
				{
					var sceneManager = GameManager.instance.sm;

					if (sceneManager == null)
					{
						sceneManager = GameObject.FindObjectOfType<SceneManager>();
					}

					if (sceneManager != null && sceneManager.sceneType != GlobalEnums.SceneType.CUTSCENE)
					{
						var uiLayer = LayerMask.NameToLayer("UI");
						wcp.gameObject.layer = uiLayer;
						for (int i = 0; i < wcp.transform.childCount; i++)
						{
							wcp.transform.GetChild(i).gameObject.layer = uiLayer;
						}
					}
				}

				if (wcp.applySnapshotOnPlay)
				{
					typeof(CinematicPlayer).GetField("masterOff", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(wcp, Music.GetSnapshot(wcp.snapshotOnPlay));
				}

				if (wcp.applySnapshotOnDone)
				{
					typeof(CinematicPlayer).GetField("masterResume", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(wcp, Music.GetSnapshot(wcp.snapshotOnDone));
				}
			}

			return true;
		}

		public IEnumerator PlayVideo()
		{
			TriggerStartVideo();
			yield return new WaitUntil(() => !VideoPlaying);
		}
	}

}