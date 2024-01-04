using UnityEngine;

public class RandomScale : MonoBehaviour, IExternalDebris
{
    public float minScale;

    public float maxScale;

    public bool scaleOnEnable;

    private bool didScale;

    private void Start()
    {
        if (!didScale)
        {
            ApplyScale();
        }
    }

    private void OnEnable()
    {
        if (scaleOnEnable)
        {
            ApplyScale();
        }
    }

    private void ApplyScale()
    {
        base.transform.localScale = Vector3.one * Random.Range(minScale, maxScale);
        didScale = true;
    }

    void IExternalDebris.InitExternalDebris()
    {
        ApplyScale();
    }
}
