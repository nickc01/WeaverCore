using System;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    /// <summary>
    /// Manages rock particle effects.
    /// </summary>
    public class RockParticles : MonoBehaviour
    {
        /// <summary>
        /// The prefab instance of RockParticles.
        /// </summary>
        static RockParticles _prefab;

        /// <summary>
        /// Gets the prefab instance of RockParticles.
        /// </summary>
        public static RockParticles Prefab => _prefab ??= WeaverAssets.LoadWeaverAsset<GameObject>("Rock Particles").GetComponent<RockParticles>();

        [NonSerialized]
        ParticleSystem _particles;

        /// <summary>
        /// Gets the ParticleSystem component.
        /// </summary>
        public ParticleSystem Particles => _particles ??= GetComponent<ParticleSystem>();

        /// <summary>
        /// Spawns directional rock particles.
        /// </summary>
        /// <param name="position">The position to spawn the particles.</param>
        /// <param name="rotation">The rotation of the particles.</param>
        /// <param name="intensity">The intensity of the particles.</param>
        /// <param name="duration">The duration of the particle effect.</param>
        /// <param name="particleSize">The size of the particles.</param>
        /// <param name="playImmediately">Whether to play the particles immediately.</param>
        /// <returns>The instance of RockParticles.</returns>
        public static RockParticles SpawnDirectional(Vector3 position, Quaternion rotation, float intensity = 100, float duration = 0.1f, float particleSize = 1f, bool playImmediately = true)
        {
            return Spawn(position, rotation, 0f, intensity, duration, particleSize, playImmediately);
        }

        /// <summary>
        /// Spawns non-directional rock particles.
        /// </summary>
        /// <param name="position">The position to spawn the particles.</param>
        /// <param name="intensity">The intensity of the particles.</param>
        /// <param name="duration">The duration of the particle effect.</param>
        /// <param name="particleSize">The size of the particles.</param>
        /// <param name="playImmediately">Whether to play the particles immediately.</param>
        /// <returns>The instance of RockParticles.</returns>
        public static RockParticles SpawnNonDirectional(Vector3 position, float intensity = 100, float duration = 0.1f, float particleSize = 1f, bool playImmediately = true)
        {
            return Spawn(position, Quaternion.identity, 1f, intensity, duration, particleSize, playImmediately);
        }

        /// <summary>
        /// Spawns rock particles.
        /// </summary>
        /// <param name="position">The position to spawn the particles.</param>
        /// <param name="rotation">The rotation of the particles.</param>
        /// <param name="directionRandomness">The randomness of the particle direction.</param>
        /// <param name="intensity">The intensity of the particles.</param>
        /// <param name="duration">The duration of the particle effect.</param>
        /// <param name="particleSize">The size of the particles.</param>
        /// <param name="playImmediately">Whether to play the particles immediately.</param>
        /// <returns>The instance of RockParticles.</returns>
        public static RockParticles Spawn(Vector3 position, Quaternion rotation, float directionRandomness, float intensity = 100, float duration = 0.1f, float particleSize = 1f, bool playImmediately = true)
        {
            var instance = Pooling.Instantiate(Prefab, position, rotation);

            var emit = instance.Particles.emission;
            var main = instance.Particles.main;
            var shape = instance.Particles.shape;
            var size = instance.Particles.sizeOverLifetime;

            emit.rateOverTimeMultiplier = intensity;
            size.sizeMultiplier = particleSize;
            shape.randomDirectionAmount = directionRandomness;
            main.duration = duration;

            if (playImmediately)
            {
                instance.Particles.Play();
            }

            return instance;
        }
    }
}
