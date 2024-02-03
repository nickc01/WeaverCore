using System;
using System.Collections;
using System.ComponentModel;
using System.Security.Cryptography;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{

    public class WeaverSoulTotem : MonoBehaviour, IHittable
	{
        [SerializeField]
        float startDistance = 6f;

        [SerializeField]
        float stopDistance = 7f;

        [SerializeField]
        int hitTimes = 5;

        [SerializeField]
        float hitTimeInterval = 0.25f;

        [SerializeField]
        float nearbyAlpha = 0.7f;

        [SerializeField]
        AudioClip soulTotemAwakeSound;

        [SerializeField]
        AudioClip soulTotemHitSound;

        [SerializeField]
        WeaverSoulOrb soulTotalOrbPrefab;

        ParticleSystem soulParticles;
        ParticleSystem.EmissionModule soulE;

        SpriteRenderer glower;
        Collider2D mainCollider;
        WeaverPersistentIntItem hitCounter;
        //SpriteRenderer mainRenderer;

        [SerializeField]
        int value = -1;

        [SerializeField]
        bool activated = false;
        bool hitReceived = false;
        HitInfo lastHit;

        private void Awake()
        {
            hitCounter = GetComponent<WeaverPersistentIntItem>();

            if (hitCounter != null)
            {
                hitCounter.OnGetSaveState += delegate (ref int initialSavedValue)
                {
                    WeaverLog.Log("GET SAVE STATE = " + value);
                    initialSavedValue = value;
                };
                hitCounter.OnSetSaveState += delegate (int retrievedVal)
                {
                    WeaverLog.Log("RETRIEVED VALUE = " + retrievedVal);
                    value = retrievedVal;
                    if (value == 0)
                    {
                        activated = true;
                    }
                    else if (value == -1)
                    {
                        value = hitTimes;
                    }
                };
            }

            mainCollider = GetComponent<Collider2D>();
            soulParticles = transform.Find("Soul Particles").GetComponent<ParticleSystem>();
            glower = transform.Find("Glower").GetComponent<SpriteRenderer>();
            //mainRenderer = GetComponent<SpriteRenderer>();
            soulE = soulParticles.emission;
            soulE.enabled = false;
            StartCoroutine(MainRoutine());

            glower.color = glower.color.With(a: 0f);
            destAlpha = glower.color.a;
        }

        IEnumerator MainRoutine()
        {
            if (value == -1)
            {
                value = hitTimes;
            }

            if (activated)
            {
                //DEPLETED
                yield return Depleted();
                yield break;
            }

            if (value <= 0)
            {
                //DEPLETED
                yield return Depleted();
                yield break;
            }

            while (true)
            {
                soulE.enabled = false;

                var distance = Vector2.Distance(transform.position, Player.Player1.transform.position);

                if (distance < startDistance)
                {
                    //MOVE
                    hitReceived = false;
                    if (soulTotemAwakeSound != null)
                    {
                        WeaverAudio.PlayAtPoint(soulTotemAwakeSound, transform.position);
                    }

                    soulE.enabled = true;

                    FadeTo(nearbyAlpha);

                    yield return new WaitUntil(() => Vector2.Distance(Player.Player1.transform.position, transform.position) > stopDistance || hitReceived || activated);

                    if (hitReceived)
                    {
                        //TAKE DAMAGE
                        hitReceived = false;
                        if (soulTotemHitSound != null)
                        {
                            WeaverAudio.PlayAtPoint(soulTotemHitSound, transform.position);
                        }

                        var oldAlpha = glower.color.a;
                        glower.color = glower.color.With(a: 1f);

                        CameraShaker.Instance.Shake(Enums.ShakeType.EnemyKillShake);

                        value--;

                        Pooling.Instantiate(EffectAssets.NailStrikePrefab, transform.position, Quaternion.identity);

                        Pooling.Instantiate(EffectAssets.WhiteFlashPrefab, transform.position, Quaternion.identity);

                        //TODO - SPAWN SOUL ORB R
                        if (soulTotalOrbPrefab != null)
                        {
                            FlingUtilities.SpawnPooledAndFling(new FlingUtils.Config
                            {
                                Prefab = soulTotalOrbPrefab.gameObject,
                                AmountMin = 8,
                                AmountMax = 9,
                                SpeedMin = 10f,
                                SpeedMax = 20f,
                                AngleMin = 0f,
                                AngleMax = 360f,
                                OriginVariationX = 1f,
                                OriginVariationY = 1f
                            }, null, transform.position);
                        }

                        yield return new WaitForSeconds(hitTimeInterval);

                        glower.color = glower.color.With(a: oldAlpha);

                        if (value == 0)
                        {
                            yield return Depleted();
                            yield break;
                        }
                    }
                }
                else
                {
                    /*StartCoroutine(FadeToTransparent(mainRenderer.color, 1f, () =>
                    {
                        mainRenderer.enabled = false;
                    }));*/
                    FadeTo(0f);
                }

                if (activated)
                {
                    //DEPLETED
                    yield return Depleted();
                    yield break;
                }
                else
                {
                    yield return null;
                }
            }
        }

        IEnumerator Depleted()
        {
            //mainRenderer.enabled = false;
            mainCollider.enabled = false;

            glower.gameObject.SetActive(false);
            activated = true;
            soulE.enabled = false;

            FadeTo(0f);

            yield return new WaitForSeconds(1f);
            /*while (true)
            {
                var distance = Vector2.Distance(transform.position, Player.Player1.transform.position);

                FadeTo(0f);

                yield return null;
            }*/
        }

        float destAlpha = 0f;
        Coroutine fadeRoutine;

        public void FadeTo(float alpha)
        {
            if (alpha != destAlpha)
            {
                if (fadeRoutine != null)
                {
                    StopCoroutine(fadeRoutine);
                    fadeRoutine = null;
                }

                destAlpha = alpha;

                fadeRoutine = StartCoroutine(Fade(glower, glower.color, destAlpha, 1f));
            }
        }

        IEnumerator Fade(SpriteRenderer renderer, Color sourceColor, float destAlpha, float time, Action onDone = null)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                renderer.color = sourceColor.With(a: Mathf.Lerp(sourceColor.a, destAlpha, t / time));
                yield return null;
            }

            //mainRenderer.color = sourceColor.With(a: destAlpha);
            fadeRoutine = null;
            onDone?.Invoke();
        }

        public bool Hit(HitInfo hit)
        {
            if (!activated)
            {
                if (hit.AttackType == Enums.AttackType.Nail)
                {
                    hitReceived = true;
                    lastHit = hit;
                    return true;
                }
            }

            return false;
        }
    }
}