using GlobalEnums;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components.HitEffects
{
    public class HitEffectsGhost : MonoBehaviour, IHitEffects
    {
        public Vector3 effectOffset;

        [SerializeField]
        AudioClip hitSound;

        [SerializeField]
        Vector2 hitSoundPitchRange = new Vector2(0.75f, 1.25f);

        [SerializeField]
        float hitSoundVolume = 1f;

        [Space]
        public GameObject ghostHitPt;

        public GameObject slashEffectGhost1;

        public GameObject slashEffectGhost2;

        private SpriteFlasher spriteFlash;

        private bool didFireThisFrame;

        protected void Awake()
        {
            spriteFlash = GetComponent<SpriteFlasher>();
        }

        protected void Update()
        {
            didFireThisFrame = false;
        }

        private void Reset()
        {
            ghostHitPt = WeaverAssets.LoadWeaverAsset<GameObject>("Ghost Hit Pt");

            slashEffectGhost1 = EffectAssets.SlashGhost1Prefab;

            slashEffectGhost2 = EffectAssets.SlashGhost2Prefab;

            hitSound = WeaverAssets.LoadWeaverAsset<AudioClip>("Dream Damage");
        }

        static GameObject PooledSpawn(GameObject prefab, Vector3 position)
        {
            return Pooling.Instantiate(prefab, position, Quaternion.identity);
        }

        public void PlayHitEffect(HitInfo hit, Vector3 effectsOffset = default)
        {
            if (!didFireThisFrame)
            {
                EventManager.SendEventToGameObject("DAMAGE FLASH", gameObject);
                //enemyDamage.SpawnAndPlayOneShot(audioPlayerPrefab, transform.position);

                if (hitSound != null)
                {
                    var hitSoundInst = WeaverAudio.PlayAtPoint(hitSound, transform.position, hitSoundVolume);
                    hitSoundInst.AudioSource.pitch = hitSoundPitchRange.RandomInRange();
                }

                if (spriteFlash != null)
                {
                    spriteFlash.flashFocusHeal();
                }
                GameObject ghostHitObj = Pooling.Instantiate(ghostHitPt, transform.position + effectOffset, Quaternion.identity);
                switch (DirectionUtilities.DegreesToDirection(hit.Direction))
                {
                    case CardinalDirection.Right:
                        {
                            ghostHitObj.transform.SetRotation2D(-22.5f);
                            FlingUtils.Config config = new FlingUtils.Config
                            {
                                Prefab = slashEffectGhost1,
                                AmountMin = 2,
                                AmountMax = 3,
                                SpeedMin = 20f,
                                SpeedMax = 35f,
                                AngleMin = -40f,
                                AngleMax = 40f,
                                OriginVariationX = 0f,
                                OriginVariationY = 0f
                            };
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectOffset);
                            config = new FlingUtils.Config
                            {
                                Prefab = slashEffectGhost2,
                                AmountMin = 2,
                                AmountMax = 3,
                                SpeedMin = 20f,
                                SpeedMax = 35f,
                                AngleMin = -40f,
                                AngleMax = 40f,
                                OriginVariationX = 0f,
                                OriginVariationY = 0f
                            };
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectOffset);
                            break;
                        }
                    case CardinalDirection.Left:
                        {
                            ghostHitObj.transform.SetRotation2D(160f);
                            FlingUtils.Config config = new FlingUtils.Config
                            {
                                Prefab = slashEffectGhost1,
                                AmountMin = 2,
                                AmountMax = 3,
                                SpeedMin = 20f,
                                SpeedMax = 35f,
                                AngleMin = 140f,
                                AngleMax = 220f,
                                OriginVariationX = 0f,
                                OriginVariationY = 0f
                            };
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectOffset);
                            config = new FlingUtils.Config
                            {
                                Prefab = slashEffectGhost2,
                                AmountMin = 2,
                                AmountMax = 3,
                                SpeedMin = 20f,
                                SpeedMax = 35f,
                                AngleMin = 140f,
                                AngleMax = 220f,
                                OriginVariationX = 0f,
                                OriginVariationY = 0f
                            };
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectOffset);
                            break;
                        }
                    case CardinalDirection.Up:
                        {
                            ghostHitObj.transform.SetRotation2D(70f);
                            FlingUtils.Config config = new FlingUtils.Config
                            {
                                Prefab = slashEffectGhost1,
                                AmountMin = 2,
                                AmountMax = 3,
                                SpeedMin = 20f,
                                SpeedMax = 35f,
                                AngleMin = 50f,
                                AngleMax = 130f,
                                OriginVariationX = 0f,
                                OriginVariationY = 0f
                            };
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectOffset);
                            config = new FlingUtils.Config
                            {
                                Prefab = slashEffectGhost2,
                                AmountMin = 2,
                                AmountMax = 3,
                                SpeedMin = 20f,
                                SpeedMax = 35f,
                                AngleMin = 50f,
                                AngleMax = 130f,
                                OriginVariationX = 0f,
                                OriginVariationY = 0f
                            };
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectOffset);
                            break;
                        }
                    case CardinalDirection.Down:
                        {
                            ghostHitObj.transform.SetRotation2D(-110f);
                            FlingUtils.Config config = new FlingUtils.Config
                            {
                                Prefab = slashEffectGhost1,
                                AmountMin = 2,
                                AmountMax = 3,
                                SpeedMin = 20f,
                                SpeedMax = 35f,
                                AngleMin = 230f,
                                AngleMax = 310f,
                                OriginVariationX = 0f,
                                OriginVariationY = 0f
                            };
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectOffset);
                            config = new FlingUtils.Config
                            {
                                Prefab = slashEffectGhost2,
                                AmountMin = 2,
                                AmountMax = 3,
                                SpeedMin = 20f,
                                SpeedMax = 35f,
                                AngleMin = 230f,
                                AngleMax = 310f,
                                OriginVariationX = 0f,
                                OriginVariationY = 0f
                            };
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectOffset);
                            break;
                        }
                }
                didFireThisFrame = true;
            }
        }
    }
}
