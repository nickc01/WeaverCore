using System;
using System.Collections;
using UnityEngine;
using WeaverCore.Utilities;

namespace WeaverCore.Components
{
    public class RevealableArea : MonoBehaviour
    {
        const float INSTA_THRESHOLD = 0.1f;

        [field: SerializeField]
        public float RevealDuration { get; protected set; } = 0.25f;

        [field: SerializeField]
        public float HideDuration { get; protected set; } = 0.25f;


        [NonSerialized]
        float spawnTime = -1f;
        [NonSerialized]
        Color defaultColor;
        [NonSerialized]
        SpriteRenderer _renderer;
        public SpriteRenderer Renderer => _renderer ??= GetComponent<SpriteRenderer>();

        Coroutine fadeRoutine;

        public virtual void Start() 
        {
            Start_Internal();
        }

        void Start_Internal() 
        {
            if (spawnTime < 0)
            {
                spawnTime = Time.time;
                defaultColor = Renderer.color;
            }

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                fadeRoutine = null;
            }
        }

        public virtual void Reveal() 
        {
            Start_Internal();
            if (Time.time > spawnTime + INSTA_THRESHOLD)
            {
                fadeRoutine = StartCoroutine(FadeRoutine(true));
            }
            else
            {
                var routine = FadeRoutine(true);
                while (routine.MoveNext()) {}
            }
        }

        public virtual void Hide()
        {
            Start_Internal();
            if (Time.time > spawnTime + INSTA_THRESHOLD)
            {
                fadeRoutine = StartCoroutine(FadeRoutine(false));
            }
            else
            {
                var routine = FadeRoutine(false);
                while (routine.MoveNext()) {}
            }
        }

        IEnumerator FadeRoutine(bool reveal) 
        {
            Color startColor = Renderer.color;
            Color destColor = reveal ? defaultColor : startColor.With(a: 0f);
            float time = reveal ? RevealDuration : HideDuration;

            if (Time.time > spawnTime + INSTA_THRESHOLD)
            {
                for (float t = 0; t < time; t += Time.deltaTime)
                {
                    Renderer.color = Color.Lerp(startColor, destColor, t / time);
                    yield return null;
                }
            }
            else
            {
                Renderer.color = destColor;
            }

            fadeRoutine = null;
            yield break;
        }
    }
}