using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WeaverCore.Implementations.GameManager;

namespace WeaverCore.Helpers
{


	public class WeaverGameManager : MonoBehaviour
	{
		static WeaverGameManager GameManager;
		static GameManagerImplementation impl;

		public enum TImeFreezePreset
		{
			Preset1,
			Preset2,
			Preset3,
			Preset4,
			Preset5,
			Preset6
		}


		static void GameManagerInit()
		{
			if (GameManager == null)
			{
				GameManager = new GameObject("__WEAVER_GAME_MANAGER__").AddComponent<WeaverGameManager>();

				GameObject.DontDestroyOnLoad(GameManager);

				impl = (GameManagerImplementation)GameManager.gameObject.AddComponent(ImplFinder.GetImplementationType<GameManagerImplementation>());
			}
		}

		public static void FreezeGameTime(TImeFreezePreset preset)
		{
			GameManagerInit();
			switch (preset)
			{
				case TImeFreezePreset.Preset1:
					GameManager.StartCoroutine(FreezeMoment(0.01f, 0.35f, 0.1f, 0f));
					break;
				case TImeFreezePreset.Preset2:
					GameManager.StartCoroutine(FreezeMoment(0.04f, 0.03f, 0.04f, 0f));
					break;
				case TImeFreezePreset.Preset3:
					GameManager.StartCoroutine(FreezeMoment(0.25f, 2f, 0.25f, 0.15f));
					break;
				case TImeFreezePreset.Preset4:
					GameManager.StartCoroutine(FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
					break;
				case TImeFreezePreset.Preset5:
					GameManager.StartCoroutine(FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
					break;
				case TImeFreezePreset.Preset6:
					GameManager.StartCoroutine(FreezeMoment(0.01f, 0.25f, 0.1f, 0f));
					break;
			}
		}

		public static IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
		{
			GameManagerInit();
			return impl.FreezeMoment(rampDownTime, waitTime, rampUpTime, targetSpeed);
			/*this.timeSlowedCount++;
			yield return this.StartCoroutine(this.SetTimeScale(targetSpeed, rampDownTime));
			for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime)
			{
				yield return null;
			}
			yield return this.StartCoroutine(this.SetTimeScale(1f, rampUpTime));
			this.timeSlowedCount--;
			yield break;*/
		}

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
