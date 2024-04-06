using System.Collections;
using UnityEditor;
using UnityEngine;

namespace WeaverCore.Elevator
{
    /// <summary>
    /// When put on a child object, and the parent object has an <see cref="Elevator"/> component, the object will bob up and down when the player lands on the elevator
    /// </summary>
    public class ElevatorBobable : MonoBehaviour
    {
        [SerializeField]
        float bobDownTime = 0.15f;

        [SerializeField]
        float bobUpTime = 0.15f;

        [SerializeField]
        float bobIntensity = 0.06f;

        [SerializeField]
        AnimationCurve bobDownCurve;

        [SerializeField]
        AnimationCurve bobUpCurve;

#if UNITY_EDITOR
        private void Reset()
        {
            bobDownCurve = Newtonsoft.Json.JsonConvert.DeserializeObject<AnimationCurve>(@"{""keys"":[{""time"":0.0,""value"":0.0,""inTangent"":0.0,""outTangent"":0.0,""inWeight"":0.333333343,""outWeight"":0.333333343,""weightedMode"":0,""tangentMode"":0},{""time"":1.0,""value"":1.0,""inTangent"":2.0,""outTangent"":2.0,""inWeight"":0.333333343,""outWeight"":0.333333343,""weightedMode"":0,""tangentMode"":0}],""length"":2,""preWrapMode"":8,""postWrapMode"":8}");

            bobUpCurve = Newtonsoft.Json.JsonConvert.DeserializeObject<AnimationCurve>(@"{""keys"":[{""time"":0.0,""value"":0.0,""inTangent"":2.0,""outTangent"":2.0,""inWeight"":0.333333343,""outWeight"":0.333333343,""weightedMode"":0,""tangentMode"":0},{""time"":1.0,""value"":1.0,""inTangent"":0.0,""outTangent"":0.0,""inWeight"":0.333333343,""outWeight"":0.333333343,""weightedMode"":0,""tangentMode"":0}],""length"":2,""preWrapMode"":8,""postWrapMode"":8}");
        }
#endif

        /// <summary>
        /// Used to bob the elevator
        /// </summary>
        /// <param name="elevator">The source elevator</param>
        /// <param name="direction">The direction the elevator should bob in</param>
        public virtual IEnumerator OnBob(Elevator elevator, Vector3 direction)
        {
            var start = transform.localPosition;

            //var end = transform.InverseTransformPoint(transform.position + (direction * bobIntensity));
            var end = transform.localPosition + (direction * bobIntensity);

            for (float t = 0; t < bobDownTime; t += Time.deltaTime)
            {
                transform.localPosition = Vector3.Lerp(start, end, bobDownCurve.Evaluate(t / bobDownTime));
                yield return null;
            }

            for (float t = 0; t < bobUpTime; t += Time.deltaTime)
            {
                transform.localPosition = Vector3.Lerp(end, start, bobUpCurve.Evaluate(t / bobUpTime));
                yield return null;
            }

            transform.localPosition = start;
        }
    }
}