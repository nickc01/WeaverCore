using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    public class ItemGetMessage : MonoBehaviour
    {
        [SerializeField]
        SpriteRenderer _icon;

        [SerializeField]
        SpriteRenderer _backboard;

        [SerializeField]
        TextMeshPro _text;

        [SerializeField]
        float upTime = 0.3f;

        [SerializeField]
        float downTime = 0.2f;

        [SerializeField]
        float showTime = 4.25f;

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
        public string Text
        {
            get => _text.text;
            set
            {
                _text.text = value;

                var width = Mathf.Clamp(_text.preferredWidth, 6f, float.PositiveInfinity);

                var backBoardWidth = 12f + (width - 6f);

                var spriteWidth = backBoardWidth / _backboard.transform.localScale.x;

                _backboard.size = _backboard.size.With(x: spriteWidth);
            }
        }

        EventListener listener;
        Coroutine fadeRoutine;

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

            HideInstant();
        }

        private void Start()
        {
            StartCoroutine(MainRoutine());
        }

        IEnumerator MainRoutine()
        {
            transform.position = new Vector3(-11.91f, -6.22f);

            Show();

            yield return new WaitForSeconds(showTime);

            Hide();

            yield return new WaitForSeconds(1f);

            Destroy(gameObject);
        }

        public void Show()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
            fadeRoutine = StartCoroutine(ShowRoutine());
        }

        public void Hide()
        {
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }
            fadeRoutine = StartCoroutine(HideRoutine());
        }

        IEnumerator ShowRoutine()
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

        IEnumerator HideRoutine()
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

        void HideInstant()
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

        void ShowInstant()
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

        public static ItemGetMessage SpawnCharm(IWeaverCharm charm)
        {
            return SpawnCharm(CharmUtilities.GetCustomCharmID(charm));
        }

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
