using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Interfaces;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
	/// <summary>
	/// The message that is displayed when a new charm is acquired
	/// </summary>
    public class GetCharmMessage : MonoBehaviour
	{
        [SerializeField]
        SpriteRenderer icon;

        [SerializeField]
        TextMeshPro text;

        [SerializeField]
        float upTime = 0.3f;

        [SerializeField]
        float downTime = 0.2f;

        [SerializeField]
        float showTime = 4.25f;

        int charmID;

        EventListener listener;

        Coroutine fadeRoutine;

        private void Awake()
        {
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

            Sprite charmIcon = null;

            if (CharmIconList.Instance != null)
            {
                //WeaverLog.Log("CHARM ICON LIST FOUND");
                //WeaverLog.Log("ID 123 = " + charmID);
                charmIcon = CharmIconList.Instance.GetSprite(charmID);
                //WeaverLog.Log("LIST CHARM ICON = " + charmIcon);
            }
            else
            {
                foreach (var charm in Registry.GetAllFeatures<IWeaverCharm>())
                {
                    //WeaverLog.Log("CHARMDEF = " + charm.GetType());
                    //WeaverLog.Log("DISABLED = " + CharmUtilities.CharmDisabled(charm));
                    //WeaverLog.Log("CHARM ID 1 = " + CharmUtilities.GetCustomCharmID(charm));
                    if (!CharmUtilities.CharmDisabled(charm) && CharmUtilities.GetCustomCharmID(charm) == charmID)
                    {
                        charmIcon = charm.CharmSprite;
                        break;
                    }
                }
            }

            /*if (charmIcon == null)
            {
                
            }*/
            //WeaverLog.Log("CHARM ICON 2 = " + charmIcon);
            icon.sprite = charmIcon;

            //WeaverLog.Log("Sprite = " + icon.sprite);

            //WeaverLog.Log("CHARM NAME ID = " + $"CHARM_NAME_{charmID}");

            var charmText = WeaverLanguage.GetString($"CHARM_NAME_{charmID}", "UI", "Unknown Charm");
            //WeaverLog.Log("CHARM TEXT = " + charmText);
            text.text = charmText;

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
                    graphic.color = graphic.color.With(a: Mathf.Lerp(0f,1f,t / upTime));
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

        public static GetCharmMessage Spawn(int charmID)
        {
            var prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Charm Get Message");

            var instance = GameObject.Instantiate(prefab).GetComponent<GetCharmMessage>();
            instance.charmID = charmID;
            return instance;
        }
    }
}
