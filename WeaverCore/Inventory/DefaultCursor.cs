using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Inventory
{
    /// <summary>
    /// The default cursor used in an inventory pane
    /// </summary>
    public class DefaultCursor : Cursor
    {
        [SerializeField]
        Transform bl;

        [SerializeField]
        Transform br;

        [SerializeField]
        Transform tl;

        [SerializeField]
        Transform tr;

        [SerializeField]
        Transform back;

        [SerializeField]
        AudioClip uiChangeSelectionClip;

        System.Collections.Generic.List<ColorFader> faders = new System.Collections.Generic.List<ColorFader>();

        [SerializeField]
        float uiChangeSelectionVolume = 0.75f;

        [SerializeField]
        float interpTime = 0.15f;

        [SerializeField]
        Vector3 scaleMin = new Vector3(1f,1f,1f);

        [SerializeField]
        Vector3 scaleMax = new Vector3(1.15f, 1.15f, 1.15f);

        [SerializeField]
        float pulseSpeed = 1f;

        Vector3 lastPos;

        UnboundCoroutine pulseCoroutine;

        Vector3 currentDestination;

        float moveTimeStamp = 0f;

        public override bool CanMove()
        {
            return Time.time >= moveTimeStamp + 0.120f;
        }

        private void Awake()
        {
            GetComponentsInChildren(faders);
        }

        private void OnEnable()
        {
            if (pulseCoroutine != null)
            {
                pulseCoroutine.Stop();
                pulseCoroutine = null;
            }
            pulseCoroutine = UnboundCoroutine.Start(PulseRoutine());
        }

        private void OnDisable()
        {
            if (pulseCoroutine != null)
            {
                pulseCoroutine.Stop();
                pulseCoroutine = null;
            }
        }

        private void OnDestroy()
        {
            if (pulseCoroutine != null)
            {
                pulseCoroutine.Stop();
                pulseCoroutine = null;
            }
        }

        protected override void OnBegin(InventoryElement beginElement)
        {
            lastPos = new Vector3(float.PositiveInfinity, float.PositiveInfinity);
            back.localPosition = new Vector3(0f, 0f, 2f);
            back.localScale = Vector3.zero;

            bl.localPosition = Vector3.zero;
            br.localPosition = Vector3.zero;
            tl.localPosition = Vector3.zero;
            tr.localPosition = Vector3.zero;

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }

            foreach (var fader in faders)
            {
                fader.Fade(true);
            }

            OnMoveTo(beginElement);
        }

        protected override void OnEnd()
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }

        protected override void OnHide()
        {
            foreach (var fader in faders)
            {
                fader.Fade(false);
            }
        }

        public override Vector3 CursorPosition => HighlightedElement == null ? transform.position : transform.TransformPoint(currentDestination);

        protected override void OnMoveTo(InventoryElement element)
        {
            moveTimeStamp = Time.time;
            var itemPos = Panel.Navigator.GetCursorPosForElement(element);
            var itemOffset = Panel.Navigator.GetCursorOffsetForElement(element);
            var itemBounds = Panel.Navigator.GetCursorBoundsForElement(element);

            itemPos.z = -4.5f;

            currentDestination = itemPos;

            //TODO - REPLICATE MOVE EVENT

            var backBoardPos = new Vector3(itemOffset.x, itemOffset.y, 0f);

            StopAllCoroutines();

            //bool playSound = Vector2.Distance(lastPos, LocalizePosition(transform, itemPos)) < 0.01f;

            if (float.IsPositiveInfinity(lastPos.x))
            {
                transform.localPosition = itemPos;
                lastPos = transform.localPosition;

                back.transform.localPosition = backBoardPos;
                back.transform.localScale = Vector3.zero;
            }
            else
            {
                StartCoroutine(InterpRoutine(transform, lastPos, itemPos, interpTime, true));
                StartCoroutine(InterpRoutine(back, back.localPosition, backBoardPos, interpTime));
            }

            StartCoroutine(InterpScaleRoutine(back,back.localScale, new Vector3(itemBounds.x, itemBounds.y,1f),interpTime));

            float leftX = (itemBounds.x * -0.5f) + itemOffset.x;
            float bottomY = (itemBounds.y * -0.5f) + itemOffset.y;
            float rightX = (itemBounds.x * 0.5f) + itemOffset.x;
            float topY = (itemBounds.y * 0.5f) + itemOffset.y;


            StartCoroutine(InterpRoutine(bl,bl.localPosition,new Vector3(leftX,bottomY, -4.5f),interpTime));
            StartCoroutine(InterpRoutine(br,br.localPosition,new Vector3(rightX,bottomY, -4.5f),interpTime));
            StartCoroutine(InterpRoutine(tl,tl.localPosition,new Vector3(leftX,topY, -4.5f),interpTime));
            StartCoroutine(InterpRoutine(tr,tr.localPosition,new Vector3(rightX, topY, -4.5f),interpTime));



            if (uiChangeSelectionClip != null)
            {
                WeaverAudio.PlayAtPoint(uiChangeSelectionClip, transform.position, uiChangeSelectionVolume);
            }


            /*float leftX = colliderBounds.x;
            float bottomY = colliderBounds.y;

            Vector2 backScale = colliderBounds;*/
        }

        Vector3 LocalizePosition(Transform t, Vector3 pos)
        {
            if (t.parent != null)
            {
                return t.parent.InverseTransformPoint(pos);
            }
            else
            {
                return pos;
            }
        }

        IEnumerator InterpRoutine(Transform obj, Vector3 from, Vector3 to, float time, bool setLastPos = false)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                obj.localPosition = Vector3.Lerp(from, to, t / time);
                if (setLastPos)
                {
                    lastPos = obj.localPosition;
                }
                yield return null;
            }
            obj.localPosition = to;
        }

        IEnumerator InterpScaleRoutine(Transform obj, Vector3 from, Vector3 to, float time)
        {
            for (float t = 0; t < time; t += Time.deltaTime)
            {
                obj.localScale = Vector3.Lerp(from, to, t / time);
                yield return null;
            }
            obj.localScale = to;
        }

        /*IEnumerator PlayDelayed(AudioClip clip, float delay, float volume = 1f)
        {
            yield return new WaitForSeconds(delay);
            WeaverAudio.PlayAtPoint(clip, transform.position, volume);
        }*/

        protected override void OnShow()
        {
            foreach (var fader in faders)
            {
                fader.Fade(true);
            }
        }

        IEnumerator PulseRoutine()
        {
            while (true)
            {
                float halfTime = pulseSpeed / 2f;
                for (float t = 0; t < halfTime; t += Time.deltaTime)
                {
                    if (transform != null)
                    {
                        transform.localScale = Vector3.Lerp(scaleMin, scaleMax, t / halfTime);
                    }
                    else
                    {
                        yield break;
                    }
                    yield return null;
                }

                for (float t = 0; t < halfTime; t += Time.deltaTime)
                {
                    if (transform != null)
                    {
                        transform.localScale = Vector3.Lerp(scaleMax, scaleMin, t / halfTime);
                    }
                    else
                    {
                        yield break;
                    }
                    yield return null;
                }
            }
        }
    }
}
