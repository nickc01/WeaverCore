using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Enums;
using WeaverCore.Utilities;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// Used to show an area title in the corner of the screen. Used to show the name of bosses or NPCs
    /// </summary>
    [ExecuteInEditMode]
    public class WeaverBossTitle : MonoBehaviour
    {
        static ObjectPool AreaTitlePool;
        static GameObject Prefab;


        [SerializeField]
        RectTransform TextContainer;

        [SerializeField]
        TextMeshProUGUI _bottomText;
        /// <summary>
        /// The bottom (larger) text in the title
        /// </summary>
        public string BottomText
        {
            get
            {
                return _bottomText.text;
            }
            set
            {
                _bottomText.text = value;
            }
        }

        /// <summary>
        /// The object that displays the bottom (larger) text in the title
        /// </summary>
        public TextMeshProUGUI BottomTextObject
        {
            get
            {
                return _bottomText;
            }
        }

        [SerializeField]
        TextMeshProUGUI _topText;
        /// <summary>
        /// The top (smaller) text in the title
        /// </summary>
        public string TopText
        {
            get
            {
                return _topText.text;
            }
            set
            {
                _topText.text = value;
            }
        }

        /// <summary>
        /// The object used to display the top (smaller) text in the title
        /// </summary>
        public TextMeshProUGUI TopTextObject
        {
            get
            {
                return _topText;
            }
        }

        /// <summary>
        /// The color the text will be when faded in
        /// </summary>
        public Color FadedInColor = Color.white;

        /// <summary>
        /// The color the text will be when faded out
        /// </summary>
        public Color FadedOutColor = default;

        /// <summary>
        /// Is the text currently fading in/out?
        /// </summary>
        public bool DoingFade
        {
            get
            {
                return fadeCoroutine != null;
            }
        }
        Coroutine fadeCoroutine;

        [SerializeField]
        AreaTitlePosition _position = AreaTitlePosition.BottomLeft;
        /// <summary>
        /// The screen-relative position of the area title
        /// </summary>
        public AreaTitlePosition Position
        {
            get
            {
                return _position;
            }
            set
            {
                if (value != _position)
                {
                    _position = value;
                    UpdatePosition(value);
                }
            }
        }

        void Update()
        {
            if (!Application.isPlaying)
            {
                UpdatePosition(_position);
            }
        }

        /// <summary>
        /// Causes the area title to fade
        /// </summary>
        /// <param name="fadeInTime">The amount of time the text will take to fade in</param>
        /// <param name="waitTime">How long the text will wait before fading out</param>
        /// <param name="fadeOutTime">The amount of time the text will take to fade out</param>
        /// <param name="deleteWhenDone">Should the <see cref="WeaverBossTitle"/> be deleted when done?</param>
        public void DoFade(float fadeInTime, float waitTime, float fadeOutTime, bool deleteWhenDone)
        {
            if (DoingFade)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            BottomTextObject.color = FadedOutColor;

            fadeCoroutine = StartCoroutine(FadeRoutine(fadeInTime, waitTime, fadeOutTime, deleteWhenDone));
        }

        IEnumerator FadeRoutine(float fadeInTime, float waitTime, float fadeOutTime, bool deleteWhenDone)
        {
            for (float i = 0; i < fadeInTime; i += Time.deltaTime)
            {
                BottomTextObject.color = Color.Lerp(FadedOutColor, FadedInColor, i / fadeInTime);
                TopTextObject.color = BottomTextObject.color;
                yield return null;
            }

            BottomTextObject.color = FadedInColor;
            TopTextObject.color = FadedInColor;

            for (float i = 0; i < waitTime; i += Time.deltaTime)
            {
                yield return null;
            }

            for (float i = 0; i < fadeOutTime; i += Time.deltaTime)
            {
                BottomTextObject.color = Color.Lerp(FadedInColor, FadedOutColor, i / fadeOutTime);
                TopTextObject.color = BottomTextObject.color;
                yield return null;
            }

            BottomTextObject.color = FadedOutColor;
            TopTextObject.color = FadedOutColor;

            fadeCoroutine = null;

            if (deleteWhenDone)
            {
                Delete();
            }
        }

        void UpdatePosition(AreaTitlePosition position)
        {
            switch (position)
            {
                case AreaTitlePosition.BottomLeft:
                    TextContainer.anchorMin = new Vector2(0.0f, 0.0f);
                    break;
                case AreaTitlePosition.BottomCenter:
                    TextContainer.anchorMin = new Vector2(0.5f, 0.0f);
                    break;
                case AreaTitlePosition.BottomRight:
                    TextContainer.anchorMin = new Vector2(1.0f, 0.0f);
                    break;
                case AreaTitlePosition.MiddleLeft:
                    TextContainer.anchorMin = new Vector2(0f, 0.5f);
                    break;
                case AreaTitlePosition.MiddleCenter:
                    TextContainer.anchorMin = new Vector2(0.5f, 0.5f);
                    break;
                case AreaTitlePosition.MiddleRight:
                    TextContainer.anchorMin = new Vector2(1f, 0.5f);
                    break;
                case AreaTitlePosition.TopLeft:
                    TextContainer.anchorMin = new Vector2(0f, 1f);
                    break;
                case AreaTitlePosition.TopCenter:
                    TextContainer.anchorMin = new Vector2(0.5f, 1f);
                    break;
                case AreaTitlePosition.TopRight:
                    TextContainer.anchorMin = new Vector2(1f, 1f);
                    break;
                default:
                    break;
            }
            TextContainer.anchorMax = TextContainer.anchorMin;
            TextContainer.pivot = TextContainer.anchorMin;
            TextContainer.anchoredPosition = Vector2.zero;
        }

        /// <summary>
        /// Deletes the Area Title
        /// </summary>
        public void Delete()
        {
            AreaTitlePool.ReturnToPool(this);
        }

        /// <summary>
        /// Spawns the Area Title, but will not fade in/out by default. You will need to control this manually by calling <see cref="DoFade(float, float, float, bool)"/>
        /// </summary>
        /// <returns>Returns the new area title object</returns>
        public static WeaverBossTitle SpawnNoFade()
        {
            return SpawnNoFade("Top Text", "Bottom Text");
        }

        /// <summary>
        /// Spawns the Area Title, but will not fade in/out by default. You will need to control this manually by calling <see cref="DoFade(float, float, float, bool)"/>
        /// </summary>
        /// <param name="topText">The top (smaller) text</param>
        /// <param name="bottomText">The bottom (larger) text</param>
        /// <returns>Returns the new area title object</returns>
        public static WeaverBossTitle SpawnNoFade(string topText, string bottomText)
        {
            if (AreaTitlePool == null)
            {
                Prefab = WeaverAssets.LoadWeaverAsset<GameObject>("Area Title Small");
                AreaTitlePool = ObjectPool.Create(Prefab);
            }

            var title = AreaTitlePool.Instantiate<WeaverBossTitle>(WeaverCanvas.SceneContent);
            title.TopText = topText;
            title.BottomText = bottomText;
            title.transform.SetZLocalPosition(0f);
            var oldRect = Prefab.GetComponent<RectTransform>();
            var newRect = title.GetComponent<RectTransform>();

            newRect.sizeDelta = oldRect.sizeDelta;
            newRect.anchoredPosition = oldRect.anchoredPosition;
            newRect.localScale = oldRect.localScale;

            return title;
        }

        /// <summary>
        /// Spawns an Area Title
        /// </summary>
        /// <param name="topText">The top (smaller) text</param>
        /// <param name="bottomText">The bottom (larger) text</param>
        /// <param name="fadeInTime">The amount of time the text will take to fade in</param>
        /// <param name="waitTime">How long the text will wait before fading out</param>
        /// <param name="fadeOutTime">The amount of time the text will take to fade out</param>
        /// <param name="deleteWhenDone">Should the <see cref="WeaverBossTitle"/> be deleted when done?</param>
        /// <returns>Returns the new area title object</returns>
        public static WeaverBossTitle Spawn(string topText, string bottomText, float fadeInTime = 1.5f, float waitTime = 4.5f, float fadeOutTime = 1.5f, bool deleteWhenDone = true)
        {
            var title = SpawnNoFade(topText, bottomText);
            title.DoFade(fadeInTime, waitTime, fadeOutTime, deleteWhenDone);
            return title;
        }

        /// <summary>
        /// Spawns an Area Title
        /// </summary>
        /// <param name="fadeInTime">The amount of time the text will take to fade in</param>
        /// <param name="waitTime">How long the text will wait before fading out</param>
        /// <param name="fadeOutTime">The amount of time the text will take to fade out</param>
        /// <param name="deleteWhenDone">Should the <see cref="WeaverBossTitle"/> be deleted when done?</param>
        /// <returns>Returns the new area title object</returns>
        public static WeaverBossTitle Spawn(float fadeInTime = 1.5f, float waitTime = 4.5f, float fadeOutTime = 1.5f, bool deleteWhenDone = true)
        {
            return Spawn("Top Text", "Bottom Text", fadeInTime, waitTime, fadeOutTime, deleteWhenDone);
        }
    }
}
