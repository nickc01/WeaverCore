using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ColorFader : MonoBehaviour
{
    public delegate void FadeEndEvent(bool up);

    public Color downColour = new Color(1f, 1f, 1f, 0f);

    public float downTime = 0.5f;

    public Color upColour = new Color(1f, 1f, 1f, 1f);

    public float upDelay;

    public float upTime = 0.4f;

    private Color initialColour;

    public bool useInitialColour = true;

    private SpriteRenderer spriteRenderer;

    private TextMeshPro textRenderer;

    private Graphic tk2dSprite;

    private bool setup;

    private Coroutine fadeRoutine;

    public event FadeEndEvent OnFadeEnd;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        if (setup)
        {
            return;
        }
        setup = true;
        if (!spriteRenderer)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if ((bool)spriteRenderer)
        {
            initialColour = (useInitialColour ? spriteRenderer.color : Color.white);
            spriteRenderer.color = downColour * initialColour;
            return;
        }
        if (!textRenderer)
        {
            textRenderer = GetComponent<TextMeshPro>();
        }
        if ((bool)textRenderer)
        {
            initialColour = (useInitialColour ? textRenderer.color : Color.white);
            textRenderer.color = downColour * initialColour;
            return;
        }
        if (!tk2dSprite)
        {
            tk2dSprite = GetComponent<Graphic>();
        }
        if ((bool)tk2dSprite)
        {
            initialColour = (useInitialColour ? tk2dSprite.color : Color.white);
            tk2dSprite.color = downColour * initialColour;
        }
    }

    public void Fade(bool up)
    {
        Setup();
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        if (up)
        {
            fadeRoutine = StartCoroutine(Fade(upColour, upTime, upDelay));
        }
        else
        {
            fadeRoutine = StartCoroutine(Fade(downColour, downTime, 0f));
        }
    }

    private IEnumerator Fade(Color to, float time, float delay)
    {
        if (!spriteRenderer)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        Color from = (spriteRenderer ? spriteRenderer.color : (textRenderer ? textRenderer.color : (tk2dSprite ? tk2dSprite.color : Color.white)));
        if (delay > 0f)
        {
            yield return new WaitForSeconds(upDelay);
        }
        for (float elapsed = 0f; elapsed < time; elapsed += Time.deltaTime)
        {
            Color color = Color.Lerp(from, to, elapsed / time) * initialColour;
            if ((bool)spriteRenderer)
            {
                spriteRenderer.color = color;
            }
            else if ((bool)textRenderer)
            {
                textRenderer.color = color;
            }
            else if ((bool)tk2dSprite)
            {
                tk2dSprite.color = color;
            }
            yield return null;
        }
        if ((bool)spriteRenderer)
        {
            spriteRenderer.color = to * initialColour;
        }
        else if ((bool)textRenderer)
        {
            textRenderer.color = to * initialColour;
        }
        else if ((bool)tk2dSprite)
        {
            tk2dSprite.color = to * initialColour;
        }
        if (this.OnFadeEnd != null)
        {
            this.OnFadeEnd(to == upColour);
        }
    }
}
