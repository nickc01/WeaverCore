using GlobalEnums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	static GameManager dummyInstance;

	private int timeSlowedCount;


	static SceneManager dummySceneManager;
	public global::SceneManager sm
	{
		get
		{
			if (dummySceneManager == null)
			{
				dummySceneManager = GameManager.instance.gameObject.AddComponent<SceneManager>();
			}
			return dummySceneManager;
		}
		private set
		{
			dummySceneManager = value;
		}
	}


	public static GameManager instance
	{
		get
		{
			if (dummyInstance == null)
			{
				dummyInstance = new GameObject("GAME_MANAGER").AddComponent<GameManager>();
				DontDestroyOnLoad(dummyInstance.gameObject);
			}
			return dummyInstance;
		}
	}

	public IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
	{
		timeSlowedCount++;
		yield return StartCoroutine(SetTimeScale(targetSpeed, rampDownTime));
		for (float timer = 0f; timer < waitTime; timer += Time.unscaledDeltaTime)
		{
			yield return null;
		}
		yield return StartCoroutine(SetTimeScale(1f, rampUpTime));
		timeSlowedCount--;
		yield break;
	}

	private IEnumerator SetTimeScale(float newTimeScale, float duration)
	{
		float lastTimeScale = Time.timeScale;
		for (float timer = 0f; timer < duration; timer += Time.unscaledDeltaTime)
		{
			float val = Mathf.Clamp01(timer / duration);
			SetTimeScale(Mathf.Lerp(lastTimeScale, newTimeScale, val));
			yield return null;
		}
		SetTimeScale(newTimeScale);
		yield break;
	}

	private void SetTimeScale(float newTimeScale)
	{
		if (timeSlowedCount > 1)
		{
			newTimeScale = Mathf.Min(newTimeScale, Time.timeScale);
		}
		Time.timeScale = ((newTimeScale <= 0.01f) ? 0f : newTimeScale);
	}

	public void AwardAchievement(string key)
	{

	}

	public void BeginSceneTransition(GameManager.SceneLoadInfo info)
	{

	}

	public class SceneLoadInfo
	{
		public virtual void NotifyFetchComplete()
		{
		}

		public virtual bool IsReadyToActivate()
		{
			return true;
		}

		public virtual void NotifyFinished()
		{
		}

		public bool IsFirstLevelForPlayer;
		public string SceneName;
		public GatePosition? HeroLeaveDirection;
		public string EntryGateName;
		public float EntryDelay;
		public bool PreventCameraFadeOut;
		public bool WaitForSceneTransitionCameraFade;
		public GameManager.SceneLoadVisualizations Visualization;
		public bool AlwaysUnloadUnusedAssets;
		public bool forceWaitFetch;
	}

	public enum SceneLoadVisualizations
	{
		// Token: 0x04002FB9 RID: 12217
		Default,
		// Token: 0x04002FBA RID: 12218
		Custom = -1,
		// Token: 0x04002FBB RID: 12219
		Dream = 1,
		// Token: 0x04002FBC RID: 12220
		Colosseum,
		// Token: 0x04002FBD RID: 12221
		GrimmDream,
		// Token: 0x04002FBE RID: 12222
		ContinueFromSave,
		// Token: 0x04002FBF RID: 12223
		GodsAndGlory
	}
}
