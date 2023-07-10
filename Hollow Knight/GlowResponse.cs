using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowResponse : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    private SpriteRenderer fadeSprite;

    public List<SpriteRenderer> FadeSprites = new List<SpriteRenderer>();

    public float fadeTime = 0.5f;

    public ParticleSystem particles;

    public Light light;

    private float lightStart;

    public AudioSource audioPlayerPrefab;

    //public AudioEventRandom soundEffect;

    private Dictionary<SpriteRenderer, Coroutine> fadeRoutines = new Dictionary<SpriteRenderer, Coroutine>();

    private void OnValidate()
    {
        HandleUpgrade();
    }

    private void Awake()
    {
        HandleUpgrade();
    }

    private void HandleUpgrade()
    {
        if ((bool)fadeSprite)
        {
            FadeSprites.Add(fadeSprite);
            fadeSprite = null;
        }
    }

    private void Start()
    {
        foreach (SpriteRenderer fadeSprite in FadeSprites)
        {
            if ((bool)fadeSprite)
            {
                Color color = fadeSprite.color;
                color.a = 0f;
                fadeSprite.color = color;
            }
        }
        if ((bool)light)
        {
            lightStart = light.intensity;
            light.intensity = 0f;
            light.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((bool)particles)
        {
            particles.Play();
        }
        Vector3 position = base.transform.position;
        position.z = 0f;
        //soundEffect.SpawnAndPlayOneShot(audioPlayerPrefab, position);
        FadeTo(1f);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if ((bool)particles)
        {
            particles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
        }
        FadeTo(0f);
    }

    private void FadeTo(float alpha)
    {
        foreach (SpriteRenderer fadeSprite in FadeSprites)
        {
            if (fadeRoutines.ContainsKey(fadeSprite) && fadeRoutines[fadeSprite] != null)
            {
                StopCoroutine(fadeRoutines[fadeSprite]);
            }
            if (base.gameObject.activeInHierarchy)
            {
                fadeRoutines[fadeSprite] = StartCoroutine(Fade(alpha, fadeSprite));
            }
        }
    }

    private IEnumerator Fade(float toAlpha, SpriteRenderer sprite)
    {
        float elapsed = 0f;
        Color initialColor = (sprite ? sprite.color : Color.white);
        Color currentColor = initialColor;
        bool fadeUp = toAlpha > 0.1f;
        float startIntensity = (light ? light.intensity : 0f);
        float toIntensity = (fadeUp ? lightStart : 0f);
        if ((bool)light && fadeUp)
        {
            light.enabled = true;
        }
        for (; elapsed < fadeTime; elapsed += Time.deltaTime)
        {
            if ((bool)sprite)
            {
                currentColor.a = Mathf.Lerp(initialColor.a, toAlpha, elapsed / fadeTime);
                sprite.color = currentColor;
            }
            if ((bool)light)
            {
                light.intensity = Mathf.Lerp(startIntensity, toIntensity, elapsed / fadeTime);
            }
            yield return null;
        }
        if ((bool)sprite)
        {
            currentColor.a = toAlpha;
            sprite.color = currentColor;
        }
        if ((bool)light && !fadeUp)
        {
            light.enabled = false;
        }
    }

    public void FadeEnd()
    {
        FadeTo(0f);
        if ((bool)particles)
        {
            particles.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmitting);
        }
        CircleCollider2D component = GetComponent<CircleCollider2D>();
        if (component != null)
        {
            component.enabled = false;
        }
    }
}
