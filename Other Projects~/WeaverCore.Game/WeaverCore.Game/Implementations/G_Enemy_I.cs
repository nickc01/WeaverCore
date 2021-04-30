using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using System.Collections;
using UnityEngine;
using WeaverCore.Implementations;
using WeaverCore.Utilities;

namespace WeaverCore.Game.Implementations
{
	public class G_Enemy_I : Enemy_I
    {
        public class G_Statics : Enemy_I.Statics
        {
            static PlayMakerFSM GetRoarFSM()
            {
                var player = Player.Player1.gameObject;

                return ActionHelpers.GetGameObjectFsm(player, "Roar Lock");
            }

            static void SetPlayerRoarObject(GameObject obj)
            {
                var roarLockFSM = GetRoarFSM();

                var roarObjectGM = roarLockFSM.FsmVariables.FindFsmGameObject("Roar Object");

                roarObjectGM.Value = obj;
            }

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

                SetPlayerRoarObject(source);
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
