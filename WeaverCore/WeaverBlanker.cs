using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Enums;
using WeaverCore.Playmaker;
using WeaverCore.Utilities;

namespace WeaverCore
{
    public class WeaverBlanker : MonoBehaviour
    {
        static WeaverBlanker _hudCameraBlack;
        static WeaverBlanker _hudCameraWhite;
        static WeaverBlanker _hudCamera2dtkBlanker;
        static WeaverBlanker _cameraStartBlanker;
        //static WeaverBlanker _cutsceneBlanker;
        //static WeaverBlanker _quitBlanker;

        public static WeaverBlanker HudCameraBlack => _hudCameraBlack ??= FindAndInitBlanker("_GameCameras/HudCamera/Blanker");
        public static WeaverBlanker HudCameraWhite => _hudCameraWhite ??= FindAndInitBlanker("_GameCameras/HudCamera/Blanker White");
        public static WeaverBlanker HudCamera2dtkBlanker => _hudCamera2dtkBlanker ??= FindAndInitBlanker("_GameCameras/HudCamera/2dtk Blanker");
        public static WeaverBlanker CameraStartBlanker => _cameraStartBlanker ??= FindAndInitBlanker("_GameCameras/CameraParent/tk2dCamera/Start Blanker");
        //public static WeaverBlanker CutsceneBlanker => _cameraStartBlanker ??= FindAndInitBlanker("Cutscene Blanker");
        //public static WeaverBlanker QuitBlanker;

        [NonSerialized]
        Color _fromColor;
        public Color FromColor
        {
            get
            {
                if (hasFSM)
                {
                    return blankerFSM.GetFsm().GetState("Fade In").GetActions()[0].InternalAction.ReflectGetField("color").ReflectGetProperty<Color>("Value");
                }
                else
                {
                    return _fromColor;
                }
            }

            set
            {
                if (hasFSM)
                {
                    var fsm = blankerFSM.GetFsm();

                    if (fsm.TryGetState("Init", out var initState) && FindAction(initState, "SetMaterialColor", out var initAction))
                    {
                        initAction.InternalAction.ReflectGetField("color").ReflectSetProperty("Value", value);
                    }

                    if (fsm.TryGetState("Fade In", out var fadeInState))
                    {
                        if (FindAction(fadeInState, "SetColorValue", out var fadeInAction))
                        {
                            fadeInAction.InternalAction.ReflectGetField("color").ReflectSetProperty("Value", value);
                        }

                        if (FindAction(fadeInState, "EaseColor", out var easeColorAction))
                        {
                            easeColorAction.InternalAction.ReflectGetField("fromValue").ReflectSetProperty("Value", value);
                        }
                    }

                    if (fsm.TryGetState("Fade Out", out var fadeOutState))
                    {
                        /*if (FindAction(fadeOutState, "SetColorValue", out var fadeOutAction))
                        {
                            fadeOutAction.InternalAction.ReflectGetField("color").ReflectSetProperty("Value", value);
                        }*/

                        if (FindAction(fadeOutState, "EaseColor", out var easeColorAction))
                        {
                            easeColorAction.InternalAction.ReflectGetField("toValue").ReflectSetProperty("Value", value);
                        }
                    }

                    if (fsm.TryGetState("Reset", out var resetState) && FindAction(resetState, "SetMaterialColor", out var resetAction))
                    {
                        resetAction.InternalAction.ReflectGetField("color").ReflectSetProperty("Value", value);
                    }
                }
                else
                {
                    _fromColor = value;
                }
            }
        }

        [NonSerialized]
        Color _toColor;
        public Color ToColor
        {
            get
            {
                if (hasFSM)
                {
                    return blankerFSM.GetFsm().GetState("Fade Out").GetActions()[0].InternalAction.ReflectGetField("color").ReflectGetProperty<Color>("Value");
                }
                else
                {
                    return _toColor;
                }
            }

            set
            {
                if (hasFSM)
                {
                    var fsm = blankerFSM.GetFsm();

                    if (fsm.TryGetState("Fade In", out var fadeInState))
                    {
                        if (FindAction(fadeInState, "EaseColor", out var easeColorAction))
                        {
                            easeColorAction.InternalAction.ReflectGetField("toValue").ReflectSetProperty("Value", value);
                        }
                    }

                    if (fsm.TryGetState("Fade Out", out var fadeOutState))
                    {
                        if (FindAction(fadeOutState, "SetColorValue", out var fadeOutAction))
                        {
                            fadeOutAction.InternalAction.ReflectGetField("color").ReflectSetProperty("Value", value);
                        }

                        if (FindAction(fadeOutState, "EaseColor", out var easeColorAction))
                        {
                            easeColorAction.InternalAction.ReflectGetField("fromValue").ReflectSetProperty("Value", value);
                        }
                    }
                }
                else
                {
                    _toColor = value;
                }
            }
        }

        [NonSerialized]
        float _fadeTime;
        public float FadeTime
        {
            get
            {
                if (hasFSM)
                {
                    return blankerFSM.GetFsm().GetFloatVariable("Fade Time");
                }
                else
                {
                    return _fadeTime;
                }
            }

            set
            {
                if (hasFSM)
                {
                    blankerFSM.GetFsm().SetFloatVariable("Fade Time", _fadeTime);
                }
                else
                {
                    _fadeTime = value;
                }
            }
        }

        [NonSerialized]
        WeaverEaseType _fadeInEaseType;

        public WeaverEaseType FadeInEaseType
        {
            get
            {
                if (hasFSM)
                {
                    var fsm = blankerFSM.GetFsm();
                    if (fsm.TryGetState("Fade In", out var state) && FindAction(state, "EaseColor", out var easeColorAction))
                    {
                        var baseType = easeColorAction.InternalAction.GetType().BaseType;
                        var easeType = easeColorAction.InternalAction.ReflectGetField("easeType", baseType);

                        if (TryEnumToInt(easeType, out var easeNum))
                        {
                            return (WeaverEaseType)easeNum;
                        }
                    }
                }

                return _fadeInEaseType;
            }

            set
            {
                if (hasFSM)
                {
                    var fsm = blankerFSM.GetFsm();
                    if (fsm.TryGetState("Fade In", out var state) && FindAction(state, "EaseColor", out var easeColorAction))
                    {
                        var baseType = easeColorAction.InternalAction.GetType().BaseType;
                        var easeType = easeColorAction.InternalAction.ReflectGetField("easeType", baseType);

                        easeColorAction.InternalAction.ReflectSetField("easeType", IntToEnum(easeType.GetType(), (int)value), baseType);
                    }
                }
                else
                {
                    _fadeInEaseType = value;
                }
            }
        }

        [NonSerialized]
        WeaverEaseType _fadeOutEaseType;

        public WeaverEaseType FadeOutEaseType
        {
            get
            {
                if (hasFSM)
                {
                    var fsm = blankerFSM.GetFsm();
                    if (fsm.TryGetState("Fade Out", out var state) && FindAction(state, "EaseColor", out var easeColorAction))
                    {
                        var baseType = easeColorAction.InternalAction.GetType().BaseType;
                        var easeType = easeColorAction.InternalAction.ReflectGetField("easeType", baseType);

                        if (TryEnumToInt(easeType, out var easeNum))
                        {
                            return (WeaverEaseType)easeNum;
                        }
                    }
                }

                return _fadeOutEaseType;
            }

            set
            {
                if (hasFSM)
                {
                    var fsm = blankerFSM.GetFsm();
                    if (fsm.TryGetState("Fade Out", out var state) && FindAction(state, "EaseColor", out var easeColorAction))
                    {
                        var baseType = easeColorAction.InternalAction.GetType().BaseType;
                        var easeType = easeColorAction.InternalAction.ReflectGetField("easeType", baseType);

                        easeColorAction.InternalAction.ReflectSetField("easeType", IntToEnum(easeType.GetType(), (int)value), baseType);
                    }
                }
                else
                {
                    _fadeInEaseType = value;
                }
            }
        }

        static bool TryEnumToInt(object enumValue, out int i)
        {
            if (enumValue is null || !enumValue.GetType().IsEnum)
                throw new ArgumentException("Pass a boxed enum value.", nameof(enumValue));

            var enumType = enumValue.GetType();
            var underlying = Enum.GetUnderlyingType(enumType);
            var boxedNumber = Convert.ChangeType(enumValue, underlying);
            try
            {
                i = Convert.ToInt32(boxedNumber);
                return true;
            }
            catch (OverflowException)
            {
                i = default;
                return false;
            }
        }

        static object IntToEnum(Type enumType, int value, bool validate = true)
        {
            if (enumType is null || !enumType.IsEnum)
                throw new ArgumentException("enumType must be an enum Type.", nameof(enumType));

            if (validate && !Enum.IsDefined(enumType, value))
                throw new ArgumentOutOfRangeException(nameof(value),
                    $"No {enumType.Name} member with underlying value {value}.");

            return Enum.ToObject(enumType, value);
        }


        static bool FindAction(FsmStateWrapper state, string actionName, out FsmActionWrapper finalAction)
        {
            var actions = state.GetActions();
            foreach (var action in actions)
            {
                if (action.InternalAction.GetType().FullName.Contains(actionName))
                {
                    finalAction = action;
                    return true;
                }
            }

            finalAction = default;
            return false;
        }

        SpriteRenderer _mainRenderer;
        public SpriteRenderer MainRenderer => _mainRenderer ??= GetComponent<SpriteRenderer>();


        static WeaverBlanker FindAndInitBlanker(string path)
        {
            var obj = GameObject.Find(path);
            if (obj != null)
            {
                return obj.AddComponent<WeaverBlanker>();
            }

            return null;
        }

        bool hasFSM = false;
        PlayMakerFsmWrapper blankerFSM;
        Coroutine fadeRoutine;

        private void Awake()
        {
            if (PlayMakerUtilities.PlayMakerAvailable)
            {
                var psFSM = PlayMakerUtilities.FindPlayMakerFSM(gameObject, "Blanker Control");
                WeaverLog.Log($"Obj {gameObject.name} has playmaker fsm = {psFSM}, null = {psFSM == null}");
                if (psFSM != null)
                {
                    hasFSM = true;
                    blankerFSM = PlayMakerUtilities.FindPlayMakerFSMWrapper(gameObject, "Blanker Control");
                }
            }
        }

        public void FadeIn()
        {
            WeaverLog.Log("HAS BLANKER = " + hasFSM);
            if (hasFSM)
            {
                blankerFSM.SendEvent("FADE IN");
            }
            else
            {
                if (fadeRoutine != null)
                {
                    StopCoroutine(fadeRoutine);
                    fadeRoutine = null;
                }
                MainRenderer.enabled = true;
                WeaverLog.Log("FADING IN = " + FadeTime);
                fadeRoutine = StartCoroutine(FadeRoutine(FromColor, ToColor, FadeTime, FadeInEaseType.ToAnimationCurve()));
            }
        }
        
        IEnumerator FadeRoutine(Color from, Color to, float time, AnimationCurve curve, Action onDone = null)
        {
            WeaverLog.Log($"Starting Fade from {from} to {to} in time {time}");
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                WeaverLog.Log($"T = {t}, Eval = {curve.Evaluate(t / time)}");
                MainRenderer.color = Color.Lerp(from, to, curve.Evaluate(t / time));
                yield return null;
            }
            MainRenderer.color = to;
            fadeRoutine = null;

            onDone?.Invoke();
        }

        public void FadeOut()
        {
            if (hasFSM)
            {
                blankerFSM.SendEvent("FADE OUT");
            }
            else
            {
                if (fadeRoutine != null)
                {
                    StopCoroutine(fadeRoutine);
                    fadeRoutine = null;
                }
                MainRenderer.enabled = true;
                WeaverLog.Log("FADING OUT = " + FadeTime);
                fadeRoutine = StartCoroutine(FadeRoutine(ToColor, FromColor, FadeTime, FadeInEaseType.ToAnimationCurve(), () => MainRenderer.enabled = false));
            }
        }

        public static WeaverBlanker Create(string name, Color from, Color to, float fadeTime, WeaverEaseType fadeInEaseType = WeaverEaseType.linear, WeaverEaseType fadeOutEaseType = WeaverEaseType.linear)
        {
            var hudCamera = GameObject.FindObjectOfType<HUDCamera>();
            if (hudCamera != null)
            {
                var template = WeaverAssets.LoadWeaverAsset<GameObject>("Blanker Template");
                var instance = GameObject.Instantiate(template, hudCamera.transform.parent);
                instance.transform.SetParent(hudCamera.transform);
                instance.transform.localPosition = template.transform.localPosition;
                instance.transform.localRotation = template.transform.localRotation;
                instance.transform.localScale = template.transform.localScale;
                instance.name = name;

                var blanker = instance.GetComponent<WeaverBlanker>();
                blanker.FromColor = from;
                blanker.ToColor = to;
                blanker.FadeTime = fadeTime;
                blanker.FadeInEaseType = fadeInEaseType;
                blanker.FadeOutEaseType = fadeOutEaseType;

                return blanker;
            }

            return null;
        }
    }
}