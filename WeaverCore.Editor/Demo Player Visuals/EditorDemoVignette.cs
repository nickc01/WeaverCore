using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public sealed class EditorDemoVignette : MonoBehaviour
{
    [SerializeField] int initialDarknessLevel;
    [SerializeField] Vector3 normalScale = new Vector3(0.8f, 0.8f, 1f);
    [SerializeField] Vector3 damageScale = new Vector3(4f, 4f, 1f);
    [SerializeField] float damageGrowTime = 0.25f;
    [SerializeField] float damagePause = 1f;
    [SerializeField] float recoverTime = 0.5f;
    [SerializeField] float darkOneAlpha = 0.45f;
    [SerializeField] float darkTwoAlpha = 0.75f;
    [SerializeField] float lanternAlpha = 0.25f;

    SpriteRenderer spriteRenderer;
    Coroutine routine;
    int darknessLevel;

    public int DarknessLevel => darknessLevel;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        transform.localPosition = new Vector3(0f, -0.58f, -1f);
        transform.localScale = normalScale;
        SetDarknessLevel(initialDarknessLevel);
    }

    public void SendEvent(string eventName)
    {
        switch (eventName)
        {
            case "DAMAGE":
                Damage();
                break;
            case "DEATH":
                Death();
                break;
            case "DARK 1":
                SetDarknessLevel(1);
                break;
            case "DARK 2":
                SetDarknessLevel(2);
                break;
            case "DARK -1":
            case "LANTERN":
                SetDarknessLevel(-1);
                break;
            case "NORMAL":
            case "RESET":
            case "SCENE RESET":
            case "SCENE RESET NO LANTERN":
                SetDarknessLevel(0);
                break;
        }
    }

    public void SetDarknessLevel(int level)
    {
        darknessLevel = Mathf.Clamp(level, -1, 2);
        float alpha = 0f;
        if (darknessLevel == 1)
        {
            alpha = darkOneAlpha;
        }
        else if (darknessLevel == 2)
        {
            alpha = darkTwoAlpha;
        }
        else if (darknessLevel == -1)
        {
            alpha = lanternAlpha;
        }

        spriteRenderer.enabled = alpha > 0f;
        var color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
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
        yield return ScaleRoutine(normalScale, recoverTime);
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
