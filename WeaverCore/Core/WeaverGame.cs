using GlobalEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WeaverCore
{


	public class WeaverGame : MonoBehaviour
	{
		//static WeaverGame GameManager;
		//static GameManager_I impl;

		public enum TimeFreezePreset
		{
			Preset0,
			Preset1,
			Preset2,
			Preset3,
			Preset4,
			Preset5
		}


		/*static void GameManagerInit()
		{
			if (GameManager == null)
			{
				GameManager = new GameObject("__WEAVER_GAME_MANAGER__").AddComponent<WeaverGame>();

				GameObject.DontDestroyOnLoad(GameManager);

				impl = (GameManager_I)GameManager.gameObject.AddComponent(ImplFinder.GetImplementationType<GameManager_I>());
			}
		}*/

		public static void FreezeGameTime(TimeFreezePreset preset)
		{
			//GameManagerInit();
			switch (preset)
			{
				case TimeFreezePreset.Preset0:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.01f, 0.35f, 0.1f, 0f));
					break;
				case TimeFreezePreset.Preset1:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.04f, 0.03f, 0.04f, 0f));
					break;
				case TimeFreezePreset.Preset2:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.25f, 2f, 0.25f, 0.15f));
					break;
				case TimeFreezePreset.Preset3:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
					break;
				case TimeFreezePreset.Preset4:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
					break;
				case TimeFreezePreset.Preset5:
					GameManager.instance.StartCoroutine(GameManager.instance.FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
					break;
			}
		}

		public static MapZone CurrentMapZone
		{
			get
			{
				return GameManager.instance.sm.mapZone;
			}
		}

		//public static IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
		//{
			//GameManagerInit();
			//return impl.FreezeMoment(rampDownTime, waitTime, rampUpTime, targetSpeed);
			/*this.timeSlowedCount++;
			yield return this.StartCoroutine(this.SetTimeScale(targetSpeed, rampDownTime));
			for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime)
			{
				yield return null;
			}
			yield return this.StartCoroutine(this.SetTimeScale(1f, rampUpTime));
			this.timeSlowedCount--;
			yield break;*/
		//}

		/*private static int timeSlowedCount;

		static WeaverGameManager GameManager;


		public static void FreezeGame()
		{

		}

		static void GameManagerInit()
		{
			if (GameManager == null)
			{
				GameManager = new GameObject("__WEAVER_GAME_MANAGER__").AddComponent<WeaverGameManager>();

				GameObject.DontDestroyOnLoad(GameManager);
			}
		}

		public static IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
		{
			timeSlowedCount++;
			yield return GameManager.StartCoroutine(SetTimeScale(targetSpeed, rampDownTime));
			for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime)
			{
				yield return null;
			}
			yield return GameManager.StartCoroutine(SetTimeScale(1f, rampUpTime));
			timeSlowedCount--;
			yield break;
		}

		private static IEnumerator SetTimeScale(float newTimeScale, float duration)
		{
			float lastTimeScale = 1;
			for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
			{
				float val = Mathf.Clamp01(timer / duration);
				SetTimeScale(Mathf.Lerp(lastTimeScale, newTimeScale, val));
				yield return null;
			}
			SetTimeScale(newTimeScale);
			yield break;
		}

		// Token: 0x06002CA4 RID: 11428 RVA: 0x0001FF69 File Offset: 0x0001E169
		private static void SetTimeScale(float newTimeScale)
		{
			if (timeSlowedCount > 1)
			{
				newTimeScale = Mathf.Min(newTimeScale, 1f);
			}
			TimeController.GenericTimeScale = ((newTimeScale <= 0.01f) ? 0f : newTimeScale);
		}*/
	}
}
