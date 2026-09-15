using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// WeaverCore's implementation of surface water
    /// </summary>
    public class WeaverSurfaceWater : MonoBehaviour
    {
        [SerializeField]
        GameObject splashInParticles;

        [SerializeField]
        GameObject splashOutParticles;

        [SerializeField]
        Color splashColor = Color.white;

        [SerializeField]
        GameObject spatterParticles;

        [SerializeField]
        GameObject dripParticles;

        [SerializeField]
        Vector3 spatterSpawnOffset = new Vector3(0f, -0.7f, 0f);

        [SerializeField]
        AudioClip splashSound;

        [SerializeField]
        float splashSoundInDelay = 0;

        [SerializeField]
        Vector2 splashInSoundPitchRange = new Vector2(0.8f, 0.95f);

        [SerializeField]
        Vector2 splashOutSoundPitchRange = new Vector2(1f, 1.15f);

        [SerializeField]
        Vector3 splashOffset = new Vector3(0f,-1.1f,0f);

        [SerializeField]
        float splashSoundOutDelay = 0;

        [SerializeField]
        float heroOffset = 1.53f;

        //[SerializeField]
        //Vector3 extraSplashOffset = new Vector3(0, -1, 0);

        public bool PlayerInWater { get; private set; } = false;

        public float HeroSurfaceY => transform.position.y + heroOffset;

        private void OnEnable()
        {
            StopAllCoroutines();
            PlayerInWater = false;
            StartCoroutine(MainRoutine());
        }

        IEnumerator MainRoutine()
        {
            yield return new WaitForSeconds(0.5f);
            var player = Player.Player1;
            yield return new WaitUntil(() => GameCameras.instance != null && GameCameras.instance.hudCamera != null);
            var hudCamera = GameCameras.instance.hudCamera;

            while (true)
            {
                yield return new WaitUntil(() => PlayerInWater);

                if (HeroController.instance.GetState("willHardLand") || HeroController.instance.GetState("spellQuake"))
                {
                    //DO BIG SPLASH
                    CameraShaker.Instance.Shake(Enums.ShakeType.EnemyKillShake);
                    Player.Player1.transform.SetPositionY(HeroSurfaceY);
                    if (spatterParticles != null)
                    {
                        FlingUtilities.SpawnPooledAndFling(new FlingUtils.Config()
                        {
                            AmountMin = 60,
                            AmountMax = 60,
                            SpeedMin = 10f,
                            SpeedMax = 30f,
                            AngleMin = 80f,
                            AngleMax = 100f,
                            OriginVariationX = 1f,
                            OriginVariationY = 0f,
                            Prefab = spatterParticles
                        },Player.Player1.transform,spatterSpawnOffset);
                    }

                    if (splashSound != null)
                    {
                        //var soundInst = WeaverAudio.PlayAtPoint(splashSound, Player.Player1.transform.position);
                        var soundInst = WeaverAudio.Create(splashSound, Player.Player1.transform.position.With(y: HeroSurfaceY));
                        soundInst.AudioSource.pitch = splashInSoundPitchRange.RandomInRange();
                        soundInst.PlayDelayed(splashSoundInDelay);
                    }

                    if (splashInParticles != null)
                    {
                        var splash = Pooling.Instantiate(splashInParticles, Player.Player1.transform.position.With(y: HeroSurfaceY) + splashOffset, Quaternion.identity);
                        splash.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

                        if (splash.TryGetComponent<SpriteRenderer>(out var splashRenderer))
                        {
                            splashRenderer.color = splashColor;
                        }
                    }

                    if (splashOutParticles != null)
                    {
                        var splash = Pooling.Instantiate(splashOutParticles, Player.Player1.transform.position.With(y: HeroSurfaceY) + splashOffset, Quaternion.identity);
                        splash.transform.localScale = new Vector3(2, 2, 1f);
                    }
                }
                else
                {
                    //DO NORMAL SPLASH
                    Player.Player1.transform.SetPositionY(HeroSurfaceY);
                    if (spatterParticles != null)
                    {
                        FlingUtilities.SpawnPooledAndFling(new FlingUtils.Config()
                        {
                            AmountMin = 6,
                            AmountMax = 8,
                            SpeedMin = 7f,
                            SpeedMax = 15f,
                            AngleMin = 120f,
                            AngleMax = 145f,
                            OriginVariationX = 0f,
                            OriginVariationY = 0f,
                            Prefab = spatterParticles
                        }, Player.Player1.transform, spatterSpawnOffset);

                        FlingUtilities.SpawnPooledAndFling(new FlingUtils.Config()
                        {
                            AmountMin = 6,
                            AmountMax = 8,
                            SpeedMin = 7f,
                            SpeedMax = 15f,
                            AngleMin = 35f,
                            AngleMax = 60f,
                            OriginVariationX = 0f,
                            OriginVariationY = 0f,
                            Prefab = spatterParticles
                        }, Player.Player1.transform, spatterSpawnOffset);
                    }

                    if (splashSound != null)
                    {
                        //var soundInst = WeaverAudio.PlayAtPoint(splashSound, Player.Player1.transform.position);
                        //soundInst.AudioSource.pitch = splashOutSoundPitchRange.RandomInRange();
                        var soundInst = WeaverAudio.Create(splashSound, Player.Player1.transform.position);
                        soundInst.AudioSource.pitch = splashOutSoundPitchRange.RandomInRange();
                        soundInst.PlayDelayed(splashSoundOutDelay);
                    }

                    if (splashInParticles != null)
                    {
                        var splash = Pooling.Instantiate(splashInParticles, Player.Player1.transform.position.With(y: HeroSurfaceY) + splashOffset, Quaternion.identity);
                        splash.transform.localScale = new Vector3(1.75f, 1.75f, 1f);

                        if (splash.TryGetComponent<SpriteRenderer>(out var splashRenderer))
                        {
                            splashRenderer.color = splashColor;
                        }
                    }

                    /*if (splashOutParticles != null)
                    {
                        var splash = Pooling.Instantiate(splashOutParticles, Player.Player1.transform.position + new Vector3(0, 1, 0), Quaternion.identity);
                        splash.transform.localScale = new Vector3(2, 2, 1f);
                    }*/
                }

                EventManager.SendEventToGameObject("SURFACE ENTER", Player.Player1.gameObject, gameObject);
                var inventory = GameObject.Find("Inventory");

                if (inventory != null)
                {
                    EventManager.SendEventToGameObject("INVENTORY CANCEL", inventory, gameObject);
                }

                yield return new WaitUntil(() => !PlayerInWater);

                if (dripParticles != null)
                {
                    Pooling.Instantiate(dripParticles, Vector3.zero, Quaternion.identity);
                }

                if (splashSound != null)
                {
                    var soundInst = WeaverAudio.PlayAtPoint(splashSound, Player.Player1.transform.position.With(y: HeroSurfaceY) + splashOffset);
                    soundInst.AudioSource.pitch = splashOutSoundPitchRange.RandomInRange();
                }

                if (splashOutParticles != null)
                {
                    Pooling.Instantiate(splashOutParticles, Player.Player1.transform.position.With(y: HeroSurfaceY) + splashOffset, Quaternion.identity);
                }

                EventManager.SendEventToGameObject("SURFACE EXIT", Player.Player1.gameObject, gameObject);

            }

        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerInWater = true;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            PlayerInWater = false;
        }
    }
}