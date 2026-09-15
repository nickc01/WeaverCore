using System;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
    /// <summary>
    /// A configurable implementation of <see cref="CustomEnvironmentType"/>.
    /// </summary>
    [CreateAssetMenu(fileName = "Basic Environment Type", menuName = "WeaverCore/Basic Environment Type")]
    public class BasicEnvironmentType : CustomEnvironmentType
    {
        public enum FootstepClipMode
        {
            None,
            CustomClips,
            BuiltIn
        }

        public enum BuiltInFootstepType
        {
            Dust,
            Grass,
            Bone,
            Spa,
            Metal,
            Water,
            Sand
        }

        [Serializable]
        public class SpawnedEffectSettings
        {
            public GameObject Prefab;
            public Vector3 PositionOffset;
            public Vector3 Rotation;
            public bool ParentToEffect;
            public bool RandomFlipX;
        }

        [Serializable]
        public class DashEffectSettings
        {
            public bool RequireHeroInPosition = true;
            public float RecycleTime = 1f;
            public bool PlayDashPuff = true;
            public bool PlayDust;
            public bool PlayGrass;
            public bool PlayBone;
            public bool PlayWaterCut;
            public bool UseBuiltInSplashClip;
            public AudioClip OneShotClip;
            public SpawnedEffectSettings ExtraEffect;
        }

        [Serializable]
        public class JumpEffectSettings
        {
            public float RecycleTime = 1f;
            public bool PlayDust;
            public bool PlayGrass;
            public bool PlayBone;
            public bool PlaySplash;
            public bool RandomFlipSplash = true;
            public bool PlayJumpPuff = true;
            public bool PlayTrail = true;
            public bool EnableFallCheck = true;
            public bool SplashOut;
            public SpawnedEffectSettings ExtraEffect;
        }

        [Serializable]
        public class SoftLandEffectSettings
        {
            public bool RequireHeroInPosition = true;
            public float RecycleTime = 1f;
            public bool PlayDust;
            public bool PlayGrass;
            public bool PlayBone;
            public bool PlaySplash;
            public bool RandomFlipSplash = true;
            public bool UseBuiltInSoftLandClip;
            public bool UseBuiltInWetLandClip;
            public AudioClip OneShotClip;
            public SpawnedEffectSettings ExtraEffect;
        }

        [Serializable]
        public class HardLandEffectSettings
        {
            public bool PlayAverageShake = true;
            public float RecycleTime = 1.5f;
            public bool PlayDust;
            public bool PlayGrass;
            public bool PlayBone;
            public bool PlaySpa;
            public bool PlayMetal;
            public bool PlayWet;
            public bool SpawnRockParticles;
            public GameObject RockParticlePrefabOverride;
            public bool SpawnSpaSpatter;
            public GameObject SpaSpatterPrefabOverride;
            public SpawnedEffectSettings ExtraEffect;
        }

        [Serializable]
        public class FallTrailSettings
        {
            public bool PlayBuiltInEffect = true;
            public SpawnedEffectSettings ExtraEffect;
        }

        [Header("Footsteps")]
        public FootstepClipMode FootstepMode = FootstepClipMode.BuiltIn;
        public BuiltInFootstepType BuiltInFootsteps = BuiltInFootstepType.Dust;
        public AudioClip RunFootstepClip;
        public AudioClip WalkFootstepClip;

        [Header("Dash")]
        public DashEffectSettings Dash = new DashEffectSettings();

        [Header("Jump")]
        public JumpEffectSettings Jump = new JumpEffectSettings();

        [Header("Soft Land")]
        public SoftLandEffectSettings SoftLand = new SoftLandEffectSettings();

        [Header("Hard Land")]
        public HardLandEffectSettings HardLand = new HardLandEffectSettings();

        [Header("Fall Trail")]
        public FallTrailSettings FallTrail = new FallTrailSettings();

        public override void ApplyFootstepSounds(AudioSource footStepsRunAudioSource, AudioSource footStepsWalkAudioSource)
        {
            switch (FootstepMode)
            {
                case FootstepClipMode.None:
                    if (footStepsRunAudioSource != null)
                    {
                        footStepsRunAudioSource.clip = null;
                    }
                    if (footStepsWalkAudioSource != null)
                    {
                        footStepsWalkAudioSource.clip = null;
                    }
                    break;
                case FootstepClipMode.BuiltIn:
                    ApplyBuiltInFootstepSounds(footStepsRunAudioSource, footStepsWalkAudioSource);
                    break;
                default:
                    if (footStepsRunAudioSource != null)
                    {
                        footStepsRunAudioSource.clip = RunFootstepClip;
                    }

                    if (footStepsWalkAudioSource != null)
                    {
                        footStepsWalkAudioSource.clip = WalkFootstepClip;
                    }
                    break;
            }
        }

        public override void ApplyDashEffect(DashEffectContext dashEffectContext)
        {
            dashEffectContext.SetRecycleTimer(Dash.RecycleTime);

            if (Dash.RequireHeroInPosition && !dashEffectContext.IsHeroInPosition)
            {
                return;
            }

            if (Dash.PlayDashPuff)
            {
                dashEffectContext.PlayDashPuff();
            }
            if (Dash.PlayDust)
            {
                dashEffectContext.PlayDust();
            }
            if (Dash.PlayGrass)
            {
                dashEffectContext.PlayGrass();
            }
            if (Dash.PlayBone)
            {
                dashEffectContext.PlayBone();
            }
            if (Dash.PlayWaterCut)
            {
                dashEffectContext.PlayWaterCut();
            }
            if (Dash.UseBuiltInSplashClip)
            {
                dashEffectContext.PlayOneShot(dashEffectContext.SplashClip);
            }
            if (Dash.OneShotClip != null)
            {
                dashEffectContext.PlayOneShot(Dash.OneShotClip);
            }

            SpawnExtraEffect(dashEffectContext, Dash.ExtraEffect);
        }

        public override void ApplyJumpEffect(JumpEffectContext jumpEffectContext)
        {
            jumpEffectContext.SetRecycleTimer(Jump.RecycleTime);

            if (Jump.PlayDust)
            {
                jumpEffectContext.PlayDust();
            }
            if (Jump.PlayGrass)
            {
                jumpEffectContext.PlayGrass();
            }
            if (Jump.PlayBone)
            {
                jumpEffectContext.PlayBone();
            }
            if (Jump.PlaySplash)
            {
                jumpEffectContext.PlaySplash(Jump.RandomFlipSplash);
            }
            if (Jump.EnableFallCheck)
            {
                jumpEffectContext.EnableFallCheck();
            }
            if (Jump.PlayJumpPuff)
            {
                jumpEffectContext.PlayJumpPuff();
            }
            if (Jump.PlayTrail)
            {
                jumpEffectContext.PlayTrail();
            }
            if (Jump.SplashOut)
            {
                jumpEffectContext.SplashOut();
            }

            SpawnExtraEffect(jumpEffectContext, Jump.ExtraEffect);
        }

        public override void ApplySoftLandEffect(SoftLandEffectContext softLandEffectContext)
        {
            softLandEffectContext.SetRecycleTimer(SoftLand.RecycleTime);

            if (SoftLand.RequireHeroInPosition && !softLandEffectContext.IsHeroInPosition)
            {
                return;
            }

            if (SoftLand.PlayDust)
            {
                softLandEffectContext.PlayDust();
            }
            if (SoftLand.PlayGrass)
            {
                softLandEffectContext.PlayGrass();
            }
            if (SoftLand.PlayBone)
            {
                softLandEffectContext.PlayBone();
            }
            if (SoftLand.PlaySplash)
            {
                softLandEffectContext.PlaySplash(SoftLand.RandomFlipSplash);
            }
            if (SoftLand.UseBuiltInSoftLandClip)
            {
                softLandEffectContext.PlayOneShot(softLandEffectContext.SoftLandClip);
            }
            if (SoftLand.UseBuiltInWetLandClip)
            {
                softLandEffectContext.PlayOneShot(softLandEffectContext.WetLandClip);
            }
            if (SoftLand.OneShotClip != null)
            {
                softLandEffectContext.PlayOneShot(SoftLand.OneShotClip);
            }

            SpawnExtraEffect(softLandEffectContext, SoftLand.ExtraEffect);
        }

        public override void ApplyHardLandEffect(HardLandEffectContext hardLandEffectContext)
        {
            hardLandEffectContext.SetRecycleTime(HardLand.RecycleTime);

            if (HardLand.PlayAverageShake)
            {
                hardLandEffectContext.PlayAverageShake();
            }

            if (HardLand.PlayDust)
            {
                hardLandEffectContext.PlayDust();
            }
            if (HardLand.PlayGrass)
            {
                hardLandEffectContext.PlayGrass();
            }
            if (HardLand.PlayBone)
            {
                hardLandEffectContext.PlayBone();
            }
            if (HardLand.PlaySpa)
            {
                hardLandEffectContext.PlaySpa();
            }
            if (HardLand.PlayMetal)
            {
                hardLandEffectContext.PlayMetal();
            }
            if (HardLand.PlayWet)
            {
                hardLandEffectContext.PlayWet();
            }
            if (HardLand.SpawnRockParticles)
            {
                hardLandEffectContext.SpawnRockParticles(HardLand.RockParticlePrefabOverride);
            }
            if (HardLand.SpawnSpaSpatter)
            {
                hardLandEffectContext.SpawnSpaSpatter(HardLand.SpaSpatterPrefabOverride);
            }

            SpawnExtraEffect(hardLandEffectContext, HardLand.ExtraEffect);
        }

        public override void ApplyFallTrail(FallTrailContext fallTrailContext)
        {
            if (FallTrail.PlayBuiltInEffect)
            {
                fallTrailContext.PlayBuiltInFallTrail();
            }

            SpawnExtraEffect(fallTrailContext, FallTrail.ExtraEffect);
        }

        void ApplyBuiltInFootstepSounds(AudioSource footStepsRunAudioSource, AudioSource footStepsWalkAudioSource)
        {
            var heroController = HeroController.instance;
            if (heroController == null)
            {
                heroController = footStepsRunAudioSource != null ? footStepsRunAudioSource.GetComponentInParent<HeroController>() : null;
            }
            if (heroController == null)
            {
                heroController = footStepsWalkAudioSource != null ? footStepsWalkAudioSource.GetComponentInParent<HeroController>() : null;
            }

            if (heroController == null)
            {
                return;
            }

            switch (BuiltInFootsteps)
            {
                case BuiltInFootstepType.Dust:
                    SetFootstepSounds(footStepsRunAudioSource, heroController.footstepsRunDust, footStepsWalkAudioSource, heroController.footstepsWalkDust);
                    break;
                case BuiltInFootstepType.Grass:
                    SetFootstepSounds(footStepsRunAudioSource, heroController.footstepsRunGrass, footStepsWalkAudioSource, heroController.footstepsWalkGrass);
                    break;
                case BuiltInFootstepType.Bone:
                    SetFootstepSounds(footStepsRunAudioSource, heroController.footstepsRunBone, footStepsWalkAudioSource, heroController.footstepsWalkBone);
                    break;
                case BuiltInFootstepType.Spa:
                    SetFootstepSounds(footStepsRunAudioSource, heroController.footstepsRunSpa, footStepsWalkAudioSource, heroController.footstepsWalkSpa);
                    break;
                case BuiltInFootstepType.Metal:
                    SetFootstepSounds(footStepsRunAudioSource, heroController.footstepsRunMetal, footStepsWalkAudioSource, heroController.footstepsWalkMetal);
                    break;
                case BuiltInFootstepType.Water:
                    SetFootstepSounds(footStepsRunAudioSource, heroController.footstepsRunWater, footStepsWalkAudioSource, heroController.footstepsRunWater);
                    break;
                case BuiltInFootstepType.Sand:
                    SetFootstepSounds(footStepsRunAudioSource, heroController.footstepsRunGrass, footStepsWalkAudioSource, heroController.footstepsWalkGrass);
                    break;
            }
        }

        static void SetFootstepSounds(AudioSource runSource, AudioClip runClip, AudioSource walkSource, AudioClip walkClip)
        {
            if (runSource != null)
            {
                runSource.clip = runClip;
            }

            if (walkSource != null)
            {
                walkSource.clip = walkClip;
            }
        }

        static void SpawnExtraEffect(EnvironmentEffectContext effectContext, SpawnedEffectSettings settings)
        {
            if (effectContext == null || settings == null || settings.Prefab == null)
            {
                return;
            }

            effectContext.SpawnPrefab(settings.Prefab, settings.PositionOffset, settings.Rotation, settings.ParentToEffect, settings.RandomFlipX);
        }

        static void SpawnExtraEffect(FallTrailContext fallTrailContext, SpawnedEffectSettings settings)
        {
            if (fallTrailContext == null || settings == null || settings.Prefab == null)
            {
                return;
            }

            fallTrailContext.SpawnPrefab(settings.Prefab, settings.PositionOffset, settings.Rotation, settings.ParentToEffect, settings.RandomFlipX);
        }
    }
}
