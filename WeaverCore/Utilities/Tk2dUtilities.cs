using System;
using System.Reflection;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Utilities
{
    public static class Tk2dUtilities
    {
        #region Type Cache

        // Cache reflection types to improve performance
        public static readonly Type Tk2dSpriteType;
        public static readonly Type Tk2dSpriteAnimatorType;
        public static readonly Type Tk2dBaseSpriteType;
        public static readonly Type Tk2dSpriteCollectionDataType;
        public static readonly Type Tk2dSpriteAnimationClipType;
        public static readonly Type Tk2dSpriteAnimationType;

        // Cache commonly used methods and properties
        private static readonly PropertyInfo SpriteIdProperty;
        private static readonly PropertyInfo CollectionProperty;
        private static readonly PropertyInfo ScaleProperty;
        private static readonly PropertyInfo ColorProperty;
        private static readonly PropertyInfo LibraryProperty;
        private static readonly PropertyInfo CurrentClipProperty;
        private static readonly PropertyInfo PlayingProperty;
        private static readonly PropertyInfo PausedProperty;
        private static readonly PropertyInfo ClipFpsProperty;
        private static readonly PropertyInfo SpriteProperty;

        internal static readonly MethodInfo BuildMethod;
        internal static readonly MethodInfo SetSpriteMethod;
        internal static readonly MethodInfo PlayMethod;
        internal static readonly MethodInfo StopMethod;
        internal static readonly MethodInfo PauseMethod;
        internal static readonly MethodInfo ResumeMethod;
        
        // Additional methods for resolving ambiguous method calls
        internal static readonly MethodInfo SetSpriteIntMethod;
        internal static readonly MethodInfo SetSpriteStringMethod;
        internal static readonly MethodInfo PlayNoArgsMethod;
        internal static readonly MethodInfo PlayStringMethod;
        internal static readonly MethodInfo PlayObjectMethod;
        internal static readonly MethodInfo PlayFromFrameIntMethod;
        internal static readonly MethodInfo PlayFromFrameStringIntMethod;
        internal static readonly MethodInfo PlayFromFrameObjectIntMethod;
        internal static readonly MethodInfo PlayFromFloatMethod;
        internal static readonly MethodInfo PlayFromStringFloatMethod;
        internal static readonly MethodInfo PlayFromObjectFloatMethod;
        internal static readonly MethodInfo IsPlayingStringMethod;
        internal static readonly MethodInfo IsPlayingObjectMethod;
        internal static readonly MethodInfo SetFrameIntMethod;
        internal static readonly MethodInfo SetFrameIntBoolMethod;
        internal static readonly MethodInfo ForceBuildMethod;

        private static readonly string SPRITE_ID_PROP_NAME = "spriteId";
        private static readonly string COLLECTION_PROP_NAME = "Collection";
        private static readonly string SCALE_PROP_NAME = "scale";
        private static readonly string COLOR_PROP_NAME = "color";
        private static readonly string LIBRARY_PROP_NAME = "Library";
        private static readonly string CURRENT_CLIP_PROP_NAME = "CurrentClip";
        private static readonly string PLAYING_PROP_NAME = "Playing";
        private static readonly string PAUSED_PROP_NAME = "Paused";
        private static readonly string CLIP_FPS_PROP_NAME = "ClipFps";
        private static readonly string SPRITE_PROP_NAME = "Sprite";

        static Tk2dUtilities()
        {
            // Initialize cached types through reflection
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var tk2dSpriteType = assembly.GetType("tk2dSprite");
                if (tk2dSpriteType != null)
                {
                    Tk2dSpriteType = tk2dSpriteType;
                    break;
                }
            }

            if (Tk2dSpriteType != null)
            {
                Tk2dSpriteAnimatorType = TypeUtilities.NameToType("tk2dSpriteAnimator", Tk2dSpriteType.Assembly.GetName().Name);
                Tk2dBaseSpriteType = TypeUtilities.NameToType("tk2dBaseSprite", Tk2dSpriteType.Assembly.GetName().Name);
                Tk2dSpriteCollectionDataType = TypeUtilities.NameToType("tk2dSpriteCollectionData", Tk2dSpriteType.Assembly.GetName().Name);
                Tk2dSpriteAnimationClipType = TypeUtilities.NameToType("tk2dSpriteAnimationClip", Tk2dSpriteType.Assembly.GetName().Name);
                Tk2dSpriteAnimationType = TypeUtilities.NameToType("tk2dSpriteAnimation", Tk2dSpriteType.Assembly.GetName().Name);

                // Cache commonly used properties for tk2dSprite/tk2dBaseSprite
                SpriteIdProperty = Tk2dBaseSpriteType?.GetProperty(SPRITE_ID_PROP_NAME);
                CollectionProperty = Tk2dBaseSpriteType?.GetProperty(COLLECTION_PROP_NAME);
                ScaleProperty = Tk2dBaseSpriteType?.GetProperty(SCALE_PROP_NAME);
                ColorProperty = Tk2dBaseSpriteType?.GetProperty(COLOR_PROP_NAME);

                // Cache commonly used methods for tk2dSprite
                BuildMethod = Tk2dSpriteType?.GetMethod("Build");
                SetSpriteMethod = Tk2dBaseSpriteType?.GetMethod("SetSprite", new[] { Tk2dSpriteCollectionDataType, typeof(int) });
                ForceBuildMethod = Tk2dSpriteType?.GetMethod("ForceBuild");

                // Cache SetSprite overloads to avoid ambiguous matches
                SetSpriteIntMethod = Tk2dBaseSpriteType?.GetMethod("SetSprite", new[] { Tk2dSpriteCollectionDataType, typeof(int) });
                SetSpriteStringMethod = Tk2dBaseSpriteType?.GetMethod("SetSprite", new[] { Tk2dSpriteCollectionDataType, typeof(string) });

                // Cache commonly used properties for tk2dSpriteAnimator
                LibraryProperty = Tk2dSpriteAnimatorType?.GetProperty(LIBRARY_PROP_NAME);
                CurrentClipProperty = Tk2dSpriteAnimatorType?.GetProperty(CURRENT_CLIP_PROP_NAME);
                PlayingProperty = Tk2dSpriteAnimatorType?.GetProperty(PLAYING_PROP_NAME);
                PausedProperty = Tk2dSpriteAnimatorType?.GetProperty(PAUSED_PROP_NAME);
                ClipFpsProperty = Tk2dSpriteAnimatorType?.GetProperty(CLIP_FPS_PROP_NAME);
                SpriteProperty = Tk2dSpriteAnimatorType?.GetProperty(SPRITE_PROP_NAME);

                // Cache commonly used methods for tk2dSpriteAnimator
                PlayMethod = Tk2dSpriteAnimatorType?.GetMethod("Play", new[] { typeof(string) });
                StopMethod = Tk2dSpriteAnimatorType?.GetMethod("Stop");
                PauseMethod = Tk2dSpriteAnimatorType?.GetMethod("Pause");
                ResumeMethod = Tk2dSpriteAnimatorType?.GetMethod("Resume");

                // Cache Play overloads to avoid ambiguous matches
                PlayNoArgsMethod = Tk2dSpriteAnimatorType?.GetMethod("Play", Type.EmptyTypes);
                PlayStringMethod = Tk2dSpriteAnimatorType?.GetMethod("Play", new[] { typeof(string) });
                PlayObjectMethod = Tk2dSpriteAnimatorType?.GetMethod("Play", new[] { Tk2dSpriteAnimationClipType });

                // Cache PlayFromFrame overloads to avoid ambiguous matches
                PlayFromFrameIntMethod = Tk2dSpriteAnimatorType?.GetMethod("PlayFromFrame", new[] { typeof(int) });
                PlayFromFrameStringIntMethod = Tk2dSpriteAnimatorType?.GetMethod("PlayFromFrame", new[] { typeof(string), typeof(int) });
                PlayFromFrameObjectIntMethod = Tk2dSpriteAnimatorType?.GetMethod("PlayFromFrame", new[] { Tk2dSpriteAnimationClipType, typeof(int) });

                // Cache PlayFrom overloads to avoid ambiguous matches
                PlayFromFloatMethod = Tk2dSpriteAnimatorType?.GetMethod("PlayFrom", new[] { typeof(float) });
                PlayFromStringFloatMethod = Tk2dSpriteAnimatorType?.GetMethod("PlayFrom", new[] { typeof(string), typeof(float) });
                PlayFromObjectFloatMethod = Tk2dSpriteAnimatorType?.GetMethod("PlayFrom", new[] { Tk2dSpriteAnimationClipType, typeof(float) });

                // Cache IsPlaying overloads to avoid ambiguous matches
                IsPlayingStringMethod = Tk2dSpriteAnimatorType?.GetMethod("IsPlaying", new[] { typeof(string) });
                IsPlayingObjectMethod = Tk2dSpriteAnimatorType?.GetMethod("IsPlaying", new[] { Tk2dSpriteAnimationClipType });

                // Cache SetFrame overloads to avoid ambiguous matches
                SetFrameIntMethod = Tk2dSpriteAnimatorType?.GetMethod("SetFrame", new[] { typeof(int) });
                SetFrameIntBoolMethod = Tk2dSpriteAnimatorType?.GetMethod("SetFrame", new[] { typeof(int), typeof(bool) });
            }
        }

        #endregion

        #region Type Checking

        public static bool IsTk2dSprite(object obj) =>
            obj != null && Tk2dSpriteType.IsAssignableFrom(obj.GetType());

        public static bool IsTk2dSpriteAnimator(object obj) =>
            obj != null && Tk2dSpriteAnimatorType.IsAssignableFrom(obj.GetType());

        public static bool IsTk2dBaseSprite(object obj) =>
            obj != null && Tk2dBaseSpriteType.IsAssignableFrom(obj.GetType());

        public static bool IsTk2dSpriteCollectionData(object obj) =>
            obj != null && obj.GetType() == Tk2dSpriteCollectionDataType;

        public static bool IsTk2dSpriteAnimationClip(object obj) =>
            obj != null && obj.GetType() == Tk2dSpriteAnimationClipType;

        public static bool IsTk2dSpriteAnimation(object obj) =>
            obj != null && obj.GetType() == Tk2dSpriteAnimationType;

        #endregion

        #region Component Finding

        public static MonoBehaviour FindTk2dSprite(GameObject gameObject)
        {
            if (gameObject == null || Tk2dSpriteType == null)
            {
                return null;
            }

            return gameObject.GetComponent(Tk2dSpriteType) as MonoBehaviour;
        }

        public static Tk2dSpriteWrapper FindTk2dSpriteWrapper(GameObject gameObject)
        {
            MonoBehaviour component = FindTk2dSprite(gameObject);
            return component != null ? new Tk2dSpriteWrapper(component) : default;
        }

        public static Component FindTk2dSpriteAnimator(GameObject gameObject)
        {
            if (gameObject == null || Tk2dSpriteAnimatorType == null)
            {
                return null;
            }

            return gameObject.GetComponent(Tk2dSpriteAnimatorType);
        }

        public static Tk2dSpriteAnimatorWrapper FindTk2dSpriteAnimatorWrapper(GameObject gameObject)
        {
            Component component = FindTk2dSpriteAnimator(gameObject);
            return component != null ? new Tk2dSpriteAnimatorWrapper(component) : default;
        }

        #endregion
    }

    #region Wrapper Structs

    public class Tk2dSpriteWrapper
    {
        public MonoBehaviour InternalComponent { get; private set; }

        public Tk2dSpriteWrapper(MonoBehaviour component)
        {
            if (!Tk2dUtilities.IsTk2dSprite(component))
            {
                throw new ArgumentException("Component is not a tk2dSprite", nameof(component));
            }
            InternalComponent = component;
        }

        public bool IsValid => InternalComponent != null;

        public int SpriteId
        {
            get => InternalComponent.ReflectGetProperty<int>("spriteId", Tk2dUtilities.Tk2dBaseSpriteType);
            set => InternalComponent.ReflectSetProperty("spriteId", value, Tk2dUtilities.Tk2dBaseSpriteType);
        }

        public object Collection
        {
            get => InternalComponent.ReflectGetProperty("Collection", Tk2dUtilities.Tk2dBaseSpriteType);
            set => InternalComponent.ReflectSetProperty("Collection", value, Tk2dUtilities.Tk2dBaseSpriteType);
        }

        public Vector3 Scale
        {
            get => InternalComponent.ReflectGetProperty<Vector3>("scale", Tk2dUtilities.Tk2dBaseSpriteType);
            set => InternalComponent.ReflectSetProperty("scale", value, Tk2dUtilities.Tk2dBaseSpriteType);
        }

        public Color Color
        {
            get => InternalComponent.ReflectGetProperty<Color>("color", Tk2dUtilities.Tk2dBaseSpriteType);
            set => InternalComponent.ReflectSetProperty("color", value, Tk2dUtilities.Tk2dBaseSpriteType);
        }

        public bool enabled
        {
            get => InternalComponent.enabled;
            set => InternalComponent.enabled = value;
        }

        public void Build()
        {
            Tk2dUtilities.BuildMethod?.Invoke(InternalComponent, null);
        }

        public void SetSprite(object spriteCollection, int spriteId)
        {
            Tk2dUtilities.SetSpriteIntMethod?.Invoke(InternalComponent, new object[] { spriteCollection, spriteId });
        }

        public void SetSprite(object spriteCollection, string spriteName)
        {
            Tk2dUtilities.SetSpriteStringMethod?.Invoke(InternalComponent, new object[] { spriteCollection, spriteName });
        }

        public void ForceBuild()
        {
            Tk2dUtilities.ForceBuildMethod?.Invoke(InternalComponent, null);
        }

        public static implicit operator bool(Tk2dSpriteWrapper wrapper) => wrapper.IsValid;
    }

    public class Tk2dSpriteAnimatorWrapper
    {
        public Component InternalComponent { get; private set; }

        public Tk2dSpriteAnimatorWrapper(Component component)
        {
            if (!Tk2dUtilities.IsTk2dSpriteAnimator(component))
            {
                throw new ArgumentException("Component is not a tk2dSpriteAnimator", nameof(component));
            }
            InternalComponent = component;
        }

        public bool IsValid => InternalComponent != null;

        public object Library
        {
            get => InternalComponent.ReflectGetProperty("Library", Tk2dUtilities.Tk2dSpriteAnimatorType);
            set => InternalComponent.ReflectSetProperty("Library", value, Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public object CurrentClip
        {
            get => InternalComponent.ReflectGetProperty("CurrentClip", Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public bool Playing
        {
            get => InternalComponent.ReflectGetProperty<bool>("Playing", Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public bool Paused
        {
            get => InternalComponent.ReflectGetProperty<bool>("Paused", Tk2dUtilities.Tk2dSpriteAnimatorType);
            set => InternalComponent.ReflectSetProperty("Paused", value, Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public float ClipFps
        {
            get => InternalComponent.ReflectGetProperty<float>("ClipFps", Tk2dUtilities.Tk2dSpriteAnimatorType);
            set => InternalComponent.ReflectSetProperty("ClipFps", value, Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public object Sprite
        {
            get => InternalComponent.ReflectGetProperty("Sprite", Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public int DefaultClipId
        {
            get => InternalComponent.ReflectGetProperty<int>("DefaultClipId", Tk2dUtilities.Tk2dSpriteAnimatorType);
            set => InternalComponent.ReflectSetProperty("DefaultClipId", value, Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public object DefaultClip
        {
            get => InternalComponent.ReflectGetProperty("DefaultClip", Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public int CurrentFrame
        {
            get => InternalComponent.ReflectGetProperty<int>("CurrentFrame", Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public float ClipTimeSeconds
        {
            get => InternalComponent.ReflectGetProperty<float>("ClipTimeSeconds", Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public void Play()
        {
            Tk2dUtilities.PlayNoArgsMethod?.Invoke(InternalComponent, null);
        }

        public void Play(string clipName)
        {
            Tk2dUtilities.PlayStringMethod?.Invoke(InternalComponent, new object[] { clipName });
        }

        public void Play(object clip)
        {
            Tk2dUtilities.PlayObjectMethod?.Invoke(InternalComponent, new object[] { clip });
        }

        public void PlayFromFrame(int frame)
        {
            Tk2dUtilities.PlayFromFrameIntMethod?.Invoke(InternalComponent, new object[] { frame });
        }

        public void PlayFromFrame(string clipName, int frame)
        {
            Tk2dUtilities.PlayFromFrameStringIntMethod?.Invoke(InternalComponent, new object[] { clipName, frame });
        }

        public void PlayFromFrame(object clip, int frame)
        {
            Tk2dUtilities.PlayFromFrameObjectIntMethod?.Invoke(InternalComponent, new object[] { clip, frame });
        }

        public void PlayFrom(float clipStartTime)
        {
            Tk2dUtilities.PlayFromFloatMethod?.Invoke(InternalComponent, new object[] { clipStartTime });
        }

        public void PlayFrom(string clipName, float clipStartTime)
        {
            Tk2dUtilities.PlayFromStringFloatMethod?.Invoke(InternalComponent, new object[] { clipName, clipStartTime });
        }

        public void PlayFrom(object clip, float clipStartTime)
        {
            Tk2dUtilities.PlayFromObjectFloatMethod?.Invoke(InternalComponent, new object[] { clip, clipStartTime });
        }

        public void Stop()
        {
            Tk2dUtilities.StopMethod?.Invoke(InternalComponent, null);
        }

        public void StopAndResetFrame()
        {
            InternalComponent.ReflectCallMethod("StopAndResetFrame", Tk2dUtilities.Tk2dSpriteAnimatorType);
        }

        public void Pause()
        {
            Tk2dUtilities.PauseMethod?.Invoke(InternalComponent, null);
        }

        public void Resume()
        {
            Tk2dUtilities.ResumeMethod?.Invoke(InternalComponent, null);
        }

        public bool IsPlaying(string clipName)
        {
            return (bool)(Tk2dUtilities.IsPlayingStringMethod?.Invoke(InternalComponent, new object[] { clipName }) ?? false);
        }

        public bool IsPlaying(object clip)
        {
            return (bool)(Tk2dUtilities.IsPlayingObjectMethod?.Invoke(InternalComponent, new object[] { clip }) ?? false);
        }

        public object GetClipById(int id)
        {
            return InternalComponent.ReflectCallMethod("GetClipById", Tk2dUtilities.Tk2dSpriteAnimatorType, new object[] { id });
        }

        public int GetClipIdByName(string name)
        {
            return (int)InternalComponent.ReflectCallMethod("GetClipIdByName", Tk2dUtilities.Tk2dSpriteAnimatorType, new object[] { name });
        }

        public object GetClipByName(string name)
        {
            return InternalComponent.ReflectCallMethod("GetClipByName", Tk2dUtilities.Tk2dSpriteAnimatorType, new object[] { name });
        }

        public void SetFrame(int frame)
        {
            Tk2dUtilities.SetFrameIntMethod?.Invoke(InternalComponent, new object[] { frame });
        }

        public void SetFrame(int frame, bool triggerEvent)
        {
            Tk2dUtilities.SetFrameIntBoolMethod?.Invoke(InternalComponent, new object[] { frame, triggerEvent });
        }

        public void UpdateAnimation(float deltaTime)
        {
            InternalComponent.ReflectCallMethod("UpdateAnimation", Tk2dUtilities.Tk2dSpriteAnimatorType, new object[] { deltaTime });
        }

        public void SetSprite(object spriteCollection, int spriteId)
        {
            Tk2dUtilities.SetSpriteIntMethod?.Invoke(InternalComponent, new object[] { spriteCollection, spriteId });
        }

        public static implicit operator bool(Tk2dSpriteAnimatorWrapper wrapper) => wrapper.IsValid;
    }

    #endregion
}
