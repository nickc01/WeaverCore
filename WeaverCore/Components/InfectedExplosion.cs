using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using WeaverCore;
using WeaverCore.Assets.Components;
using WeaverCore.Attributes;
using WeaverCore.Enums;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Component used for controlling infected explosions and their effects.
    /// </summary>
    public class InfectedExplosion : MonoBehaviour, IOnPool
    {
        static CachedPrefab<InfectedExplosion> defaultPrefab = new CachedPrefab<InfectedExplosion>();

        [SerializeField]
        [Tooltip("The sound played when the explosion occurs.")]
        AudioClip ExplosionSound;

        [SerializeField]
        [Tooltip("The minimum pitch for the explosion sound.")]
        float explosionPitchMin = 0.85f;

        [SerializeField]
        [Tooltip("The maximum pitch for the explosion sound.")]
        float explosionPitchMax = 1.1f;

        [SerializeField]
        [Tooltip("The scale of the death wave.")]
        float deathWaveScale = 3f;

        [SerializeField]
        [Tooltip("Behavior when the explosion is done.")]
        OnDoneBehaviour whenDone = OnDoneBehaviour.DestroyOrPool;

        new Collider2D collider;

        /// <summary>
        /// Gets the default scale of the explosion.
        /// </summary>
        public float DefaultScale { get; private set; }

        [NonSerialized]
        [ExcludeFieldFromPool]
        ParticleSystem _particles;

        /// <summary>
        /// Gets the ParticleSystem component for the explosion.
        /// </summary>
        public ParticleSystem Particles
        {
            get
            {
                if (_particles == null)
                {
                    _particles = GetComponentInChildren<ParticleSystem>();
                }
                return _particles;
            }
        }

        void OnEnable()
        {
            if (collider == null)
            {
                collider = GetComponent<Collider2D>();
            }
            collider.enabled = true;
            if (ExplosionSound != null)
            {
                var audio = WeaverAudio.PlayAtPoint(ExplosionSound, transform.position);
                audio.AudioSource.pitch = UnityEngine.Random.Range(explosionPitchMin, explosionPitchMax);
            }

            CameraShaker.Instance.Shake(ShakeType.AverageShake);
            DeathWave.Spawn(transform.position, 0.5f);
            StartCoroutine(Waiter());
        }

        IEnumerator Waiter()
        {
            yield return new WaitForSeconds(0.5f);
            collider.enabled = false;
            yield return new WaitForSeconds(1f);
            whenDone.DoneWithObject(this);
        }

        /// <summary>
        /// Spawns an InfectedExplosion at the specified position
        /// </summary>
        /// <param name="position">The position to spawn the explosion.</param>
        /// <returns>The spawned InfectedExplosion instance.</returns>
        public static InfectedExplosion Spawn(Vector3 position)
        {
            return Spawn(position, 1f, null);
        }

        /// <summary>
        /// Spawns an InfectedExplosion at the specified position with a specified prefab.
        /// </summary>
        /// <param name="position">The position to spawn the explosion.</param>
        /// <param name="prefab">The prefab to use for spawning.</param>
        /// <returns>The spawned InfectedExplosion instance.</returns>
        public static InfectedExplosion Spawn(Vector3 position, InfectedExplosion prefab)
        {
            return Spawn(position, 1f, prefab);
        }

        /// <summary>
        /// Spawns an InfectedExplosion at the specified position with a specified scale.
        /// </summary>
        /// <param name="position">The position to spawn the explosion.</param>
        /// <param name="scale">The scale of the explosion.</param>
        /// <returns>The spawned InfectedExplosion instance.</returns>
        public static InfectedExplosion Spawn(Vector3 position, float scale)
        {
            return Spawn(position, scale, null);
        }

        /// <summary>
        /// Spawns an InfectedExplosion at the specified position with a specified scale and prefab.
        /// </summary>
        /// <param name="position">The position to spawn the explosion.</param>
        /// <param name="scale">The scale of the explosion.</param>
        /// <param name="prefab">The prefab to use for spawning.</param>
        /// <returns>The spawned InfectedExplosion instance.</returns>
        public static InfectedExplosion Spawn(Vector3 position, float scale, InfectedExplosion prefab)
        {
            if (prefab == null)
            {
                if (defaultPrefab.Value == null)
                {
                    defaultPrefab.Value = WeaverAssets.LoadWeaverAsset<GameObject>("Infected Explosion").GetComponent<InfectedExplosion>();
                }
                prefab = defaultPrefab.Value;
            }
            var instance = Pooling.Instantiate(prefab, position, Quaternion.identity);

            instance.DefaultScale = instance.transform.GetXLocalScale();

            instance.transform.SetLocalScaleXY(scale, scale);

            var emission = instance.Particles.emission;
            emission.rateOverTimeMultiplier = 1000f * scale;

            return instance;
        }

        void IOnPool.OnPool()
        {
            transform.SetLocalScaleXY(DefaultScale, DefaultScale);
            var emission = Particles.emission;

            emission.rateOverTimeMultiplier = 1000f;
        }
    }
}