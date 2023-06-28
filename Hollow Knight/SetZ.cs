using System.Collections;
using UnityEngine;

public class SetZ : MonoBehaviour
{
    public float z;

    public bool dontRandomize;

    public bool randomizeFromStartingValue;

    public float delayBeforeRandomizing = 0.5f;

    private float setZ;

    private void OnEnable()
    {
        setZ = z;
        if (!dontRandomize)
        {
            setZ = Random.Range(z, z + 0.0009999f);
        }
        if (randomizeFromStartingValue)
        {
            setZ = Random.Range(base.transform.position.z, base.transform.position.z + 0.0009999f);
        }
        StartCoroutine(SetPosition());
    }

    private IEnumerator SetPosition()
    {
        yield return new WaitForSeconds(delayBeforeRandomizing);
        base.transform.SetPositionZ(setZ);
    }
}
