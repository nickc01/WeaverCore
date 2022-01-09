using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using TMPro;

namespace WeaverCore.Settings
{
	/// <summary>
	/// The main button on the main menu and pause menu for opening up the Weaver Settings Menu
	/// </summary>
	public class ConfigButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		[SerializeField]
		[Tooltip("How fast to interpolate from the button's shrunken size to expanded size, and vice versa")]
		float interpolationTime = 0.4f;
		[SerializeField]
		[Tooltip("The curve to be applied when interpolating the button's size")]
		AnimationCurve interpolationCurve;

		[SerializeField]
		[Tooltip("How big the button is when expanded")]
		Vector2 ExpandedSize;
		[SerializeField]
		[Tooltip("How big the button is when shrunken")]
		Vector2 ShrunkSize;

		[SerializeField]
		[Tooltip("The icon that is shown when the button is shrunken")]
		RawImage hamburgerIcon;
		[SerializeField]
		[Tooltip("The text that is shown when the button is expanded")]
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

			hamburgerIconColorFaded = new Color(hamburgerIconColor.r, hamburgerIconColor.g, hamburgerIconColor.b,0f);
			buttonTextColorFaded = new Color(buttonTextColor.r, buttonTextColor.g, buttonTextColor.b, 0f);

			ShrinkInstant();
		}

		/// <summary>
		/// Called when the mouse hovers over this button
		/// </summary>
		public void OnPointerEnter(PointerEventData eventData)
		{
			ChangeSize(ExpandedSize,hamburgerIconColorFaded,buttonTextColor);
		}

		/// <summary>
		/// Called when the mouse stops hovering over this button
		/// </summary>
		public void OnPointerExit(PointerEventData eventData)
		{
			ChangeSize(ShrunkSize, hamburgerIconColor,buttonTextColorFaded);
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
				rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Lerp(originalSize.x,destSize.x,interpolationCurve.Evaluate(t / interpolationTime)));
				rTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Lerp(originalSize.y,destSize.y,interpolationCurve.Evaluate(t / interpolationTime)));

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
