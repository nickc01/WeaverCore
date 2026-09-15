using UnityEditor;
using UnityEngine;
using WeaverCore.Features;

namespace WeaverCore.Editor
{
    [CustomEditor(typeof(BasicEnvironmentType))]
    public class BasicEnvironmentTypeEditor : UnityEditor.Editor
    {
        enum PresetType
        {
            Dust,
            Grass,
            Bone,
            Spa,
            Metal,
            Wet
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawPresetSection((BasicEnvironmentType)target);

            EditorGUILayout.Space();
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawPresetSection(BasicEnvironmentType environmentType)
        {
            EditorGUILayout.LabelField("Built-In Presets", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("These presets update the behavior settings to match Hollow Knight's built-in environment handling. The Environment ID is left unchanged.", MessageType.Info);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawPresetRow(environmentType, PresetType.Dust, PresetType.Grass, PresetType.Bone);
                DrawPresetRow(environmentType, PresetType.Spa, PresetType.Metal, PresetType.Wet);
            }
        }

        void DrawPresetRow(BasicEnvironmentType environmentType, params PresetType[] presets)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                foreach (var preset in presets)
                {
                    if (GUILayout.Button(GetPresetLabel(preset)))
                    {
                        ApplyPreset(environmentType, preset);
                    }
                }
            }
        }

        static string GetPresetLabel(PresetType preset)
        {
            return preset == PresetType.Wet ? "Wet" : preset.ToString();
        }

        void ApplyPreset(BasicEnvironmentType environmentType, PresetType preset)
        {
            Undo.RecordObject(environmentType, $"Apply {GetPresetLabel(preset)} Environment Preset");

            environmentType.FootstepMode = BasicEnvironmentType.FootstepClipMode.BuiltIn;
            environmentType.RunFootstepClip = null;
            environmentType.WalkFootstepClip = null;

            switch (preset)
            {
                case PresetType.Dust:
                    environmentType.BuiltInFootsteps = BasicEnvironmentType.BuiltInFootstepType.Dust;
                    environmentType.Dash = CreateDash(playDashPuff: true, playDust: true);
                    environmentType.Jump = CreateJump(playDust: true, playJumpPuff: true, playTrail: true, enableFallCheck: true);
                    environmentType.SoftLand = CreateSoftLand(playDust: true, useBuiltInSoftLandClip: true);
                    environmentType.HardLand = CreateHardLand(playDust: true, spawnRockParticles: true);
                    environmentType.FallTrail = CreateFallTrail(playBuiltInEffect: true);
                    break;
                case PresetType.Grass:
                    environmentType.BuiltInFootsteps = BasicEnvironmentType.BuiltInFootstepType.Grass;
                    environmentType.Dash = CreateDash(playDashPuff: true, playGrass: true);
                    environmentType.Jump = CreateJump(playGrass: true, playJumpPuff: true, playTrail: true, enableFallCheck: true);
                    environmentType.SoftLand = CreateSoftLand(playGrass: true, useBuiltInSoftLandClip: true);
                    environmentType.HardLand = CreateHardLand(playGrass: true, spawnRockParticles: true);
                    environmentType.FallTrail = CreateFallTrail(playBuiltInEffect: true);
                    break;
                case PresetType.Bone:
                    environmentType.BuiltInFootsteps = BasicEnvironmentType.BuiltInFootstepType.Bone;
                    environmentType.Dash = CreateDash(playDashPuff: true, playBone: true);
                    environmentType.Jump = CreateJump(playBone: true, playJumpPuff: true, playTrail: true, enableFallCheck: true);
                    environmentType.SoftLand = CreateSoftLand(playBone: true, useBuiltInSoftLandClip: true);
                    environmentType.HardLand = CreateHardLand(playBone: true, spawnRockParticles: true);
                    environmentType.FallTrail = CreateFallTrail(playBuiltInEffect: true);
                    break;
                case PresetType.Spa:
                    environmentType.BuiltInFootsteps = BasicEnvironmentType.BuiltInFootstepType.Spa;
                    environmentType.Dash = CreateDash(playWaterCut: true, useBuiltInSplashClip: true);
                    environmentType.Jump = CreateJump(splashOut: true);
                    environmentType.SoftLand = CreateSoftLand(useBuiltInWetLandClip: true);
                    environmentType.HardLand = CreateHardLand(playSpa: true, spawnSpaSpatter: true);
                    environmentType.FallTrail = CreateFallTrail(playBuiltInEffect: true);
                    break;
                case PresetType.Metal:
                    environmentType.BuiltInFootsteps = BasicEnvironmentType.BuiltInFootstepType.Metal;
                    environmentType.Dash = CreateDash(playDashPuff: true, playDust: true);
                    environmentType.Jump = CreateJump(playDust: true, playJumpPuff: true, playTrail: true, enableFallCheck: true);
                    environmentType.SoftLand = CreateSoftLand(playDust: true, useBuiltInSoftLandClip: true);
                    environmentType.HardLand = CreateHardLand(playMetal: true);
                    environmentType.FallTrail = CreateFallTrail(playBuiltInEffect: true);
                    break;
                case PresetType.Wet:
                    environmentType.BuiltInFootsteps = BasicEnvironmentType.BuiltInFootstepType.Water;
                    environmentType.Dash = CreateDash();
                    environmentType.Jump = CreateJump(playSplash: true);
                    environmentType.SoftLand = CreateSoftLand(playSplash: true, useBuiltInWetLandClip: true);
                    environmentType.HardLand = CreateHardLand(playWet: true);
                    environmentType.FallTrail = CreateFallTrail(playBuiltInEffect: false);
                    break;
            }

            EditorUtility.SetDirty(environmentType);
            serializedObject.UpdateIfRequiredOrScript();
        }

        static BasicEnvironmentType.DashEffectSettings CreateDash(
            bool playDashPuff = false,
            bool playDust = false,
            bool playGrass = false,
            bool playBone = false,
            bool playWaterCut = false,
            bool useBuiltInSplashClip = false)
        {
            return new BasicEnvironmentType.DashEffectSettings
            {
                RequireHeroInPosition = true,
                RecycleTime = 1f,
                PlayDashPuff = playDashPuff,
                PlayDust = playDust,
                PlayGrass = playGrass,
                PlayBone = playBone,
                PlayWaterCut = playWaterCut,
                UseBuiltInSplashClip = useBuiltInSplashClip,
                OneShotClip = null,
                ExtraEffect = null
            };
        }

        static BasicEnvironmentType.JumpEffectSettings CreateJump(
            bool playDust = false,
            bool playGrass = false,
            bool playBone = false,
            bool playSplash = false,
            bool playJumpPuff = false,
            bool playTrail = false,
            bool enableFallCheck = false,
            bool splashOut = false)
        {
            return new BasicEnvironmentType.JumpEffectSettings
            {
                RecycleTime = 1f,
                PlayDust = playDust,
                PlayGrass = playGrass,
                PlayBone = playBone,
                PlaySplash = playSplash,
                RandomFlipSplash = true,
                PlayJumpPuff = playJumpPuff,
                PlayTrail = playTrail,
                EnableFallCheck = enableFallCheck,
                SplashOut = splashOut,
                ExtraEffect = null
            };
        }

        static BasicEnvironmentType.SoftLandEffectSettings CreateSoftLand(
            bool playDust = false,
            bool playGrass = false,
            bool playBone = false,
            bool playSplash = false,
            bool useBuiltInSoftLandClip = false,
            bool useBuiltInWetLandClip = false)
        {
            return new BasicEnvironmentType.SoftLandEffectSettings
            {
                RequireHeroInPosition = true,
                RecycleTime = 1f,
                PlayDust = playDust,
                PlayGrass = playGrass,
                PlayBone = playBone,
                PlaySplash = playSplash,
                RandomFlipSplash = true,
                UseBuiltInSoftLandClip = useBuiltInSoftLandClip,
                UseBuiltInWetLandClip = useBuiltInWetLandClip,
                OneShotClip = null,
                ExtraEffect = null
            };
        }

        static BasicEnvironmentType.HardLandEffectSettings CreateHardLand(
            bool playDust = false,
            bool playGrass = false,
            bool playBone = false,
            bool playSpa = false,
            bool playMetal = false,
            bool playWet = false,
            bool spawnRockParticles = false,
            bool spawnSpaSpatter = false)
        {
            return new BasicEnvironmentType.HardLandEffectSettings
            {
                PlayAverageShake = true,
                RecycleTime = 1.5f,
                PlayDust = playDust,
                PlayGrass = playGrass,
                PlayBone = playBone,
                PlaySpa = playSpa,
                PlayMetal = playMetal,
                PlayWet = playWet,
                SpawnRockParticles = spawnRockParticles,
                RockParticlePrefabOverride = null,
                SpawnSpaSpatter = spawnSpaSpatter,
                SpaSpatterPrefabOverride = null,
                ExtraEffect = null
            };
        }

        static BasicEnvironmentType.FallTrailSettings CreateFallTrail(bool playBuiltInEffect)
        {
            return new BasicEnvironmentType.FallTrailSettings
            {
                PlayBuiltInEffect = playBuiltInEffect,
                ExtraEffect = null
            };
        }
    }
}
