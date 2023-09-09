using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCursor : MonoBehaviour
{
    [SerializeField]
    Transform tl;

    [SerializeField]
    Transform tr;

    [SerializeField]
    Transform bl;

    [SerializeField]
    Transform br;

    [SerializeField]
    float pulseTime = 0.5f;

    [SerializeField]
    float pulseIntensity = 0.25f;

    [SerializeField]
    float moveTime = 0.4f;

    Coroutine moveCoroutine;

    [NonSerialized]
    public Vector3 CursorDestination;

    private void Awake()
    {
        StartCoroutine(PulseRoutine());
    }

    public void MoveToPosition(Vector3 position, Vector2 size, Vector2 offset)
    {
        CursorDestination = position;
        tl.localPosition = new Vector3((-size.x / 2) + offset.x, (size.y / 2) + offset.y);
        tr.localPosition = new Vector3((size.x / 2) + offset.x, (size.y / 2) + offset.y);
        bl.localPosition = new Vector3((-size.x / 2) + offset.x, -(size.y / 2) + offset.y);
        br.localPosition = new Vector3((size.x / 2) + offset.x, -(size.y / 2) + offset.y);

        if (moveCoroutine != null)
        {
            StopCoroutine(moveCoroutine);
            moveCoroutine = null;
        }

        moveCoroutine = StartCoroutine(MoveCoroutine(transform.position, position));
    }
    
    IEnumerator PulseRoutine()
    {
        var baseScale = transform.localScale;
        while (true)
        {
            for (float t = 0; t < pulseTime; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(baseScale, baseScale + new Vector3(pulseIntensity, pulseIntensity, 0f), t / pulseTime);
                yield return null;
            }

            for (float t = 0; t < pulseTime; t += Time.deltaTime)
            {
                transform.localScale = Vector3.Lerp(baseScale + new Vector3(pulseIntensity, pulseIntensity, 0f), baseScale, t / pulseTime);
                yield return null;
            }
        }
    }

    IEnumerator MoveCoroutine(Vector3 from, Vector3 to)
    {
        for (float t = 0; t < moveTime; t += Time.deltaTime)
        {
            transform.position = Vector3.Lerp(from, to, t / moveTime);
            yield return null;
        }

        if (moveCoroutine != null)
        {
            moveCoroutine = null;
        }
    }
}
