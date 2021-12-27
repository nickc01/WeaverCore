using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Components.DeathEffects
{
    /// <summary>
    /// The death effects for infected enemies
    /// </summary>
    public class InfectedDeathEffects : BasicDeathEffects
    {
        [Tooltip("Defines how large the effects should be")]
        public InfectedDeathType DeathType;

        [SerializeField]
        protected GameObject InfectedDeathWavePrefab;

        [SerializeField]
        protected GameObject DeathPuffPrefab;

        [SerializeField]
        protected AudioClip DamageSound;

        [SerializeField]
        protected float damageSoundVolume;

        [SerializeField]
        protected float damageSoundMinPitch;

        [SerializeField]
        protected float damageSoundMaxPitch;

        [SerializeField]
        protected AudioClip SwordDeathSound;

        [SerializeField]
        protected float swordDeathSoundVolume;

        [SerializeField]
        protected float swordDeathSoundMinPitch;

        [SerializeField]
        protected float swordDeathSoundMaxPitch;

        /// <inheritdoc/>
        public override void EmitEffects()
        {
            if (DeathType != InfectedDeathType.SmallInfected)
            {
                if (DeathType != InfectedDeathType.LargeInfected)
                {
                    if (DeathType != InfectedDeathType.Infected)
                    {
                        Debug.LogWarningFormat(this, "Enemy death type {0} not implemented!", new object[]
                        {
                            DeathType
                        });
                    }
                    else
                    {
                        EmitInfectedEffects();
                    }
                }
                else
                {
                    EmitLargeInfectedEffects();
                }
            }
            else
            {
                EmitSmallInfectedEffects();
            }
        }

        /// <inheritdoc/>
        public override void EmitSounds()
        {
            base.EmitSounds();
            if (SwordDeathSound != null)
            {
                AudioPlayer weaverAudioPlayer = WeaverAudio.PlayAtPoint(SwordDeathSound, transform.position, swordDeathSoundVolume, AudioChannel.Sound);
                weaverAudioPlayer.AudioSource.pitch = UnityEngine.Random.Range(swordDeathSoundMinPitch, swordDeathSoundMaxPitch);
            }
            if (DamageSound != null)
            {
                AudioPlayer weaverAudioPlayer2 = WeaverAudio.PlayAtPoint(DamageSound, transform.position, damageSoundVolume, AudioChannel.Sound);
                weaverAudioPlayer2.AudioSource.pitch = UnityEngine.Random.Range(damageSoundMinPitch, damageSoundMaxPitch);
            }
        }

        private void EmitInfectedEffects()
        {
            EmitSounds();
            if (InfectedDeathWavePrefab != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(InfectedDeathWavePrefab, transform.position + EffectsOffset, Quaternion.identity);
            }
            gameObject.transform.SetXLocalScale(1.25f);
            gameObject.transform.SetYLocalScale(1.25f);
            Blood.SpawnRandomBlood(transform.position + EffectsOffset);
            if (DeathPuffPrefab != null)
            {
                UnityEngine.Object.Instantiate<GameObject>(DeathPuffPrefab, transform.position + EffectsOffset, Quaternion.identity);
            }
            ShakeCamera(ShakeType.EnemyKillShake);
        }

        private void ShakeCamera(ShakeType shakeType = ShakeType.EnemyKillShake)
        {
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null)
            {
                renderer = base.GetComponentInChildren<Renderer>();
            }
            if (renderer != null && renderer.isVisible)
            {
                CameraShaker.Instance.Shake(shakeType);
            }
        }

        private void EmitSmallInfectedEffects()
        {
            EmitSounds();
            if (InfectedDeathWavePrefab != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(InfectedDeathWavePrefab, transform.position + EffectsOffset, Quaternion.identity);
                Vector3 localScale = gameObject.transform.localScale;
                localScale.x = 0.5f;
                localScale.y = 0.5f;
                gameObject.transform.localScale = localScale;
            }
            Blood.SpawnRandomBlood(transform.position + EffectsOffset);
        }

        private void EmitLargeInfectedEffects()
        {
            EmitSounds();
            if (DeathPuffPrefab != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(DeathPuffPrefab, transform.position + EffectsOffset, Quaternion.identity);
                gameObject.transform.localScale = new Vector3(2f, 2f, gameObject.transform.GetZLocalScale());
            }
            ShakeCamera(ShakeType.AverageShake);
            if (InfectedDeathWavePrefab != null)
            {
                GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(InfectedDeathWavePrefab, transform.position + EffectsOffset, Quaternion.identity);
                gameObject2.transform.SetXLocalScale(2f);
                gameObject2.transform.SetYLocalScale(2f);
            }
            Blood.SpawnRandomBlood(transform.position + EffectsOffset);
        }

        public enum InfectedDeathType
        {
            Infected,
            SmallInfected,
            LargeInfected
        }
    }
}
