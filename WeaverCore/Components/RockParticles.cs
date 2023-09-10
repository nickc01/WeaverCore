using System;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class RockParticles : MonoBehaviour
    {
        static RockParticles _prefab;

        public static RockParticles Prefab => _prefab ??= WeaverAssets.LoadWeaverAsset<GameObject>("Rock Particles").GetComponent<RockParticles>();

        [NonSerialized]
        ParticleSystem _particles;

        public ParticleSystem Particles => _particles ??= GetComponent<ParticleSystem>();

        public static RockParticles SpawnDirectional(Vector3 position, Quaternion rotation, float intensity = 100, float duration = 0.1f, float particleSize = 1f, bool playImmediately = true)
        {
            return Spawn(position, rotation, 0f, intensity, duration, particleSize, playImmediately);
        }

        public static RockParticles SpawnNonDirectional(Vector3 position, float intensity = 100, float duration = 0.1f, float particleSize = 1f, bool playImmediately = true)
        {
            return Spawn(position, Quaternion.identity, 1f, intensity, duration, particleSize, playImmediately);
        }

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
