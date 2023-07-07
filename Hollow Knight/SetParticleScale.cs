using UnityEngine;

public class SetParticleScale : MonoBehaviour
{
    public bool grandParent;

    public bool greatGrandParent;

    private float parentXScale;

    private float selfXScale;

    private Vector3 scaleVector;

    private bool unparented;

    private GameObject parent;

    private void Start()
    {
        if (grandParent)
        {
            if (transform.parent != null && transform.parent.parent != null)
            {
                parent = transform.parent.gameObject.transform.parent.gameObject;
            }
        }
        else if (greatGrandParent)
        {
            if (transform.parent != null && transform.parent.parent != null && transform.parent.parent.parent != null)
            {
                parent = transform.parent.gameObject.transform.parent.gameObject.transform.parent.gameObject;
            }
        }
        else if (transform.parent != null)
        {
            parent = transform.parent.gameObject;
        }
    }

    private void Update()
    {
        if (parent != null && !unparented)
        {
            parentXScale = parent.transform.localScale.x;
            selfXScale = transform.localScale.x;
            if ((parentXScale < 0f && selfXScale > 0f) || (parentXScale > 0f && selfXScale < 0f))
            {
                scaleVector.Set(0f - transform.localScale.x, transform.localScale.y, transform.localScale.z);
                transform.localScale = scaleVector;
            }
        }
        else
        {
            unparented = true;
            selfXScale = transform.localScale.x;
            if (selfXScale < 0f)
            {
                scaleVector.Set(0f - transform.localScale.x, transform.localScale.y, transform.localScale.z);
                transform.localScale = scaleVector;
            }
        }
    }
}
