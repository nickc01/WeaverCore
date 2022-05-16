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
        [Header("Infected Config")]
        [Tooltip("Defines how large the effects should be")]
        public InfectedDeathType DeathType = InfectedDeathType.Infected;

        [SerializeField]
        protected GameObject InfectedDeathWavePrefab;

        [SerializeField]
        protected GameObject DeathPuffPrefab;

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
        }

        private void EmitInfectedEffects()
        {
            EmitSounds();
            if (InfectedDeathWavePrefab != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(InfectedDeathWavePrefab, transform.position + EffectsOffset, Quaternion.identity);
                gameObject.transform.SetXLocalScale(1.25f);
                gameObject.transform.SetYLocalScale(1.25f);
            }
            Blood.SpawnRandomBlood(transform.position + EffectsOffset);
            if (DeathPuffPrefab != null)
            {
                UnityEngine.Object.Instantiate<GameObject>(DeathPuffPrefab, transform.position + EffectsOffset, Quaternion.identity);
            }
            ShakeCameraIfVisible(ShakeType.EnemyKillShake);
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
            ShakeCameraIfVisible(ShakeType.AverageShake);
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
