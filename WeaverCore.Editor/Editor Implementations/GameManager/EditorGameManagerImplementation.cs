using System.Collections;
using UnityEngine;
using WeaverCore.Implementations.GameManager;

namespace WeaverCore.Editor.Implementations
{
	public class EditorGameManagerImplementation : GameManagerImplementation
	{
		private int timeSlowedCount;

		public override IEnumerator FreezeMoment(float rampDownTime, float waitTime, float rampUpTime, float targetSpeed)
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

		// Token: 0x06002CA4 RID: 11428 RVA: 0x0001FF69 File Offset: 0x0001E169
		private void SetTimeScale(float newTimeScale)
		{
			if (timeSlowedCount > 1)
			{
				newTimeScale = Mathf.Min(newTimeScale, Time.timeScale);
			}
			Time.timeScale = ((newTimeScale <= 0.01f) ? 0f : newTimeScale);
			//TimeController.GenericTimeScale = 
		}
	}
}
