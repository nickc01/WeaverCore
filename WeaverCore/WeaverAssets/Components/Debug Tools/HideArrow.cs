using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using WeaverCore.Utilities;

namespace WeaverCore.Assets
{
    public class HideArrow : MonoBehaviour
    {
        [SerializeField]
        RectTransform parent;

        [SerializeField]
        [Tooltip("The position the parent should be located when shown")]
        Vector2 shownPosition;

        [SerializeField]
        [Tooltip("The position the parent should be located when hidden")]
        Vector2 hiddenPosition;

        [SerializeField]
        [Tooltip("The time it takes for the parent to interpolate to the new position")]
        float time;

        [SerializeField]
        AnimationCurve interpolationCurve;


        [SerializeField]
        bool hiddenByDefault = false;

        //[SerializeField]
        //[Tooltip("The direction the arrow faces when open")]
        //bool arrowFacesRight = true;

        bool hidden = false;
        Button button;
        Coroutine interRoutine;

		private void Awake()
		{
            button = GetComponent<Button>();
            button.onClick.AddListener(Toggle);
            Debug.Log("Anchored Position = " + parent.anchoredPosition);
            if (hiddenByDefault)
            {
                parent.anchoredPosition = hiddenPosition;
            }
            else
			{
                parent.anchoredPosition = shownPosition;
            }
            hidden = hiddenByDefault;
            UpdateArrowVisual();
        }

        void UpdateArrowVisual()
		{
            //arrowFacesRight -- TODO TODO TODO
            /*if (!hidden)
			{
                transform.localScale = transform.localScale.With(x: -1f);
			}
            else
			{
                transform.localScale = transform.localScale.With(x: 1f);
            }*/
            Vector2 difference;
			if (hidden)
			{
                difference = shownPosition - hiddenPosition;
			}
            else
			{
                difference = hiddenPosition - shownPosition;
			}

            var angle = Mathf.RoundToInt(Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg / 90f) * 90f;
            transform.SetZRotation(angle);

        }

        public void Toggle()
		{
			if (interRoutine != null)
			{
                StopCoroutine(interRoutine);
                interRoutine = null;
            }
			if (hidden)
			{
                interRoutine = StartCoroutine(InterpolationRoutine(parent.anchoredPosition,shownPosition));
            }
            else
			{
                interRoutine = StartCoroutine(InterpolationRoutine(parent.anchoredPosition, hiddenPosition));
            }
            hidden = !hidden;
            UpdateArrowVisual();
        }

        IEnumerator InterpolationRoutine(Vector2 from, Vector2 to)
		{
			for (float t = 0; t < time; t += Time.deltaTime)
			{
                parent.anchoredPosition = Vector2.Lerp(from,to,interpolationCurve.Evaluate(t / time));
                yield return null;
			}
            parent.anchoredPosition = to;

        }
	}
}
