using System.Collections;
using UnityEngine;

namespace WeaverCore.Components
{
    /// <summary>
    /// Component to oscillate the scale of a GameObject back and forth.
    /// </summary>
    public class OscillateScale : MonoBehaviour
    {
        /// <summary>
        /// The initial scale of the GameObject.
        /// </summary>
		[SerializeField]
        Vector3 fromScale;

        /// <summary>
        /// The target scale of the GameObject.
        /// </summary>
		[SerializeField]
        Vector3 toScale;

        /// <summary>
        /// The animation curve to control the scaling behavior over time.
        /// </summary>
		[SerializeField]
        [Tooltip("Animation curve for controlling scaling behavior over time.")]
        AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

        /// <summary>
        /// The duration for scaling up.
        /// </summary>
		[SerializeField]
        [Tooltip("Duration for scaling up.")]
        float upTime = 1.78333f;

        /// <summary>
        /// The duration for scaling down.
        /// </summary>
		[SerializeField]
        [Tooltip("Duration for scaling down.")]
        float downTime = 1.78333f;

        /// <summary>
        /// Called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            StartCoroutine(OscillateRoutine());
        }

        /// <summary>
        /// Coroutine to handle the oscillation of the scale.
        /// </summary>
		IEnumerator OscillateRoutine()
        {
            while (true)
            {
                for (float t = 0; t < upTime; t += Time.deltaTime)
                {
                    transform.localScale = Vector3.Lerp(fromScale, toScale, curve.Evaluate(t / upTime));
                    yield return null;
                }

                for (float t = 0; t < downTime; t += Time.deltaTime)
                {
                    transform.localScale = Vector3.Lerp(toScale, fromScale, curve.Evaluate(t / downTime));
                    yield return null;
                }
            }
        }
    }
}
