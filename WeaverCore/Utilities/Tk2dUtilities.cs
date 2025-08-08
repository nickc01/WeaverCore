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

        private static readonly MethodInfo BuildMethod;
        private static readonly MethodInfo SetSpriteMethod;
        private static readonly MethodInfo PlayMethod;
        private static readonly MethodInfo StopMethod;
        private static readonly MethodInfo PauseMethod;
        private static readonly MethodInfo ResumeMethod;

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

        public static Component FindTk2dSprite(GameObject gameObject)
        {
            if (gameObject == null || Tk2dSpriteType == null)
            {
                return null;
            }

            return gameObject.GetComponent(Tk2dSpriteType);
        }

        public static Tk2dSpriteWrapper FindTk2dSpriteWrapper(GameObject gameObject)
        {
            Component component = FindTk2dSprite(gameObject);
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

    public struct Tk2dSpriteWrapper
    {
        public Component InternalComponent { get; private set; }

        public Tk2dSpriteWrapper(Component component)
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
            get => InternalComponent.ReflectGetProperty<int>("spriteId");
            set => InternalComponent.ReflectSetProperty("spriteId", value);
        }

        public object Collection
        {
            get => InternalComponent.ReflectGetProperty("Collection");
            set => InternalComponent.ReflectSetProperty("Collection", value);
        }

        public Vector3 Scale
        {
            get => InternalComponent.ReflectGetProperty<Vector3>("scale");
            set => InternalComponent.ReflectSetProperty("scale", value);
        }

        public Color Color
        {
            get => InternalComponent.ReflectGetProperty<Color>("color");
            set => InternalComponent.ReflectSetProperty("color", value);
        }

        public void Build()
        {
            InternalComponent.ReflectCallMethod("Build");
        }

        public void SetSprite(object spriteCollection, int spriteId)
        {
            InternalComponent.ReflectCallMethod("SetSprite", new object[] { spriteCollection, spriteId });
        }

        public void SetSprite(object spriteCollection, string spriteName)
        {
            InternalComponent.ReflectCallMethod("SetSprite", new object[] { spriteCollection, spriteName });
        }

        public void ForceBuild()
        {
            InternalComponent.ReflectCallMethod("ForceBuild");
        }

        public static implicit operator bool(Tk2dSpriteWrapper wrapper) => wrapper.IsValid;
    }

    public struct Tk2dSpriteAnimatorWrapper
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
            get => InternalComponent.ReflectGetProperty("Library");
            set => InternalComponent.ReflectSetProperty("Library", value);
        }

        public object CurrentClip
        {
            get => InternalComponent.ReflectGetProperty("CurrentClip");
        }

        public bool Playing
        {
            get => InternalComponent.ReflectGetProperty<bool>("Playing");
        }

        public bool Paused
        {
            get => InternalComponent.ReflectGetProperty<bool>("Paused");
            set => InternalComponent.ReflectSetProperty("Paused", value);
        }

        public float ClipFps
        {
            get => InternalComponent.ReflectGetProperty<float>("ClipFps");
            set => InternalComponent.ReflectSetProperty("ClipFps", value);
        }

        public object Sprite
        {
            get => InternalComponent.ReflectGetProperty("Sprite");
        }

        public int DefaultClipId
        {
            get => InternalComponent.ReflectGetProperty<int>("DefaultClipId");
            set => InternalComponent.ReflectSetProperty("DefaultClipId", value);
        }

        public object DefaultClip
        {
            get => InternalComponent.ReflectGetProperty("DefaultClip");
        }

        public int CurrentFrame
        {
            get => InternalComponent.ReflectGetProperty<int>("CurrentFrame");
        }

        public float ClipTimeSeconds
        {
            get => InternalComponent.ReflectGetProperty<float>("ClipTimeSeconds");
        }

        public void Play()
        {
            InternalComponent.ReflectCallMethod("Play");
        }

        public void Play(string clipName)
        {
            InternalComponent.ReflectCallMethod("Play", new object[] { clipName });
        }

        public void Play(object clip)
        {
            InternalComponent.ReflectCallMethod("Play", new object[] { clip });
        }

        public void PlayFromFrame(int frame)
        {
            InternalComponent.ReflectCallMethod("PlayFromFrame", new object[] { frame });
        }

        public void PlayFromFrame(string clipName, int frame)
        {
            InternalComponent.ReflectCallMethod("PlayFromFrame", new object[] { clipName, frame });
        }

        public void PlayFromFrame(object clip, int frame)
        {
            InternalComponent.ReflectCallMethod("PlayFromFrame", new object[] { clip, frame });
        }

        public void PlayFrom(float clipStartTime)
        {
            InternalComponent.ReflectCallMethod("PlayFrom", new object[] { clipStartTime });
        }

        public void PlayFrom(string clipName, float clipStartTime)
        {
            InternalComponent.ReflectCallMethod("PlayFrom", new object[] { clipName, clipStartTime });
        }

        public void PlayFrom(object clip, float clipStartTime)
        {
            InternalComponent.ReflectCallMethod("PlayFrom", new object[] { clip, clipStartTime });
        }

        public void Stop()
        {
            InternalComponent.ReflectCallMethod("Stop");
        }

        public void StopAndResetFrame()
        {
            InternalComponent.ReflectCallMethod("StopAndResetFrame");
        }

        public void Pause()
        {
            InternalComponent.ReflectCallMethod("Pause");
        }

        public void Resume()
        {
            InternalComponent.ReflectCallMethod("Resume");
        }

        public bool IsPlaying(string clipName)
        {
            return (bool)InternalComponent.ReflectCallMethod("IsPlaying", new object[] { clipName });
        }

        public bool IsPlaying(object clip)
        {
            return (bool)InternalComponent.ReflectCallMethod("IsPlaying", new object[] { clip });
        }

        public object GetClipById(int id)
        {
            return InternalComponent.ReflectCallMethod("GetClipById", new object[] { id });
        }

        public int GetClipIdByName(string name)
        {
            return (int)InternalComponent.ReflectCallMethod("GetClipIdByName", new object[] { name });
        }

        public object GetClipByName(string name)
        {
            return InternalComponent.ReflectCallMethod("GetClipByName", new object[] { name });
        }

        public void SetFrame(int frame)
        {
            InternalComponent.ReflectCallMethod("SetFrame", new object[] { frame });
        }

        public void SetFrame(int frame, bool triggerEvent)
        {
            InternalComponent.ReflectCallMethod("SetFrame", new object[] { frame, triggerEvent });
        }

        public void UpdateAnimation(float deltaTime)
        {
            InternalComponent.ReflectCallMethod("UpdateAnimation", new object[] { deltaTime });
        }

        public void SetSprite(object spriteCollection, int spriteId)
        {
            InternalComponent.ReflectCallMethod("SetSprite", new object[] { spriteCollection, spriteId });
        }

        public static implicit operator bool(Tk2dSpriteAnimatorWrapper wrapper) => wrapper.IsValid;
    }

    #endregion
}