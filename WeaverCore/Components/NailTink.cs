using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Interfaces;

namespace WeaverCore.Components
{

    /// <summary>
    /// When the player hits an object with this component attached, it will cause a nail parry to occur
    /// </summary>
    public class NailTink : MonoBehaviour, IHittable
    {
        [Tooltip("The sound that is played when the player hits this object")]
        public AudioClip TinkSound;
        [Tooltip("The tink prefab that is spawned when the player hits this object")]
        public GameObject TinkEffectPrefab;

        string collisionLayerName = "Tinker";
        int collisionLayerID = 16;

        Enemy enemy;
        EntityHealth healthManager;

        public bool Hit(HitInfo hit)
        {
            if (!(hit.AttackType == AttackType.Nail || hit.AttackType == AttackType.NailBeam))
            {
                return false;
            }

            if (healthManager == null)
            {
                healthManager = GetComponentInParent<EntityHealth>();
            }

            if (enemy == null)
            {
                enemy = GetComponentInParent<Enemy>();
            }

            if (healthManager != null)
            {
                var validity = healthManager.IsValidHit(hit);
                if (validity == EntityHealth.HitResult.Valid)
                {
                    StartCoroutine(HitRoutine(hit));
                }
                return validity == EntityHealth.HitResult.Valid;
            }
            else
            {
                StartCoroutine(HitRoutine(hit));
                return true;
            }
        }

        IEnumerator HitRoutine(HitInfo hit)
        {
            WeaverGameManager.FreezeGameTime(WeaverGameManager.TimeFreezePreset.Preset3);
            Player.Player1.EnterParryState();
            CameraShaker.Instance.Shake(ShakeType.EnemyKillShake);

            //PLAY AUDIO
            WeaverAudio.PlayAtPoint(TinkSound, transform.position);

            var attackDirection = hit.Direction;

            CardinalDirection direction = CardinalDirection.Right;

            if (attackDirection < 360f && attackDirection > 225f)
            {
                direction = CardinalDirection.Down;
            }
            else if (attackDirection <= 225f && attackDirection > 135f)
            {
                direction = CardinalDirection.Left;
            }
            else if (attackDirection <= 135 && attackDirection > 45f)
            {
                direction = CardinalDirection.Up;
            }
            else
            {
                direction = CardinalDirection.Right;
            }

            switch (direction)
            {
                case CardinalDirection.Up:
                    Player.Player1.Recoil(CardinalDirection.Down);
                    Pooling.Instantiate(TinkEffectPrefab, Player.Player1.transform.position + new Vector3(0f, 1.5f, 0f), Quaternion.identity);
                    break;
                case CardinalDirection.Down:
                    Player.Player1.Recoil(CardinalDirection.Up);
                    Pooling.Instantiate(TinkEffectPrefab, Player.Player1.transform.position + new Vector3(0f, -1.5f, 0f), Quaternion.identity);
                    break;
                case CardinalDirection.Left:
                    Player.Player1.Recoil(CardinalDirection.Right);
                    Pooling.Instantiate(TinkEffectPrefab, Player.Player1.transform.position + new Vector3(-1.5f, 0f, 0f), Quaternion.identity);
                    break;
                case CardinalDirection.Right:
                    Player.Player1.Recoil(CardinalDirection.Left);
                    Pooling.Instantiate(TinkEffectPrefab, Player.Player1.transform.position + new Vector3(1.5f, 0f, 0f), Quaternion.identity);
                    break;
            }

            yield return null;


            Player.Player1.RecoverFromParry();


            if (enemy != null)
            {
                enemy.OnParry(this, hit);
            }


            yield return null;

            yield return new WaitForSeconds(0.15f);
        }
    }
}