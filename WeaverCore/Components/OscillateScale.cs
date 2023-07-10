using System.Collections;
using UnityEngine;

namespace WeaverCore.Components
{
    public class OscillateScale : MonoBehaviour
	{
		[SerializeField]
		Vector3 fromScale;

		[SerializeField]
		Vector3 toScale;

		[SerializeField]
		AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

		[SerializeField]
		float upTime = 1.78333f;

		[SerializeField]
		float downTime = 1.78333f;

        private void Awake()
        {
			StartCoroutine(OscillateRoutine());
        }

		IEnumerator OscillateRoutine()
		{
			while (true)
			{
				for (float t = 0; t < upTime; t += Time.deltaTime)
				{
					transform.localScale = Vector3.Lerp(fromScale, toScale, t / upTime);
					yield return null;
				}

                for (float t = 0; t < downTime; t += Time.deltaTime)
                {
                    transform.localScale = Vector3.Lerp(toScale, fromScale, t / downTime);
                    yield return null;
                }
            }
		}
    }
}
