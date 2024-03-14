using GlobalEnums;
using System;
using System.Collections;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    public class UIGetItemMessage : MonoBehaviour
    {
        static CachedPrefab<UIGetItemMessage> _defaultPrefab = new CachedPrefab<UIGetItemMessage>();

        public static UIGetItemMessage DefaultPrefab
        {
            get
            {
                if (_defaultPrefab.Value == null)
                {
                    _defaultPrefab.Value = WeaverAssets.LoadWeaverAsset<GameObject>("UI Msg Get Item").GetComponent<UIGetItemMessage>();
                }
                return _defaultPrefab.Value;
            }
        }

        public event Action OnDone;

        public bool IsDone { get; private set; } = false;

        MonoBehaviour actionButtonInstance = null;

        [FormerlySerializedAs("onOpenSound")]
        public AudioClip OnOpenSound;

        [FormerlySerializedAs("buttonPressSound")]
        public AudioClip ButtonPressSound;

        [FormerlySerializedAs("fleur")]
        public ColorFader Fleur;

        [FormerlySerializedAs("stop")]
        public ColorFader Stop;

        [FormerlySerializedAs("background")]
        public ColorFader Background;

        [FormerlySerializedAs("button")]
        public ColorFader Button;

        public ColorFader ButtonCharacter;

        [FormerlySerializedAs("ConfirmButton")]
        [SerializeField]
        HeroActionButton buttonIconAction = HeroActionButton.DASH;

        [Space]
        [Header("Timings")]
        [SerializeField]
        float openSoundDelay = 0.75f;

        [SerializeField]
        float openSoundPitch = 1f;

        [SerializeField]
        float extraTextDelay = 2.5f;

        [SerializeField]
        float stopDelay = 3f;

        public HeroActionButton ButtonIconAction
        {
            get => buttonIconAction;
            set
            {
                if (buttonIconAction != value)
                {
                    buttonIconAction = value;

                    if (actionButtonInstance != null)
                    {
                        actionButtonInstance.ReflectSetField("action", buttonIconAction);
                        actionButtonInstance.GetType().BaseType.GetMethod("RefreshButtonIcon").Invoke(actionButtonInstance, null);
                    }
                }
            }
        }

        public SpriteRenderer IconRenderer;

        public TextMeshPro ItemNameRenderer;

        public TextMeshPro ItemNamePrefixRenderer;

        public TextMeshPro PressTextRenderer;

        public TextMeshPro Msg1Renderer;

        public TextMeshPro Msg2Renderer;

        public OnDoneBehaviour OnDoneBehavior = OnDoneBehaviour.Destroy;

        public Sprite Icon
        {
            get => NullPass(IconRenderer)?.sprite;
            set => TrySet(IconRenderer, value);
        }

        public string Title
        {
            get => NullPass(ItemNameRenderer)?.text;
            set => TrySet(ItemNameRenderer, value);
        }

        public string TitlePrefix
        {
            get => NullPass(ItemNamePrefixRenderer)?.text;
            set => TrySet(ItemNamePrefixRenderer, value);
        }

        public string Extra1
        {
            get => NullPass(Msg1Renderer)?.text;
            set => TrySet(Msg1Renderer, value);
        }

        public string Extra2
        {
            get => NullPass(Msg2Renderer)?.text;
            set => TrySet(Msg2Renderer, value);
        }

        Coroutine mainRoutine;

        static Type buttonIconComponentType;
        static MethodInfo buttonIconAwakeMethod;


        protected virtual void Awake()
        {
            if (buttonIconComponentType == null)
            {
                buttonIconComponentType = typeof(BossScene).Assembly.GetType("ActionButtonIcon");

                if (buttonIconComponentType != null)
                {
                    buttonIconAwakeMethod = buttonIconComponentType.BaseType.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
                }
            }

            if (buttonIconComponentType != null && Button != null)
            {
                if (Button.TryGetComponent(buttonIconComponentType, out var result))
                {
                    actionButtonInstance = (MonoBehaviour)result;
                }
                else
                {
                    var instance = (MonoBehaviour)Button.gameObject.AddComponent(buttonIconComponentType);

                    actionButtonInstance = instance;
                }

                buttonIconAwakeMethod.Invoke(actionButtonInstance, null);
                actionButtonInstance.ReflectSetField("action", buttonIconAction);
            }
        }

        protected virtual void OnEnable()
        {
            if (mainRoutine != null)
            {
                StopCoroutine(mainRoutine);
            }

            IsDone = false;
            mainRoutine = StartCoroutine(MainRoutine());
        }

        protected virtual void OnDisable()
        {
            if (mainRoutine != null)
            {
                StopCoroutine(mainRoutine);
                mainRoutine = null;
            }
        }

        protected static T NullPass<T>(T input) where T : UnityEngine.Object
        {
            if (input == null)
            {
                return null;
            }
            return input;
        }

        protected static void TrySet(SpriteRenderer rend, Sprite sprite)
        {
            if (rend != null)
            {
                rend.sprite = sprite;
            }
        }

        protected static void TrySet(TextMeshPro tRenderer, string text)
        {
            if (tRenderer != null)
            {
                tRenderer.text = text;
            }
        }

        protected void StartFadeIn()
        {
            NullPass(Background)?.Fade(true);
            GameManager.instance.SaveGame();
            if (OnOpenSound != null)
            {
                var instance = WeaverAudio.PlayAtPointDelayed(openSoundDelay, OnOpenSound, transform.position);
                instance.AudioSource.pitch = openSoundPitch;
            }

            NullPass(IconRenderer)?.GetComponent<ColorFader>().Fade(true);
            NullPass(ItemNameRenderer)?.GetComponent<ColorFader>().Fade(true);
            NullPass(ItemNamePrefixRenderer)?.GetComponent<ColorFader>().Fade(true);

            NullPass(Fleur)?.gameObject.SetActive(true);
            NullPass(Fleur)?.Fade(true);

            if (actionButtonInstance != null)
            {
                actionButtonInstance.GetType().BaseType.GetMethod("RefreshButtonIcon").Invoke(actionButtonInstance, null);
            }

        }

        protected IEnumerator EndFadeOut()
        {
            foreach (var fader in gameObject.GetComponentsInChildren<ColorFader>())
            {
                fader.Fade(false);
            }

            if (ButtonPressSound != null)
            {
                var instance = WeaverAudio.PlayAtPoint(ButtonPressSound, transform.position, 0.3f);
                instance.AudioSource.pitch = 0.8f;
            }

            yield return new WaitForSeconds(1f);

            EventManager.BroadcastEvent("GET ITEM MSG END", gameObject);

            OnDone?.Invoke();

            NullPass(Fleur)?.gameObject.SetActive(false);
            NullPass(Stop)?.gameObject.SetActive(false);

            IsDone = true;
            OnDoneBehavior.DoneWithObject(this);
        }

        protected virtual IEnumerator MainRoutine(bool doFadeIn = true, bool doFadeOut = true)
        {
            if (doFadeIn)
            {
                StartFadeIn();
            }
            yield return new WaitForSeconds(extraTextDelay);

            NullPass(PressTextRenderer)?.GetComponent<ColorFader>().Fade(true);
            NullPass(Msg1Renderer)?.GetComponent<ColorFader>().Fade(true);
            NullPass(Msg2Renderer)?.GetComponent<ColorFader>().Fade(true);
            NullPass(Button)?.Fade(true);
            NullPass(ButtonCharacter)?.Fade(true);

            yield return new WaitForSeconds(stopDelay);

            NullPass(Stop)?.gameObject.SetActive(true);
            NullPass(Stop)?.Fade(true);

            yield return new WaitForSeconds(0.25f);

            while (!(PlayerInput.jump.IsPressed || PlayerInput.attack.IsPressed || PlayerInput.pause.IsPressed || PlayerInput.jump.WasPressed || PlayerInput.attack.WasPressed || PlayerInput.jump.WasPressed))
            {
                yield return null;
            }

            if (doFadeOut)
            {
                yield return EndFadeOut();
            }
        }

        public static UIGetItemMessage Spawn(UIGetItemMessage prefab = null)
        {
            if (prefab == null)
            {
                prefab = DefaultPrefab;
            }

            var instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);

#if UNITY_EDITOR
            instance.transform.SetZLocalPosition(10f);
#endif
            return instance;
        }

        public static UIGetItemMessage Spawn(Sprite icon, string prefix, string title, string extra1, string extra2, HeroActionButton? buttonIcon = null, UIGetItemMessage prefab = null)
        {
            if (prefab == null)
            {
                prefab = DefaultPrefab;
            }

            var instance = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);

#if UNITY_EDITOR
            instance.transform.SetZLocalPosition(10f);
#endif

            instance.Icon = icon;
            instance.TitlePrefix = prefix;
            instance.Title = title;
            instance.Extra1 = extra1;
            instance.Extra2 = extra2;
            if (buttonIcon == null)
            {
                instance.Button.gameObject.SetActive(false);
                instance.PressTextRenderer.gameObject.SetActive(false);
            }
            else
            {
                instance.Button.gameObject.SetActive(true);
                instance.PressTextRenderer.gameObject.SetActive(true);
                instance.ButtonIconAction = buttonIcon.Value;
            }

            return instance;
        }

        public static UIGetItemMessage SpawnPooled(UIGetItemMessage prefab = null)
        {
            if (prefab == null)
            {
                prefab = DefaultPrefab;
            }

            var instance = Pooling.Instantiate(prefab, Vector3.zero, Quaternion.identity);

#if UNITY_EDITOR
            instance.transform.SetZLocalPosition(10f);
#endif
            return instance;
        }

        public static UIGetItemMessage SpawnPooled(Sprite icon, string prefix, string title, string extra1, string extra2, HeroActionButton? buttonIcon = null, UIGetItemMessage prefab = null)
        {
            if (prefab == null)
            {
                prefab = DefaultPrefab;
            }

            var instance = Pooling.Instantiate(prefab, Vector3.zero, Quaternion.identity);

#if UNITY_EDITOR
            instance.transform.SetZLocalPosition(10f);
#endif

            instance.Icon = icon;
            instance.TitlePrefix = prefix;
            instance.Title = title;
            instance.Extra1 = extra1;
            instance.Extra2 = extra2;
            if (buttonIcon == null)
            {
                instance.Button.gameObject.SetActive(false);
            }
            else
            {
                instance.Button.gameObject.SetActive(true);
                instance.ButtonIconAction = buttonIcon.Value;
            }

            return instance;
        }

        public static void FreezePlayer()
        {
            EventManager.SendEventToGameObject("FSM CANCEL", Player.Player1.gameObject);
            HeroController.instance.RelinquishControl();
        }

        public static void UnFreezePlayer()
        {
            HeroController.instance.RegainControl();
        }

        public IEnumerator WaitUntilDone(bool freezePlayer)
        {
            if (this == null || gameObject == null || gameObject.activeSelf == false || enabled == false || IsDone == true)
            {
                yield break;
            }
            if (freezePlayer)
            {
                FreezePlayer();
            }

            yield return new WaitUntil(() => this == null || gameObject == null || gameObject.activeSelf == false || enabled == false || IsDone == true);

            if (freezePlayer)
            {
                UnFreezePlayer();
            }
        }
    }
}
