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

            public override IEnumerator Roar(GameObject source, float duration, AudioClip roarSound)
            {
                if (roarSound != null)
                {
                    WeaverAudio.PlayAtPoint(roarSound, source.transform.position);
                }

                var emitter = RoarEmitter.Spawn(source.transform.position);

                emitter.stopAfterTime = duration;

                SetPlayerRoarObject(source);
                PlayerRoarEnter();

                yield return new WaitForSeconds(duration);

                SendEventToPlayer("ROAR EXIT");
            }

            /*public override IEnumerator Roar(GameObject source, float duration)
            {
                var roarPrefab = WeaverAssets.LoadWeaverAsset<GameObject>("Roar Prefab");

                var roar = GameObject.Instantiate(roarPrefab,source.transform.position,Quaternion.identity);

                SetPlayerRoarObject(source);
                PlayerRoarEnter();

                yield return new WaitForSeconds(duration);

                SendEventToPlayer("END");
                SendEventToPlayer("ROAR EXIT");
                SendEventToPlayer("START SPAWN");

            }*/

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

            static void PlayerRoarEnter()
            {
                SendEventToPlayer("ROAR ENTER");
            }

            static void SendEventToPlayer(string eventName)
            {
                var owner = new FsmOwnerDefault();
                owner.GameObject = Player.Player1.gameObject;
                GetRoarFSM().Fsm.Event(new FsmEventTarget()
                {
                    excludeSelf = false,
                    target = FsmEventTarget.EventTarget.GameObject,
                    gameObject = owner,
                    sendToChildren = false

                }, eventName);


                /*
                if (roarSound != null)
                {
                    WeaverAudio.PlayAtPoint(roarSound, source.transform.position);
                }

                var emitter = RoarEmitter.Spawn(source.transform.position);

                emitter.stopAfterTime = duration;

                yield return new WaitForSeconds(duration);
                */
            }
        }
    }
}
