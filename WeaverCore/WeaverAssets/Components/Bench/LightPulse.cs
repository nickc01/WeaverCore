using System.Collections;
using UnityEngine;


namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// Used to creating a pulsating light behind the bench
	/// </summary>
	public class LightPulse : MonoBehaviour
	{
		[SerializeField]
		AnimationCurve curve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		[SerializeField]
		[Tooltip("How much time to complete a single cycle")]
		float time = 2f;
		[SerializeField]
		Vector3 multiplier = new Vector3(1.8f,1.8f,1.8f);

		private void Start()
		{
			StartCoroutine(ScaleRoutine());
		}

		IEnumerator ScaleRoutine()
		{
			while (true)
			{
				Vector3 oldScale = transform.localScale;
				Vector3 newScale = new Vector3(oldScale.x * multiplier.x, oldScale.y * multiplier.y, oldScale.z * multiplier.z);
				for (float i = 0f; i < time; i += Time.deltaTime)
				{
					transform.localScale = Vector3.Lerp(oldScale,newScale,curve.Evaluate(i / time));
					yield return null;
				}

				for (float i = 0f; i < time; i += Time.deltaTime)
				{
					transform.localScale = Vector3.Lerp(newScale, oldScale, curve.Evaluate(i / time));
					yield return null;
				}

				transform.localScale = oldScale;
			}
		}
	}
}
