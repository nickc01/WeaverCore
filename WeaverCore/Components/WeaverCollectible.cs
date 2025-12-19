using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Internal;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class WeaverCollectible : MonoBehaviour
    {
        public bool HasBeenCollected { get; private set; } = false;

        [SerializeField]
        bool isCollectible = true;

        [SerializeField]
        GameObject pulseWave = null;

        [SerializeField]
        bool playPickupEffects = true;

        [Space]
        [Header("Appear")]
        [SerializeField]
        bool appear = false;

        [SerializeField]
        Vector3 appearPos = default;

        [SerializeField]
        ParticleSystem appearTrail;

        [SerializeField]
        ParticleSystem appearCloud;

        [SerializeField]
        ParticleSystem getCloud;

        [SerializeField]
        GameObject getAnim;

        [SerializeField]
        GameObject plink;

        [SerializeField]
        List<AudioClip> appearSounds;

        [SerializeField]
        float appearSoundsVolume = 1f;

        [SerializeField]
        Vector2 appearSoundsPitchRange = new Vector2(1.25f, 1.25f);

        [SerializeField]
        List<AudioClip> fullyRevealSounds;

        [SerializeField]
        float fullyRevealSoundsVolume = 1f;

        [SerializeField]
        Vector2 fullyRevealSoundsPitchRange = new Vector2(1f, 1f);

        [SerializeField]
        List<AudioClip> collectSounds;

        [SerializeField]
        float collectSoundsVolume = 1f;

        [SerializeField]
        Vector2 collectSoundsPitchRange = new Vector2(1f, 1f);

        [SerializeField]
        bool doCameraShake = true;

        [SerializeField]
        ShakeType shakeType = ShakeType.EnemyKillShake;

        [SerializeField]
        GameObject whiteWave;




        [Space]
        [Header("Appear Cloud Tween")]
        [SerializeField]
        Vector3 appearCloudScaleTween = new Vector3(0.1f, 0.1f, 0.1f);

        [SerializeField]
        float appearCloudTweenTime = 1.5f;

        [SerializeField]
        float appearCloudTweenDelay = 1f;

        [SerializeField]
        AnimationCurve appearCloudTweenCurve = AnimationCurve.Linear(0,0,1,1);

        [Space]
        [Header("Idle")]
        [SerializeField]
        ParticleSystem idleParticles;

        [Space]
        [Header("Collect")]
        [SerializeField]
        GameObject heartPieceOrbPrefab;

        [SerializeField]
        Vector2Int orbSpawnAmount = new Vector2Int(20, 20);

        [SerializeField]
        Vector2 orbSpeedRange = new Vector2(18f, 25f);

        [SerializeField]
        Vector2 orbAngleRange = new Vector2(0f, 360f);

        [SerializeField]
        float orbOriginVariation = 1f;

        [NonSerialized]
        SpriteRenderer _mainRenderer;
        public SpriteRenderer MainRenderer
        {
            get
            {
                if (_mainRenderer == null)
                {
                    var spriteChild = transform.Find("Sprite");
                    if (spriteChild != null)
                    {
                        _mainRenderer = spriteChild.GetComponent<SpriteRenderer>();
                    }
                    else
                    {
                        _mainRenderer = GetComponent<SpriteRenderer>();
                    }
                }
                return _mainRenderer;
            }
        }

        [NonSerialized]
        bool checkingForCollision = false;

        [NonSerialized]
        Coroutine pulseRoutine;



        protected virtual void Awake()
        {
            checkingForCollision = false;
            StartCoroutine(MainRoutine());
            pulseRoutine = StartCoroutine(PulseWaveRoutine());
        }

        IEnumerator PulseWaveRoutine()
        {
            if (pulseWave != null)
            {
                while (true)
                {
                    pulseWave.SetActive(true);
                    yield return new WaitForSeconds(2.2f);
                }
            }
        }

        IEnumerator MainRoutine()
        {
            checkingForCollision = false;
            if (!IsCollectible())
            {
                Destroy(gameObject);
            }

            if (TryGetComponent<AudioSource>(out var audio))
            {
                audio.volume = 1f;
                audio.Play();
            }

            if (appear)
            {
                foreach (var sound in appearSounds)
                {
                    if (sound != null)
                    {
                        var inst = WeaverAudio.PlayAtPoint(sound, transform.position + appearPos, appearSoundsVolume);
                        inst.AudioSource.pitch = appearSoundsPitchRange.RandomInRange();
                    }
                }

                appearCloud.transform.localPosition = appearPos;
                appearTrail.transform.localPosition = appearPos;
                plink.transform.localPosition = appearPos;

                plink.SetActive(true);

                //STOP Event

                idleParticles.Stop();

                MainRenderer.enabled = false;

                appearCloud.Play();

                yield return new WaitForSeconds(appearCloudTweenDelay);

                var oldScale = appearCloud.transform.localScale;

                for (float t = 0; t < appearCloudTweenTime; t += Time.deltaTime)
                {
                    appearCloud.transform.localScale = Vector3.Lerp(oldScale, appearCloudScaleTween, appearCloudTweenCurve.Evaluate(t / appearCloudTweenTime));
                    yield return null;
                }

                appearCloud.Stop();

                appearTrail.Play();

                foreach (var sound in fullyRevealSounds)
                {
                    if (sound != null)
                    {
                        var inst = WeaverAudio.PlayAtPoint(sound, transform.position + appearPos, fullyRevealSoundsVolume);
                        inst.AudioSource.pitch = fullyRevealSoundsPitchRange.RandomInRange();
                    }
                }

                if (doCameraShake)
                {
                    CameraShaker.Instance.Shake(shakeType);
                }

                getCloud.Play();
                idleParticles.Play();
                appearTrail.Stop();
                MainRenderer.enabled = true;
                //START Event
                whiteWave.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
                whiteWave.SetActive(true);

                yield return new WaitForSeconds(1.5f);
            }

            while (true)
            {
                checkingForCollision = true;

                yield return new WaitUntil(() => !checkingForCollision);

                if (CanCollect())
                {
                    break;
                }
            }

            Pickup();

            yield break;
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if ((collision.gameObject.CompareTag("HeroBox") || collision.gameObject.CompareTag("Player")) && checkingForCollision)
            {
                checkingForCollision = false;
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D collision)
        {
            if ((collision.CompareTag("HeroBox") || collision.CompareTag("Player")) && checkingForCollision)
            {
                checkingForCollision = false;
            }
        }

        public bool CanCollect()
        {
            var sd = HeroController.instance.GetCState("superDashing");
            var recoiling = HeroController.instance.GetCState("recoiling");

            WeaverLog.Log("Can Collect = " + (!sd && !recoiling));

            return !sd && !recoiling;
        }

        public void Pickup()
        {
            if (TryGetComponent<AudioSource>(out var audio))
            {
                audio.Stop();
            }
            if (playPickupEffects)
            {
                EventManager.SendEventToGameObject("FSM CANCEL", Player.Player1.gameObject);
                PlayerData.instance.SetBool("isInvincible", true);
                PlayerData.instance.SetBool("disablePause", true);
            }

            foreach (var sound in collectSounds)
            {
                if (sound != null)
                {
                    var inst = WeaverAudio.PlayAtPoint(sound, transform.position + appearPos, collectSoundsVolume);
                    inst.AudioSource.pitch = collectSoundsPitchRange.RandomInRange();
                }
            }

            //Vessel Fragment Collected
            HasBeenCollected = true;

            if (playPickupEffects)
            {
                HeroController.instance.RelinquishControl();
                HeroController.instance.StopAnimationControl();
                HeroUtilities.PlayPlayerClip("Collect Heart Piece");
                if (Player.Player1.TryGetComponent<Rigidbody2D>(out var playerRB))
                {
                    playerRB.gravityScale = 0f;
                }
            }

            //GET Event

            StopCoroutine(pulseRoutine);
            pulseRoutine = null;

            idleParticles.Stop();
            MainRenderer.enabled = false;
            if (playPickupEffects)
            {
                getAnim.SetActive(true);
                CameraShaker.Instance.Shake(ShakeType.AverageShake);

                if (heartPieceOrbPrefab != null)
                {
                    var spawnAmount = orbSpawnAmount.RandomInRange();
                    for (int i = 0; i < spawnAmount; i++)
                    {
                        GameObject orb = Instantiate(heartPieceOrbPrefab, transform);

                        float angle = UnityEngine.Random.Range(orbAngleRange.x, orbAngleRange.y) * Mathf.Deg2Rad;

                        orb.transform.localPosition = orbOriginVariation * new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                        orb.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * orbSpeedRange.RandomInRange();
                    }
                }
            }

            StartCoroutine(PickupRoutine());
        }

        IEnumerator PickupRoutine()
        {
            if (playPickupEffects)
            {
                yield return new WaitForSeconds(0.6f);
            }

            if (playPickupEffects && Player.Player1.TryGetComponent<Rigidbody2D>(out var playerRB))
            {
                playerRB.linearVelocity = default;
            }
            yield return OnPickup();
            if (!PlayerData.instance.GetBool("isInvincible") && !PlayerData.instance.GetBool("disablePause"))
            {
                OnEnd();
                yield break;
            }
            if (playPickupEffects)
            {
                yield return HeroUtilities.PlayPlayerClipTillDone("Collect Heart Piece End");
                if (Player.Player1.TryGetComponent<Rigidbody2D>(out playerRB))
                {
                    playerRB.gravityScale = 0.79f;
                }
                PlayerData.instance.SetBool("isInvincible", false);
                HeroController.instance.RegainControl();
                HeroController.instance.StartAnimationControl();
                PlayerData.instance.SetBool("disablePause", false);
            }
            OnEnd();
        }

        protected virtual void OnEnd()
        {

        }

        protected virtual IEnumerator OnPickup()
        {
            if (Other_Preloads.VesselFragmentUIPrefab != null)
            {
                GameManager.instance.StoryRecord_soulPiece();
                PlayerData.instance.SetBool("vesselFragmentCollected", true);
                PlayerData.instance.IntAdd("vesselFragments", 1);   
                
                var uiInstance = GameObject.Instantiate(Other_Preloads.VesselFragmentUIPrefab, Vector3.zero, Quaternion.identity);
                var vesselFragments = PlayerData.instance.GetInt("vesselFragments");
                PlayMakerUtilities.SetFsmInt(uiInstance, "Vessel Fragment UI", "Pieces", vesselFragments);

                float time = Time.time;

                yield return new WaitUntil(() => uiInstance == null || Time.time >= time + 10f); 
            }

            yield break;
        }

        public virtual bool IsCollectible()
        {
            return isCollectible;
        }
    }
}