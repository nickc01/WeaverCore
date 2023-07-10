using UnityEngine;

public class SimpleFadeOut : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public float fadeDuration = 1f;

    public bool waitForCall;

    private Color startColor;

    private Color fadeColor;

    private float currentLerpTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startColor = spriteRenderer.color;
        fadeColor = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }

    private void OnEnable()
    {
        currentLerpTime = 0f;
    }

    private void Update()
    {
        if (!waitForCall)
        {
            currentLerpTime += Time.deltaTime;
            if (currentLerpTime > fadeDuration)
            {
                currentLerpTime = fadeDuration;
                base.gameObject.SetActive(value: false);
            }
            float t = currentLerpTime / fadeDuration;
            spriteRenderer.color = Color.Lerp(startColor, fadeColor, t);
        }
    }

    public void FadeOut()
    {
        waitForCall = false;
    }
}
