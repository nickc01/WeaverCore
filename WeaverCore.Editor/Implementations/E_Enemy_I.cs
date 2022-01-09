using System.Collections;
using UnityEngine;
using WeaverCore.Implementations;

namespace WeaverCore.Editor.Implementations
{
	public class E_Enemy_I : Enemy_I
    {
        public class E_Statics : Enemy_I.Statics
        {
            public override IEnumerator Roar(GameObject source, float duration, AudioClip roarSound, bool lockPlayer)
            {
                return Roar(source, source.transform.position, duration, roarSound, lockPlayer);
            }

            public override IEnumerator Roar(GameObject source, Vector3 spawnPosition, float duration, AudioClip roarSound, bool lockPlayer)
            {
                if (roarSound != null)
                {
                    WeaverAudio.PlayAtPoint(roarSound, source.transform.position);
                }

                var emitter = RoarEmitter.Spawn(source.transform.position);

                emitter.stopAfterTime = duration;

                if (lockPlayer)
                {
                    Player.Player1.EnterRoarLock();
                }

                yield return new WaitForSeconds(duration);

                if (lockPlayer)
                {
                    Player.Player1.ExitRoarLock();
                }
            }
        }
    }
}
