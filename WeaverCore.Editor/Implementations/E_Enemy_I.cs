using System.Collections;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_Enemy_I : Enemy_I
    {
        public class E_Statics : Enemy_I.Statics
        {
            public override IEnumerator Roar(GameObject source, float duration, AudioClip roarSound)
            {
                if (roarSound != null)
                {
                    WeaverAudio.PlayAtPoint(roarSound, source.transform.position);
                }

                var emitter = RoarEmitter.Spawn(source.transform.position);

                emitter.stopAfterTime = duration;

                yield return new WaitForSeconds(duration);
            }
        }
    }
}
