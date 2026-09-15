using GlobalEnums;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using WeaverCore.Attributes;
using WeaverCore.Utilities;

namespace WeaverCore.Features
{
    /// <summary>
    /// Used to create custom environment types.
    /// </summary>
    [ShowFeature]
    public abstract class CustomEnvironmentType : ScriptableObject
    {
        /// <summary>
        /// The name shown in editor selectors.
        /// </summary>
        public string EnvironmentName;

        /// <summary>
        /// The environment type ID written to scene managers and enviro regions.
        /// </summary>
        public int EnvironmentID;

        /// <summary>
        /// Applies custom footstep sounds for the environment.
        /// </summary>
        public abstract void ApplyFootstepSounds(AudioSource footStepsRunAudioSource, AudioSource footStepsWalkAudioSource);

        /// <summary>
        /// Applies custom dash effects for the environment.
        /// </summary>
        public abstract void ApplyDashEffect(DashEffectContext dashEffectContext);

        /// <summary>
        /// Applies custom jump effects for the environment.
        /// </summary>
        public abstract void ApplyJumpEffect(JumpEffectContext jumpEffectContext);

        /// <summary>
        /// Applies custom soft landing effects for the environment.
        /// </summary>
        public abstract void ApplySoftLandEffect(SoftLandEffectContext softLandEffectContext);

        /// <summary>
        /// Applies custom hard landing effects for the environment.
        /// </summary>
        public abstract void ApplyHardLandEffect(HardLandEffectContext hardLandEffectContext);

        /// <summary>
        /// Applies custom fall trail behavior when the hero leaves the ground.
        /// </summary>
        public abstract void ApplyFallTrail(FallTrailContext fallTrailContext);

        public string GetDisplayName()
        {
            if (string.IsNullOrWhiteSpace(EnvironmentName))
            {
                return $"Environment {EnvironmentID}";
            }

            return EnvironmentName;
        }

        [OnHarmonyPatch]
        static void OnHarmonyPatch(HarmonyPatcher patcher)
        {
            PatchHeroController(patcher);
            PatchEffectType(patcher, "DashEffect", nameof(DashEffect_OnEnable_Prefix));
            PatchEffectType(patcher, "JumpEffects", nameof(JumpEffects_OnEnable_Prefix));
            PatchEffectType(patcher, "SoftLandEffect", nameof(SoftLandEffect_OnEnable_Prefix));
            PatchEffectType(patcher, "HardLandEffect", nameof(HardLandEffect_OnEnable_Prefix));
        }

        static void PatchHeroController(HarmonyPatcher patcher)
        {
            {
                var original = typeof(HeroController).GetMethod(nameof(HeroController.checkEnvironment), BindingFlags.Public | BindingFlags.Instance);
                var prefix = typeof(CustomEnvironmentType).GetMethod(nameof(CheckEnvironment_Prefix), BindingFlags.NonPublic | BindingFlags.Static);

                if (original != null && prefix != null)
                {
                    patcher.Patch(original, prefix, null);
                }
            }

            {
                var original = typeof(HeroController).GetMethod("OnCollisionExit2D", BindingFlags.NonPublic | BindingFlags.Instance);
                var prefix = typeof(CustomEnvironmentType).GetMethod(nameof(OnCollisionExit2D_Prefix), BindingFlags.NonPublic | BindingFlags.Static);

                if (original != null && prefix != null)
                {
                    patcher.Patch(original, prefix, null);
                }
            }
        }

        static bool IsCustomFallTrailScenario(HeroController heroController, Collision2D collision, bool fallTrailGenerated)
        {
            if (heroController == null || collision == null || heroController.playerData == null)
            {
                return false;
            }

            if (heroController.hero_state == ActorStates.no_input || heroController.cState == null || heroController.cState.recoiling)
            {
                return false;
            }

            if (collision.gameObject.layer != 8)
            {
                return false;
            }

            var touchingGround = heroController.ReflectCallMethod("CheckTouchingGround");
            if (touchingGround is bool isTouchingGround && isTouchingGround)
            {
                return false;
            }

            return !heroController.cState.jumping && !fallTrailGenerated && heroController.cState.onGround;
        }

        static void PatchEffectType(HarmonyPatcher patcher, string typeName, string prefixName)
        {
            var effectType = ResolveHollowKnightType(typeName);
            if (effectType == null)
            {
                return;
            }

            var original = effectType.GetMethod("OnEnable", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var prefix = typeof(CustomEnvironmentType).GetMethod(prefixName, BindingFlags.NonPublic | BindingFlags.Static);

            if (original != null && prefix != null)
            {
                patcher.Patch(original, prefix, null);
            }
        }

        static Type ResolveHollowKnightType(string typeName)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeName, false);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        static bool TryGetCustomEnvironmentType(int environmentType, out CustomEnvironmentType customEnvironmentType)
        {
            return EnvironmentTypeUtilities.TryGetCustomEnvironmentType(environmentType, out customEnvironmentType);
        }

        static bool TryGetCurrentCustomEnvironmentType(out CustomEnvironmentType customEnvironmentType)
        {
            var playerData = GameManager.instance != null ? GameManager.instance.playerData : PlayerData.instance;

            if (playerData == null)
            {
                customEnvironmentType = null;
                return false;
            }

            return TryGetCustomEnvironmentType(playerData.GetInt("environmentType"), out customEnvironmentType);
        }

        static bool CheckEnvironment_Prefix(HeroController __instance)
        {
            if (__instance == null || __instance.playerData == null)
            {
                return true;
            }

            if (!TryGetCustomEnvironmentType(__instance.playerData.GetInt("environmentType"), out var customEnvironmentType))
            {
                return true;
            }

            customEnvironmentType.ApplyFootstepSounds(__instance.footStepsRunAudioSource, __instance.footStepsWalkAudioSource);
            return false;
        }

        static bool DashEffect_OnEnable_Prefix(Component __instance)
        {
            if (__instance == null || !TryGetCurrentCustomEnvironmentType(out var customEnvironmentType))
            {
                return true;
            }

            var context = new DashEffectContext(__instance);
            context.Reset();
            customEnvironmentType.ApplyDashEffect(context);
            return false;
        }

        static bool JumpEffects_OnEnable_Prefix(Component __instance)
        {
            if (__instance == null || !TryGetCurrentCustomEnvironmentType(out var customEnvironmentType))
            {
                return true;
            }

            var context = new JumpEffectContext(__instance);
            context.Reset();
            customEnvironmentType.ApplyJumpEffect(context);
            return false;
        }

        static bool SoftLandEffect_OnEnable_Prefix(Component __instance)
        {
            if (__instance == null || !TryGetCurrentCustomEnvironmentType(out var customEnvironmentType))
            {
                return true;
            }

            var context = new SoftLandEffectContext(__instance);
            context.Reset();
            customEnvironmentType.ApplySoftLandEffect(context);
            return false;
        }

        static bool HardLandEffect_OnEnable_Prefix(Component __instance)
        {
            if (__instance == null || !TryGetCurrentCustomEnvironmentType(out var customEnvironmentType))
            {
                return true;
            }

            var context = new HardLandEffectContext(__instance);
            context.Reset();
            customEnvironmentType.ApplyHardLandEffect(context);
            return false;
        }

        static bool OnCollisionExit2D_Prefix(HeroController __instance, Collision2D collision, ref bool ___fallTrailGenerated)
        {
            if (!TryGetCurrentCustomEnvironmentType(out var customEnvironmentType))
            {
                return true;
            }

            if (!IsCustomFallTrailScenario(__instance, collision, ___fallTrailGenerated))
            {
                return true;
            }

            customEnvironmentType.ApplyFallTrail(new FallTrailContext(__instance, collision));
            ___fallTrailGenerated = true;
            return true;
        }
    }
}
