using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// Represents a message displayed when an item is obtained in the game.
    /// </summary>
    public class ItemGetMessage : MonoBehaviour
    {
        [Tooltip("The sprite renderer for the item icon.")]
        [SerializeField]
        private SpriteRenderer _icon;

        [Tooltip("The backboard sprite renderer behind the item text.")]
        [SerializeField]
        private SpriteRenderer _backboard;

        [Tooltip("The TextMeshPro component for displaying the item text.")]
        [SerializeField]
        private TextMeshPro _text;

        [Tooltip("Time taken for the message to move upwards when shown.")]
        [SerializeField]
        private float upTime = 0.3f;

        [Tooltip("Time taken for the message to move downwards when hidden.")]
        [SerializeField]
        private float downTime = 0.2f;

        [Tooltip("Total time the message is displayed.")]
        [SerializeField]
        private float showTime = 4.25f;

        /// <summary>
        /// Gets or sets the icon of the item.
        /// </summary>
        public Sprite Icon
        {
            get => _icon.sprite;
            set
            {
                _icon.sprite = value;
                Vector3 scale = value.bounds.size;
                scale.x = (1f / scale.x) + 0.1666667f;
                scale.y = (1f / scale.y) + 0.1666667f;
                scale.z = 1f;
                _icon.transform.localScale = scale;
            }
        }

        RectTransform _textTransform;

        /// <summary>
        /// Gets or sets the text of the item message.
        /// </summary>
        public string Text
        {
            get => _text.text;
            set
            {
                _text.text = value;

                if (_textTransform == null)
                {
                    _textTransform = _text.GetComponent<RectTransform>();
                }

                _textTransform.pivot = _textTransform.pivot.With(x: 0f);

                var width = Mathf.Clamp(_text.preferredWidth, 6f, float.PositiveInfinity);

                var backBoardWidth = 12f + (width - 6f);

                var spriteWidth = backBoardWidth / _backboard.transform.localScale.x;

                _backboard.size = _backboard.size.With(x: spriteWidth);
            }
        }

        private EventListener listener;
        private Coroutine fadeRoutine;

        private void Awake()
        {
            _icon.transform.localScale = Vector3.one;
            EventManager.BroadcastEvent("DESTROY JOURNAL MSG", gameObject);
            listener = GetComponent<EventListener>();
            listener.ListenForEvent("DESTROY JOURNAL MSG", (source, destination) =>
            {
                StopAllCoroutines();
                Destroy(gameObject);
            });

            if (_textTransform == null)
            {
                _textTransform = _text.GetComponent<RectTransform>();
            }
            _textTransform.pivot = _textTransform.pivot.With(x: 0f);

            HideInstant();
        }

        private void Start()
        {
            StartCoroutine(MainRoutine());
        }

        /// <summary>
        /// Main coroutine for controlling the item message lifecycle.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MainRoutine()
        {
            transform.position = new Vector3(-11.91f, -6.22f);

            Show();

            yield return new WaitForSeconds(showTime);

            Hide();

            yield return new WaitForSeconds(1f);

            Destroy(gameObject);
        }

        /// <summary>
        /// Displays the item message instantly without animation.
        /// </summary>
        public void ShowInstant()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            var graphics = GetComponentsInChildren<Graphic>();

            foreach (var graphic in graphics)
            {
                graphic.color = graphic.color.With(a: 1f);
            }
        }

        /// <summary>
        /// Hides the item message instantly without animation.
        /// </summary>
        private void HideInstant()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            var graphics = GetComponentsInChildren<Graphic>();

            foreach (var graphic in graphics)
            {
                graphic.color = graphic.color.With(a: 0f);
            }
        }

        /// <summary>
        /// Shows the item message with a fade-in animation.
        /// </summary>
        public void Show()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
            fadeRoutine = StartCoroutine(ShowRoutine());
        }

        /// <summary>
        /// Hides the item message with a fade-out animation.
        /// </summary>
        public void Hide()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
            fadeRoutine = StartCoroutine(HideRoutine());
        }

        /// <summary>
        /// Coroutine for fading in the item message.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShowRoutine()
        {
            var graphics = GetComponentsInChildren<Graphic>();

            for (float t = 0; t < upTime; t += Time.deltaTime)
            {
                foreach (var graphic in graphics)
                {
                    graphic.color = graphic.color.With(a: Mathf.Lerp(0f, 1f, t / upTime));
                }
                yield return null;
            }
        }

        /// <summary>
        /// Coroutine for fading out the item message.
        /// </summary>
        /// <returns></returns>
        private IEnumerator HideRoutine()
        {
            var graphics = GetComponentsInChildren<Graphic>();

            for (float t = 0; t < downTime; t += Time.deltaTime)
            {
                foreach (var graphic in graphics)
                {
                    graphic.color = graphic.color.With(a: Mathf.Lerp(1f, 0f, t / downTime));
                }
                yield return null;
            }
            yield break;
        }

        /// <summary>
        /// Spawns an item message for a specific WeaverCharm.
        /// </summary>
        /// <param name="charm">The WeaverCharm to display the message for.</param>
        /// <returns>The spawned ItemGetMessage instance.</returns>
        public static ItemGetMessage SpawnCharm(IWeaverCharm charm)
        {
            return SpawnCharm(CharmUtilities.GetCustomCharmID(charm));
        }

        /// <summary>
        /// Spawns an item message for a specific charm ID.
        /// </summary>
        /// <param name="charmID">The ID of the charm to display the message for.</param>
        /// <returns>The spawned ItemGetMessage instance.</returns>
        public static ItemGetMessage SpawnCharm(int charmID)
        {
            Sprite charmIcon = null;

            if (CharmIconList.Instance != null)
            {
                charmIcon = CharmIconList.Instance.GetSprite(charmID);
            }
            else
            {
                foreach (var charm in Registry.GetAllFeatures<IWeaverCharm>())
                {
                    if (!CharmUtilities.CharmDisabled(charm) && CharmUtilities.GetCustomCharmID(charm) == charmID)
                    {
                        charmIcon = charm.CharmSprite;
                        break;
                    }
                }
            }

            return Spawn(charmIcon, WeaverLanguage.GetString($"CHARM_NAME_{charmID}", "UI", "Unknown Charm"));
        }

        /// <summary>
        /// Spawns an item message for a specific item sprite and text.
        /// </summary>
        /// <param name="itemSprite">The sprite of the item to display.</param>
        /// <param name="itemText">The text to display for the item message.</param>
        /// <returns>The spawned ItemGetMessage instance.</returns>
        public static ItemGetMessage Spawn(Sprite itemSprite, string itemText)
        {
            var prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Item Get Message");

            var instance = GameObject.Instantiate(prefab).GetComponent<ItemGetMessage>();
            instance.Icon = itemSprite;
            instance.Text = itemText;
            return instance;
        }
    }
}
