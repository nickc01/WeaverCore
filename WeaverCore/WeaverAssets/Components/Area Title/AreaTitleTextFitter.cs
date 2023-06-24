using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace WeaverCore.Assets.Components
{
    /// <summary>
    /// Used for fitting the text in the <see cref="WeaverBossTitle"/> into place
    /// </summary>
    [ExecuteInEditMode]
    public class AreaTitleTextFitter : MonoBehaviour
    {
        [SerializeField]
        bool resizeToWidth;

        [SerializeField]
        bool resizeToHeight;

        [SerializeField]
        float minWidth = 0f;

        [SerializeField]
        float minHeight = 0f;

        [SerializeField]
        float horizontalSpacing = 0f;
        [SerializeField]
        float verticalSpacing = 0f;

        RectTransform rect;

        List<TextMeshProUGUI> textObjects = new List<TextMeshProUGUI>();

        void Start()
        {
            GetComponentsInChildren(textObjects);
            rect = GetComponent<RectTransform>();
        }

        void Update()
        {
            if (Application.isEditor)
            {
                GetComponentsInChildren(textObjects);
            }
            if (Application.isEditor && rect == null)
            {
                rect = GetComponent<RectTransform>();
            }
            if (rect != null)
            {
                var newSize = rect.sizeDelta;
                if (resizeToWidth)
                {
                    newSize.x = 0f;
                    foreach (var t in textObjects)
                    {
                        if (t.preferredWidth > newSize.x)
                        {
                            newSize.x = t.preferredWidth;
                        }
                    }

                    newSize.x += horizontalSpacing;
                    if (newSize.x < minWidth)
                    {
                        newSize.x = minWidth;
                    }
                }
                if (resizeToHeight)
                {
                    newSize.y = 0f;
                    foreach (var t in textObjects)
                    {
                        if (t.preferredHeight > newSize.y)
                        {
                            newSize.y = t.preferredHeight;
                        }
                    }

                    newSize.y += verticalSpacing;
                    if (newSize.y < minHeight)
                    {
                        newSize.y = minHeight;
                    }
                }
                rect.sizeDelta = newSize;
            }
        }
    }
}