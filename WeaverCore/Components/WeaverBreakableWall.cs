using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WeaverCore.Assets;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Settings;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class WeaverBreakableWall : MonoBehaviour, IHittable
    {
        [SerializeField]
        SaveSpecificSettings saveSettings;

        [SerializeField]
        [SaveSpecificFieldName(typeof(bool), nameof(saveSettings))]
        string isBrokenFieldName;

        [field: SerializeField]
        public float RecoilSpeed { get; protected set; } = 1f;

        [field: SerializeField]
        public int Hits {get; protected set; } = 4;

        [SerializeField]
        float invincibilityTime = 0.25f;

        [SerializeField]
        List<AudioClip> hitSounds;

        [SerializeField]
        Vector2 hitSoundPitchRange = new Vector2(0.85f, 1.15f);

        [SerializeField]
        List<AudioClip> breakSounds;

        [SerializeField]
        List<ParticleSystem> hitParticles;

        [SerializeField]
        List<ParticleSystem> breakParticles;

        [SerializeField]
        ShakeType breakShakeType = ShakeType.AverageShake;

        [SerializeField]
        ParticleSystem dustHitMedPrefab;

        [SerializeField]
        ParticleSystem dustHitMedDownPrefab;

        [SerializeField]
        ParticleSystem dustBreakWallPrefab;

        [Space]
        [Header("Before Broken")]
        [SerializeField]
        List<GameObject> shownWhenUnbroken;
        [SerializeField]
        List<GameObject> hiddenWhenUnbroken;

        [Space]
        [Header("After Broken")]
        [SerializeField]
        List<GameObject> shownWhenBroken;
        [SerializeField]
        List<GameObject> hiddenWhenBroken;

        public UnityEvent<bool> OnBrokenEvent;

        [NonSerialized]
        SpriteRenderer _mainRenderer;
        public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponentInChildren<SpriteRenderer>();

        [NonSerialized]
        Collider2D _mainCollider;
        public Collider2D MainCollider => _mainCollider ??= GetComponent<Collider2D>();

        [NonSerialized]
        Rigidbody2D _mainRigidBody;
        public Rigidbody2D MainRigidbody => _mainRigidBody ??= GetComponent<Rigidbody2D>();


        [NonSerialized]
        bool canTakeHit = true;

        [NonSerialized]
        bool _is_broken_internal = false;


        protected virtual void Awake()
        {
            foreach (var g in shownWhenUnbroken)
            {
                if (g.TryGetComponent<RevealableArea>(out var r))
                {
                    r.Reveal();
                }
                else
                {
                    r.gameObject.SetActive(true);
                }
            }

            foreach (var g in hiddenWhenUnbroken)
            {
                if (g.TryGetComponent<RevealableArea>(out var r))
                {
                    r.Hide();
                }
                else
                {
                    r.gameObject.SetActive(false);
                }
            }
            canTakeHit = true;
            if (IsBroken)
            {
                OnBroken(true);
            }
        }

        public virtual bool IsBroken
        {
            get => saveSettings.TryGetFieldValue(isBrokenFieldName, out bool result) ? result : _is_broken_internal;
            protected set {
                if (!saveSettings.TrySetFieldValue(isBrokenFieldName, value))
                {
                    _is_broken_internal = value;
                }
            }
        }

        public void TakeDamage(CardinalDirection hitDirection)
        {
            {
                var instance = WeaverAudio.PlayAtPoint(hitSounds.GetRandomElement(), transform.position);
                instance.AudioSource.pitch = hitSoundPitchRange.RandomInRange();
            }

            foreach (var p in hitParticles)
            {
                p.Play();
            }

            if (hitDirection == CardinalDirection.Right)
            {
                var slashImpact = Pooling.Instantiate(EffectAssets.SlashImpactPrefab, transform.position, Quaternion.identity);
                var chooser = UnityEngine.Random.Range(340f, 380f);
                slashImpact.transform.SetRotationZ(chooser);

                slashImpact.transform.SetLocalScaleXY(-1.5f, 1.5f);
                Pooling.Instantiate(dustHitMedPrefab, transform.position + new Vector3(-1, 0), transform.rotation * Quaternion.Euler(0, 90, 270));
            }
            else if (hitDirection == CardinalDirection.Left)
            {
                var slashImpact = Pooling.Instantiate(EffectAssets.SlashImpactPrefab, transform.position, Quaternion.identity);
                var chooser = UnityEngine.Random.Range(340f, 380f);
                slashImpact.transform.SetRotationZ(chooser);

                slashImpact.transform.SetLocalScaleXY(1.5f, 1.5f);
                Pooling.Instantiate(dustHitMedPrefab, transform.position + new Vector3(-1, 0), transform.rotation * Quaternion.Euler(180, 90, 270));
            }
            else if (hitDirection == CardinalDirection.Up)
            {
                Pooling.Instantiate(dustHitMedPrefab, transform.position + new Vector3(-1, 0), transform.rotation * Quaternion.Euler(270, 90, 270));
            }
            else if (hitDirection == CardinalDirection.Down)
            {
                Pooling.Instantiate(dustHitMedDownPrefab, transform.position + new Vector3(-1, 0), transform.rotation * Quaternion.Euler(-72.5f, -180, -180));
            }

            if (--Hits <= 0)
            {
                Break();
            }
            else
            {
                Vector2 recoil = default;
                switch (hitDirection)
                {
                    case CardinalDirection.Up:
                        recoil = new Vector2(0f, -RecoilSpeed);
                        break;
                    case CardinalDirection.Down:
                        recoil = new Vector2(0f, RecoilSpeed);
                        break;
                    case CardinalDirection.Left:
                        recoil = new Vector2(RecoilSpeed, 0f);
                        break;
                    case CardinalDirection.Right:
                        recoil = new Vector2(-RecoilSpeed, 0f);
                        break;
                    default:
                        break;
                }

                StartCoroutine(TakeDamageRoutine(hitDirection, recoil));
            }
        }

        IEnumerator TakeDamageRoutine(CardinalDirection direction, Vector2 recoil) 
        {
            for (float t = 0; t < 0.1f; t += Time.deltaTime)
            {
                MainRigidbody.velocity = recoil;
                yield return null;
            }

            for (float t = 0; t < 0.1f; t += Time.deltaTime)
            {
                MainRigidbody.velocity = -recoil;
                yield return null;
            }

            MainRigidbody.velocity = default;
            canTakeHit = true;
            yield break;
        }

        public void Break()
        {
            IsBroken = true;
            OnBroken(false);
        }



        protected virtual void OnBroken(bool wasAlreadyBroken)
        {
            foreach (var g in shownWhenBroken)
            {
                if (g.TryGetComponent<RevealableArea>(out var r))
                {
                    r.Reveal();
                }
                else
                {
                    r.gameObject.SetActive(true);
                }
            }

            foreach (var g in hiddenWhenBroken)
            {
                if (g.TryGetComponent<RevealableArea>(out var r))
                {
                    r.Hide();
                }
                else
                {
                    r.gameObject.SetActive(false);
                }
            }

            canTakeHit = false;
            OnBrokenEvent?.Invoke(wasAlreadyBroken);

            if (TryGetComponent<RevealableArea>(out var selfReveal))
            {
                selfReveal.Hide();
            }
            else
            {
                MainRenderer.enabled = false;
            }

            if (MainCollider != null)
            {
                MainCollider.enabled = false;
            }

            if (wasAlreadyBroken)
            {
                return;
            }


            foreach (var p in breakParticles)
            {
                p.Play();
            }
            foreach (var s in breakSounds)
            {
                WeaverAudio.PlayAtPoint(s, transform.position);
            }

            CameraShaker.Instance.Shake(breakShakeType);
            //GameObject.Destroy(gameObject);
        }

        public bool Hit(HitInfo hit)
        {
            if (canTakeHit)
            {
                canTakeHit = false;
                if (hit.AttackType == AttackType.Nail || hit.AttackType == AttackType.NailBeam || hit.AttackType == AttackType.Generic)
                {
                    var strikeEffect = Pooling.Instantiate(EffectAssets.NailStrikePrefab, transform.position, Quaternion.identity);
                    strikeEffect.transform.localScale = new Vector3(1.5f, 1.5f, 0f);
                    TakeDamage(DirectionUtilities.DegreesToDirection(hit.Direction + 180f));
                }
                else if (hit.AttackType == AttackType.Spell)
                {
                    Pooling.Instantiate(EffectAssets.FireballHitPrefab, transform.position, Quaternion.identity);
                    Break();
                    //TakeDamage(DirectionUtilities.DegreesToDirection(hit.Direction + 180f), 9999);
                    return true;
                }
            }

            return false;
        }
    }
}