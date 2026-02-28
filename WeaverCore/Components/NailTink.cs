using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore;
using WeaverCore.Enums;
using WeaverCore.Features;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// When the player hits an object with this component attached, it will cause a nail parry to occur
    /// </summary>
    public class NailTink : MonoBehaviour, IHittable
    {
        const float EffectOffset = 1.5f;

        static NailTink _global;
        public static NailTink Global => _global ??= WeaverAssets.LoadWeaverAsset<GameObject>("Global Nail Tink").GetComponent<NailTink>();

        [Tooltip("The sound that is played when the player hits this object")]
        public AudioClip TinkSound;
        [Tooltip("The tink prefab that is spawned when the player hits this object")]
        public GameObject TinkEffectPrefab;

        [Tooltip("The volume of the tink sound")]
        public float TinkSoundVolume = 1f;

        [Tooltip("The pitch of the tink sound")]
        public float TinkSoundPitch = 1f;

        [SerializeField]
        [Tooltip("If set to true, then the tink effect will play even if the EntityHealth component is marked as invicible")]
        bool forceValidHit = false;

        [SerializeField]
        float evasionTime = 0.2f;

        string collisionLayerName = "Tinker";
        int collisionLayerID = 16;

        Enemy enemy;
        EntityHealth healthManager;

        public event Action<IHittable, HitInfo> OnTink;

        float lastHitTime = 0;

        public static void Play(Vector3 position, float attackDirection)
        {
            var global = Global;
            if (global == null || global.gameObject == null)
            {
                return;
            }

            var instanceObj = Pooling.Instantiate(global.gameObject, position, global.transform.rotation);
            var instance = instanceObj.GetComponent<NailTink>();
            if (instance == null)
            {
                Pooling.Destroy(instanceObj);
                return;
            }

            instance.StartCoroutine(instance.PlayRoutine(attackDirection));
        }

        public static void Play(float attackDirection)
        {
            Vector3 position = Player.Player1 != null ? Player.Player1.transform.position : Vector3.zero;
            Play(position, attackDirection);
        }

        public bool Hit(HitInfo hit)
        {
            if (Time.time < lastHitTime + evasionTime)
            {
                return false;
            }

            if (!(hit.AttackType == AttackTypes.Nail || hit.AttackType == AttackTypes.NailBeam))
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
                if (forceValidHit)
                {
                    OnTink?.Invoke(this, hit);
                    StartCoroutine(HitRoutine(hit));
                    lastHitTime = Time.time;
                    return true;
                }
                else
                {
                    var validity = healthManager.IsValidHit(ref hit);
                    if (validity == EntityHealth.HitResult.Valid)
                    {
                        OnTink?.Invoke(this, hit);
                        StartCoroutine(HitRoutine(hit));
                        lastHitTime = Time.time;
                    }
                    return validity == EntityHealth.HitResult.Valid;
                }
            }
            else
            {
                OnTink?.Invoke(this, hit);
                StartCoroutine(HitRoutine(hit));
                lastHitTime = Time.time;
                return true;
            }
        }

        IEnumerator HitRoutine(HitInfo hit)
        {
            WeaverGameManager.FreezeGameTime(WeaverGameManager.TimeFreezePreset.Preset3);
            Player.Player1.EnterParryState();
            CameraShaker.Instance.Shake(ShakeType.EnemyKillShake);

            //PLAY AUDIO
            PlayAudio(transform.position);
            var direction = ResolveDirection(hit.Direction);
            ApplyRecoil(direction);
            SpawnEffect(Player.Player1.transform.position, direction);

            yield return null;


            Player.Player1.RecoverFromParry();


            if (enemy != null)
            {
                enemy.OnParry(this, hit);
            }


            yield return null;

            yield return new WaitForSeconds(0.15f);
        }

        IEnumerator PlayRoutine(float attackDirection)
        {
            WeaverGameManager.FreezeGameTime(WeaverGameManager.TimeFreezePreset.Preset3);
            if (Player.Player1 != null)
            {
                Player.Player1.EnterParryState();
            }

            CameraShaker.Instance.Shake(ShakeType.EnemyKillShake);

            PlayAudio(transform.position);
            var direction = ResolveDirection(attackDirection);
            ApplyRecoil(direction);
            SpawnEffect(transform.position, direction);

            yield return null;

            if (Player.Player1 != null)
            {
                Player.Player1.RecoverFromParry();
            }

            yield return null;
            yield return new WaitForSeconds(0.15f);

            Pooling.Destroy(gameObject);
        }

        void PlayAudio(Vector3 origin)
        {
            if (TinkSound == null || TinkSoundVolume <= 0.01f)
            {
                return;
            }

            var instance = WeaverAudio.PlayAtPoint(TinkSound, origin, TinkSoundVolume);
            if (instance != null && instance.AudioSource != null)
            {
                instance.AudioSource.pitch = TinkSoundPitch;
            }
        }

        static CardinalDirection ResolveDirection(float attackDirection)
        {
            if (attackDirection < 360f && attackDirection > 225f)
            {
                return CardinalDirection.Down;
            }

            if (attackDirection <= 225f && attackDirection > 135f)
            {
                return CardinalDirection.Left;
            }

            if (attackDirection <= 135 && attackDirection > 45f)
            {
                return CardinalDirection.Up;
            }

            return CardinalDirection.Right;
        }

        static void ApplyRecoil(CardinalDirection direction)
        {
            if (Player.Player1 == null)
            {
                return;
            }

            switch (direction)
            {
                case CardinalDirection.Up:
                    Player.Player1.Recoil(CardinalDirection.Down);
                    break;
                case CardinalDirection.Down:
                    Player.Player1.Recoil(CardinalDirection.Up);
                    break;
                case CardinalDirection.Left:
                    Player.Player1.Recoil(CardinalDirection.Right);
                    break;
                case CardinalDirection.Right:
                    Player.Player1.Recoil(CardinalDirection.Left);
                    break;
            }
        }

        void SpawnEffect(Vector3 origin, CardinalDirection direction)
        {
            if (TinkEffectPrefab == null)
            {
                return;
            }

            Vector3 offset = Vector3.zero;
            switch (direction)
            {
                case CardinalDirection.Up:
                    offset = new Vector3(0f, EffectOffset, 0f);
                    break;
                case CardinalDirection.Down:
                    offset = new Vector3(0f, -EffectOffset, 0f);
                    break;
                case CardinalDirection.Left:
                    offset = new Vector3(-EffectOffset, 0f, 0f);
                    break;
                case CardinalDirection.Right:
                    offset = new Vector3(EffectOffset, 0f, 0f);
                    break;
            }

            Pooling.Instantiate(TinkEffectPrefab, origin + offset, Quaternion.identity);
        }
    }
}
