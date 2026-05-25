using System;
using System.Collections;
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
        public static readonly Type Tk2dSpriteAnimationFrameType;

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
        internal static readonly FieldInfo ClipNameField;
        internal static readonly FieldInfo ClipFramesField;
        internal static readonly FieldInfo ClipFpsField;
        internal static readonly FieldInfo ClipLoopStartField;
        internal static readonly FieldInfo ClipWrapModeField;
        internal static readonly FieldInfo FrameSpriteCollectionField;
        internal static readonly FieldInfo FrameSpriteIdField;

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
                Tk2dSpriteAnimationFrameType = TypeUtilities.NameToType("tk2dSpriteAnimationFrame", Tk2dSpriteType.Assembly.GetName().Name);

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
                ClipNameField = Tk2dSpriteAnimationClipType?.GetField("name", BindingFlags.Public | BindingFlags.Instance);
                ClipFramesField = Tk2dSpriteAnimationClipType?.GetField("frames", BindingFlags.Public | BindingFlags.Instance);
                ClipFpsField = Tk2dSpriteAnimationClipType?.GetField("fps", BindingFlags.Public | BindingFlags.Instance);
                ClipLoopStartField = Tk2dSpriteAnimationClipType?.GetField("loopStart", BindingFlags.Public | BindingFlags.Instance);
                ClipWrapModeField = Tk2dSpriteAnimationClipType?.GetField("wrapMode", BindingFlags.Public | BindingFlags.Instance);
                FrameSpriteCollectionField = Tk2dSpriteAnimationFrameType?.GetField("spriteCollection", BindingFlags.Public | BindingFlags.Instance);
                FrameSpriteIdField = Tk2dSpriteAnimationFrameType?.GetField("spriteId", BindingFlags.Public | BindingFlags.Instance);

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
        enum RawWrapMode
        {
            Loop = 0,
            LoopSection = 1,
            Once = 2,
            PingPong = 3,
            RandomFrame = 4,
            RandomLoop = 5,
            Single = 6
        }

        sealed class RawPlaybackState
        {
            public MonoBehaviour Behaviour;
            public Coroutine Coroutine;
            public bool Completed;
            public bool Cancelled;
        }

        static readonly Func<bool, bool> CompletedRawHandle = _ => true;

        public Component InternalComponent { get; private set; }
        RawPlaybackState rawPlaybackState;

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
            StopRaw();
            Tk2dUtilities.PlayNoArgsMethod?.Invoke(InternalComponent, null);
        }

        public Func<bool, bool> PlayRaw()
        {
            return PlayRaw(CurrentClip ?? DefaultClip);
        }

        public Func<bool, bool> PlayRaw(string clipName)
        {
            if (!IsValid || string.IsNullOrEmpty(clipName))
            {
                return CompletedRawHandle;
            }

            return PlayRaw(GetClipByName(clipName));
        }

        public Func<bool, bool> PlayRaw(object clip)
        {
            if (!IsValid || clip == null)
            {
                return CompletedRawHandle;
            }

            var behaviour = InternalComponent as MonoBehaviour;
            if (behaviour == null)
            {
                return CompletedRawHandle;
            }

            Stop();
            StopRaw();
            var state = new RawPlaybackState
            {
                Behaviour = behaviour
            };
            rawPlaybackState = state;
            state.Coroutine = behaviour.StartCoroutine(PlayRawRoutine(clip, state));
            return cancel => QueryRawPlayback(state, cancel);
        }

        public void Play(string clipName)
        {
            StopRaw();
            Tk2dUtilities.PlayStringMethod?.Invoke(InternalComponent, new object[] { clipName });
        }

        public void Play(object clip)
        {
            StopRaw();
            Tk2dUtilities.PlayObjectMethod?.Invoke(InternalComponent, new object[] { clip });
        }

        public void PlayFromFrame(int frame)
        {
            StopRaw();
            Tk2dUtilities.PlayFromFrameIntMethod?.Invoke(InternalComponent, new object[] { frame });
        }

        public void PlayFromFrame(string clipName, int frame)
        {
            StopRaw();
            Tk2dUtilities.PlayFromFrameStringIntMethod?.Invoke(InternalComponent, new object[] { clipName, frame });
        }

        public void PlayFromFrame(object clip, int frame)
        {
            StopRaw();
            Tk2dUtilities.PlayFromFrameObjectIntMethod?.Invoke(InternalComponent, new object[] { clip, frame });
        }

        public void PlayFrom(float clipStartTime)
        {
            StopRaw();
            Tk2dUtilities.PlayFromFloatMethod?.Invoke(InternalComponent, new object[] { clipStartTime });
        }

        public void PlayFrom(string clipName, float clipStartTime)
        {
            StopRaw();
            Tk2dUtilities.PlayFromStringFloatMethod?.Invoke(InternalComponent, new object[] { clipName, clipStartTime });
        }

        public void PlayFrom(object clip, float clipStartTime)
        {
            StopRaw();
            Tk2dUtilities.PlayFromObjectFloatMethod?.Invoke(InternalComponent, new object[] { clip, clipStartTime });
        }

        public void Stop()
        {
            StopRaw();
            Tk2dUtilities.StopMethod?.Invoke(InternalComponent, null);
        }

        public void StopRaw()
        {
            CancelRawPlayback(rawPlaybackState);
        }

        public void StopAndResetFrame()
        {
            StopRaw();
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
            if (!IsValid || string.IsNullOrEmpty(clipName))
            {
                return false;
            }

            try
            {
                if (!Playing)
                {
                    return false;
                }

                var currentClip = CurrentClip;
                if (currentClip == null || Tk2dUtilities.ClipNameField == null)
                {
                    return false;
                }

                return string.Equals(Tk2dUtilities.ClipNameField.GetValue(currentClip) as string, clipName, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        public bool IsPlaying(object clip)
        {
            if (!IsValid || clip == null)
            {
                return false;
            }

            try
            {
                return Playing && ReferenceEquals(CurrentClip, clip);
            }
            catch
            {
                return false;
            }
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
            StopRaw();
            Tk2dUtilities.SetFrameIntMethod?.Invoke(InternalComponent, new object[] { frame });
        }

        public void SetFrame(int frame, bool triggerEvent)
        {
            StopRaw();
            Tk2dUtilities.SetFrameIntBoolMethod?.Invoke(InternalComponent, new object[] { frame, triggerEvent });
        }

        public void UpdateAnimation(float deltaTime)
        {
            InternalComponent.ReflectCallMethod("UpdateAnimation", Tk2dUtilities.Tk2dSpriteAnimatorType, new object[] { deltaTime });
        }

        public void SetSprite(object spriteCollection, int spriteId)
        {
            Tk2dUtilities.SetSpriteIntMethod?.Invoke(Sprite, new object[] { spriteCollection, spriteId });
        }

        bool QueryRawPlayback(RawPlaybackState state, bool cancel)
        {
            if (state == null)
            {
                return true;
            }

            if (cancel)
            {
                CancelRawPlayback(state);
            }

            return state.Completed || state.Cancelled;
        }

        void CancelRawPlayback(RawPlaybackState state)
        {
            if (state == null || state.Completed || state.Cancelled)
            {
                return;
            }

            state.Cancelled = true;
            if (ReferenceEquals(rawPlaybackState, state))
            {
                rawPlaybackState = null;
            }

            var behaviour = state.Behaviour;
            var coroutine = state.Coroutine;
            state.Coroutine = null;

            if (behaviour != null && coroutine != null)
            {
                behaviour.StopCoroutine(coroutine);
            }
        }

        void CompleteRawPlayback(RawPlaybackState state)
        {
            if (state == null || state.Cancelled)
            {
                return;
            }

            state.Coroutine = null;
            state.Completed = true;
            if (ReferenceEquals(rawPlaybackState, state))
            {
                rawPlaybackState = null;
            }
        }

        IEnumerator PlayRawRoutine(object clip, RawPlaybackState state)
        {
            try
            {
                var frames = Tk2dUtilities.ClipFramesField?.GetValue(clip) as Array;
                if (frames == null || frames.Length == 0)
                {
                    yield break;
                }

                float fps = 30f;
                if (Tk2dUtilities.ClipFpsField?.GetValue(clip) is float clipFps && clipFps > 0f)
                {
                    fps = clipFps;
                }
                float frameDuration = 1f / fps;

                int loopStart = 0;
                if (Tk2dUtilities.ClipLoopStartField?.GetValue(clip) is int clipLoopStart)
                {
                    loopStart = Mathf.Clamp(clipLoopStart, 0, frames.Length - 1);
                }

                var wrapMode = RawWrapMode.Loop;
                if (Tk2dUtilities.ClipWrapModeField?.GetValue(clip) is Enum wrapModeValue)
                {
                    wrapMode = (RawWrapMode)Convert.ToInt32(wrapModeValue);
                }

                switch (wrapMode)
                {
                    case RawWrapMode.Single:
                        ApplyRawFrame(frames, 0);
                        break;
                    case RawWrapMode.RandomFrame:
                        ApplyRawFrame(frames, UnityEngine.Random.Range(0, frames.Length));
                        break;
                    case RawWrapMode.Once:
                        yield return PlayRawForward(frames, 0, frames.Length - 1, frameDuration, loopLastFrame: false);
                        break;
                    case RawWrapMode.Loop:
                        while (true)
                        {
                            yield return PlayRawForward(frames, 0, frames.Length - 1, frameDuration, loopLastFrame: true);
                        }
                    case RawWrapMode.LoopSection:
                        if (loopStart > 0)
                        {
                            yield return PlayRawForward(frames, 0, frames.Length - 1, frameDuration, loopLastFrame: false);
                        }
                        while (true)
                        {
                            yield return PlayRawForward(frames, loopStart, frames.Length - 1, frameDuration, loopLastFrame: true);
                        }
                    case RawWrapMode.PingPong:
                        while (true)
                        {
                            yield return PlayRawForward(frames, 0, frames.Length - 1, frameDuration, loopLastFrame: frames.Length == 1);
                            if (frames.Length <= 1)
                            {
                                continue;
                            }

                            for (int i = frames.Length - 2; i >= 1; i--)
                            {
                                ApplyRawFrame(frames, i);
                                yield return new WaitForSeconds(frameDuration);
                            }
                        }
                    case RawWrapMode.RandomLoop:
                        while (true)
                        {
                            ApplyRawFrame(frames, UnityEngine.Random.Range(0, frames.Length));
                            yield return new WaitForSeconds(frameDuration);
                        }
                }
            }
            finally
            {
                CompleteRawPlayback(state);
            }
        }

        IEnumerator PlayRawForward(Array frames, int startIndex, int endIndex, float frameDuration, bool loopLastFrame)
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                ApplyRawFrame(frames, i);
                if (i < endIndex || loopLastFrame)
                {
                    yield return new WaitForSeconds(frameDuration);
                }
            }
        }

        void ApplyRawFrame(Array frames, int index)
        {
            if (index < 0 || index >= frames.Length)
            {
                return;
            }

            var frame = frames.GetValue(index);
            if (frame == null)
            {
                return;
            }

            var spriteCollection = Tk2dUtilities.FrameSpriteCollectionField?.GetValue(frame);
            if (spriteCollection == null)
            {
                return;
            }

            int spriteId = 0;
            if (Tk2dUtilities.FrameSpriteIdField?.GetValue(frame) is int rawSpriteId)
            {
                spriteId = rawSpriteId;
            }

            SetSprite(spriteCollection, spriteId);
        }

        public static implicit operator bool(Tk2dSpriteAnimatorWrapper wrapper) => wrapper.IsValid;
    }

    #endregion
}
