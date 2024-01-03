using GlobalEnums;
using UnityEngine;
using WeaverCore.Assets;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components.HitEffects
{
    /// <summary>
    /// Handles hit effects for a ghost character.
    /// </summary>
    public class HitEffectsGhost : MonoBehaviour, IHitEffects
    {
        [SerializeField]
        [Tooltip("Sound played when the ghost is hit.")]
        AudioClip hitSound;

        [SerializeField]
        [Tooltip("Pitch range for the hit sound.")]
        Vector2 hitSoundPitchRange = new Vector2(0.75f, 1.25f);

        [SerializeField]
        [Tooltip("Volume of the hit sound.")]
        float hitSoundVolume = 1f;

        [Space]
        [Tooltip("Prefab for ghost hit particles.")]
        public GameObject ghostHitPt;

        [Tooltip("Prefab for ghost slash effect 1.")]
        public GameObject slashEffectGhost1;

        [Tooltip("Prefab for ghost slash effect 2.")]
        public GameObject slashEffectGhost2;

        private SpriteFlasher spriteFlash;

        private bool didFireThisFrame;

        protected virtual void Awake()
        {
            spriteFlash = GetComponent<SpriteFlasher>();
        }

        protected virtual void Update()
        {
            didFireThisFrame = false;
        }

        protected virtual Reset()
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

        /// <summary>
        /// Spawns and plays hit effects based on the hit information.
        /// </summary>
        /// <param name="hit">Information about the hit.</param>
        /// <param name="effectsOffset">Offset for hit effects.</param>
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
                GameObject ghostHitObj = Pooling.Instantiate(ghostHitPt, transform.position + effectsOffset, Quaternion.identity);
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
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectsOffset);
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
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectsOffset);
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
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectsOffset);
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
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectsOffset);
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
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectsOffset);
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
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectsOffset);
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
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectsOffset);
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
                            FlingUtilities.SpawnPooledAndFling(config, transform, effectsOffset);
                            break;
                        }
                }
                didFireThisFrame = true;
            }
        }
    }
}
