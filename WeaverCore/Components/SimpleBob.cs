using System;
using UnityEngine;

namespace WeaverCore.Components
{
    public class SimpleBob : MonoBehaviour
    {
        [SerializeField]
        Vector3 bobAmount = new Vector3(0f, 0.28f, 0f);

        [SerializeField]
        float bobTime = 2f;

        [SerializeField]
        AnimationCurve curve;

        [SerializeField]
        bool loop = true;

        [NonSerialized]
        float _timer = 0f;

        [NonSerialized]
        Vector3 originalStartPos;

        void Awake()
        {
            originalStartPos = transform.position;
        }

        void Reset()
        {
            curve = Newtonsoft.Json.JsonConvert.DeserializeObject<AnimationCurve>(@"{""keys"":[{""time"":0.0,""value"":0.0,""inTangent"":-0.00326524372,""outTangent"":-0.00326524372,""inWeight"":0.0,""outWeight"":1.0,""weightedMode"":0,""tangentMode"":0},{""time"":0.5,""value"":1.0,""inTangent"":0.014890735,""outTangent"":0.014890735,""inWeight"":0.333333343,""outWeight"":0.333333343,""weightedMode"":0,""tangentMode"":0},{""time"":1.0,""value"":0.0,""inTangent"":-0.00163539872,""outTangent"":-0.00163539872,""inWeight"":1.0,""outWeight"":0.0,""weightedMode"":0,""tangentMode"":0}],""length"":3,""preWrapMode"":8,""postWrapMode"":8}");
        }

        void Update()
        {
            if (!float.IsNaN(_timer))
            {
                var oldDest = Vector3.Lerp(originalStartPos, originalStartPos + bobAmount, curve.Evaluate(_timer / bobTime));
                _timer += Time.deltaTime;

                if (_timer >= bobTime)
                {
                    if (!loop)
                    {
                        _timer = float.NaN;
                        return;
                    }
                    _timer -= bobTime;
                }

                var newDest = Vector3.Lerp(originalStartPos, originalStartPos + bobAmount, curve.Evaluate(_timer / bobTime));

                transform.position += (newDest - oldDest);
            }
        }
    }

}