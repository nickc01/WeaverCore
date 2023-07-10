using UnityEngine;

public class RandomRotationRange : MonoBehaviour
{
    public float min;

    public float max;

    private void Start()
    {
        RandomRotate();
    }

    private void OnEnable()
    {
        RandomRotate();
    }

    private void RandomRotate()
    {
        base.transform.localEulerAngles = new Vector3(base.transform.rotation.x, base.transform.rotation.y, Random.Range(min, max));
    }
}
