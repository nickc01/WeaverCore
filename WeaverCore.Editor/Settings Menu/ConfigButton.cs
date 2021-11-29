using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

namespace WeaverCore.Editor.Settings
{
    public class ConfigButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        float interpolationTime = 0.4f;
        [SerializeField]
        AnimationCurve interpolationCurve;

        [SerializeField]
        Vector2 ExpandedSize;
        [SerializeField]
        Vector2 ShrunkSize;

        [SerializeField]
        RawImage hamburgerIcon;
        [SerializeField]
        TextMeshProUGUI buttonText;



        Button button;
        RectTransform rTransform;
        Color hamburgerIconColor;
        Color buttonTextColor;

        Color hamburgerIconColorFaded;
        Color buttonTextColorFaded;

        void Awake()
        {
            button = GetComponent<Button>();
            rTransform = GetComponent<RectTransform>();
            hamburgerIconColor = hamburgerIcon.color;
            buttonTextColor = buttonText.color;

            hamburgerIconColorFaded = new Color(hamburgerIconColor.r, hamburgerIconColor.g, hamburgerIconColor.b, 0f);
            buttonTextColorFaded = new Color(buttonTextColor.r, buttonTextColor.g, buttonTextColor.b, 0f);

            ShrinkInstant();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            ChangeSize(ExpandedSize, hamburgerIconColorFaded, buttonTextColor);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ChangeSize(ShrunkSize, hamburgerIconColor, buttonTextColorFaded);
        }

        public void ChangeSize(Vector2 destSize, Color hamburgerColor, Color buttonColor)
        {
            if (sizeRoutine != null)
            {
                StopCoroutine(sizeRoutine);
            }
            sizeRoutine = StartCoroutine(ChangeSizeRoutine(destSize, hamburgerColor, buttonColor));
        }

        public void ChangeSizeInstant(Vector2 destSize)
        {
            if (sizeRoutine != null)
            {
                StopCoroutine(sizeRoutine);
                sizeRoutine = null;
            }
            rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, destSize.x);
            rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, destSize.y);
        }

        public void ShrinkInstant()
        {
            ChangeSizeInstant(ShrunkSize);
            hamburgerIcon.color = hamburgerIconColor;
            buttonText.color = buttonTextColorFaded;
        }

        Coroutine sizeRoutine;
        IEnumerator ChangeSizeRoutine(Vector2 destSize, Color hamburgerColor, Color buttonColor)
        {
            Color oldHamburgerColor = hamburgerIcon.color;
            Color oldButtonColor = buttonText.color;

            Vector2 originalSize = rTransform.sizeDelta;
            for (float t = 0; t < interpolationTime; t += Time.unscaledDeltaTime)
            {
                rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(originalSize.x, destSize.x, interpolationCurve.Evaluate(t / interpolationTime)));
                rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(originalSize.y, destSize.y, interpolationCurve.Evaluate(t / interpolationTime)));

                buttonText.color = Color.Lerp(oldButtonColor, buttonColor, interpolationCurve.Evaluate(t / interpolationTime));
                hamburgerIcon.color = Color.Lerp(oldHamburgerColor, hamburgerColor, interpolationCurve.Evaluate(t / interpolationTime));
                yield return null;
            }

            rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, destSize.x);
            rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, destSize.y);

            buttonText.color = buttonColor;
            hamburgerIcon.color = hamburgerColor;

            sizeRoutine = null;
        }
    }
}
