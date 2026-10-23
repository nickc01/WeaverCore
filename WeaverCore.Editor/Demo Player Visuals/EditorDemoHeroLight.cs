using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class EditorDemoHeroLight : MonoBehaviour
{
    [SerializeField] Vector3 idleScale = new Vector3(3f, 3f, 1f);
    [SerializeField] Vector3 damageScale = Vector3.one;
    [SerializeField] float damageGrowTime = 0.25f;
    [SerializeField] float damagePause = 2f;
    [SerializeField] float recoverTime = 0.5f;

    Coroutine routine;
    SpriteRenderer spriteRenderer;
    GameCameras gameCameras;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localPosition = new Vector3(0f, -0.6f, 0.0312f);
        transform.localScale = idleScale;
    }

    void LateUpdate()
    {
        if (gameCameras == null)
        {
            gameCameras = FindObjectOfType<GameCameras>();
        }

        if (gameCameras == null || gameCameras.sceneColorManager == null)
        {
            return;
        }

        spriteRenderer.color = gameCameras.sceneColorManager.HeroLightColorA;
    }

    public void Damage()
    {
        StartRoutine(DamageRoutine());
    }

    public void Death()
    {
        StartRoutine(ScaleRoutine(Vector3.zero, recoverTime));
    }

    IEnumerator DamageRoutine()
    {
        yield return ScaleRoutine(damageScale, damageGrowTime);
        yield return new WaitForSecondsRealtime(damagePause);
        yield return ScaleRoutine(idleScale, recoverTime);
    }

    IEnumerator ScaleRoutine(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        for (float elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
        {
            transform.localScale = Vector3.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        transform.localScale = target;
    }

    void StartRoutine(IEnumerator next)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(next);
    }
}
