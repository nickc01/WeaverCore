using TMPro;
using UnityEngine;

public class FadeGroup : MonoBehaviour
{
    public SpriteRenderer[] spriteRenderers;

    public TextMeshPro[] texts;

    //public InvAnimateUpAndDown[] animators;

    public float fadeInTime = 0.2f;

    public float fadeOutTime = 0.2f;

    public float fadeOutTimeFast = 0.2f;

    public float fullAlpha = 1f;

    public float downAlpha;

    public bool activateTexts;

    private int state;

    private float timer;

    private Color currentColour;

    private Color fadeOutColour = new Color(1f, 1f, 1f, 0f);

    private Color fadeInColour = new Color(1f, 1f, 1f, 1f);

    private float currentAlpha;

    public bool disableRenderersOnEnable;

    private void OnEnable()
    {
        if (disableRenderersOnEnable)
        {
            DisableRenderers();
        }
    }

    private void Update()
    {
        if (state == 0)
        {
            return;
        }
        float t = 0f;
        if (state == 1)
        {
            timer += Time.deltaTime;
            if (timer > fadeInTime)
            {
                timer = fadeInTime;
                state = 0;
                for (int i = 0; i < spriteRenderers.Length; i++)
                {
                    if (spriteRenderers[i] != null)
                    {
                        Color color = spriteRenderers[i].color;
                        color.a = fullAlpha;
                        spriteRenderers[i].color = color;
                    }
                }
                for (int j = 0; j < texts.Length; j++)
                {
                    if (texts[j] != null)
                    {
                        Color color2 = texts[j].color;
                        color2.a = fullAlpha;
                        texts[j].color = color2;
                    }
                }
            }
            t = timer / fadeInTime;
        }
        else if (state == 2)
        {
            timer -= Time.deltaTime;
            if (timer < 0f)
            {
                timer = 0f;
                state = 0;
                if (downAlpha > 0f)
                {
                    for (int k = 0; k < spriteRenderers.Length; k++)
                    {
                        if (spriteRenderers[k] != null)
                        {
                            Color color3 = spriteRenderers[k].color;
                            color3.a = downAlpha;
                            spriteRenderers[k].color = color3;
                        }
                    }
                    for (int l = 0; l < texts.Length; l++)
                    {
                        if (texts[l] != null)
                        {
                            Color color4 = texts[l].color;
                            color4.a = downAlpha;
                            texts[l].color = color4;
                        }
                    }
                }
                else
                {
                    DisableRenderers();
                }
            }
            t = timer / fadeOutTime;
        }
        if (state == 0)
        {
            return;
        }
        currentAlpha = Mathf.Lerp(downAlpha, fullAlpha, t);
        for (int m = 0; m < spriteRenderers.Length; m++)
        {
            if (spriteRenderers[m] != null)
            {
                Color color5 = spriteRenderers[m].color;
                color5.a = currentAlpha;
                spriteRenderers[m].color = color5;
            }
        }
        for (int n = 0; n < texts.Length; n++)
        {
            if (texts[n] != null)
            {
                Color color6 = texts[n].color;
                color6.a = currentAlpha;
                texts[n].color = color6;
            }
        }
    }

    public void FadeUp()
    {
        timer = 0f;
        state = 1;
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                Color color = spriteRenderers[i].color;
                color.a = 0f;
                spriteRenderers[i].color = color;
                spriteRenderers[i].enabled = true;
            }
        }
        for (int j = 0; j < texts.Length; j++)
        {
            if (texts[j] != null)
            {
                Color color2 = texts[j].color;
                color2.a = 0f;
                texts[j].color = color2;
                texts[j].gameObject.GetComponent<MeshRenderer>().SetActiveWithChildren(value: true);
            }
        }
        /*for (int k = 0; k < animators.Length; k++)
        {
            if (animators[k] != null)
            {
                animators[k].AnimateUp();
            }
        }*/
    }

    public void FadeDown()
    {
        timer = fadeOutTime;
        state = 2;
        /*for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                animators[i].AnimateDown();
            }
        }*/
    }

    public void FadeDownFast()
    {
        timer = fadeOutTimeFast;
        state = 2;
        /*for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null)
            {
                animators[i].AnimateDown();
            }
        }*/
    }

    private void DisableRenderers()
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] != null)
            {
                spriteRenderers[i].enabled = false;
            }
        }
        for (int j = 0; j < texts.Length; j++)
        {
            if (texts[j] != null)
            {
                Color color = texts[j].color;
                color.a = 0f;
                texts[j].color = color;
                texts[j].gameObject.GetComponent<MeshRenderer>().SetActiveWithChildren(value: false);
            }
        }
    }
}
